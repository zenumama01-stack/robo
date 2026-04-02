    internal class ConvertArgBuilder : SimpleArgBuilder
        private readonly Type _marshalType;
        internal ConvertArgBuilder(Type parameterType, Type marshalType)
            _marshalType = marshalType;
            parameter = base.Marshal(parameter);
            return Expression.Convert(parameter, _marshalType);
        internal override Expression UnmarshalFromRef(Expression newValue)
            return base.UnmarshalFromRef(Expression.Convert(newValue, ParameterType));
