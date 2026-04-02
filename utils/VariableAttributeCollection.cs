    /// A collection of the attributes on the PSVariable object.
    internal class PSVariableAttributeCollection : Collection<Attribute>
        /// Constructs a variable attribute collection attached to
        /// the specified variable. Whenever the attributes change
        /// the variable value is verified against the attribute.
        /// The variable that needs to be verified anytime an attribute
        /// changes.
        internal PSVariableAttributeCollection(PSVariable variable)
            _variable = variable;
        #region Collection overrides
        /// Ensures that the variable that the attribute is being added to is still
        /// valid after the attribute is added.
        /// The zero-based index at which <paramref name="item"/> should be inserted.
        /// The attribute being added to the collection.
        /// If the new attribute causes the variable to be in an invalid state.
        /// If the new attribute is an ArgumentTransformationAttribute and the transformation
        /// fails.
        protected override void InsertItem(int index, Attribute item)
            object variableValue = VerifyNewAttribute(item);
            _variable.SetValueRaw(variableValue, true);
        /// Ensures that the variable that the attribute is being set to is still
        /// valid after the attribute is set.
        /// The zero-based index at which <paramref name="item"/> should be set.
        /// The attribute being set in the collection.
        protected override void SetItem(int index, Attribute item)
        #endregion Collection overrides
        /// Ordinarily, the collection checks/converts the value (by applying the attribute)
        /// when an attribute is added.  This is both slow and wrong when the attributes
        /// have already been checked/applied during parameter binding.  So if checking
        /// has already been done, this function will add the attribute without checking
        /// and possibly updating the value.
        /// <param name="item">The attribute to add.</param>
        internal void AddAttributeNoCheck(Attribute item)
            base.InsertItem(this.Count, item);
        /// Validates and performs any transformations that the new attribute
        /// implements.
        /// The new attribute to be added to the collection.
        /// The new variable value. This may change from the original value if the
        /// new attribute is an ArgumentTransformationAttribute.
        private object VerifyNewAttribute(Attribute item)
            object variableValue = _variable.Value;
            // Perform transformation before validating
            ArgumentTransformationAttribute argumentTransformation = item as ArgumentTransformationAttribute;
            if (argumentTransformation != null)
                variableValue = argumentTransformation.TransformInternal(engine, variableValue);
            if (!PSVariable.IsValidValue(variableValue, item))
                    Metadata.InvalidMetadataForCurrentValue,
                    _variable.Name,
                    ((_variable.Value != null) ? _variable.Value.ToString() : string.Empty));
            return variableValue;
        /// The variable whose value needs to be verified anytime
        /// the attributes change.
        private readonly PSVariable _variable;
