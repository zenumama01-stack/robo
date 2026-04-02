    internal sealed class IDispatchMetaObject : ComFallbackMetaObject
        private readonly IDispatchComObject _self;
        internal IDispatchMetaObject(Expression expression, IDispatchComObject self)
            : base(expression, BindingRestrictions.Empty, self)
            _self = self;
            ComMethodDesc method = null;
            // See if this is actually a property set
            ComBinder.ComInvokeMemberBinder comInvokeBinder = binder as ComBinder.ComInvokeMemberBinder;
            if ((comInvokeBinder != null) && (comInvokeBinder.IsPropertySet))
                DynamicMetaObject value = args[args.Length - 1];
                if (!_self.TryGetPropertySetter(binder.Name, out method, value.LimitType, holdsNull))
                    _self.TryGetPropertySetterExplicit(binder.Name, out method, value.LimitType, holdsNull);
            // Otherwise, try property get
                if (!_self.TryGetMemberMethod(binder.Name, out method))
                    _self.TryGetMemberMethodExplicit(binder.Name, out method);
                return BindComInvoke(args, method, binder.CallInfo, isByRef, temps, initTemps);
            return base.BindInvokeMember(binder, args);
            if (_self.TryGetGetItem(out ComMethodDesc method))
            return base.BindInvoke(binder, args);
        private DynamicMetaObject BindComInvoke(DynamicMetaObject[] args, ComMethodDesc method, CallInfo callInfo, bool[] isByRef,
                IDispatchRestriction(),
                    Helpers.Convert(Expression, typeof(IDispatchComObject)),
                    typeof(IDispatchComObject).GetProperty(nameof(IDispatchComObject.DispatchObject))
            ComBinder.ComGetMemberBinder comBinder = binder as ComBinder.ComGetMemberBinder;
            bool canReturnCallables = comBinder?._canReturnCallables ?? false;
            // 1. Try methods
            if (_self.TryGetMemberMethod(binder.Name, out ComMethodDesc method))
                if (((method.InvokeKind & INVOKEKIND.INVOKE_PROPERTYGET) ==
                    INVOKEKIND.INVOKE_PROPERTYGET) &&
                    (method.ParamCount == 0))
                    return BindGetMember(method, canReturnCallables);
            // 2. Try events
            if (_self.TryGetMemberEvent(binder.Name, out ComEventDesc @event))
                return BindEvent(@event);
            // 3. Try methods explicitly by name
            if (_self.TryGetMemberMethodExplicit(binder.Name, out method))
            // 4. Fallback
            return base.BindGetMember(binder);
        private DynamicMetaObject BindGetMember(ComMethodDesc method, bool canReturnCallables)
            if (method.IsDataMember)
                if (method.ParamCount == 0)
                    return BindComInvoke(DynamicMetaObject.EmptyMetaObjects, method, new CallInfo(0), Array.Empty<bool>(), null, null);
            // ComGetMemberBinder does not expect callables. Try to call always.
            if (!canReturnCallables)
                    typeof(ComRuntimeHelpers).GetMethod(nameof(ComRuntimeHelpers.CreateDispCallable)),
                    Expression.Constant(method)
                IDispatchRestriction()
        private DynamicMetaObject BindEvent(ComEventDesc eventDesc)
            // BoundDispEvent CreateComEvent(object rcw, Guid sourceIid, int dispid)
            Expression result =
                    typeof(ComRuntimeHelpers).GetMethod(nameof(ComRuntimeHelpers.CreateComEvent)),
                    Expression.Constant(eventDesc.SourceIID),
                    Expression.Constant(eventDesc.Dispid)
            if (_self.TryGetGetItem(out ComMethodDesc getItem))
                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(getItem, ref indexes, temps, initTemps);
                return BindComInvoke(indexes, getItem, binder.CallInfo, isByRef, temps, initTemps);
            return base.BindGetIndex(binder, indexes);
            if (_self.TryGetSetItem(out ComMethodDesc setItem))
                bool[] isByRef = ComBinderHelpers.ProcessArgumentsForCom(setItem, ref indexes, temps, initTemps);
                        setItem.Name,
                var result = BindComInvoke(indexes.AddLast(updatedValue), setItem, binder.CallInfo, isByRef, temps, initTemps);
                // 1. Check for simple property put
                TryPropertyPut(binder, value) ??
                // 2. Check for event handler hookup where the put is dropped
                TryEventHandlerNoop(binder, value) ??
                // 3. Fallback
                base.BindSetMember(binder, value);
        private DynamicMetaObject TryPropertyPut(SetMemberBinder binder, DynamicMetaObject value)
            if (_self.TryGetPropertySetter(binder.Name, out ComMethodDesc method, value.LimitType, holdsNull) ||
                _self.TryGetPropertySetterExplicit(binder.Name, out method, value.LimitType, holdsNull))
                BindingRestrictions restrictions = IDispatchRestriction();
                Expression dispatch =
                DynamicMetaObject result = new ComInvokeBinder(
                    new CallInfo(1),
                    new[] { value },
                    new bool[] { false },
                    dispatch,
        private DynamicMetaObject TryEventHandlerNoop(SetMemberBinder binder, DynamicMetaObject value)
            if (_self.TryGetMemberEvent(binder.Name, out _) && value.LimitType == typeof(BoundDispEvent))
                // Drop the event property set.
                    value.Restrictions.Merge(IDispatchRestriction()).Merge(BindingRestrictions.GetTypeRestriction(value.Expression, typeof(BoundDispEvent)))
        private BindingRestrictions IDispatchRestriction()
            return IDispatchRestriction(Expression, _self.ComTypeDesc);
        internal static BindingRestrictions IDispatchRestriction(Expression expr, ComTypeDesc typeDesc)
            return BindingRestrictions.GetTypeRestriction(
                expr, typeof(IDispatchComObject)
            ).Merge(
                    Expression.Equal(
                            Helpers.Convert(expr, typeof(IDispatchComObject)),
                            typeof(IDispatchComObject).GetProperty(nameof(IDispatchComObject.ComTypeDesc))
                        Expression.Constant(typeDesc)
        protected override ComUnwrappedMetaObject UnwrapSelf()
                _self.RuntimeCallableWrapper
