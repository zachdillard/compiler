using Xunit;

namespace Compiler.Tests;

public class EmitterTests
{
  [Theory]
  [InlineData("main", 0, "0")]
  [InlineData("_entry1", 42, "42")]
  [InlineData("minimum", int.MinValue, "-2147483648")]
  [InlineData("maximum", int.MaxValue, "2147483647")]
  public void Run_AssemblyProgram_ReturnsExpectedText(string identifier, int value, string immediate)
  {
    var program = new Assembly.Program(new Assembly.Function(identifier,
    [
      new Assembly.Mov(new Assembly.Imm(value), new Assembly.Register()),
      new Assembly.Ret()
    ]));
    var expected = string.Join(Environment.NewLine,
      $"\t.globl _{identifier}", $"_{identifier}:", $"\tmovl ${immediate}, %eax", "\tret", "");

    Assert.Equal(expected, Emitter.Run(program));
  }

  [Fact]
  public void Run_MultipleInstructions_PreservesOrder()
  {
    var program = new Assembly.Program(new Assembly.Function("main",
    [
      new Assembly.Mov(new Assembly.Register(), new Assembly.Imm(7)),
      new Assembly.Mov(new Assembly.Imm(42), new Assembly.Register()),
      new Assembly.Ret()
    ]));
    var expected = string.Join(Environment.NewLine,
      "\t.globl _main", "_main:", "\tmovl %eax, $7", "\tmovl $42, %eax", "\tret", "");

    Assert.Equal(expected, Emitter.Run(program));
  }

  [Fact]
  public void Run_EmptyInstructions_EmitsFunctionHeader()
  {
    var program = new Assembly.Program(new Assembly.Function("empty", []));
    var expected = string.Join(Environment.NewLine, "\t.globl _empty", "_empty:", "");

    Assert.Equal(expected, Emitter.Run(program));
  }

  [Fact]
  public void Run_DefaultInstruction_ThrowsDiagnostic()
  {
    var program = new Assembly.Program(new Assembly.Function("main", [default]));

    var exception = Assert.Throws<InvalidOperationException>(() => Emitter.Run(program));

    Assert.Equal("Unknown assembly instruction.", exception.Message);
  }

  [Theory]
  [InlineData(true)]
  [InlineData(false)]
  public void Run_DefaultOperand_ThrowsDiagnostic(bool invalidSource)
  {
    Assembly.Operand source = invalidSource ? default(Assembly.Operand) : new Assembly.Imm(42);
    Assembly.Operand destination = invalidSource ? new Assembly.Register() : default(Assembly.Operand);
    var program = new Assembly.Program(new Assembly.Function("main", [new Assembly.Mov(source, destination)]));

    var exception = Assert.Throws<InvalidOperationException>(() => Emitter.Run(program));

    Assert.Equal("Unknown assembly operand.", exception.Message);
  }
}
