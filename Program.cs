using System.ComponentModel;
using System.Diagnostics;

if (args.Length != 1)
{
  Console.Error.WriteLine("Usage: Compiler <source-file>");
  return 1;
}

var inputFile = args[0];
var preprocessedFile = Path.ChangeExtension(inputFile, ".i");
var assemblyFile = Path.ChangeExtension(inputFile, ".s");
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
  if (process.ExitCode != 0)
  {
    return process.ExitCode;
  }

  try
  {
    new Compiler().Compile(preprocessedFile, assemblyFile);
    return 0;
  }
  catch (IOException)
  {
    Console.Error.WriteLine("Error: could not create the assembly file.");
    return 1;
  }
  catch (UnauthorizedAccessException)
  {
    Console.Error.WriteLine("Error: could not access a compiler file.");
    return 1;
  }
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
