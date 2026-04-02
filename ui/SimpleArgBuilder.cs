    /// SimpleArgBuilder produces the value produced by the user as the argument value.  It
    /// also tracks information about the original parameter and is used to create extended
    /// methods for params arrays and param dictionary functions.
    internal class SimpleArgBuilder : ArgBuilder
        internal SimpleArgBuilder(Type parameterType)
            ParameterType = parameterType;
        protected Type ParameterType { get; }
            Debug.Assert(parameter != null);
            return Helpers.Convert(parameter, ParameterType);
            Debug.Assert(newValue != null && newValue.Type.IsAssignableFrom(ParameterType));
            return base.UnmarshalFromRef(newValue);
