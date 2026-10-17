#!/bin/bash
# Exit on unhandled failures (-e), reject unset variables (-u), and preserve
# failures within pipelines (pipefail), including dotnet publish piped to tee.
set -euo pipefail

# Resolve paths relative to this script ($0), then enter the repository root so
# dotnet uses its global.json even when the script is invoked from elsewhere.
repo_root=$(cd "$(dirname "$0")/.." && pwd)
cd "$repo_root"

# Print a diagnostic to standard error (>&2) and return a failing exit status.
fail() {
  echo "Error: $*" >&2
  exit 1
}

# Darwin is macOS. Require native ARM64 execution; a Rosetta shell reports x86_64.
# Each "condition || fail" below stops with a message when the condition fails.
[[ $(uname -s) == Darwin && $(uname -m) == arm64 ]] ||
  fail "Publish on an ARM64 Mac, outside Rosetta."

# Check PATH before building. Discard command -v's output; only its status matters.
for tool in dotnet xcode-select xcodebuild xcrun file lipo otool plutil /usr/bin/python3; do
  command -v "$tool" >/dev/null || fail "Required tool is missing: $tool"
done

# Read the SDK pin from JSON and compare it with dotnet's actual selection.
# Releases require the exact version, even if global.json allows patch roll-forward.
pinned_sdk=$(plutil -extract sdk.version raw global.json)
selected_sdk=$(dotnet --version)
[[ "$selected_sdk" == "$pinned_sdk" ]] ||
  fail "Install SDK $pinned_sdk from global.json (selected $selected_sdk)."
source_commit=$(git rev-parse HEAD)
source_changes=$(git status --porcelain)

# Report the toolchain for local/CI diagnostics. DEVELOPER_DIR takes precedence
# over xcode-select's default; neither this check nor xcrun changes that default.
echo "Host: $(sw_vers -productVersion) ($(uname -m))"
echo "Developer directory: ${DEVELOPER_DIR:-$(xcode-select -p)}"
xcodebuild -version
echo "macOS SDK: $(xcrun --sdk macosx --show-sdk-version)"
# Pass this selected SDK explicitly to Native AOT through SysRoot below.
sdk_path=$(xcrun --sdk macosx --show-sdk-path)
[[ -d "$sdk_path" ]] || fail "macOS SDK is unavailable: $sdk_path"
echo "macOS SDK path: $sdk_path"
xcrun --sdk macosx clang --version
dotnet --info

# Keep every run isolated, including intermediates and diagnostics. Never package
# this whole directory: only Compiler (and optionally Compiler.dSYM) is an asset.
# mktemp replaces XXXXXX with a unique suffix. Retain the directory for inspection.
mkdir -p artifacts/release
release_dir=$(mktemp -d "$repo_root/artifacts/release/osx-arm64.XXXXXX")
publish_dir="$release_dir/publish"
echo "Release directory: $release_dir"

# Publish only the compiler as a Release, self-contained native ARM64 executable.
# These settings apply to this invocation; development builds keep their defaults.
# -p: passes an MSBuild property. AppleMinOSVersion and the command-scoped
# MACOSX_DEPLOYMENT_TARGET agree on macOS 14.0; runtime compatibility still needs testing.
# Treat build, AOT, and trimming warnings as errors, with detailed trimming warnings.
# Keep intermediates in build/ and final files in publish/. Use one MSBuild worker
# and disable persistent build servers to avoid Codex sandbox socket failures.
# Save a binary MSBuild log (-bl); merge stderr into stdout (2>&1) so tee both
# displays and saves publish output. pipefail ensures a publish failure reaches CI.
MACOSX_DEPLOYMENT_TARGET=14.0 dotnet publish src/Compiler.csproj \
  -c Release -r osx-arm64 --self-contained true \
  -p:PublishAot=true -p:AppleMinOSVersion=14.0 -p:SysRoot="$sdk_path" \
  -p:TreatWarningsAsErrors=true -p:IlcTreatWarningsAsErrors=true \
  -p:ILLinkTreatWarningsAsErrors=true -p:TrimmerSingleWarn=false \
  --artifacts-path "$release_dir/build" -o "$publish_dir" \
  -m:1 --disable-build-servers "-bl:$release_dir/publish.binlog" \
  2>&1 | tee "$release_dir/publish.log"

