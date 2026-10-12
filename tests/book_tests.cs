#!/usr/bin/env dotnet

using System.Diagnostics;

var bookTestsDirectory = Environment.GetEnvironmentVariable("BOOK_TESTS_DIR");
if (string.IsNullOrEmpty(bookTestsDirectory))
  bookTestsDirectory = "../writing-a-c-compiler-tests";

var configuration = Environment.GetEnvironmentVariable("BUILD_CONFIGURATION");
if (string.IsNullOrEmpty(configuration))
  configuration = "Debug";

var buildExitCode = Run("dotnet",
[
  "build", "src/Compiler.csproj",
  "--configuration", configuration,
  "--output", "artifacts/book-tests",
  "-m:1", "-p:UseSharedCompilation=false"
]);

if (buildExitCode != 0)
  return buildExitCode;

return Run("python3",
[
  Path.Combine(bookTestsDirectory, "test_compiler"),
  Path.GetFullPath("artifacts/book-tests/Compiler"),
  .. args
]);

static int Run(string fileName, string[] arguments)
{
  var startInfo = new ProcessStartInfo(fileName)
  {
    UseShellExecute = false
  };

  foreach (var argument in arguments)
    startInfo.ArgumentList.Add(argument);

  using var process = new Process { StartInfo = startInfo };
  process.Start();
  process.WaitForExit();
  return process.ExitCode;
}
