    /// The ValidatingSelectorValue class provides support for selecting
    /// a value from a collection of available values.
    /// The generic parameter.
    public class ValidatingSelectorValue<T> : ValidatingValueBase
        /// Initializes a new instance of the <see cref="ValidatingSelectorValue{T}"/> class.
        public ValidatingSelectorValue()
        /// <param name="source">The source to initialize from.</param>
        public ValidatingSelectorValue(ValidatingSelectorValue<T> source)
            : base(source)
            availableValues.EnsureCapacity(source.availableValues.Count);
            if (typeof(IDeepCloneable).IsAssignableFrom(typeof(T)))
                foreach (var value in source.availableValues)
                    availableValues.Add((T)((IDeepCloneable)value).DeepClone());
                availableValues.AddRange(source.availableValues);
            selectedIndex = source.selectedIndex;
            displayNameConverter = source.displayNameConverter;
        #region Consts
        private static readonly DataErrorInfoValidationResult InvalidSelectionResult = new DataErrorInfoValidationResult(false, null, UICultureResources.ValidatingSelectorValueOutOfBounds);
        #endregion Consts
        #region AvailableValues
        private List<T> availableValues = new List<T>();
        /// Gets the collection of values available for selection.
        public IList<T> AvailableValues
                return this.availableValues;
        #endregion AvailableValues
        #region SelectedIndex
        private const string SelectedIndexPropertyName = "SelectedIndex";
        private int selectedIndex;
        /// Gets or sets the index of the currently selected item or
        /// returns negative one (-1) if the selection is empty.
        /// If you set SelectedIndex to a value less that -1, an
        /// ArgumentException is thrown. If you set SelectedIndex to a
        /// value equal or greater than the number of child elements,
        /// the value is ignored.
        public int SelectedIndex
                return this.IsIndexWithinBounds(this.selectedIndex) ? this.selectedIndex : -1;
                if (value < -1)
                    throw new ArgumentException("value out of range", "value");
                if (value < this.availableValues.Count)
                    var oldValue = this.selectedIndex;
                    this.selectedIndex = value;
                    this.InvalidateValidationResult();
                    this.NotifySelectedValueChanged(oldValue, this.selectedIndex);
                    this.NotifyPropertyChanged(SelectedIndexPropertyName);
                    this.NotifyPropertyChanged(SelectedValuePropertyName);
        #endregion SelectedIndex
        #region SelectedValue
        private const string SelectedValuePropertyName = "SelectedValue";
        /// Gets the item within AvailableValues at the offset indicated
        /// by SelectedIndex or returns default(T) if the selection is empty.
        public T SelectedValue
                if (!this.IsIndexWithinBounds(this.SelectedIndex))
                return this.availableValues[this.SelectedIndex];
        #endregion SelectedValue
        #region DisplayNameConverter
        private IValueConverter displayNameConverter;
        /// Gets or sets the converter used to display a friendly
        /// value to the user.
        public IValueConverter DisplayNameConverter
                return this.displayNameConverter;
                this.displayNameConverter = value;
        #endregion DisplayNameConverter
        /// Notifies listeners that the selected value has changed.
        public event EventHandler<PropertyChangedEventArgs<T>> SelectedValueChanged;
        /// <inheritdoc cref="IDeepCloneable.DeepClone()" />
        public override object DeepClone()
            return new ValidatingSelectorValue<T>(this);
        #region Validate
        /// Called to validate the entire object.
        /// Returns a DataErrorInfoValidationResult which indicates the validation state
        /// of the object.
        protected override DataErrorInfoValidationResult Validate()
            return this.Validate(SelectedIndexPropertyName);
        /// Called to validate the property with the given name.
        /// <param name="columnName">
        /// The name of the property whose error message will be checked.
        /// Returns a DataErrorInfoValidationResult which indicates
        /// the validation state of the property.
        /// <exception cref="ArgumentException">
        /// <paramref name="columnName"/> may only be
        /// <see cref="SelectedIndexPropertyName"/>.
        /// </exception>
        protected override DataErrorInfoValidationResult Validate(string columnName)
            if (!columnName.Equals(SelectedIndexPropertyName, StringComparison.CurrentCulture))
                throw new ArgumentException(string.Create(CultureInfo.CurrentCulture, $"{columnName} is not a valid column name."), "columnName");
                return InvalidSelectionResult;
            return this.EvaluateValidationRules(this.SelectedValue, System.Globalization.CultureInfo.CurrentCulture);
        #endregion Validate
        #region NotifySelectedValueChanged
        /// Notifies listeners that the selected value with the available
        /// values has changed.
        /// <param name="oldValue">
        /// The previous selected value.
        /// <param name="newValue">
        /// The current selected value.
        protected void NotifySelectedValueChanged(T oldValue, T newValue)
            EventHandler<PropertyChangedEventArgs<T>> eh = this.SelectedValueChanged;
                eh(this, new PropertyChangedEventArgs<T>(oldValue, newValue));
        #endregion NotifySelectedValueChanged
        #region IsIndexWithinBounds
        private bool IsIndexWithinBounds(int value)
            return value >= 0 && value < this.AvailableValues.Count;
        #endregion IsIndexWithinBounds
        private void NotifySelectedValueChanged(int oldIndex, int newIndex)
            if (this.IsIndexWithinBounds(oldIndex) && this.IsIndexWithinBounds(newIndex))
                this.NotifySelectedValueChanged(this.availableValues[oldIndex], this.availableValues[newIndex]);
