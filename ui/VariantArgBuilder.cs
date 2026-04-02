    internal class VariantArgBuilder : SimpleArgBuilder
        internal VariantArgBuilder(Type parameterType)
            _isWrapper = parameterType == typeof(VariantWrapper);
                    Helpers.Convert(parameter, typeof(VariantWrapper)),
                    typeof(VariantWrapper).GetProperty(nameof(VariantWrapper.WrappedObject))
            // parameter == UnsafeMethods.GetVariantForObject(parameter);
                typeof(UnsafeMethods).GetMethod(nameof(UnsafeMethods.GetVariantForObject), BindingFlags.Static | BindingFlags.NonPublic),
            // value == IntPtr.Zero ? null : Marshal.GetObjectForNativeVariant(value);
            Expression unmarshal = Expression.Call(
                typeof(UnsafeMethods).GetMethod(nameof(UnsafeMethods.GetObjectForVariant)),
                    typeof(VariantWrapper).GetConstructor(new Type[] { typeof(object) }),
