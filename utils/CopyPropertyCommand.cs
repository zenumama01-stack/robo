    /// A command to copy a property on an item.
    [Cmdlet(VerbsCommon.Copy, "ItemProperty", DefaultParameterSetName = "Path", SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096589")]
    public class CopyItemPropertyCommand : PassThroughItemPropertyCommandBase
            get { return paths; }
            set { paths = value; }
        /// The name of the property to create on the item.
        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("PSProperty")]
        /// The path to the destination item to copy the property to.
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }
                return InvokeProvider.Property.CopyPropertyDynamicParameters(
                    Destination,
        /// Copies the property from one item to another.
                    InvokeProvider.Property.Copy(
                        GetCurrentContext());
