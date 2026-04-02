        /// Determines if an object is a COM object.
        /// <param name="value">The object to test.</param>
        /// <returns>True if the object is a COM object, false otherwise.</returns>
        public static bool IsComObject(object value)
            return value != null && Marshal.IsComObject(value);
        /// <param name="binder">An instance of the <see cref="GetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="instance">The target of the dynamic operation. </param>
        /// <param name="result">The new <see cref="DynamicMetaObject"/> representing the result of the binding.</param>
        /// <param name="delayInvocation">True if member evaluation may be delayed.</param>
        /// <returns>True if operation was bound successfully; otherwise, false.</returns>
            Requires.NotNull(binder);
            Requires.NotNull(instance);
            if (TryGetMetaObject(ref instance))
                var comGetMember = new ComGetMemberBinder(binder, delayInvocation);
                result = instance.BindGetMember(comGetMember);
                if (result.Expression.Type.IsValueType)
                    result = new DynamicMetaObject(
                        Expression.Convert(result.Expression, typeof(object)),
                        result.Restrictions
        /// <param name="binder">An instance of the <see cref="SetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="instance">The target of the dynamic operation.</param>
        /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value for the set member operation.</param>
            Requires.NotNull(value);
                result = instance.BindSetMember(binder, value);
                result = new DynamicMetaObject(result.Expression, result.Restrictions.Merge(value.PSGetMethodArgumentRestriction()));
        /// Tries to perform binding of the dynamic invoke operation.
        /// <param name="binder">An instance of the <see cref="InvokeBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="args">An array of <see cref="DynamicMetaObject"/> instances - arguments to the invoke member operation.</param>
        public static bool TryBindInvoke(InvokeBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, out DynamicMetaObject result)
            Requires.NotNull(args);
            if (TryGetMetaObjectInvoke(ref instance))
                result = instance.BindInvoke(binder, args);
        /// <param name="binder">An instance of the <see cref="InvokeMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="isSetProperty">True if this is for setting a property, false otherwise.</param>
                var comInvokeMember = new ComInvokeMemberBinder(binder, isSetProperty);
                result = instance.BindInvokeMember(comInvokeMember, args);
                BindingRestrictions argRestrictions = args.Aggregate(
                    BindingRestrictions.Empty, (current, arg) => current.Merge(arg.PSGetMethodArgumentRestriction()));
                var newRestrictions = result.Restrictions.Merge(argRestrictions);
                        newRestrictions
                    result = new DynamicMetaObject(result.Expression, newRestrictions);
        /// <param name="binder">An instance of the <see cref="GetIndexBinder"/> that represents the details of the dynamic operation.</param>
                result = instance.BindGetIndex(binder, args);
        /// <param name="binder">An instance of the <see cref="SetIndexBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value for the set index operation.</param>
                result = instance.BindSetIndex(binder, args, value);
        /// Tries to perform binding of the dynamic Convert operation.
        /// <param name="binder">An instance of the <see cref="ConvertBinder"/> that represents the details of the dynamic operation.</param>
        public static bool TryConvert(ConvertBinder binder, DynamicMetaObject instance, out DynamicMetaObject result)
            if (IsComObject(instance.Value))
                // Converting a COM object to any interface is always considered possible - it will result in
                // a QueryInterface at runtime
                if (binder.Type.IsInterface)
                            instance.Expression,
                            binder.Type
                        BindingRestrictions.GetExpressionRestriction(
                            Expression.Call(
                                typeof(ComBinder).GetMethod(nameof(ComBinder.IsComObject), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public),
                                Helpers.Convert(instance.Expression, typeof(object))
        /// Gets the member names of the data-like members associated with the object.
        /// This function can operate only with objects for which <see cref="IsComObject"/> returns true.
        /// <param name="value">The object for which member names are requested.</param>
        /// <returns>The collection of member names.</returns>
        internal static IList<string> GetDynamicDataMemberNames(object value)
            Requires.Condition(IsComObject(value), nameof(value));
            return ComObject.ObjectToComObject(value).GetMemberNames(true);
        /// Gets the data-like members and associated data for an object.
        /// <param name="value">The object for which data members are requested.</param>
        /// <param name="names">The enumeration of names of data members for which to retrieve values.</param>
        /// <returns>The collection of pairs that represent data member's names and their data.</returns>
        internal static IList<KeyValuePair<string, object>> GetDynamicDataMembers(object value, IEnumerable<string> names)
            return ComObject.ObjectToComObject(value).GetMembers(names);
        private static bool TryGetMetaObject(ref DynamicMetaObject instance)
            // If we're already a COM MO don't make a new one
            // (we do this to prevent recursion if we call Fallback from COM)
            if (instance is ComUnwrappedMetaObject)
                instance = new ComMetaObject(instance.Expression, instance.Restrictions, instance.Value);
        private static bool TryGetMetaObjectInvoke(ref DynamicMetaObject instance)
            if (instance.Value is IPseudoComObject o)
                instance = o.GetMetaObject(instance.Expression);
        /// Special binder that indicates special semantics for COM GetMember operation.
        internal class ComGetMemberBinder : GetMemberBinder
            private readonly GetMemberBinder _originalBinder;
            internal bool _canReturnCallables;
            internal ComGetMemberBinder(GetMemberBinder originalBinder, bool canReturnCallables)
             : base(originalBinder.Name, originalBinder.IgnoreCase)
                _originalBinder = originalBinder;
                _canReturnCallables = canReturnCallables;
            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
                return _originalBinder.FallbackGetMember(target, errorSuggestion);
                return _originalBinder.GetHashCode() ^ (_canReturnCallables ? 1 : 0);
                return obj is ComGetMemberBinder other
                    && _canReturnCallables == other._canReturnCallables
                    && _originalBinder.Equals(other._originalBinder);
        /// Special binder that indicates special semantics for COM InvokeMember operation.
        internal class ComInvokeMemberBinder : InvokeMemberBinder
            private readonly InvokeMemberBinder _originalBinder;
            internal bool IsPropertySet;
            internal ComInvokeMemberBinder(InvokeMemberBinder originalBinder, bool isPropertySet)
                : base(originalBinder.Name, originalBinder.IgnoreCase, originalBinder.CallInfo)
                this.IsPropertySet = isPropertySet;
            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
                return _originalBinder.FallbackInvoke(target, args, errorSuggestion);
            public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
                return _originalBinder.FallbackInvokeMember(target, args, errorSuggestion);
                return _originalBinder.GetHashCode() ^ (IsPropertySet ? 1 : 0);
                ComInvokeMemberBinder other = obj as ComInvokeMemberBinder;
                    IsPropertySet == other.IsPropertySet &&
                    _originalBinder.Equals(other._originalBinder);
