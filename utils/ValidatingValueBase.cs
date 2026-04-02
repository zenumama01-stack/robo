    /// The ValidatingValueBase class provides basic services for base
    /// classes to support validation via the IDataErrorInfo interface.
    public abstract class ValidatingValueBase : IDataErrorInfo, INotifyPropertyChanged, IDeepCloneable
        /// Initializes a new instance of the <see cref="ValidatingValueBase"/> class.
        protected ValidatingValueBase()
        /// Initializes a new instance of the  <see cref="ValidatingValueBase"/> class.
        protected ValidatingValueBase(ValidatingValueBase source)
            ArgumentNullException.ThrowIfNull(source);
            validationRules.EnsureCapacity(source.validationRules.Count);
            foreach (var rule in source.validationRules)
                validationRules.Add((DataErrorInfoValidationRule)rule.DeepClone());
        #region ValidationRules
        private List<DataErrorInfoValidationRule> validationRules = new List<DataErrorInfoValidationRule>();
        private ReadOnlyCollection<DataErrorInfoValidationRule> readonlyValidationRules;
        private bool isValidationRulesCollectionDirty = true;
        private DataErrorInfoValidationResult cachedValidationResult;
        /// Gets the collection of validation rules used to validate the value.
        public ReadOnlyCollection<DataErrorInfoValidationRule> ValidationRules
                if (this.isValidationRulesCollectionDirty)
                    this.readonlyValidationRules = new ReadOnlyCollection<DataErrorInfoValidationRule>(this.validationRules);
                return this.readonlyValidationRules;
        #endregion ValidationRules
        #region IsValid
        /// Gets a value indicating whether the value is valid.
        public bool IsValid
                return this.GetValidationResult().IsValid;
        #endregion IsValid
        #region IDataErrorInfo implementation
        #region Item
        /// Gets the error message for the property with the given name.
        /// The error message for the property, or an empty string ("") if
        /// the property is valid.
        /// <paramref name="columnName"/> is invalid.
        public string this[string columnName]
                ArgumentException.ThrowIfNullOrEmpty(columnName);
                this.UpdateValidationResult(columnName);
                return this.GetValidationResult().ErrorMessage;
        #endregion Item
        #region Error
        /// Gets an error message indicating what is wrong with this object.
        public string Error
                DataErrorInfoValidationResult result = this.GetValidationResult();
                return (!result.IsValid) ? result.ErrorMessage : string.Empty;
        #endregion Error
        #endregion IDataErrorInfo implementation
        #region PropertyChanged
        /// Occurs when a property value changes.
        /// The listeners attached to this event are not serialized.
        #endregion PropertyChanged
        public abstract object DeepClone();
        #region AddValidationRule
        /// Adds a validation rule to the ValidationRules collection.
        /// <param name="rule">The validation rule to add.</param>
        public void AddValidationRule(DataErrorInfoValidationRule rule)
            this.validationRules.Add(rule);
            this.isValidationRulesCollectionDirty = true;
            this.NotifyPropertyChanged("ValidationRules");
        #endregion AddValidationRule
        #region RemoveValidationRule
        /// Removes a validation rule from the ValidationRules collection.
        /// <param name="rule">The rule to remove.</param>
        public void RemoveValidationRule(DataErrorInfoValidationRule rule)
            this.validationRules.Remove(rule);
        #endregion RemoveValidationRule
        #region ClearValidationRules
        /// Clears the ValidationRules collection.
        public void ClearValidationRules()
            this.validationRules.Clear();
        #endregion ClearValidationRules
        protected abstract DataErrorInfoValidationResult Validate();
        /// of the property.
        protected abstract DataErrorInfoValidationResult Validate(string propertyName);
        #region EvaluateValidationRules
        internal DataErrorInfoValidationResult EvaluateValidationRules(object value, System.Globalization.CultureInfo cultureInfo)
            foreach (DataErrorInfoValidationRule rule in this.ValidationRules)
                DataErrorInfoValidationResult result = rule.Validate(value, cultureInfo);
                if (result == null)
                    throw new InvalidOperationException(string.Create(CultureInfo.CurrentCulture, $"DataErrorInfoValidationResult not returned by ValidationRule: {rule}"));
                if (!result.IsValid)
            return DataErrorInfoValidationResult.ValidResult;
        #endregion EvaluateValidationRules
        #region InvalidateValidationResult
        /// Calling InvalidateValidationResult causes the
        /// Validation to be reevaluated.
        protected void InvalidateValidationResult()
            this.ClearValidationResult();
        #endregion InvalidateValidationResult
        #region GetValidationResult
        private DataErrorInfoValidationResult GetValidationResult()
            if (this.cachedValidationResult == null)
                this.UpdateValidationResult();
            return this.cachedValidationResult;
        #endregion GetValidationResult
        #region UpdateValidationResult
        private void UpdateValidationResult()
            this.cachedValidationResult = this.Validate();
            this.NotifyValidationResultUpdated();
        private void UpdateValidationResult(string columnName)
            this.cachedValidationResult = this.Validate(columnName);
        private void NotifyValidationResultUpdated()
            Debug.Assert(this.cachedValidationResult != null, "not null");
            this.NotifyPropertyChanged("IsValid");
            this.NotifyPropertyChanged("Error");
        #endregion UpdateValidationResult
        #region ClearValidationResult
        private void ClearValidationResult()
            this.cachedValidationResult = null;
        #endregion ClearValidationResult
