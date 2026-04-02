using BenchmarkDotNet.Validators;
    /// this class makes sure that every benchmark belongs to a mandatory category
    /// categories are used by the CI for filtering
    public class MandatoryCategoryValidator : IValidator
        private readonly ImmutableHashSet<string> _mandatoryCategories;
        public bool TreatsWarningsAsErrors => true;
        public MandatoryCategoryValidator(ImmutableHashSet<string> categories) => _mandatoryCategories = categories;
        public IEnumerable<ValidationError> Validate(ValidationParameters validationParameters)
            => validationParameters.Benchmarks
                .Where(benchmark => !benchmark.Descriptor.Categories.Any(category => _mandatoryCategories.Contains(category)))
                .Select(benchmark => benchmark.Descriptor.GetFilterName())
                .Select(benchmarkId =>
                    new ValidationError(
                        isCritical: TreatsWarningsAsErrors,
                        $"{benchmarkId} does not belong to one of the mandatory categories: {string.Join(", ", _mandatoryCategories)}. Use [BenchmarkCategory(Categories.$)]")
