#!/usr/bin/env python3
"""Package an existing validated Native AOT executable; never build it."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tarfile
import tempfile
import xml.etree.ElementTree as ET

REPO = Path(__file__).resolve().parent.parent


def require(condition, message):
    if not condition:
        raise ValueError(message)


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def output(*args):
    return subprocess.check_output(args, text=True).strip()


def archive(source, destination):
    # macOS extended attributes/resource forks are deliberately not tar members.
    with tarfile.open(destination, "w:gz") as bundle:
        bundle.add(source, arcname=source.name)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("compiler", type=Path, help="published Compiler with adjacent build-metadata.json")
    parser.add_argument("version", nargs="?", help="optional MAJOR.MINOR.PATCH or vMAJOR.MINOR.PATCH")
    parser.add_argument("--symbols", action="store_true", help="also package the adjacent Compiler.dSYM")
    args = parser.parse_args()
    version = ET.parse(REPO / "Directory.Build.props").findtext(".//Version")
    require(version and re.fullmatch(r"[0-9]+\.[0-9]+\.[0-9]+", version), "Project version must be MAJOR.MINOR.PATCH.")
    supplied = args.version[1:] if args.version and args.version.startswith("v") else args.version
    require(supplied is None or supplied == version, "Release version must match project version " + version)
    require(sys.platform == "darwin", "Package on macOS with GCC and Rosetta for smoke checks.")
    require(args.compiler.is_file() and not args.compiler.is_symlink(), "Supply a regular published executable.")
    compiler = args.compiler.resolve()
    require(os.access(compiler, os.X_OK), "Published Compiler must be executable.")
    require(output("lipo", "-archs", str(compiler)) == "arm64", "Compiler must contain only ARM64 code.")
    subprocess.run([str(REPO / "scripts/check-release-signature.sh"), str(compiler)], check=True)
    require(output(str(compiler), "--version") == "Compiler " + version, "Compiler version does not match the project.")

    metadata = json.loads((compiler.parent / "build-metadata.json").read_text())
    original_hash = sha256(compiler)
    require(metadata["compiler_sha256"] == original_hash, "Compiler differs from its build metadata.")
    require(metadata["version"] == version and metadata["runtime_identifier"] == "osx-arm64", "Build metadata version/RID mismatch.")
    require(re.fullmatch(r"[0-9a-f]{40}", metadata["source_commit"]), "Invalid source commit in build metadata.")
    for key in ("sdk", "build_os", "build_os_build", "xcode", "minimum_macos", "signing_policy"):
        require(metadata[key], "Missing build metadata: " + key)
    notices = json.loads((REPO / "third-party/versions.json").read_text())
    require(metadata["runtime_version"] == notices["runtime"], "Audit .NET notices for this runtime before packaging.")
    require(metadata["system_commandline_version"] == notices["system_commandline"], "Audit System.CommandLine notices before packaging.")

    book = Path(os.environ.get("BOOK_TESTS_DIR", str(REPO.parent / "writing-a-c-compiler-tests"))).resolve()
    require((book / "test_compiler").is_file(), "Set BOOK_TESTS_DIR to the upstream checkout used for validation.")
    book_commit = output("git", "-C", str(book), "rev-parse", "HEAD")
    symbols = compiler.with_name(compiler.name + ".dSYM")
    if args.symbols:
        require(symbols.is_dir() and not symbols.is_symlink(), "Missing adjacent Compiler.dSYM directory.")
        require(not any(p.is_symlink() for p in symbols.rglob("*")), "Symbols must not contain symlinks.")

    parent = REPO / "artifacts/release"
    parent.mkdir(parents=True, exist_ok=True)
    destination = Path(tempfile.mkdtemp(prefix="package.", dir=parent))
    print("Package directory: " + str(destination), flush=True)
    name = "compiler-" + version + "-osx-arm64"
    staged = destination / "stage" / name
    staged.mkdir(parents=True)
    shutil.copy2(compiler, staged / "Compiler")
    shutil.copyfile(REPO / "LICENSE", staged / "LICENSE")
    readme = (REPO / "docs/binary-readme.md").read_text().replace("@VERSION@", version)
    (staged / "README.md").write_text(readme)
    notice_text = "Redistributed .NET runtime and System.CommandLine notices\n\n"
    for filename in ("dotnet-LICENSE.txt", "dotnet-THIRD-PARTY-NOTICES.txt", "system-commandline-LICENSE.md"):
        notice_text += "\n--- " + filename + " ---\n\n" + (REPO / "third-party" / filename).read_text() + "\n"
    (staged / "THIRD-PARTY-NOTICES.txt").write_text(notice_text)
    runtime_archive = destination / (name + ".tar.gz")
    archive(staged, runtime_archive)
    archives = [runtime_archive]
    if args.symbols:
        symbol_stage = destination / "stage" / (name + "-symbols")
        symbol_stage.mkdir()
        shutil.copytree(symbols, symbol_stage / "Compiler.dSYM")
        symbol_archive = destination / (name + "-symbols.tar.gz")
        archive(symbol_stage, symbol_archive)
        archives.append(symbol_archive)

    checksums = destination / "SHA256SUMS"
    checksums.write_text("".join(sha256(path) + "  " + path.name + "\n" for path in archives))
    subprocess.run(["shasum", "-a", "256", "-c", checksums.name], cwd=destination, check=True)
    extracted = destination / "extracted"
    extracted.mkdir()
    with tarfile.open(runtime_archive, "r:gz") as bundle:
        expected = {name, *(name + "/" + item for item in ("Compiler", "LICENSE", "README.md", "THIRD-PARTY-NOTICES.txt"))}
        members = bundle.getmembers()
        require(len(members) == len(expected) and {m.name for m in members} == expected, "Unexpected archive contents.")
        require(all(m.isdir() if m.name == name else m.isfile() for m in members), "Unexpected archive member type.")
        # Exact member names and types above exclude traversal and symlink entries,
        # including on Python 3.8, which lacks the newer extraction filters.
        bundle.extractall(extracted)
    extracted_compiler = extracted / name / "Compiler"
    require(os.access(extracted_compiler, os.X_OK), "Archive lost executable permissions.")
    require(sha256(extracted_compiler) == original_hash, "Extracted Compiler differs from the validated executable.")
    with (destination / "validation.log").open("w") as log:
        result = subprocess.run([str(REPO / "tests/release_smoke.sh"), str(extracted_compiler)],
                                env=dict(os.environ, EXPECTED_VERSION=version), stdout=log, stderr=subprocess.STDOUT)
    print((destination / "validation.log").read_text(), end="")
    result.check_returncode()
    require(sha256(compiler) == original_hash, "Supplied executable changed during packaging.")
    manifest = dict(metadata, book_test_commit=book_commit, packaging_commit=output("git", "-C", str(REPO), "rev-parse", "HEAD"),
                    archives={p.name: sha256(p) for p in archives}, extracted_smoke_checks="passed",
                    reproducibility="Not tested for bit-for-bit reproducibility.")
    (destination / "BUILD-MANIFEST.json").write_text(json.dumps(manifest, indent=2) + "\n")
    if os.environ.get("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a") as outputs:
            for key, value in (("package-directory", destination), ("runtime-archive", runtime_archive),
                               ("checksums-path", checksums), ("manifest-path", destination / "BUILD-MANIFEST.json")):
                outputs.write(str(key) + "=" + str(value) + "\n")
            if args.symbols:
                outputs.write("symbols-archive=" + str(symbol_archive) + "\n")
    print("Package validation passed; upload archives, SHA256SUMS, and BUILD-MANIFEST.json from " + str(destination))


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, KeyError, ET.ParseError, tarfile.TarError, subprocess.CalledProcessError) as error:
        sys.exit("Packaging failed: " + str(error))
