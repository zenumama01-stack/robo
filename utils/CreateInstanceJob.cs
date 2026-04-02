    /// Job wrapping invocation of a CreateInstance intrinsic CIM method.
    internal sealed class CreateInstanceJob : PropertySettingJob<CimInstance>
        private CimInstance _resultFromCreateInstance;
        private CimInstance _resultFromGetInstance;
        private static CimInstance GetEmptyInstance(CimJobContext jobContext)
            var result = new CimInstance(jobContext.ClassName, jobContext.Namespace);
        internal CreateInstanceJob(CimJobContext jobContext, MethodInvocationInfo methodInvocationInfo)
                : base(jobContext, true /* passThru */, GetEmptyInstance(jobContext), methodInvocationInfo)
        private IObservable<CimInstance> GetCreateInstanceOperation()
            CimInstance instanceToCreate = GetEmptyInstance(JobContext);
            ModifyLocalCimInstance(instanceToCreate);
            IObservable<CimInstance> observable = this.JobContext.Session.CreateInstanceAsync(
                this.JobContext.Namespace,
                instanceToCreate,
                this.CreateOperationOptions());
            return observable;
        private IObservable<CimInstance> GetGetInstanceOperation()
            Dbg.Assert(_resultFromCreateInstance != null, "GetInstance should only be called after CreteInstance came back with a keyed instance");
            IObservable<CimInstance> observable = this.JobContext.Session.GetInstanceAsync(
                _resultFromCreateInstance,
        private bool _createInstanceOperationGotStarted;
        private bool _getInstanceOperationGotStarted;
        internal override IObservable<CimInstance> GetCimOperation()
            if (_resultFromCreateInstance == null)
                if (!this.ShouldProcess())
                Dbg.Assert(!_getInstanceOperationGotStarted, "CreateInstance should be started *before* GetInstance");
                Dbg.Assert(!_createInstanceOperationGotStarted, "Should not start CreateInstance operation twice");
                _createInstanceOperationGotStarted = true;
                return GetCreateInstanceOperation();
                Dbg.Assert(_createInstanceOperationGotStarted, "GetInstance should be started *after* CreateInstance");
                Dbg.Assert(!_getInstanceOperationGotStarted, "Should not start GetInstance operation twice");
                Dbg.Assert(_resultFromGetInstance == null, "GetInstance operation shouldn't happen twice");
                _getInstanceOperationGotStarted = true;
                return GetGetInstanceOperation();
        public override void OnNext(CimInstance item)
            Dbg.Assert(item != null, "CreateInstance and GetInstance should never return null");
                _resultFromCreateInstance = item;
                _resultFromGetInstance = item;
        public override void OnError(Exception exception)
            if (this.DidUserSuppressTheOperation)
                // If user suppressed CreateInstance operation, then no instance should be returned by the cmdlet
                // If the provider's CreateInstance implementation doesn't post an instance and returns a success, then WMI infra will error out to flag an incorrect implementation of CreateInstance (by design)
                // Therefore cmdletization layer has to suppress the error and treat this as normal/successful completion
                this.OnCompleted();
                base.OnError(exception);
        public override void OnCompleted()
            Dbg.Assert(this.DidUserSuppressTheOperation || (_resultFromCreateInstance != null), "OnNext should always be called before OnComplete by CreateInstance");
            Dbg.Assert(
                !_getInstanceOperationGotStarted || this.DidUserSuppressTheOperation || (_resultFromGetInstance != null),
                // <=> (this._getInstanceOperationGotStarted => (this._resultFromGetInstance != null))
                "GetInstance should cause OnNext to be called which should set this._resultFromGetInstance to non-null");
            if (this.IsPassThruObjectNeeded() && (_resultFromGetInstance == null))
                IObservable<CimInstance> observable = this.GetGetInstanceOperation();
                observable.Subscribe(this);
                base.OnCompleted();
        internal override object PassThruObject
                return _resultFromGetInstance;
