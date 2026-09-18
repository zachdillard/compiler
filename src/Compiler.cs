using System.ComponentModel;
using System.Diagnostics;

public enum CompilationStage { Lex, Parse, Codegen, Emit }

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

  public int Run(string preprocessedFile, string assemblyFile, CompilationStage stage)
  {
    try
    {
      string input = File.ReadAllText(preprocessedFile);
      if (stage == CompilationStage.Lex)
      {
        Lexer.Run(input);
        return 0;
      }

      List<Token> tokens = Lexer.Tokenize(input);
      C.Program program = new Parser().Parse(tokens);
      if (stage == CompilationStage.Parse)
        return 0;

      Assembly.Program assembly = new Generator().Generate(program);
      if (stage == CompilationStage.Codegen)
        return 0;

      File.WriteAllText(assemblyFile, new Emitter().Emit(assembly));
      return 0;
    }
    catch (InvalidOperationException exception)
    {
      Console.Error.WriteLine($"Error: {exception.Message}");
      return 1;
    }
    catch (PlatformNotSupportedException exception)
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
