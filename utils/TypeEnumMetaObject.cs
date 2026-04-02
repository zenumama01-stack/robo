    internal class TypeEnumMetaObject : DynamicMetaObject
        private readonly ComTypeEnumDesc _desc;
        internal TypeEnumMetaObject(ComTypeEnumDesc desc, Expression expression)
            : base(expression, BindingRestrictions.Empty, desc)
            _desc = desc;
            if (_desc.HasMember(binder.Name))
                    // return (.bound $arg0).GetValue("<name>")
                    Expression.Constant(((ComTypeEnumDesc)Value).GetValue(binder.Name), typeof(object)),
                    EnumRestrictions()
            return _desc.GetMemberNames();
        private BindingRestrictions EnumRestrictions()
                Expression, typeof(ComTypeEnumDesc)
                // ((ComTypeEnumDesc)<arg>).TypeLib.Guid == <guid>
                                AstUtils.Convert(Expression, typeof(ComTypeEnumDesc)),
                                typeof(ComTypeDesc).GetProperty(nameof(ComTypeDesc.TypeLib))),
                            typeof(ComTypeLibDesc).GetProperty(nameof(ComTypeLibDesc.Guid))),
                        Expression.Constant(_desc.TypeLib.Guid)
                            typeof(ComTypeEnumDesc).GetProperty(nameof(ComTypeEnumDesc.TypeName))
                        Expression.Constant(_desc.TypeName)
