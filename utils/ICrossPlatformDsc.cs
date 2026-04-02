namespace System.Management.Automation.Subsystem.DSC
    /// Interface for implementing a cross platform desired state configuration component.
    public interface ICrossPlatformDsc : ISubsystem
        /// Default implementation. No function is required for this subsystem.
        Dictionary<string, string>? ISubsystem.FunctionsToDefine => null;
        /// DSC initializer function.
        void LoadDefaultKeywords(Collection<Exception> errors);
        /// Clear internal class caches.
        void ClearCache();
        /// Returns resource usage string.
        string GetDSCResourceUsageString(DynamicKeyword keyword);
        /// Checks if a string is one of dynamic keywords that can be used in both configuration and meta configuration.
        bool IsSystemResourceName(string name);
        /// Checks if a string matches default module name used for meta configuration resources.
        bool IsDefaultModuleNameForMetaConfigResource(string name);
