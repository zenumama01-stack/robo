    internal class DispCallableMetaObject : DynamicMetaObject
        private readonly DispCallable _callable;
        internal DispCallableMetaObject(Expression expression, DispCallable callable)
            : base(expression, BindingRestrictions.Empty, callable)
            _callable = callable;
            return BindGetOrInvoke(indexes, binder.CallInfo) ??
                base.BindGetIndex(binder, indexes);
            return BindGetOrInvoke(args, binder.CallInfo) ??
                base.BindInvoke(binder, args);
        private DynamicMetaObject BindGetOrInvoke(DynamicMetaObject[] args, CallInfo callInfo)
            IDispatchComObject target = _callable.DispatchComObject;
            string name = _callable.MemberName;
            if (target.TryGetMemberMethod(name, out ComMethodDesc method) ||
                target.TryGetMemberMethodExplicit(name, out method))
                List<ParameterExpression> temps = new List<ParameterExpression>();
                List<Expression> initTemps = new List<Expression>();
                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(method, ref args, temps, initTemps);
                return BindComInvoke(method, args, callInfo, isByRef, temps, initTemps);
            bool holdsNull = value.Value == null && value.HasValue;
            if (target.TryGetPropertySetter(name, out ComMethodDesc method, value.LimitType, holdsNull) ||
                target.TryGetPropertySetterExplicit(name, out method, value.LimitType, holdsNull))
                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(method, ref indexes, temps, initTemps);
                isByRef = isByRef.AddLast(false);
                // Convert the value to the target type
                DynamicMetaObject updatedValue = new DynamicMetaObject(
                    value.CastOrConvertMethodArgument(
                        value.LimitType,
                        "SetIndex",
                    value.Restrictions);
                var result = BindComInvoke(method, indexes.AddLast(updatedValue), binder.CallInfo, isByRef, temps, initTemps);
                // Make sure to return the value; some languages need it.
                    Expression.Block(result.Expression, Expression.Convert(value.Expression, typeof(object))),
            return base.BindSetIndex(binder, indexes, value);
        private DynamicMetaObject BindComInvoke(ComMethodDesc method, DynamicMetaObject[] indexes, CallInfo callInfo, bool[] isByRef,
            Expression callable = Expression;
            Expression dispCall = Helpers.Convert(callable, typeof(DispCallable));
            DynamicMetaObject invoke = new ComInvokeBinder(
                callInfo,
                indexes,
                isByRef,
                DispCallableRestrictions(),
                Expression.Constant(method),
                Expression.Property(
                    dispCall,
                    typeof(DispCallable).GetProperty(nameof(DispCallable.DispatchObject))
                method
            ).Invoke();
            if (temps != null && temps.Count > 0)
                Expression invokeExpression = invoke.Expression;
                Expression call = Expression.Block(invokeExpression.Type, temps, initTemps.Append(invokeExpression));
                invoke = new DynamicMetaObject(call, invoke.Restrictions);
            return invoke;
        private BindingRestrictions DispCallableRestrictions()
            BindingRestrictions callableTypeRestrictions = BindingRestrictions.GetTypeRestriction(callable, typeof(DispCallable));
            MemberExpression dispatch = Expression.Property(dispCall, typeof(DispCallable).GetProperty(nameof(DispCallable.DispatchComObject)));
            MemberExpression dispId = Expression.Property(dispCall, typeof(DispCallable).GetProperty(nameof(DispCallable.DispId)));
            BindingRestrictions dispatchRestriction = IDispatchMetaObject.IDispatchRestriction(dispatch, _callable.DispatchComObject.ComTypeDesc);
            BindingRestrictions memberRestriction = BindingRestrictions.GetExpressionRestriction(
                Expression.Equal(dispId, Expression.Constant(_callable.DispId))
            return callableTypeRestrictions.Merge(dispatchRestriction).Merge(memberRestriction);
