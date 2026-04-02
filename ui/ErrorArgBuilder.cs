    internal class ErrorArgBuilder : SimpleArgBuilder
        internal ErrorArgBuilder(Type parameterType)
            Debug.Assert(parameterType == typeof(ErrorWrapper));
            // parameter.ErrorCode
                Helpers.Convert(base.Marshal(parameter), typeof(ErrorWrapper)),
                nameof(ErrorWrapper.ErrorCode)
            // new ErrorWrapper(value)
                    typeof(ErrorWrapper).GetConstructor(new Type[] { typeof(int) }),
