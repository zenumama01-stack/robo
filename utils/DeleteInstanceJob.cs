    /// Job wrapping invocation of a DeleteInstance intrinsic CIM method.
    internal sealed class DeleteInstanceJob : MethodInvocationJobBase<object>
        private readonly CimInstance _objectToDelete;
        internal DeleteInstanceJob(CimJobContext jobContext, bool passThru, CimInstance objectToDelete, MethodInvocationInfo methodInvocationInfo)
                : base(
                    objectToDelete.ToString(),
                    methodInvocationInfo)
            Dbg.Assert(objectToDelete != null, "Caller should verify objectToDelete != null");
            _objectToDelete = objectToDelete;
        internal override IObservable<object> GetCimOperation()
            IObservable<object> observable = this.JobContext.Session.DeleteInstanceAsync(
                _objectToDelete,
        public override void OnNext(object item)
            Dbg.Assert(false, "DeleteInstance should not result in ObjectReady callbacks");
            get { return _objectToDelete; }
        internal override CimCustomOptionsDictionary CalculateJobSpecificCustomOptions()
            return CimCustomOptionsDictionary.MergeOptions(
                base.CalculateJobSpecificCustomOptions(),
                _objectToDelete);
