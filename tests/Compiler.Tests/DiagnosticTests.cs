using Xunit;

namespace CompilerIntegrationTests;

public sealed class DiagnosticTests
{
  [Fact]
  public void Render_ShowsLocationSourceRangeAndAlignedTabs()
  {
    var diagnostic = new DiagnosticException("invalid literal.",
      new SourceSpan("program.c", 12, 9, 3, "\treturn 12x;"));

    Assert.Equal(string.Join(Environment.NewLine,
      "program.c:12:9: error: invalid literal.",
      "12 | \treturn 12x;",
      "     \t       ^~~", ""), DiagnosticRenderer.Render(diagnostic));
  }

  [Fact]
  public void Render_EndOfInput_ShowsCaretAndRelatedLocation()
  {
    var diagnostic = new DiagnosticException("expected '}'.", new SourceSpan("a.c", 2, 1, 0, ""),
      new DiagnosticNote("opened here.", new SourceSpan("a.c", 1, 1, 1, "{")));

    Assert.Equal(string.Join(Environment.NewLine,
      "a.c:2:1: error: expected '}'.", "2 | ", "    ^",
      "a.c:1:1: note: opened here.", "1 | {", "    ^", ""), DiagnosticRenderer.Render(diagnostic));
  }

  [Fact]
  public void Render_PreprocessedText_ExplainsColumnMapping()
  {
    var diagnostic = new DiagnosticException("invalid token.", new SourceSpan("a.c", 1, 1, 1, "@", true));

    Assert.Equal(string.Join(Environment.NewLine,
      "a.c:1:1: error: invalid token.", "1 | @", "    ^",
      "note: columns and source excerpts refer to preprocessed text.", ""), DiagnosticRenderer.Render(diagnostic));
  }
}
