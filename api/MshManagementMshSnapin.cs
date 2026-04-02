namespace Microsoft.PowerShell
    /// PSManagementPSSnapIn is a class for facilitating registry
    /// of necessary information for PowerShell management PSSnapin.
    /// This class will be built with monad management dll.
    public sealed class PSManagementPSSnapIn : PSSnapIn
        /// Create an instance of this class.
        public PSManagementPSSnapIn()
        /// Get name of this PSSnapin.
                return RegistryStrings.ManagementMshSnapinName;
        /// Get the default vendor string for this PSSnapin.
                return "ManagementMshSnapInResources,Vendor";
        /// Get the default description string for this PSSnapin.
                return "This PSSnapIn contains general management cmdlets used to manage Windows components.";
                return "ManagementMshSnapInResources,Description";
