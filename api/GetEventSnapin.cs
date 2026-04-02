    /// Create the PowerShell snap-in used to register the
    /// Get-WinEvent cmdlet. Declaring the PSSnapIn class identifies
    /// this .cs file as a PowerShell snap-in.
    [RunInstaller(true)]
    public class GetEventPSSnapIn : PSSnapIn
        /// Create an instance of the GetEventPSSnapIn class.
        public GetEventPSSnapIn()
        /// Specify the name of the PowerShell snap-in.
        public override string Name
                return "Microsoft.Powershell.GetEvent";
        /// Specify the vendor of the PowerShell snap-in.
        public override string Vendor
                return "Microsoft";
        /// Get resource information for vendor. This is a string of format: resourceBaseName,resourceName.
        public override string VendorResource
                return "GetEventResources,Vendor";
        /// Specifies the description of the PowerShell snap-in.
        public override string Description
                return "This PS snap-in contains Get-WinEvent cmdlet used to read Windows event log data and configuration.";
        /// Get resource information for description. This is a string of format: resourceBaseName,resourceName.
        public override string DescriptionResource
                return "GetEventResources,Description";
        /// Get type files to be used for this PSSnapin.
        public override string[] Types
                return _types;
        private string[] _types = new string[] { "getevent.types.ps1xml" };
        /// Get format files to be used for this PSSnapin.
        public override string[] Formats
                return _formats;
        private string[] _formats = new string[] { "Event.format.ps1xml" };
