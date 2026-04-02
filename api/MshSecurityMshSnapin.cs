using System.Configuration.Install;
    /// PSSecurityPSSnapIn is a class for facilitating registry
    /// of necessary information for PowerShell security PSSnapin.
    public sealed class PSSecurityPSSnapIn : PSSnapIn
        public PSSecurityPSSnapIn()
                return RegistryStrings.SecurityMshSnapinName;
                return "SecurityMshSnapInResources,Vendor";
                return "This PSSnapIn contains cmdlets to manage MSH security.";
                return "SecurityMshSnapInResources,Description";
