    #region AsyncResultType
    /// Async result type
    public enum AsyncResultType
        Result,
        Exception,
        Completion
    #region CimResultContext
    /// Cim Result Context.
    internal class CimResultContext
        /// Initializes a new instance of the <see cref="CimResultContext"/> class.
        /// <param name="ErrorSource"></param>
        internal CimResultContext(object ErrorSource)
            this.ErrorSource = ErrorSource;
        /// ErrorSource property.
        internal object ErrorSource { get; }
    #region AsyncResultEventArgsBase
    /// Base class of async result event argument
    internal abstract class AsyncResultEventArgsBase : EventArgs
        /// Initializes a new instance of the <see cref="AsyncResultEventArgsBase"/> class.
        /// <param name="observable"></param>
        /// <param name="resultType"></param>
        protected AsyncResultEventArgsBase(
            IObservable<object> observable,
            AsyncResultType resultType)
            this.session = session;
            this.observable = observable;
            this.resultType = resultType;
        /// <param name="context"></param>
            AsyncResultType resultType,
            CimResultContext cimResultContext)
            this.context = cimResultContext;
        public readonly CimSession session;
        public readonly IObservable<object> observable;
        public readonly AsyncResultType resultType;
        // property ErrorSource
        public readonly CimResultContext context;
    #region AsyncResult*Args
    /// operation successfully completed event argument
    internal class AsyncResultCompleteEventArgs : AsyncResultEventArgsBase
        /// Initializes a new instance of the <see cref="AsyncResultCompleteEventArgs"/> class.
        /// <param name="session"><see cref="CimSession"/> object.</param>
        /// <param name="cancellationDisposable"></param>
        public AsyncResultCompleteEventArgs(
            IObservable<object> observable)
            : base(session, observable, AsyncResultType.Completion)
    /// async result argument with object
    internal class AsyncResultObjectEventArgs : AsyncResultEventArgsBase
        /// Initializes a new instance of the <see cref="AsyncResultObjectEventArgs"/> class.
        public AsyncResultObjectEventArgs(
            object resultObject)
            : base(session, observable, AsyncResultType.Result)
            this.resultObject = resultObject;
        public readonly object resultObject;
    /// operation completed with exception event argument
    internal class AsyncResultErrorEventArgs : AsyncResultEventArgsBase
        /// Initializes a new instance of the <see cref="AsyncResultErrorEventArgs"/> class.
        /// <param name="error"></param>
        public AsyncResultErrorEventArgs(
            Exception error)
            : base(session, observable, AsyncResultType.Exception)
            this.error = error;
            Exception error,
            : base(session, observable, AsyncResultType.Exception, cimResultContext)
        public readonly Exception error;
    #region CimResultObserver
    /// Observer to consume results from asynchronous operations, such as,
    /// EnumerateInstancesAsync operation of <see cref="CimSession"/> object.
    /// (See https://channel9.msdn.com/posts/J.Van.Gogh/Reactive-Extensions-API-in-depth-Contract/)
    /// for the IObserver/IObservable contact
    /// - the only possible sequence is OnNext* (OnCompleted|OnError)?
    /// - callbacks are serialized
    /// - Subscribe never throws
    /// <typeparam name="T">object type</typeparam>
    internal class CimResultObserver<T> : IObserver<T>
        /// Define an Event based on the NewActionHandler.
        public event EventHandler<AsyncResultEventArgsBase> OnNewResult;
        /// Initializes a new instance of the <see cref="CimResultObserver{T}"/> class.
        /// <param name="session"><see cref="CimSession"/> object that issued the operation.</param>
        /// <param name="observable">Operation that can be observed.</param>
        public CimResultObserver(CimSession session, IObservable<object> observable)
            this.CurrentSession = session;
        public CimResultObserver(CimSession session,
        /// Operation completed successfully
        public virtual void OnCompleted()
            // callbacks should never throw any exception to
            // protocol layer, otherwise the client process will be
            // terminated because of unhandled exception, same with
            // OnNext, OnError
                AsyncResultCompleteEventArgs completeArgs = new(
                    this.CurrentSession, this.observable);
                this.OnNewResult(this, completeArgs);
            catch (Exception ex)
                this.OnError(ex);
                DebugHelper.WriteLogEx("{0}", 0, ex);
        /// Operation completed with an error
        /// <param name="error">Error object.</param>
        public virtual void OnError(Exception error)
                AsyncResultErrorEventArgs errorArgs = new(
                    this.CurrentSession, this.observable, error, this.context);
                this.OnNewResult(this, errorArgs);
                // !!ignore the exception
        /// Deliver the result value.
        protected void OnNextCore(object value)
            DebugHelper.WriteLogEx("value = {0}.", 1, value);
                AsyncResultObjectEventArgs resultArgs = new(
                    this.CurrentSession, this.observable, value);
                this.OnNewResult(this, resultArgs);
        /// Operation got a new result object
        /// <param name="value">Result object.</param>
        public virtual void OnNext(T value)
            // do not allow null value
            this.OnNextCore(value);
        /// Session object of the operation.
        protected CimSession CurrentSession { get; }
        /// Async operation that can be observed.
        private readonly IObservable<object> observable;
        /// <see cref="CimResultContext"/> object used during delivering result.
        private readonly CimResultContext context;
    /// CimSubscriptionResultObserver class definition.
    internal class CimSubscriptionResultObserver : CimResultObserver<CimSubscriptionResult>
        /// Initializes a new instance of the <see cref="CimSubscriptionResultObserver"/> class.
        public CimSubscriptionResultObserver(CimSession session, IObservable<object> observable)
            : base(session, observable)
        public CimSubscriptionResultObserver(
            CimResultContext context)
            : base(session, observable, context)
        /// Override the OnNext method.
        public override void OnNext(CimSubscriptionResult value)
            base.OnNextCore(value);
    /// CimMethodResultObserver class definition.
    internal class CimMethodResultObserver : CimResultObserver<CimMethodResultBase>
        /// Initializes a new instance of the <see cref="CimMethodResultObserver"/> class.
        public CimMethodResultObserver(CimSession session, IObservable<object> observable)
        public CimMethodResultObserver(
        public override void OnNext(CimMethodResultBase value)
            const string PSTypeCimMethodResult = @"Microsoft.Management.Infrastructure.CimMethodResult";
            const string PSTypeCimMethodStreamedResult = @"Microsoft.Management.Infrastructure.CimMethodStreamedResult";
            const string PSTypeCimMethodResultTemplate = @"{0}#{1}#{2}";
            string resultObjectPSType = null;
            PSObject resultObject = null;
            if (value is CimMethodResult methodResult)
                resultObjectPSType = PSTypeCimMethodResult;
                resultObject = new PSObject();
                foreach (CimMethodParameter param in methodResult.OutParameters)
                    resultObject.Properties.Add(new PSNoteProperty(param.Name, param.Value));
                if (value is CimMethodStreamedResult methodStreamedResult)
                    resultObjectPSType = PSTypeCimMethodStreamedResult;
                    resultObject.Properties.Add(new PSNoteProperty(@"ParameterName", methodStreamedResult.ParameterName));
                    resultObject.Properties.Add(new PSNoteProperty(@"ItemType", methodStreamedResult.ItemType));
                    resultObject.Properties.Add(new PSNoteProperty(@"ItemValue", methodStreamedResult.ItemValue));
            if (resultObject != null)
                resultObject.Properties.Add(new PSNoteProperty(@"PSComputerName", this.CurrentSession.ComputerName));
                resultObject.TypeNames.Insert(0, resultObjectPSType);
                resultObject.TypeNames.Insert(0, string.Format(CultureInfo.InvariantCulture, PSTypeCimMethodResultTemplate, resultObjectPSType, ClassName, MethodName));
                base.OnNextCore(resultObject);
        /// Methodname.
        internal string MethodName
        /// Classname.
        internal string ClassName
    /// IgnoreResultObserver class definition.
    internal class IgnoreResultObserver : CimResultObserver<CimInstance>
        /// Initializes a new instance of the <see cref="IgnoreResultObserver"/> class.
        public IgnoreResultObserver(CimSession session, IObservable<object> observable)
        public override void OnNext(CimInstance value)
