#pragma warning disable 1591, 1572, 1571, 1573, 1587, 1570, 0067
#region PS_STUBS
// Include PS types that are not needed for PowerShell on CSS
    #region PSTransaction
    /// We don't need PSTransaction related types on CSS because System.Transactions
    /// namespace is not available in CoreCLR.
    public sealed class PSTransactionContext : IDisposable
        internal PSTransactionContext(Internal.PSTransactionManager transactionManager) { }
        public void Dispose() { }
    /// The severity of error that causes PowerShell to automatically
    /// rollback the transaction.
    public enum RollbackSeverity
        /// Non-terminating errors or worse.
        Error,
        /// Terminating errors or worse.
        TerminatingError,
        /// Do not rollback the transaction on error.
        Never
    #endregion PSTransaction
namespace System.Management.Automation.Internal
    internal sealed class PSTransactionManager : IDisposable
        /// Determines if you have a transaction that you can set active and work on.
        /// Always return false in CoreCLR
        internal bool HasTransaction
        /// Determines if the last transaction has been committed.
        internal bool IsLastTransactionCommitted
                throw new NotImplementedException("IsLastTransactionCommitted");
        /// Determines if the last transaction has been rolled back.
        internal bool IsLastTransactionRolledBack
                throw new NotImplementedException("IsLastTransactionRolledBack");
        /// Gets the rollback preference for the active transaction.
        internal RollbackSeverity RollbackPreference
                throw new NotImplementedException("RollbackPreference");
        /// Called by engine APIs to ensure they are protected from
        /// ambient transactions.
        /// Always return null in CoreCLR
        internal static IDisposable GetEngineProtectionScope()
        /// Aborts the current transaction, no matter how many subscribers are part of it.
        internal void Rollback(bool suppressErrors)
            throw new NotImplementedException("Rollback");
    #region TransactedRegistryKey
    internal abstract class TransactedRegistryKey : IDisposable
        public void SetValue(string name, object value)
            throw new NotImplementedException("SetValue(string name, obj value) is not implemented. TransactedRegistry related APIs should not be used.");
        public void SetValue(string name, object value, RegistryValueKind valueKind)
            throw new NotImplementedException("SetValue(string name, obj value, RegistryValueKind valueKind) is not implemented. TransactedRegistry related APIs should not be used.");
        public string[] GetValueNames()
            throw new NotImplementedException("GetValueNames() is not implemented. TransactedRegistry related APIs should not be used.");
        public void DeleteValue(string name)
            throw new NotImplementedException("DeleteValue(string name) is not implemented. TransactedRegistry related APIs should not be used.");
        public string[] GetSubKeyNames()
            throw new NotImplementedException("GetSubKeyNames() is not implemented. TransactedRegistry related APIs should not be used.");
        public TransactedRegistryKey CreateSubKey(string subkey)
            throw new NotImplementedException("CreateSubKey(string subkey) is not implemented. TransactedRegistry related APIs should not be used.");
        public TransactedRegistryKey OpenSubKey(string name, bool writable)
            throw new NotImplementedException("OpenSubKey(string name, bool writeable) is not implemented. TransactedRegistry related APIs should not be used.");
        public void DeleteSubKeyTree(string subkey)
            throw new NotImplementedException("DeleteSubKeyTree(string subkey) is not implemented. TransactedRegistry related APIs should not be used.");
        public object GetValue(string name)
            throw new NotImplementedException("GetValue(string name) is not implemented. TransactedRegistry related APIs should not be used.");
        public object GetValue(string name, object defaultValue, RegistryValueOptions options)
            throw new NotImplementedException("GetValue(string name, object defaultValue, RegistryValueOptions options) is not implemented. TransactedRegistry related APIs should not be used.");
        public RegistryValueKind GetValueKind(string name)
            throw new NotImplementedException("GetValueKind(string name) is not implemented. TransactedRegistry related APIs should not be used.");
        public void Close()
            throw new NotImplementedException("Close() is not implemented. TransactedRegistry related APIs should not be used.");
        public abstract string Name { get; }
        public abstract int SubKeyCount { get; }
        public void SetAccessControl(ObjectSecurity securityDescriptor)
            throw new NotImplementedException("SetAccessControl(ObjectSecurity securityDescriptor) is not implemented. TransactedRegistry related APIs should not be used.");
        public ObjectSecurity GetAccessControl(AccessControlSections includeSections)
            throw new NotImplementedException("GetAccessControl(AccessControlSections includeSections) is not implemented. TransactedRegistry related APIs should not be used.");
    internal sealed class TransactedRegistry
        internal static readonly TransactedRegistryKey LocalMachine;
        internal static readonly TransactedRegistryKey ClassesRoot;
        internal static readonly TransactedRegistryKey Users;
        internal static readonly TransactedRegistryKey CurrentConfig;
        internal static readonly TransactedRegistryKey CurrentUser;
    internal sealed class TransactedRegistrySecurity : ObjectSecurity
        public override Type AccessRightType
        public override Type AccessRuleType
        public override Type AuditRuleType
        public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        protected override bool ModifyAccess(AccessControlModification modification, AccessRule rule, out bool modified)
        protected override bool ModifyAudit(AccessControlModification modification, AuditRule rule, out bool modified)
    #endregion TransactedRegistryKey
