    /// Job that handles executing a WQL (in the future CQL?) query on a remote CIM server.
    internal sealed class EnumerateAssociatedInstancesJob : QueryJobBase
        private readonly CimInstance _associatedObject;
        private readonly string _associationName;
        private readonly string _resultRole;
        private readonly string _sourceRole;
        internal EnumerateAssociatedInstancesJob(CimJobContext jobContext, CimQuery cimQuery, CimInstance associatedObject, string associationName, string resultRole, string sourceRole)
                : base(jobContext, cimQuery)
            _associatedObject = associatedObject;
            Dbg.Assert(_associatedObject != null, "Caller should verify that associatedObject is not null");
            _associationName = associationName;
            Dbg.Assert(_associationName != null, "Caller should verify that associationName is not null");
            _resultRole = resultRole;
            Dbg.Assert(_resultRole != null, "Caller should verify that resultRole is not null");
            _sourceRole = sourceRole;
            Dbg.Assert(_sourceRole != null, "Caller should verify that sourceRole is not null");
            this.WriteVerboseStartOfCimOperation();
            IObservable<CimInstance> observable = this.JobContext.Session.EnumerateAssociatedInstancesAsync(
                _associatedObject,
                _associationName,
                this.JobContext.ClassNameOrNullIfResourceUriIsUsed,
                _sourceRole,
                _resultRole,
        internal override string Description
                    CmdletizationResources.CimJob_AssociationDescription,
                    this.JobContext.CmdletizationClassName,
                    this.JobContext.Session.ComputerName,
                    _associatedObject.ToString());
        internal override string FailSafeDescription
                    CmdletizationResources.CimJob_SafeAssociationDescription,
                    this.JobContext.Session.ComputerName);
                _associatedObject);
        internal override void WriteObject(object outputObject)
            if (IsShowComputerNameMarkerPresent(_associatedObject))
                PSObject pso = PSObject.AsPSObject(outputObject);
                AddShowComputerNameMarker(pso);
            base.WriteObject(outputObject);
        internal override string GetProviderVersionExpectedByJob()
            // CDXML doesn't allow expressing of separate "ClassVersion" attribute for association operations - Windows 8 Bugs: #642140
