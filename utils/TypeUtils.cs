    internal static class TypeUtils
        //CONFORMING
        internal static Type GetNonNullableType(Type type)
            if (IsNullableType(type))
                return type.GetGenericArguments()[0];
        internal static bool IsNullableType(this Type type)
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        internal static bool AreReferenceAssignable(Type dest, Type src)
            // WARNING: This actually implements "Is this identity assignable and/or reference assignable?"
            if (dest == src)
            if (!dest.IsValueType && !src.IsValueType && AreAssignable(dest, src))
        internal static bool AreAssignable(Type dest, Type src)
            if (dest.IsAssignableFrom(src))
            if (dest.IsArray && src.IsArray && dest.GetArrayRank() == src.GetArrayRank() && AreReferenceAssignable(dest.GetElementType(), src.GetElementType()))
            if (src.IsArray && dest.IsGenericType &&
                (dest.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>)
                || dest.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IList<>)
                || dest.GetGenericTypeDefinition() == typeof(System.Collections.Generic.ICollection<>))
                && dest.GetGenericArguments()[0] == src.GetElementType())
        internal static bool IsImplicitlyConvertible(Type source, Type destination)
            return IsIdentityConversion(source, destination) ||
                IsImplicitNumericConversion(source, destination) ||
                IsImplicitReferenceConversion(source, destination) ||
                IsImplicitBoxingConversion(source, destination);
        internal static bool IsImplicitlyConvertible(Type source, Type destination, bool considerUserDefined)
            return IsImplicitlyConvertible(source, destination) ||
                (considerUserDefined && GetUserDefinedCoercionMethod(source, destination, true) != null);
        internal static MethodInfo GetUserDefinedCoercionMethod(Type convertFrom, Type convertToType, bool implicitOnly)
            // check for implicit coercions first
            Type nnExprType = TypeUtils.GetNonNullableType(convertFrom);
            Type nnConvType = TypeUtils.GetNonNullableType(convertToType);
            // try exact match on types
            MethodInfo[] eMethods = nnExprType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo method = FindConversionOperator(eMethods, convertFrom, convertToType, implicitOnly);
            MethodInfo[] cMethods = nnConvType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            method = FindConversionOperator(cMethods, convertFrom, convertToType, implicitOnly);
            // try lifted conversion
            if (nnExprType != convertFrom || nnConvType != convertToType)
                method =
                    FindConversionOperator(eMethods, nnExprType, nnConvType, implicitOnly) ??
                    FindConversionOperator(cMethods, nnExprType, nnConvType, implicitOnly);
        internal static MethodInfo FindConversionOperator(MethodInfo[] methods, Type typeFrom, Type typeTo, bool implicitOnly)
            foreach (MethodInfo mi in methods)
                if (mi.Name != "op_Implicit" && (implicitOnly || mi.Name != "op_Explicit"))
                if (mi.ReturnType != typeTo)
                ParameterInfo[] pis = mi.GetParameters();
                if (pis[0].ParameterType != typeFrom)
        private static bool IsIdentityConversion(Type source, Type destination)
            return source == destination;
        private static bool IsImplicitNumericConversion(Type source, Type destination)
            TypeCode tcSource = Type.GetTypeCode(source);
            TypeCode tcDest = Type.GetTypeCode(destination);
            switch (tcSource)
                    switch (tcDest)
                case TypeCode.Char:
                    return (tcDest == TypeCode.Double);
        private static bool IsImplicitReferenceConversion(Type source, Type destination)
            return AreAssignable(destination, source);
        private static bool IsImplicitBoxingConversion(Type source, Type destination)
            if (source.IsValueType && (destination == typeof(object) || destination == typeof(System.ValueType)))
            if (source.IsEnum && destination == typeof(System.Enum))
