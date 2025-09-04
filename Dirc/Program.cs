using CommandLine;
using CommandLine.Text;
using Dirc.Compiling;

namespace Dirc;

class Program
{
    static void Main(string[] args)
    {
        var parser = new Parser(with => { with.HelpWriter = null; with.AutoVersion = false; with.AutoHelp = false; with.CaseInsensitiveEnumValues = true; });
        ParserResult<Options> result = parser.ParseArguments<Options>(args);
        result.WithParsed(options =>
        {
            if (options.InputFiles.Count() == 0)
            {
                Console.WriteLine("No input files given.");
            }

            if (options.LibName != null) options.CompileOnly = true;

            new Driver().Run(options);
        })
        .WithNotParsed(errs => DisplayHelp(result, errs));
    }

    static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
    {
        HelpText helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.AutoVersion = false;
            h.AddEnumValuesToHelpText = true;

            h.Heading = "Usage: dirc [options] file...";
            h.Copyright = "";

            return HelpText.DefaultParsingErrorsHandler(result, h);
        }, e => e);
        Console.WriteLine(helpText);
    }
}
