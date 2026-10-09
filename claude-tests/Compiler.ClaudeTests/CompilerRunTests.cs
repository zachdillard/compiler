namespace ClaudeTests;

[CollectionDefinition("Console", DisableParallelization = true)]
public class ConsoleCollection;

[Collection("Console")]
public sealed class CompilerRunTests : IDisposable
{
    private readonly string directory = Directory.CreateTempSubdirectory("claude-tests-").FullName;
    private readonly StringWriter error = new();
    private readonly TextWriter originalError = Console.Error;

    public CompilerRunTests()
    {
        Console.SetError(error);
    }

    public void Dispose()
    {
        Console.SetError(originalError);
        Directory.Delete(directory, recursive: true);
    }

    private (string Preprocessed, string Assembly) WriteSource(string source)
    {
        string preprocessed = Path.Combine(directory, "program.i");
        File.WriteAllText(preprocessed, source);
        return (preprocessed, Path.Combine(directory, "program.s"));
    }

    [Fact]
    public void ValidProgramWritesAssemblyAndDeletesPreprocessedFile()
    {
        var (preprocessed, assembly) = WriteSource("int main(void) { return 2; }");

        int exitCode = new Compiler().Run(preprocessed, assembly);

        Assert.Equal(0, exitCode);
        Assert.Equal("\t.globl _main\n_main:\n\tmovl $2, %eax\n\tret\n", File.ReadAllText(assembly));
        Assert.False(File.Exists(preprocessed));
        Assert.Equal("", error.ToString());
    }

    [Fact]
    public void LexOnlyWritesNoAssembly()
    {
        var (preprocessed, assembly) = WriteSource("int main(void) { return 2; }");

        int exitCode = new Compiler().Run(preprocessed, assembly, lexOnly: true);

        Assert.Equal(0, exitCode);
        Assert.False(File.Exists(assembly));
        Assert.False(File.Exists(preprocessed));
    }

    [Fact]
    public void LexOnlySucceedsForProgramThatDoesNotParse()
    {
        var (preprocessed, assembly) = WriteSource("return return ;");

        Assert.Equal(0, new Compiler().Run(preprocessed, assembly, lexOnly: true));
    }

    [Theory]
    [InlineData("int main(void) { return @; }", "Error: Unexpected character: '@'")]
    [InlineData("int main(void) { return 2 }", "Error: Expected Semicolon.")]
    public void InvalidProgramReportsErrorAndCleansUp(string source, string message)
    {
        var (preprocessed, assembly) = WriteSource(source);

        int exitCode = new Compiler().Run(preprocessed, assembly);

        Assert.Equal(1, exitCode);
        Assert.Equal(message, error.ToString().TrimEnd());
        Assert.False(File.Exists(assembly));
        Assert.False(File.Exists(preprocessed));
    }

    [Fact]
    public void OverflowingConstantEscapesButStillDeletesPreprocessedFile()
    {
        // Pins current behavior: OverflowException is not caught, so the driver would crash.
        var (preprocessed, assembly) = WriteSource("int main(void) { return 2147483648; }");

        Assert.Throws<OverflowException>(() => new Compiler().Run(preprocessed, assembly));
        Assert.False(File.Exists(preprocessed));
    }
}
