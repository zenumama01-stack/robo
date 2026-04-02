    /// PSHostPSSnapIn is a class for facilitating registry
    /// of necessary information for PowerShell host PSSnapin.
    public sealed class PSHostPSSnapIn : PSSnapIn
        public PSHostPSSnapIn()
                return RegistryStrings.HostMshSnapinName;
                return "HostMshSnapInResources,Vendor";
                return "This PSSnapIn contains cmdlets used by the MSH host.";
                return "HostMshSnapInResources,Description";
