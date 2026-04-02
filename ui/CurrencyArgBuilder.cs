    internal sealed class CurrencyArgBuilder : SimpleArgBuilder
        internal CurrencyArgBuilder(Type parameterType)
            Debug.Assert(parameterType == typeof(CurrencyWrapper));
            // parameter.WrappedObject
                Helpers.Convert(base.Marshal(parameter), typeof(CurrencyWrapper)),
                nameof(CurrencyWrapper.WrappedObject)
            // Decimal.ToOACurrency(parameter.WrappedObject)
                typeof(decimal).GetMethod(nameof(decimal.ToOACurrency)),
                Marshal(parameter)
            // Decimal.FromOACurrency(value)
                    typeof(CurrencyWrapper).GetConstructor(new Type[] { typeof(decimal) }),
                        typeof(decimal).GetMethod(nameof(decimal.FromOACurrency)),
                        value
