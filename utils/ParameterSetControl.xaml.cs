using Microsoft.PowerShell.Commands.ShowCommandExtension;
    /// Interaction logic for ParameterSetControl.xaml.
    public partial class ParameterSetControl : UserControl
        /// First focusable element in the generated UI.
        private UIElement firstFocusableElement;
        /// Field used for the CurrentParameterSetViewModel parameter.
        private ParameterSetViewModel currentParameterSetViewModel;
        /// Initializes a new instance of the ParameterSetControl class.
        public ParameterSetControl()
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.ParameterSetControl_DataContextChanged);
        /// Focuses the first focusable element in this control.
        public void FocusFirstElement()
            if (this.firstFocusableElement != null)
                this.firstFocusableElement.Focus();
        #region Private Property
        /// Gets current ParameterSetViewModel.
        private ParameterSetViewModel CurrentParameterSetViewModel
            get { return this.currentParameterSetViewModel; }
        /// Creates a CheckBox for switch parameters.
        /// <param name="parameterViewModel">DataContext object.</param>
        /// <param name="rowNumber">Row number.</param>
        /// <returns>a CheckBox for switch parameters.</returns>
        private static CheckBox CreateCheckBox(ParameterViewModel parameterViewModel, int rowNumber)
            CheckBox checkBox = new CheckBox();
            checkBox.SetBinding(Label.ContentProperty, new Binding("NameCheckLabel"));
            checkBox.DataContext = parameterViewModel;
            checkBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            checkBox.SetValue(Grid.ColumnProperty, 0);
            checkBox.SetValue(Grid.ColumnSpanProperty, 2);
            checkBox.SetValue(Grid.RowProperty, rowNumber);
            checkBox.IsThreeState = false;
            checkBox.Margin = new Thickness(8, rowNumber == 0 ? 7 : 5, 0, 5);
            checkBox.SetBinding(CheckBox.ToolTipProperty, new Binding("ToolTip"));
            checkBox.SetBinding(AutomationProperties.HelpTextProperty, new Binding("ToolTip"));
            Binding valueBinding = new Binding("Value");
            checkBox.SetBinding(CheckBox.IsCheckedProperty, valueBinding);
            //// Add AutomationProperties.AutomationId for Ui Automation test.
            checkBox.SetValue(
                System.Windows.Automation.AutomationProperties.AutomationIdProperty,
                string.Create(CultureInfo.CurrentCulture, $"chk{parameterViewModel.Name}"));
                System.Windows.Automation.AutomationProperties.NameProperty,
                parameterViewModel.Name);
            return checkBox;
        /// Creates a ComboBox control for input type field.
        /// <param name="itemsSource">Control data source.</param>
        /// <returns>Return a ComboBox control.</returns>
        private static ComboBox CreateComboBoxControl(ParameterViewModel parameterViewModel, int rowNumber, IEnumerable itemsSource)
            ComboBox comboBox = new ComboBox();
            comboBox.DataContext = parameterViewModel;
            comboBox.SetValue(Grid.ColumnProperty, 1);
            comboBox.SetValue(Grid.RowProperty, rowNumber);
            comboBox.Margin = new Thickness(2);
            comboBox.SetBinding(TextBox.ToolTipProperty, new Binding("ToolTip"));
            comboBox.ItemsSource = itemsSource;
            Binding selectedItemBinding = new Binding("Value");
            comboBox.SetBinding(ComboBox.SelectedItemProperty, selectedItemBinding);
            string automationId = string.Create(CultureInfo.CurrentCulture, $"combox{parameterViewModel.Name}");
            comboBox.SetValue(
                automationId);
            return comboBox;
        /// Creates a MultiSelectCombo control for input type field.
        /// <returns>Return a MultiSelectCombo control.</returns>
        private static MultipleSelectionControl CreateMultiSelectComboControl(ParameterViewModel parameterViewModel, int rowNumber, IEnumerable itemsSource)
            MultipleSelectionControl multiControls = new MultipleSelectionControl();
            multiControls.DataContext = parameterViewModel;
            multiControls.SetValue(Grid.ColumnProperty, 1);
            multiControls.SetValue(Grid.RowProperty, rowNumber);
            multiControls.Margin = new Thickness(2);
            multiControls.comboxParameter.ItemsSource = itemsSource;
            multiControls.SetBinding(TextBox.ToolTipProperty, new Binding("ToolTip"));
            valueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            multiControls.comboxParameter.SetBinding(ComboBox.TextProperty, valueBinding);
            // Add AutomationProperties.AutomationId for Ui Automation test.
            multiControls.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, string.Create(CultureInfo.CurrentCulture, $"combox{parameterViewModel.Name}"));
            multiControls.comboxParameter.SetValue(
            string buttonToolTipAndName = string.Format(
                CultureInfo.CurrentUICulture,
                ShowCommandResources.SelectMultipleValuesForParameterFormat,
            multiControls.multipleValueButton.SetValue(Button.ToolTipProperty, buttonToolTipAndName);
            multiControls.multipleValueButton.SetValue(
                buttonToolTipAndName);
            return multiControls;
        /// Creates a TextBox control for input type field.
        /// <returns>Return a TextBox control.</returns>
        private static TextBox CreateTextBoxControl(ParameterViewModel parameterViewModel, int rowNumber)
            TextBox textBox = new TextBox();
            textBox.DataContext = parameterViewModel;
            textBox.SetValue(Grid.ColumnProperty, 1);
            textBox.SetValue(Grid.RowProperty, rowNumber);
            textBox.Margin = new Thickness(2);
            textBox.SetBinding(TextBox.ToolTipProperty, new Binding("ToolTip"));
            textBox.SetBinding(TextBox.TextProperty, valueBinding);
            //// Add AutomationProperties.AutomationId for UI Automation test.
            textBox.SetValue(
                string.Create(CultureInfo.CurrentCulture, $"txt{parameterViewModel.Name}"));
            ShowCommandParameterType parameterType = parameterViewModel.Parameter.ParameterType;
            if (parameterType.IsArray)
                parameterType = parameterType.ElementType;
            if (parameterType.IsScriptBlock || parameterType.ImplementsDictionary)
                textBox.AcceptsReturn = true;
                textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBox.Loaded += ParameterSetControl.MultiLineTextBox_Loaded;
            return textBox;
        /// Called for a newly created multiline text box to increase its height and.
        private static void MultiLineTextBox_Loaded(object sender, RoutedEventArgs e)
            TextBox senderTextBox = (TextBox)sender;
            senderTextBox.Loaded -= ParameterSetControl.MultiLineTextBox_Loaded;
            // This will set the height to about 3 lines since the total height of the
            // TextBox is a bit greater than a line's height
            senderTextBox.Height = senderTextBox.ActualHeight * 2;
        #region Event Methods
        /// When user switch ParameterSet.It will trigger this event.
        /// This event method will renew generate all controls for current ParameterSet.
        private void ParameterSetControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            this.firstFocusableElement = null;
            this.MainGrid.Children.Clear();
            this.MainGrid.RowDefinitions.Clear();
            ParameterSetViewModel viewModel = e.NewValue as ParameterSetViewModel;
            this.currentParameterSetViewModel = viewModel;
            for (int rowNumber = 0; rowNumber < viewModel.Parameters.Count; rowNumber++)
                ParameterViewModel parameter = viewModel.Parameters[rowNumber];
                this.MainGrid.RowDefinitions.Add(this.CreateNewRow());
                if (parameter.Parameter.ParameterType.IsSwitch)
                    this.AddControlToMainGrid(ParameterSetControl.CreateCheckBox(parameter, rowNumber));
                    this.CreateAndAddLabel(parameter, rowNumber);
                    Control control = null;
                    if (parameter.Parameter.HasParameterSet)
                        // For ValidateSet parameter
                        ArrayList itemsSource = new ArrayList();
                        itemsSource.Add(string.Empty);
                        for (int i = 0; i < parameter.Parameter.ValidParamSetValues.Count; i++)
                            itemsSource.Add(parameter.Parameter.ValidParamSetValues[i]);
                        control = ParameterSetControl.CreateComboBoxControl(parameter, rowNumber, itemsSource);
                    else if (parameter.Parameter.ParameterType.IsEnum)
                        if (parameter.Parameter.ParameterType.HasFlagAttribute)
                            itemsSource.AddRange(parameter.Parameter.ParameterType.EnumValues);
                            control = ParameterSetControl.CreateMultiSelectComboControl(parameter, rowNumber, parameter.Parameter.ParameterType.EnumValues);
                    else if (parameter.Parameter.ParameterType.IsBoolean)
                        control = ParameterSetControl.CreateComboBoxControl(parameter, rowNumber, new string[] { string.Empty, "$True", "$False" });
                        // For input parameter
                        control = ParameterSetControl.CreateTextBoxControl(parameter, rowNumber);
                    if (control != null)
                        this.AddControlToMainGrid(control);
        /// When user trigger click on anyone CheckBox. Get value from sender.
        private void CheckBox_Click(object sender, RoutedEventArgs e)
            CheckBox senderCheck = (CheckBox)sender;
            ((ParameterViewModel)senderCheck.DataContext).Value = senderCheck.IsChecked.ToString();
        #region Private Method
        /// Creates a RowDefinition for MainGrid.
        /// <returns>Return a RowDefinition object.</returns>
        private RowDefinition CreateNewRow()
            RowDefinition row = new RowDefinition();
            row.Height = GridLength.Auto;
            return row;
        /// Adds a control to MainGrid;.
        /// <param name="uiControl">Will adding UIControl.</param>
        private void AddControlToMainGrid(UIElement uiControl)
            if (this.firstFocusableElement == null && uiControl is not Label)
                this.firstFocusableElement = uiControl;
            this.MainGrid.Children.Add(uiControl);
        /// Creates a Label control and add it to MainGrid.
        private void CreateAndAddLabel(ParameterViewModel parameterViewModel, int rowNumber)
            Label label = this.CreateLabel(parameterViewModel, rowNumber);
            this.AddControlToMainGrid(label);
        /// Creates a Label control for input type field.
        /// <returns>Return a Label control.</returns>
        private Label CreateLabel(ParameterViewModel parameterViewModel, int rowNumber)
            Label label = new Label();
            label.SetBinding(Label.ContentProperty, new Binding("NameTextLabel"));
            label.DataContext = parameterViewModel;
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            label.SetValue(Grid.ColumnProperty, 0);
            label.SetValue(Grid.RowProperty, rowNumber);
            label.Margin = new Thickness(2);
            label.SetBinding(Label.ToolTipProperty, new Binding("ToolTip"));
            label.SetValue(
                string.Create(CultureInfo.CurrentCulture, $"lbl{parameterViewModel.Name}"));
