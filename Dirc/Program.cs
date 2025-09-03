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

        List<string> files = [];

        foreach (var arg in argsList)
        {
            if (File.Exists(arg))
            {
                files.Add(Path.GetFullPath(arg));
            }
            else
            {
                Console.Error.WriteLine($"Warning: File or pattern not found: {arg}");
            }
        }

        if (files.Count == 0)
        {
            Console.Error.WriteLine("Couldn't find any given input files");
            return;
        }

        BuildOptions buildOptions = new(flags);
        new Driver().Run(files, buildOptions);
    }
}
