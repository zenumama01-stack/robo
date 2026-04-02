    internal class ConversionArgBuilder : ArgBuilder
        private readonly SimpleArgBuilder _innerBuilder;
        private readonly Type _parameterType;
        internal ConversionArgBuilder(Type parameterType, SimpleArgBuilder innerBuilder)
            _innerBuilder = innerBuilder;
        internal override Expression Marshal(Expression parameter)
            return _innerBuilder.Marshal(Helpers.Convert(parameter, _parameterType));
            //we are not supporting conversion InOut
            throw Assert.Unreachable;
