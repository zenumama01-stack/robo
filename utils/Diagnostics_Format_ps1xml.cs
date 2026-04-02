    internal sealed class Diagnostics_Format_Ps1Xml
                "Microsoft.PowerShell.Commands.GetCounter.PerformanceCounterSampleSet",
                ViewsOf_Microsoft_PowerShell_Commands_GetCounter_PerformanceCounterSampleSet());
                "Microsoft.PowerShell.Commands.GetCounter.CounterFileInfo",
                ViewsOf_Microsoft_PowerShell_Commands_GetCounter_CounterFileInfo());
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_GetCounter_PerformanceCounterSampleSet()
            yield return new FormatViewDefinition("Counter",
                    .AddHeader(Alignment.Left, label: "Timestamp", width: 26)
                    .AddHeader(Alignment.Left, label: "CounterSamples", width: 100)
                    .StartRowDefinition(wrap: true)
                        .AddPropertyColumn("Timestamp")
                        .AddPropertyColumn("Readings")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_GetCounter_CounterFileInfo()
                    .AddHeader(Alignment.Left, width: 30)
                        .AddPropertyColumn("OldestRecord")
                        .AddPropertyColumn("NewestRecord")
                        .AddPropertyColumn("SampleCount")
