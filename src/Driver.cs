using System.ComponentModel;
using System.Diagnostics;

const string usage = "Usage: Compiler [-gcc] [-S] [--lex] <source-file>";

if (args.Length < 1 || args.Length > 3)
{
    Console.Error.WriteLine(usage);
    return 1;
}

var assemblyOnly = false;
var lexOnly = false;
var useGcc = false;
string? inputFile = null;

foreach (var argument in args)
{
    switch (argument)
    {
        case "-S" when !assemblyOnly:
            assemblyOnly = true;
            break;
        case "--lex" when !lexOnly:
            lexOnly = true;
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

var preprocessingExitCode = Preprocessor.Run(inputFile, preprocessedFile);
if (preprocessingExitCode != 0)
{
    return preprocessingExitCode;
}

try
{
    try
    {
        if (useGcc && !lexOnly)
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

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var diagnostics = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Console.Error.Write(diagnostics);
            if (process.ExitCode != 0)
            {
                return process.ExitCode;
            }
        }
        else
        {
            List<Token> tokens = Lexer.Run(File.ReadAllText(preprocessedFile));
            if (!lexOnly)
            {
                C.Program program = Parser.Run(tokens);
                Assembly.Program assembly = Generator.Run(program);
                string output = Emitter.Run(assembly);
                Writer.Run(output, assemblyFile);
            }
        }
    }
    catch (Win32Exception) when (useGcc && !lexOnly)
    {
        Console.Error.WriteLine("Error: could not start gcc. Ensure gcc is installed and available on PATH.");
        return 1;
    }
    catch (InvalidOperationException exception)
    {
        Console.Error.WriteLine(useGcc && !lexOnly
            ? "Error: could not start gcc."
            : $"Error: {exception.Message}");
        return 1;
    }
    finally
    {
        File.Delete(preprocessedFile);
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

if (lexOnly || assemblyOnly)
{
    return 0;
}

return Assembler.Run(assemblyFile, outputFile, x86_64: !useGcc);
