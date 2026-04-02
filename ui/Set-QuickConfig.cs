    #region Set-WsManQuickConfig
    /// Performs configuration actions to enable the local machine for remote
    /// management. Steps include:
    /// 1. Check if WinRM service is running. If not start the WinRM service
    /// 2. Set the WinRM service type to auto start
    /// 3. Create a listener to accept request on any IP address. By default
    /// transport is http
    /// 4. Enable firewall exception for WS-Management traffic.
    [Cmdlet(VerbsCommon.Set, "WSManQuickConfig", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097112")]
    public class SetWSManQuickConfigCommand : PSCmdlet, IDisposable
        /// Indicates a https listener to be created. If this switch is not specified
        /// then by default a http listener will be created.
        /// Property that sets force parameter. This will allow
        /// configuring WinRM without prompting the user.
        /// Property that will allow configuring WinRM with Public profile exception enabled.
        public SwitchParameter SkipNetworkProfileCheck
            get { return skipNetworkProfileCheck; }
            set { skipNetworkProfileCheck = value; }
        private bool skipNetworkProfileCheck = false;
            string query = helper.GetResourceMsgFromResourcetext("QuickConfigContinueQuery");
            string caption = helper.GetResourceMsgFromResourcetext("QuickConfigContinueCaption");
            QuickConfigRemoting(true);
            QuickConfigRemoting(false);
        private void QuickConfigRemoting(bool serviceonly)
                string transport;
                string xpathEnabled = string.Empty;
                string xpathText = string.Empty;
                string xpathUpdate = string.Empty;
                string analysisInputXml = string.Empty;
                string action = string.Empty;
                string xpathStatus = string.Empty;
                string xpathResult = string.Empty;
                if (!usessl)
                    transport = "http";
                    transport = "https";
                if (serviceonly)
                    analysisInputXml = @"<AnalyzeService_INPUT xmlns=""http://schemas.microsoft.com/wbem/wsman/1/config/service""></AnalyzeService_INPUT>";
                    action = "AnalyzeService";
                    string openAllProfiles = skipNetworkProfileCheck ? "<Force/>" : string.Empty;
                    analysisInputXml = @"<Analyze_INPUT xmlns=""http://schemas.microsoft.com/wbem/wsman/1/config/service""><Transport>" + transport + "</Transport>" + openAllProfiles + "</Analyze_INPUT>";
                    action = "Analyze";
                string analysisOutputXml = m_SessionObj.Invoke(action, "winrm/config/service", analysisInputXml, 0);
                resultopxml.LoadXml(analysisOutputXml);
                    xpathEnabled = "/cfg:AnalyzeService_OUTPUT/cfg:RemotingEnabled";
                    xpathText = "/cfg:AnalyzeService_OUTPUT/cfg:Results";
                    xpathUpdate = "/cfg:AnalyzeService_OUTPUT/cfg:EnableService_INPUT";
                    xpathEnabled = "/cfg:Analyze_OUTPUT/cfg:RemotingEnabled";
                    xpathText = "/cfg:Analyze_OUTPUT/cfg:Results";
                    xpathUpdate = "/cfg:Analyze_OUTPUT/cfg:EnableRemoting_INPUT";
                nsmgr.AddNamespace("cfg", "http://schemas.microsoft.com/wbem/wsman/1/config/service");
                string enabled = resultopxml.SelectSingleNode(xpathEnabled, nsmgr).InnerText;
                XmlNode sourceAttribute = resultopxml.SelectSingleNode(xpathEnabled, nsmgr).Attributes.GetNamedItem("Source");
                string source = null;
                if (sourceAttribute != null)
                    source = sourceAttribute.Value;
                string rxml = string.Empty;
                if (enabled.Equals("true"))
                    string Err_Msg = string.Empty;
                        Err_Msg = WSManResourceLoader.GetResourceString("L_QuickConfigNoServiceChangesNeeded_Message");
                        Err_Msg = WSManResourceLoader.GetResourceString("L_QuickConfigNoChangesNeeded_Message");
                    //  ArgumentException e = new ArgumentException(Err_Msg);
                    // ErrorRecord er = new ErrorRecord(e, "InvalidOperation", ErrorCategory.InvalidOperation, null);
                    //  WriteError(er);
                    WriteObject(Err_Msg);
                if (!enabled.Equals("false"))
                    ArgumentException e = new ArgumentException(WSManResourceLoader.GetResourceString("L_QuickConfig_InvalidBool_0_ErrorMessage"));
                    ErrorRecord er = new ErrorRecord(e, "InvalidOperation", ErrorCategory.InvalidOperation, null);
                string resultAction = resultopxml.SelectSingleNode(xpathText, nsmgr).InnerText;
                if (source != null && source.Equals("GPO"))
                    string Info_Msg = WSManResourceLoader.GetResourceString("L_QuickConfig_RemotingDisabledbyGP_00_ErrorMessage");
                    Info_Msg += " " + resultAction;
                    ArgumentException e = new ArgumentException(Info_Msg);
                    WriteError(new ErrorRecord(e, "NotSpecified", ErrorCategory.NotSpecified, null));
                string inputXml = resultopxml.SelectSingleNode(xpathUpdate, nsmgr).OuterXml;
                if (resultAction.Equals(string.Empty) || inputXml.Equals(string.Empty))
                    ArgumentException e = new ArgumentException(WSManResourceLoader.GetResourceString("L_ERR_Message") + WSManResourceLoader.GetResourceString("L_QuickConfig_MissingUpdateXml_0_ErrorMessage"));
                    action = "EnableService";
                    action = "EnableRemoting";
                rxml = m_SessionObj.Invoke(action, "winrm/config/service", inputXml, 0);
                XmlDocument finalxml = new XmlDocument();
                finalxml.LoadXml(rxml);
                    xpathStatus = "/cfg:EnableService_OUTPUT/cfg:Status";
                    xpathResult = "/cfg:EnableService_OUTPUT/cfg:Results";
                    xpathStatus = "/cfg:EnableRemoting_OUTPUT/cfg:Status";
                    xpathResult = "/cfg:EnableRemoting_OUTPUT/cfg:Results";
                if (finalxml.SelectSingleNode(xpathStatus, nsmgr).InnerText.Equals("succeeded"))
                        WriteObject(WSManResourceLoader.GetResourceString("L_QuickConfigUpdatedService_Message"));
                        WriteObject(WSManResourceLoader.GetResourceString("L_QuickConfigUpdated_Message"));
                    WriteObject(finalxml.SelectSingleNode(xpathResult, nsmgr).InnerText);
                    helper.AssertError(WSManResourceLoader.GetResourceString("L_ERR_Message") + WSManResourceLoader.GetResourceString("L_QuickConfigUpdateFailed_ErrorMessage"), false, null);
    #endregion Set-WsManQuickConfig
