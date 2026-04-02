    internal sealed class Event_Format_Ps1Xml
                "System.Diagnostics.Eventing.Reader.EventLogRecord",
                ViewsOf_System_Diagnostics_Eventing_Reader_EventLogRecord());
                "System.Diagnostics.Eventing.Reader.EventLogConfiguration",
                ViewsOf_System_Diagnostics_Eventing_Reader_EventLogConfiguration());
                "System.Diagnostics.Eventing.Reader.ProviderMetadata",
                ViewsOf_System_Diagnostics_Eventing_Reader_ProviderMetadata());
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Diagnostics_Eventing_Reader_EventLogRecord()
            yield return new FormatViewDefinition("Default",
                    .GroupByProperty("ProviderName", label: "ProviderName")
                    .AddHeader(Alignment.Right, width: 8)
                        .AddPropertyColumn("TimeCreated")
                        .AddPropertyColumn("LevelDisplayName")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Diagnostics_Eventing_Reader_EventLogConfiguration()
                    .AddHeader(label: "LogMode", width: 9)
                    .AddHeader(Alignment.Right, label: "MaximumSizeInBytes", width: 18)
                    .AddHeader(Alignment.Right, label: "RecordCount", width: 11)
                        .AddPropertyColumn("LogMode")
                        .AddPropertyColumn("MaximumSizeInBytes")
                        .AddPropertyColumn("RecordCount")
                        .AddPropertyColumn("LogName")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Diagnostics_Eventing_Reader_ProviderMetadata()
                        .AddItemProperty(@"LogLinks")
                        .AddItemProperty(@"Opcodes")
                        .AddItemProperty(@"Tasks")
