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
}
