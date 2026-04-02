    public class CounterSet
        internal CounterSet(string setName,
                            string machineName,
                            PerformanceCounterCategoryType categoryType,
                            string setHelp,
                            ref Dictionary<string, string[]> counterInstanceMapping)
            CounterSetName = setName;
            if (machineName == null || machineName.Length == 0)
                machineName = ".";
                MachineName = machineName;
                if (!MachineName.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
                    MachineName = @"\\" + MachineName;
            CounterSetType = categoryType;
            Description = setHelp;
            CounterInstanceMapping = counterInstanceMapping;
        public string CounterSetName { get; } = string.Empty;
        public string MachineName { get; } = ".";
        public PerformanceCounterCategoryType CounterSetType { get; }
        public string Description { get; } = string.Empty;
        internal Dictionary<string, string[]> CounterInstanceMapping { get; }
        public StringCollection Paths
                StringCollection retColl = new();
                foreach (string counterName in this.CounterInstanceMapping.Keys)
                    string path;
                    if (CounterInstanceMapping[counterName].Length != 0)
                        path = (MachineName == ".") ?
                          ("\\" + CounterSetName + "(*)\\" + counterName) :
                          (MachineName + "\\" + CounterSetName + "(*)\\" + counterName);
                         ("\\" + CounterSetName + "\\" + counterName) :
                         (MachineName + "\\" + CounterSetName + "\\" + counterName);
                    retColl.Add(path);
                return retColl;
        public StringCollection PathsWithInstances
                foreach (string counterName in CounterInstanceMapping.Keys)
                    foreach (string instanceName in CounterInstanceMapping[counterName])
                        string path = (MachineName == ".") ?
                          ("\\" + CounterSetName + "(" + instanceName + ")\\" + counterName) :
                          (MachineName + "\\" + CounterSetName + "(" + instanceName + ")\\" + counterName);
