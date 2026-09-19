using System.ComponentModel;
using System.Diagnostics;

public class Preprocessor
{
  public int Run(string inputFile, string preprocessedFile)
  {
    var startInfo = new ProcessStartInfo
    {
      FileName = "gcc",
      UseShellExecute = false,
      RedirectStandardError = true,
      CreateNoWindow = true,
      ArgumentList =
      {
        "-E",
        "-P",
        inputFile,
        "-o",
        preprocessedFile
      }
    };

    try
    {
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
  }
}
