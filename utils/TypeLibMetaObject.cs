    internal class TypeLibMetaObject : DynamicMetaObject
        private readonly ComTypeLibDesc _lib;
        internal TypeLibMetaObject(Expression expression, ComTypeLibDesc lib)
            : base(expression, BindingRestrictions.Empty, lib)
            _lib = lib;
        private DynamicMetaObject TryBindGetMember(string name)
            if (_lib.HasMember(name))
                BindingRestrictions restrictions =
                    BindingRestrictions.GetTypeRestriction(
                        Expression, typeof(ComTypeLibDesc)
                                    AstUtils.Convert(
                                    typeof(ComTypeLibDesc).GetProperty(nameof(ComTypeLibDesc.Guid))
                                Expression.Constant(_lib.Guid)
                    Expression.Constant(
                        ((ComTypeLibDesc)Value).GetTypeLibObjectDesc(name)
                    restrictions
            return TryBindGetMember(binder.Name) ?? base.BindGetMember(binder);
            DynamicMetaObject result = TryBindGetMember(binder.Name);
                return binder.FallbackInvoke(result, args, null);
            return _lib.GetMemberNames();
