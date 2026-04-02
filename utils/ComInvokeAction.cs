    /// Invokes the object. If it falls back, just produce an error.
    internal sealed class ComInvokeAction : InvokeBinder
        internal ComInvokeAction(CallInfo callInfo)
            : base(callInfo)
            if (ComBinder.TryBindInvoke(this, target, args, out DynamicMetaObject res))
            return errorSuggestion ?? new DynamicMetaObject(
                Expression.Throw(
                    Expression.New(
                        typeof(NotSupportedException).GetConstructor(new[] { typeof(string) }),
                        Expression.Constant(ParserStrings.CannotCall)
                    typeof(object)
                target.Restrictions.Merge(BindingRestrictions.Combine(args))
    /// Splats the arguments to another nested dynamic site, which does the
    /// real invocation of the IDynamicMetaObjectProvider.
    internal sealed class SplatInvokeBinder : CallSiteBinder
        internal static readonly SplatInvokeBinder Instance = new SplatInvokeBinder();
        // Just splat the args and dispatch through a nested site
        public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
            Debug.Assert(args.Length == 2);
            int count = ((object[])args[1]).Length;
            ParameterExpression array = parameters[1];
            var nestedArgs = new ReadOnlyCollectionBuilder<Expression>(count + 1);
            var delegateArgs = new Type[count + 3]; // args + target + returnType + CallSite
            nestedArgs.Add(parameters[0]);
            delegateArgs[0] = typeof(CallSite);
            delegateArgs[1] = typeof(object);
                nestedArgs.Add(Expression.ArrayAccess(array, Expression.Constant(i)));
                delegateArgs[i + 2] = typeof(object).MakeByRefType();
            delegateArgs[delegateArgs.Length - 1] = typeof(object);
            return Expression.IfThen(
                Expression.Equal(Expression.ArrayLength(array), Expression.Constant(count)),
                Expression.Return(
                    returnLabel,
                    Expression.MakeDynamic(
                        Expression.GetDelegateType(delegateArgs),
                        new ComInvokeAction(new CallInfo(count)),
                        nestedArgs
