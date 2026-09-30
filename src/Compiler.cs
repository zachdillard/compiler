using System.ComponentModel;
using System.Diagnostics;

public class Compiler
{
  public int Compile(string preprocessedFile, string assemblyFile)
  {
    var startInfo = new ProcessStartInfo
    {
      FileName = "gcc",
      UseShellExecute = false,
      RedirectStandardError = true,
      CreateNoWindow = true,
      ArgumentList =
      {
        "-S",
        preprocessedFile,
        "-o",
        assemblyFile
      }
    };

    try
    {
      // Temporary GCC implementation for end-to-end testing; replace this with a custom compiler.
      using var process = new Process { StartInfo = startInfo };
      process.Start();

      var diagnostics = process.StandardError.ReadToEnd();
      process.WaitForExit();

      Console.Error.Write(diagnostics);
      return process.ExitCode;
    }
    catch (Win32Exception)
    {
      Console.Error.WriteLine("Error: could not start gcc. Ensure gcc is installed and available on PATH.");
      return 1;
    }
    catch (InvalidOperationException)
    {
      Console.Error.WriteLine("Error: could not start gcc.");
      return 1;
    }
    finally
    {
      File.Delete(preprocessedFile);
    }
  }

  public int Run(string preprocessedFile, string assemblyFile, bool lexOnly = false)
  {
    try
    {
      List<Token> tokens = Lexer.Run(File.ReadAllText(preprocessedFile));
      if (lexOnly)
        return 0;

      C.Program program = Parser.Run(tokens);
      Assembly.Program assembly = Generator.Run(program);
      string output = Emitter.Run(assembly);
      Writer.Run(output, assemblyFile);
      return 0;
    }
    catch (InvalidOperationException exception)
    {
      Console.Error.WriteLine($"Error: {exception.Message}");
      return 1;
    }
    finally
    {
      File.Delete(preprocessedFile);
    }
  }
}
