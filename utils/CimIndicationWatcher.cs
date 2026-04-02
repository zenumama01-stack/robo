using System.ComponentModel;
    /// Abstract Cimindication event args, which containing all elements related to
    /// an Cimindication.
    public abstract class CimIndicationEventArgs : EventArgs
        /// Returns an Object value for an operation context
        public object Context
                return context;
        internal object context;
    /// Cimindication exception event args, which containing occurred exception.
    public class CimIndicationEventExceptionEventArgs : CimIndicationEventArgs
        /// Returns an exception
        public Exception Exception { get; }
        /// Initializes a new instance of the <see cref="CimIndicationEventExceptionEventArgs"/> class.
        /// <param name="result"></param>
        public CimIndicationEventExceptionEventArgs(Exception theException)
            context = null;
            this.Exception = theException;
    /// Cimindication event args, which containing all elements related to
    public class CimIndicationEventInstanceEventArgs : CimIndicationEventArgs
        /// Get ciminstance of the indication object.
        public CimInstance NewEvent
                return result?.Instance;
        /// Get MachineId of the indication object.
        public string MachineId
                return result?.MachineId;
        /// Get BookMark of the indication object.
        public string Bookmark
                return result?.Bookmark;
        /// Initializes a new instance of the <see cref="CimIndicationEventInstanceEventArgs"/> class.
        public CimIndicationEventInstanceEventArgs(CimSubscriptionResult result)
            this.result = result;
        /// subscription result
        private readonly CimSubscriptionResult result;
    /// A public class used to start/stop the subscription to specific indication source,
    /// and listen to the incoming indications, event <see cref="CimIndicationArrived"/>
    /// will be raised for each cimindication.
    public class CimIndicationWatcher
        /// Status of <see cref="CimIndicationWatcher"/> object.
        internal enum Status
            Default,
            Started,
            Stopped
        /// CimIndication arrived event
        public event EventHandler<CimIndicationEventArgs> CimIndicationArrived;
        /// Initializes a new instance of the <see cref="CimIndicationWatcher"/> class.
        /// <param name="nameSpace"></param>
        /// <param name="queryExpression"></param>
        /// <param name="operationTimeout"></param>
        public CimIndicationWatcher(
            string theNamespace,
            string queryDialect,
            string queryExpression,
            uint operationTimeout)
            ValidationHelper.ValidateNoNullorWhiteSpaceArgument(queryExpression, queryExpressionParameterName);
            computerName = ConstValue.GetComputerName(computerName);
            theNamespace = ConstValue.GetNamespace(theNamespace);
            Initialize(computerName, null, theNamespace, queryDialect, queryExpression, operationTimeout);
        /// <param name="cimSession"></param>
            CimSession cimSession,
            ValidationHelper.ValidateNoNullArgument(cimSession, cimSessionParameterName);
            Initialize(null, cimSession, theNamespace, queryDialect, queryExpression, operationTimeout);
        /// Initialize
        private void Initialize(
            string theComputerName,
            CimSession theCimSession,
            string theNameSpace,
            string theQueryDialect,
            string theQueryExpression,
            uint theOperationTimeout)
            enableRaisingEvents = false;
            status = Status.Default;
            myLock = new object();
            cimRegisterCimIndication = new CimRegisterCimIndication();
            cimRegisterCimIndication.OnNewSubscriptionResult += NewSubscriptionResultHandler;
            this.cimSession = theCimSession;
            this.nameSpace = theNameSpace;
            this.queryDialect = ConstValue.GetQueryDialectWithDefault(theQueryDialect);
            this.queryExpression = theQueryExpression;
            this.operationTimeout = theOperationTimeout;
            this.computerName = theComputerName;
        /// Handler of new subscription result
        /// <param name="src"></param>
        /// <param name="args"></param>
        private void NewSubscriptionResultHandler(object src, CimSubscriptionEventArgs args)
            EventHandler<CimIndicationEventArgs> temp = this.CimIndicationArrived;
            if (temp != null)
                // raise the event
                if (args is CimSubscriptionResultEventArgs resultArgs)
                    temp(this, new CimIndicationEventInstanceEventArgs(resultArgs.Result));
                else if (args is CimSubscriptionExceptionEventArgs exceptionArgs)
                    temp(this, new CimIndicationEventExceptionEventArgs(exceptionArgs.Exception));
        /// Will be called by admin\monad\src\eengine\EventManager.cs:
        /// PSEventManager::ProcessNewSubscriber to start to listen to the Cim Indication.
        /// If set EnableRaisingEvents to false, which will be ignored
        [Browsable(false)]
        public bool EnableRaisingEvents
                return enableRaisingEvents;
                if (value && !enableRaisingEvents)
                    enableRaisingEvents = value;
                    Start();
        private bool enableRaisingEvents;
        /// Start the subscription
        public void Start()
            lock (myLock)
                if (status == Status.Default)
                    if (this.cimSession == null)
                        cimRegisterCimIndication.RegisterCimIndication(
                            this.computerName,
                            this.nameSpace,
                            this.queryDialect,
                            this.queryExpression,
                            this.operationTimeout);
                            this.cimSession,
                    status = Status.Started;
        /// Unsubscribe the subscription
        public void Stop()
            DebugHelper.WriteLogEx("Status = {0}", 0, this.status);
                if (status == Status.Started)
                    if (this.cimRegisterCimIndication != null)
                        DebugHelper.WriteLog("Dispose CimRegisterCimIndication object", 4);
                        this.cimRegisterCimIndication.Dispose();
                    status = Status.Stopped;
        #region internal method
        /// Set the cmdlet object to throw ThrowTerminatingError
        /// in case there is a subscription failure.
        internal void SetCmdlet(Cmdlet cmdlet)
                this.cimRegisterCimIndication.Cmdlet = cmdlet;
        /// CimRegisterCimIndication object
        private CimRegisterCimIndication cimRegisterCimIndication;
        /// The status of <see cref="CimIndicationWatcher"/> object.
        private Status status;
        /// Lock started field.
        private object myLock;
        /// CimSession parameter name.
        private const string cimSessionParameterName = "cimSession";
        /// QueryExpression parameter name.
        private const string queryExpressionParameterName = "queryExpression";
        #region parameters
        /// parameters used to start the subscription
        private string computerName;
        private CimSession cimSession;
        private string nameSpace;
        private string queryDialect;
        private string queryExpression;
        private uint operationTimeout;
