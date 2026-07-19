if (args.Length != 1 && args.Length != 2)
{
  Console.Error.WriteLine("Usage: Compiler [-S] <source-file>");
  return 1;
}

var assemblyOnly = args.Length == 2 && args[0] == "-S";
if (args.Length == 2 && !assemblyOnly)
{
  Console.Error.WriteLine("Usage: Compiler [-S] <source-file>");
  return 1;
}

var inputFile = assemblyOnly ? args[1] : args[0];
var preprocessedFile = Path.ChangeExtension(inputFile, ".i");
var assemblyFile = Path.ChangeExtension(inputFile, ".s");
var outputFile = Path.ChangeExtension(inputFile, null);

var preprocessingExitCode = new Preprocessor().Preprocess(inputFile, preprocessedFile);
if (preprocessingExitCode != 0)
{
  return preprocessingExitCode;
}

try
{
  var compilerExitCode = new Compiler().Compile(preprocessedFile, assemblyFile);
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

return new Assembler().Assemble(assemblyFile, outputFile);
