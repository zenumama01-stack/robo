    internal class EtwActivityReverterMethodInvoker :
        IMethodInvoker
        private readonly IEtwEventCorrelator _eventCorrelator;
        private readonly Func<Guid, Delegate, object[], object> _invoker;
        public EtwActivityReverterMethodInvoker(IEtwEventCorrelator eventCorrelator)
            ArgumentNullException.ThrowIfNull(eventCorrelator);
            _eventCorrelator = eventCorrelator;
            _invoker = DoInvoke;
        public Delegate Invoker
            get { return _invoker; }
        public object[] CreateInvokerArgs(Delegate methodToInvoke, object[] methodToInvokeArgs)
            // See DoInvoke method for what these args mean.
            var retInvokerArgs = new object[]
                _eventCorrelator.CurrentActivityId,
                methodToInvoke,
                methodToInvokeArgs,
            return retInvokerArgs;
        private object DoInvoke(Guid relatedActivityId, Delegate method, object[] methodArgs)
            using (_eventCorrelator.StartActivity(relatedActivityId))
                return method.DynamicInvoke(methodArgs);
