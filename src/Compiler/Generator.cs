public class Generator
{
    public Assembly.Program Generate(C.Program program)
    {
        Assembly.Function function = new(
            program.Function.Identifier,
            [
                new Assembly.Mov(new Assembly.Imm(program.Function.Statement.Expression.Value), new Assembly.Register()),
                new Assembly.Ret()
            ]);
        return new Assembly.Program(function);
    }
}
