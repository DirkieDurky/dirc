class Function
{
    public string Name;
    public string[] Arguments;
    public int StartingLine;
    public bool Custom;

    public Function(string name, string[] arguments, int startingLine, bool custom)
    {
        Name = name;
        Arguments = arguments;
        StartingLine = startingLine;
        Custom = custom;
    }
}
