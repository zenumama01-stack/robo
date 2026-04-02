    /// This control is the row in the ListOrganizer and offers editing functionality.
    ///     PART_DeleteButton - A required template part which must be of type Button.  Button which keeps track of whether the row should be deleted.
    ///     PART_EditBox - A required template part which must be of type TextBox.  Displays the text content in an editable manner.
    ///     PART_LinkButton - A required template part which must be of type Button.  Displays the text content in a read-only manner and allows single click selection.
    ///     PART_RenameButton - A required template part which must be of type ToggleButton.  Button which allows for editing the name of the item.
    [TemplatePart(Name="PART_DeleteButton", Type=typeof(Button))]
    [TemplatePart(Name="PART_EditBox", Type=typeof(TextBox))]
    [TemplatePart(Name="PART_LinkButton", Type=typeof(Button))]
    [TemplatePart(Name="PART_RenameButton", Type=typeof(ToggleButton))]
    partial class ListOrganizerItem
        private Button deleteButton;
        private TextBox editBox;
        private Button linkButton;
        private ToggleButton renameButton;
        public static readonly DependencyProperty TextContentPropertyNameProperty = DependencyProperty.Register( "TextContentPropertyName", typeof(string), typeof(ListOrganizerItem), new PropertyMetadata( string.Empty, TextContentPropertyNameProperty_PropertyChanged) );
            ListOrganizerItem obj = (ListOrganizerItem) o;
            this.deleteButton = WpfHelp.GetTemplateChild<Button>(this,"PART_DeleteButton");
            this.editBox = WpfHelp.GetTemplateChild<TextBox>(this,"PART_EditBox");
            this.linkButton = WpfHelp.GetTemplateChild<Button>(this,"PART_LinkButton");
            this.renameButton = WpfHelp.GetTemplateChild<ToggleButton>(this,"PART_RenameButton");
        static ListOrganizerItem()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListOrganizerItem), new FrameworkPropertyMetadata(typeof(ListOrganizerItem)));
