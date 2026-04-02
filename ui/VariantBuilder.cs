    /// VariantBuilder handles packaging of arguments into a Variant for a call to IDispatch.Invoke.
    internal class VariantBuilder
        private MemberExpression _variant;
        private readonly ArgBuilder _argBuilder;
        private readonly VarEnum _targetComType;
        internal ParameterExpression TempVariable { get; private set; }
        internal VariantBuilder(VarEnum targetComType, ArgBuilder builder)
            _targetComType = targetComType;
            _argBuilder = builder;
        internal bool IsByRef
            get { return (_targetComType & VarEnum.VT_BYREF) != 0; }
        internal Expression InitializeArgumentVariant(MemberExpression variant, Expression parameter)
            //NOTE: we must remember our variant
            //the reason is that argument order does not map exactly to the order of variants for invoke
            //and when we are doing clean-up we must be sure we are cleaning the variant we have initialized.
            _variant = variant;
            if (IsByRef)
                // temp = argument
                // paramVariants._elementN.SetAsByrefT(ref temp)
                Debug.Assert(TempVariable == null);
                Expression argExpr = _argBuilder.MarshalToRef(parameter);
                TempVariable = Expression.Variable(argExpr.Type, null);
                return Expression.Block(
                    Expression.Assign(TempVariable, argExpr),
                        variant,
                        Variant.GetByrefSetter(_targetComType & ~VarEnum.VT_BYREF),
                        TempVariable
            Expression argument = _argBuilder.Marshal(parameter);
            // we are forced to special case ConvertibleArgBuilder since it does not have
            // a corresponding _targetComType.
            if (_argBuilder is ConvertibleArgBuilder)
                    typeof(Variant).GetMethod(nameof(Variant.SetAsIConvertible)),
                    argument
            if (Variant.IsPrimitiveType(_targetComType) ||
               (_targetComType == VarEnum.VT_DISPATCH) ||
               (_targetComType == VarEnum.VT_UNKNOWN) ||
               (_targetComType == VarEnum.VT_VARIANT) ||
               (_targetComType == VarEnum.VT_RECORD) ||
               (_targetComType == VarEnum.VT_ARRAY))
                // paramVariants._elementN.AsT = (cast)argN
                return Expression.Assign(
                        Variant.GetAccessor(_targetComType)
            switch (_targetComType)
                    // paramVariants._elementN.SetAsNull();
                    return Expression.Call(variant, typeof(Variant).GetMethod(nameof(Variant.SetAsNULL)));
                    Debug.Assert(false, "Unexpected VarEnum");
        private static Expression Release(Expression pUnk)
            return Expression.Call(typeof(UnsafeMethods).GetMethod(nameof(UnsafeMethods.IUnknownReleaseNotZero)), pUnk);
        internal Expression Clear()
                if (_argBuilder is StringArgBuilder)
                    Debug.Assert(TempVariable != null);
                    return Expression.Call(typeof(Marshal).GetMethod(nameof(Marshal.FreeBSTR)), TempVariable);
                if (_argBuilder is DispatchArgBuilder)
                    return Release(TempVariable);
                if (_argBuilder is UnknownArgBuilder)
                if (_argBuilder is VariantArgBuilder)
                    return Expression.Call(TempVariable, typeof(Variant).GetMethod(nameof(Variant.Clear)));
                    // paramVariants._elementN.Clear()
                    return Expression.Call(_variant, typeof(Variant).GetMethod(nameof(Variant.Clear)));
                    Debug.Assert(Variant.IsPrimitiveType(_targetComType), "Unexpected VarEnum");
        internal Expression UpdateFromReturn(Expression parameter)
            if (TempVariable == null)
                    _argBuilder.UnmarshalFromRef(TempVariable),
                    parameter.Type
