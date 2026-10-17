#!/usr/bin/env pwsh
# PowerShell 7.3 or newer is needed for $PSNativeCommandUseErrorActionPreference.
#Requires -Version 7.3

# Stop on unhandled failures, reject unset variables, and treat a nonzero exit
# from a native command (dotnet, xcrun, ...) as a failure, including in pipelines.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version Latest

# Print a diagnostic to standard error and return a failing exit status.
function Fail([string] $Message) {
  [Console]::Error.WriteLine("Error: $Message")
  exit 1
}

# Resolve paths relative to this script, then enter the repository root so dotnet
# uses its global.json even when the script is invoked from elsewhere. Push-Location
# and the finally block below restore the caller's location and environment, which
# matters when the script runs inside an existing PowerShell session.
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
$previousDeploymentTarget = $env:MACOSX_DEPLOYMENT_TARGET
try {
  # $IsMacOS is true on Darwin. Require native ARM64 execution; a Rosetta shell
  # reports x86_64 from uname -m.
  if (-not $IsMacOS -or (uname -m) -ne 'arm64') {
    Fail 'Publish on an ARM64 Mac, outside Rosetta.'
  }

  # Check PATH before building. Only whether each command exists matters.
  foreach ($tool in 'dotnet', 'xcode-select', 'xcodebuild', 'xcrun', 'file', 'lipo', 'otool') {
    if (-not (Get-Command $tool -CommandType Application -ErrorAction SilentlyContinue)) {
      Fail "Required tool is missing: $tool"
    }
  }

  # Read the SDK pin from JSON and compare it with dotnet's actual selection.
  # Releases require the exact version, even if global.json allows patch roll-forward.
  $pinnedSdk = (Get-Content global.json -Raw | ConvertFrom-Json).sdk.version
  $selectedSdk = dotnet --version
  if ($selectedSdk -ne $pinnedSdk) {
    Fail "Install SDK $pinnedSdk from global.json (selected $selectedSdk)."
  }

  # Report the toolchain for local/CI diagnostics. DEVELOPER_DIR takes precedence
  # over xcode-select's default; neither this check nor xcrun changes that default.
  $developerDir = if ($env:DEVELOPER_DIR) { $env:DEVELOPER_DIR } else { xcode-select -p }
  Write-Output "Host: $(sw_vers -productVersion) ($(uname -m))"
  Write-Output "Developer directory: $developerDir"
  xcodebuild -version
  Write-Output "macOS SDK: $(xcrun --sdk macosx --show-sdk-version)"
  # Pass this selected SDK explicitly to Native AOT through SysRoot below.
  $sdkPath = xcrun --sdk macosx --show-sdk-path
  if (-not (Test-Path -LiteralPath $sdkPath -PathType Container)) {
    Fail "macOS SDK is unavailable: $sdkPath"
  }
  Write-Output "macOS SDK path: $sdkPath"
  xcrun --sdk macosx clang --version
  dotnet --info

  # Keep every run isolated, including intermediates and diagnostics. Never package
  # this whole directory: only Compiler (and optionally Compiler.dSYM) is an asset.
  # mktemp replaces XXXXXX with a unique suffix. Retain the directory for inspection.
  New-Item -ItemType Directory -Force artifacts/release | Out-Null
  $releaseDir = mktemp -d "$repoRoot/artifacts/release/osx-arm64.XXXXXX"
  $publishDir = "$releaseDir/publish"
  Write-Output "Release directory: $releaseDir"

  # Publish only the compiler as a Release, self-contained native ARM64 executable.
  # These settings apply to this invocation; development builds keep their defaults.
  # -p: passes an MSBuild property. AppleMinOSVersion and MACOSX_DEPLOYMENT_TARGET
  # (restored in the finally block) agree on macOS 14.0; runtime compatibility
  # still needs testing.
  $publishArguments = @(
    'publish', 'src/Compiler.csproj'
    '-c', 'Release', '-r', 'osx-arm64', '--self-contained', 'true'
    '-p:PublishAot=true', '-p:AppleMinOSVersion=14.0', "-p:SysRoot=$sdkPath"
    # Treat build, AOT, and trimming warnings as errors, with detailed trimming warnings.
    '-p:TreatWarningsAsErrors=true', '-p:IlcTreatWarningsAsErrors=true'
    '-p:ILLinkTreatWarningsAsErrors=true', '-p:TrimmerSingleWarn=false'
    # Keep intermediates in build/ and final files in publish/.
    '--artifacts-path', "$releaseDir/build", '-o', $publishDir
    # Use one MSBuild worker and disable persistent build servers to avoid Codex
    # sandbox socket failures. Save a binary MSBuild log (-bl).
    '-m:1', '--disable-build-servers', "-bl:$releaseDir/publish.binlog"
  )
  # Merge stderr into stdout (2>&1) so Tee-Object both displays and saves publish
  # output. A dotnet failure still stops the script through the preference above.
  $env:MACOSX_DEPLOYMENT_TARGET = '14.0'
  dotnet @publishArguments 2>&1 | Tee-Object -FilePath "$releaseDir/publish.log"

  # Require an executable, report its format, and reject Intel or universal binaries.
  $compiler = "$publishDir/Compiler"
  if (-not (Test-Path -LiteralPath $compiler -PathType Leaf) -or
      -not ((Get-Item -LiteralPath $compiler).UnixFileMode -band [IO.UnixFileMode]::UserExecute)) {
    Fail 'Publication did not produce Compiler.'
  }
  file $compiler
  if ((lipo -archs $compiler) -ne 'arm64') {
    Fail 'Compiler must contain only ARM64 code.'
  }
  # Record linked dynamic libraries so unexpected runtime dependencies are visible.
  $otoolOutput = otool -L $compiler
  $otoolOutput | Tee-Object -FilePath "$releaseDir/dependencies.txt"
  # Only public macOS system libraries may be loaded; reject @rpath, .NET,
  # toolchain libraries, and absolute paths specific to the build machine.
  # Skip otool's filename heading, then remove indentation and version
  # annotations, leaving paths. -clike matches case-sensitively.
  foreach ($line in $otoolOutput | Select-Object -Skip 1) {
    $dependency = $line -replace '^\s+', '' -replace ' \(compatibility version.*', ''
    if (-not ($dependency -clike '/usr/lib/*' -or $dependency -clike '/System/Library/Frameworks/*')) {
      Fail "Unexpected dynamic dependency: $dependency"
    }
  }

  # Retain separate native debug symbols for optional packaging; Compiler runs alone.
  if (-not (Test-Path -LiteralPath "$compiler.dSYM" -PathType Container)) {
    Fail "Native debug symbols are missing: $compiler.dSYM"
  }
  Write-Output "Published executable: $compiler"
  Write-Output "Optional symbols: $compiler.dSYM"
}
finally {
  $env:MACOSX_DEPLOYMENT_TARGET = $previousDeploymentTarget
  Pop-Location
}
