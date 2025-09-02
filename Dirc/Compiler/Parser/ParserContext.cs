namespace Dirc.Compiling.Parsing;

class ParserContext
{
    public ParserBase ParserBase { get; set; }
    public ArrayParser ArrayParser { get; set; }
    public ControlFlowParser ControlFlowParser { get; set; }
    public ExpressionParser ExpressionParser { get; set; }
    public FunctionParser FunctionParser { get; set; }
    public OperatorPrecedence OperatorPrecedence { get; set; }
    public PointerParser PointerParser { get; set; }
    public StatementParser StatementParser { get; set; }
    public TypeParser TypeParser { get; set; }
    public VariableParser VariableParser { get; set; }

    public ParserContext(ParserBase parserBase)
    {
        ParserBase = parserBase;
        OperatorPrecedence = new OperatorPrecedence();
        ArrayParser = new ArrayParser(this);
        ControlFlowParser = new ControlFlowParser(this);
        ExpressionParser = new ExpressionParser(this);
        FunctionParser = new FunctionParser(this);
        PointerParser = new PointerParser(this);
        StatementParser = new StatementParser(this);
        TypeParser = new TypeParser(this);
        VariableParser = new VariableParser(this);
    }
}
