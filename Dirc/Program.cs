using Dirc.Compiling;

namespace Dirc;

class Program
{
    static void Main(string[] args)
    {
        List<string> flags = new();
        List<string> argsList = new();
        foreach (string arg in args)
        {
            if (arg.StartsWith("-"))
            {
                flags.Add(arg);
            }
            else
            {
                argsList.Add(arg);
            }
        }

        if (argsList.Count == 0)
        {
            Console.WriteLine("No file argument provided.");
            return;
        }

        string sourcePath = argsList[0];

        if (File.Exists(sourcePath))
        {
            BuildOptions buildOptions = new(flags);
            new Driver().Run(sourcePath, buildOptions);
        }
        else
        {
            Console.WriteLine($"No file or directory found at {sourcePath}");
        }
    }
}
