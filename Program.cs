using System.ComponentModel;
using System.Diagnostics;

if (args.Length != 1)
{
  Console.Error.WriteLine("Usage: Compiler <source-file>");
  return 1;
}

var inputFile = args[0];
var preprocessedFile = Path.ChangeExtension(inputFile, ".i");
var startInfo = new ProcessStartInfo
{
  FileName = "gcc",
  UseShellExecute = false,
  RedirectStandardError = true,
  CreateNoWindow = true
};

startInfo.ArgumentList.Add("-E");
startInfo.ArgumentList.Add("-P");
startInfo.ArgumentList.Add(inputFile);
startInfo.ArgumentList.Add("-o");
startInfo.ArgumentList.Add(preprocessedFile);

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
