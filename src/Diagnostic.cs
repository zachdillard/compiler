using System.Text;

public sealed record SourceSpan(string File, int Line, int Column, int Length, string LineText,
  bool Preprocessed = false);

public sealed record DiagnosticNote(string Message, SourceSpan Span);

public sealed class DiagnosticException(string message, SourceSpan span, DiagnosticNote? note = null)
  : Exception(message)
{
  public SourceSpan Span { get; } = span;
  public DiagnosticNote? Note { get; } = note;
}

public static class DiagnosticRenderer
{
  public static string Render(DiagnosticException diagnostic)
  {
    StringBuilder output = new();
    Append(output, "error", diagnostic.Message, diagnostic.Span);
    if (diagnostic.Note is { } note)
      Append(output, "note", note.Message, note.Span);
    if (diagnostic.Span.Preprocessed)
      output.AppendLine("note: columns and source excerpts refer to preprocessed text.");
    return output.ToString();
  }

  private static void Append(StringBuilder output, string severity, string message, SourceSpan span)
  {
    output.AppendLine($"{span.File}:{span.Line}:{span.Column}: {severity}: {message}");
    string prefix = $"{span.Line} | ";
    output.AppendLine(prefix + span.LineText);
    // Preserve tabs in the indentation so the caret aligns with the displayed text.
    string indentation = new(span.LineText.Take(span.Column - 1)
      .Select(character => character == '\t' ? '\t' : ' ').ToArray());
    output.AppendLine(new string(' ', prefix.Length) + indentation + "^" +
      new string('~', Math.Max(0, span.Length - 1)));
  }
}
