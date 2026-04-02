using BenchmarkDotNet.Filters;
    class ExclusionFilter : IFilter
        private readonly GlobFilter globFilter;
        public ExclusionFilter(List<string> _filter)
            if (_filter != null && _filter.Count != 0)
                globFilter = new GlobFilter(_filter.ToArray());
        public bool Predicate(BenchmarkCase benchmarkCase)
            if(globFilter == null)
            return !globFilter.Predicate(benchmarkCase);
    class CategoryExclusionFilter : IFilter
        private readonly AnyCategoriesFilter filter;
        public CategoryExclusionFilter(List<string> patterns)
                filter = new AnyCategoriesFilter(patterns.ToArray());
            return !filter.Predicate(benchmarkCase);
