    /// The parameter binder for real CLR objects that have properties and fields decorated with the parameter attributes.
    internal class ReflectionParameterBinder : ParameterBinderBase
        internal ReflectionParameterBinder(
            Cmdlet command)
        /// The dictionary to use to record the parameters set by this object...
            Cmdlet command,
        /// The default value of the specified parameter.
        /// <exception cref="GetValueException">
        /// If the ETS call to get the property value throws an exception.
                return GetGetter(Target.GetType(), name)(Target);
                throw new GetValueInvocationException("CatchFromBaseAdapterGetValueTI",
                    name, inner.Message);
                throw new GetValueInvocationException("CatchFromBaseAdapterGetValue",
                    name, e.Message);
        /// <exception cref="SetValueException">
        /// If the setter raises an exception.
            Diagnostics.Assert(!string.IsNullOrEmpty(name), "caller to verify name parameter");
                var setter = parameterMetadata != null
                    ? (parameterMetadata.Setter ??= GetSetter(Target.GetType(), name))
                    : GetSetter(Target.GetType(), name);
                setter(Target, value);
                throw new SetValueInvocationException("CatchFromBaseAdapterSetValueTI",
                throw new SetValueInvocationException("CatchFromBaseAdapterSetValue",
        #endregion Internal members
        static ReflectionParameterBinder()
            // Statically add delegates that we typically need on startup or every time we run PowerShell - this avoids the JIT
            s_getterMethods.TryAdd(Tuple.Create(typeof(OutDefaultCommand), "InputObject"), static o => ((OutDefaultCommand)o).InputObject);
            s_setterMethods.TryAdd(Tuple.Create(typeof(OutDefaultCommand), "InputObject"), static (o, v) => ((OutDefaultCommand)o).InputObject = (PSObject)v);
            s_getterMethods.TryAdd(Tuple.Create(typeof(OutLineOutputCommand), "InputObject"), static o => ((OutLineOutputCommand)o).InputObject);
            s_getterMethods.TryAdd(Tuple.Create(typeof(OutLineOutputCommand), "LineOutput"), static o => ((OutLineOutputCommand)o).LineOutput);
            s_setterMethods.TryAdd(Tuple.Create(typeof(OutLineOutputCommand), "InputObject"), static (o, v) => ((OutLineOutputCommand)o).InputObject = (PSObject)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(OutLineOutputCommand), "LineOutput"), static (o, v) => ((OutLineOutputCommand)o).LineOutput = v);
            s_getterMethods.TryAdd(Tuple.Create(typeof(FormatDefaultCommand), "InputObject"), static o => ((FormatDefaultCommand)o).InputObject);
            s_setterMethods.TryAdd(Tuple.Create(typeof(FormatDefaultCommand), "InputObject"), static (o, v) => ((FormatDefaultCommand)o).InputObject = (PSObject)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(SetStrictModeCommand), "Off"), static (o, v) => ((SetStrictModeCommand)o).Off = (SwitchParameter)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(SetStrictModeCommand), "Version"), static (o, v) => ((SetStrictModeCommand)o).Version = (Version)v);
            s_getterMethods.TryAdd(Tuple.Create(typeof(ForEachObjectCommand), "InputObject"), static o => ((ForEachObjectCommand)o).InputObject);
            s_setterMethods.TryAdd(Tuple.Create(typeof(ForEachObjectCommand), "InputObject"), static (o, v) => ((ForEachObjectCommand)o).InputObject = (PSObject)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(ForEachObjectCommand), "Process"), static (o, v) => ((ForEachObjectCommand)o).Process = (ScriptBlock[])v);
            s_getterMethods.TryAdd(Tuple.Create(typeof(WhereObjectCommand), "InputObject"), static o => ((WhereObjectCommand)o).InputObject);
            s_setterMethods.TryAdd(Tuple.Create(typeof(WhereObjectCommand), "InputObject"), static (o, v) => ((WhereObjectCommand)o).InputObject = (PSObject)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(WhereObjectCommand), "FilterScript"), static (o, v) => ((WhereObjectCommand)o).FilterScript = (ScriptBlock)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(ImportModuleCommand), "Name"), static (o, v) => ((ImportModuleCommand)o).Name = (string[])v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(ImportModuleCommand), "ModuleInfo"), static (o, v) => ((ImportModuleCommand)o).ModuleInfo = (PSModuleInfo[])v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(ImportModuleCommand), "Scope"), static (o, v) => ((ImportModuleCommand)o).Scope = (string)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(ImportModuleCommand), "PassThru"), static (o, v) => ((ImportModuleCommand)o).PassThru = (SwitchParameter)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(GetCommandCommand), "Name"), static (o, v) => ((GetCommandCommand)o).Name = (string[])v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(GetCommandCommand), "Module"), static (o, v) => ((GetCommandCommand)o).Module = (string[])v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(GetModuleCommand), "Name"), static (o, v) => ((GetModuleCommand)o).Name = (string[])v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(GetModuleCommand), "ListAvailable"), static (o, v) => ((GetModuleCommand)o).ListAvailable = (SwitchParameter)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(GetModuleCommand), "FullyQualifiedName"), static (o, v) => ((GetModuleCommand)o).FullyQualifiedName = (ModuleSpecification[])v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "ErrorAction"),
                (o, v) =>
                    v ??= LanguagePrimitives.ThrowInvalidCastException(null, typeof(ActionPreference));
                    ((CommonParameters)o).ErrorAction = (ActionPreference)v;
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "WarningAction"),
                    ((CommonParameters)o).WarningAction = (ActionPreference)v;
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "InformationAction"),
                    ((CommonParameters)o).InformationAction = (ActionPreference)v;
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "ProgressAction"),
                    ((CommonParameters)o).ProgressAction = (ActionPreference)v;
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "Verbose"), static (o, v) => ((CommonParameters)o).Verbose = (SwitchParameter)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "Debug"), static (o, v) => ((CommonParameters)o).Debug = (SwitchParameter)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "ErrorVariable"), static (o, v) => ((CommonParameters)o).ErrorVariable = (string)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "WarningVariable"), static (o, v) => ((CommonParameters)o).WarningVariable = (string)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "InformationVariable"), static (o, v) => ((CommonParameters)o).InformationVariable = (string)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "OutVariable"), static (o, v) => ((CommonParameters)o).OutVariable = (string)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "OutBuffer"), static (o, v) => ((CommonParameters)o).OutBuffer = (int)v);
            s_setterMethods.TryAdd(Tuple.Create(typeof(CommonParameters), "PipelineVariable"), static (o, v) => ((CommonParameters)o).PipelineVariable = (string)v);
        private static readonly ConcurrentDictionary<Tuple<Type, string>, Func<object, object>> s_getterMethods
            = new ConcurrentDictionary<Tuple<Type, string>, Func<object, object>>();
        private static readonly ConcurrentDictionary<Tuple<Type, string>, Action<object, object>> s_setterMethods =
            new ConcurrentDictionary<Tuple<Type, string>, Action<object, object>>();
        private static Func<object, object> GetGetter(Type type, string property)
            return s_getterMethods.GetOrAdd(Tuple.Create(type, property),
                (Tuple<Type, string> _) =>
                    var target = Expression.Parameter(typeof(object));
                    return Expression.Lambda<Func<object, object>>(
                        Expression.Convert(
                            GetPropertyOrFieldExpr(type, property, Expression.Convert(target, type)),
                            typeof(object)),
                        new[] { target }).Compile();
        private static Action<object, object> GetSetter(Type type, string property)
            return s_setterMethods.GetOrAdd(Tuple.Create(type, property),
                _ =>
                    var propertyExpr = GetPropertyOrFieldExpr(type, property, Expression.Convert(target, type));
                    Expression expr = Expression.Assign(propertyExpr, Expression.Convert(value, propertyExpr.Type));
                    if (propertyExpr.Type.IsValueType && Nullable.GetUnderlyingType(propertyExpr.Type) == null)
                        var throwInvalidCastExceptionExpr =
                            Expression.Call(Language.CachedReflectionInfo.LanguagePrimitives_ThrowInvalidCastException,
                                            Language.ExpressionCache.NullConstant,
                                            Expression.Constant(propertyExpr.Type, typeof(Type)));
                        // The return type of 'ThrowInvalidCastException' is System.Object, but the method actually always
                        // throws 'PSInvalidCastException' when it's executed. So converting 'throwInvalidCastExceptionExpr'
                        // to 'propertyExpr.Type' is fine, because the conversion will never be hit.
                        expr = Expression.Condition(Expression.Equal(value, Language.ExpressionCache.NullConstant),
                                                    Expression.Convert(throwInvalidCastExceptionExpr, propertyExpr.Type),
                                                    expr);
                    return Expression.Lambda<Action<object, object>>(expr, new[] { target, value }).Compile();
        private static Expression GetPropertyOrFieldExpr(Type type, string name, Expression target)
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
                var propertyInfo = type.GetProperty(name, bindingFlags);
                    return Expression.Property(target, propertyInfo);
                // This is uncommon - in C#, there is "new" property that hides a base property.
                // To get the correct property, get all properties, and assume the first that matches
                // the name we want is the correct one.  This seems fragile, but the DotNetAdapter
                // does the same thing
                foreach (var propertyInfo in type.GetProperties(bindingFlags))
                    if (propertyInfo.Name.Equals(name, StringComparison.Ordinal))
                var fieldInfo = type.GetField(name, bindingFlags);
                    return Expression.Field(target, fieldInfo);
                foreach (var fieldInfo in type.GetFields(bindingFlags))
                    if (fieldInfo.Name.Equals(name, StringComparison.Ordinal))
            Diagnostics.Assert(false, "Can't find property or field?");
        #endregion Private members
