    internal class DispatchArgBuilder : SimpleArgBuilder
        private readonly bool _isWrapper;
        internal DispatchArgBuilder(Type parameterType)
            _isWrapper = parameterType == typeof(DispatchWrapper);
            if (_isWrapper)
                parameter = Expression.Property(
                    Helpers.Convert(parameter, typeof(DispatchWrapper)),
                    typeof(DispatchWrapper).GetProperty(nameof(DispatchWrapper.WrappedObject))
            return Helpers.Convert(parameter, typeof(object));
            parameter = Marshal(parameter);
            // parameter == null ? IntPtr.Zero : Marshal.GetIDispatchForObject(parameter);
                Expression.Equal(parameter, Expression.Constant(null)),
                Expression.Constant(IntPtr.Zero),
                    typeof(Marshal).GetMethod(nameof(System.Runtime.InteropServices.Marshal.GetIDispatchForObject)),
                    parameter
            // value == IntPtr.Zero ? null : Marshal.GetObjectForIUnknown(value);
            Expression unmarshal = Expression.Condition(
                Expression.Equal(value, Expression.Constant(IntPtr.Zero)),
                Expression.Constant(null),
                    typeof(Marshal).GetMethod(nameof(System.Runtime.InteropServices.Marshal.GetObjectForIUnknown)),
                unmarshal = Expression.New(
                    typeof(DispatchWrapper).GetConstructor(new Type[] { typeof(object) }),
                    unmarshal
            return base.UnmarshalFromRef(unmarshal);
