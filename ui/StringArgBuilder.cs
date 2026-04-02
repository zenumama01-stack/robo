    internal class StringArgBuilder : SimpleArgBuilder
        internal StringArgBuilder(Type parameterType)
            Debug.Assert(parameterType == typeof(string) || parameterType == typeof(BStrWrapper));
            _isWrapper = parameterType == typeof(BStrWrapper);
                    Helpers.Convert(parameter, typeof(BStrWrapper)),
                    typeof(BStrWrapper).GetProperty(nameof(BStrWrapper.WrappedObject))
            return parameter;
            // Marshal.StringToBSTR(parameter)
                typeof(Marshal).GetMethod(nameof(System.Runtime.InteropServices.Marshal.StringToBSTR)),
            // value == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(value);
                Expression.Constant(null, typeof(string)),   // default value
                    typeof(Marshal).GetMethod(nameof(System.Runtime.InteropServices.Marshal.PtrToStringBSTR)),
                    typeof(BStrWrapper).GetConstructor(new Type[] { typeof(string) }),
