using System.Diagnostics;
using Xunit;

namespace CompilerIntegrationTests;

public sealed class CompilerIntegrationTests
{
  [Fact]
  public void NoArgumentsPrintUsageAndFail()
  {
    var result = RunCompiler();

    Assert.Equal(1, result.ExitCode);
    Assert.Contains("Usage: Compiler [-gcc] [-S] <source-file>", result.StandardError);
  }

  [Theory]
  [InlineData("-S")]
  [InlineData("--assembly")]
  [InlineData("-unknown")]
  public void MalformedArgumentsPrintUsageAndFail(string argument)
  {
    var result = RunCompiler(argument);

    Assert.Equal(1, result.ExitCode);
    Assert.Contains("Usage: Compiler [-gcc] [-S] <source-file>", result.StandardError);
  }

  [Fact]
  public void AssemblyOnlyCompilationCreatesAssemblyWithoutExecutable()
  {
    using var fixture = new TestFixture("int main(void) { return 7; }");

    var result = RunCompiler("-gcc", "-S", fixture.SourcePath);

    Assert.Equal(0, result.ExitCode);
    Assert.True(File.Exists(fixture.AssemblyPath));
    Assert.False(File.Exists(fixture.ExecutablePath));
    Assert.False(File.Exists(fixture.PreprocessedPath));
  }

  [Fact]
  public void AssemblyOnlyGccCompilationAcceptsFlagsInEitherOrder()
  {
    using var fixture = new TestFixture("int main(void) { return 7; }");

    var result = RunCompiler("-S", "-gcc", fixture.SourcePath);

    Assert.Equal(0, result.ExitCode);
    Assert.True(File.Exists(fixture.AssemblyPath));
    Assert.False(File.Exists(fixture.PreprocessedPath));
  }

  [Fact]
  public void NormalCompilationProducesExecutableAndRemovesIntermediateFiles()
  {
    using var fixture = new TestFixture("int main(void) { return 7; }");

    var result = RunCompiler("-gcc", fixture.SourcePath);

    Assert.Equal(0, result.ExitCode);
    Assert.True(File.Exists(fixture.ExecutablePath));
    Assert.False(File.Exists(fixture.PreprocessedPath));
    Assert.False(File.Exists(fixture.AssemblyPath));

    var executableResult = Process.Start(new ProcessStartInfo
    {
      FileName = fixture.ExecutablePath,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true
    });

    Assert.NotNull(executableResult);
    executableResult.WaitForExit();
    Assert.Equal(7, executableResult.ExitCode);
  }

  [Fact]
  public void DefaultCompilationUsesCustomCompilerAndCleansPreprocessedFile()
  {
    using var fixture = new TestFixture("int main(void) { return 7; }");

    var result = RunCompiler(fixture.SourcePath);

    Assert.Equal(1, result.ExitCode);
    Assert.False(File.Exists(fixture.PreprocessedPath));
    Assert.False(File.Exists(fixture.ExecutablePath));
  }

  [Fact]
  public void DuplicateFlagsPrintUsageAndFail()
  {
    using var fixture = new TestFixture("int main(void) { return 7; }");

    var result = RunCompiler("-gcc", "-gcc", fixture.SourcePath);

    Assert.Equal(1, result.ExitCode);
    Assert.Contains("Usage: Compiler [-gcc] [-S] <source-file>", result.StandardError);
  }

  [Fact]
  public void PreprocessingFailurePropagatesDiagnosticsAndFails()
  {
    using var fixture = new TestFixture("int main(void) { return 0; }");
    File.WriteAllText(fixture.SourcePath, "#include <header-that-does-not-exist.h>\n");

    var result = RunCompiler(fixture.SourcePath);

    Assert.NotEqual(0, result.ExitCode);
    Assert.Contains("header-that-does-not-exist.h", result.StandardError);
  }

  [Fact]
  public void CompilerFailurePropagatesDiagnosticsAndFails()
  {
    using var fixture = new TestFixture("int main( { return 0; }");

    var result = RunCompiler("-gcc", fixture.SourcePath);

    Assert.NotEqual(0, result.ExitCode);
    Assert.Contains("error", result.StandardError, StringComparison.OrdinalIgnoreCase);
    Assert.False(File.Exists(fixture.PreprocessedPath));
  }

  [Fact]
  public void MissingSourceFailsWithDiagnostics()
  {
    using var fixture = new TestFixture("int main(void) { return 0; }");
    File.Delete(fixture.SourcePath);

    var result = RunCompiler(fixture.SourcePath);

    Assert.NotEqual(0, result.ExitCode);
    Assert.Contains("No such file or directory", result.StandardError, StringComparison.OrdinalIgnoreCase);
  }

  private static ProcessResult RunCompiler(params string[] arguments)
  {
    var compilerProjectDirectory = FindRepositoryRoot();
    var compilerAssembly = Path.Combine(compilerProjectDirectory, "bin", "Debug", "net11.0", "Compiler.dll");

    Assert.True(File.Exists(compilerAssembly), $"Build the compiler before running integration tests: {compilerAssembly}");

    var startInfo = new ProcessStartInfo
    {
      FileName = "dotnet",
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true
    };
    startInfo.ArgumentList.Add(compilerAssembly);
    foreach (var argument in arguments)
    {
      startInfo.ArgumentList.Add(argument);
    }

    using var process = Process.Start(startInfo);
    Assert.NotNull(process);

    var standardOutput = process.StandardOutput.ReadToEnd();
    var standardError = process.StandardError.ReadToEnd();
    process.WaitForExit();

    return new ProcessResult(process.ExitCode, standardOutput, standardError);
  }

  private static string FindRepositoryRoot()
  {
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Compiler.csproj")))
    {
      directory = directory.Parent;
    }

    return directory?.FullName
      ?? throw new DirectoryNotFoundException("Could not find the compiler project root.");
  }

  private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

  private sealed class TestFixture : IDisposable
  {
    private readonly string directory;

    public TestFixture(string source)
    {
      directory = Directory.CreateTempSubdirectory("compiler-test-").FullName;
      SourcePath = Path.Combine(directory, "program.c");
      File.WriteAllText(SourcePath, source);
    }

    public string SourcePath { get; }
    public string PreprocessedPath => Path.ChangeExtension(SourcePath, ".i");
    public string AssemblyPath => Path.ChangeExtension(SourcePath, ".s");
    public string ExecutablePath => Path.ChangeExtension(SourcePath, null);

    public void Dispose()
    {
      Directory.Delete(directory, recursive: true);
    }
  }
}
