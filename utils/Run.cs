    public class Run
        public bool Hidden { get; set; }
        public string CorrelationId { get; set; }
        public string PerfRepoHash { get; set; }
        public string Queue { get; set; }
        public IDictionary<string, string> Configurations { get; set; } = new Dictionary<string, string>();
