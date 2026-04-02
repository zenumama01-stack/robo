namespace MicroBenchmarks
    public static class Categories
        /// Benchmarks belonging to this category are executed for CI jobs.
        public const string Components = "Components";
        public const string Engine = "Engine";
        /// Benchmarks belonging to this category are targeting internal APIs.
        public const string Internal = "Internal";
        /// Benchmarks belonging to this category are targeting public APIs.
        public const string Public = "Public";
