using Xunit;

namespace Compiler.Tests;

public class PipelineTests
{
  [Fact]
  public void Run_ChapterOneSource_ProducesExpectedAssembly()
  {
    const string source = "int main(void) { return 42; }";
    var expected = string.Join(Environment.NewLine,
      "\t.globl _main", "_main:", "\tmovl $42, %eax", "\tret", "");

    var tokens = Lexer.Run(source);
    var program = Parser.Run(tokens);
    var assembly = Generator.Run(program);
    var text = Emitter.Run(assembly);

    Assert.Equal(expected, text);
  }
}