#endregion PS_STUBS
// -- Will port the actual PS component [update: Not necessarily porting all PS components listed here]
#region TEMPORARY
    /// TODO:CORECLR - The actual PowerShellModuleAssemblyAnalyzer cannot be enabled because we don't have 'System.Reflection.Metadata.dll' in our branch yet.
    /// This stub will be removed once we enable the actual 'PowerShellModuleAssemblyAnalyzer'.
    internal static class PowerShellModuleAssemblyAnalyzer
        internal static BinaryAnalysisResult AnalyzeModuleAssembly(string path, out Version assemblyVersion)
            assemblyVersion = new Version("0.0.0.0");
    #region RegistryStringResourceIndirect
    internal sealed class RegistryStringResourceIndirect : IDisposable
        internal static RegistryStringResourceIndirect GetResourceIndirectReader()
            return new RegistryStringResourceIndirect();
        /// Dispose method unloads the app domain that was
        /// created in the constructor.
        internal string GetResourceStringIndirect(
            string assemblyName,
            string modulePath,
            string baseNameRIDPair)
            throw new NotImplementedException("RegistryStringResourceIndirect.GetResourceStringIndirect - 3 params");
            RegistryKey key,
            string valueName,
            string modulePath)
            throw new NotImplementedException("RegistryStringResourceIndirect.GetResourceStringIndirect - 4 params");
namespace System.Management.Automation.ComInterop
    using System.Dynamic;
    /// Provides helper methods to bind COM objects dynamically.
    /// COM is not supported on Unix platforms. So this is a stub type.
    internal static class ComBinder
        /// Tries to perform binding of the dynamic get index operation.
        /// Always return false in CoreCLR.
        public static bool TryBindGetIndex(GetIndexBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, out DynamicMetaObject result)
        /// Tries to perform binding of the dynamic set index operation.
        public static bool TryBindSetIndex(SetIndexBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, DynamicMetaObject value, out DynamicMetaObject result)
        /// Tries to perform binding of the dynamic get member operation.
        public static bool TryBindGetMember(GetMemberBinder binder, DynamicMetaObject instance, out DynamicMetaObject result, bool delayInvocation)
        /// Tries to perform binding of the dynamic set member operation.
        public static bool TryBindSetMember(SetMemberBinder binder, DynamicMetaObject instance, DynamicMetaObject value, out DynamicMetaObject result)
        /// Tries to perform binding of the dynamic invoke member operation.
        public static bool TryBindInvokeMember(InvokeMemberBinder binder, bool isSetProperty, DynamicMetaObject instance, DynamicMetaObject[] args, out DynamicMetaObject result)
    internal static class VarEnumSelector
        internal static Type GetTypeForVarEnum(VarEnum vt)
namespace System.Management.Automation.Security
    /// Application white listing security policies only affect Windows OSs.
    public sealed class SystemPolicy
        private SystemPolicy() { }
        /// Writes to PowerShell WDAC Audit mode ETW log.
        /// <param name="context">Current execution context.</param>
        /// <param name="title">Audit message title.</param>
        /// <param name="message">Audit message message.</param>
        /// <param name="fqid">Fully Qualified ID.</param>
        /// <param name="dropIntoDebugger">Stops code execution and goes into debugger mode.</param>
        internal static void LogWDACAuditMessage(
            ExecutionContext context,
            string title,
            string fqid,
            bool dropIntoDebugger = false)
        /// Gets the system lockdown policy.
        /// <remarks>Always return SystemEnforcementMode.None on non-Windows platforms.</remarks>
        public static SystemEnforcementMode GetSystemLockdownPolicy()
            return SystemEnforcementMode.None;
        /// Gets lockdown policy as applied to a file.
        public static SystemEnforcementMode GetLockdownPolicy(string path, System.Runtime.InteropServices.SafeHandle handle)
        internal static bool IsClassInApprovedList(Guid clsid)
            throw new NotImplementedException("SystemPolicy.IsClassInApprovedList not implemented");
        /// Gets the system wide script file policy enforcement for an open file.
        /// Based on system WDAC (Windows Defender Application Control) or AppLocker policies.
        /// <param name="filePath">Script file path for policy check.</param>
        /// <param name="fileStream">FileStream object to script file path.</param>
        /// <returns>Policy check result for script file.</returns>
        public static SystemScriptFileEnforcement GetFilePolicyEnforcement(
            string filePath,
            System.IO.FileStream fileStream)
            return SystemScriptFileEnforcement.None;
    /// How the policy is being enforced.
    public enum SystemEnforcementMode
        /// Not enforced at all
        /// Enabled - allow, but audit
        Audit = 1,
        /// Enabled, enforce restrictions
        Enforce = 2
    /// System wide policy enforcement for a specific script file.
    public enum SystemScriptFileEnforcement
        /// No policy enforcement.
        /// Script file is blocked from running.
        Block = 1,
        /// Script file is allowed to run without restrictions (FullLanguage mode).
        Allow = 2,
        /// Script file is allowed to run in ConstrainedLanguage mode only.
        AllowConstrained = 3,
        /// Script file is allowed to run in FullLanguage mode but will emit ConstrainedLanguage restriction audit logs.
        AllowConstrainedAudit = 4
