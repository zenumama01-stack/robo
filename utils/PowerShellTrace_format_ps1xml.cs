    internal sealed class PowerShellTrace_Format_Ps1Xml
                "System.Management.Automation.PSTraceSource",
                ViewsOf_System_Management_Automation_PSTraceSource());
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSTraceSource()
            yield return new FormatViewDefinition("System.Management.Automation.PSTraceSource",
                        .AddPropertyColumn("Options")
                        .AddPropertyColumn("Listeners")
                        .AddItemProperty(@"Options")
                        .AddItemProperty(@"Listeners")
                        .AddItemProperty(@"Attributes")
                        .AddItemProperty(@"Switch")
