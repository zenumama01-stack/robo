    internal class ConvertibleArgBuilder : ArgBuilder
            return Helpers.Convert(parameter, typeof(IConvertible));
            //we are not supporting convertible InOut
