    internal sealed class InstanceMethodInvocationJob : ExtrinsicMethodInvocationJob
        private readonly CimInstance _targetInstance;
        internal InstanceMethodInvocationJob(CimJobContext jobContext, bool passThru, CimInstance targetInstance, MethodInvocationInfo methodInvocationInfo)
                    targetInstance.ToString(),
            Dbg.Assert(targetInstance != null, "Caller should verify targetInstance != null");
            _targetInstance = targetInstance;
        internal override IObservable<CimMethodResultBase> GetCimOperation()
            CimMethodParametersCollection methodParameters = this.GetCimMethodParametersCollection();
            CimOperationOptions operationOptions = this.CreateOperationOptions();
            operationOptions.EnableMethodResultStreaming = true;
            IObservable<CimMethodResultBase> observable = this.JobContext.Session.InvokeMethodAsync(
                _targetInstance,
                methodParameters,
                operationOptions);
            get { return _targetInstance; }
                _targetInstance);
