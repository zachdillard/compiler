if (args.Length != 1)
{
  Console.Error.WriteLine("Usage: Compiler <source-file>");
  return 1;
}

var inputFile = args[0];
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

return new Assembler().Assemble(assemblyFile, outputFile);
