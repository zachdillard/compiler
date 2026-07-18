using System.Text;

if (args.Length != 1)
{
  Console.Error.WriteLine("Usage: Compiler <source-file>");
  return 1;
}

try
{
  var contents = File.ReadAllText(args[0], Encoding.UTF8);
  Console.Write(contents);
  return 0;
}
catch (IOException)
{
  Console.Error.WriteLine($"Error: could not read source file '{args[0]}'.");
  return 1;
}
catch (UnauthorizedAccessException)
{
  Console.Error.WriteLine($"Error: could not read source file '{args[0]}'.");
  return 1;
}