// Porting note: Tracing is absolutely not available on Linux
namespace System.Management.Automation.Tracing
    public abstract class EtwActivity
        /// <param name="activityId"></param>
        public static bool SetActivityId(Guid activityId)
            return Guid.Empty;
        public static Guid GetActivityId()
    public enum PowerShellTraceTask
        /// None.
        /// CreateRunspace.
        CreateRunspace = 1,
        /// ExecuteCommand.
        ExecuteCommand = 2,
        /// Serialization.
        Serialization = 3,
        /// PowerShellConsoleStartup.
        PowerShellConsoleStartup = 4,
    /// Defines Keywords.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028")]
    public enum PowerShellTraceKeywords : ulong
        Runspace = 0x1,
        /// Pipeline.
        Pipeline = 0x2,
        /// Protocol.
        Protocol = 0x4,
        /// Transport.
        Transport = 0x8,
        /// Host.
        Host = 0x10,
        /// Cmdlets.
        Cmdlets = 0x20,
        /// Serializer.
        Serializer = 0x40,
        Session = 0x80,
        /// ManagedPlugIn.
        ManagedPlugIn = 0x100,
        UseAlwaysDebug = 0x2000000000000000,
        UseAlwaysOperational = 0x8000000000000000,
        UseAlwaysAnalytic = 0x4000000000000000,
    public sealed partial class Tracer : System.Management.Automation.Tracing.EtwActivity
        static Tracer() { }
        public void EndpointRegistered(string endpointName, string endpointType, string registeredBy)
        public void EndpointUnregistered(string endpointName, string unregisteredBy)
        public void EndpointDisabled(string endpointName, string disabledBy)
        public void EndpointEnabled(string endpointName, string enabledBy)
        public void EndpointModified(string endpointName, string modifiedBy)
        public void BeginContainerParentJobExecution(Guid containerParentJobInstanceId)
        public void BeginProxyJobExecution(Guid proxyJobInstanceId)
        public void ProxyJobRemoteJobAssociation(Guid proxyJobInstanceId, Guid containerParentJobInstanceId)
        public void EndProxyJobExecution(Guid proxyJobInstanceId)
        public void BeginProxyJobEventHandler(Guid proxyJobInstanceId)
        public void EndProxyJobEventHandler(Guid proxyJobInstanceId)
        public void BeginProxyChildJobEventHandler(Guid proxyChildJobInstanceId)
        public void EndContainerParentJobExecution(Guid containerParentJobInstanceId)
    public sealed class PowerShellTraceSource : IDisposable
        internal PowerShellTraceSource(PowerShellTraceTask task, PowerShellTraceKeywords keywords)
        public bool WriteMessage(string message)
        /// <param name="message1"></param>
        /// <param name="message2"></param>
        public bool WriteMessage(string message1, string message2)
        public bool WriteMessage(string message, Guid instanceId)
        /// <param name="workflowId"></param>
        public void WriteMessage(string className, string methodName, Guid workflowId, string message, params string[] parameters)
        /// <param name="job"></param>
        public void WriteMessage(string className, string methodName, Guid workflowId, Job job, string message, params string[] parameters)
        public bool TraceException(Exception exception)
    /// TraceSourceFactory will return an instance of TraceSource every time GetTraceSource method is called.
    public static class PowerShellTraceSourceFactory
        /// Returns an instance of BaseChannelWriter.
        /// If the Etw is not supported by the platform it will return NullWriter.Instance
        /// A Task and a set of Keywords can be specified in the GetTraceSource method (See overloads).
        ///    The supplied task and keywords are used to pass to the Etw provider in case they are
        /// not defined in the manifest file.
        public static PowerShellTraceSource GetTraceSource()
            return new PowerShellTraceSource(PowerShellTraceTask.None, PowerShellTraceKeywords.None);
        public static PowerShellTraceSource GetTraceSource(PowerShellTraceTask task)
            return new PowerShellTraceSource(task, PowerShellTraceKeywords.None);
        public static PowerShellTraceSource GetTraceSource(PowerShellTraceTask task, PowerShellTraceKeywords keywords)
            return new PowerShellTraceSource(task, keywords);
    internal static class NativeCultureResolver
        internal static void SetThreadUILanguage(Int16 langId) { }
        internal static CultureInfo UICulture
                return CultureInfo.CurrentUICulture; // this is actually wrong, but until we port "hostifaces\NativeCultureResolver.cs" to Nano, this will do and will help avoid build break.
        internal static CultureInfo Culture
                return CultureInfo.CurrentCulture; // this is actually wrong, but until we port "hostifaces\NativeCultureResolver.cs" to Nano, this will do and will help avoid build break.
#endregion TEMPORARY
#pragma warning restore 1591, 1572, 1571, 1573, 1587, 1570, 0067
