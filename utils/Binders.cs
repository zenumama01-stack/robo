    // Item1 - member name
    // Item2 - class containing dynamic site (for protected/private member access)
    // Item3 - static (true) or instance (false)
    // Item4 - enumerating (true) or not (false)
    using PSGetMemberBinderKeyType = Tuple<string, Type, bool, bool>;
    // Item3 - enumerating (true) or not (false)
    using PSSetMemberBinderKeyType = Tuple<string, Type, bool>;
    // Item2 - callinfo (# of args and (not used) named arguments)
    // Item3 - property setter (true) or not (false)
    // Item5 - invocation constraints (casts used in the invocation expression used to guide overload resolution)
    // Item6 - static (true) or instance (false)
    // Item7 - class containing dynamic site (for protected/private member access)
    using PSInvokeMemberBinderKeyType = Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints, bool, Type>;
    // Item1 - callinfo (# of args and (not used) named arguments)
    // Item2 - invocation constraints (casts used in the invocation expression used to guide overload resolution)
    // Item4 - static (true) or instance (false)
    // Item5 - class containing dynamic site (for protected/private member access)
    using PSInvokeDynamicMemberBinderKeyType = Tuple<CallInfo, PSMethodInvocationConstraints, bool, bool, Type>;
    // Item1 - class containing dynamic site (for protected/private member access)
    // Item2 - static (true) or instance (false)
    using PSGetOrSetDynamicMemberBinderKeyType = Tuple<Type, bool>;
    /// Extension methods for DynamicMetaObject.  Some of these extensions help handle PSObject, both in terms of
    /// getting the type or value from a DynamicMetaObject, but also help to generate binding restrictions that
    /// account for values that are optionally wrapped in PSObject.
    internal static class DynamicMetaObjectExtensions
        internal static readonly DynamicMetaObject FakeError
            = new DynamicMetaObject(ExpressionCache.NullConstant, BindingRestrictions.Empty);
        internal static DynamicMetaObject WriteToDebugLog(this DynamicMetaObject obj, DynamicMetaObjectBinder binder)
            if (obj != FakeError)
                System.Diagnostics.Debug.WriteLine("Binder: {0}\r\n    Restrictions: {2}\r\n    Target: {1}",
                                                   binder.ToString(),
                                                   obj.Expression.ToDebugString(),
                                                   obj.Restrictions.ToDebugString());
        internal static BindingRestrictions GetSimpleTypeRestriction(this DynamicMetaObject obj)
            if (obj.Value == null)
                return BindingRestrictions.GetInstanceRestriction(obj.Expression, obj.Value);
            return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.Value.GetType());
        internal static BindingRestrictions PSGetMethodArgumentRestriction(this DynamicMetaObject obj)
            var baseValue = PSObject.Base(obj.Value);
            if (baseValue != null && baseValue.GetType() == typeof(object[]))
                var effectiveArgType = Adapter.EffectiveArgumentType(obj.Value);
                var methodInfo = effectiveArgType != typeof(object[])
                    ? CachedReflectionInfo.PSInvokeMemberBinder_IsHomogeneousArray.MakeGenericMethod(effectiveArgType.GetElementType())
                    : CachedReflectionInfo.PSInvokeMemberBinder_IsHeterogeneousArray;
                BindingRestrictions restrictions;
                if (obj.Value != baseValue)
                    // Need PSObject...
                    restrictions = BindingRestrictions.GetTypeRestriction(obj.Expression, typeof(PSObject));
                    var temp = Expression.Variable(typeof(object[]));
                    test = Expression.Block(
                        Expression.Assign(temp, Expression.TypeAs(Expression.Call(CachedReflectionInfo.PSObject_Base, obj.Expression), typeof(object[]))),
                        Expression.AndAlso(
                            Expression.NotEqual(temp, ExpressionCache.NullObjectArray),
                            Expression.Call(methodInfo, temp)));
                    restrictions = BindingRestrictions.GetTypeRestriction(obj.Expression, typeof(object[]));
                    var arrayExpr = obj.Expression.Cast(typeof(object[]));
                    test = Expression.Call(methodInfo, arrayExpr);
                return restrictions.Merge(BindingRestrictions.GetExpressionRestriction(test));
            return obj.PSGetTypeRestriction();
        internal static BindingRestrictions PSGetStaticMemberRestriction(this DynamicMetaObject obj)
            if (obj.Restrictions != BindingRestrictions.Empty)
                return obj.Restrictions;
                Diagnostics.Assert(obj.Value == AutomationNull.Value, "PSObject.Base should only return null for AutomationNull.Value");
                return BindingRestrictions.GetExpressionRestriction(Expression.Equal(obj.Expression, Expression.Constant(AutomationNull.Value)));
            if (baseValue is Type)
                if (obj.Value == baseValue)
                    //     newObj == oldObj  (if not wrapped in PSObject) or
                    restrictions = BindingRestrictions.GetInstanceRestriction(obj.Expression, obj.Value);
                    //     newObj.GetType() == typeof(PSObject) && PSObject.Base(newObj) == oldObj
                    restrictions = BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
                    restrictions = restrictions.Merge(
                        BindingRestrictions.GetInstanceRestriction(
                            Expression.Call(CachedReflectionInfo.PSObject_Base, obj.Expression),
                            baseValue));
            else if (obj.Value != baseValue)
                // Binding restriction will look like:
                //     newObj.GetType() == typeof(PSObject) && PSObject.Base(newObj).GetType() == typeof(oldType)
                restrictions = BindingRestrictions.GetTypeRestriction(
                    baseValue.GetType());
            return restrictions;
        internal static BindingRestrictions PSGetTypeRestriction(this DynamicMetaObject obj)
            // The default restriction is a simple type test.  We use this type test even if the object is a PSObject,
            // this way we can avoid calling PSObject.Base in all restriction checks.
            var restrictions = BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
                    BindingRestrictions.GetTypeRestriction(Expression.Call(CachedReflectionInfo.PSObject_Base, obj.Expression),
                                                           baseValue.GetType()));
            else if (baseValue is PSObject)
                // We have an empty custom object.  The restrictions must check this explicitly, otherwise we have
                // a simple type test on PSObject, which obviously tests true for many objects.
                // So we end up with:
                //     newObj.GetType() == typeof(PSObject) && PSObject.Base(newObj) == newObj
                        Expression.Equal(Expression.Call(CachedReflectionInfo.PSObject_Base, obj.Expression), obj.Expression)));
        internal static BindingRestrictions CombineRestrictions(this DynamicMetaObject target, params DynamicMetaObject[] args)
            var restrictions = target.Restrictions == BindingRestrictions.Empty ? target.PSGetTypeRestriction() : target.Restrictions;
            for (int index = 0; index < args.Length; index++)
                var r = args[index];
                restrictions =
                    restrictions.Merge(r.Restrictions == BindingRestrictions.Empty
                                           ? r.PSGetTypeRestriction()
                                           : r.Restrictions);
        internal static Expression CastOrConvertMethodArgument(this DynamicMetaObject target,
                                                               List<Expression> initTemps)
            if (target.Value == AutomationNull.Value)
                return Expression.Constant(null, parameterType);
            var argType = target.LimitType;
            if (parameterType == typeof(object) && argType == typeof(PSObject))
                return Expression.Call(CachedReflectionInfo.PSObject_Base, target.Expression.Cast(typeof(PSObject)));
            // If the conversion can't fail, skip wrapping the conversion in try/catch so we generate less code.
            if (parameterType.IsAssignableFrom(argType))
                return target.Expression.Cast(parameterType);
            ConversionRank? rank = null;
            if (parameterType.IsByRefLike && allowCastingToByRefLikeType)
                var conversionResult = PSConvertBinder.ConvertToByRefLikeTypeViaCasting(target, parameterType);
                    return conversionResult;
            var exceptionParam = Expression.Variable(typeof(Exception));
            var targetTemp = Expression.Variable(target.Expression.Type);
            bool debase = false;
            // ConstrainedLanguage note - calls to this conversion are covered by the method resolution algorithm
            // (which ignores method arguments with disallowed types)
            var conversion = rank == ConversionRank.None
                ? LanguagePrimitives.NoConversion
                : LanguagePrimitives.FigureConversion(target.Value, parameterType, out debase);
            var invokeConverter = PSConvertBinder.InvokeConverter(conversion, targetTemp, parameterType, debase, ExpressionCache.InvariantCulture);
            var expr =
                Expression.Block(new[] { targetTemp },
                    Expression.TryCatch(
                            Expression.Assign(targetTemp, target.Expression),
                            invokeConverter),
                        Expression.Catch(exceptionParam,
                                Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_ConvertToArgumentConversionException,
                                                exceptionParam,
                                                Expression.Constant(parameterName),
                                                targetTemp.Cast(typeof(object)),
                                                Expression.Constant(methodName),
                                                Expression.Constant(parameterType, typeof(Type))),
                                Expression.Default(invokeConverter.Type)))));
            var tmp = Expression.Variable(expr.Type);
            initTemps.Add(Expression.Assign(tmp, expr));
        internal static Expression CastOrConvert(this DynamicMetaObject target, Type type)
            if (target.LimitType == type)
                return target.Expression.Cast(type);
            // ConstrainedLanguage note - calls to this conversion are done by:
            // Switch statements (always to Object), method invocation (protected by InvokeMember binder),
            // and hard-coded casts to integral types.
            var conversion = LanguagePrimitives.FigureConversion(target.Value, type, out debase);
            return PSConvertBinder.InvokeConverter(conversion, target.Expression, type, debase, ExpressionCache.InvariantCulture);
        internal static DynamicMetaObject ThrowRuntimeError(this DynamicMetaObject target, DynamicMetaObject[] args,
                                                            BindingRestrictions moreTests, string errorID,
                                                            string resourceString, params Expression[] exceptionArgs)
            return new DynamicMetaObject(Compiler.ThrowRuntimeError(errorID, resourceString, exceptionArgs),
                                         target.CombineRestrictions(args).Merge(moreTests));
        internal static DynamicMetaObject ThrowRuntimeError(this DynamicMetaObject target, BindingRestrictions bindingRestrictions,
                                                            string errorID, string resourceString, params Expression[] exceptionArgs)
                                         bindingRestrictions);
        internal static string ToDebugString(this BindingRestrictions restrictions)
            return restrictions.ToExpression().ToDebugString();
    internal static class DynamicMetaObjectBinderExtensions
        internal static DynamicMetaObject DeferForPSObject(this DynamicMetaObjectBinder binder, DynamicMetaObject target, bool targetIsComObject = false)
            Diagnostics.Assert(target.Value is PSObject, "target must be a psobject");
            BindingRestrictions restrictions = BindingRestrictions.Empty;
            Expression expr = ProcessOnePSObject(target, ref restrictions, argIsComObject: targetIsComObject);
            return new DynamicMetaObject(DynamicExpression.Dynamic(binder, binder.ReturnType, expr), restrictions);
        internal static DynamicMetaObject DeferForPSObject(this DynamicMetaObjectBinder binder, DynamicMetaObject target, DynamicMetaObject arg, bool targetIsComObject = false)
            Diagnostics.Assert(target.Value is PSObject || arg.Value is PSObject, "At least one arg must be a psobject");
            Expression expr1 = ProcessOnePSObject(target, ref restrictions, argIsComObject: targetIsComObject);
            Expression expr2 = ProcessOnePSObject(arg, ref restrictions, argIsComObject: false);
            return new DynamicMetaObject(DynamicExpression.Dynamic(binder, binder.ReturnType, expr1, expr2), restrictions);
        internal static DynamicMetaObject DeferForPSObject(this DynamicMetaObjectBinder binder, DynamicMetaObject[] args, bool targetIsComObject = false)
            Diagnostics.Assert(args != null && args.Length > 0, "args should not be null or empty");
            Diagnostics.Assert(args.Any(mo => mo.Value is PSObject), "At least one arg must be a psobject");
            Expression[] exprs = new Expression[args.Length];
            // Target maps to arg[0] of the binder.
            exprs[0] = ProcessOnePSObject(args[0], ref restrictions, targetIsComObject);
            for (int i = 1; i < args.Length; i++)
                exprs[i] = ProcessOnePSObject(args[i], ref restrictions, argIsComObject: false);
        private static Expression ProcessOnePSObject(DynamicMetaObject arg, ref BindingRestrictions restrictions, bool argIsComObject = false)
            Expression expr = null;
            object baseValue = PSObject.Base(arg.Value);
            if (baseValue != arg.Value)
                expr = Expression.Call(CachedReflectionInfo.PSObject_Base, arg.Expression.Cast(typeof(object)));
                if (argIsComObject)
                    // The 'base' is a COM object, so bake that in the rule.
                    restrictions = restrictions
                        .Merge(arg.GetSimpleTypeRestriction())
                        .Merge(BindingRestrictions.GetExpressionRestriction(Expression.Call(CachedReflectionInfo.Utils_IsComObject, expr)));
                    // Use a more general condition for the rule: 'arg' is a PSObject and 'base != arg'.
                        .Merge(BindingRestrictions.GetExpressionRestriction(Expression.NotEqual(expr, arg.Expression)));
                expr = arg.Expression;
                restrictions = restrictions.Merge(arg.PSGetTypeRestriction());
        internal static DynamicMetaObject UpdateComRestrictionsForPsObject(this DynamicMetaObject binder, DynamicMetaObject[] args)
            // Add a restriction that prevents PSObject arguments (so that they get based)
            BindingRestrictions newRestrictions = binder.Restrictions;
            newRestrictions = args.Aggregate(newRestrictions, (current, arg) =>
                if (arg.LimitType.IsValueType)
                    return current.Merge(arg.GetSimpleTypeRestriction());
                    return current.Merge(BindingRestrictions.GetExpressionRestriction(
                            Expression.Call(CachedReflectionInfo.PSObject_Base, arg.Expression),
                            arg.Expression)));
            return new DynamicMetaObject(binder.Expression, newRestrictions);
    internal static class BinderUtils
        internal static BindingRestrictions GetVersionCheck(DynamicMetaObjectBinder binder, int expectedVersionNumber)
            return BindingRestrictions.GetExpressionRestriction(
                Expression.Equal(Expression.Field(Expression.Constant(binder), "_version"),
                                 ExpressionCache.Constant(expectedVersionNumber)));
        internal static BindingRestrictions GetLanguageModeCheckIfHasEverUsedConstrainedLanguage()
            // Also add a language mode check to detect toggling between language modes
                var tmp = Expression.Variable(typeof(ExecutionContext));
                var langModeFromContext = Expression.Property(tmp, CachedReflectionInfo.ExecutionContext_LanguageMode);
                var constrainedLanguageMode = Expression.Constant(PSLanguageMode.ConstrainedLanguage);
                // Execution context might be null if we're called from a thread with no runspace (e.g. a PSObject
                // is used in some C# w/ dynamic). This is sometimes fine, we don't always need a runspace to access
                // properties.
                Expression test = context?.LanguageMode == PSLanguageMode.ConstrainedLanguage
                          Expression.NotEqual(tmp, ExpressionCache.NullExecutionContext),
                          Expression.Equal(langModeFromContext, constrainedLanguageMode))
                    : Expression.OrElse(
                          Expression.Equal(tmp, ExpressionCache.NullExecutionContext),
                          Expression.NotEqual(langModeFromContext, constrainedLanguageMode));
                        Expression.Assign(tmp, ExpressionCache.GetExecutionContextFromTLS),
                        test));
            return BindingRestrictions.Empty;
        internal static BindingRestrictions GetOptionalVersionAndLanguageCheckForType(DynamicMetaObjectBinder binder, Type targetType, int expectedVersionNumber)
            BindingRestrictions additionalBindingRestrictions = BindingRestrictions.Empty;
            // If this uses a potentially unsafe type, we also need a version check
            if (!CoreTypes.Contains(targetType))
                if (expectedVersionNumber != -1)
                    additionalBindingRestrictions = additionalBindingRestrictions.Merge(BinderUtils.GetVersionCheck(binder, expectedVersionNumber));
                additionalBindingRestrictions = additionalBindingRestrictions.Merge(GetLanguageModeCheckIfHasEverUsedConstrainedLanguage());
            return additionalBindingRestrictions;
    #region PowerShell non-standard language binders
    /// Some classes that implement IEnumerable are not considered as enumerable from the perspective of pipelines,
    /// this binder implements those semantics.
    /// The standard interop ConvertBinder is used to allow third party dynamic objects to get the first chance
    /// at the conversion in case they do support enumeration, but do not implement IEnumerable directly.
    internal sealed class PSEnumerableBinder : ConvertBinder
        private static readonly PSEnumerableBinder s_binder = new PSEnumerableBinder();
        internal static PSEnumerableBinder Get()
            return s_binder;
        private PSEnumerableBinder()
            : base(typeof(IEnumerator), false)
            CacheTarget((Func<CallSite, object, IEnumerator>)(PSObjectStringRule));
            CacheTarget((Func<CallSite, object, IEnumerator>)(ArrayRule));
            CacheTarget((Func<CallSite, object, IEnumerator>)(StringRule));
            CacheTarget((Func<CallSite, object, IEnumerator>)(NotEnumerableRule));
            CacheTarget((Func<CallSite, object, IEnumerator>)(PSObjectNotEnumerableRule));
            CacheTarget((Func<CallSite, object, IEnumerator>)(AutomationNullRule));
            return "ToEnumerable";
        internal static BindingRestrictions GetRestrictions(DynamicMetaObject target)
            return (target.Value is PSObject)
                ? BindingRestrictions.GetTypeRestriction(target.Expression, target.Value.GetType())
                : target.PSGetTypeRestriction();
        private DynamicMetaObject NullResult(DynamicMetaObject target)
            // The object is not enumerable from PowerShell's perspective.  Rather than raise an exception, we let the
            // caller check for null and take the appropriate action.
                MaybeDebase(this, static e => ExpressionCache.NullEnumerator, target),
                GetRestrictions(target));
        internal static Expression MaybeDebase(DynamicMetaObjectBinder binder, Func<Expression, Expression> generator, DynamicMetaObject target)
            if (target.Value is not PSObject)
                return generator(target.Expression);
            object targetValue = PSObject.Base(target.Value);
            var tmp = Expression.Parameter(typeof(object), "value");
                new ParameterExpression[] { tmp },
                Expression.Assign(tmp, Expression.Call(CachedReflectionInfo.PSObject_Base, target.Expression)),
                    targetValue == null ?
                        (Expression)Expression.AndAlso(Expression.Equal(tmp, ExpressionCache.NullConstant),
                                                       Expression.Not(Expression.Equal(target.Expression, ExpressionCache.AutomationNullConstant)))
                        : Expression.TypeEqual(tmp, targetValue.GetType()),
                    generator(tmp), binder.GetUpdateExpression(binder.ReturnType)));
        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            if (!target.HasValue)
                return Defer(target).WriteToDebugLog(this);
                    Expression.Call(Expression.Constant(Array.Empty<object>()), typeof(Array).GetMethod("GetEnumerator")),
                    BindingRestrictions.GetInstanceRestriction(target.Expression, AutomationNull.Value)).WriteToDebugLog(this);
            var targetValue = PSObject.Base(target.Value);
            if (targetValue == null || targetValue is string || targetValue is PSObject)
                return (errorSuggestion ?? NullResult(target)).WriteToDebugLog(this);
            if (targetValue.GetType().IsArray)
                return (new DynamicMetaObject(
                    MaybeDebase(this, static e => Expression.Call(Expression.Convert(e, typeof(Array)), typeof(Array).GetMethod("GetEnumerator")),
                        target),
                    GetRestrictions(target))).WriteToDebugLog(this);
            if (targetValue is IDictionary || targetValue is XmlNode)
            if (targetValue is DataTable)
                //  DataRowCollection rows;
                //  DataTable table = (DataTable)obj;
                //  if ((rows = table.Rows) != null)
                //      return rows.GetEnumerator();
                //  else
                //      return null;
                    MaybeDebase(this, e =>
                            var table = Expression.Parameter(typeof(DataTable), "table");
                            var rows = Expression.Parameter(typeof(DataRowCollection), "rows");
                            return Expression.Block(new ParameterExpression[] { table, rows },
                                Expression.Assign(table, e.Cast(typeof(DataTable))),
                                    Expression.NotEqual(Expression.Assign(rows, Expression.Property(table, "Rows")), ExpressionCache.NullConstant),
                                    Expression.Call(rows, typeof(DataRowCollection).GetMethod("GetEnumerator")),
                                    ExpressionCache.NullEnumerator));
            if (Marshal.IsComObject(targetValue))
                // Pretend that all com objects are enumerable, even if they aren't.  We do this because it's technically impossible
                // to know if a com object is enumerable without just trying to cast it to IEnumerable.  We could generate a rule like:
                //     if (IsComObject(obj)) { return obj as IEnumerable; } else { UpdateSite; }
                // But code that calls PSEnumerableBinder.IsEnumerable and generate code based on the true/false result of that
                // function wouldn't work properly.  Instead, we'll fix things up after the binding decisions are made, see
                // EnumerableOps.NonEnumerableObjectEnumerator for more comments on how this works.
                var bindingRestrictions = BindingRestrictions.GetExpressionRestriction(
                    Expression.Call(CachedReflectionInfo.Utils_IsComObject,
                                    Expression.Call(CachedReflectionInfo.PSObject_Base, target.Expression)));
                    Expression.Call(CachedReflectionInfo.EnumerableOps_GetCOMEnumerator, target.Expression), bindingRestrictions).WriteToDebugLog(this);
            var enumerable = targetValue as IEnumerable;
                // Normally it's safe to just call IEnumerable.GetEnumerator, but in some rare cases, the
                // non-generic implementation throws or returns null, so we'll just avoid that problem and
                // call the generic version if it exists.
                foreach (var i in targetValue.GetType().GetInterfaces())
                            MaybeDebase(this, e => Expression.Call(
                                CachedReflectionInfo.EnumerableOps_GetGenericEnumerator.MakeGenericMethod(i.GetGenericArguments()[0]), Expression.Convert(e, i)),
                    MaybeDebase(this, static e => Expression.Call(CachedReflectionInfo.EnumerableOps_GetEnumerator, Expression.Convert(e, typeof(IEnumerable))),
            var enumerator = targetValue as IEnumerator;
                    MaybeDebase(this, static e => e.Cast(typeof(IEnumerator)), target),
        /// Check if the statically known type is potentially enumerable.  We can avoid some dynamic sites if we know the
        /// type is never enumerable.
        internal static bool IsStaticTypePossiblyEnumerable(Type type)
            if (type == typeof(object) || type == typeof(PSObject) || type.IsArray)
            if (type == typeof(string) || typeof(IDictionary).IsAssignableFrom(type) || typeof(XmlNode).IsAssignableFrom(type))
            if (type.IsSealed && !typeof(IEnumerable).IsAssignableFrom(type) && !typeof(IEnumerator).IsAssignableFrom(type))
        // Binders normally cannot return null, but we want a way to detect if something is enumerable,
        // so we return null if the target is not enumerable.
        internal static DynamicMetaObject IsEnumerable(DynamicMetaObject target)
            var binder = PSEnumerableBinder.Get();
            var result = binder.FallbackConvert(target, DynamicMetaObjectExtensions.FakeError);
            return (result == DynamicMetaObjectExtensions.FakeError) ? null : result;
        private static IEnumerator AutomationNullRule(CallSite site, object obj)
            return obj == AutomationNull.Value
                ? Array.Empty<object>().GetEnumerator()
                : ((CallSite<Func<CallSite, object, IEnumerator>>)site).Update(site, obj);
        private static IEnumerator NotEnumerableRule(CallSite site, object obj)
            if (obj is not PSObject
                && obj is not IEnumerable
                && obj is not IEnumerator
                && obj is not DataTable
                && !Marshal.IsComObject(obj))
            return ((CallSite<Func<CallSite, object, IEnumerator>>)site).Update(site, obj);
        private static IEnumerator PSObjectNotEnumerableRule(CallSite site, object obj)
            return psobj != null && obj != AutomationNull.Value
                ? NotEnumerableRule(site, PSObject.Base(obj))
        private static IEnumerator ArrayRule(CallSite site, object obj)
            var array = obj as Array;
            if (array != null) return array.GetEnumerator();
        private static IEnumerator StringRule(CallSite site, object obj)
            return obj is string ? null : ((CallSite<Func<CallSite, object, IEnumerator>>)site).Update(site, obj);
        private static IEnumerator PSObjectStringRule(CallSite site, object obj)
            if (psobj != null && PSObject.Base(psobj) is string) return null;
    /// This binder is used for the @() operator.
    internal sealed class PSToObjectArrayBinder : DynamicMetaObjectBinder
        private static readonly PSToObjectArrayBinder s_binder = new PSToObjectArrayBinder();
        internal static PSToObjectArrayBinder Get()
        private PSToObjectArrayBinder()
            return "ToObjectArray";
        public override Type ReturnType { get { return typeof(object[]); } }
        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
                return Defer(target, args);
                return new DynamicMetaObject(Expression.Constant(Array.Empty<object>()),
            var enumerable = PSEnumerableBinder.IsEnumerable(target);
                    Expression.NewArrayInit(typeof(object), target.Expression.Cast(typeof(object))),
                    target.PSGetTypeRestriction()).WriteToDebugLog(this);
            var value = PSObject.Base(target.Value);
            if (value is List<object>)
                    Expression.Call(PSEnumerableBinder.MaybeDebase(this, static e => e.Cast(typeof(List<object>)), target), CachedReflectionInfo.ObjectList_ToArray),
                    PSEnumerableBinder.GetRestrictions(target)).WriteToDebugLog(this);
            // It's enumerable, but not an List<object>.  Call EnumerableOps.ToArray
                Expression.Call(CachedReflectionInfo.EnumerableOps_ToArray, enumerable.Expression),
    internal sealed class PSPipeWriterBinder : DynamicMetaObjectBinder
        private static readonly PSPipeWriterBinder s_binder = new PSPipeWriterBinder();
        internal static PSPipeWriterBinder Get()
        private PSPipeWriterBinder()
            CacheTarget((Action<CallSite, object, Pipe, ExecutionContext>)StringRule);
            CacheTarget((Action<CallSite, object, Pipe, ExecutionContext>)AutomationNullRule);
            CacheTarget((Action<CallSite, object, Pipe, ExecutionContext>)BoolRule);
            CacheTarget((Action<CallSite, object, Pipe, ExecutionContext>)IntRule);
            return "PipelineWriter";
            // args[0] is the pipe
            // args[1] is the execution context, only used if we're enumerating
                    Expression.Block(typeof(void), Expression.Call(CachedReflectionInfo.PipelineOps_Nop)),
                    BindingRestrictions.GetInstanceRestriction(target.Expression, AutomationNull.Value))).WriteToDebugLog(this);
                var bindingResult = PSVariableAssignmentBinder.Get().Bind(target, Array.Empty<DynamicMetaObject>());
                var restrictions = target.LimitType.IsValueType
                    ? bindingResult.Restrictions
                    Expression.Call(args[0].Expression,
                                    bindingResult.Expression.Cast(typeof(object))),
                    restrictions)).WriteToDebugLog(this);
            bool needsToDispose = PSObject.Base(target.Value) is not IEnumerator;
                Expression.Call(CachedReflectionInfo.EnumerableOps_WriteEnumerableToPipe,
                                enumerable.Expression,
                                args[0].Expression,
                                args[1].Expression,
                                ExpressionCache.Constant(needsToDispose)),
                enumerable.Restrictions)).WriteToDebugLog(this);
        private static void BoolRule(CallSite site, object obj, Pipe pipe, ExecutionContext context)
            if (obj is bool) { pipe.Add(obj); }
            else { ((CallSite<Action<CallSite, object, Pipe, ExecutionContext>>)site).Update(site, obj, pipe, context); }
        private static void IntRule(CallSite site, object obj, Pipe pipe, ExecutionContext context)
            if (obj is int) { pipe.Add(obj); }
        private static void StringRule(CallSite site, object obj, Pipe pipe, ExecutionContext context)
            if (obj is string) { pipe.Add(obj); }
        private static void AutomationNullRule(CallSite site, object obj, Pipe pipe, ExecutionContext context)
            if (obj != AutomationNull.Value) { ((CallSite<Action<CallSite, object, Pipe, ExecutionContext>>)site).Update(site, obj, pipe, context); }
    /// This binder creates the collection we use to do multi-assignments, e.g.:
    ///     $x,$y = $z
    ///     $x,$y,$z = 1,2,3,4,5
    /// The target in this binder is the RHS, the result expression is an IList where the Count matches the
    /// number of values assigned (_elements) on the left hand side of the assign.
    internal sealed class PSArrayAssignmentRHSBinder : DynamicMetaObjectBinder
        private static readonly List<PSArrayAssignmentRHSBinder> s_binders = new List<PSArrayAssignmentRHSBinder>();
        private readonly int _elements;
        internal static PSArrayAssignmentRHSBinder Get(int i)
            lock (s_binders)
                while (s_binders.Count <= i)
                    s_binders.Add(null);
                return s_binders[i] ?? (s_binders[i] = new PSArrayAssignmentRHSBinder(i));
        private PSArrayAssignmentRHSBinder(int elements)
            _elements = elements;
            return string.Create(CultureInfo.InvariantCulture, $"MultiAssignRHSBinder {_elements}");
        public override Type ReturnType { get { return typeof(IList); } }
            Diagnostics.Assert(args.Length == 0, "binder doesn't expect any arguments");
            if (target.Value is PSObject && (PSObject.Base(target.Value) != target.Value))
                return this.DeferForPSObject(target).WriteToDebugLog(this);
            var iList = target.Value as IList;
            if (iList != null)
                // 3 possibilities - too few, exact, or too many elements.
                var getListCountExpr = Expression.Property(target.Expression.Cast(typeof(ICollection)), CachedReflectionInfo.ICollection_Count);
                var restrictions = target.PSGetTypeRestriction().Merge(
                    BindingRestrictions.GetExpressionRestriction(Expression.Equal(getListCountExpr,
                                                                                  ExpressionCache.Constant(iList.Count))));
                if (iList.Count == _elements)
                    // Exact, we can use the value as is.
                    return new DynamicMetaObject(target.Expression.Cast(typeof(IList)), restrictions).WriteToDebugLog(this);
                Expression[] newArrayElements = new Expression[_elements];
                var temp = Expression.Variable(typeof(IList));
                if (iList.Count < _elements)
                    // Too few, create an array with the correct number assigned, fill in null for the extras
                    for (i = 0; i < iList.Count; ++i)
                        newArrayElements[i] =
                            Expression.Call(temp, CachedReflectionInfo.IList_get_Item, ExpressionCache.Constant(i));
                    for (; i < _elements; ++i)
                        newArrayElements[i] = ExpressionCache.NullConstant;
                    // Too many, create an array with the correct number assigned, and the last element contains
                    // all the extras.
                    for (i = 0; i < _elements - 1; ++i)
                    newArrayElements[_elements - 1] =
                        Expression.Call(CachedReflectionInfo.EnumerableOps_GetSlice, temp, ExpressionCache.Constant(_elements - 1)).Cast(typeof(object));
                        Expression.Assign(temp, target.Expression.Cast(typeof(IList))),
                        Expression.NewArrayInit(typeof(object), newArrayElements)),
            // We have a single element, the rest must be null.
                Expression.NewArrayInit(typeof(object), Enumerable.Repeat(ExpressionCache.NullConstant, _elements - 1)
                                                .Prepend(target.Expression.Cast(typeof(object)))),
                target.PSGetTypeRestriction())).WriteToDebugLog(this);
    /// This binder is used to convert objects to string in specific circumstances, including:
    ///     * The LHS of a format expression.  The arguments (the RHS objects) of the format
    ///       expression are not converted to string here, that is deferred to string.Format which
    ///       may have some custom formatting to apply.
    ///     * The objects passed to the format expression as part of an expandable string.  In this
    ///       case, the format string is generated by the parser, so we know that there is no custom
    ///       formatting to consider.
    internal sealed class PSToStringBinder : DynamicMetaObjectBinder
        private static readonly PSToStringBinder s_binder = new PSToStringBinder();
        internal static PSToStringBinder Get()
        public override Type ReturnType { get { return typeof(string); } }
        private PSToStringBinder()
            // target is the object to convert
            // args[0] is the ExecutionContext, needed to get $OFS, and possibly the type table.
            if (!target.HasValue || !args[0].HasValue)
                return Defer(target, args).WriteToDebugLog(this);
                return this.DeferForPSObject(target, args[0]).WriteToDebugLog(this);
            var restrictions = target.PSGetTypeRestriction();
            if (target.LimitType == typeof(string))
                    target.Expression.Cast(typeof(string)),
            // PSObject.ToStringParser will handle everything, but if you want to speed up conversion to string,
            // add special cases here.
                InvokeToString(args[0].Expression, target.Expression),
        internal static Expression InvokeToString(Expression context, Expression target)
            if (target.Type == typeof(string))
            return Expression.Call(CachedReflectionInfo.PSObject_ToStringParser,
                                   context.Cast(typeof(ExecutionContext)),
                                   target.Cast(typeof(object)));
    /// This binder is used to optimize the conversion of the result.
    internal sealed class PSPipelineResultToBoolBinder : DynamicMetaObjectBinder
        private static readonly PSPipelineResultToBoolBinder s_binder = new PSPipelineResultToBoolBinder();
        internal static PSPipelineResultToBoolBinder Get()
        private PSPipelineResultToBoolBinder()
        public override Type ReturnType { get { return typeof(bool); } }
            IList pipelineResult = target.Value as IList;
            Diagnostics.Assert(pipelineResult != null, "Pipeline result is always an IList");
            var ilistExpr = target.Expression;
            if (typeof(IList) != ilistExpr.Type)
                ilistExpr = Expression.Convert(ilistExpr, typeof(IList));
            var countExpr = Expression.Property(
                Expression.Convert(ilistExpr, typeof(ICollection)), CachedReflectionInfo.ICollection_Count);
            switch (pipelineResult.Count)
                    result = ExpressionCache.Constant(false);
                    restrictions = BindingRestrictions.GetExpressionRestriction(
                        Expression.Equal(countExpr, ExpressionCache.Constant(0)));
                    result = Expression.Call(ilistExpr,
                                             CachedReflectionInfo.IList_get_Item,
                                             ExpressionCache.Constant(0)).Convert(typeof(bool));
                        Expression.Equal(countExpr, ExpressionCache.Constant(1)));
                    result = ExpressionCache.Constant(true);
                        Expression.GreaterThan(countExpr, ExpressionCache.Constant(1)));
            return (new DynamicMetaObject(result, restrictions)).WriteToDebugLog(this);
    internal sealed class PSInvokeDynamicMemberBinder : DynamicMetaObjectBinder
        private sealed class KeyComparer : IEqualityComparer<PSInvokeDynamicMemberBinderKeyType>
            public bool Equals(PSInvokeDynamicMemberBinderKeyType x, PSInvokeDynamicMemberBinderKeyType y)
                return x.Item1.Equals(y.Item1) &&
                       ((x.Item2 == null) ? y.Item2 == null : x.Item2.Equals(y.Item2)) &&
                       x.Item3 == y.Item3 &&
                       x.Item4 == y.Item4 &&
                       x.Item5 == y.Item5;
            public int GetHashCode(PSInvokeDynamicMemberBinderKeyType obj)
        private static readonly
            Dictionary<PSInvokeDynamicMemberBinderKeyType, PSInvokeDynamicMemberBinder> s_binderCache
            = new Dictionary<PSInvokeDynamicMemberBinderKeyType, PSInvokeDynamicMemberBinder>(new KeyComparer());
        internal static PSInvokeDynamicMemberBinder Get(CallInfo callInfo, TypeDefinitionAst classScopeAst, bool @static, bool propertySetter, PSMethodInvocationConstraints constraints)
            PSInvokeDynamicMemberBinder result;
            var classScope = classScopeAst?.Type;
            lock (s_binderCache)
                var key = Tuple.Create(callInfo, constraints, propertySetter, @static, classScope);
                if (!s_binderCache.TryGetValue(key, out result))
                    result = new PSInvokeDynamicMemberBinder(callInfo, @static, propertySetter, constraints, classScope);
                    s_binderCache.Add(key, result);
        private readonly bool _static;
        private readonly bool _propertySetter;
        private readonly PSMethodInvocationConstraints _constraints;
        private readonly Type _classScope;
        private PSInvokeDynamicMemberBinder(CallInfo callInfo, bool @static, bool propertySetter, PSMethodInvocationConstraints constraints, Type classScope)
            Diagnostics.Assert(callInfo != null, "callers make sure 'callInfo' is not null");
            _static = @static;
            _propertySetter = propertySetter;
            _constraints = constraints;
            _classScope = classScope;
                return Defer(target, args[0]).WriteToDebugLog(this);
            var memberNameArg = args[0];
            object memberNameValue = PSObject.Base(memberNameArg.Value);
            Expression bindingStrExpr;
            var memberName = memberNameValue as string;
                if (memberNameArg.Value is PSObject)
                    bindingStrExpr = Expression.Call(CachedReflectionInfo.PSObject_Base, memberNameArg.Expression).Cast(typeof(string));
                    bindingStrExpr = memberNameArg.Expression.Cast(typeof(string));
                // Context is explicitly null here, we don't want $OFS to influence the property name, or
                // we'd generate code that wouldn't work consistently, depending on the value of $OFS.
                memberName = PSObject.ToStringParser(null, memberNameArg.Value);
                bindingStrExpr = PSToStringBinder.InvokeToString(ExpressionCache.NullConstant, memberNameArg.Expression);
            // Note: Need to create DynamicExpression to support dynamic member invoke, see PSSetDynamicMemberBinder for example
            var result = PSInvokeMemberBinder.Get(memberName, _callInfo, _static, _propertySetter, _constraints, _classScope).FallbackInvokeMember(target, args.Skip(1).ToArray());
            BindingRestrictions restrictions = result.Restrictions;
            restrictions = restrictions.Merge(args[0].PSGetTypeRestriction());
            restrictions = restrictions.Merge(BindingRestrictions.GetExpressionRestriction(
                Expression.Call(CachedReflectionInfo.String_Equals, Expression.Constant(memberName), bindingStrExpr, ExpressionCache.Ordinal)));
            return (new DynamicMetaObject(result.Expression, restrictions)).WriteToDebugLog(this);
    internal class PSDynamicGetOrSetBinderKeyComparer : IEqualityComparer<PSGetOrSetDynamicMemberBinderKeyType>
        public bool Equals(PSGetOrSetDynamicMemberBinderKeyType x, PSGetOrSetDynamicMemberBinderKeyType y)
            return x.Item1 == y.Item1 && x.Item2 == y.Item2;
        public int GetHashCode(PSGetOrSetDynamicMemberBinderKeyType obj)
    internal sealed class PSGetDynamicMemberBinder : DynamicMetaObjectBinder
        private static readonly Dictionary<PSGetOrSetDynamicMemberBinderKeyType, PSGetDynamicMemberBinder> s_binderCache =
            new Dictionary<PSGetOrSetDynamicMemberBinderKeyType, PSGetDynamicMemberBinder>(new PSDynamicGetOrSetBinderKeyComparer());
        internal static PSGetDynamicMemberBinder Get(TypeDefinitionAst classScope, bool @static)
            PSGetDynamicMemberBinder binder;
                var type = classScope?.Type;
                var tuple = Tuple.Create(type, @static);
                if (!s_binderCache.TryGetValue(tuple, out binder))
                    binder = new PSGetDynamicMemberBinder(type, @static);
                    s_binderCache.Add(tuple, binder);
            return binder;
        private PSGetDynamicMemberBinder(Type classScope, bool @static)
            Diagnostics.Assert(args.Length == 1, "PSGetDynamicMemberBinder::Bind handles one and only one argument");
            else if (PSObject.Base(target.Value) is IDictionary)
                // We don't want to convert the member name to a string, we'll just try indexing the dictionary and nothing else.
                restrictions = target.PSGetTypeRestriction();
                    Expression.Not(Expression.TypeIs(args[0].Expression, typeof(string)))));
                    Expression.Call(CachedReflectionInfo.PSGetDynamicMemberBinder_GetIDictionaryMember,
                                    PSGetMemberBinder.GetTargetExpr(target, typeof(IDictionary)),
                                    args[0].Expression.Cast(typeof(object))),
                    restrictions).WriteToDebugLog(this);
            var result = DynamicExpression.Dynamic(PSGetMemberBinder.Get(memberName, _classScope, _static), typeof(object), target.Expression);
            restrictions = BindingRestrictions.Empty;
        internal static object GetIDictionaryMember(IDictionary hash, object key)
                if (hash.Contains(key))
                    return hash[key];
            if (context.IsStrictVersion(2))
                // If the member is undefined and we're in strict mode, throw an exception...
                throw new PropertyNotFoundException("PropertyNotFoundStrict", null, ParserStrings.PropertyNotFoundStrict,
                                                    LanguagePrimitives.ConvertTo<string>(key));
    internal sealed class PSSetDynamicMemberBinder : DynamicMetaObjectBinder
        private static readonly Dictionary<PSGetOrSetDynamicMemberBinderKeyType, PSSetDynamicMemberBinder> s_binderCache =
            new Dictionary<PSGetOrSetDynamicMemberBinderKeyType, PSSetDynamicMemberBinder>(new PSDynamicGetOrSetBinderKeyComparer());
        internal static PSSetDynamicMemberBinder Get(TypeDefinitionAst classScope, bool @static)
            PSSetDynamicMemberBinder binder;
                    binder = new PSSetDynamicMemberBinder(type, @static);
        private PSSetDynamicMemberBinder(Type classScope, bool @static)
            Diagnostics.Assert(args.Length == 2, "PSSetDynamicMemberBinder::Bind handles two and only two arguments");
            var result = DynamicExpression.Dynamic(PSSetMemberBinder.Get(memberName, _classScope, _static), typeof(object), target.Expression, args[1].Expression);
            var restrictions = BindingRestrictions.Empty;
            // Note: Optimal restriction is test target type is IDictionary or not
            Expression resultExpr;
            if (target.Value is IDictionary)
                // We should first try:
                //     $target[$arg[0]] = $arg[1]
                // And if that fails, try arg[0] converted to string, setting a property, etc.
                var exception = Expression.Variable(typeof(Exception));
                var indexResult = PSSetIndexBinder.Get(1).FallbackSetIndex(target, new[] { args[0] }, args[1]);
                resultExpr = Expression.TryCatch(
                    indexResult.Expression,
                    Expression.Catch(exception, result));
                resultExpr = result;
            return (new DynamicMetaObject(resultExpr, restrictions)).WriteToDebugLog(this);
    internal sealed class PSSwitchClauseEvalBinder : DynamicMetaObjectBinder
        // Increase this cache size if we add a new flag to the switch statement that:
        //    - Influences evaluation of switch elements
        //    - Is commonly used
        private static readonly PSSwitchClauseEvalBinder[] s_binderCache = new PSSwitchClauseEvalBinder[32];
        private readonly SwitchFlags _flags;
        internal static PSSwitchClauseEvalBinder Get(SwitchFlags flags)
                return s_binderCache[(int)flags] ?? (s_binderCache[(int)flags] = new PSSwitchClauseEvalBinder(flags));
        private PSSwitchClauseEvalBinder(SwitchFlags flags)
            _flags = flags;
            // target is the condition
            // args[0] is the value to test against the condition
            // args[1] is the execution context to use, if needed
                return Defer(target, args[0], args[1]).WriteToDebugLog(this);
            // No need to add binding restrictions on either arg, the type of the clause is all that matters.
            // We can skip the restrictions on the arg, the generated code will contain a dynamic site to convert
            // the arg to string if applicable.  If the dynamic site is removed, then arg restrictions must be added.
            var targetRestrictions = target.PSGetTypeRestriction();
            // If any args are PSObject, we typically call DeferForPSObject, but in this case,
            // we don't because args[0] should not be unwrapped, which DeferForPSObject would do.
            if (target.Value is PSObject)
                    DynamicExpression.Dynamic(this, this.ReturnType,
                                              Expression.Call(CachedReflectionInfo.PSObject_Base,
                                                              target.Expression.Cast(typeof(object))),
                                              args[1].Expression),
                    targetRestrictions)).WriteToDebugLog(this);
            if (target.Value == null)
                // If the condition is null, the we simply test the value against null.  It seems like
                // this is a silly thing to allow in a switch, maybe it should be disallowed in strict mode.
                    Expression.Equal(args[0].Expression.Cast(typeof(object)), ExpressionCache.NullConstant),
            if (target.Value is ScriptBlock)
                var call = Expression.Call(target.Expression.Cast(typeof(ScriptBlock)),
                    /*dollarUnder=*/           args[0].CastOrConvert(typeof(object)),
                    DynamicExpression.Dynamic(PSConvertBinder.Get(typeof(bool)), typeof(bool), call),
            // From here on out, arg must be a string.
            var executionContext = args[1].Expression;
            var argAsString = DynamicExpression.Dynamic(PSToStringBinder.Get(), typeof(string), args[0].Expression,
                                                        executionContext);
            if (target.Value is Regex || (_flags & SwitchFlags.Regex) != 0)
                var call = Expression.Call(CachedReflectionInfo.SwitchOps_ConditionSatisfiedRegex,
                    /*caseSensitive=*/ ExpressionCache.Constant((_flags & SwitchFlags.CaseSensitive) != 0),
                    /*condition=*/     target.Expression.Cast(typeof(object)),
                    /*errorPosition=*/ ExpressionCache.NullExtent,
                    /*str=*/           argAsString,
                    /*context=*/       executionContext);
                return (new DynamicMetaObject(call, targetRestrictions)).WriteToDebugLog(this);
            if (target.Value is WildcardPattern || (_flags & SwitchFlags.Wildcard) != 0)
                var call = Expression.Call(CachedReflectionInfo.SwitchOps_ConditionSatisfiedWildcard,
                // Binding restrictions must test the target, even if the switch is in -regex mode.
                // If we didn't add restrictions on the target, we'd incorrectly use this rule when the target
                // is a script block.
                // We can skip the restrictions on the arg, the generated code contains a dynamic site to convert
                // the arg to string, so that site handles any arg type properly.
            var targetAsString = DynamicExpression.Dynamic(PSToStringBinder.Get(), typeof(string), target.Expression,
                Compiler.CallStringEquals(targetAsString, argAsString, ((_flags & SwitchFlags.CaseSensitive) == 0)),
    // This class implements the standard binder CreateInstanceBinder, but this binder handles the CallInfo a little differently.
    // The ArgumentNames are not used to invoke a constructor, instead they are used to set properties/fields in the attribute.
    internal sealed class PSAttributeGenerator : CreateInstanceBinder
        private static readonly Dictionary<CallInfo, PSAttributeGenerator> s_binderCache =
            new Dictionary<CallInfo, PSAttributeGenerator>();
        internal static PSAttributeGenerator Get(CallInfo callInfo)
                PSAttributeGenerator binder;
                if (!s_binderCache.TryGetValue(callInfo, out binder))
                    binder = new PSAttributeGenerator(callInfo);
                    s_binderCache.Add(callInfo, binder);
        private PSAttributeGenerator(CallInfo callInfo)
        public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
            Diagnostics.Assert(target.HasValue && target.Value is Type, "caller to verify arguments");
            var attributeType = (Type)target.Value;
            // We can't use a type restriction on target, it's always a System.Type.  So make sure we always use an instance restriction
            target = new DynamicMetaObject(target.Expression, BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value), target.Value);
            var positionalArgCount = CallInfo.ArgumentCount - CallInfo.ArgumentNames.Count;
                args.Take(positionalArgCount).Select(static arg => arg.Value).ToArray(),
                        Expression.New(CachedReflectionInfo.MethodException_ctor, Expression.Constant(errorId),
                                       Expression.Constant(null, typeof(Exception)), Expression.Constant(errorMsg),
                                       Expression.NewArrayInit(typeof(object),
                                            Expression.Constant(".ctor").Cast(typeof(object)),
                                            ExpressionCache.Constant(positionalArgCount).Cast(typeof(object)))),
                        this.ReturnType),
                    target.CombineRestrictions(args));
            var ctorArgs = new Expression[parameterInfo.Length];
            var argIndex = 0;
            for (; argIndex < parameterInfo.Length; ++argIndex)
                    var paramsArray = new List<Expression>();
                    int ctorArgIndex = argIndex;
                    for (int i = argIndex; i < positionalArgCount; ++i, ++argIndex)
                        // ConstrainedLanguage note - calls to this conversion are done by constructors with params arguments.
                        // Protection against conversions are covered by the method resolution algorithm
                        var conversion = LanguagePrimitives.FigureConversion(args[argIndex].Value, elementType, out debase);
                        Diagnostics.Assert(conversion.Rank != ConversionRank.None, "FindBestMethod should have failed if there is no conversion");
                        paramsArray.Add(
                            PSConvertBinder.InvokeConverter(conversion, args[i].Expression, elementType, debase, ExpressionCache.InvariantCulture));
                    ctorArgs[ctorArgIndex] = Expression.NewArrayInit(elementType, paramsArray);
                    var conversion = LanguagePrimitives.FigureConversion(args[argIndex].Value, resultType, out bool debase);
                    ctorArgs[argIndex] = PSConvertBinder.InvokeConverter(conversion, args[argIndex].Expression, resultType, debase, ExpressionCache.InvariantCulture);
            Expression result = Expression.New(constructorInfo, ctorArgs);
            if (CallInfo.ArgumentNames.Count > 0)
                var tmp = Expression.Parameter(result.Type);
                var blockExprs = new List<Expression>();
                foreach (var name in CallInfo.ArgumentNames)
                        return target.ThrowRuntimeError(args, BindingRestrictions.Empty, "PropertyNotFoundForType",
                                                        ParserStrings.PropertyNotFoundForType, Expression.Constant(name),
                                                        Expression.Constant(attributeType, typeof(Type)));
                    var member = members[0];
                    Expression lhs;
                    Type propertyType;
                            return target.ThrowRuntimeError(args, BindingRestrictions.Empty, "PropertyIsReadOnly",
                                                            ParserStrings.PropertyIsReadOnly, Expression.Constant(name));
                        propertyType = propertyInfo.PropertyType;
                        lhs = Expression.Property(tmp.Cast(propertyInfo.DeclaringType), propertyInfo);
                        propertyType = ((FieldInfo)member).FieldType;
                        lhs = Expression.Field(tmp.Cast(member.DeclaringType), (FieldInfo)member);
                    // ConstrainedLanguage note - calls to these property assignment conversions are enforced by the
                    // property assignment binding rules (which disallow property conversions to disallowed types)
                    var conversion = LanguagePrimitives.FigureConversion(args[argIndex].Value, propertyType, out debase);
                        return PSConvertBinder.ThrowNoConversion(args[argIndex], propertyType, this, -1,
                            args.Except(new DynamicMetaObject[] { args[argIndex] }).Prepend(target).ToArray());
                    blockExprs.Add(
                        Expression.Assign(lhs, PSConvertBinder.InvokeConverter(conversion, args[argIndex].Expression, propertyType, debase, ExpressionCache.InvariantCulture)));
                    argIndex += 1;
                // We wrap the block of assignments in a try/catch and issue a general error message whenever the assignment fails.
                var exception = Expression.Parameter(typeof(Exception));
                        Expression.Block(typeof(void), blockExprs),
                            Compiler.ThrowRuntimeErrorWithInnerException("PropertyAssignmentException", Expression.Property(exception, "Message"), exception, typeof(void)))),
                    // The result of the block is the object constructed, so the tmp must be the last expr in the block.
            return new DynamicMetaObject(result, target.CombineRestrictions(args));
    internal sealed class PSCustomObjectConverter : DynamicMetaObjectBinder
        private static readonly PSCustomObjectConverter s_binder = new PSCustomObjectConverter();
        internal static PSCustomObjectConverter Get()
        private PSCustomObjectConverter()
            var baseObjectValue = PSObject.Base(target.Value);
            var toType = baseObjectValue is OrderedDictionary || baseObjectValue is Hashtable
                ? typeof(LanguagePrimitives.InternalPSCustomObject)
                : typeof(PSObject);
            // ConstrainedLanguage note - calls to this conversion only target PSCustomObject / PSObject,
            // which is safe.
            var conversion = LanguagePrimitives.FigureConversion(target.Value, toType, out debase);
                PSConvertBinder.InvokeConverter(conversion, target.Expression, toType, debase, ExpressionCache.InvariantCulture),
    internal sealed class PSDynamicConvertBinder : DynamicMetaObjectBinder
        private static readonly PSDynamicConvertBinder s_binder = new PSDynamicConvertBinder();
        internal static PSDynamicConvertBinder Get()
        private PSDynamicConvertBinder()
            // target is the type and is never a PSObject
            // arg is the object to be converted
            DynamicMetaObject arg = args[0];
            if (!target.HasValue || !arg.HasValue)
                return Defer(target, arg).WriteToDebugLog(this);
            var toType = target.Value as Type;
            Diagnostics.Assert(toType != null, "target must be a type");
            var bindingRestrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, toType).Merge(arg.PSGetTypeRestriction());
                DynamicExpression.Dynamic(PSConvertBinder.Get(toType), toType, arg.Expression).Cast(typeof(object)),
                                          bindingRestrictions).WriteToDebugLog(this);
    /// This binder is used to copy mutable value types when assigning to variables, otherwise just assigning the target object directly.
    internal sealed class PSVariableAssignmentBinder : DynamicMetaObjectBinder
        private static readonly PSVariableAssignmentBinder s_binder = new PSVariableAssignmentBinder();
        internal static int s_mutableValueWithInstanceMemberVersion;
        internal static PSVariableAssignmentBinder Get()
        private PSVariableAssignmentBinder()
            CacheTarget((Func<CallSite, object, object>)(PSObjectStringRule));
            CacheTarget((Func<CallSite, object, object>)(ObjectRule));
            CacheTarget((Func<CallSite, object, object>)(IntRule));
            CacheTarget((Func<CallSite, object, object>)(EnumRule));
            CacheTarget((Func<CallSite, object, object>)(BoolRule));
            CacheTarget((Func<CallSite, object, object>)(NullRule));
                return Defer(target);
            var value = target.Value;
                return new DynamicMetaObject(ExpressionCache.NullConstant, target.PSGetTypeRestriction()).WriteToDebugLog(this);
            var psobj = value as PSObject;
                var restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, psobj.GetType());
                var expr = target.Expression;
                var baseObject = psobj.BaseObject;
                var baseObjExpr = Expression.Property(expr.Cast(typeof(PSObject)), CachedReflectionInfo.PSObject_BaseObject);
                    var baseObjType = baseObject.GetType();
                    restrictions = restrictions.Merge(BindingRestrictions.GetTypeRestriction(baseObjExpr, baseObjType));
                    if (baseObjType.IsValueType)
                        expr = GetExprForValueType(baseObjType, Expression.Convert(baseObjExpr, baseObjType), expr, ref restrictions);
                        restrictions.Merge(
                            BindingRestrictions.GetExpressionRestriction(Expression.Equal(baseObjExpr,
                                                                                          ExpressionCache.NullConstant)));
                return new DynamicMetaObject(expr, restrictions).WriteToDebugLog(this);
            var type = value.GetType();
                expr = GetExprForValueType(type, Expression.Convert(expr, type), expr, ref restrictions);
            // This rule is meant to cover all classes except PSObject.
                target.Expression, BindingRestrictions.GetExpressionRestriction(
                        Expression.Not(Expression.TypeIs(target.Expression, typeof(ValueType))),
                        Expression.Not(Expression.TypeIs(target.Expression, typeof(PSObject)))))).WriteToDebugLog(this);
        private static Expression GetExprForValueType(Type type,
                                                      Expression convertedExpr,
                                                      Expression originalExpr,
                                                      ref BindingRestrictions restrictions)
            // A binding restriction for value types will be either:
            //    if (obj is SomeValueType)
            //    if (obj is SomeValueType && _mutableValueWithInstanceMemberVersion == someVersionNumber)
            // And the expression will be one of 3 possibilities:
            //    * return expr
            //    * tmp = expr; return tmp (make a copy)
            //    * return CopyInstanceMembersOfValueType((T)expr, expr) (make a copy, also copy instance members)
            // If we've never seen an instance member for a given type, we can avoid the expensive call to
            // CopyInstanceMembersOfValueType, but if somebody adds an instance member in the future, we need to invalidate
            // previously generated rules.  We do that with the version check.
            // We can skip a version check if we're already generating the worst case code.  This also avoids regenerating
            // new bindings when the new binding won't differ from a previous binding.
            Expression expr;
            bool needVersionCheck = true;
            if (s_mutableValueTypesWithInstanceMembers.ContainsKey(type))
                var genericMethodInfo = CachedReflectionInfo.PSVariableAssignmentBinder_CopyInstanceMembersOfValueType.MakeGenericMethod(new Type[] { type });
                expr = Expression.Call(genericMethodInfo, convertedExpr, originalExpr);
                needVersionCheck = false;
            else if (IsValueTypeMutable(type))
                var temp = Expression.Variable(type);
                expr = Expression.Block(new[] { temp },
                                        // The valuetype is mutable, so force a copy by assigning to a temp.
                                        Expression.Assign(temp, convertedExpr),
                                        // Box the return value because the dynamic site expects object
                                        temp.Cast(typeof(object)));
                expr = originalExpr;
            if (needVersionCheck)
                restrictions = restrictions.Merge(GetVersionCheck(s_mutableValueWithInstanceMemberVersion));
        private static object EnumRule(CallSite site, object obj)
            if (obj is Enum) { return obj; }
            return ((CallSite<Func<CallSite, object, object>>)site).Update(site, obj);
        private static object BoolRule(CallSite site, object obj)
            if (obj is bool) { return obj; }
        private static object IntRule(CallSite site, object obj)
            if (obj is int) { return obj; }
        private static object ObjectRule(CallSite site, object obj)
            if (obj is not ValueType && obj is not PSObject) { return obj; }
        private static object PSObjectStringRule(CallSite site, object obj)
            if (psobj != null && psobj.BaseObject is string) return obj;
        private static object NullRule(CallSite site, object obj)
            return obj == null ? null : ((CallSite<Func<CallSite, object, object>>)site).Update(site, obj);
        internal static bool IsValueTypeMutable(Type type)
            // First, check for enums/primitives and compiler-defined attributes.
            if (type.IsPrimitive
                || type.IsEnum
                || type.IsDefined(typeof(System.Runtime.CompilerServices.IsReadOnlyAttribute), inherit: false))
            // If the builtin attribute is not present, check for a custom attribute from by the compiler. If the
            // library targets netstandard2.0, the compiler can't be sure the attribute will be provided by the runtime,
            // and defines its own attribute of the same name during compilation. To account for this, we must check the
            // type by name, not by reference.
            foreach (object attribute in type.GetCustomAttributes(inherit: false))
                if (attribute.GetType().FullName.Equals(
                    "System.Runtime.CompilerServices.IsReadOnlyAttribute",
                    StringComparison.Ordinal))
            // Fallback: check all fields (public + private) to verify whether they're all readonly.
            // If any field is not readonly, the value type is potentially mutable.
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (!field.IsInitOnly)
            // If all fields are init-only (read-only), then the value type is immutable.
        private static readonly ConcurrentDictionary<Type, bool> s_mutableValueTypesWithInstanceMembers =
            new ConcurrentDictionary<Type, bool>();
        internal static void NoteTypeHasInstanceMemberOrTypeName(Type type)
            if (!type.IsValueType || !IsValueTypeMutable(type))
            if (s_mutableValueTypesWithInstanceMembers.TryAdd(type, true))
                s_mutableValueWithInstanceMemberVersion += 1;
        internal static object CopyInstanceMembersOfValueType<T>(T t, object boxedT) where T : struct
            if (PSObject.HasInstanceMembers(boxedT, out _) || PSObject.HasInstanceTypeName(boxedT, out _))
                var psobj = PSObject.AsPSObject(boxedT);
                return PSObject.Base(psobj.Copy());
            // We want a copy (because the value type is mutable), so return t, not boxedT.
        internal static BindingRestrictions GetVersionCheck(int expectedVersionNumber)
                Expression.Equal(Expression.Field(null, CachedReflectionInfo.PSVariableAssignmentBinder__mutableValueWithInstanceMemberVersion),
    #endregion PowerShell non-standard language binders
    #region Standard binders
    /// The binder for common binary operators.  PowerShell specific binary operators are handled elsewhere.
    internal sealed class PSBinaryOperationBinder : BinaryOperationBinder
        #region Constructors and factory methods
        private static readonly Dictionary<Tuple<ExpressionType, bool, bool>, PSBinaryOperationBinder> s_binderCache =
            new Dictionary<Tuple<ExpressionType, bool, bool>, PSBinaryOperationBinder>();
        internal static PSBinaryOperationBinder Get(ExpressionType operation, bool ignoreCase = true, bool scalarCompare = false)
            PSBinaryOperationBinder result;
                var key = Tuple.Create(operation, ignoreCase, scalarCompare);
                    result = new PSBinaryOperationBinder(operation, ignoreCase, scalarCompare);
        private readonly bool _ignoreCase;
        private readonly bool _scalarCompare;
        internal int _version;
        private PSBinaryOperationBinder(ExpressionType operation, bool ignoreCase, bool scalarCompare)
            : base(operation)
            _ignoreCase = ignoreCase;
            _scalarCompare = scalarCompare;
            this._version = 0;
        #endregion Constructors and factory methods
        #region Delegates for enumerable comparisons
        private Func<object, object, bool> _compareDelegate;
        private Func<object, object, bool> GetScalarCompareDelegate()
            if (_compareDelegate == null)
                var lvalExpr = Expression.Parameter(typeof(object), "lval");
                var rvalExpr = Expression.Parameter(typeof(object), "rval");
                var compareDelegate = Expression.Lambda<Func<object, object, bool>>(
                    DynamicExpression.Dynamic(Get(Operation, _ignoreCase, scalarCompare: true),
                                              typeof(object), lvalExpr, rvalExpr).Cast(typeof(bool)),
                    lvalExpr, rvalExpr).Compile();
                Interlocked.CompareExchange(ref _compareDelegate, compareDelegate, null);
            return _compareDelegate;
        #endregion Delegates for enumerable comparisons
        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
            if ((target.Value is PSObject && PSObject.Base(target.Value) != target.Value) ||
                     (arg.Value is PSObject && PSObject.Base(arg.Value) != arg.Value))
                // When adding to an array, we don't want to unwrap the RHS - it's unnecessary,
                // and in the case of strings, we actually lose instance members on the PSObject.
                if (!(Operation == ExpressionType.Add && PSEnumerableBinder.IsEnumerable(target) != null))
                    // Defer when the object is wrapped, but not for empty objects.
                    return this.DeferForPSObject(target, arg).WriteToDebugLog(this);
            switch (Operation)
                    return BinaryAdd(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return BinarySub(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return BinaryMultiply(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return BinaryDivide(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return BinaryRemainder(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return BinaryBitwiseAnd(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return BinaryBitwiseOr(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return BinaryBitwiseXor(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return CompareEQ(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return CompareNE(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return CompareGT(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return CompareGE(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return CompareLT(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return CompareLE(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return LeftShift(target, arg, errorSuggestion).WriteToDebugLog(this);
                    return RightShift(target, arg, errorSuggestion).WriteToDebugLog(this);
            return (errorSuggestion ??
                    new DynamicMetaObject(
                        Compiler.CreateThrow(typeof(object), typeof(PSNotImplementedException), "Unimplemented operation"),
                        target.CombineRestrictions(arg))).WriteToDebugLog(this);
                "PSBinaryOperationBinder {0}{1} ver:{2}",
                GetOperatorText(),
                _scalarCompare ? " scalarOnly" : string.Empty,
                _version);
        internal static void InvalidateCache()
            // Invalidate binders
                foreach (PSBinaryOperationBinder binder in s_binderCache.Values)
                    binder._version += 1;
        private string GetOperatorText()
                case ExpressionType.Add: return TokenKind.Plus.Text();
                case ExpressionType.Subtract: return TokenKind.Minus.Text();
                case ExpressionType.Multiply: return TokenKind.Multiply.Text();
                case ExpressionType.Divide: return TokenKind.Divide.Text();
                case ExpressionType.Modulo: return TokenKind.Rem.Text();
                case ExpressionType.And: return TokenKind.Band.Text();
                case ExpressionType.Or: return TokenKind.Bor.Text();
                case ExpressionType.ExclusiveOr: return TokenKind.Bxor.Text();
                case ExpressionType.Equal: return _ignoreCase ? TokenKind.Ieq.Text() : TokenKind.Ceq.Text();
                case ExpressionType.NotEqual: return _ignoreCase ? TokenKind.Ine.Text() : TokenKind.Cne.Text();
                case ExpressionType.GreaterThan: return _ignoreCase ? TokenKind.Igt.Text() : TokenKind.Cgt.Text();
                case ExpressionType.GreaterThanOrEqual: return _ignoreCase ? TokenKind.Ige.Text() : TokenKind.Cge.Text();
                case ExpressionType.LessThan: return _ignoreCase ? TokenKind.Ilt.Text() : TokenKind.Clt.Text();
                case ExpressionType.LessThanOrEqual: return _ignoreCase ? TokenKind.Ile.Text() : TokenKind.Cle.Text();
                case ExpressionType.LeftShift: return TokenKind.Shl.Text();
                case ExpressionType.RightShift: return TokenKind.Shr.Text();
            Diagnostics.Assert(false, "Unexpected operator");
        private static DynamicMetaObject CallImplicitOp(string methodName, DynamicMetaObject target, DynamicMetaObject arg, string errorOperator, DynamicMetaObject errorSuggestion)
            // We will assume that if we got here with a non-null errorSuggestion and DynamicObject, that we
            // are trying to generate the expression that calls the override to DynamicObject.TryBinaryOperation.
            // We get called twice for the same target object, once with a null errorSuggestion (in which case we'll have
            // returned the result below), and then a second time with a non-null errorSuggestion, which we return as is.
            if (errorSuggestion != null && target.Value is DynamicObject)
                return errorSuggestion;
            // TODO: use a dynamic call site to invoke the correct method or throw an error.
                Expression.Call(CachedReflectionInfo.ParserOps_ImplicitOp,
                                target.Expression.Cast(typeof(object)),
                                arg.Expression.Cast(typeof(object)),
                                ExpressionCache.NullExtent,
                                Expression.Constant(errorOperator)),
                target.CombineRestrictions(arg));
        private static bool IsValueNegative(object value, TypeCode typeCode)
                case TypeCode.SByte: return (sbyte)value < 0;
                case TypeCode.Int16: return (short)value < 0;
                case TypeCode.Int32: return (int)value < 0;
                case TypeCode.Int64: return (long)value < 0;
            Diagnostics.Assert(false, "Invalid type code for testing negative value");
        private static Expression TypedZero(TypeCode typeCode)
                case TypeCode.SByte: return Expression.Constant((sbyte)0);
                case TypeCode.Int16: return Expression.Constant((short)0);
                case TypeCode.Int32: return ExpressionCache.Constant(0);
                case TypeCode.Int64: return Expression.Constant((long)0);
        private static DynamicMetaObject FigureSignedUnsignedInt(DynamicMetaObject obj, TypeCode typeCode, TypeCode currentOpType, out Type opImplType, out Type argType, out bool shouldFallbackToDoubleInCaseOfOverflow)
            opImplType = null;
            argType = null;
            shouldFallbackToDoubleInCaseOfOverflow = false;
            if (IsValueNegative(obj.Value, typeCode))
                switch (currentOpType)
                        opImplType = typeof(LongOps);
                        argType = typeof(long);
                        opImplType = typeof(DecimalOps);
                        argType = typeof(decimal);
                        // multiply operation may overflow within the [decimal] context, i.e. [int64]::minvalue * [uint64]::maxvalue.
                        // if overflow happens, we fallback to the [double] context
                        shouldFallbackToDoubleInCaseOfOverflow = true;
                        Diagnostics.Assert(false, "Need to figure out opImplType only for UIn32 and UInt64");
                    obj.Expression,
                    obj.PSGetTypeRestriction()
                        .Merge(BindingRestrictions.GetExpressionRestriction(
                            Expression.LessThan(obj.Expression.Cast(obj.LimitType), TypedZero(typeCode)))),
                    obj.Value);
                        Expression.GreaterThanOrEqual(obj.Expression.Cast(obj.LimitType), TypedZero(typeCode)))),
        private DynamicMetaObject BinaryNumericOp(string methodName, DynamicMetaObject target, DynamicMetaObject arg)
            // The type code comparison and the code generated by this routine only supports primitive types
            // for both operands.
            Diagnostics.Assert(target.LimitType.IsNumericOrPrimitive() && arg.LimitType.IsNumericOrPrimitive(),
                               "numeric ops are only supported with primitive types");
            Type opImplType = null, argType = null;
            bool fallbackToDoubleInCaseOfOverflow = false;
            TypeCode leftTypeCode = LanguagePrimitives.GetTypeCode(target.LimitType);
            TypeCode rightTypeCode = LanguagePrimitives.GetTypeCode(arg.LimitType);
            TypeCode opTypeCode = (int)leftTypeCode >= (int)rightTypeCode ? leftTypeCode : rightTypeCode;
                opImplType = typeof(IntOps);
                argType = typeof(int);
                Diagnostics.Assert(opTypeCode == TypeCode.UInt32, "opType must be UInt32 if it gets in this code path");
                // If one of the operands is signed, we need to promote to long if the value is negative, but
                // we can stay w/ an integer if the value is positive.  Either way, we'll need a type test.
                if (LanguagePrimitives.IsSignedInteger(leftTypeCode))
                    target = FigureSignedUnsignedInt(target, leftTypeCode, opTypeCode, out opImplType, out argType, out fallbackToDoubleInCaseOfOverflow);
                else if (LanguagePrimitives.IsSignedInteger(rightTypeCode))
                    arg = FigureSignedUnsignedInt(arg, rightTypeCode, opTypeCode, out opImplType, out argType, out fallbackToDoubleInCaseOfOverflow);
                if (opImplType == null)
                    opImplType = typeof(UIntOps);
                    argType = typeof(uint);
                Diagnostics.Assert(opTypeCode == TypeCode.UInt64, "opType must be UInt64 if it gets in this code path");
                    opImplType = typeof(ULongOps);
                    argType = typeof(ulong);
                if (methodName.StartsWith("Compare", StringComparison.Ordinal))
                    // Casting a double to decimal can overflow.  Instead, we are "smarter" and avoid
                    // the cast, and allow the comparison.  There may be a precision problem with values
                    // near Decimal.MaxValue or Decimal.MinValue, but V2 allowed the comparisons
                    // w/o errors, so we continue to do so.
                    if (LanguagePrimitives.IsFloating(leftTypeCode))
                            Expression.Call(typeof(DecimalOps).GetMethod(methodName + "1", BindingFlags.NonPublic | BindingFlags.Static),
                                            target.Expression.Cast(target.LimitType).Cast(typeof(double)),
                                            arg.Expression.Cast(arg.LimitType).Cast(typeof(decimal))),
                    if (LanguagePrimitives.IsFloating(rightTypeCode))
                            Expression.Call(typeof(DecimalOps).GetMethod(methodName + "2", BindingFlags.NonPublic | BindingFlags.Static),
                                            target.Expression.Cast(target.LimitType).Cast(typeof(decimal)),
                                            arg.Expression.Cast(arg.LimitType).Cast(typeof(double))),
                opImplType = typeof(DoubleOps);
                argType = typeof(double);
            Expression expr =
                Expression.Call(opImplType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static),
                                target.Expression.Cast(target.LimitType).Cast(argType),
                                arg.Expression.Cast(arg.LimitType).Cast(argType));
            if (fallbackToDoubleInCaseOfOverflow)
                Type doubleOpType = typeof(DoubleOps);
                Type doubleArgType = typeof(double);
                Expression fallbackToDoubleExpr =
                    Expression.Call(doubleOpType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static),
                                target.Expression.Cast(target.LimitType).Cast(doubleArgType),
                                arg.Expression.Cast(arg.LimitType).Cast(doubleArgType));
                var exception = Expression.Variable(typeof(RuntimeException), "psBinaryNumericOpException");
                var catchExpr =
                                Expression.Not(Expression.TypeIs(Expression.Property(exception, "InnerException"), typeof(OverflowException))),
                                Expression.Rethrow()),
                            fallbackToDoubleExpr)
                expr = Expression.TryCatch(
                           catchExpr);
            if (target.LimitType.IsEnum)
                // Make sure the result type is an enum unless we were expecting a bool.
                        expr = expr.Cast(target.LimitType).Cast(typeof(object));
            if (Operation == ExpressionType.Equal || Operation == ExpressionType.NotEqual)
                expr = Expression.TryCatch(expr,
                    Expression.Catch(typeof(InvalidCastException),
                                     Operation == ExpressionType.Equal ? ExpressionCache.BoxedFalse : ExpressionCache.BoxedTrue));
            return new DynamicMetaObject(expr, target.CombineRestrictions(arg));
        private DynamicMetaObject BinaryNumericStringOp(DynamicMetaObject target, DynamicMetaObject arg)
            // We can't determine the operation type at compile time, we'll need another dynamic site
            //     tmp = Parser.ScanNumber(arg)
            //     if (tmp == null) { throw new RuntimeError("BadNumericConstant") }
            //     dynamic op target tmp
            // For equality comparison operators, if the conversion fails, we shouldn't raise
            // an exception, so those must be wrapped in try/catch, like:
            //     try { /* as above */ }
            //     catch (InvalidCastException) { true or false (depending on Operation type) }
            List<Expression> stmts = new List<Expression>();
            Expression targetExpr = target.Expression;
                    Operation == ExpressionType.Subtract || Operation == ExpressionType.Divide
                    || Operation == ExpressionType.Modulo || Operation == ExpressionType.And
                    || Operation == ExpressionType.Or || Operation == ExpressionType.ExclusiveOr
                    || Operation == ExpressionType.LeftShift || Operation == ExpressionType.RightShift,
                    "string lhs not allowed for operation");
                targetExpr = ConvertStringToNumber(target.Expression, arg.LimitType);
            var argExpr = arg.LimitType == typeof(string) ? ConvertStringToNumber(arg.Expression, target.LimitType) : arg.Expression;
            stmts.Add(
                DynamicExpression.Dynamic(PSBinaryOperationBinder.Get(Operation), typeof(object), targetExpr, argExpr));
            Expression expr = Expression.Block(temps, stmts);
        /// Use the tokenizer to scan a number and convert it to a number of any type.
        /// <param name="expr">
        /// The expression that refers to a string to be converted to a number of any type.
        /// Primarily used as part of an error message.  If the string is not a number, we want to raise an exception saying the
        /// string can't be converted to this type.  Note that if the string is a valid number, it need not be this type.
        internal static Expression ConvertStringToNumber(Expression expr, Type toType)
            if (!toType.IsNumeric())
                // toType is only mostly for diagnostics, so it doesn't need to be correct.  If it's not numeric, we're
                // doing something like "42" - "10", and toType is the type of the "other" operand, in this case, it
                // would string.  Fall back to int if Parser.ScanNumber fails.
                toType = typeof(int);
                CachedReflectionInfo.Parser_ScanNumber,
                expr.Cast(typeof(string)),
                Expression.Constant(toType, typeof(Type)),
                Expression.Constant(true));
        private static DynamicMetaObject GetArgAsNumericOrPrimitive(DynamicMetaObject arg, Type targetType)
            if (arg.Value == null)
                return new DynamicMetaObject(ExpressionCache.Constant(0), arg.PSGetTypeRestriction(), 0);
            bool boolToDecimal = false;
            if (arg.LimitType.IsNumericOrPrimitive() && !arg.LimitType.IsEnum)
                if (!(targetType == typeof(decimal) && arg.LimitType == typeof(bool)))
                    return arg;
                // All other numeric conversions are simple casts, but bool=>decimal is not supported by LINQ (via Convert), so
                // we must do the conversion ourselves.
                boolToDecimal = true;
            // ConstrainedLanguage note - calls to this conversion only target numeric types.
            var conversion = LanguagePrimitives.FigureConversion(arg.Value, targetType, out debase);
            if (conversion.Rank == ConversionRank.ImplicitCast || boolToDecimal || arg.LimitType.IsEnum)
                    PSConvertBinder.InvokeConverter(conversion, arg.Expression, targetType, debase, ExpressionCache.InvariantCulture),
                    arg.PSGetTypeRestriction());
        private static Type GetBitwiseOpType(TypeCode opTypeCode)
            if ((int)opTypeCode <= (int)TypeCode.Int32) { opType = typeof(int); }
            else if ((int)opTypeCode <= (int)TypeCode.UInt32) { opType = typeof(uint); }
            else if ((int)opTypeCode <= (int)TypeCode.Int64) { opType = typeof(long); }
            // Because we use unsigned for -bnot, to be consistent, we promote to unsigned here too (-band,-bor,-xor)
            else { opType = typeof(ulong); }
            return opType;
        #region "Arithmetic" operations
        private DynamicMetaObject BinaryAdd(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
                return new DynamicMetaObject(arg.Expression.Cast(typeof(object)), target.CombineRestrictions(arg));
            if (target.LimitType.IsNumericOrPrimitive() && target.LimitType != typeof(char))
                var numericArg = GetArgAsNumericOrPrimitive(arg, target.LimitType);
                if (numericArg != null)
                    return BinaryNumericOp("Add", target, numericArg);
                if (arg.LimitType == typeof(string))
                    return BinaryNumericStringOp(target, arg);
            Expression lhsStringExpr = null;
                lhsStringExpr = target.Expression.Cast(typeof(string));
            else if (target.LimitType == typeof(char))
                lhsStringExpr =
                    Expression.New(CachedReflectionInfo.String_ctor_char_int,
                                   target.Expression.Cast(typeof(char)),
                                   ExpressionCache.Constant(1));
            if (lhsStringExpr != null)
                // For string concatenation, simply add the 2 strings, possibly converting the rhs first.
                    Expression.Call(CachedReflectionInfo.String_Concat_String,
                                    lhsStringExpr,
                                    PSToStringBinder.InvokeToString(
                                        ExpressionCache.GetExecutionContextFromTLS,
                                        arg.Expression)),
            var lhsEnumerator = PSEnumerableBinder.IsEnumerable(target);
            if (lhsEnumerator != null)
                // target is enumerable, so we're creating a new array.
                var rhsEnumerator = PSEnumerableBinder.IsEnumerable(arg);
                Expression call;
                if (rhsEnumerator != null)
                    // Adding 2 lists
                    call = Expression.Call(CachedReflectionInfo.EnumerableOps_AddEnumerable,
                                           lhsEnumerator.Expression.Cast(typeof(IEnumerator)),
                                           rhsEnumerator.Expression.Cast(typeof(IEnumerator)));
                else if (target.Value is object[] targetArray)
                    // Adding 1 item to an object[]
                    // This is an optimisation over the default EnumerableOps_AddObject.
                    call = Expression.Call(CachedReflectionInfo.ArrayOps_AddObject,
                                           target.Expression.Cast(typeof(object[])),
                                           arg.Expression.Cast(typeof(object)));
                    // Adding 1 item to a list
                    call = Expression.Call(CachedReflectionInfo.EnumerableOps_AddObject,
                return new DynamicMetaObject(call, target.CombineRestrictions(arg));
                if (arg.Value is IDictionary)
                        Expression.Call(CachedReflectionInfo.HashtableOps_Add,
                                        target.Expression.Cast(typeof(IDictionary)),
                                        arg.Expression.Cast(typeof(IDictionary))),
                return target.ThrowRuntimeError(new DynamicMetaObject[] { arg }, BindingRestrictions.Empty,
                                                "AddHashTableToNonHashTable", ParserStrings.AddHashTableToNonHashTable);
            return CallImplicitOp("op_Addition", target, arg, "+", errorSuggestion);
        private DynamicMetaObject BinarySub(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
            return BinarySubDivOrRem(target, arg, errorSuggestion, "Sub", "op_Subtraction", "-");
        private DynamicMetaObject BinaryMultiply(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
                // Result is null regardless of the arg.
                return new DynamicMetaObject(ExpressionCache.NullConstant, target.PSGetTypeRestriction());
            if (target.LimitType.IsNumeric())
                    return BinaryNumericOp("Multiply", target, numericArg);
                Expression argExpr = arg.LimitType == typeof(string)
                                         ? ConvertStringToNumber(arg.Expression, typeof(int)).Convert(typeof(int))
                                         : arg.CastOrConvert(typeof(int));
                    Expression.Call(CachedReflectionInfo.StringOps_Multiply,
                                    argExpr),
                                         ? ConvertStringToNumber(arg.Expression, typeof(int)).Convert(typeof(uint))
                                         : arg.CastOrConvert(typeof(uint));
                if (target.LimitType.IsArray)
                    var elementType = target.LimitType.GetElementType();
                        Expression.Call(CachedReflectionInfo.ArrayOps_Multiply.MakeGenericMethod(elementType),
                                        target.Expression.Cast(elementType.MakeArrayType()),
                    Expression.Call(CachedReflectionInfo.EnumerableOps_Multiply,
                                    lhsEnumerator.Expression,
            return CallImplicitOp("op_Multiply", target, arg, "*", errorSuggestion);
        private DynamicMetaObject BinaryDivide(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
            return BinarySubDivOrRem(target, arg, errorSuggestion, "Divide", "op_Division", "/");
        private DynamicMetaObject BinaryRemainder(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
            return BinarySubDivOrRem(target, arg, errorSuggestion, "Remainder", "op_Modulus", "%");
        private DynamicMetaObject BinarySubDivOrRem(DynamicMetaObject target,
                                                    DynamicMetaObject arg,
                                                    DynamicMetaObject errorSuggestion,
                                                    string numericOpMethodName,
                                                    string implicitOpMethodName,
                                                    string errorOperatorText)
                // if target is null, just use 0
                target = new DynamicMetaObject(ExpressionCache.Constant(0), target.PSGetTypeRestriction(), 0);
            if (target.LimitType.IsNumericOrPrimitive())
                    return BinaryNumericOp(numericOpMethodName, target, numericArg);
                // Left is a string.  We convert it to a number and try binding again.
            return CallImplicitOp(implicitOpMethodName, target, arg, errorOperatorText, errorSuggestion);
        private DynamicMetaObject Shift(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion, string userOp, Func<Expression, Expression, Expression> exprGenerator)
                return new DynamicMetaObject(ExpressionCache.Constant(0).Convert(typeof(object)), target.PSGetTypeRestriction());
            if (target.LimitType == typeof(string) || arg.LimitType == typeof(string))
            var typeCode = LanguagePrimitives.GetTypeCode(target.LimitType);
            if (!target.LimitType.IsNumeric())
                return CallImplicitOp(userOp, target, arg, GetOperatorText(), errorSuggestion);
            var resultType = typeof(int);
            var conversion = LanguagePrimitives.FigureConversion(arg.Value, resultType, out debase);
                return PSConvertBinder.ThrowNoConversion(arg, typeof(int), this, _version);
            var numericArg = PSConvertBinder.InvokeConverter(conversion, arg.Expression, resultType, debase, ExpressionCache.InvariantCulture);
            if (typeCode == TypeCode.Decimal || typeCode == TypeCode.Double || typeCode == TypeCode.Single)
                var opType = (typeCode == TypeCode.Decimal) ? typeof(DecimalOps) : typeof(DoubleOps);
                var castType = (typeCode == TypeCode.Decimal) ? typeof(decimal) : typeof(double);
                var methodName = userOp.Substring(3);  // Drop the 'op_' prefix to figure out our internal method name.
                    Expression.Call(opType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static), target.Expression.Cast(castType), numericArg),
            var targetExpr = target.Expression.Cast(target.LimitType);
            numericArg = Expression.And(numericArg, Expression.Constant(typeCode < TypeCode.Int64 ? 0x1f : 0x3f, typeof(int)));
                exprGenerator(targetExpr, numericArg).Cast(typeof(object)),
        private DynamicMetaObject LeftShift(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
            return Shift(target, arg, errorSuggestion, "op_LeftShift", Expression.LeftShift);
        private DynamicMetaObject RightShift(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
            return Shift(target, arg, errorSuggestion, "op_RightShift", Expression.RightShift);
        private DynamicMetaObject BinaryBitwiseXor(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
            return BinaryBitwiseOp(target, arg, errorSuggestion, Expression.ExclusiveOr, "op_ExclusiveOr", "-bxor", "BXor");
        private DynamicMetaObject BinaryBitwiseOr(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
            return BinaryBitwiseOp(target, arg, errorSuggestion, Expression.Or, "op_BitwiseOr", "-bor", "BOr");
        private DynamicMetaObject BinaryBitwiseAnd(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
            return BinaryBitwiseOp(target, arg, errorSuggestion, Expression.And, "op_BitwiseAnd", "-band", "BAnd");
        private DynamicMetaObject BinaryBitwiseOp(DynamicMetaObject target,
                                                  Func<Expression, Expression, Expression> exprGenerator,
                                                  string implicitMethodName,
                                                  string errorOperatorName,
            if (target.Value == null && arg.Value == null)
                return new DynamicMetaObject(ExpressionCache.Constant(0).Cast(typeof(object)), target.CombineRestrictions(arg));
            var targetUnderlyingType = (target.LimitType.IsEnum) ? Enum.GetUnderlyingType(target.LimitType) : target.LimitType;
            var argUnderlyingType = (arg.LimitType.IsEnum) ? Enum.GetUnderlyingType(arg.LimitType) : arg.LimitType;
            if (targetUnderlyingType.IsNumericOrPrimitive() || argUnderlyingType.IsNumericOrPrimitive())
                TypeCode leftTypeCode = LanguagePrimitives.GetTypeCode(targetUnderlyingType);
                TypeCode rightTypeCode = LanguagePrimitives.GetTypeCode(argUnderlyingType);
                Type opImplType;
                Type toType;
                DynamicMetaObject numericTarget;
                DynamicMetaObject numericArg;
                if (!targetUnderlyingType.IsNumericOrPrimitive())
                    opType = GetBitwiseOpType(rightTypeCode);
                    numericTarget = GetArgAsNumericOrPrimitive(target, opType);
                    numericArg = arg;
                else if (!argUnderlyingType.IsNumericOrPrimitive())
                    opType = GetBitwiseOpType(leftTypeCode);
                    numericTarget = target;
                    numericArg = GetArgAsNumericOrPrimitive(arg, opType);
                if (opTypeCode == TypeCode.Decimal)
                    toType = typeof(decimal);
                                numericTarget.Expression.Cast(numericTarget.LimitType).Convert(toType),
                                numericArg.Expression.Cast(numericArg.LimitType).Convert(toType)),
                                numericTarget.CombineRestrictions(numericArg));
                if (opTypeCode == TypeCode.Double || opTypeCode == TypeCode.Single)
                    toType = typeof(double);
                // Figure out the smallest type necessary so we don't lose information.
                // For uint, V2 promoted to long, but it's more correct to use uint.
                // For ulong, V2 incorrectly used long, this is fixed here.
                // For float, double, and decimal operands, we used to use long because V2 did.
                opType = GetBitwiseOpType((int)leftTypeCode >= (int)rightTypeCode ? leftTypeCode : rightTypeCode);
                if (numericTarget != null && numericArg != null)
                    var expr = exprGenerator(numericTarget.Expression.Cast(numericTarget.LimitType).Cast(opType),
                                             numericArg.Expression.Cast(numericArg.LimitType).Cast(opType));
                        expr = expr.Cast(target.LimitType);
                    expr = expr.Cast(typeof(object));
                    return new DynamicMetaObject(expr, numericTarget.CombineRestrictions(numericArg));
            return CallImplicitOp(implicitMethodName, target, arg, errorOperatorName, errorSuggestion);
        #endregion "Arithmetic" operations
        #region Comparison operations
        private DynamicMetaObject CompareEQ(DynamicMetaObject target,
                                            DynamicMetaObject errorSuggestion)
                    arg.Value == null ? ExpressionCache.BoxedTrue : ExpressionCache.BoxedFalse,
            if (enumerable == null && arg.Value == null)
                    ExpressionCache.BoxedFalse,
            return BinaryComparisonCommon(enumerable, target, arg)
                ?? BinaryEqualityComparison(target, arg);
        private DynamicMetaObject CompareNE(DynamicMetaObject target,
                    arg.Value == null ? ExpressionCache.BoxedFalse : ExpressionCache.BoxedTrue,
                return new DynamicMetaObject(ExpressionCache.BoxedTrue,
        private DynamicMetaObject BinaryEqualityComparison(DynamicMetaObject target, DynamicMetaObject arg)
            var toResult = Operation == ExpressionType.NotEqual ? (Func<Expression, Expression>)Expression.Not : e => e;
                var targetExpr = target.Expression.Cast(typeof(string));
                // Doing a string comparison no matter what.
                var argExpr = arg.LimitType != typeof(string)
                                  ? DynamicExpression.Dynamic(PSToStringBinder.Get(), typeof(string),
                                                              arg.Expression, ExpressionCache.GetExecutionContextFromTLS)
                                  : arg.Expression.Cast(typeof(string));
                    toResult(Compiler.CallStringEquals(targetExpr, argExpr, _ignoreCase)).Cast(typeof(object)),
            if (target.LimitType == typeof(char) && _ignoreCase)
                if (arg.LimitType == typeof(char))
                            Operation == ExpressionType.Equal ? CachedReflectionInfo.CharOps_CompareIeq : CachedReflectionInfo.CharOps_CompareIne,
                            arg.Expression.Cast(typeof(char))),
                        target.PSGetTypeRestriction().Merge(arg.PSGetTypeRestriction()));
                            Operation == ExpressionType.Equal ? CachedReflectionInfo.CharOps_CompareStringIeq : CachedReflectionInfo.CharOps_CompareStringIne,
                            arg.Expression.Cast(typeof(string))),
            Expression objectEqualsCall = Expression.Call(target.Expression.Cast(typeof(object)),
                                                          CachedReflectionInfo.Object_Equals,
            var targetType = target.LimitType;
            // ConstrainedLanguage note - calls to this conversion are protected by the binding rules below.
            if (conversion.Rank == ConversionRank.Identity || conversion.Rank == ConversionRank.Assignable
                || (conversion.Rank == ConversionRank.NullToRef && targetType != typeof(PSReference)))
                // In these cases, no actual conversion is happening, and conversion.Converter will just return
                // the value to be converted. So there is no need to convert the value and compare again.
                return new DynamicMetaObject(toResult(objectEqualsCall).Cast(typeof(object)), target.CombineRestrictions(arg));
            BindingRestrictions bindingRestrictions = target.CombineRestrictions(arg);
            bindingRestrictions = bindingRestrictions.Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, targetType, _version));
            // If there is no conversion, then just rely on 'objectEqualsCall' which most likely will return false. If we attempted the
            // conversion, we'd need extra code to catch an exception we know will happen just to return false.
                return new DynamicMetaObject(toResult(objectEqualsCall).Cast(typeof(object)), bindingRestrictions);
            // A conversion exists.  Generate:
            //    tmp = target.Equals(arg)
            //        if (!tmp) { tmp = target.Equals(Convert(arg, target.GetType())) }
            //    } catch (InvalidCastException) { tmp = false }
            //    return (operator is -eq/-ceq/-ieq) ? tmp : !tmp
            var resultTmp = Expression.Parameter(typeof(bool));
            Expression secondEqualsCall =
                Expression.Call(target.Expression.Cast(typeof(object)),
                                PSConvertBinder.InvokeConverter(conversion, arg.Expression, targetType, debase, ExpressionCache.InvariantCulture).Cast(typeof(object)));
            var expr = Expression.Block(
                new ParameterExpression[] { resultTmp },
                Expression.Assign(resultTmp, objectEqualsCall),
                Expression.IfThen(Expression.Not(resultTmp),
                                  Expression.TryCatch(Expression.Assign(resultTmp, secondEqualsCall),
                                                   Expression.Assign(resultTmp, ExpressionCache.Constant(false))))),
                toResult(resultTmp));
            return new DynamicMetaObject(expr.Cast(typeof(object)), bindingRestrictions);
        private static Expression CompareWithZero(DynamicMetaObject target, Func<Expression, Expression, Expression> comparer)
            return comparer(target.Expression.Cast(target.LimitType), ExpressionCache.Constant(0).Cast(target.LimitType)).Cast(typeof(object));
        private DynamicMetaObject CompareLT(DynamicMetaObject target,
            if (enumerable == null && (target.Value == null || arg.Value == null))
                      target.LimitType.IsNumeric() ? CompareWithZero(target, Expression.LessThan)
                    : arg.LimitType.IsNumeric() ? CompareWithZero(arg, Expression.GreaterThanOrEqual)
                    : arg.Value != null ? ExpressionCache.BoxedTrue
                    : ExpressionCache.BoxedFalse;
                return new DynamicMetaObject(result, target.CombineRestrictions(arg));
                ?? BinaryComparison(target, arg, static e => Expression.LessThan(e, ExpressionCache.Constant(0)));
        private DynamicMetaObject CompareLE(DynamicMetaObject target,
                    : target.Value != null ? ExpressionCache.BoxedFalse
                    : ExpressionCache.BoxedTrue;
                ?? BinaryComparison(target, arg, static e => Expression.LessThanOrEqual(e, ExpressionCache.Constant(0)));
        private DynamicMetaObject CompareGT(DynamicMetaObject target,
            // Handle a null operand as a special case here unless the target is enumerable or if one of the operands is numeric,
            // in which case null is converted to 0 and regular numeric comparison is done.
                      target.LimitType.IsNumeric() ? CompareWithZero(target, Expression.GreaterThanOrEqual)
                    : arg.LimitType.IsNumeric() ? CompareWithZero(arg, Expression.LessThan)
                    : target.Value != null ? ExpressionCache.BoxedTrue
                ?? BinaryComparison(target, arg, static e => Expression.GreaterThan(e, ExpressionCache.Constant(0)));
        private DynamicMetaObject CompareGE(DynamicMetaObject target,
                    : arg.Value != null ? ExpressionCache.BoxedFalse
                ?? BinaryComparison(target, arg, static e => Expression.GreaterThanOrEqual(e, ExpressionCache.Constant(0)));
        private DynamicMetaObject BinaryComparison(DynamicMetaObject target, DynamicMetaObject arg, Func<Expression, Expression> toResult)
                var expr = Expression.Call(CachedReflectionInfo.StringOps_Compare, targetExpr, argExpr, ExpressionCache.InvariantCulture,
                                           _ignoreCase
                                           ? ExpressionCache.CompareOptionsIgnoreCase
                                           : ExpressionCache.CompareOptionsNone);
                    toResult(expr).Cast(typeof(object)),
            Expression argConverted;
            if (conversion.Rank == ConversionRank.Identity || conversion.Rank == ConversionRank.Assignable)
                argConverted = arg.Expression;
            else if (conversion.Rank == ConversionRank.None)
                // If there is no conversion, then don't bother to invoke the converter. We raise the exception directly.
                var valueToConvert = debase
                    ? Expression.Call(CachedReflectionInfo.PSObject_Base, arg.Expression)
                    : arg.Expression.Cast(typeof(object));
                var errorMsgTuple = Expression.Call(
                    CachedReflectionInfo.LanguagePrimitives_GetInvalidCastMessages,
                    valueToConvert, Expression.Constant(targetType, typeof(Type)));
                argConverted = Compiler.ThrowRuntimeError(
                    "ComparisonFailure", ExtendedTypeSystem.ComparisonFailure, targetType,
                    DynamicExpression.Dynamic(PSToStringBinder.Get(), typeof(string), target.Expression, ExpressionCache.GetExecutionContextFromTLS),
                    DynamicExpression.Dynamic(PSToStringBinder.Get(), typeof(string), arg.Expression, ExpressionCache.GetExecutionContextFromTLS),
                    Expression.Property(errorMsgTuple, "Item2"));
                // Invoke the converter. We can raise the exception if the conversion throws InvalidCastException.
                var innerException = Expression.Parameter(typeof(InvalidCastException));
                argConverted =
                            Compiler.ThrowRuntimeErrorWithInnerException("ComparisonFailure",
                                Expression.Constant(ExtendedTypeSystem.ComparisonFailure), innerException, targetType,
                                Expression.Property(innerException, CachedReflectionInfo.Exception_Message))));
            // Prefer IComparable<T> over IComparable if possible
            if (target.LimitType == arg.LimitType)
                foreach (var i in target.Value.GetType().GetInterfaces())
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IComparable<>))
                            toResult(Expression.Call(Expression.Convert(target.Expression, i),
                                                     i.GetMethod("CompareTo"),
                                                     argConverted.Cast(arg.LimitType))).Cast(typeof(object)),
            if (target.Value is IComparable)
                    toResult(Expression.Call(target.Expression.Cast(typeof(IComparable)),
                                             CachedReflectionInfo.IComparable_CompareTo,
                                             argConverted.Cast(typeof(object)))).Cast(typeof(object)),
            var throwExpr = Compiler.ThrowRuntimeError("NotIcomparable", ExtendedTypeSystem.NotIcomparable, this.ReturnType, target.Expression);
            // Try object.Equals.  If the objects compare equal, the result is known (true for -ge or -le, false for -gt or -lt), otherwise
            // throw because the objects can't be compared in any meaningful way.
                                    arg.Expression.Cast(typeof(object))),
                    (Operation == ExpressionType.GreaterThanOrEqual || Operation == ExpressionType.LessThanOrEqual)
                        ? ExpressionCache.BoxedTrue : ExpressionCache.BoxedFalse,
                    throwExpr),
        private DynamicMetaObject BinaryComparisonCommon(DynamicMetaObject targetAsEnumerator, DynamicMetaObject target, DynamicMetaObject arg)
            if (targetAsEnumerator != null && !_scalarCompare)
                // If the target is enumerable, the we generate an object[] result with elements matching.
                // The iteration will be done in a pre-compiled method, but the comparison is done with
                // a dynamically generated lambda that uses a binder
                    Expression.Call(CachedReflectionInfo.EnumerableOps_Compare,
                                    targetAsEnumerator.Expression,
                                    Expression.Constant(GetScalarCompareDelegate())),
                    targetAsEnumerator.Restrictions.Merge(arg.PSGetTypeRestriction()));
                    string numericMethod = null;
                        case ExpressionType.Equal: numericMethod = "CompareEq"; break;
                        case ExpressionType.NotEqual: numericMethod = "CompareNe"; break;
                        case ExpressionType.GreaterThan: numericMethod = "CompareGt"; break;
                        case ExpressionType.GreaterThanOrEqual: numericMethod = "CompareGe"; break;
                        case ExpressionType.LessThan: numericMethod = "CompareLt"; break;
                        case ExpressionType.LessThanOrEqual: numericMethod = "CompareLe"; break;
                    return BinaryNumericOp(numericMethod, target, numericArg);
        #endregion Comparison operations
    /// The binder for unary operators like !, -, or +.
    internal sealed class PSUnaryOperationBinder : UnaryOperationBinder
        private static PSUnaryOperationBinder s_notBinder;
        private static PSUnaryOperationBinder s_bnotBinder;
        private static PSUnaryOperationBinder s_unaryMinus;
        private static PSUnaryOperationBinder s_unaryPlusBinder;
        private static PSUnaryOperationBinder s_incrementBinder;
        private static PSUnaryOperationBinder s_decrementBinder;
        internal static PSUnaryOperationBinder Get(ExpressionType operation)
                    if (s_notBinder == null)
                        Interlocked.CompareExchange(ref s_notBinder, new PSUnaryOperationBinder(operation), null);
                    return s_notBinder;
                    if (s_bnotBinder == null)
                        Interlocked.CompareExchange(ref s_bnotBinder, new PSUnaryOperationBinder(operation), null);
                    return s_bnotBinder;
                    if (s_unaryPlusBinder == null)
                        Interlocked.CompareExchange(ref s_unaryPlusBinder, new PSUnaryOperationBinder(operation), null);
                    return s_unaryPlusBinder;
                    if (s_unaryMinus == null)
                        Interlocked.CompareExchange(ref s_unaryMinus, new PSUnaryOperationBinder(operation), null);
                    return s_unaryMinus;
                    if (s_incrementBinder == null)
                        Interlocked.CompareExchange(ref s_incrementBinder, new PSUnaryOperationBinder(operation), null);
                    return s_incrementBinder;
                    if (s_decrementBinder == null)
                        Interlocked.CompareExchange(ref s_decrementBinder, new PSUnaryOperationBinder(operation), null);
                    return s_decrementBinder;
            throw new NotImplementedException("Unimplemented unary operation");
        private PSUnaryOperationBinder(ExpressionType operation) : base(operation)
        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
                return this.DeferForPSObject(target);
                    return Not(target, errorSuggestion).WriteToDebugLog(this);
                    return BNot(target, errorSuggestion).WriteToDebugLog(this);
                    return UnaryPlus(target, errorSuggestion).WriteToDebugLog(this);
                    return UnaryMinus(target, errorSuggestion).WriteToDebugLog(this);
                    return IncrDecr(target, 1, errorSuggestion).WriteToDebugLog(this);
                    return IncrDecr(target, -1, errorSuggestion).WriteToDebugLog(this);
            return string.Create(CultureInfo.InvariantCulture, $"PSUnaryOperationBinder {this.Operation}");
        internal DynamicMetaObject Not(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            // TODO: check op_LogicalNot
            // This could generate a dynamic site in the expr, which means we might have the same type test twice.
            // We should do better, but this is the simplest implementation, we can add specific cases to handle more common
            // cases if necessary.
            var targetExpr = target.CastOrConvert(typeof(bool));
                Expression.Not(targetExpr).Cast(typeof(object)),
                target.PSGetTypeRestriction());
        internal DynamicMetaObject BNot(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
                return new DynamicMetaObject(ExpressionCache.Constant(-1).Cast(typeof(object)), target.PSGetTypeRestriction());
            // If the type implements the operator, prefer that.
            var method = target.LimitType.GetMethod("op_OnesComplement", BindingFlags.Static | BindingFlags.Public, null,
                                                    new Type[] { target.LimitType }, null);
                    Expression.OnesComplement(target.Expression.Cast(target.LimitType), method).Cast(typeof(object)),
            // Otherwise, do a conversion, as necessary, or throw (and say we can't convert to int, a better message
            // would be nice, but this error is good enough.)
                // If we have a string, we defer resolving the operation type until after we know the type of the operand,
                // so generate a dynamic site.
                                              PSBinaryOperationBinder.ConvertStringToNumber(target.Expression, typeof(int))),
            Expression targetExpr = null;
                var conversion = LanguagePrimitives.FigureConversion(target.Value, resultType, out debase);
                if (conversion.Rank != ConversionRank.None)
                    targetExpr = PSConvertBinder.InvokeConverter(conversion, target.Expression, resultType, debase,
                                                                 ExpressionCache.InvariantCulture);
                    conversion = LanguagePrimitives.FigureConversion(target.Value, resultType, out debase);
                if (typeCode < TypeCode.Int32)
                    targetExpr = target.LimitType.IsEnum
                        ? target.Expression.Cast(Enum.GetUnderlyingType(target.LimitType))
                        : target.Expression.Cast(target.LimitType);
                    targetExpr = targetExpr.Cast(typeof(int));
                else if (typeCode <= TypeCode.UInt64)
                    var targetConvertType = target.LimitType;
                    if (targetConvertType.IsEnum)
                        targetConvertType = Enum.GetUnderlyingType(targetConvertType);
                    targetExpr = target.Expression.Cast(targetConvertType);
                        Expression.Call(opType.GetMethod("BNot", BindingFlags.Static | BindingFlags.NonPublic), target.Expression.Convert(castType)),
            if (targetExpr != null)
                Expression result = Expression.OnesComplement(targetExpr);
                    result = result.Cast(target.LimitType);
                return new DynamicMetaObject(result.Cast(typeof(object)), target.PSGetTypeRestriction());
            return errorSuggestion ?? PSConvertBinder.ThrowNoConversion(target, typeof(int), this, -1);
        private DynamicMetaObject UnaryPlus(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
                var expr = target.Expression.Cast(target.LimitType);
                if (target.LimitType == typeof(byte) || target.LimitType == typeof(sbyte))
                    // promote to int, unary plus doesn't support byte directly.
                    expr = expr.Cast(typeof(int));
                    Expression.UnaryPlus(expr).Cast(typeof(object)),
            // Use a nested dynamic site that adds 0.  This won't change the sign, but it will attempt conversions.  This is slower than it needs
            // to be, but should be hit rarely.
                DynamicExpression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Add), typeof(object), ExpressionCache.Constant(0), target.Expression),
        private DynamicMetaObject UnaryMinus(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
                    Expression.Negate(expr).Cast(typeof(object)),
            // Use a nested dynamic site that subtracts from 0.  This won't change the sign, but it will attempt conversions.  This is slower than it needs
                DynamicExpression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Subtract), typeof(object), ExpressionCache.Constant(0), target.Expression),
        private DynamicMetaObject IncrDecr(DynamicMetaObject target, int valueToAdd, DynamicMetaObject errorSuggestion)
                return new DynamicMetaObject(ExpressionCache.Constant(valueToAdd).Cast(typeof(object)), target.PSGetTypeRestriction());
                var arg = new DynamicMetaObject(ExpressionCache.Constant(valueToAdd), BindingRestrictions.Empty, valueToAdd);
                var result = PSBinaryOperationBinder.Get(ExpressionType.Add).FallbackBinaryOperation(target, arg, errorSuggestion);
                    result.Expression,
            return errorSuggestion ?? target.ThrowRuntimeError(
                Array.Empty<DynamicMetaObject>(),
                BindingRestrictions.Empty,
                "OperatorRequiresNumber",
                ParserStrings.OperatorRequiresNumber,
                Expression.Constant((Operation == ExpressionType.Increment ? TokenKind.PlusPlus : TokenKind.MinusMinus).Text()),
                Expression.Constant(target.LimitType, typeof(Type)));
    /// The binder for converting a value, e.g. [int]"42"
    internal sealed class PSConvertBinder : ConvertBinder
        private static readonly Dictionary<Type, PSConvertBinder> s_binderCache = new Dictionary<Type, PSConvertBinder>();
        public static PSConvertBinder Get(Type type)
            PSConvertBinder result;
                if (!s_binderCache.TryGetValue(type, out result))
                    result = new PSConvertBinder(type);
                    s_binderCache.Add(type, result);
        private PSConvertBinder(Type type)
            : base(type, /*explicit=*/false)
                CacheTarget((Func<CallSite, object, string>)(StringToStringRule));
                return new DynamicMetaObject(Expression.Default(this.Type), target.PSGetTypeRestriction()).WriteToDebugLog(this);
            var resultType = this.Type;
            // ConstrainedLanguage note - this is the main conversion mechanism. If the runspace has ever used
            // ConstrainedLanguage, then start baking in the language mode to the binding rules.
                return errorSuggestion.WriteToDebugLog(this);
            BindingRestrictions restrictions = target.PSGetTypeRestriction();
            restrictions = restrictions.Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, resultType, _version));
                InvokeConverter(conversion, target.Expression, resultType, debase, ExpressionCache.InvariantCulture),
                "PSConvertBinder [{0}]  ver:{1}",
                Microsoft.PowerShell.ToStringCodeMethods.Type(this.Type, true),
                foreach (PSConvertBinder binder in s_binderCache.Values)
        internal static DynamicMetaObject ThrowNoConversion(DynamicMetaObject target, Type toType, DynamicMetaObjectBinder binder,
            int currentVersion, params DynamicMetaObject[] args)
            // No conversion, so the result expression raises an error:
            //   throw new PSInvalidCastException("ConvertToFinalInvalidCastException", null,
            //       ExtendedTypeSystem.InvalidCastException,
            //       valueToConvert.ToString(), ObjectToTypeNameString(valueToConvert), resultType.ToString());
            Expression expr = Expression.Call(CachedReflectionInfo.LanguagePrimitives_ThrowInvalidCastException,
                                              Expression.Constant(toType, typeof(Type)));
            if (binder.ReturnType != typeof(void))
                expr = Expression.Block(expr, Expression.Default(binder.ReturnType));
            BindingRestrictions bindingRestrictions = target.CombineRestrictions(args);
            bindingRestrictions = bindingRestrictions.Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(binder, toType, currentVersion));
            return new DynamicMetaObject(expr, bindingRestrictions);
        /// Convert argument to a ByRef-like type via implicit or explicit conversion.
        /// The argument to be converted to a ByRef-like type.
        /// <param name="resultType">
        /// The ByRef-like type to convert to.
        internal static Expression ConvertToByRefLikeTypeViaCasting(DynamicMetaObject argument, Type resultType)
            var baseObject = PSObject.Base(argument.Value);
            // Source value cannot be null or AutomationNull, and it cannot be a pure PSObject.
            if (baseObject != null && baseObject is not PSObject)
                Type fromType = baseObject.GetType();
                LanguagePrimitives.FigureCastConversion(fromType, resultType, ref rank);
                if (rank != ConversionRank.None)
                    var valueToConvert = baseObject == argument.Value
                        ? argument.Expression
                        : Expression.Call(CachedReflectionInfo.PSObject_Base, argument.Expression);
                    return Expression.Convert(valueToConvert.Cast(fromType), resultType);
        internal static Expression InvokeConverter(LanguagePrimitives.IConversionData conversion,
                                                   Expression value,
                                                   bool debase,
                                                   Expression formatProvider)
            Expression conv;
                conv = debase ? Expression.Call(CachedReflectionInfo.PSObject_Base, value) : value;
                Expression valueToConvert, valueAsPSObject;
                    // Caller has verified the value is a PSObject.
                    valueToConvert = Expression.Call(CachedReflectionInfo.PSObject_Base, value);
                    valueAsPSObject = value.Cast(typeof(PSObject));
                    // Caller has verified the value is not a PSObject, or that PSObject.Base should not be called.
                    // If the object is some sort of PSObject, it's most likely a derived to base conversion.
                    valueToConvert = value.Cast(typeof(object));
                    valueAsPSObject = ExpressionCache.NullPSObject;
                conv = Expression.Call(
                    Expression.Constant(conversion.Converter),
                    conversion.Converter.GetType().GetMethod("Invoke"),
                    /*valueToConvert=*/         valueToConvert,
                    /*resultType=*/             Expression.Constant(resultType, typeof(Type)),
                    /*recurse=*/                ExpressionCache.Constant(true),
                    /*originalValueToConvert=*/ valueAsPSObject,
                    /*formatProvider=*/         formatProvider,
                    /*backupTable=*/            ExpressionCache.NullTypeTable);
            // Skip adding the Convert if unnecessary (same type), or impossible (InternalPSCustomObject)
            if (conv.Type == resultType || resultType == typeof(LanguagePrimitives.InternalPSCustomObject))
                return conv;
            if (resultType.IsValueType && Nullable.GetUnderlyingType(resultType) == null)
                return Expression.Unbox(conv, resultType);
            return Expression.Convert(conv, resultType);
        private static string StringToStringRule(CallSite site, object obj)
            var str = obj as string;
            return str ?? ((CallSite<Func<CallSite, object, string>>)site).Update(site, obj);
    /// The binder to get the value of an indexable object, e.g. $x[1]
    internal sealed class PSGetIndexBinder : GetIndexBinder
        private static readonly Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints, bool>, PSGetIndexBinder> s_binderCache
                = new Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints, bool>, PSGetIndexBinder>();
        private readonly bool _allowSlicing;
        public static PSGetIndexBinder Get(int argCount, PSMethodInvocationConstraints constraints, bool allowSlicing = true)
                PSGetIndexBinder binder;
                var tuple = Tuple.Create(new CallInfo(argCount), constraints, allowSlicing);
                    binder = new PSGetIndexBinder(tuple);
        private PSGetIndexBinder(Tuple<CallInfo, PSMethodInvocationConstraints, bool> tuple)
            : base(tuple.Item1)
            _constraints = tuple.Item2;
            _allowSlicing = tuple.Item3;
                "PSGetIndexBinder indexCount={0}{1}{2} ver:{3}",
                this.CallInfo.ArgumentCount,
                _allowSlicing ? string.Empty : " slicing disallowed",
                _constraints == null ? string.Empty : " constraints: " + _constraints,
                foreach (PSGetIndexBinder binder in s_binderCache.Values)
        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
            if (!target.HasValue || indexes.Any(static mo => !mo.HasValue))
                return Defer(indexes.Prepend(target).ToArray()).WriteToDebugLog(this);
            if ((target.Value is PSObject && (PSObject.Base(target.Value) != target.Value)) ||
                indexes.Any(static mo => mo.Value is PSObject && (PSObject.Base(mo.Value) != mo.Value)))
                return this.DeferForPSObject(indexes.Prepend(target).ToArray()).WriteToDebugLog(this);
            // Check if this is a COM Object
            DynamicMetaObject comResult;
            if (ComInterop.ComBinder.TryBindGetIndex(this, target, indexes, out comResult))
                return comResult.UpdateComRestrictionsForPsObject(indexes).WriteToDebugLog(this);
                        target.ThrowRuntimeError(indexes, BindingRestrictions.Empty, "NullArray", ParserStrings.NullArray)).WriteToDebugLog(this);
            // A null index is not allowed unless the index is one of the indices used while slicing, in which case we'll attempt
            // the usual conversions from null to whatever the value being indexed supports.
            // This is oddly inconsistent e.g.:
            //     $a[$null] # error
            //     $a[$null,$null] # no error, result is an empty array
            // The rationale: V1/V2 did it, and when people are slicing, it's better to return some of the results than none.
            if (indexes.Length == 1 && indexes[0].Value == null && _allowSlicing)
                        target.ThrowRuntimeError(indexes, BindingRestrictions.Empty, "NullArrayIndex", ParserStrings.NullArrayIndex)).WriteToDebugLog(this);
                return GetIndexArray(target, indexes, errorSuggestion).WriteToDebugLog(this);
            var defaultMember = target.LimitType.GetCustomAttributes<DefaultMemberAttribute>(true).FirstOrDefault();
            PropertyInfo lengthProperty = null;
            foreach (var i in target.LimitType.GetInterfaces())
                    var result = GetIndexDictionary(target, indexes, i);
                        return result.WriteToDebugLog(this);
                // If the type explicitly implements an indexer specified by an interface
                // then the DefaultMemberAttribute will not carry over to the implementation.
                // This check will catch those cases.
                if (defaultMember == null)
                    defaultMember = i.GetCustomAttributes<DefaultMemberAttribute>(inherit: false).FirstOrDefault();
                        lengthProperty = i.GetProperty("Count") ?? i.GetProperty("Length");
                return InvokeIndexer(target, indexes, errorSuggestion, defaultMember.MemberName, lengthProperty).WriteToDebugLog(this);
            return errorSuggestion ?? CannotIndexTarget(target, indexes).WriteToDebugLog(this);
        private DynamicMetaObject CannotIndexTarget(DynamicMetaObject target, DynamicMetaObject[] indexes)
            // We want to have
            //    $x[0]
            // Be equivalent to
            //    $x
            // When $x doesn't have any other way of being indexed.  If the index is anything other than 0, we'll
            // throw an error.
            // The motivation for this magic is largely driven by the desire to avoid breaking scripts written around
            // workflows.  A workflow that wrote a single object to the pipeline had always returned a collection, so
            // scripts needed to index.  This semantic doesn't match the script semantics - a single object written is
            // not wrapped in a collection.  The inconsistency had to be addressed, but this code had to be added to
            // avoid breaking partner scripts.
            // Add version / language checks
            BindingRestrictions bindingRestrictions = target.CombineRestrictions(indexes);
            // This may be thrown due to a type that was disallowed only due to constrained language.
            // Because of this, we also need a version check
            bindingRestrictions = bindingRestrictions.Merge(BinderUtils.GetVersionCheck(this, _version));
            bindingRestrictions = bindingRestrictions.Merge(BinderUtils.GetLanguageModeCheckIfHasEverUsedConstrainedLanguage());
            var call = Expression.Call(CachedReflectionInfo.ArrayOps_GetNonIndexable, target.Expression.Cast(typeof(object)),
                                       Expression.NewArrayInit(typeof(object), indexes.Select(static d => d.Expression.Cast(typeof(object)))));
            return new DynamicMetaObject(call, bindingRestrictions);
        // Index a generic dictionary via TryGetValue.  This routine does not handle slicing,
        // we defer to InvokeIndexer to handle slicing (dictionaries also support general indexing.)
        private DynamicMetaObject GetIndexDictionary(DynamicMetaObject target,
                                                     DynamicMetaObject[] indexes,
                                                     Type idictionary)
            if (indexes.Length > 1)
                // Let InvokeIndexer generate the slicing code, we wouldn't generate anything special here.
            var tryGetValue = idictionary.GetMethod("TryGetValue");
            Diagnostics.Assert(tryGetValue != null, "IDictionary<K,V> has TryGetValue");
            var parameters = tryGetValue.GetParameters();
            var keyType = parameters[0].ParameterType;
            // ConstrainedLanguage note - Calls to this conversion are protected by the binding rules below
            var conversion = LanguagePrimitives.FigureConversion(indexes[0].Value, keyType, out debase);
                // No conversion allows us to call TryGetValue, let InvokeIndexer make the decision (possibly
                // slicing, or possibly just invoke the indexer.
            if (indexes[0].LimitType.IsArray && !keyType.IsArray)
                // There was a conversion, but it's far more likely (and backwards compatible) that we want to do slicing
            bindingRestrictions = bindingRestrictions.Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, keyType, _version));
            var keyExpr = PSConvertBinder.InvokeConverter(conversion, indexes[0].Expression, keyType, debase, ExpressionCache.InvariantCulture);
            var outParam = Expression.Parameter(parameters[1].ParameterType.GetElementType(), "outParam");
                    new ParameterExpression[] { outParam },
                        Expression.Call(target.Expression.Cast(idictionary), tryGetValue, keyExpr, outParam),
                        outParam.Cast(typeof(object)),
                        GetNullResult())),
        internal static bool CanIndexFromEndWithNegativeIndex(
            DynamicMetaObject target,
            MethodInfo indexer,
            ParameterInfo[] getterParams)
            // PowerShell supports negative indexing for types that meet the following criteria:
            //      - Indexer method accepts one parameter that is typed as int
            //      - The int parameter is not a type argument from a constructed generic type
            //        (this is to exclude indexers for types that could use a negative index as
            //        a valid key like System.Linq.ILookup)
            //      - Declares a "Count" or "Length" property
            //      - Does not inherit from IDictionary<> as that is handled earlier in the binder
            // For those types, generate special code to check for negative indices, otherwise just generate
            // the call. Before we test for the above criteria explicitly, we will determine if the
            // target is of a type known to be compatible. This is done to avoid the call to Module.ResolveMethod
            // when possible.
            if (getterParams.Length != 1 || getterParams[0].ParameterType != typeof(int))
            Type limitType = target.LimitType;
            if (limitType.IsArray || limitType == typeof(string) || limitType == typeof(StringBuilder))
            if (typeof(IList).IsAssignableFrom(limitType))
            if (typeof(OrderedDictionary).IsAssignableFrom(limitType))
            // target implements IList<T>?
            if (limitType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
            // Get the base method definition of the indexer to determine if the int
            // parameter is a generic type parameter. Module.ResolveMethod is used
            // because the indexer could be a method from a constructed generic type.
            MethodBase baseMethod = indexer.Module.ResolveMethod(indexer.MetadataToken);
            return !baseMethod.GetParameters()[0].ParameterType.IsGenericParameter;
        private DynamicMetaObject IndexWithNegativeChecks(
            DynamicMetaObject index,
            PropertyInfo lengthProperty,
            Func<Expression, Expression, Expression> generateIndexOperation)
            //       len = obj.Length
            //       if (index < 0)
            //           index = index + len
            //       obj[index]
            //    } catch (Exception e) {
            //        if (StrictMode(3)) { throw }
            //        $null
            var targetTmp = Expression.Parameter(target.LimitType, "target");
            var lenTmp = Expression.Parameter(typeof(int), "len");
            var indexTmp = Expression.Parameter(typeof(int), "index");
            Expression block = Expression.Block(
                new ParameterExpression[] { targetTmp, lenTmp, indexTmp },
                // Save the target because we use it multiple times.
                Expression.Assign(targetTmp, target.Expression.Cast(target.LimitType)),
                // Save the length because we use it multiple times.
                Expression.Assign(lenTmp,
                                  Expression.Property(targetTmp, lengthProperty)),
                // Save the index because we use it multiple times
                Expression.Assign(indexTmp, index.Expression),
                // Adjust the index if it's negative
                Expression.IfThen(Expression.LessThan(indexTmp, ExpressionCache.Constant(0)),
                                  Expression.Assign(indexTmp, Expression.Add(indexTmp, lenTmp))),
                // Generate the index operation
                generateIndexOperation(targetTmp, indexTmp));
                // Do the indexing within a try/catch so we can return $null if the index is out of bounds,
                // or if the index cast fails, e.g. $a = @(1); $a['abc']
                SafeIndexResult(block),
                target.CombineRestrictions(index));
        private DynamicMetaObject GetIndexArray(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
            var array = (Array)target.Value;
            if (array.Rank > 1)
                return GetIndexMultiDimensionArray(target, indexes, errorSuggestion);
                // If the binder allows slicing, we're definitely slicing, otherwise,
                // calling the indexer will fail because there are either too many or too few indices, so
                // throw an error in that case (and not null.)
                return _allowSlicing
                           ? InvokeSlicingIndexer(target, indexes)
                           : (errorSuggestion ?? CannotIndexTarget(target, indexes));
            var slicingResult = CheckForSlicing(target, indexes);
            if (slicingResult != null)
                return slicingResult;
            var indexAsInt = ConvertIndex(indexes[0], typeof(int));
            if (indexAsInt == null)
                // Calling the indexer will fail because we can't convert an index to the correct type.
                return errorSuggestion ?? PSConvertBinder.ThrowNoConversion(target, typeof(int), this, _version, indexes);
            return IndexWithNegativeChecks(
                new DynamicMetaObject(target.Expression.Cast(target.LimitType), target.PSGetTypeRestriction()),
                new DynamicMetaObject(indexAsInt, indexes[0].PSGetTypeRestriction()),
                target.LimitType.GetProperty("Length"),
                static (t, i) => Expression.ArrayIndex(t, i).Cast(typeof(object)));
        private DynamicMetaObject GetIndexMultiDimensionArray(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
            // We have lots of possibilities here
            //   * single index - enumerable, all ints
            //        - single result, count must match array rank
            //   * single index - enumerable, all enumerable of ints
            //        - slicing, falls back to previous case
            //   * multiple indices - all ints
            //        - single result, must match array rank
            //   * multiple indices - enumerable of ints
            //        - slicing, falls back to first case
            // In script, the above cases look like:
            //     $x = [array]::CreateInstance([int], 3, 3)
            //     $y = 0,0
            //     $z = $y,$y
            //     $x[$y] # case 1
            //     $x[$z] # case 2
            //     $x[1,1] # case 3
            //     $x[(1,1),(0,0)] # case 4
            if (indexes.Length == 1)
                var enumerable = PSEnumerableBinder.IsEnumerable(indexes[0]);
                    return target.ThrowRuntimeError(indexes, BindingRestrictions.Empty, "NeedMultidimensionalIndex",
                                                    ParserStrings.NeedMultidimensionalIndex,
                                                    ExpressionCache.Constant(array.Rank),
                                                    DynamicExpression.Dynamic(PSToStringBinder.Get(), typeof(string),
                                                                              indexes[0].Expression, ExpressionCache.GetExecutionContextFromTLS));
                    Expression.Call(CachedReflectionInfo.ArrayOps_GetMDArrayValueOrSlice,
                                    Expression.Convert(target.Expression, typeof(Array)),
                                    indexes[0].Expression.Cast(typeof(object))),
                    target.CombineRestrictions(indexes));
            var intIndexes = indexes.Select(static index => ConvertIndex(index, typeof(int))).Where(static i => i != null).ToArray();
            if (intIndexes.Length != indexes.Length)
                if (!_allowSlicing)
                    return errorSuggestion ?? CannotIndexTarget(target, indexes);
                return InvokeSlicingIndexer(target, indexes);
                Expression.Call(CachedReflectionInfo.ArrayOps_GetMDArrayValue,
                                Expression.NewArrayInit(typeof(int), intIndexes),
                                ExpressionCache.Constant(!_allowSlicing)),
        private DynamicMetaObject InvokeIndexer(DynamicMetaObject target,
                                                PropertyInfo lengthProperty)
            MethodInfo getter = PSInvokeMemberBinder.FindBestMethod(target, indexes, "get_" + methodName, false, _constraints);
                return CheckForSlicing(target, indexes) ?? errorSuggestion ?? CannotIndexTarget(target, indexes);
            var getterParams = getter.GetParameters();
            if (getterParams.Length != indexes.Length)
                if (getterParams.Length == 1 && _allowSlicing)
                    // We have a slicing operation.
                // Calling the indexer will fail because there are either too many or too few indices.
            if (getterParams.Length == 1)
                // The getter takes a single argument, so first check if we're slicing.
            if (getter.ReturnType.IsByRefLike)
                // We cannot return a ByRef-like value in PowerShell, so we disallow getting such an indexer.
                            Compiler.IsStrictMode(3),
                            Compiler.ThrowRuntimeError(
                                nameof(ParserStrings.CannotIndexWithByRefLikeReturnType),
                                ParserStrings.CannotIndexWithByRefLikeReturnType,
                                Expression.Constant(target.LimitType, typeof(Type)),
                                Expression.Constant(getter.ReturnType, typeof(Type)))),
                        GetNullResult()),
            Expression[] indexExprs = new Expression[getterParams.Length];
            for (int i = 0; i < getterParams.Length; ++i)
                var parameterType = getterParams[i].ParameterType;
                indexExprs[i] = parameterType.IsByRefLike
                    ? PSConvertBinder.ConvertToByRefLikeTypeViaCasting(indexes[i], parameterType)
                    : ConvertIndex(indexes[i], parameterType);
                if (indexExprs[i] == null)
                    return errorSuggestion ?? PSConvertBinder.ThrowNoConversion(target, parameterType, this, _version, indexes);
            if (CanIndexFromEndWithNegativeIndex(target, getter, getterParams))
                if (lengthProperty == null)
                    // Count is declared by most supported types, Length will catch some edge cases like strings.
                    lengthProperty = target.LimitType.GetProperty("Count") ??
                                     target.LimitType.GetProperty("Length");
                if (lengthProperty != null)
                        new DynamicMetaObject(target.Expression.Cast(target.LimitType),
                                              target.PSGetTypeRestriction()),
                        new DynamicMetaObject(indexExprs[0], indexes[0].PSGetTypeRestriction()),
                        lengthProperty,
                        (t, i) => Expression.Call(t, getter, i).Cast(typeof(object)));
            // An indexer may do conversion to an unsafe type, so we need version checks
                SafeIndexResult(Expression.Call(target.Expression.Cast(getter.DeclaringType), getter, indexExprs)),
        internal static Expression ConvertIndex(DynamicMetaObject index, Type resultType)
            // ConstrainedLanguage note - Calls to this conversion are protected by the binding rules that call it.
            var conversion = LanguagePrimitives.FigureConversion(index.Value, resultType, out bool debase);
            return conversion.Rank == ConversionRank.None
                       : PSConvertBinder.InvokeConverter(conversion, index.Expression, resultType, debase, ExpressionCache.InvariantCulture);
        private DynamicMetaObject CheckForSlicing(DynamicMetaObject target, DynamicMetaObject[] indexes)
                var nonSlicingBinder = PSGetIndexBinder.Get(1, _constraints, allowSlicing: false);
                var expr = Expression.NewArrayInit(typeof(object),
                    indexes.Select(i => DynamicExpression.Dynamic(nonSlicingBinder, typeof(object), target.Expression, i.Expression)));
                return new DynamicMetaObject(expr, target.CombineRestrictions(indexes));
            var enumerableIndex = PSEnumerableBinder.IsEnumerable(indexes[0]);
            if (enumerableIndex != null)
                    Expression.Call(CachedReflectionInfo.EnumerableOps_SlicingIndex,
                                    enumerableIndex.Expression.Cast(typeof(IEnumerator)),
                                    Expression.Constant(GetNonSlicingIndexer())),
                    target.CombineRestrictions(enumerableIndex));
        private DynamicMetaObject InvokeSlicingIndexer(DynamicMetaObject target, DynamicMetaObject[] indexes)
            Diagnostics.Assert(_allowSlicing, "Slicing is not recursive");
                Expression.Call(CachedReflectionInfo.ArrayOps_SlicingIndex,
                                                        indexes.Select(static dmo => dmo.Expression.Cast(typeof(object)))),
        private Expression SafeIndexResult(Expression expr)
            return Expression.TryCatch(
                        Expression.IfThen(Compiler.IsStrictMode(3), Expression.Rethrow()),
                        GetNullResult())));
        private Expression GetNullResult()
            return _allowSlicing ? ExpressionCache.NullConstant : ExpressionCache.AutomationNullConstant;
        private Func<object, object, object> GetNonSlicingIndexer()
            // Rather than cache a single delegate, we create one for each generated rule under the assumption
            // that, although the generated rule may be used in multiple sites, it's better to have
            // multiple delegates (and hence, multiple nested sites) rather than a single nested site for
            // all non-slicing indexing.
            var targetParamExpr = Expression.Parameter(typeof(object));
            var indexParamExpr = Expression.Parameter(typeof(object));
            return Expression.Lambda<Func<object, object, object>>(
                DynamicExpression.Dynamic(PSGetIndexBinder.Get(1, _constraints, allowSlicing: false), typeof(object), targetParamExpr,
                                          indexParamExpr),
                targetParamExpr, indexParamExpr).Compile();
    /// The binder for setting the value of an indexable element, like $x[1] = 5.
    internal sealed class PSSetIndexBinder : SetIndexBinder
        private static readonly Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints>, PSSetIndexBinder> s_binderCache
                = new Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints>, PSSetIndexBinder>();
        public static PSSetIndexBinder Get(int argCount, PSMethodInvocationConstraints constraints = null)
                PSSetIndexBinder binder;
                var tuple = Tuple.Create(new CallInfo(argCount), constraints);
                    binder = new PSSetIndexBinder(tuple);
        private PSSetIndexBinder(Tuple<CallInfo, PSMethodInvocationConstraints> tuple)
                "PSSetIndexBinder indexCnt={0}{1} ver:{2}",
                CallInfo.ArgumentCount,
                foreach (PSSetIndexBinder binder in s_binderCache.Values)
        public override DynamicMetaObject FallbackSetIndex(
            DynamicMetaObject value,
            if (!target.HasValue || indexes.Any(static mo => !mo.HasValue) || !value.HasValue)
                return Defer(indexes.Prepend(target).Append(value).ToArray()).WriteToDebugLog(this);
            if (target.Value is PSObject && (PSObject.Base(target.Value) != target.Value) ||
                return this.DeferForPSObject(indexes.Prepend(target).Append(value).ToArray()).WriteToDebugLog(this);
            DynamicMetaObject result;
            if (ComInterop.ComBinder.TryBindSetIndex(this, target, indexes, value, out result))
                return result.UpdateComRestrictionsForPsObject(indexes).WriteToDebugLog(this);
            if (indexes.Length == 1 && indexes[0].Value == null)
                        target.ThrowRuntimeError(indexes, BindingRestrictions.Empty, "NullArrayIndex", ParserStrings.NullArrayIndex).WriteToDebugLog(this));
                return SetIndexArray(target, indexes, value, errorSuggestion).WriteToDebugLog(this);
                return (InvokeIndexer(target, indexes, value, errorSuggestion, defaultMember.MemberName)).WriteToDebugLog(this);
            return errorSuggestion ?? CannotIndexTarget(target, indexes, value).WriteToDebugLog(this);
        private DynamicMetaObject CannotIndexTarget(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value)
            BindingRestrictions bindingRestrictions = value.PSGetTypeRestriction();
            return target.ThrowRuntimeError(indexes, bindingRestrictions, "CannotIndex", ParserStrings.CannotIndex, Expression.Constant(target.LimitType, typeof(Type)));
        private DynamicMetaObject InvokeIndexer(
            MethodInfo setter = PSInvokeMemberBinder.FindBestMethod(target, indexes.Append(value), "set_" + methodName, false, _constraints);
                return errorSuggestion ?? CannotIndexTarget(target, indexes, value);
            var setterParams = setter.GetParameters();
            int paramLength = setterParams.Length;
            if (paramLength != indexes.Length + 1)
            if (setterParams[paramLength - 1].ParameterType.IsByRefLike)
                // In theory, it's possible to call the setter with a value that can be implicitly/explicitly casted to the target ByRef-like type.
                // However, the set-property/set-indexer semantics in PowerShell requires returning the value after the setting operation. We cannot
                // return a ByRef-like value back, so we just disallow setting an indexer that takes a ByRef-like type value.
                        Expression.Constant(setterParams[paramLength - 1].ParameterType, typeof(Type))),
            Expression[] indexExprs = new Expression[paramLength];
            for (int i = 0; i < paramLength; ++i)
                var parameterType = setterParams[i].ParameterType;
                var argument = (i == paramLength - 1) ? value : indexes[i];
                    ? PSConvertBinder.ConvertToByRefLikeTypeViaCasting(argument, parameterType)
                    : PSGetIndexBinder.ConvertIndex(argument, parameterType);
                    return errorSuggestion ?? PSConvertBinder.ThrowNoConversion(target, parameterType, this, _version, indexes.Append(value).ToArray());
            if (paramLength == 2
                && setterParams[0].ParameterType == typeof(int)
                && target.Value is not IDictionary)
                // PowerShell supports negative indexing for some types (specifically, those with a single
                // int parameter to the indexer, and also have either a Length or Count property.)  For
                // those types, generate special code to check for negative indices, otherwise just generate
                // the call.
                PropertyInfo lengthProperty = target.LimitType.GetProperty("Length") ??
                                              target.LimitType.GetProperty("Count");
                        new DynamicMetaObject(indexExprs[1], value.PSGetTypeRestriction()),
                        (t, i, v) => Expression.Call(t, setter, i, v));
            BindingRestrictions bindingRestrictions = target.CombineRestrictions(indexes).Merge(value.PSGetTypeRestriction());
            // Add the version checks and (potentially) language mode checks, as this setter
            // may invoke a conversion to an unsafe type.
            // We'll store the value in a temp so we can return it.  We'll also replace the expr in our array of arguments
            // to the indexer with the temp so any conversions are executed just once.
            var valExpr = indexExprs[indexExprs.Length - 1];
            var valTmp = Expression.Parameter(valExpr.Type, "value");
            indexExprs[indexExprs.Length - 1] = valTmp;
                    new ParameterExpression[] { valTmp },
                    Expression.Assign(valTmp, valExpr),
                    Expression.Call(target.Expression.Cast(setter.DeclaringType), setter, indexExprs),
                    valTmp.Cast(typeof(object))),
            Func<Expression, Expression, Expression, Expression> generateIndexOperation)
            BindingRestrictions bindingRestrictions = target.CombineRestrictions(index).Merge(value.Restrictions);
            // If the target is of an unsafe type for ConstrainedLanguage, we need to pay
            // attention to version and language mode. Otherwise, strongly-typed arrays of unsafe types
            // can be used for type conversion.
            bindingRestrictions = bindingRestrictions.Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, target.LimitType, _version));
            //    len = obj.Length
            //    if (index < 0)
            //        index = index + len
            //    obj[index] = value
            var valueExpr = value.Expression;
            var valTmp = Expression.Parameter(valueExpr.Type, "value");
                    new ParameterExpression[] { targetTmp, valTmp, lenTmp, indexTmp },
                    // Save the value because it too is used multiple times, but only to keep the DLR happy
                    Expression.Assign(valTmp, valueExpr),
                    // Do the indexing
                    generateIndexOperation(targetTmp, indexTmp, valTmp),
                    // Make sure the result of this operation is the value.  PowerShell won't use this value
                    // in any way, but the DLR requires it (and in theory, if PSObject uses this binder, other
                    // languages could use this value.)
        private DynamicMetaObject SetIndexArray(DynamicMetaObject target,
                return SetIndexMultiDimensionArray(target, indexes, value, errorSuggestion);
                return errorSuggestion ??
                       target.ThrowRuntimeError(indexes, value.PSGetTypeRestriction(), "ArraySliceAssignmentFailed",
                                                ParserStrings.ArraySliceAssignmentFailed,
                                                Expression.Call(CachedReflectionInfo.ArrayOps_IndexStringMessage,
                                                                                        indexes.Select(static i => i.Expression.Cast(typeof(object))))));
            var intIndex = PSGetIndexBinder.ConvertIndex(indexes[0], typeof(int));
            if (intIndex == null)
                       PSConvertBinder.ThrowNoConversion(indexes[0], typeof(int), this, _version, target, value);
            var valueExpr = PSGetIndexBinder.ConvertIndex(value, elementType);
            if (valueExpr == null)
                       PSConvertBinder.ThrowNoConversion(value, elementType, this, _version, indexes.Prepend(target).ToArray());
                new DynamicMetaObject(intIndex, indexes[0].PSGetTypeRestriction()),
                new DynamicMetaObject(valueExpr, value.PSGetTypeRestriction()), target.LimitType.GetProperty("Length"),
                static (t, i, v) => Expression.Assign(Expression.ArrayAccess(t, i), v));
        private DynamicMetaObject SetIndexMultiDimensionArray(DynamicMetaObject target,
                var indexExpr = PSGetIndexBinder.ConvertIndex(indexes[0], typeof(int[]));
                           PSConvertBinder.ThrowNoConversion(indexes[0], typeof(int[]), this, _version, new DynamicMetaObject[] { target, value });
                    Expression.Call(CachedReflectionInfo.ArrayOps_SetMDArrayValue,
                                    target.Expression.Cast(typeof(Array)),
                                    valueExpr.Cast(typeof(object))),
                    target.CombineRestrictions(indexes).Merge(value.PSGetTypeRestriction()));
            if (indexes.Length != array.Rank)
                       target.ThrowRuntimeError(indexes, value.PSGetTypeRestriction(), "NeedMultidimensionalIndex",
            var indexExprs = new Expression[indexes.Length];
            for (int i = 0; i < indexes.Length; i++)
                indexExprs[i] = PSGetIndexBinder.ConvertIndex(indexes[i], typeof(int));
                    return PSConvertBinder.ThrowNoConversion(indexes[i], typeof(int), this, _version,
                        indexes.Except(new DynamicMetaObject[] { indexes[i] }).Append(target).Append(value).ToArray());
                                Expression.NewArrayInit(typeof(int), indexExprs),
    /// The binder for getting a member of a class, like $foo.bar or [foo]::bar.
    internal class PSGetMemberBinder : GetMemberBinder
        private sealed class KeyComparer : IEqualityComparer<PSGetMemberBinderKeyType>
            public bool Equals(PSGetMemberBinderKeyType x, PSGetMemberBinderKeyType y)
                // The non-static binder cache is case-sensitive because sites need the name used per site
                // when the target object is a case-sensitive IDictionary.  Under all other circumstances,
                // binding is case-insensitive.
                var stringComparison = x.Item3 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return x.Item1.Equals(y.Item1, stringComparison) &&
                       x.Item2 == y.Item2 &&
                       x.Item4 == y.Item4;
            public int GetHashCode(PSGetMemberBinderKeyType obj)
                var stringComparer = obj.Item3 ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                return Utils.CombineHashCodes(stringComparer.GetHashCode(obj.Item1),
                    obj.Item2 == null ? 0 : obj.Item2.GetHashCode(),
                    obj.Item3.GetHashCode(),
                    obj.Item4.GetHashCode());
        private sealed class ReservedMemberBinder : PSGetMemberBinder
            internal ReservedMemberBinder(string name, bool ignoreCase, bool @static) : base(name, null, ignoreCase, @static, nonEnumerating: false)
                switch (Name)
                        mi = CachedReflectionInfo.ReservedNameMembers_GeneratePSAdaptedMemberSet;
                        targetExpr = target.Expression.Cast(typeof(object));
                        mi = CachedReflectionInfo.ReservedNameMembers_GeneratePSBaseMemberSet;
                        mi = CachedReflectionInfo.ReservedNameMembers_GeneratePSExtendedMemberSet;
                        mi = CachedReflectionInfo.ReservedNameMembers_GeneratePSObjectMemberSet;
                        mi = CachedReflectionInfo.ReservedNameMembers_PSTypeNames;
                        targetExpr = target.Expression.Convert(typeof(PSObject));
                Diagnostics.Assert(mi != null, "ReservedMemberBinder doesn't support member Name");
                return new DynamicMetaObject(WrapGetMemberInTry(Expression.Call(mi, targetExpr)), target.PSGetTypeRestriction());
        private static readonly Dictionary<PSGetMemberBinderKeyType, PSGetMemberBinder> s_binderCache
            = new Dictionary<PSGetMemberBinderKeyType, PSGetMemberBinder>(new KeyComparer());
        // Because the non-static binder is case-sensitive, we need a list of all binders for a given
        // name when we discover an instance member or type table member for that given name so we
        // can update each of those binders.
        private static readonly ConcurrentDictionary<string, List<PSGetMemberBinder>> s_binderCacheIgnoringCase
            = new ConcurrentDictionary<string, List<PSGetMemberBinder>>(StringComparer.OrdinalIgnoreCase);
        static PSGetMemberBinder()
            s_binderCache.Add(Tuple.Create(PSObject.AdaptedMemberSetName, (Type)null, false, false),
                new ReservedMemberBinder(PSObject.AdaptedMemberSetName, ignoreCase: true, @static: false));
            s_binderCache.Add(Tuple.Create(PSObject.ExtendedMemberSetName, (Type)null, false, false),
                new ReservedMemberBinder(PSObject.ExtendedMemberSetName, ignoreCase: true, @static: false));
            s_binderCache.Add(Tuple.Create(PSObject.BaseObjectMemberSetName, (Type)null, false, false),
                new ReservedMemberBinder(PSObject.BaseObjectMemberSetName, ignoreCase: true, @static: false));
            s_binderCache.Add(Tuple.Create(PSObject.PSObjectMemberSetName, (Type)null, false, false),
                new ReservedMemberBinder(PSObject.PSObjectMemberSetName, ignoreCase: true, @static: false));
            s_binderCache.Add(Tuple.Create(PSObject.PSTypeNames, (Type)null, false, false),
                new ReservedMemberBinder(PSObject.PSTypeNames, ignoreCase: true, @static: false));
        private readonly bool _nonEnumerating;
        private bool _hasInstanceMember;
        internal bool HasInstanceMember { get { return _hasInstanceMember; } }
        internal static void SetHasInstanceMember(string memberName)
            // We must invalidate dynamic sites (if any) when the first instance member (for this binder)
            // is created, but we don't need to invalidate any sites after the first instance member.
            // Before any instance members exist, restrictions might look like:
            //     if (binderVersion == oldBinderVersion && obj is string) { ... }
            // After an instance member is known to exist, the above test (for an object that has no instance
            // member) will look like:
            //    MemberInfo mi;
            //    if (binderVersion == oldBinderVersion && !TryGetInstanceMember(obj, memberName, out mi) && obj is string)
            //        return ((string)obj).memberName;
            //    else { update site }
            // And if there is an instance member, the generic rule will look like:
            //    if (binderVersion == oldBinderVersion && TryGetInstanceMember(obj, memberName, out mi))
            //        return mi.Value;
            // This way, we can avoid the call to TryGetInstanceMember for binders when we know there aren't any instance
            // members, yet invalidate those rules once somebody adds an instance member.
            var binderList = s_binderCacheIgnoringCase.GetOrAdd(memberName, static _ => new List<PSGetMemberBinder>());
            lock (binderList)
                if (binderList.Count == 0)
                    // Force one binder to be created if one hasn't been created already.
                    PSGetMemberBinder.Get(memberName, (Type)null, @static: false);
                foreach (var binder in binderList)
                    if (!binder._hasInstanceMember)
                        lock (binder)
                                binder._hasInstanceMember = true;
        private bool _hasTypeTableMember;
        internal static void TypeTableMemberAdded(string memberName)
                        binder._hasTypeTableMember = true;
        internal static void TypeTableMemberPossiblyUpdated(string memberName)
                    Interlocked.Increment(ref binder._version);
        public static PSGetMemberBinder Get(string memberName, TypeDefinitionAst classScope, bool @static)
            return Get(memberName, classScope?.Type, @static, false);
        public static PSGetMemberBinder Get(string memberName, Type classScope, bool @static)
            return Get(memberName, classScope, @static, false);
        private PSGetMemberBinder GetNonEnumeratingBinder()
            return Get(this.Name, _classScope, @static: false, nonEnumerating: true);
        private static PSGetMemberBinder Get(string memberName, Type classScope, bool @static, bool nonEnumerating)
            PSGetMemberBinder result;
                var tuple = Tuple.Create(memberName, classScope, @static, nonEnumerating);
                if (!s_binderCache.TryGetValue(tuple, out result))
                    // We might be seeing a reserved name with a different case.  Check for that before
                    // creating a new binder.  For reserved names, we can safely use a single binder for
                    // any case.
                    if (PSMemberInfoCollection<PSMemberInfo>.IsReservedName(memberName))
                        var tupleLower = Tuple.Create(memberName.ToLowerInvariant(), (Type)null, @static, nonEnumerating);
                        result = s_binderCache[tupleLower];
                        result = new PSGetMemberBinder(memberName, classScope, true, @static, nonEnumerating);
                        if (!@static)
                                if (binderList.Count > 0)
                                    result._hasInstanceMember = binderList[0]._hasInstanceMember;
                                    result._hasTypeTableMember = binderList[0]._hasTypeTableMember;
                                binderList.Add(result);
                                Diagnostics.Assert(binderList.All(b => b._hasInstanceMember == result._hasInstanceMember),
                                                   "All binders in the list should have _hasInstanceMember set identically");
                                Diagnostics.Assert(binderList.All(b => b._hasTypeTableMember == result._hasTypeTableMember),
                                                   "All binders in the list should have _hasTypeTableMember set identically");
                    s_binderCache.Add(tuple, result);
        private PSGetMemberBinder(string name, Type classScope, bool ignoreCase, bool @static, bool nonEnumerating)
            : base(name, ignoreCase)
            _nonEnumerating = nonEnumerating;
                "GetMember: {0}{1}{2} ver:{3}",
                _static ? " static" : string.Empty,
                _nonEnumerating ? " nonEnumerating" : string.Empty,
            // Defer COM objects or arguments wrapped in PSObjects
                object baseObject = PSObject.Base(target.Value);
                if (baseObject != null && Marshal.IsComObject(baseObject))
                    // We unwrap only if the 'base' is a COM object. It's unnecessary to unwrap in other cases,
                    // especially in the case of strings, we would lose instance members on the PSObject.
                    // Therefore, we need to use a stricter restriction to make sure PSObject 'target' with other
                    // base types doesn't get unwrapped.
                    return this.DeferForPSObject(target, targetIsComObject: true).WriteToDebugLog(this);
            if (ComInterop.ComBinder.TryBindGetMember(this, target, out result, delayInvocation: false))
                result = new DynamicMetaObject(WrapGetMemberInTry(result.Expression), result.Restrictions);
                // PSGetTypeRestriction will actually create an instance restriction because the targetValue is null.
                return PropertyDoesntExist(target, target.PSGetTypeRestriction()).WriteToDebugLog(this);
            PSMemberInfo memberInfo;
            if (_hasInstanceMember && TryGetInstanceMember(target.Value, Name, out memberInfo))
                // If there is an instance member, we generate (roughly) the following:
                //     PSMemberInfo memberInfo;
                //     if (PSGetMemberBinder.TryGetInstanceMember(target.Value, Name, out memberInfo))
                //         return memberInfo.Value;
                //     else
                //         update the site
                // We use a generic method like this because:
                //   * If one object has an instance property with a given name, it's like many others do as well
                //   * We want to avoid generating new sites for every object with an instance member
                // As an alternative, would could generate the following psuedo-code:
                //     if (target.Value == previousInstance)
                //         return optimized value (depending on the exact PSMemberInfo subclass)
                //     else update the site
                // But the assumption here is that many sites probably performs worse than the dictionary lookup
                // and unoptimized virtual call to PSMemberInfo.Value.
                // The binding restrictions could avoid a version check because it's never wrong to look for an instance member,
                // but we add the check because the DLR requires a non-empty check when the target implements IDynamicMetaObjectProvider,
                // which PSObject does.  The version check is also marginally useful if we knew we'd never see another
                // instance member with this member name, but we're not tracking things to make that a useful test.
                var memberInfoVar = Expression.Variable(typeof(PSMemberInfo));
                expr = Expression.Condition(
                    Expression.Call(CachedReflectionInfo.PSGetMemberBinder_TryGetInstanceMember, target.Expression.Cast(typeof(object)), Expression.Constant(Name), memberInfoVar),
                    Expression.Property(memberInfoVar, "Value"),
                    this.GetUpdateExpression(typeof(object)));
                expr = WrapGetMemberInTry(expr);
                return (new DynamicMetaObject(Expression.Block(new[] { memberInfoVar }, expr), BinderUtils.GetVersionCheck(this, _version))).WriteToDebugLog(this);
            bool canOptimize;
            Type aliasConversionType;
            memberInfo = GetPSMemberInfo(target, out restrictions, out canOptimize, out aliasConversionType, MemberTypes.Property);
            if (!canOptimize)
                Diagnostics.Assert(memberInfo == null, "We don't bother returning members if we can't optimize.");
                    WrapGetMemberInTry(Expression.Call(CachedReflectionInfo.PSGetMemberBinder_GetAdaptedValue,
                                                       GetTargetExpr(target, typeof(object)),
                                                       Expression.Constant(Name))),
                Diagnostics.Assert(memberInfo.instance == null, "We shouldn't be here if a member is already bound.");
                // The most common case - we're getting some property.  We can optimize many different kinds
                // of property accessors, so we special case each possibility.
                var propertyInfo = memberInfo as PSPropertyInfo;
                        return GenerateGetPropertyException(restrictions).WriteToDebugLog(this);
                    var property = propertyInfo as PSProperty;
                        var adapterData = property.adapterData as DotNetAdapter.PropertyCacheEntry;
                        Diagnostics.Assert(adapterData != null, "We have an unknown PSProperty that we aren't correctly optimizing.");
                        if (adapterData.member.DeclaringType.IsGenericTypeDefinition || adapterData.propertyType.IsByRefLike)
                            // We really should throw an error, but accessing property getter
                            // doesn't throw error in PowerShell since V2, even in strict mode.
                            // For static property access, the target expr must be null.  For non-static, we must convert
                            // because target.Expression is typeof(object) because this is a dynamic site.
                            var targetExpr = _static ? null : GetTargetExpr(target, adapterData.member.DeclaringType);
                            var propertyAccessor = adapterData.member as PropertyInfo;
                            if (propertyAccessor != null)
                                var propertyGetter = propertyAccessor.GetMethod;
                                if ((propertyGetter.IsFamily || propertyGetter.IsFamilyOrAssembly) &&
                                    (_classScope == null || !_classScope.IsSubclassOf(propertyAccessor.DeclaringType)))
                                if (propertyAccessor.PropertyType.IsByRef)
                                        CachedReflectionInfo.ByRefOps_GetByRefPropertyValue,
                                        Expression.Constant(propertyAccessor));
                                    expr = Expression.Property(targetExpr, propertyAccessor);
                                Diagnostics.Assert(adapterData.member is FieldInfo,
                                                   "A DotNetAdapter.PropertyCacheEntry has something other than PropertyInfo or FieldInfo.");
                                expr = Expression.Field(targetExpr, (FieldInfo)adapterData.member);
                    var scriptProperty = propertyInfo as PSScriptProperty;
                        expr = Expression.Call(Expression.Constant(scriptProperty, typeof(PSScriptProperty)),
                                               CachedReflectionInfo.PSScriptProperty_InvokeGetter, target.Expression.Cast(typeof(object)));
                    var codeProperty = propertyInfo as PSCodeProperty;
                        Diagnostics.Assert(codeProperty.GetterCodeReference != null, "CodeProperty isn't gettable, should have generated error code above.");
                        Diagnostics.Assert(codeProperty.GetterCodeReference.IsStatic, "CodeProperty should be a static method.");
                        expr = PSInvokeMemberBinder.InvokeMethod(codeProperty.GetterCodeReference, null, new[] { target },
                            false, PSInvokeMemberBinder.MethodInvocationType.Getter);
                    var noteProperty = propertyInfo as PSNoteProperty;
                    if (noteProperty != null)
                        Diagnostics.Assert(!noteProperty.IsSettable, "If the note is settable, incorrect code is generated.");
                        expr = Expression.Property(Expression.Constant(propertyInfo, typeof(PSNoteProperty)), CachedReflectionInfo.PSNoteProperty_Value);
                    Diagnostics.Assert(expr != null, "Unexpected property type encountered");
                    if (aliasConversionType != null)
                        expr = expr.Convert(aliasConversionType);
                    expr = Expression.Call(CachedReflectionInfo.PSGetMemberBinder_CloneMemberInfo,
                                           Expression.Constant(memberInfo, typeof(PSMemberInfo)),
                                           target.Expression.Cast(typeof(object)));
            if (targetValue is IDictionary)
                Type genericTypeArg = null;
                bool isGeneric = IsGenericDictionary(targetValue, ref genericTypeArg);
                if (!isGeneric || genericTypeArg != null)
                    var temp = Expression.Variable(typeof(object));
                    // If expr is not null, it's the fallback when no member exists.  If it is null,
                    // the fallback is the result from PropertyDoesntExist.
                    expr ??= (errorSuggestion ?? PropertyDoesntExist(target, restrictions)).Expression;
                    var method = isGeneric
                        ? CachedReflectionInfo.PSGetMemberBinder_TryGetGenericDictionaryValue.MakeGenericMethod(genericTypeArg)
                        : CachedReflectionInfo.PSGetMemberBinder_TryGetIDictionaryValue;
                            Expression.Call(method, GetTargetExpr(target, method.GetParameters()[0].ParameterType), Expression.Constant(Name), temp),
                            temp,
                            expr.Cast(typeof(object))));
            return expr != null
                ? new DynamicMetaObject(WrapGetMemberInTry(expr), restrictions).WriteToDebugLog(this)
                : (errorSuggestion ?? PropertyDoesntExist(target, restrictions)).WriteToDebugLog(this);
        private DynamicMetaObject GenerateGetPropertyException(BindingRestrictions restrictions)
                Compiler.ThrowRuntimeError("WriteOnlyProperty", ExtendedTypeSystem.WriteOnlyProperty,
                    this.ReturnType, Expression.Constant(Name)),
                restrictions);
        internal static bool IsGenericDictionary(object value, ref Type genericTypeArg)
            bool isGeneric = false;
            foreach (var i in value.GetType().GetInterfaces())
                    isGeneric = true;
                    var genericArguments = i.GetGenericArguments();
                    if (genericArguments[0] == typeof(string))
                        // Our generic method for lookup takes IDictionary<string,T>, we need
                        // to remember T.
                        genericTypeArg = genericArguments[1];
            return isGeneric;
        /// Get the actual value, as an expression, of the object represented by target.  This
        /// will get the base object if it's a psobject, plus correctly handle Nullable.
        internal static Expression GetTargetExpr(DynamicMetaObject target, Type castToType = null)
            // If the target value is actually a deserialized PSObject, we should use the original value
            if (psobj != null && psobj != AutomationNull.Value && !psobj.IsDeserialized)
                expr = Expression.Call(CachedReflectionInfo.PSObject_Base, expr);
            var type = castToType ?? ((value != null) ? value.GetType() : typeof(object));
            if (expr.Type != type)
                // Unbox value types (or use Nullable<T>.Value) to avoid a copy in case the value is mutated.
                // In case that castToType is System.Object and expr.Type is Nullable<ValueType>, expr.Cast(System.Object) will
                // get the underlying value by default. So "GetTargetExpr(target).Cast(typeof(object))" is actually the same as
                // "GetTargetExpr(target, typeof(object))".
                expr = type.IsValueType
                           ? (Nullable.GetUnderlyingType(expr.Type) != null
                                  ? (Expression)Expression.Property(expr, "Value")
                                  : Expression.Unbox(expr, type))
                           : expr.Cast(type);
        /// Return the binding result when no property exists.
        private DynamicMetaObject PropertyDoesntExist(DynamicMetaObject target, BindingRestrictions restrictions)
            // If the property does not exist, but the target is enumerable, we'll turn this expression into roughly the equivalent
            // pipeline:
            //     $x | foreach-object { $_.Property }
            // I say roughly because we'll actually iterate through $_ if it doesn't have Property and it is enumerable, and we'll
            // do this recursively.  This makes it easy to chain property references and not worry if the property returns collections or not, e.g.:
            //     $x.Modules.ModuleName
            // If Modules returns a collection, but you want all the module names of all the modules, then it just works.
            // The _nonEnumerating aspect of this binder is simply a way of avoiding the recursing inside the binder, allowing us to
            // collect the results in the helper method we call.  One alternative to _nonEnumerating is to have the helper method
            // not recurse, but mark it's return value specially so that recursive calls to the helper can detect that the results
            // need to be flattened.
            // IsEnumerable treats AutomationNull.Value as a zero length array which we don't want to do here.
            if (!_nonEnumerating && target.Value != AutomationNull.Value)
                        Expression.Call(CachedReflectionInfo.EnumerableOps_PropertyGetter,
                                        Expression.Constant(this.GetNonEnumeratingBinder()),
                                        enumerable.Expression), restrictions);
            // As part of our effort to hide how a command can return a singleton or array, we want to allow people to iterate
            // over singletons with the foreach statement (which has worked since V1) and a for loop, for example:
            //     for ($i = 0; $i -lt $x.Length; $i++) { $x[$i] }
            // If $x is a singleton, we want to return 1 for the length so code like this works correctly.
            // We do not want this magic to show up in Get-Member output, tab completion, intellisense, etc.
            if (Name.Equals("Length", StringComparison.OrdinalIgnoreCase) || Name.Equals("Count", StringComparison.OrdinalIgnoreCase))
                // $null.Count should be 0, anything else should be 1
                var resultCount = PSObject.Base(target.Value) == null ? 0 : 1;
                        Compiler.IsStrictMode(2),
                        ThrowPropertyNotFoundStrict(),
                        ExpressionCache.Constant(resultCount).Cast(typeof(object))), restrictions);
            var result = Expression.Condition(
                _nonEnumerating ? ExpressionCache.AutomationNullConstant : ExpressionCache.NullConstant);
            return new DynamicMetaObject(result, restrictions);
        private Expression ThrowPropertyNotFoundStrict()
            return Compiler.CreateThrow(typeof(object), typeof(PropertyNotFoundException),
                                        "PropertyNotFoundStrict", null, ParserStrings.PropertyNotFoundStrict,
                                        new object[] { Name });
        internal static DynamicMetaObject EnsureAllowedInLanguageMode(DynamicMetaObject target, object targetValue,
            string name, bool isStatic, DynamicMetaObject[] args, BindingRestrictions moreTests, string errorID, string resourceString)
            if (context.LanguageMode == PSLanguageMode.ConstrainedLanguage &&
                !IsAllowedInConstrainedLanguage(targetValue, name, isStatic))
                    return target.ThrowRuntimeError(args, moreTests, errorID, resourceString);
                string targetName = (targetValue as Type)?.FullName ?? targetValue?.GetType().FullName;
                    title: ParameterBinderStrings.WDACBinderInvocationLogTitle,
                    message: StringUtil.Format(ParameterBinderStrings.WDACBinderInvocationLogMessage, name, targetName ?? string.Empty),
                    fqid: "MethodOrPropertyInvocationNotAllowed",
        internal static bool IsAllowedInConstrainedLanguage(object targetValue, string name, bool isStatic)
            // ToString allowed on any type
            if (string.Equals(name, "ToString", StringComparison.OrdinalIgnoreCase))
            // Otherwise, check if it's a core type
            Type targetType = targetValue as Type;
            if ((!isStatic) || (targetType == null))
                targetType = targetValue.GetType();
            return CoreTypes.Contains(targetType);
        /// Return the binding restriction that tests that an instance member does not exist, used when the binder
        /// knows instance members might exist (because the name was added to some instance), but the object we're
        /// currently binding does not have an instance member with the given member name.
        internal BindingRestrictions NotInstanceMember(DynamicMetaObject target)
            var expr = Expression.Call(CachedReflectionInfo.PSGetMemberBinder_TryGetInstanceMember,
                                       target.Expression.Cast(typeof(object)), Expression.Constant(Name), memberInfoVar);
            return BindingRestrictions.GetExpressionRestriction(Expression.Block(new[] { memberInfoVar }, Expression.Not(expr)));
        private static Expression WrapGetMemberInTry(Expression expr)
            // This code ensures that getting a member doesn't raise an exception.  Mostly this is so that formatting
            // always works.  As currently implemented, this will also affect C# code that uses the dynamic keyword.
            // If we decide that the dynamic keyword should not mask exceptions, then we should create a new binder
            // from PSObject.PSDynamicMetaObject.BindGetMember that passes in a flag so we know not to wrap in a try/catch.
                Expression.Catch(typeof(TerminateException), Expression.Rethrow(typeof(object))),
                // Not sure if the following catch is necessary, but the interpreter has it.
                Expression.Catch(typeof(MethodException), Expression.Rethrow(typeof(object))),
                // This catch is only needed if we have an IDictionary
                Expression.Catch(typeof(PropertyNotFoundException), Expression.Rethrow(typeof(object))),
                Expression.Catch(typeof(Exception), ExpressionCache.NullConstant));
        /// Resolve the alias, throwing an exception if a cycle is detected while resolving the alias.
        private PSMemberInfo ResolveAlias(PSAliasProperty alias, DynamicMetaObject target, HashSet<string> aliases,
            List<BindingRestrictions> aliasRestrictions)
            Diagnostics.Assert(aliasRestrictions != null, "aliasRestrictions cannot be null");
                aliases = new HashSet<string> { alias.Name };
                if (aliases.Contains(alias.Name))
                    throw new ExtendedTypeSystemException("CycleInAliasLookup", null, ExtendedTypeSystem.CycleInAlias, alias.Name);
                aliases.Add(alias.Name);
            PSGetMemberBinder binder = PSGetMemberBinder.Get(alias.ReferencedMemberName, _classScope, false);
            // if binder has instance member, then GetPSMemberInfo will not be able to resolve that..only FallbackGetMember
            // can resolve that. In that case we simply return without further evaluation.
            if (binder.HasInstanceMember)
            PSMemberInfo result = binder.GetPSMemberInfo(target, out restrictions, out canOptimize, out aliasConversionType,
                                                         MemberTypes.Property, aliases, aliasRestrictions);
        internal PSMemberInfo GetPSMemberInfo(DynamicMetaObject target,
                                              out BindingRestrictions restrictions,
                                              out bool canOptimize,
                                              out Type aliasConversionType,
                                              MemberTypes memberTypeToOperateOn,
                                              HashSet<string> aliases = null,
                                              List<BindingRestrictions> aliasRestrictions = null)
            aliasConversionType = null;
            bool hasTypeTableMember;
            bool hasInstanceMember;
            BindingRestrictions versionRestriction;
                versionRestriction = BinderUtils.GetVersionCheck(this, _version);
                hasTypeTableMember = _hasTypeTableMember;
                hasInstanceMember = _hasInstanceMember;
            if (_static)
                restrictions = target.PSGetStaticMemberRestriction();
                restrictions = restrictions.Merge(versionRestriction);
                canOptimize = true;
                return PSObject.GetStaticCLRMember(target.Value, Name);
            canOptimize = false;
            Diagnostics.Assert(!TryGetInstanceMember(target.Value, Name, out _),
                                "shouldn't get here if there is an instance member");
            PSMemberInfo memberInfo = null;
            ConsolidatedString typenames = null;
            var typeTable = context?.TypeTable;
            if (hasTypeTableMember)
                typenames = PSObject.GetTypeNames(target.Value);
                    memberInfo = typeTable.GetMembers<PSMemberInfo>(typenames)[Name];
            // Check if the target value is actually a deserialized PSObject.
            // - If so, we want to use the original value.
            //   Mostly, a deserialized object is a PSObject with an empty immediate base object, and it's OK to call PSObject.Base()
            //   on it in this case, because the method would just return the original PSObject. But if it's the deserialized object of
            //   a container object (i.e. an object derived from IEnumerable, IList, or IDictionary), the immediate base object is a
            //   Hashtable or ArrayList. In such case, we sometimes would lose the psadapted/psextended properties that we actually care
            //   by using the base object.
            //   One example is the XmlElement, which derives from IEnumerable. It is serialized/deserialized as a container object, and
            //   the its element properties (i.e. $xmlElement.IP, where IP is actually an attribute name) are stored as psadapted properties
            //   in the top-level PSObject.
            //   See the comments about 'three interesting cases' in PSInvokeMemberBinder.FallbackInvokeMember for more info.
            // - If not, we want to use the base object, so that we might generate optimized code.
            var psobj = target.Value as PSObject;
            bool isTargetDeserializedObject = (psobj != null) && (psobj.IsDeserialized);
            object value = isTargetDeserializedObject ? target.Value : PSObject.Base(target.Value);
            var adapterSet = PSObject.GetMappedAdapter(value, typeTable);
            if (memberInfo == null)
                canOptimize = adapterSet.OriginalAdapter.CanSiteBinderOptimize(memberTypeToOperateOn);
                // Don't bother looking for the member if we're not going to use it.
                if (canOptimize)
                    memberInfo = adapterSet.OriginalAdapter.BaseGetMember<PSMemberInfo>(value, Name);
            if (memberInfo == null && canOptimize && adapterSet.DotNetAdapter != null)
                memberInfo = adapterSet.DotNetAdapter.BaseGetMember<PSMemberInfo>(value, Name);
            // The member came from the type table or an adapter and isn't instance based, so the restriction will start
            // with a version check
            restrictions = versionRestriction;
            // When returning aliasRestrictions always include the version restriction
            aliasRestrictions?.Add(versionRestriction);
            var alias = memberInfo as PSAliasProperty;
                aliasConversionType = alias.ConversionType;
                aliasRestrictions ??= new List<BindingRestrictions>();
                memberInfo = ResolveAlias(alias, target, aliases, aliasRestrictions);
                    // this can happen in the cases where referenced name of the alias property
                    // maps to an adapter that cannot optimize (like ManagementObjectAdapter)
                // Merge alias restrictions
                foreach (var aliasRestriction in aliasRestrictions)
                    restrictions = restrictions.Merge(aliasRestriction);
            if (_classScope != null && (target.LimitType == _classScope || target.LimitType.IsSubclassOf(_classScope)) && adapterSet.OriginalAdapter == PSObject.DotNetInstanceAdapter)
                List<MethodBase> candidateMethods = null;
                foreach (var member in _classScope.GetMembers(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic))
                    if (this.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase))
                            var getMethod = propertyInfo.GetGetMethod(nonPublic: true);
                            var setMethod = propertyInfo.GetSetMethod(nonPublic: true);
                            if ((getMethod == null || getMethod.IsPublic || getMethod.IsFamily || getMethod.IsFamilyOrAssembly) &&
                                (setMethod == null || setMethod.IsPublic || setMethod.IsFamily || setMethod.IsFamilyOrAssembly))
                                memberInfo = new PSProperty(this.Name, PSObject.DotNetInstanceAdapter, target.Value, new DotNetAdapter.PropertyCacheEntry(propertyInfo));
                                if (fieldInfo.IsFamily || fieldInfo.IsFamilyOrAssembly)
                                    memberInfo = new PSProperty(this.Name, PSObject.DotNetInstanceAdapter, target.Value, new DotNetAdapter.PropertyCacheEntry(fieldInfo));
                                var methodInfo = member as MethodInfo;
                                if (methodInfo != null && (methodInfo.IsPublic || methodInfo.IsFamily || methodInfo.IsFamilyOrAssembly))
                                    candidateMethods ??= new List<MethodBase>();
                                    candidateMethods.Add(methodInfo);
                if (candidateMethods != null && candidateMethods.Count > 0)
                    var psMethodInfo = memberInfo as PSMethod;
                    if (psMethodInfo != null)
                        var cacheEntry = (DotNetAdapter.MethodCacheEntry)psMethodInfo.adapterData;
                        candidateMethods.AddRange(cacheEntry.methodInformationStructures.Select(static e => e.method));
                        memberInfo = null;
                        // Ambiguous, it'd be better to report an error other than "can't find member", but I'm lazy.
                        DotNetAdapter.MethodCacheEntry method = new DotNetAdapter.MethodCacheEntry(candidateMethods);
                        memberInfo = PSMethod.Create(this.Name, PSObject.DotNetInstanceAdapter, null, method);
            if (hasInstanceMember)
                // If this binder knows instance members exist, we need to make sure future objects going through this
                // rule ensure they don't have an instance member.  I don't expect this rule to be generated or hit frequently.
                restrictions = restrictions.Merge(NotInstanceMember(target));
            // We always need a type check, even if we'll be using the PSTypeNames because our generated code may contain
            // conversions that don't work for arbitrary types.
            restrictions = restrictions.Merge(target.PSGetTypeRestriction());
            // If the target value is actually a deserialized PSObject, add a check to ensure that's the case. This check
            // should be done after the type check.
            if (isTargetDeserializedObject)
                    Expression.Property(target.Expression.Cast(typeof(PSObject)), CachedReflectionInfo.PSObject_IsDeserialized)));
                // We need to make sure the type table we would use to find a member is the same type table that we used here to
                // find (or not find) a member.  If they were different type tables, we could easily get different results.
                    BindingRestrictions.GetInstanceRestriction(Expression.Call(CachedReflectionInfo.PSGetMemberBinder_GetTypeTableFromTLS), typeTable));
                // We also need to make sure the pstypename is the same.  It doesn't matter if we found something in the type table
                // or not - the fact that we might find something in the type table is enough to require a check on the pstypename.
                            Expression.Call(CachedReflectionInfo.PSGetMemberBinder_IsTypeNameSame, target.Expression.Cast(typeof(object)), Expression.Constant(typenames.Key))));
            return memberInfo;
        #region Runtime helper methods
        internal static PSMemberInfo CloneMemberInfo(PSMemberInfo memberInfo, object obj)
            memberInfo = memberInfo.Copy();
            memberInfo.ReplicateInstance(obj);
        internal static object GetAdaptedValue(object obj, string member)
            if ((context != null) && (context.TypeTable != null))
                ConsolidatedString typenames = PSObject.GetTypeNames(obj);
                memberInfo = context.TypeTable.GetMembers<PSMemberInfo>(typenames)[member];
                    memberInfo = CloneMemberInfo(memberInfo, obj);
            var adapterSet = PSObject.GetMappedAdapter(obj, context?.TypeTable);
            memberInfo ??= adapterSet.OriginalAdapter.BaseGetMember<PSMemberInfo>(obj, member);
            if (memberInfo == null && adapterSet.DotNetAdapter != null)
                memberInfo = adapterSet.DotNetAdapter.BaseGetMember<PSMemberInfo>(obj, member);
                return memberInfo.Value;
            if (string.Equals(member, "Length", StringComparison.OrdinalIgnoreCase) || string.Equals(member, "Count", StringComparison.OrdinalIgnoreCase))
            if (context != null && context.IsStrictVersion(2))
                                                    LanguagePrimitives.ConvertTo<string>(member));
        internal static bool IsTypeNameSame(object value, string typeName)
            return value != null && string.Equals(PSObject.GetTypeNames(value).Key, typeName, StringComparison.OrdinalIgnoreCase);
        internal static TypeTable GetTypeTableFromTLS()
            return executionContext?.TypeTable;
        internal static bool TryGetInstanceMember(object value, string memberName, out PSMemberInfo memberInfo)
            memberInfo = PSObject.HasInstanceMembers(value, out instanceMembers) ? instanceMembers[memberName] : null;
            return (memberInfo != null);
        internal static bool TryGetIDictionaryValue(IDictionary hash, string memberName, out object value)
                if (hash.Contains(memberName))
                    value = hash[memberName];
        internal static bool TryGetGenericDictionaryValue<T>(IDictionary<string, T> hash, string memberName, out object value)
            if (hash.TryGetValue(memberName, out result))
                value = result;
        #endregion Runtime helper methods
    /// The binder for setting a member, like $foo.bar = 1 or [foo]::bar = 1.
    internal class PSSetMemberBinder : SetMemberBinder
        private sealed class KeyComparer : IEqualityComparer<PSSetMemberBinderKeyType>
            public bool Equals(PSSetMemberBinderKeyType x, PSSetMemberBinderKeyType y)
                       x.Item3 == y.Item3;
            public int GetHashCode(PSSetMemberBinderKeyType obj)
                    obj.Item3.GetHashCode());
        private static readonly Dictionary<PSSetMemberBinderKeyType, PSSetMemberBinder> s_binderCache
            = new Dictionary<PSSetMemberBinderKeyType, PSSetMemberBinder>(new KeyComparer());
        private readonly PSGetMemberBinder _getMemberBinder;
        public static PSSetMemberBinder Get(string memberName, TypeDefinitionAst classScopeAst, bool @static)
            return Get(memberName, classScope, @static);
        public static PSSetMemberBinder Get(string memberName, Type classScope, bool @static)
            PSSetMemberBinder result;
                var tuple = Tuple.Create(memberName, classScope, @static);
                    result = new PSSetMemberBinder(memberName, true, @static, classScope);
        public PSSetMemberBinder(string name, bool ignoreCase, bool @static, Type classScope)
            _getMemberBinder = PSGetMemberBinder.Get(name, _classScope, @static);
                "SetMember: {0}{1} ver:{2}",
                _static ? "static " : string.Empty,
                _getMemberBinder._version);
        private static Expression GetTransformedExpression(IEnumerable<ArgumentTransformationAttribute> transformationAttributes, Expression originalExpression)
            if (transformationAttributes == null)
                return originalExpression;
            var attributesArray = transformationAttributes.ToArray();
            if (attributesArray.Length == 0)
            Expression transformedExpression = originalExpression.Convert(typeof(object));
            var engineIntrinsicsTempVar = Expression.Variable(typeof(EngineIntrinsics));
            // apply transformation attributes from right to left
            for (int i = attributesArray.Length - 1; i >= 0; i--)
                transformedExpression = Expression.Call(Expression.Constant(attributesArray[i]),
                                                CachedReflectionInfo.ArgumentTransformationAttribute_Transform,
                                                engineIntrinsicsTempVar,
                                                transformedExpression);
            return Expression.Block(new[] { engineIntrinsicsTempVar },
                    Expression.Property(ExpressionCache.GetExecutionContextFromTLS,
                                        CachedReflectionInfo.ExecutionContext_EngineIntrinsics)),
        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            if (!target.HasValue || !value.HasValue)
                return Defer(target, value);
                (value.Value is PSObject && (PSObject.Base(value.Value) != value.Value)))
                    // We unwrap only if the 'base' of 'target' is a COM object. It's unnecessary to unwrap in other cases,
                    // especially in the case that 'target' is a string, we would lose instance members on the PSObject.
                    // Therefore, we need to use a stricter restriction to make sure PSObject 'target' with other base types
                    // doesn't get unwrapped.
                    return this.DeferForPSObject(target, value, targetIsComObject: true).WriteToDebugLog(this);
            if (ComInterop.ComBinder.TryBindSetMember(this, target, value, out result))
                return result.UpdateComRestrictionsForPsObject(new DynamicMetaObject[] { value }).WriteToDebugLog(this);
                return (target.ThrowRuntimeError(new[] { value }, BindingRestrictions.Empty, "PropertyNotFound",
                                                 ParserStrings.PropertyNotFound,
                                                 Expression.Constant(Name))).WriteToDebugLog(this);
            if (value.Value == AutomationNull.Value)
                // Pretend the value was null (so we actually use null as the expression, but be sure
                // to use a restriction that checks
                value = new DynamicMetaObject(ExpressionCache.NullConstant, value.PSGetTypeRestriction(), null);
            if (_getMemberBinder.HasInstanceMember && PSGetMemberBinder.TryGetInstanceMember(target.Value, Name, out memberInfo))
                //         memberInfo.Value = value;
                    Expression.Call(CachedReflectionInfo.PSGetMemberBinder_TryGetInstanceMember,
                                    target.Expression.Cast(typeof(object)), Expression.Constant(Name), memberInfoVar),
                    Expression.Assign(Expression.Property(memberInfoVar, "Value"), value.Expression.Cast(typeof(object))),
                var bindingRestrictions = BinderUtils.GetVersionCheck(_getMemberBinder, _getMemberBinder._version)
                    .Merge(value.PSGetTypeRestriction());
                return (new DynamicMetaObject(Expression.Block(new[] { memberInfoVar, temp }, expr),
                    bindingRestrictions)).WriteToDebugLog(this);
                // We never look for properties in the underlying object, we always try to add the key.
                bool isGeneric = PSGetMemberBinder.IsGenericDictionary(targetValue, ref genericTypeArg);
                    // If it's a generic, we must convert our value to genericTypeArg.
                    var hashType = isGeneric
                        ? typeof(IDictionary<,>).MakeGenericType(typeof(string), genericTypeArg)
                        : typeof(IDictionary);
                    var mi = hashType.GetMethod("set_Item");
                    var temp = Expression.Variable(genericTypeArg ?? typeof(object));
                    Type elementType = temp.Type;
                    var conversion = LanguagePrimitives.FigureConversion(value.Value, elementType, out debase);
                        var valueExpr = PSConvertBinder.InvokeConverter(conversion, value.Expression, elementType,
                                                                        debase, ExpressionCache.InvariantCulture);
                            Expression.Block(new[] { temp },
                                Expression.Assign(temp, valueExpr),
                                Expression.Call(PSGetMemberBinder.GetTargetExpr(target, hashType), mi,
                                                Expression.Constant(Name), valueExpr),
                            target.CombineRestrictions(value)).WriteToDebugLog(this);
            memberInfo = _getMemberBinder.GetPSMemberInfo(target, out restrictions, out canOptimize, out aliasConversionType, MemberTypes.Property);
            restrictions = restrictions.Merge(value.PSGetTypeRestriction());
            // If the process has ever used ConstrainedLanguage, then we need to add the language mode
            // to the binding restrictions, and check whether it is allowed. We can't limit
            // the language check to unsafe types, as a safe type might have an unsafe method.
                restrictions = restrictions.Merge(BinderUtils.GetLanguageModeCheckIfHasEverUsedConstrainedLanguage());
                // Validate that this is allowed in the current language mode.
                DynamicMetaObject runtimeError = PSGetMemberBinder.EnsureAllowedInLanguageMode(
                    target, targetValue, Name, _static, new[] { value }, restrictions,
                    "PropertySetterNotSupportedInConstrainedLanguage", ParserStrings.PropertySetConstrainedLanguage);
                if (runtimeError != null)
                    return runtimeError.WriteToDebugLog(this);
                    Expression.Call(CachedReflectionInfo.PSSetMemberBinder_SetAdaptedValue,
                                    PSGetMemberBinder.GetTargetExpr(target, typeof(object)),
                                    Expression.Constant(Name),
                                    value.Expression.Cast(typeof(object))),
                return (errorSuggestion ?? new DynamicMetaObject(
                    Compiler.ThrowRuntimeError("PropertyAssignmentException", ParserStrings.PropertyNotFound, this.ReturnType, Expression.Constant(Name)),
            var psPropertyInfo = memberInfo as PSPropertyInfo;
                if (!psPropertyInfo.IsSettable)
                    return GeneratePropertyAssignmentException(restrictions).WriteToDebugLog(this);
                var psProperty = psPropertyInfo as PSProperty;
                if (psProperty != null)
                    var data = psProperty.adapterData as DotNetAdapter.PropertyCacheEntry;
                        if (data.member.DeclaringType.IsGenericTypeDefinition)
                            Expression innerException = Expression.New(
                                CachedReflectionInfo.SetValueException_ctor,
                                Expression.Constant(ExtendedTypeSystem.CannotInvokeStaticMethodOnUninstantiatedGenericType),
                                Expression.NewArrayInit(typeof(object), Expression.Constant(data.member.DeclaringType.FullName)));
                            expr = Compiler.ThrowRuntimeErrorWithInnerException(
                                "PropertyAssignmentException",
                                this.ReturnType,
                                Expression.Constant(data.member.DeclaringType.FullName));
                        if (data.propertyType.IsByRefLike)
                            // return a ByRef-like value back, so we just disallow setting a member that takes a ByRef-like type value.
                            expr = Expression.Throw(
                                        Expression.Constant(data.member.Name),
                                        Expression.Constant(data.propertyType, typeof(Type)))),
                                this.ReturnType);
                        var propertyInfo = data.member as PropertyInfo;
                        Type lhsType;
                        // Populate transformation attributes.
                        // Order of attributes is the same as order provided by user in the code
                        // We assume that GetCustomAttributes implemented that way.
                        IEnumerable<ArgumentTransformationAttribute> argumentTransformationAttributes =
                            data.member.GetCustomAttributes<ArgumentTransformationAttribute>();
                        bool transformationNeeded = argumentTransformationAttributes.Any();
                        var targetExpr = _static ? null : PSGetMemberBinder.GetTargetExpr(target, data.member.DeclaringType);
                            var propertySetter = propertyInfo.SetMethod;
                            if ((propertySetter.IsFamily || propertySetter.IsFamilyOrAssembly) &&
                                (_classScope == null || !_classScope.IsSubclassOf(propertyInfo.DeclaringType)))
                            lhsType = propertyInfo.PropertyType;
                            lhs = Expression.Property(targetExpr, propertyInfo);
                            Diagnostics.Assert(data.member is FieldInfo,
                            var fieldInfo = (FieldInfo)data.member;
                            lhsType = fieldInfo.FieldType;
                            lhs = Expression.Field(targetExpr, fieldInfo);
                        var nullableUnderlyingType = Nullable.GetUnderlyingType(lhsType);
                        if (nullableUnderlyingType != null)
                            if (value.Value == null)
                                expr = Expression.Block(
                                    Expression.Assign(lhs, GetTransformedExpression(argumentTransformationAttributes, Expression.Constant(null, lhsType))),
                                var tmp = Expression.Variable(nullableUnderlyingType);
                                Expression assignmentExpression;
                                if (transformationNeeded)
                                    var transformedExpr = GetTransformedExpression(argumentTransformationAttributes, value.Expression);
                                    assignmentExpression = DynamicExpression.Dynamic(PSConvertBinder.Get(nullableUnderlyingType), nullableUnderlyingType, transformedExpr);
                                    assignmentExpression = value.CastOrConvert(nullableUnderlyingType);
                                    Expression.Assign(tmp, assignmentExpression),
                                    Expression.Assign(lhs, Expression.New(lhsType.GetConstructor(new[] { nullableUnderlyingType }), tmp)),
                                    tmp.Cast(typeof(object)));
                            var tmp = Expression.Variable(lhsType);
                            Expression assignedValue;
                                assignedValue = DynamicExpression.Dynamic(PSConvertBinder.Get(lhsType), lhsType,
                                   GetTransformedExpression(argumentTransformationAttributes, value.Expression));
                                assignedValue = (lhsType == typeof(object) && value.LimitType == typeof(PSObject))
                                                           ? Expression.Call(CachedReflectionInfo.PSObject_Base, value.Expression.Cast(typeof(PSObject)))
                                                           : value.CastOrConvert(lhsType);
                                Expression.Assign(tmp, assignedValue),
                                Expression.Assign(lhs, tmp),
                        var e = Expression.Variable(typeof(Exception));
                        expr = Expression.TryCatch(expr.Cast(typeof(object)),
                            Expression.Catch(e,
                                    Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_ConvertToMethodInvocationException,
                                                    Expression.Constant(typeof(SetValueInvocationException), typeof(Type)),
                                                    Expression.Constant(null, typeof(MemberInfo))),
                                    Expression.Rethrow(typeof(object)))));
                var codeProperty = psPropertyInfo as PSCodeProperty;
                    var setterMethod = codeProperty.SetterCodeReference;
                    var parameters = setterMethod.GetParameters();
                    var propertyType = parameters[parameters.Length - 1].ParameterType;
                    if (propertyType.IsByRefLike)
                        var expr = Expression.Throw(
                                    Expression.Constant(codeProperty.Name),
                                    Expression.Constant(propertyType, typeof(Type)))),
                            Expression.Assign(temp, value.CastOrConvert(temp.Type)),
                            PSInvokeMemberBinder.InvokeMethod(
                                setterMethod,
                                target: null,
                                new[] { target, value },
                                expandParameters: false,
                                PSInvokeMemberBinder.MethodInvocationType.Setter),
                            temp),
                var scriptProperty = psPropertyInfo as PSScriptProperty;
                    // Invoke Setter
                        Expression.Call(Expression.Constant(scriptProperty, typeof(PSScriptProperty)),
                                        CachedReflectionInfo.PSScriptProperty_InvokeSetter,
                                        PSGetMemberBinder.GetTargetExpr(target), value.Expression.Cast(typeof(object))),
                Diagnostics.Assert(false, "The property we're trying to set was unexpected.");
            if (errorSuggestion != null)
            // If we get here, the property isn't settable.  Call SetAdaptedValue, which will eventually call the setter and raise an exception
            // with a suitable error message.
                                PSGetMemberBinder.GetTargetExpr(target, typeof(object)), Expression.Constant(Name),
        private DynamicMetaObject GeneratePropertyAssignmentException(BindingRestrictions restrictions)
                Expression.Constant(ParserStrings.PropertyIsReadOnly),
                Expression.NewArrayInit(typeof(object), Expression.Constant(Name)));
            var expr = Compiler.ThrowRuntimeErrorWithInnerException("PropertyAssignmentException",
                Expression.Constant(ParserStrings.PropertyIsReadOnly), innerException,
                this.ReturnType, Expression.Constant(Name));
            return new DynamicMetaObject(expr, restrictions);
        internal static object SetAdaptedValue(object obj, string member, object value)
                        memberInfo = PSGetMemberBinder.CloneMemberInfo(memberInfo, obj);
                                                                   null, "PropertyAssignmentException", ParserStrings.PropertyNotFound, member);
                ExceptionHandlingOps.ConvertToMethodInvocationException(e, typeof(SetValueInvocationException), member, 0);
            // Invalidate regular binders
                foreach (PSSetMemberBinder binder in s_binderCache.Values)
                    binder._getMemberBinder._version += 1;
    internal class PSInvokeBinder : InvokeBinder
        internal PSInvokeBinder(CallInfo callInfo) : base(callInfo)
            return errorSuggestion ?? target.ThrowRuntimeError(args, BindingRestrictions.Empty, "CannotInvoke", ParserStrings.CannotInvoke);
    internal sealed class PSInvokeMemberBinder : InvokeMemberBinder
        [TraceSource("MethodInvocation", "Traces the invocation of .NET methods.")]
        internal static readonly PSTraceSource MethodInvocationTracer =
                "MethodInvocation",
                "Traces the invocation of .NET methods.",
        private static readonly SearchValues<string> s_whereSearchValues = SearchValues.Create(
            ["Where", "PSWhere"],
        private static readonly SearchValues<string> s_foreachSearchValues = SearchValues.Create(
            ["ForEach", "PSForEach"],
        internal enum MethodInvocationType
            Ordinary,
            Setter,
            Getter,
            BaseCtor,
            NonVirtual,
        private sealed class KeyComparer : IEqualityComparer<PSInvokeMemberBinderKeyType>
            public bool Equals(PSInvokeMemberBinderKeyType x, PSInvokeMemberBinderKeyType y)
                return x.Item1.Equals(y.Item1, StringComparison.OrdinalIgnoreCase)
                       && x.Item2.Equals(y.Item2)
                       && x.Item3 == y.Item3
                       && x.Item4 == y.Item4
                       && ((x.Item5 == null) ? (y.Item5 == null) : x.Item5.Equals(y.Item5))
                       && x.Item6 == y.Item6
                       && x.Item7 == y.Item7;
            public int GetHashCode(PSInvokeMemberBinderKeyType obj)
                return Utils.CombineHashCodes(
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item1),
                    obj.Item2.GetHashCode(),
                    obj.Item4.GetHashCode(),
                    obj.Item5 == null ? 0 : obj.Item5.GetHashCode(),
                    obj.Item6.GetHashCode(),
                    obj.Item7 == null ? 0 : obj.Item7.GetHashCode());
            Dictionary<PSInvokeMemberBinderKeyType, PSInvokeMemberBinder> s_binderCache = new Dictionary<PSInvokeMemberBinderKeyType, PSInvokeMemberBinder>(new KeyComparer());
        internal readonly PSMethodInvocationConstraints _invocationConstraints;
        internal readonly PSGetMemberBinder _getMemberBinder;
        private PSInvokeMemberBinder GetNonEnumeratingBinder()
            return Get(Name, _classScope, CallInfo, @static: false, propertySetter: _propertySetter, nonEnumerating: true, constraints: _invocationConstraints);
        public static PSInvokeMemberBinder Get(string memberName, CallInfo callInfo, bool @static, bool propertySetter,
                                               PSMethodInvocationConstraints constraints, Type classScope)
            return Get(memberName, classScope, callInfo, @static, propertySetter, nonEnumerating: false, constraints: constraints);
        private static PSInvokeMemberBinder Get(string memberName, Type classScope, CallInfo callInfo, bool @static, bool propertySetter,
                                                bool nonEnumerating, PSMethodInvocationConstraints constraints)
            PSInvokeMemberBinder result;
                var key = Tuple.Create(memberName, callInfo, propertySetter, nonEnumerating, constraints, @static, classScope);
                    result = new PSInvokeMemberBinder(memberName, true, @static, propertySetter, nonEnumerating, callInfo, constraints, classScope);
        private PSInvokeMemberBinder(string name,
                                     bool ignoreCase,
                                     bool propertySetter,
                                     bool nonEnumerating,
                                     Type classScope)
            : base(name, ignoreCase, callInfo)
            this._invocationConstraints = invocationConstraints;
            this._getMemberBinder = PSGetMemberBinder.Get(name, classScope, @static);
                "PSInvokeMember: {0}{1}{2} ver:{3} args:{4} constraints:<{5}>",
                _propertySetter ? "propset " : string.Empty,
                _getMemberBinder._version,
                _invocationConstraints != null ? _invocationConstraints.ToString() : string.Empty);
            if (!target.HasValue || args.Any(static arg => !arg.HasValue))
                return Defer(args.Prepend(target).ToArray());
                args.Any(static mo => mo.Value is PSObject && (PSObject.Base(mo.Value) != mo.Value)))
                    // Therefore, we need to use a stricter restriction to make sure other type of PSObject 'target'
                    return this.DeferForPSObject(args.Prepend(target).ToArray(), targetIsComObject: true).WriteToDebugLog(this);
            if (ComInterop.ComBinder.TryBindInvokeMember(this, _propertySetter, target, args, out result))
                return result.UpdateComRestrictionsForPsObject(args).WriteToDebugLog(this);
                if (!_static && !_nonEnumerating)
                    // As discussed with Bruce, the Where/ForEach operators should work on $null and return an empty collection.
                    // e.g. $null.Where{"I didn't run"} should return an empty collection
                    DynamicMetaObject emptyEnumerator = new DynamicMetaObject(
                        Expression.Call(Expression.NewArrayInit(typeof(object)), CachedReflectionInfo.IEnumerable_GetEnumerator),
                        BindingRestrictions.GetInstanceRestriction(Expression.Call(CachedReflectionInfo.PSObject_Base, target.Expression), null))
                        .WriteToDebugLog(this);
                    BindingRestrictions argRestrictions = args.Aggregate(BindingRestrictions.Empty, static (current, arg) => current.Merge(arg.PSGetMethodArgumentRestriction()));
                    // We need to pass the empty enumerator to the ForEach/Where operators, so that they can return an empty collection.
                    // The ForEach/Where operators will not be able to call the script block if the enumerator is empty.
                    if (s_whereSearchValues.Contains(Name))
                        return InvokeWhereOnCollection(emptyEnumerator, args, argRestrictions).WriteToDebugLog(this);
                    if (s_foreachSearchValues.Contains(Name))
                        return InvokeForEachOnCollection(emptyEnumerator, args, argRestrictions).WriteToDebugLog(this);
                return target.ThrowRuntimeError(args, BindingRestrictions.Empty, "InvokeMethodOnNull", ParserStrings.InvokeMethodOnNull).WriteToDebugLog(this);
                //     PSMethodInfo methodInfo;
                //     if (PSInvokeMemberBinder.TryGetInstanceMethod(target.Value, Name, out methodInfoInfo))
                //         return methodInfo.Invoke(target, args);
                var methodInfoVar = Expression.Variable(typeof(PSMethodInfo));
                    Expression.Call(CachedReflectionInfo.PSInvokeMemberBinder_TryGetInstanceMethod,
                                    target.Expression.Cast(typeof(object)), Expression.Constant(Name), methodInfoVar),
                    Expression.Call(methodInfoVar, CachedReflectionInfo.PSMethodInfo_Invoke,
                                    Expression.NewArrayInit(typeof(object), args.Select(static dmo => dmo.Expression.Cast(typeof(object))))),
                return (new DynamicMetaObject(Expression.Block(new[] { methodInfoVar }, expr),
                    BinderUtils.GetVersionCheck(_getMemberBinder, _getMemberBinder._version))).WriteToDebugLog(this);
            var methodInfo = _getMemberBinder.GetPSMemberInfo(target, out restrictions, out canOptimize, out aliasConversionType, MemberTypes.Method) as PSMethodInfo;
            restrictions = args.Aggregate(restrictions, static (current, arg) => current.Merge(arg.PSGetMethodArgumentRestriction()));
                    target, targetValue, Name, _static, args, restrictions,
                    "MethodInvocationNotSupportedInConstrainedLanguage", ParserStrings.InvokeMethodConstrainedLanguage);
                Diagnostics.Assert(methodInfo == null, "We don't bother returning members if we can't optimize.");
                if (_propertySetter)
                    call = Expression.Call(
                        CachedReflectionInfo.PSInvokeMemberBinder_InvokeAdaptedSetMember,
                                                args.Take(args.Length - 1).Select(static arg => arg.Expression.Cast(typeof(object)))),
                        args.Last().Expression.Cast(typeof(object)));
                        CachedReflectionInfo.PSInvokeMemberBinder_InvokeAdaptedMember,
                                                args.Select(static arg => arg.Expression.Cast(typeof(object)))));
                return new DynamicMetaObject(call, restrictions).WriteToDebugLog(this);
            // If the target value is a PSObject and its base object happens to be a Hashtable or ArrayList,
            // we might have three interesting cases here:
            //  (1) the target value could be a regular PSObject that wraps the Hashtable/ArrayList, i.e. $target = [PSObject]::AsPSObject($hash)
            //  (2) the target value could be a deserialized object (PSObject) with the 'IsDeserialized' property to be false, i.e. deserialized Hashtable/ArrayList/Dictionary[string, string]
            //  (3) the target value could be a deserialized object (PSObject) with the 'IsDeserialized' property to be true, i.e. deserialized XmlElement
            // For the first two cases, it's OK to call a .NET method from the base object, such as $target.Add().
            // For the third case, calling a .NET method from the base object is incorrect, because the original type of the deserialized object doesn't have the method.
            //  example: XmlElement derives from IEnumerable, so it's treated as a container object when powershell does the serialization -- using an ArrayList to hold
            //  its elements -- but we cannot call Add() on it.
            // We add restriction to do this check only if the methodInfo is a .NET method/parameterizedProperty, otherwise it's not affected by the above cases, for example, a PSScriptMethod
            // defined in the TypeTable will only get affected by the PSTypeNames.
            if (methodInfo is PSMethod || methodInfo is PSParameterizedProperty)
                var psObj = target.Value as PSObject;
                if (psObj != null && (targetValue.GetType() == typeof(Hashtable) || targetValue.GetType() == typeof(ArrayList)))
                    // If we get here, then the target value should have 'isDeserialized == false', otherwise we cannot get a .NET methodInfo
                    // from _getMemberBinder.GetPSMemberInfo(). This is because when 'isDeserialized' is true, we use the PSObject to find the
                    // corresponding Adapter -- PSObjectAdapter, which cannot be optimized.
                    Diagnostics.Assert(!psObj.IsDeserialized,
                        "isDeserialized should be false, because if not, we cannot get a .NET method/parameterizedProperty from GetPSMemberInfo");
                        Expression.Not(Expression.Property(target.Expression.Cast(typeof(PSObject)), CachedReflectionInfo.PSObject_IsDeserialized))));
            var psMethod = methodInfo as PSMethod;
                var data = (DotNetAdapter.MethodCacheEntry)psMethod.adapterData;
                return InvokeDotNetMethod(CallInfo, Name, _invocationConstraints, _propertySetter ? MethodInvocationType.Setter : MethodInvocationType.Ordinary, target, args, restrictions,
                    data.methodInformationStructures, typeof(MethodException)).WriteToDebugLog(this);
            var scriptMethod = methodInfo as PSScriptMethod;
                    Expression.Call(CachedReflectionInfo.PSScriptMethod_InvokeScript,
                                    Expression.Constant(scriptMethod.Script),
                                                            args.Select(static e => e.Expression.Cast(typeof(object))))),
            var codeMethod = methodInfo as PSCodeMethod;
                Expression expr = InvokeMethod(codeMethod.CodeReference, null, args.Prepend(target).ToArray(), false, MethodInvocationType.Ordinary);
                if (codeMethod.CodeReference.ReturnType == typeof(void))
                    expr = Expression.Block(expr, ExpressionCache.AutomationNullConstant);
            var parameterizedProperty = methodInfo as PSParameterizedProperty;
            if (parameterizedProperty != null)
                var p = (DotNetAdapter.ParameterizedPropertyCacheEntry)parameterizedProperty.adapterData;
                    _propertySetter ? p.setterInformation : p.getterInformation,
                    _propertySetter ? typeof(SetValueInvocationException) : typeof(GetValueInvocationException)).WriteToDebugLog(this);
            // See comment on PSGetMemberBinder.PropertyDoesntExistCheckSpecialCases - the same applies here for method calls.
            if (!_static && !_nonEnumerating && target.Value != AutomationNull.Value)
                // Invoking Where and ForEach operators on collections.
                    return InvokeWhereOnCollection(target, args, restrictions).WriteToDebugLog(this);
                    return InvokeForEachOnCollection(target, args, restrictions).WriteToDebugLog(this);
                var enumerableTarget = PSEnumerableBinder.IsEnumerable(target);
                if (enumerableTarget != null)
                    // Try calling the method on each member of the collection.
                    return InvokeMemberOnCollection(enumerableTarget, args, targetValue.GetType(), restrictions).WriteToDebugLog(this);
            var typeForMessage = _static && targetValue is Type ? (Type)targetValue : targetValue.GetType();
                Compiler.ThrowRuntimeError(ParserOps.MethodNotFoundErrorId, ParserStrings.MethodNotFound,
                                           Expression.Constant(typeForMessage.FullName), Expression.Constant(Name)),
        internal static DynamicMetaObject InvokeDotNetMethod(
            PSMethodInvocationConstraints psMethodInvocationConstraints,
            MethodInvocationType methodInvocationType,
            MethodInformation[] mi,
            Type errorExceptionType)
            int numArgs = args.Length;
            if (methodInvocationType == MethodInvocationType.Setter)
                numArgs -= 1;
            object[] argValues = new object[numArgs];
            for (int i = 0; i < numArgs; ++i)
                object arg = args[i].Value;
                argValues[i] = arg == AutomationNull.Value ? null : arg;
            var result = Adapter.FindBestMethod(
                mi,
                psMethodInvocationConstraints,
                allowCastingToByRefLikeType: true,
                argValues,
            if (callNonVirtually && methodInvocationType != MethodInvocationType.BaseCtor)
                methodInvocationType = MethodInvocationType.NonVirtual;
                var methodInfo = result.method;
                var expr = InvokeMethod(methodInfo, target, args, expandParamsOnBest, methodInvocationType);
                if (MethodInvocationTracer.IsEnabled)
                            Expression.Constant(MethodInvocationTracer),
                            CachedReflectionInfo.PSTraceSource_WriteLine,
                            Expression.Constant("Invoking method: {0}"),
                            Expression.Constant(result.methodDefinition)),
                // If we're calling SteppablePipeline.{Begin|Process|End}, we don't want
                // to wrap exceptions - this is very much a special case to help error
                // propagation and ensure errors are attributed to the correct code (the
                // cmdlet invoked, not the method call from some proxy.)
                if (methodInfo.DeclaringType == typeof(SteppablePipeline)
                    && (methodInfo.Name.Equals("Begin", StringComparison.Ordinal))
                        || methodInfo.Name.Equals("Process", StringComparison.Ordinal)
                        || methodInfo.Name.Equals("End", StringComparison.Ordinal))
                // Likewise, when calling methods in types defined by PowerShell, we don't
                // want to wrap the exception.
                if (methodInfo.DeclaringType.Assembly.GetCustomAttributes(typeof(DynamicClassImplementationAssemblyAttribute)).Any())
                            expr.Type,
                                            Expression.Constant(errorExceptionType, typeof(Type)),
                                            Expression.Constant(methodInfo.Name),
                                            ExpressionCache.Constant(args.Length),
                                            Expression.Constant(methodInfo, typeof(MethodBase))),
                            Expression.Rethrow(expr.Type))));
                Compiler.CreateThrow(typeof(object), errorExceptionType,
                                     errorId, null, errorMsg, new object[] { name, callInfo.ArgumentCount }),
        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target,
                DynamicExpression.Dynamic(
                    new PSInvokeBinder(CallInfo),
                    args.Prepend(target).Select(static dmo => dmo.Expression)
        internal static MethodInfo FindBestMethod(DynamicMetaObject target,
                                                  IEnumerable<DynamicMetaObject> args,
                                                  PSMethodInvocationConstraints invocationConstraints)
            MethodInfo result = null;
            var psMethod = PSObject.DotNetInstanceAdapter.GetDotNetMethod<PSMethod>(PSObject.Base(target.Value), methodName);
                bool expandParameters;
                var mi = Adapter.FindBestMethod(
                    data.methodInformationStructures,
                    args.Select(static arg => arg.Value == AutomationNull.Value ? null : arg.Value).ToArray(),
                    out expandParameters,
                if (mi != null)
                    result = (MethodInfo)mi.method;
        internal static Expression InvokeMethod(MethodBase mi, DynamicMetaObject target, DynamicMetaObject[] args, bool expandParameters, MethodInvocationType invocationType)
            List<Expression> copyOutTemps = new List<Expression>();
            ConstructorInfo constructorInfo = null;
            MethodInfo methodInfo = mi as MethodInfo;
                Type returnType = methodInfo.ReturnType;
                if (returnType.IsByRefLike)
                    ConstructorInfo exceptionCtorInfo;
                    switch (invocationType)
                        case MethodInvocationType.Getter:
                            exceptionCtorInfo = CachedReflectionInfo.GetValueException_ctor;
                        case MethodInvocationType.Setter:
                            exceptionCtorInfo = CachedReflectionInfo.SetValueException_ctor;
                            exceptionCtorInfo = CachedReflectionInfo.MethodException_ctor;
                            exceptionCtorInfo,
                            Expression.Constant(nameof(ExtendedTypeSystem.CannotCallMethodWithByRefLikeReturnType)),
                            Expression.Constant(ExtendedTypeSystem.CannotCallMethodWithByRefLikeReturnType),
                                Expression.Constant(returnType, typeof(Type)))),
                constructorInfo = (ConstructorInfo)mi;
                Type declaringType = constructorInfo.DeclaringType;
                if (declaringType.IsByRefLike)
                            CachedReflectionInfo.MethodException_ctor,
                            Expression.Constant(nameof(ExtendedTypeSystem.CannotInstantiateBoxedByRefLikeType)),
                            Expression.Constant(ExtendedTypeSystem.CannotInstantiateBoxedByRefLikeType),
                                Expression.Constant(declaringType, typeof(Type)))),
            // Invoking a base constructor or a base method (non-virtual call) depends reflection invocation
            // via helper methods, and thus all arguments need to be casted to 'object'. The ByRef-like types
            // cannot be boxed and won't work with reflection.
            bool allowCastingToByRefLikeType =
                invocationType != MethodInvocationType.BaseCtor &&
                invocationType != MethodInvocationType.NonVirtual;
            var parameters = mi.GetParameters();
            var argExprs = new Expression[parameters.Length];
            var argsToLog = new List<Expression>(Math.Max(parameters.Length, args.Length));
                Type parameterType = parameters[i].ParameterType;
                string paramName = parameters[i].Name;
                if (string.IsNullOrWhiteSpace(paramName))
                    paramName = i.ToString(CultureInfo.InvariantCulture);
                var paramArrayAttrs = parameters[i].GetCustomAttributes(typeof(ParamArrayAttribute), false);
                    Diagnostics.Assert(i == parameters.Length - 1, "vararg parameter is not the last");
                    var paramElementType = parameterType.GetElementType();
                    if (expandParameters)
                        IEnumerable<Expression> elements = args
                            .Skip(i)
                            .Select(a =>
                                a.CastOrConvertMethodArgument(
                                    paramElementType,
                                    paramName,
                                    mi.Name,
                                    initTemps))
                        argExprs[i] = Expression.NewArrayInit(paramElementType, elements);
                        // User specified the element arguments, so we log them instead of the compiler-created array.
                        argsToLog.AddRange(elements);
                        var arg = args[i].CastOrConvertMethodArgument(
                            initTemps);
                        argExprs[i] = arg;
                        argsToLog.Add(arg);
                else if (i >= args.Length)
                    // We don't log the default value for an optional parameter, as it's not specified by the user.
                        parameters[i].IsOptional,
                        "if there are too few arguments, FindBestMethod should only succeed if parameters are optional");
                    var argValue = parameters[i].DefaultValue;
                    if (argValue == null)
                            // When the default value is null for a ByRef parameter (e.g. an optional `in` parameter
                            // using `default`), expression trees cannot create Expression.Default for the T& type.
                            // In that case we switch to the element type and use Default(TElement) instead.
                        argExprs[i] = Expression.Default(parameterType);
                    else if (!parameters[i].HasDefaultValue && parameterType != typeof(object) && argValue == Type.Missing)
                        // If the method contains just [Optional] without a default value set then we cannot use
                        // Type.Missing as a placeholder. Instead we use the default value for that type. Only
                        // exception to this rule is when the parameter type is object.
                        // We don't specify the parameter type in the constant expression. Normally the default
                        // argument's type should match the parameter type, but sometimes it won't, e.g. with COM,
                        // the default can be System.Reflection.Missing.Value and this value is handled specially.
                        argExprs[i] = Expression.Constant(argValue);
                        if (args[i].Value is not PSReference)
                            return Compiler.CreateThrow(typeof(object), typeof(MethodException),
                                     "NonRefArgumentToRefParameterMsg", null, ExtendedTypeSystem.NonRefArgumentToRefParameter,
                                     new object[] { i + 1, typeof(PSReference).FullName, "[ref]" });
                        var temp = Expression.Variable(parameterType.GetElementType());
                        temps.Add(temp);
                        var psRefValue = Expression.Property(args[i].Expression.Cast(typeof(PSReference)), CachedReflectionInfo.PSReference_Value);
                        initTemps.Add(Expression.Assign(temp, psRefValue.Convert(temp.Type)));
                        copyOutTemps.Add(Expression.Assign(psRefValue, temp.Cast(typeof(object))));
                        argExprs[i] = temp;
                        argsToLog.Add(temp);
                        var convertedArg = args[i].CastOrConvertMethodArgument(
                            allowCastingToByRefLikeType,
                        argExprs[i] = convertedArg;
                        // If the converted arg is a byref-like type, then we log the original arg.
                        argsToLog.Add(convertedArg.Type.IsByRefLike
                            ? args[i].Expression
                            : convertedArg);
            if (constructorInfo != null)
                if (invocationType == MethodInvocationType.BaseCtor)
                    var targetExpr = target.Value is PSObject
                        ? target.Expression.Cast(constructorInfo.DeclaringType)
                        : PSGetMemberBinder.GetTargetExpr(target, constructorInfo.DeclaringType);
                        CachedReflectionInfo.ClassOps_CallBaseCtor,
                        Expression.Constant(constructorInfo, typeof(ConstructorInfo)),
                        Expression.NewArrayInit(typeof(object), argExprs.Select(static x => x.Cast(typeof(object)))));
                    call = Expression.New(constructorInfo, argExprs);
                if (invocationType == MethodInvocationType.NonVirtual && !methodInfo.IsStatic)
                        methodInfo.ReturnType == typeof(void)
                            ? CachedReflectionInfo.ClassOps_CallVoidMethodNonVirtually
                            : CachedReflectionInfo.ClassOps_CallMethodNonVirtually,
                        PSGetMemberBinder.GetTargetExpr(target, methodInfo.DeclaringType),
                        Expression.Constant(methodInfo, typeof(MethodInfo)),
                    call = methodInfo.IsStatic
                           ? Expression.Call(methodInfo, argExprs)
                           : Expression.Call(
                               methodInfo, argExprs);
            // We need to add one expression to log the .NET invocation before actually invoking:
            //  - Log method invocation to AMSI Notifications (can throw PSSecurityException)
            //  - Invoke method
            string targetName = mi.ReflectedType?.FullName ?? string.Empty;
            string methodName = mi.Name is ".ctor" ? "new" : mi.Name;
            if (temps.Count > 0)
                if (call.Type != typeof(void) && copyOutTemps.Count > 0)
                    var retValue = Expression.Variable(call.Type);
                    temps.Add(retValue);
                    call = Expression.Assign(retValue, call);
                    copyOutTemps.Add(retValue);
                AddMemberInvocationLogging(initTemps, targetName, methodName, argsToLog);
                call = Expression.Block(call.Type, temps, initTemps.Append(call).Concat(copyOutTemps));
                call = AddMemberInvocationLogging(call, targetName, methodName, argsToLog);
            return call;
        private DynamicMetaObject InvokeMemberOnCollection(DynamicMetaObject targetEnumerator, DynamicMetaObject[] args, Type typeForMessage, BindingRestrictions restrictions)
            var d = DynamicExpression.Dynamic(this, this.ReturnType, args.Select(static a => a.Expression).Prepend(ExpressionCache.NullConstant));
                Expression.Call(CachedReflectionInfo.EnumerableOps_MethodInvoker,
                                Expression.Constant(d.DelegateType, typeof(Type)),
                                targetEnumerator.Expression,
                                                        args.Select(static a => a.Expression.Cast(typeof(object)))),
                                Expression.Constant(typeForMessage, typeof(Type))
                targetEnumerator.Restrictions.Merge(restrictions));
        private static DynamicMetaObject GetTargetAsEnumerable(DynamicMetaObject target)
            // If null wrap the target in an array.
            enumerableTarget ??= PSEnumerableBinder.IsEnumerable(
                    target.GetSimpleTypeRestriction()));
            return enumerableTarget;
        /// <param name="target">The target to operate against.</param>
        ///     Arguments to the operator. The first argument must be either a scriptblock
        ///     or a string representing a 'simple where' expression. The second is an enum that controls
        ///     the matching behaviour returning the first, last or all matching elements.
        /// <param name="argRestrictions">The binding restrictions for the arguments.</param>
        private DynamicMetaObject InvokeWhereOnCollection(DynamicMetaObject target, DynamicMetaObject[] args, BindingRestrictions argRestrictions)
            var lhsEnumerator = GetTargetAsEnumerable(target);
            switch (args.Length)
                            Expression.Call(CachedReflectionInfo.EnumerableOps_Where,
                                PSGetMemberBinder.GetTargetExpr(args[0]).Convert(typeof(ScriptBlock)),
                                Expression.Constant(WhereOperatorSelectionMode.Default),
                                Expression.Constant(0)),
                            lhsEnumerator.Restrictions.Merge(argRestrictions));
                                PSGetMemberBinder.GetTargetExpr(args[1]).Convert(typeof(WhereOperatorSelectionMode)),
                            PSGetMemberBinder.GetTargetExpr(args[2]).Convert(typeof(int))),
                        // If the arity is wrong, throw the extended type system exception.
                                Expression.New(CachedReflectionInfo.MethodException_ctor,
                                    Expression.Constant("MethodCountCouldNotFindBest"),
                                    Expression.Constant(ExtendedTypeSystem.MethodArgumentCountException),
                                        Expression.Constant(".Where({ expression } [, mode [, numberToReturn]])").Cast(typeof(object)),
                                        ExpressionCache.Constant(args.Length).Cast(typeof(object)))),
        private DynamicMetaObject InvokeForEachOnCollection(DynamicMetaObject targetEnumerator, DynamicMetaObject[] args, BindingRestrictions restrictions)
            targetEnumerator = GetTargetAsEnumerable(targetEnumerator);
            if (args.Length < 1)
                                Expression.Constant(".ForEach(expression [, arguments...])").Cast(typeof(object)),
            var lhsEnumerator = PSEnumerableBinder.IsEnumerable(targetEnumerator).Expression;
            Expression argsToPass;
            if (args.Length > 1)
                argsToPass = Expression.NewArrayInit(typeof(object),
                                      args.Skip(1).Select(static a => a.Expression.Cast(typeof(object))));
                argsToPass = Expression.NewArrayInit(typeof(object));
                Expression.Call(CachedReflectionInfo.EnumerableOps_ForEach,
                            lhsEnumerator, PSGetMemberBinder.GetTargetExpr(args[0], typeof(object)), argsToPass),
        #region Runtime helpers
        internal static bool IsHomogeneousArray<T>(object[] args)
            foreach (object element in args)
                if (Adapter.GetObjectType(element, debase: true) != typeof(T))
        internal static bool IsHeterogeneousArray(object[] args)
            var firstElement = PSObject.Base(args[0]);
            if (firstElement == null)
            var firstType = firstElement.GetType();
            if (firstType.Equals(typeof(object)))
                // When the effective argument type is object[], it's for one of 2 reasons
                //     * the array contains elements with different types
                //     * the array contains elements that are all object
                // Arrays of all object is rare, which is why this method is called IsHeterogeneousArray,
                // but we still want to correctly handle object[] full of objects, so we return true
                // for that case as well.
                if (Adapter.GetObjectType(args[i], debase: true) != firstType)
        internal static object InvokeAdaptedMember(object obj, string methodName, object[] args)
            var methodInfo = adapterSet.OriginalAdapter.BaseGetMember<PSMemberInfo>(obj, methodName) as PSMethodInfo;
            if (methodInfo == null && adapterSet.DotNetAdapter != null)
                methodInfo = adapterSet.DotNetAdapter.BaseGetMember<PSMemberInfo>(obj, methodName) as PSMethodInfo;
                return methodInfo.Invoke(args);
            // The object doesn't have 'Where' and 'ForEach' methods.
            // As a last resort, we invoke 'Where' and 'ForEach' operators on singletons like
            //    ([pscustomobject]@{ foo = 'bar' }).Foreach({$_})
            //    ([pscustomobject]@{ foo = 'bar' }).Where({1})
            if (s_whereSearchValues.Contains(methodName))
                var enumerator = (new object[] { obj }).GetEnumerator();
                        return EnumerableOps.Where(enumerator, args[0] as ScriptBlock, WhereOperatorSelectionMode.Default, 0);
                        return EnumerableOps.Where(enumerator, args[0] as ScriptBlock,
                                                   LanguagePrimitives.ConvertTo<WhereOperatorSelectionMode>(args[1]), 0);
                                                   LanguagePrimitives.ConvertTo<WhereOperatorSelectionMode>(args[1]), LanguagePrimitives.ConvertTo<int>(args[2]));
            if (s_foreachSearchValues.Contains(methodName))
                object[] argsToPass;
                    int length = args.Length - 1;
                    argsToPass = new object[length];
                    Array.Copy(args, sourceIndex: 1, argsToPass, destinationIndex: 0, length: length);
                    argsToPass = Array.Empty<object>();
                return EnumerableOps.ForEach(enumerator, args[0], argsToPass);
            throw InterpreterError.NewInterpreterException(methodName, typeof(RuntimeException), null,
                "MethodNotFound", ParserStrings.MethodNotFound, ParserOps.GetTypeFullName(obj), methodName);
        internal static object InvokeAdaptedSetMember(object obj, string methodName, object[] args, object valueToSet)
            var methodInfo = adapterSet.OriginalAdapter.BaseGetMember<PSParameterizedProperty>(obj, methodName);
                methodInfo = adapterSet.DotNetAdapter.BaseGetMember<PSParameterizedProperty>(obj, methodName);
                methodInfo.InvokeSet(valueToSet, args);
        internal static bool TryGetInstanceMethod(object value, string memberName, out PSMethodInfo methodInfo)
            var memberInfo = PSObject.HasInstanceMembers(value, out instanceMembers) ? instanceMembers[memberName] : null;
            methodInfo = memberInfo as PSMethodInfo;
                // No member, just return false
                // Found a member, but it wasn't a method, throw an exception because we can't call it.
                throw InterpreterError.NewInterpreterException(memberName, typeof(RuntimeException), null,
                    "MethodNotFound", ParserStrings.MethodNotFound, ParserOps.GetTypeFullName(value), memberName);
                foreach (PSInvokeMemberBinder binder in s_binderCache.Values)
        private static Expression AddMemberInvocationLogging(
            Expression expr,
            List<Expression> args)
            // For efficiency this is a no-op on non-Windows platforms.
            Expression[] invocationArgs = new Expression[args.Count];
            for (int i = 0; i < args.Count; i++)
                invocationArgs[i] = args[i].Cast(typeof(object));
                    CachedReflectionInfo.MemberInvocationLoggingOps_LogMemberInvocation,
                    Expression.Constant(targetName),
                    Expression.Constant(name),
                    Expression.NewArrayInit(typeof(object), invocationArgs)),
        private static void AddMemberInvocationLogging(
                Expression.NewArrayInit(typeof(object), invocationArgs)));
    internal class PSCreateInstanceBinder : CreateInstanceBinder
        private readonly bool _publicTypeOnly;
        private int _version;
        private sealed class KeyComparer : IEqualityComparer<Tuple<CallInfo, PSMethodInvocationConstraints, bool>>
            public bool Equals(Tuple<CallInfo, PSMethodInvocationConstraints, bool> x,
                               Tuple<CallInfo, PSMethodInvocationConstraints, bool> y)
                return x.Item1.Equals(y.Item1)
                       && ((x.Item2 == null) ? (y.Item2 == null) : x.Item2.Equals(y.Item2))
                       && x.Item3 == y.Item3;
            public int GetHashCode(Tuple<CallInfo, PSMethodInvocationConstraints, bool> obj)
            Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints, bool>, PSCreateInstanceBinder>
            s_binderCache =
                new Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints, bool>, PSCreateInstanceBinder>(new KeyComparer());
        public static PSCreateInstanceBinder Get(CallInfo callInfo, PSMethodInvocationConstraints constraints, bool publicTypeOnly = false)
            PSCreateInstanceBinder result;
                var key = Tuple.Create(callInfo, constraints, publicTypeOnly);
                    result = new PSCreateInstanceBinder(callInfo, constraints, publicTypeOnly);
                foreach (var binder in s_binderCache.Values)
        internal PSCreateInstanceBinder(CallInfo callInfo, PSMethodInvocationConstraints constraints, bool publicTypeOnly)
            _publicTypeOnly = publicTypeOnly;
                "PSCreateInstanceBinder: ver:{0} args:{1} constraints:<{2}>",
                _callInfo.ArgumentCount,
                _constraints != null ? _constraints.ToString() : string.Empty);
            var instanceType = targetValue as Type ?? targetValue.GetType();
            if (instanceType.IsByRefLike)
                // ByRef-like types are not boxable and should be used only on stack
                    Expression.Call(CachedReflectionInfo.PSCreateInstanceBinder_IsTargetTypeByRefLike, target.Expression));
                return target.ThrowRuntimeError(
                        CachedReflectionInfo.PSCreateInstanceBinder_GetTargetTypeName,
                        target.Expression)).WriteToDebugLog(this);
            if (_publicTypeOnly && !TypeResolver.IsPublic(instanceType))
                // If 'publicTypeOnly' specified, we only support creating instance for public types.
                        Expression.Call(CachedReflectionInfo.PSCreateInstanceBinder_IsTargetTypeNonPublic, target.Expression));
                    nameof(ParserStrings.MethodNotFound),
                    ParserStrings.MethodNotFound,
                        target.Expression),
                    Expression.Constant("new")).WriteToDebugLog(this);
            var ctors = instanceType.GetConstructors();
            restrictions = ReferenceEquals(instanceType, targetValue)
                               ? (target.Value is PSObject)
                                     ? BindingRestrictions.GetInstanceRestriction(Expression.Call(CachedReflectionInfo.PSObject_Base, target.Expression), instanceType)
                                     : BindingRestrictions.GetInstanceRestriction(target.Expression, instanceType)
            restrictions = restrictions.Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, instanceType, _version));
            if (ctors.Length == 0 && _callInfo.ArgumentCount == 0 && instanceType.IsValueType)
                // No ctors, just call the default ctor
                return new DynamicMetaObject(Expression.New(instanceType).Cast(this.ReturnType), restrictions).WriteToDebugLog(this);
            if (context != null && context.LanguageMode == PSLanguageMode.ConstrainedLanguage && !CoreTypes.Contains(instanceType))
                    return target.ThrowRuntimeError(restrictions, "CannotCreateTypeConstrainedLanguage", ParserStrings.CannotCreateTypeConstrainedLanguage).WriteToDebugLog(this);
                string targetName = instanceType?.FullName;
                    title: ParameterBinderStrings.WDACBinderTypeCreationLogTitle,
                    message: StringUtil.Format(ParameterBinderStrings.WDACBinderTypeCreationLogMessage, targetName ?? string.Empty),
                    fqid: "TypeCreationNotAllowed",
            var newConstructors = DotNetAdapter.GetMethodInformationArray(ctors);
            return PSInvokeMemberBinder.InvokeDotNetMethod(_callInfo, "new", _constraints, PSInvokeMemberBinder.MethodInvocationType.Ordinary,
                                                           target, args, restrictions, newConstructors, typeof(MethodException)).WriteToDebugLog(this);
        /// Check if the target type is ByRef-like.
        internal static bool IsTargetTypeByRefLike(object target)
            var targetValue = PSObject.Base(target);
            if (targetValue == null) { return false; }
            return instanceType.IsByRefLike;
        /// Check if the target type is not public.
        internal static bool IsTargetTypeNonPublic(object target)
            return !TypeResolver.IsPublic(instanceType);
        /// Return the full name of the target type.
        internal static string GetTargetTypeName(object target)
            Diagnostics.Assert(targetValue != null, "caller makes sure target is not null");
            return instanceType.FullName;
    internal class PSInvokeBaseCtorBinder : InvokeMemberBinder
        private sealed class KeyComparer : IEqualityComparer<Tuple<CallInfo, PSMethodInvocationConstraints>>
            public bool Equals(Tuple<CallInfo, PSMethodInvocationConstraints> x,
                               Tuple<CallInfo, PSMethodInvocationConstraints> y)
                       && ((x.Item2 == null) ? (y.Item2 == null) : x.Item2.Equals(y.Item2));
            public int GetHashCode(Tuple<CallInfo, PSMethodInvocationConstraints> obj)
            Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints>, PSInvokeBaseCtorBinder>
                new Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints>, PSInvokeBaseCtorBinder>(new KeyComparer());
        public static PSInvokeBaseCtorBinder Get(CallInfo callInfo, PSMethodInvocationConstraints constraints)
            PSInvokeBaseCtorBinder result;
                var key = Tuple.Create(callInfo, constraints);
                    result = new PSInvokeBaseCtorBinder(callInfo, constraints);
        internal PSInvokeBaseCtorBinder(CallInfo callInfo, PSMethodInvocationConstraints constraints)
            : base(".ctor", false, callInfo)
            var ctors = _constraints.MethodTargetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var restrictions = target.Value is PSObject
            var newConstructors = DotNetAdapter.GetMethodInformationArray(ctors.Where(static c => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly).ToArray());
            return PSInvokeMemberBinder.InvokeDotNetMethod(_callInfo, "new", _constraints, PSInvokeMemberBinder.MethodInvocationType.BaseCtor,
                                                           target, args, restrictions, newConstructors, typeof(MethodException));
    #endregion Standard binders
