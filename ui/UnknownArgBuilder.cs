    internal class UnknownArgBuilder : SimpleArgBuilder
        internal UnknownArgBuilder(Type parameterType)
            _isWrapper = parameterType == typeof(UnknownWrapper);
                    Helpers.Convert(parameter, typeof(UnknownWrapper)),
                    typeof(UnknownWrapper).GetProperty(nameof(UnknownWrapper.WrappedObject))
            // parameter == null ? IntPtr.Zero : Marshal.GetIUnknownForObject(parameter);
                    typeof(Marshal).GetMethod(nameof(System.Runtime.InteropServices.Marshal.GetIUnknownForObject)),
                    typeof(UnknownWrapper).GetConstructor(new Type[] { typeof(object) }),
