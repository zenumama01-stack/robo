    /// Job wrapping invocation of a ModifyInstance intrinsic CIM method.
    internal sealed class ModifyInstanceJob : PropertySettingJob<CimInstance>
        private CimInstance _resultFromModifyInstance;
        private bool _resultFromModifyInstanceHasBeenPassedThru;
        private readonly CimInstance _originalInstance;
        private CimInstance _temporaryInstance;
        internal ModifyInstanceJob(CimJobContext jobContext, bool passThru, CimInstance managementObject, MethodInvocationInfo methodInvocationInfo)
                : base(jobContext, passThru, managementObject, methodInvocationInfo)
            Dbg.Assert(this.MethodSubject != null, "Caller should verify managementObject != null");
            _originalInstance = managementObject;
            _temporaryInstance = new CimInstance(_originalInstance);
            ModifyLocalCimInstance(_temporaryInstance);
            IObservable<CimInstance> observable = this.JobContext.Session.ModifyInstanceAsync(
                _temporaryInstance,
            Dbg.Assert(item != null, "ModifyInstance and GetInstance should not return a null instance");
            _resultFromModifyInstance = item;
            Dbg.Assert(_resultFromModifyInstance != null, "ModifyInstance should return an instance over DCOM and WSMan");
            ModifyLocalCimInstance(_originalInstance); /* modify input CimInstance only upon success (fix for bug WinBlue #) */
                if (IsShowComputerNameMarkerPresent(_originalInstance))
                    PSObject pso = PSObject.AsPSObject(_resultFromModifyInstance);
                _resultFromModifyInstanceHasBeenPassedThru = true;
                return _resultFromModifyInstance;
                _originalInstance);
        protected override void Dispose(bool disposing)
            if (!_resultFromModifyInstanceHasBeenPassedThru && _resultFromModifyInstance != null)
                _resultFromModifyInstance.Dispose();
                _resultFromModifyInstance = null;
            if (_temporaryInstance != null)
                _temporaryInstance.Dispose();
                _temporaryInstance = null;
            base.Dispose(disposing);
