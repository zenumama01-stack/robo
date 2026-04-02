    /// Subscription result event args
    internal abstract class CimSubscriptionEventArgs : EventArgs
        protected object context;
    internal class CimSubscriptionResultEventArgs : CimSubscriptionEventArgs
        public CimSubscriptionResult Result { get; }
        /// Initializes a new instance of the <see cref="CimSubscriptionResultEventArgs"/> class.
        /// <param name="theResult"></param>
        public CimSubscriptionResultEventArgs(
            CimSubscriptionResult theResult)
            this.context = null;
            this.Result = theResult;
    internal class CimSubscriptionExceptionEventArgs : CimSubscriptionEventArgs
        /// Initializes a new instance of the <see cref="CimSubscriptionExceptionEventArgs"/> class.
        public CimSubscriptionExceptionEventArgs(
            Exception theException)
    /// Implements operations of register-cimindication cmdlet.
    internal sealed class CimRegisterCimIndication : CimAsyncOperation
        /// New subscription result event
        public event EventHandler<CimSubscriptionEventArgs> OnNewSubscriptionResult;
        /// Initializes a new instance of the <see cref="CimRegisterCimIndication"/> class.
        public CimRegisterCimIndication()
            this.ackedEvent = new ManualResetEventSlim(false);
        /// Start an indication subscription target to the given computer.
        /// <param name="computerName">Null stands for localhost.</param>
        /// <param name="queryDialect"></param>
        public void RegisterCimIndication(
            string nameSpace,
            DebugHelper.WriteLogEx("queryDialect = '{0}'; queryExpression = '{1}'", 0, queryDialect, queryExpression);
            this.TargetComputerName = computerName;
            CimSessionProxy proxy = CreateSessionProxy(computerName, operationTimeout);
            proxy.SubscribeAsync(nameSpace, queryDialect, queryExpression);
            WaitForAckMessage();
        /// Start an indication subscription through a given <see cref="CimSession"/>.
        /// <param name="cimSession">Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Throw if cimSession is null.</exception>
            ArgumentNullException.ThrowIfNull(cimSession, string.Format(CultureInfo.CurrentUICulture, CimCmdletStrings.NullArgument, nameof(cimSession)));
            this.TargetComputerName = cimSession.ComputerName;
            CimSessionProxy proxy = CreateSessionProxy(cimSession, operationTimeout);
        #region override methods
        protected override void SubscribeToCimSessionProxyEvent(CimSessionProxy proxy)
            DebugHelper.WriteLog("SubscribeToCimSessionProxyEvent", 4);
            // Raise event instead of write object to ps
            proxy.OnNewCmdletAction += this.CimIndicationHandler;
            proxy.EnableMethodResultStreaming = false;
        private void CimIndicationHandler(object cimSession, CmdletActionEventArgs actionArgs)
            DebugHelper.WriteLogEx("action is {0}. Disposed {1}", 0, actionArgs.Action, this.Disposed);
            // NOTES: should move after this.Disposed, but need to log the exception
            if (actionArgs.Action is CimWriteError cimWriteError)
                this.Exception = cimWriteError.Exception;
                if (!this.ackedEvent.IsSet)
                    // an exception happened
                    DebugHelper.WriteLogEx("an exception happened", 0);
                    this.ackedEvent.Set();
                EventHandler<CimSubscriptionEventArgs> temp = this.OnNewSubscriptionResult;
                    DebugHelper.WriteLog("Raise an exception event", 2);
                    temp(this, new CimSubscriptionExceptionEventArgs(this.Exception));
                DebugHelper.WriteLog("Got an exception: {0}", 2, Exception);
            if (actionArgs.Action is CimWriteResultObject cimWriteResultObject)
                if (cimWriteResultObject.Result is CimSubscriptionResult result)
                        DebugHelper.WriteLog("Raise an result event", 2);
                        temp(this, new CimSubscriptionResultEventArgs(result));
                        // an ACK message returned
                        DebugHelper.WriteLogEx("an ack message happened", 0);
                        DebugHelper.WriteLogEx("an ack message should not happen here", 0);
        /// Block the ps thread until ACK message or Error happened.
        private void WaitForAckMessage()
            this.ackedEvent.Wait();
            if (this.Exception != null)
                DebugHelper.WriteLogEx("error happened", 0);
                if (this.Cmdlet != null)
                    DebugHelper.WriteLogEx("Throw Terminating error", 1);
                    // throw terminating error
                    ErrorRecord errorRecord = ErrorToErrorRecord.ErrorRecordFromAnyException(
                        new InvocationContext(this.TargetComputerName, null), this.Exception, null);
                    this.Cmdlet.ThrowTerminatingError(errorRecord);
                    DebugHelper.WriteLogEx("Throw exception", 1);
                    // throw exception out
                    throw this.Exception;
            DebugHelper.WriteLogEx("ACK happened", 0);
        #region internal property
        /// The cmdlet object who issue this subscription,
        /// to throw ThrowTerminatingError
        internal Cmdlet Cmdlet
        /// Target computername.
        internal string TargetComputerName
        /// <param name="timeout"></param>
            uint timeout)
            proxy.OperationTimeout = timeout;
        /// Exception occurred while start the subscription.
        internal Exception Exception { get; private set; }
