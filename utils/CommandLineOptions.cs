namespace BenchmarkDotNet.Extensions
    public class CommandLineOptions
        // Find and parse given parameter with expected int value, then remove it and its value from the list of arguments to then pass to BenchmarkDotNet
        // Throws ArgumentException if the parameter does not have a value or that value is not parsable as an int
        public static List<string> ParseAndRemoveIntParameter(List<string> argsList, string parameter, out int? parameterValue)
            int parameterIndex = argsList.IndexOf(parameter);
            parameterValue = null;
            if (parameterIndex != -1)
                if (parameterIndex + 1 < argsList.Count && Int32.TryParse(argsList[parameterIndex+1], out int parsedParameterValue))
                    // remove --partition-count args
                    parameterValue = parsedParameterValue;
                    argsList.RemoveAt(parameterIndex+1);
                    argsList.RemoveAt(parameterIndex);
                    throw new ArgumentException($"{parameter} must be followed by an integer");
            return argsList;
        public static List<string> ParseAndRemoveStringsParameter(List<string> argsList, string parameter, out List<string> parameterValue)
            parameterValue = new List<string>();
            if (parameterIndex + 1 < argsList.Count)
                while (parameterIndex + 1 < argsList.Count && !argsList[parameterIndex + 1].StartsWith('-'))
                    // remove each filter string and stop when we get to the next argument flag
                    parameterValue.Add(argsList[parameterIndex + 1]);
                    argsList.RemoveAt(parameterIndex + 1);
            //We only want to remove the --exclusion-filter if it exists
        public static void ParseAndRemoveBooleanParameter(List<string> argsList, string parameter, out bool parameterValue)
                parameterValue = true;
                parameterValue = false;
        public static void ValidatePartitionParameters(int? count, int? index)
            // Either count and index must both be specified or neither specified
            if (!(count.HasValue == index.HasValue))
                throw new ArgumentException("If either --partition-count or --partition-index is specified, both must be specified");
            // Check values of count and index parameters
            else if (count.HasValue && index.HasValue)
                if (count < 2)
                    throw new ArgumentException("When specified, value of --partition-count must be greater than 1");
                else if (!(index < count))
                    throw new ArgumentException("Value of --partition-index must be less than --partition-count");
                else if (index < 0)
                    throw new ArgumentException("Value of --partition-index must be greater than or equal to 0");
using CommandLine;
using CommandLine.Text;
namespace ResultsComparer
        [Option("base", HelpText = "Path to the folder/file with base results.")]
        public string BasePath { get; set; }
        [Option("diff", HelpText = "Path to the folder/file with diff results.")]
        public string DiffPath { get; set; }
        [Option("threshold", Required = true, HelpText = "Threshold for Statistical Test. Examples: 5%, 10ms, 100ns, 1s.")]
        public string StatisticalTestThreshold { get; set; }
        [Option("noise", HelpText = "Noise threshold for Statistical Test. The difference for 1.0ns and 1.1ns is 10%, but it's just a noise. Examples: 0.5ns 1ns.", Default = "0.3ns" )]
        public string NoiseThreshold { get; set; }
        [Option("top", HelpText = "Filter the diff to top/bottom N results. Optional.")]
        public int? TopCount { get; set; }
        [Option("csv", HelpText = "Path to exported CSV results. Optional.")]
        public FileInfo CsvPath { get; set; }
        [Option("xml", HelpText = "Path to exported XML results. Optional.")]
        public FileInfo XmlPath { get; set; }
        [Option('f', "filter", HelpText = "Filter the benchmarks by name using glob pattern(s). Optional.")]
        public IEnumerable<string> Filters { get; set; }
        [Usage(ApplicationAlias = "")]
        public static IEnumerable<Example> Examples
                yield return new Example(@"Compare the results stored in 'C:\results\win' (base) vs 'C:\results\unix' (diff) using 5% threshold.",
                    new CommandLineOptions { BasePath = @"C:\results\win", DiffPath = @"C:\results\unix", StatisticalTestThreshold = "5%" });
                yield return new Example(@"Compare the results stored in 'C:\results\win' (base) vs 'C:\results\unix' (diff) using 5% threshold and show only top/bottom 10 results.",
                    new CommandLineOptions { BasePath = @"C:\results\win", DiffPath = @"C:\results\unix", StatisticalTestThreshold = "5%", TopCount = 10 });
                yield return new Example(@"Compare the results stored in 'C:\results\win' (base) vs 'C:\results\unix' (diff) using 5% threshold and 0.5ns noise filter.",
                    new CommandLineOptions { BasePath = @"C:\results\win", DiffPath = @"C:\results\unix", StatisticalTestThreshold = "5%", NoiseThreshold = "0.5ns" });
                yield return new Example(@"Compare the System.Math benchmark results stored in 'C:\results\ubuntu16' (base) vs 'C:\results\ubuntu18' (diff) using 5% threshold.",
                    new CommandLineOptions { Filters = new[] { "System.Math*" }, BasePath = @"C:\results\win", DiffPath = @"C:\results\unix", StatisticalTestThreshold = "5%" });
