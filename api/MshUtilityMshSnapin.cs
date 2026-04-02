    /// PSUtilityPSSnapIn is a class for facilitating registry
    /// of necessary information for PowerShell utility PSSnapin.
    public sealed class PSUtilityPSSnapIn : PSSnapIn
        public PSUtilityPSSnapIn()
                return RegistryStrings.UtilityMshSnapinName;
                return "UtilityMshSnapInResources,Vendor";
                return "This PSSnapIn contains utility cmdlets used to manipulate data.";
                return "UtilityMshSnapInResources,Description";
