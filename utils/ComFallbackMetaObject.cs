    // ComFallbackMetaObject just delegates everything to the binder.
    // Note that before performing FallBack on a ComObject we need to unwrap it so that
    // binder would act upon the actual object (typically Rcw)
    // Also: we don't need to implement these for any operations other than those
    // supported by ComBinder
    internal class ComFallbackMetaObject : DynamicMetaObject
        internal ComFallbackMetaObject(Expression expression, BindingRestrictions restrictions, object arg)
            : base(expression, restrictions, arg)
            return binder.FallbackGetIndex(UnwrapSelf(), indexes);
            return binder.FallbackSetIndex(UnwrapSelf(), indexes, value);
            return binder.FallbackGetMember(UnwrapSelf());
            return binder.FallbackInvokeMember(UnwrapSelf(), args);
            return binder.FallbackSetMember(UnwrapSelf(), value);
        protected virtual ComUnwrappedMetaObject UnwrapSelf()
            return new ComUnwrappedMetaObject(
                ComObject.RcwFromComObject(Expression),
                Restrictions.Merge(ComBinderHelpers.GetTypeRestrictionForDynamicMetaObject(this)),
                ((ComObject)Value).RuntimeCallableWrapper
    // This type exists as a signal type, so ComBinder knows not to try to bind
    // again when we're trying to fall back
    internal sealed class ComUnwrappedMetaObject : DynamicMetaObject
        internal ComUnwrappedMetaObject(Expression expression, BindingRestrictions restrictions, object value)
            : base(expression, restrictions, value)