# Require an executable, report its format, and reject Intel or universal binaries.
compiler="$publish_dir/Compiler"
[[ -x "$compiler" ]] || fail "Publication did not produce Compiler."
file "$compiler"
[[ $(lipo -archs "$compiler") == arm64 ]] || fail "Compiler must contain only ARM64 code."
# Record linked dynamic libraries so unexpected runtime dependencies are visible.
otool -L "$compiler" | tee "$release_dir/dependencies.txt"
# Only public macOS system libraries may be loaded; reject @rpath, .NET,
# toolchain libraries, and absolute paths specific to the build machine.
# The < <(...) syntax feeds command output into the loop. tail removes otool's
# filename heading; sed removes indentation and version annotations, leaving paths.
# IFS= and read -r preserve whitespace and backslashes within each dependency path.
while IFS= read -r dependency; do
  case "$dependency" in
    /usr/lib/*|/System/Library/Frameworks/*) ;;
    *) fail "Unexpected dynamic dependency: $dependency" ;;
  esac
done < <(otool -L "$compiler" | tail -n +2 | sed 's/^[[:space:]]*//; s/ (compatibility version.*//')

# Retain separate native debug symbols for optional packaging; Compiler runs alone.
[[ -d "$compiler.dSYM" ]] || fail "Native debug symbols are missing: $compiler.dSYM"
# Inspect the chosen distribution policy before any final artifact tests. Do
# not replace the linker's ad hoc signature with a locally trusted signature.
./scripts/check-release-signature.sh "$compiler" 2>&1 | tee "$release_dir/signature.txt"
# Capture build provenance here, rather than guessing the build host later when
# a downloaded CI artifact is packaged on another machine. Bind it to the bytes.
/usr/bin/python3 - "$release_dir" "$source_commit" "$source_changes" "$selected_sdk" <<'PY'
import hashlib, json, pathlib, subprocess, sys, xml.etree.ElementTree as ET

release = pathlib.Path(sys.argv[1])
assets = json.loads((release / "build/obj/Compiler/project.assets.json").read_text())
framework = next(iter(assets["project"]["frameworks"].values()))
runtime = next(item["version"] for item in framework["downloadDependencies"]
               if item["name"] == "Microsoft.NETCore.App.Runtime.NativeAOT.osx-arm64")
runtime_bounds = runtime.strip("[]").split(", ")
if len(runtime_bounds) != 2 or runtime_bounds[0] != runtime_bounds[1]:
    sys.exit("Expected an exact Native AOT runtime version in restored assets.")
runtime = runtime_bounds[0]
commandline = next(key.split("/")[1] for key in assets["libraries"]
                   if key.startswith("System.CommandLine/"))
def output(*args):
    return subprocess.check_output(args, text=True).strip()
metadata = {
    "source_commit": sys.argv[2], "source_dirty": bool(sys.argv[3]),
    "sdk": sys.argv[4], "build_os": output("sw_vers", "-productVersion"),
    "build_os_build": output("sw_vers", "-buildVersion"),
    "xcode": output("xcodebuild", "-version"),
    "runtime_identifier": "osx-arm64", "minimum_macos": "14.0",
    "runtime_version": runtime, "system_commandline_version": commandline,
    "version": ET.parse("Directory.Build.props").findtext(".//Version"),
    "signing_policy": "ad-hoc-only; no Developer ID; not notarized",
    "compiler_sha256": hashlib.sha256((release / "publish/Compiler").read_bytes()).hexdigest(),
}
(release / "publish/build-metadata.json").write_text(json.dumps(metadata, indent=2) + "\n")
PY
echo "Published executable: $compiler"
echo "Optional symbols: $compiler.dSYM"

# Reusable CI jobs consume these paths without copying the publish command or
# guessing which output directory belongs to this invocation.
if [[ -n ${GITHUB_OUTPUT:-} ]]; then
  echo "compiler-path=$compiler" >>"$GITHUB_OUTPUT"
  echo "release-directory=$release_dir" >>"$GITHUB_OUTPUT"
fi
