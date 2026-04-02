#region Using directives
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
#endregion
namespace Microsoft.Management.Infrastructure.CimCmdlets
    /// <para>
    /// Async operation base class, it will issue async operation through
    /// 1...* CimSession object(s), processing the async results, extended
    /// pssemantics operations, and manage the lifecycle of created
    /// CimSession object(s).
    /// </para>
    internal abstract class CimAsyncOperation : IDisposable
        #region Constructor
        /// Initializes a new instance of the <see cref="CimAsyncOperation"/> class.
        protected CimAsyncOperation()
            this.moreActionEvent = new ManualResetEventSlim(false);
            this.actionQueue = new ConcurrentQueue<CimBaseAction>();
            this._disposed = 0;
            this.operationCount = 0;
        #region Event handler
        /// Handler used to handle new action event from
        /// <seealso cref="CimSessionProxy"/> object.
        /// <param name="cimSession">
        /// <seealso cref="CimSession"/> object raised the event
        /// </param>
        /// <param name="actionArgs">Event argument.</param>
        protected void NewCmdletActionHandler(object cimSession, CmdletActionEventArgs actionArgs)
            DebugHelper.WriteLogEx("Disposed {0}, action type = {1}", 0, this.Disposed, actionArgs.Action);
            if (this.Disposed)
                if (actionArgs.Action is CimSyncAction)
                    // unblock the thread waiting for response
                    (actionArgs.Action as CimSyncAction).OnComplete();
            bool isEmpty = this.actionQueue.IsEmpty;
            this.actionQueue.Enqueue(actionArgs.Action);
            if (isEmpty)
                this.moreActionEvent.Set();
        /// Handler used to handle new operation event from
        /// <seealso cref="CimSession"/> object raised the event.
        protected void OperationCreatedHandler(object cimSession, OperationEventArgs actionArgs)
            DebugHelper.WriteLogEx();
            lock (this.a_lock)
                this.operationCount++;
        /// Handler used to handle operation deletion event from
        protected void OperationDeletedHandler(object cimSession, OperationEventArgs actionArgs)
                this.operationCount--;
                if (this.operationCount == 0)
        /// process all actions in the action queue
        /// <param name="cmdletOperation">
        /// Wrapper of cmdlet, <seealso cref="CmdletOperationBase"/> for details.
        public void ProcessActions(CmdletOperationBase cmdletOperation)
            if (!this.actionQueue.IsEmpty)
                CimBaseAction action;
                while (GetActionAndRemove(out action))
                    action.Execute(cmdletOperation);
        /// Process remaining actions until all operations are completed or
        /// current cmdlet is terminated by user.
        public void ProcessRemainActions(CmdletOperationBase cmdletOperation)
            while (true)
                ProcessActions(cmdletOperation);
                if (!this.IsActive())
                    DebugHelper.WriteLogEx("Either disposed or all operations completed.", 2);
                    this.moreActionEvent.Wait();
                    this.moreActionEvent.Reset();
                catch (ObjectDisposedException ex)
                    // This might happen if this object is being disposed,
                    // while another thread is processing the remaining actions
                    DebugHelper.WriteLogEx("moreActionEvent was disposed: {0}.", 2, ex);
        #region helper methods
        /// Get action object from action queue.
        /// <param name="action">Next action to execute.</param>
        /// <returns>True indicates there is an valid action, otherwise false.</returns>
        protected bool GetActionAndRemove(out CimBaseAction action)
            return this.actionQueue.TryDequeue(out action);
        /// Add temporary <seealso cref="CimSessionProxy"/> object to cache.
        /// <param name="sessionproxy">Cimsession wrapper object.</param>
        protected void AddCimSessionProxy(CimSessionProxy sessionproxy)
            lock (cimSessionProxyCacheLock)
                this.cimSessionProxyCache ??= new List<CimSessionProxy>();
                if (!this.cimSessionProxyCache.Contains(sessionproxy))
                    this.cimSessionProxyCache.Add(sessionproxy);
        /// Are there active operations?
        /// <returns>True for having active operations, otherwise false.</returns>
        protected bool IsActive()
            DebugHelper.WriteLogEx("Disposed {0}, Operation Count {1}", 2, this.Disposed, this.operationCount);
            bool isActive = (!this.Disposed) && (this.operationCount > 0);
            return isActive;
        /// Create <see cref="CimSessionProxy"/> object.
        /// <param name="session"></param>
        protected CimSessionProxy CreateCimSessionProxy(CimSessionProxy originalProxy)
            CimSessionProxy proxy = new(originalProxy);
            this.SubscribeEventAndAddProxytoCache(proxy);
            return proxy;
        protected CimSessionProxy CreateCimSessionProxy(CimSessionProxy originalProxy, bool passThru)
            CimSessionProxy proxy = new CimSessionProxySetCimInstance(originalProxy, passThru);
        protected CimSessionProxy CreateCimSessionProxy(CimSession session)
            CimSessionProxy proxy = new(session);
        protected CimSessionProxy CreateCimSessionProxy(CimSession session, bool passThru)
            CimSessionProxy proxy = new CimSessionProxySetCimInstance(session, passThru);
        /// Create <see cref="CimSessionProxy"/> object, and
        /// add the proxy into cache.
        /// <param name="computerName"></param>
        protected CimSessionProxy CreateCimSessionProxy(string computerName)
            CimSessionProxy proxy = new(computerName);
        /// <param name="cimInstance"></param>
        /// <returns></returns>
        protected CimSessionProxy CreateCimSessionProxy(string computerName, CimInstance cimInstance)
            CimSessionProxy proxy = new(computerName, cimInstance);
        /// <param name="passThru"></param>
        protected CimSessionProxy CreateCimSessionProxy(string computerName, CimInstance cimInstance, bool passThru)
            CimSessionProxy proxy = new CimSessionProxySetCimInstance(computerName, cimInstance, passThru);
        /// Subscribe event from proxy and add proxy to cache.
        /// <param name="proxy"></param>
        protected void SubscribeEventAndAddProxytoCache(CimSessionProxy proxy)
            this.AddCimSessionProxy(proxy);
            SubscribeToCimSessionProxyEvent(proxy);
        /// Subscribe to the events issued by <see cref="CimSessionProxy"/>.
        protected virtual void SubscribeToCimSessionProxyEvent(CimSessionProxy proxy)
            proxy.OnNewCmdletAction += this.NewCmdletActionHandler;
            proxy.OnOperationCreated += this.OperationCreatedHandler;
            proxy.OnOperationDeleted += this.OperationDeletedHandler;
        /// Retrieve the base object out if wrapped in psobject.
        /// <param name="value"></param>
        protected object GetBaseObject(object value)
            if (value is not PSObject psObject)
                return value;
                object baseObject = psObject.BaseObject;
                if (baseObject is not object[] arrayObject)
                    return baseObject;
                    object[] arraybaseObject = new object[arrayObject.Length];
                    for (int i = 0; i < arrayObject.Length; i++)
                        arraybaseObject[i] = GetBaseObject(arrayObject[i]);
                    return arraybaseObject;
        /// Retrieve the reference object or reference array object.
        /// The returned object has to be either CimInstance or CImInstance[] type,
        /// if not thrown exception.
        /// <param name="referenceType">Output the cimtype of the value, either Reference or ReferenceArray.</param>
        /// <returns>The object.</returns>
        protected object GetReferenceOrReferenceArrayObject(object value, ref CimType referenceType)
            if (value is PSReference cimReference)
                object baseObject = GetBaseObject(cimReference.Value);
                if (baseObject is not CimInstance cimInstance)
                    return null;
                referenceType = CimType.Reference;
                return cimInstance;
                if (value is not object[] cimReferenceArray)
                else if (cimReferenceArray[0] is not PSReference)
                CimInstance[] cimInstanceArray = new CimInstance[cimReferenceArray.Length];
                for (int i = 0; i < cimReferenceArray.Length; i++)
                    if (cimReferenceArray[i] is not PSReference tempCimReference)
                    object baseObject = GetBaseObject(tempCimReference.Value);
                    cimInstanceArray[i] = baseObject as CimInstance;
                    if (cimInstanceArray[i] == null)
                referenceType = CimType.ReferenceArray;
                return cimInstanceArray;
        #region IDisposable
        /// Indicates whether this object was disposed or not
        protected bool Disposed
            get
                return this._disposed == 1;
        private int _disposed;
        /// Dispose() calls Dispose(true).
        /// Implement IDisposable. Do not make this method virtual.
        /// A derived class should not be able to override this method.
        public void Dispose()
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// <param name="disposing">Whether it is directly called.</param>
        protected virtual void Dispose(bool disposing)
            if (Interlocked.CompareExchange(ref this._disposed, 1, 0) == 0)
                if (disposing)
                    // free managed resources
                    Cleanup();
                // free native resources if there are any
        /// Clean up managed resources.
        private void Cleanup()
            // unblock thread that waiting for more actions
                DebugHelper.WriteLog("Action {0}", 2, action);
                if (action is CimSyncAction)
                    (action as CimSyncAction).OnComplete();
            if (this.cimSessionProxyCache != null)
                List<CimSessionProxy> temporaryProxy;
                lock (this.cimSessionProxyCache)
                    temporaryProxy = new List<CimSessionProxy>(this.cimSessionProxyCache);
                    this.cimSessionProxyCache.Clear();
                // clean up all proxy objects
                foreach (CimSessionProxy proxy in temporaryProxy)
                    DebugHelper.WriteLog("Dispose proxy ", 2);
                    proxy.Dispose();
            this.moreActionEvent.Dispose();
            this.ackedEvent?.Dispose();
            DebugHelper.WriteLog("Cleanup complete.", 2);
        #region private members
        /// Lock object.
        private readonly object a_lock = new();
        /// Number of active operations.
        private uint operationCount;
        /// Event to notify ps thread that more action is available.
        private readonly ManualResetEventSlim moreActionEvent;
        /// The following is the definition of action queue.
        /// The queue holding all actions to be executed in the context of either
        /// ProcessRecord or EndProcessing.
        private readonly ConcurrentQueue<CimBaseAction> actionQueue;
        private readonly object cimSessionProxyCacheLock = new();
        /// Cache all <see cref="CimSessionProxy"/> objects related to
        /// the current operation.
        private List<CimSessionProxy> cimSessionProxyCache;
        #region protected members
        /// Event to notify ps thread that either a ACK message sent back
        /// or a error happened. Currently only used by
        /// <see cref="CimRegisterCimIndication"/>.
        protected ManualResetEventSlim ackedEvent;
        #region const strings
        internal const string ComputerNameArgument = @"ComputerName";
        internal const string CimSessionArgument = @"CimSession";
