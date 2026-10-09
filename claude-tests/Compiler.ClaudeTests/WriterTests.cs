namespace ClaudeTests;

public sealed class WriterTests : IDisposable
{
    private readonly string directory = Directory.CreateTempSubdirectory("claude-tests-").FullName;

    public void Dispose() => Directory.Delete(directory, recursive: true);

    [Fact]
    public void WritesExactContent()
    {
        string path = Path.Combine(directory, "out.s");

        Writer.Run("\t.globl _main\n_main:\n", path);

        Assert.Equal("\t.globl _main\n_main:\n", File.ReadAllText(path));
    }

    [Fact]
    public void OverwritesExistingFile()
    {
        string path = Path.Combine(directory, "out.s");
        File.WriteAllText(path, "old content that is longer than the new content");

        Writer.Run("new", path);

        Assert.Equal("new", File.ReadAllText(path));
    }
}
