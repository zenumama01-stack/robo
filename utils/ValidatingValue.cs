    /// The ValidatingValue class supports setting a value and validating the
    /// value.
    public class ValidatingValue<T> : ValidatingValueBase
        /// Initializes a new instance of the <see cref="ValidatingValue{T}"/> class.
        public ValidatingValue()
        public ValidatingValue(ValidatingValue<T> source)
            value = source.Value is IDeepCloneable deepClone ? deepClone.DeepClone() : source.Value;
        #region Value
        private const string ValuePropertyName = "Value";
        private object value;
        /// Gets or sets a value.
        public object Value
                return this.value;
                this.value = value;
                this.NotifyPropertyChanged(ValuePropertyName);
        #endregion Value
            return new ValidatingValue<T>(this);
        /// Gets the raw value cast/transformed into
        /// type T.
        /// The cast value.
        public T GetCastValue()
            if (!this.IsValid)
                throw new InvalidOperationException("Cannot return cast value when value is invalid");
            T castValue;
            if (!this.TryGetCastValue(this.Value, out castValue))
                throw new InvalidOperationException("Validation passed yet a cast value was not retrieved");
            return castValue;
        #region ForceValidationUpdate
        /// Forces a validation update to occur.
        /// The validation update occurs via signaling that
        /// the Value property has changed.
        public void ForceValidationUpdate()
            this.NotifyPropertyChanged("Value");
        #endregion ForceValidationUpdate
            return this.Validate(ValuePropertyName);
        /// <see cref="ValuePropertyName"/>.
            if (!columnName.Equals(ValuePropertyName, StringComparison.Ordinal))
                throw new ArgumentOutOfRangeException("columnName");
            if (this.IsValueEmpty())
                return new DataErrorInfoValidationResult(false, null, string.Empty);
                string errorMessage = FilterRuleCustomizationFactory.FactoryInstance.GetErrorMessageForInvalidValue(
                    this.Value.ToString(),
                    typeof(T));
                return new DataErrorInfoValidationResult(
                    false,
                    errorMessage);
            return this.EvaluateValidationRules(castValue, System.Globalization.CultureInfo.CurrentCulture);
        private bool IsValueEmpty()
            if (this.Value == null)
            string stringValue = this.Value.ToString();
            if (string.IsNullOrEmpty(stringValue))
        private bool TryGetCastValue(object rawValue, out T castValue)
            castValue = default(T);
            ArgumentNullException.ThrowIfNull(rawValue);
            if (typeof(T).IsEnum)
                return this.TryGetEnumValue(rawValue, out castValue);
                castValue = (T)Convert.ChangeType(rawValue, typeof(T), CultureInfo.CurrentCulture);
            catch (FormatException)
            catch (OverflowException)
        private bool TryGetEnumValue(object rawValue, out T castValue)
            Debug.Assert(rawValue != null, "rawValue not null");
            Debug.Assert(typeof(T).IsEnum, "is enum");
                castValue = (T)Enum.Parse(typeof(T), rawValue.ToString(), true);
            catch (ArgumentException)
