    /// we need to tell our users that having more than 16 test cases per benchmark is a VERY BAD idea
    public class TooManyTestCasesValidator : IValidator
        private const int Limit = 16;
        public static readonly IValidator FailOnError = new TooManyTestCasesValidator();
            var byDescriptor = validationParameters.Benchmarks.GroupBy(benchmark => (benchmark.Descriptor, benchmark.Job)); // descriptor = type + method
            return byDescriptor.Where(benchmarkCase => benchmarkCase.Count() > Limit).Select(group =>
                    isCritical: true,
                    message: $"{group.Key.Descriptor.Type.Name}.{group.Key.Descriptor.WorkloadMethod.Name} has {group.Count()} test cases. It MUST NOT have more than {Limit} test cases. We don't have infinite amount of time to run all the benchmarks!!",
                    benchmarkCase: group.First()));
