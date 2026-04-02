    #region SnapIn
    /// WsManPSSnapIn cmdlets. Declaring the PSSnapIn class identifies
    public class WSManPSSnapIn : PSSnapIn
        /// Create an instance of the WsManSnapin class.
        public WSManPSSnapIn()
                return "WsManPSSnapIn";
        /// Specify the vendor for the PowerShell snap-in.
        /// Specify the localization resource information for the vendor.
        /// Use the format: resourceBaseName,VendorName.
                return "WsManPSSnapIn,Microsoft";
        /// Specify a description of the PowerShell snap-in.
                return "This is a PowerShell snap-in that includes the WsMan cmdlets.";
        /// Specify the localization resource information for the description.
        /// Use the format: resourceBaseName,Description.
                return "WsManPSSnapIn,This is a PowerShell snap-in that includes the WsMan cmdlets.";
    #endregion SnapIn
