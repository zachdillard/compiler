public class Compiler
{
  public void Compile(string preprocessedFile, string assemblyFile)
  {
    try
    {
      _ = File.ReadAllText(preprocessedFile);
      File.WriteAllText(assemblyFile, string.Empty);
    }
    finally
    {
      File.Delete(preprocessedFile);
    }
  }
}
