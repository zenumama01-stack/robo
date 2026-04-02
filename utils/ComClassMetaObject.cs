using AstUtils = System.Management.Automation.Interpreter.Utils;
    internal class ComClassMetaObject : DynamicMetaObject
        internal ComClassMetaObject(Expression expression, ComTypeClassDesc cls)
            : base(expression, BindingRestrictions.Empty, cls)
        public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
            return new DynamicMetaObject(
                    AstUtils.Convert(Expression, typeof(ComTypeClassDesc)),
                    typeof(ComTypeClassDesc).GetMethod(nameof(ComTypeClassDesc.CreateInstance))
                BindingRestrictions.Combine(args).Merge(
                    BindingRestrictions.GetTypeRestriction(Expression, typeof(ComTypeClassDesc))
