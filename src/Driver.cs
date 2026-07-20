const string usage = "Usage: Compiler [-gcc] [-S] <source-file>";

if (args.Length < 1 || args.Length > 3)
{
  Console.Error.WriteLine(usage);
  return 1;
}

var assemblyOnly = false;
var useGcc = false;
string? inputFile = null;

foreach (var argument in args)
{
  switch (argument)
  {
    case "-S" when !assemblyOnly:
      assemblyOnly = true;
      break;
    case "-gcc" when !useGcc:
      useGcc = true;
      break;
    case var _ when argument.StartsWith('-'):
      Console.Error.WriteLine(usage);
      return 1;
    case var _ when inputFile is null:
      inputFile = argument;
      break;
    case var _:
      Console.Error.WriteLine(usage);
      return 1;
  }
}

if (inputFile is null)
{
  Console.Error.WriteLine(usage);
  return 1;
}

var preprocessedFile = Path.ChangeExtension(inputFile, ".i");
var assemblyFile = Path.ChangeExtension(inputFile, ".s");
var outputFile = Path.ChangeExtension(inputFile, null);

var preprocessingExitCode = new Preprocessor().Run(inputFile, preprocessedFile);
if (preprocessingExitCode != 0)
{
  return preprocessingExitCode;
}

try
{
  var compiler = new Compiler();
  var compilerExitCode = useGcc
    ? compiler.Compile(preprocessedFile, assemblyFile)
    : compiler.Run(preprocessedFile, assemblyFile);
  if (compilerExitCode != 0)
  {
    return compilerExitCode;
  }
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

if (assemblyOnly)
{
  return 0;
}

return new Assembler().Run(assemblyFile, outputFile);
