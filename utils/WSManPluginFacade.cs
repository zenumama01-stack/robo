    // TODO: Does this comment still apply?
    // The following complex delegate + native function pointer model is used
    // because of a problem with GCRoot. GCRoots cannot hold reference to the
    // AppDomain that created it. In the IIS hosting scenario, there may be
    // cases where multiple AppDomains exist in the same hosting process. In such
    // cases if GCRoot is used, CLR will pick up the first AppDomain in the list
    // to get the managed handle. Delegates are not just function pointers, they
    // also contain a reference to the AppDomain that created it. However the catch
    // is that delegates must be marshalled into their respective unmanaged function
    // pointers (otherwise we end up storing the delegate into a GCRoot).
    /// <param name="pluginContext">PVOID.</param>
    /// <param name="requestDetails">WSMAN_PLUGIN_REQUEST*.</param>
    /// <param name="flags">DWORD.</param>
    /// <param name="extraInfo">PCWSTR.</param>
    /// <param name="startupInfo">WSMAN_SHELL_STARTUP_INFO*.</param>
    /// <param name="inboundShellInformation">WSMAN_DATA*.</param>
    internal delegate void WSManPluginShellDelegate(
        [MarshalAs(UnmanagedType.LPWStr)] string extraInfo,
        IntPtr inboundShellInformation);
    /// <param name="shellContext">PVOID.</param>
    internal delegate void WSManPluginReleaseShellContextDelegate(
        IntPtr shellContext);
    /// <param name="commandContext">PVOID optional.</param>
    /// <param name="inboundConnectInformation">WSMAN_DATA* optional.</param>
    internal delegate void WSManPluginConnectDelegate(
        IntPtr inboundConnectInformation);
    /// <param name="commandLine">PCWSTR.</param>
    /// <param name="arguments">WSMAN_COMMAND_ARG_SET*.</param>
    internal delegate void WSManPluginCommandDelegate(
        IntPtr arguments);
    /// Delegate that is passed to native layer for callback on operation shutdown notifications.
    /// <param name="shutdownContext">IntPtr.</param>
    internal delegate void WSManPluginOperationShutdownDelegate(
    /// <param name="commandContext">PVOID.</param>
    internal delegate void WSManPluginReleaseCommandContextDelegate(
        IntPtr commandContext);
    /// <param name="stream">PCWSTR.</param>
    /// <param name="inboundData">WSMAN_DATA*.</param>
    internal delegate void WSManPluginSendDelegate(
        IntPtr inboundData);
    /// <param name="streamSet">WSMAN_STREAM_ID_SET* optional.</param>
    internal delegate void WSManPluginReceiveDelegate(
        IntPtr streamSet);
    /// <param name="code">PCWSTR.</param>
    internal delegate void WSManPluginSignalDelegate(
        [MarshalAs(UnmanagedType.LPWStr)] string code);
    /// Callback that handles shell shutdown notification events.
    /// <param name="timedOut"></param>
    internal delegate void WaitOrTimerCallbackDelegate(
        IntPtr state,
        bool timedOut);
    internal delegate void WSManShutdownPluginDelegate(
        IntPtr pluginContext);
    internal sealed class WSManPluginEntryDelegates : IDisposable
        // Holds the delegate pointers in a structure that has identical layout to the native structure.
        private readonly WSManPluginEntryDelegatesInternal _unmanagedStruct = new WSManPluginEntryDelegatesInternal();
        internal WSManPluginEntryDelegatesInternal UnmanagedStruct
            get { return _unmanagedStruct; }
        // Flag: Has Dispose already been called?
        /// GC handle which prevents garbage collector from collecting this delegate.
        private GCHandle _pluginShellGCHandle;
        private GCHandle _pluginReleaseShellContextGCHandle;
        private GCHandle _pluginCommandGCHandle;
        private GCHandle _pluginReleaseCommandContextGCHandle;
        private GCHandle _pluginSendGCHandle;
        private GCHandle _pluginReceiveGCHandle;
        private GCHandle _pluginSignalGCHandle;
        private GCHandle _pluginConnectGCHandle;
        private GCHandle _shutdownPluginGCHandle;
        private GCHandle _WSManPluginOperationShutdownGCHandle;
        /// Initializes the delegate struct for later use.
        internal WSManPluginEntryDelegates()
            populateDelegates();
        #region IDisposable Methods
        /// Internal implementation of Dispose pattern callable by consumers.
        /// Protected implementation of Dispose pattern.
            // Free any unmanaged objects here.
            this.CleanUpDelegates();
        /// Finalizes an instance of the <see cref="WSManPluginEntryDelegates"/> class.
        ~WSManPluginEntryDelegates()
        #endregion IDisposable Methods
        /// Creates delegates and populates the managed version of the
        /// structure that will be passed to unmanaged callers.
        private void populateDelegates()
                WSManPluginShellDelegate pluginShell = new WSManPluginShellDelegate(WSManPluginManagedEntryWrapper.WSManPluginShell);
                _pluginShellGCHandle = GCHandle.Alloc(pluginShell);
                // marshal the delegate to a unmanaged function pointer so that AppDomain reference is stored correctly.
                // Populate the outgoing structure so the caller has access to the entry points
                _unmanagedStruct.wsManPluginShellCallbackNative = Marshal.GetFunctionPointerForDelegate(pluginShell);
                WSManPluginReleaseShellContextDelegate pluginReleaseShellContext = new WSManPluginReleaseShellContextDelegate(WSManPluginManagedEntryWrapper.WSManPluginReleaseShellContext);
                _pluginReleaseShellContextGCHandle = GCHandle.Alloc(pluginReleaseShellContext);
                _unmanagedStruct.wsManPluginReleaseShellContextCallbackNative = Marshal.GetFunctionPointerForDelegate(pluginReleaseShellContext);
                WSManPluginCommandDelegate pluginCommand = new WSManPluginCommandDelegate(WSManPluginManagedEntryWrapper.WSManPluginCommand);
                _pluginCommandGCHandle = GCHandle.Alloc(pluginCommand);
                _unmanagedStruct.wsManPluginCommandCallbackNative = Marshal.GetFunctionPointerForDelegate(pluginCommand);
                WSManPluginReleaseCommandContextDelegate pluginReleaseCommandContext = new WSManPluginReleaseCommandContextDelegate(WSManPluginManagedEntryWrapper.WSManPluginReleaseCommandContext);
                _pluginReleaseCommandContextGCHandle = GCHandle.Alloc(pluginReleaseCommandContext);
                _unmanagedStruct.wsManPluginReleaseCommandContextCallbackNative = Marshal.GetFunctionPointerForDelegate(pluginReleaseCommandContext);
                WSManPluginSendDelegate pluginSend = new WSManPluginSendDelegate(WSManPluginManagedEntryWrapper.WSManPluginSend);
                _pluginSendGCHandle = GCHandle.Alloc(pluginSend);
                _unmanagedStruct.wsManPluginSendCallbackNative = Marshal.GetFunctionPointerForDelegate(pluginSend);
                WSManPluginReceiveDelegate pluginReceive = new WSManPluginReceiveDelegate(WSManPluginManagedEntryWrapper.WSManPluginReceive);
                _pluginReceiveGCHandle = GCHandle.Alloc(pluginReceive);
                _unmanagedStruct.wsManPluginReceiveCallbackNative = Marshal.GetFunctionPointerForDelegate(pluginReceive);
                WSManPluginSignalDelegate pluginSignal = new WSManPluginSignalDelegate(WSManPluginManagedEntryWrapper.WSManPluginSignal);
                _pluginSignalGCHandle = GCHandle.Alloc(pluginSignal);
                _unmanagedStruct.wsManPluginSignalCallbackNative = Marshal.GetFunctionPointerForDelegate(pluginSignal);
                WSManPluginConnectDelegate pluginConnect = new WSManPluginConnectDelegate(WSManPluginManagedEntryWrapper.WSManPluginConnect);
                _pluginConnectGCHandle = GCHandle.Alloc(pluginConnect);
                _unmanagedStruct.wsManPluginConnectCallbackNative = Marshal.GetFunctionPointerForDelegate(pluginConnect);
                WSManShutdownPluginDelegate shutdownPlugin = new WSManShutdownPluginDelegate(WSManPluginManagedEntryWrapper.ShutdownPlugin);
                _shutdownPluginGCHandle = GCHandle.Alloc(shutdownPlugin);
                _unmanagedStruct.wsManPluginShutdownPluginCallbackNative = Marshal.GetFunctionPointerForDelegate(shutdownPlugin);
                WSManPluginOperationShutdownDelegate pluginShutDownDelegate = new WSManPluginOperationShutdownDelegate(WSManPluginManagedEntryWrapper.WSManPSShutdown);
                _WSManPluginOperationShutdownGCHandle = GCHandle.Alloc(pluginShutDownDelegate);
                _unmanagedStruct.wsManPluginShutdownCallbackNative = Marshal.GetFunctionPointerForDelegate(pluginShutDownDelegate);
        private void CleanUpDelegates()
            // Free GCHandles so that the memory they point to may be unpinned (garbage collected)
            if (_pluginShellGCHandle.IsAllocated)
                _pluginShellGCHandle.Free();
                _pluginReleaseShellContextGCHandle.Free();
                _pluginCommandGCHandle.Free();
                _pluginReleaseCommandContextGCHandle.Free();
                _pluginSendGCHandle.Free();
                _pluginReceiveGCHandle.Free();
                _pluginSignalGCHandle.Free();
                _pluginConnectGCHandle.Free();
                _shutdownPluginGCHandle.Free();
                    _WSManPluginOperationShutdownGCHandle.Free();
        /// Structure definition to match the native one.
        /// NOTE: The layout of this structure must be IDENTICAL between here and PwrshPluginWkr_Ptrs in pwrshplugindefs.h!
        internal class WSManPluginEntryDelegatesInternal
            /// WsManPluginShutdownPluginCallbackNative.
            internal IntPtr wsManPluginShutdownPluginCallbackNative;
            /// WSManPluginShellCallbackNative.
            internal IntPtr wsManPluginShellCallbackNative;
            /// WSManPluginReleaseShellContextCallbackNative.
            internal IntPtr wsManPluginReleaseShellContextCallbackNative;
            /// WSManPluginCommandCallbackNative.
            internal IntPtr wsManPluginCommandCallbackNative;
            /// WSManPluginReleaseCommandContextCallbackNative.
            internal IntPtr wsManPluginReleaseCommandContextCallbackNative;
            /// WSManPluginSendCallbackNative.
            internal IntPtr wsManPluginSendCallbackNative;
            /// WSManPluginReceiveCallbackNative.
            internal IntPtr wsManPluginReceiveCallbackNative;
            /// WSManPluginSignalCallbackNative.
            internal IntPtr wsManPluginSignalCallbackNative;
            /// WSManPluginConnectCallbackNative.
            internal IntPtr wsManPluginConnectCallbackNative;
            internal IntPtr wsManPluginShutdownCallbackNative;
    /// Class containing the public static managed entry functions that are callable from outside
    /// the module.
    public sealed class WSManPluginManagedEntryWrapper
        /// Constructor is private because it only contains static members and properties.
        private WSManPluginManagedEntryWrapper() { }
        /// Immutable container that holds the delegates and their unmanaged pointers.
        internal static readonly WSManPluginEntryDelegates workerPtrs = new WSManPluginEntryDelegates();
        #region Managed Entry Points
        /// Called only once after the assembly is loaded..This is used to perform
        /// various initializations.
        /// <param name="wkrPtrs">IntPtr to WSManPluginEntryDelegates.WSManPluginEntryDelegatesInternal.</param>
        /// <returns>0 = Success, 1 = Failure.</returns>
        public static int InitPlugin(
            IntPtr wkrPtrs)
            if (wkrPtrs == IntPtr.Zero)
                return WSManPluginConstants.ExitCodeFailure;
            Marshal.StructureToPtr<WSManPluginEntryDelegates.WSManPluginEntryDelegatesInternal>(workerPtrs.UnmanagedStruct, wkrPtrs, false);
            return WSManPluginConstants.ExitCodeSuccess;
        /// Called only once during shutdown. This is used to perform various deinitializations.
        public static void ShutdownPlugin(
            WSManPluginInstance.PerformShutdown(pluginContext);
            workerPtrs?.Dispose();
        public static void WSManPluginConnect(
            if (pluginContext == IntPtr.Zero)
                WSManPluginInstance.ReportOperationComplete(
                    WSManPluginErrorCodes.NullPluginContext,
                        RemotingErrorIdStrings.WSManPluginNullPluginContext,
                        "pluginContext",
                        "WSManPluginConnect")
            WSManPluginInstance.PerformWSManPluginConnect(pluginContext, requestDetails, flags, shellContext, commandContext, inboundConnectInformation);
        public static void WSManPluginShell(
            IntPtr inboundShellInformation)
                        "WSManPluginShell")
#if(DEBUG)
            // In debug builds, allow remote runspaces to wait for debugger attach
            if (Environment.GetEnvironmentVariable("__PSRemoteRunspaceWaitForDebugger", EnvironmentVariableTarget.Machine) != null)
                bool debuggerAttached = false;
                while (!debuggerAttached)
            WSManPluginInstance.PerformWSManPluginShell(pluginContext, requestDetails, flags, extraInfo, startupInfo, inboundShellInformation);
        public static void WSManPluginReleaseShellContext(
            IntPtr shellContext)
            // NO-OP..as our plugin does not own the memory related
            // to shellContext and so there is nothing to release
        /// <param name="arguments">WSMAN_COMMAND_ARG_SET* optional.</param>
        public static void WSManPluginCommand(
            IntPtr arguments)
                        "Plugin Context",
                        "WSManPluginCommand")
            WSManPluginInstance.PerformWSManPluginCommand(pluginContext, requestDetails, flags, shellContext, commandLine, arguments);
        /// Operation shutdown notification that was registered with the native layer for each of the shellCreate operations.
        public static void WSManPSShutdown(
            GCHandle gch = GCHandle.FromIntPtr(shutdownContext);
            EventWaitHandle eventHandle = (EventWaitHandle)gch.Target;
            eventHandle.Set();
            gch.Free();
        public static void WSManPluginReleaseCommandContext(
            // to commandContext and so there is nothing to release.
        public static void WSManPluginSend(
            IntPtr inboundData)
                        "WSManPluginSend")
            WSManPluginInstance.PerformWSManPluginSend(pluginContext, requestDetails, flags, shellContext, commandContext, stream, inboundData);
        public static void WSManPluginReceive(
            IntPtr streamSet)
                        "WSManPluginReceive")
            WSManPluginInstance.PerformWSManPluginReceive(pluginContext, requestDetails, flags, shellContext, commandContext, streamSet);
        public static void WSManPluginSignal(
            [MarshalAs(UnmanagedType.LPWStr)] string code)
            if ((pluginContext == IntPtr.Zero) || (shellContext == IntPtr.Zero))
                        "WSManPluginSignal")
            WSManPluginInstance.PerformWSManPluginSignal(pluginContext, requestDetails, flags, shellContext, commandContext, code);
        /// Callback used to register with thread pool to notify when a plugin operation shuts down.
        /// Conforms to:
        ///     public delegate void WaitOrTimerCallback( Object state, bool timedOut )
        /// <param name="operationContext">PVOID.</param>
        /// <param name="timedOut">BOOLEAN.</param>
        public static void PSPluginOperationShutdownCallback(
            object operationContext,
            bool timedOut)
            if (operationContext == null)
            WSManPluginOperationShutdownContext context = (WSManPluginOperationShutdownContext)operationContext;
            context.isShuttingDown = true;
            WSManPluginInstance.PerformCloseOperation(context);
    /// This is a thin wrapper around WSManPluginManagedEntryWrapper.InitPlugin()
    /// so that it can be called from native COM code in a non-static context.
    /// This was done to get around an FXCop error: AvoidStaticMembersInComVisibleTypes.
    public sealed class WSManPluginManagedEntryInstanceWrapper : IDisposable
            _initDelegateHandle.Free();
        /// Finalizes an instance of the <see cref="WSManPluginManagedEntryInstanceWrapper"/> class.
        ~WSManPluginManagedEntryInstanceWrapper()
        #region Delegate Handling
        /// Matches signature for WSManPluginManagedEntryWrapper.InitPlugin.
        /// <param name="wkrPtrs"></param>
        private delegate int InitPluginDelegate(
            IntPtr wkrPtrs);
        /// Prevents the delegate object from being garbage collected so it can be passed to the native code.
        private GCHandle _initDelegateHandle;
        /// Entry point for native code that cannot call static methods.
        /// <returns>A function pointer for the static entry point for the WSManPlugin initialization function.</returns>
        public IntPtr GetEntryDelegate()
            InitPluginDelegate initDelegate = new InitPluginDelegate(WSManPluginManagedEntryWrapper.InitPlugin);
            _initDelegateHandle = GCHandle.Alloc(initDelegate);
            return Marshal.GetFunctionPointerForDelegate(initDelegate);
