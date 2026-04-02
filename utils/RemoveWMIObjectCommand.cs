    /// A command to Remove WMI Object.
    [Cmdlet(VerbsCommon.Remove, "WmiObject", DefaultParameterSetName = "class", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=113381", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class RemoveWmiObject : WmiBaseCmdlet
        [Parameter(Mandatory = true, ParameterSetName = "path")]
        /// Remove an object given either path,class name or pipeline input.
                RunAsJob("Remove-WMIObject");
                    if (!ShouldProcess(_inputObject["__PATH"].ToString()))
                    _inputObject.Delete();
                    ErrorRecord errorRecord = new ErrorRecord(e, "RemoveWMIManagementException", ErrorCategory.InvalidOperation, null);
                    ErrorRecord errorRecord = new ErrorRecord(e, "RemoveWMICOMException", ErrorCategory.InvalidOperation, null);
                            this.ComputerName));
                        if (!ShouldProcess(mObject["__PATH"].ToString()))
                        mObject.Delete();
