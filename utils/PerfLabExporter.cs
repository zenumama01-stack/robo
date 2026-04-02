using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using Reporting;
    internal class PerfLabExporter : ExporterBase
        protected override string FileExtension => "json";
        protected override string FileCaption => "perf-lab-report";
        public PerfLabExporter()
        public override void ExportToLog(Summary summary, ILogger logger)
            var reporter = Reporter.CreateReporter();
            DisassemblyDiagnoser disassemblyDiagnoser = summary.Reports
                .FirstOrDefault()? // disassembler was either enabled for all or none of them (so we use the first one)
                .BenchmarkCase.Config.GetDiagnosers().OfType<DisassemblyDiagnoser>().FirstOrDefault();
            foreach (var report in summary.Reports)
                var test = new Test();
                test.Name = FullNameProvider.GetBenchmarkName(report.BenchmarkCase);
                test.Categories = report.BenchmarkCase.Descriptor.Categories;
                var results = from result in report.AllMeasurements
                              where result.IterationMode == Engines.IterationMode.Workload && result.IterationStage == Engines.IterationStage.Result
                              orderby result.LaunchIndex, result.IterationIndex
                              select new { result.Nanoseconds, result.Operations};
                var overheadResults = from result in report.AllMeasurements
                                      where result.IsOverhead() && result.IterationStage != Engines.IterationStage.Jitting
                                      select new { result.Nanoseconds, result.Operations };
                test.Counters.Add(new Counter
                    Name = "Duration of single invocation",
                    TopCounter = true,
                    DefaultCounter = true,
                    HigherIsBetter = false,
                    MetricName = "ns",
                    Results = (from result in results
                               select result.Nanoseconds / result.Operations).ToList()
                    Name = "Overhead invocation",
                    TopCounter = false,
                    DefaultCounter = false,
                    Results = (from result in overheadResults
                    Name = "Duration",
                    MetricName = "ms",
                               select result.Nanoseconds).ToList()
                    Name = "Operations",
                    HigherIsBetter = true,
                    MetricName = "Count",
                               select  (double)result.Operations).ToList()
                foreach (var metric in report.Metrics.Keys)
                    var m = report.Metrics[metric];
                        Name = m.Descriptor.DisplayName,
                        HigherIsBetter = m.Descriptor.TheGreaterTheBetter,
                        MetricName = m.Descriptor.Unit,
                        Results = new[] { m.Value }
                if (disassemblyDiagnoser != null && disassemblyDiagnoser.Results.TryGetValue(report.BenchmarkCase, out var disassemblyResult))
                    string disassembly = DiffableDisassemblyExporter.BuildDisassemblyString(disassemblyResult, disassemblyDiagnoser.Config);
                    test.AdditionalData["disasm"] = disassembly;
                reporter.AddTest(test);
            logger.WriteLine(reporter.GetJson());
