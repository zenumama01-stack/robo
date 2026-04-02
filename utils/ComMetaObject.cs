    // Note: we only need to support the operations used by ComBinder
    internal class ComMetaObject : DynamicMetaObject
        internal ComMetaObject(Expression expression, BindingRestrictions restrictions, object arg)
            return binder.Defer(args.AddFirst(WrapSelf()));
            return binder.Defer(WrapSelf());
            return binder.Defer(WrapSelf(), value);
            return binder.Defer(WrapSelf(), indexes);
            return binder.Defer(WrapSelf(), indexes.AddLast(value));
        private DynamicMetaObject WrapSelf()
                ComObject.RcwToComObject(Expression),
                        Helpers.Convert(Expression, typeof(object))
