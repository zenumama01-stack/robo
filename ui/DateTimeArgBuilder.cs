    internal sealed class DateTimeArgBuilder : SimpleArgBuilder
        internal DateTimeArgBuilder(Type parameterType)
            Debug.Assert(parameterType == typeof(DateTime));
            // parameter.ToOADate()
                typeof(DateTime).GetMethod(nameof(DateTime.ToOADate))
            // DateTime.FromOADate(value)
                    typeof(DateTime).GetMethod(nameof(DateTime.FromOADate)),
