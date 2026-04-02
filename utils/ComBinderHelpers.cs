#pragma warning disable 618 // CurrencyWrapper is obsolete
    internal static class ComBinderHelpers
        internal static bool PreferPut(Type type, bool holdsNull)
            Debug.Assert(type != null);
            if (type.IsValueType
                || type.IsArray
                || type == typeof(string)
                || type == typeof(DBNull)
                || holdsNull
                || type == typeof(System.Reflection.Missing)
                || type == typeof(CurrencyWrapper))
        internal static bool IsByRef(DynamicMetaObject mo)
            return mo.Expression is ParameterExpression pe && pe.IsByRef;
        internal static bool IsPSReferenceArg(DynamicMetaObject o)
            Type t = o.LimitType;
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PSReference<>);
        // This helper prepares arguments for COM binding by transforming ByVal StrongBox arguments
        // into ByRef expressions that represent the argument's Value fields.
        internal static bool[] ProcessArgumentsForCom(ComMethodDesc method, ref DynamicMetaObject[] args,
            List<ParameterExpression> temps, List<Expression> initTemps)
            Debug.Assert(args != null);
            DynamicMetaObject[] newArgs = new DynamicMetaObject[args.Length];
            bool[] isByRefArg = new bool[args.Length];
                DynamicMetaObject curArgument = args[i];
                // set new arg infos to their original values or set default ones
                // we will do this fixup early so that we can assume we always have
                // arginfos in COM binder.
                if (IsByRef(curArgument))
                    newArgs[i] = curArgument;
                    isByRefArg[i] = true;
                    if (IsPSReferenceArg(curArgument))
                        var restrictions = curArgument.Restrictions.Merge(
                            GetTypeRestrictionForDynamicMetaObject(curArgument)
                        // we have restricted this argument to LimitType so we can convert and conversion will be trivial cast.
                        Expression boxedValueAccessor = Expression.Property(
                            Helpers.Convert(
                                curArgument.Expression,
                                curArgument.LimitType
                            curArgument.LimitType.GetProperty("Value")
                        PSReference value = curArgument.Value as PSReference;
                        object boxedValue = value?.Value;
                        newArgs[i] = new DynamicMetaObject(
                            boxedValueAccessor,
                            restrictions,
                            boxedValue
                        if ((method.ParameterInformation != null) && (i < method.ParameterInformation.Length))
                                curArgument.CastOrConvertMethodArgument(
                                    method.ParameterInformation[i].parameterType,
                                    i.ToString(CultureInfo.InvariantCulture),
                                    temps,
                                    initTemps),
                                curArgument.Restrictions);
                        isByRefArg[i] = false;
            args = newArgs;
            return isByRefArg;
        internal static BindingRestrictions GetTypeRestrictionForDynamicMetaObject(DynamicMetaObject obj)
            if (obj.Value == null && obj.HasValue)
                //If the meta object holds a null value, create an instance restriction for checking null
                return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);
            return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
