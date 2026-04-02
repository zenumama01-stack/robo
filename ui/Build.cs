namespace Reporting
    public sealed class Build
        public string Repo { get; set; }
        public string Branch { get; set; }
        public string Architecture { get; set; }
        public string Locale { get; set; }
        public string GitHash { get; set; }
        public string BuildName { get; set; }
        public DateTime TimeStamp { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();
