using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
    #region Parameter Set Resolving Classes
    /// Define class <c>ParameterDefinitionEntry</c>.
    internal class ParameterDefinitionEntry
        /// Initializes a new instance of the <see cref="ParameterDefinitionEntry"/> class.
        /// <param name="parameterSetName"></param>
        /// <param name="mandatory"></param>
        internal ParameterDefinitionEntry(string parameterSetName, bool mandatory)
            this.IsMandatory = mandatory;
            this.ParameterSetName = parameterSetName;
        /// Property ParameterSetName.
        internal string ParameterSetName { get; }
        /// Whether the parameter is mandatory to the set.
        internal bool IsMandatory { get; }
    /// Define class <c>ParameterSetEntry</c>.
    internal class ParameterSetEntry
        /// Initializes a new instance of the <see cref="ParameterSetEntry"/> class.
        /// <param name="mandatoryParameterCount"></param>
        internal ParameterSetEntry(uint mandatoryParameterCount)
            this.MandatoryParameterCount = mandatoryParameterCount;
            this.IsDefaultParameterSet = false;
            reset();
        /// <param name="toClone"></param>
        internal ParameterSetEntry(ParameterSetEntry toClone)
            this.MandatoryParameterCount = toClone.MandatoryParameterCount;
            this.IsDefaultParameterSet = toClone.IsDefaultParameterSet;
        internal ParameterSetEntry(uint mandatoryParameterCount, bool isDefault)
            this.IsDefaultParameterSet = isDefault;
        /// Reset the internal status.
        internal void reset()
            this.SetMandatoryParameterCount = this.SetMandatoryParameterCountAtBeginProcess;
            this.IsValueSet = this.IsValueSetAtBeginProcess;
        /// Property <c>DefaultParameterSet</c>
        internal bool IsDefaultParameterSet { get; }
        /// Property <c>MandatoryParameterCount</c>
        internal uint MandatoryParameterCount { get; } = 0;
        /// Property <c>IsValueSet</c>
        internal bool IsValueSet { get; set; }
        /// Property <c>IsValueSetAtBeginProcess</c>
        internal bool IsValueSetAtBeginProcess { get; set; }
        /// Property <c>SetMandatoryParameterCount</c>
        internal uint SetMandatoryParameterCount { get; set; } = 0;
        /// Property <c>SetMandatoryParameterCountAtBeginProcess</c>
        internal uint SetMandatoryParameterCountAtBeginProcess { get; set; } = 0;
    /// Define class <c>ParameterBinder</c>.
    internal class ParameterBinder
        /// Initializes a new instance of the <see cref="ParameterBinder"/> class.
        /// <param name="parameters"></param>
        /// <param name="sets"></param>
        internal ParameterBinder(
            Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters,
            Dictionary<string, ParameterSetEntry> sets)
            this.CloneParameterEntries(parameters, sets);
        #region Two dictionaries used to determine the bound parameter set
        /// Define the parameter definition entries,
        /// each parameter may belong a set of parameterSets, each parameter set
        /// are defined by a <seealso cref="ParameterDefinitionEntry"/>.
        private Dictionary<string, HashSet<ParameterDefinitionEntry>> parameterDefinitionEntries;
        /// Define parameter set entries,
        /// each cmdlet has a list of parameter set, each has number of mandatory parameters, etc.
        /// This data structure is used to track the number of mandatory parameter has been set for
        /// current parameterset, whether the parameter set was been set by user.
        private Dictionary<string, ParameterSetEntry> parameterSetEntries;
        /// Used to remember the set of parameterset were set
        /// if any conflict occurred with current parameter,
        /// throw exception
        private List<string> parametersetNamesList = new();
        /// Parameter names list.
        private readonly List<string> parameterNamesList = new();
        /// Used to remember the set of parameterset were set before begin process
        private List<string> parametersetNamesListAtBeginProcess = new();
        /// Parameter names list before begin process.
        private readonly List<string> parameterNamesListAtBeginProcess = new();
        /// Reset the status of parameter set entries
            foreach (KeyValuePair<string, ParameterSetEntry> setEntry in parameterSetEntries)
                setEntry.Value.reset();
            this.parametersetNamesList.Clear();
            foreach (string parametersetName in this.parametersetNamesListAtBeginProcess)
                this.parametersetNamesList.Add(parametersetName);
            this.parameterNamesList.Clear();
            foreach (string parameterName in this.parameterNamesListAtBeginProcess)
                this.parameterNamesList.Add(parameterName);
        /// A given parameter's value was set by cmdlet caller,
        /// check and change the status of parameter set,
        /// throw exception if confliction occurred
        /// <param name="parameterName"></param>
        /// <exception cref="PSArgumentException">Throw if conflict parameter was set.</exception>
        internal void SetParameter(string parameterName, bool isBeginProcess)
            DebugHelper.WriteLogEx("ParameterName = {0}, isBeginProcess = {1}", 0, parameterName, isBeginProcess);
            if (this.parameterNamesList.Contains(parameterName))
                DebugHelper.WriteLogEx("ParameterName {0} is already bound ", 1, parameterName);
                if (isBeginProcess)
                    this.parameterNamesListAtBeginProcess.Add(parameterName);
            if (this.parametersetNamesList.Count == 0)
                List<string> nameset = new();
                foreach (ParameterDefinitionEntry parameterDefinitionEntry in this.parameterDefinitionEntries[parameterName])
                    DebugHelper.WriteLogEx("parameterset name = '{0}'; mandatory = '{1}'", 1, parameterDefinitionEntry.ParameterSetName, parameterDefinitionEntry.IsMandatory);
                    ParameterSetEntry psEntry = this.parameterSetEntries[parameterDefinitionEntry.ParameterSetName];
                    if (psEntry == null)
                    if (parameterDefinitionEntry.IsMandatory)
                        psEntry.SetMandatoryParameterCount++;
                            psEntry.SetMandatoryParameterCountAtBeginProcess++;
                        DebugHelper.WriteLogEx("parameterset name = '{0}'; SetMandatoryParameterCount = '{1}'", 1, parameterDefinitionEntry.ParameterSetName, psEntry.SetMandatoryParameterCount);
                    if (!psEntry.IsValueSet)
                        psEntry.IsValueSet = true;
                            psEntry.IsValueSetAtBeginProcess = true;
                    nameset.Add(parameterDefinitionEntry.ParameterSetName);
                this.parametersetNamesList = nameset;
                    this.parametersetNamesListAtBeginProcess = nameset;
                foreach (ParameterDefinitionEntry entry in this.parameterDefinitionEntries[parameterName])
                    if (this.parametersetNamesList.Contains(entry.ParameterSetName))
                        nameset.Add(entry.ParameterSetName);
                        if (entry.IsMandatory)
                            ParameterSetEntry psEntry = this.parameterSetEntries[entry.ParameterSetName];
                            DebugHelper.WriteLogEx("parameterset name = '{0}'; SetMandatoryParameterCount = '{1}'",
                                1,
                                entry.ParameterSetName,
                                psEntry.SetMandatoryParameterCount);
                if (nameset.Count == 0)
                    throw new PSArgumentException(CimCmdletStrings.UnableToResolveParameterSetName);
        /// Get the parameter set name based on current binding results.
        internal string GetParameterSet()
            string boundParameterSetName = null;
            string defaultParameterSetName = null;
            List<string> noMandatoryParameterSet = new();
            // Looking for parameter set which have mandatory parameters
            foreach (string parameterSetName in this.parameterSetEntries.Keys)
                ParameterSetEntry entry = this.parameterSetEntries[parameterSetName];
                DebugHelper.WriteLogEx(
                    "parameterset name = {0}, {1}/{2} mandatory parameters.",
                    parameterSetName,
                    entry.SetMandatoryParameterCount,
                    entry.MandatoryParameterCount);
                // Ignore the parameter set which has no mandatory parameter firstly
                if (entry.MandatoryParameterCount == 0)
                    if (entry.IsDefaultParameterSet)
                        defaultParameterSetName = parameterSetName;
                    if (entry.IsValueSet)
                        noMandatoryParameterSet.Add(parameterSetName);
                if ((entry.SetMandatoryParameterCount == entry.MandatoryParameterCount) &&
                    this.parametersetNamesList.Contains(parameterSetName))
                    if (boundParameterSetName != null)
                    boundParameterSetName = parameterSetName;
            // Looking for parameter set which has no mandatory parameters
            if (boundParameterSetName == null)
                // throw if there are > 1 parameter set
                if (noMandatoryParameterSet.Count > 1)
                else if (noMandatoryParameterSet.Count == 1)
                    boundParameterSetName = noMandatoryParameterSet[0];
            // Looking for default parameter set
            boundParameterSetName ??= defaultParameterSetName;
            // throw if still can not find the parameter set name
            return boundParameterSetName;
        /// Deep clone the parameter entries to member variable.
        private void CloneParameterEntries(
            this.parameterDefinitionEntries = parameters;
            this.parameterSetEntries = new Dictionary<string, ParameterSetEntry>();
            foreach (KeyValuePair<string, ParameterSetEntry> parameterSet in sets)
                this.parameterSetEntries.Add(parameterSet.Key, new ParameterSetEntry(parameterSet.Value));
    /// Base command for all cim cmdlets.
    public class CimBaseCommand : Cmdlet, IDisposable
        #region resolve parameter set name
        /// Check set parameters and set ParameterSetName
        /// Following are special types to be handled
        /// Microsoft.Management.Infrastructure.Options.PacketEncoding
        /// Microsoft.Management.Infrastructure.Options.ImpersonationType
        /// UInt32
        /// Authentication.None  (default value)?
        /// ProxyType.None
        internal void CheckParameterSet()
            if (this.parameterBinder != null)
                    this.ParameterSetName = this.parameterBinder.GetParameterSet();
                finally
                    this.parameterBinder.reset();
            DebugHelper.WriteLog("current parameterset is: " + this.ParameterSetName, 4);
        /// Redirect to parameterBinder to set one parameter.
        internal void SetParameter(object value, string parameterName)
            // Ignore the null value being set,
            // Null value could be set by caller unintentionally,
            // or by powershell to reset the parameter to default value
            // before the next parameter binding, and ProcessRecord call
            if (value == null)
            this.parameterBinder?.SetParameter(parameterName, this.AtBeginProcess);
        #region constructors
        /// Initializes a new instance of the <see cref="CimBaseCommand"/> class.
        internal CimBaseCommand()
            this.disposed = false;
            this.parameterBinder = null;
        internal CimBaseCommand(Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters,
            this.parameterBinder = new ParameterBinder(parameters, sets);
        #region override functions of Cmdlet
        /// StopProcessing method.
        protected override void StopProcessing()
        private bool disposed;
        protected void Dispose(bool disposing)
            if (!this.disposed)
                    DisposeInternal();
                disposed = true;
        /// Clean up resources.
        protected virtual void DisposeInternal()
            this.operation?.Dispose();
        /// Parameter binder used to resolve parameter set name.
        private readonly ParameterBinder parameterBinder;
        /// Async operation handler
        private CimAsyncOperation operation;
        private readonly object myLock = new();
        /// This flag is introduced to resolve the parameter set name
        /// during process record
        /// Whether at begin process time, false means in processrecord.
        private bool atBeginProcess = true;
        internal bool AtBeginProcess
                return this.atBeginProcess;
            set
                this.atBeginProcess = value;
        #region internal properties
        /// Set <see cref="CimAsyncOperation"/> object, to which
        /// current cmdlet will delegate all operations.
        internal CimAsyncOperation AsyncOperation
                return this.operation;
                lock (this.myLock)
                    Debug.Assert(this.operation == null, "Caller should verify that operation is null");
                    this.operation = value;
        /// Get current ParameterSetName of the cmdlet
        internal string ParameterSetName { get; private set; }
        /// Gets/Sets cmdlet operation wrapper object.
        internal virtual CmdletOperationBase CmdletOperation
            get;
            set;
        /// Throw terminating error
        [System.Diagnostics.CodeAnalysis.DoesNotReturn]
        internal void ThrowTerminatingError(Exception exception, string operation)
            ErrorRecord errorRecord = new(exception, operation, ErrorCategory.InvalidOperation, this);
            this.CmdletOperation.ThrowTerminatingError(errorRecord);
        #region internal const strings
        /// Alias CN - computer name.
        internal const string AliasCN = "CN";
        /// Alias ServerName - computer name.
        internal const string AliasServerName = "ServerName";
        /// Alias OT - operation timeout.
        internal const string AliasOT = "OT";
        /// Session set name.
        internal const string SessionSetName = "SessionSet";
        /// Computer set name.
        internal const string ComputerSetName = "ComputerSet";
        /// Class name computer set name.
        internal const string ClassNameComputerSet = "ClassNameComputerSet";
        /// Resource Uri computer set name.
        internal const string ResourceUriComputerSet = "ResourceUriComputerSet";
        /// <see cref="CimInstance"/> computer set name.
        internal const string CimInstanceComputerSet = "CimInstanceComputerSet";
        /// Query computer set name.
        internal const string QueryComputerSet = "QueryComputerSet";
        /// Class name session set name.
        internal const string ClassNameSessionSet = "ClassNameSessionSet";
        /// Resource Uri session set name.
        internal const string ResourceUriSessionSet = "ResourceUriSessionSet";
        /// <see cref="CimInstance"/> session set name.
        internal const string CimInstanceSessionSet = "CimInstanceSessionSet";
        /// Query session set name.
        internal const string QuerySessionSet = "QuerySessionSet";
        /// <see cref="CimClass"/> computer set name.
        internal const string CimClassComputerSet = "CimClassComputerSet";
        /// <see cref="CimClass"/> session set name.
        internal const string CimClassSessionSet = "CimClassSessionSet";
        #region Session related parameter set name
        internal const string ComputerNameSet = "ComputerNameSet";
        internal const string SessionIdSet = "SessionIdSet";
        internal const string InstanceIdSet = "InstanceIdSet";
        internal const string NameSet = "NameSet";
        internal const string CimSessionSet = "CimSessionSet";
        internal const string WSManParameterSet = "WSManParameterSet";
        internal const string DcomParameterSet = "DcomParameterSet";
        internal const string ProtocolNameParameterSet = "ProtocolTypeSet";
        #region register cimindication parameter set name
        internal const string QueryExpressionSessionSet = "QueryExpressionSessionSet";
        internal const string QueryExpressionComputerSet = "QueryExpressionComputerSet";
        /// Credential parameter set.
        internal const string CredentialParameterSet = "CredentialParameterSet";
        /// Certificate parameter set.
        internal const string CertificateParameterSet = "CertificateParameterSet";
        /// CimInstance parameter alias.
        internal const string AliasCimInstance = "CimInstance";
        #region internal helper function
        /// Throw invalid AuthenticationType
        /// <param name="operationName"></param>
        /// <param name="authentication"></param>
        internal void ThrowInvalidAuthenticationTypeError(
            string operationName,
            string parameterName,
            PasswordAuthenticationMechanism authentication)
            string message = string.Format(CultureInfo.CurrentUICulture, CimCmdletStrings.InvalidAuthenticationTypeWithNullCredential,
                authentication,
                ImpersonatedAuthenticationMechanism.None,
                ImpersonatedAuthenticationMechanism.Negotiate,
                ImpersonatedAuthenticationMechanism.Kerberos,
                ImpersonatedAuthenticationMechanism.NtlmDomain);
            PSArgumentOutOfRangeException exception = new(
                parameterName, authentication, message);
            ThrowTerminatingError(exception, operationName);
        /// Throw conflict parameter error.
        /// <param name="conflictParameterName"></param>
        internal void ThrowConflictParameterWasSet(
            string conflictParameterName)
            string message = string.Format(CultureInfo.CurrentUICulture,
                CimCmdletStrings.ConflictParameterWasSet,
                parameterName, conflictParameterName);
            PSArgumentException exception = new(message, parameterName);
        /// Throw not found property error
        internal void ThrowInvalidProperty(
            IEnumerable<string> propertiesList,
            string className,
            IDictionary actualValue)
            StringBuilder propList = new();
            foreach (string property in propertiesList)
                if (propList.Length > 0)
                    propList.Append(',');
                propList.Append(property);
            string message = string.Format(CultureInfo.CurrentUICulture, CimCmdletStrings.CouldNotFindPropertyFromGivenClass,
                className, propList);
                parameterName, actualValue, message);
        /// Create credentials based on given authentication type and PSCredential.
        /// <param name="psCredentials"></param>
        /// <param name="passwordAuthentication"></param>
        internal CimCredential CreateCimCredentials(PSCredential psCredentials,
            PasswordAuthenticationMechanism passwordAuthentication,
            string parameterName)
            DebugHelper.WriteLogEx("PSCredential:{0}; PasswordAuthenticationMechanism:{1}; operationName:{2}; parameterName:{3}.", 0, psCredentials, passwordAuthentication, operationName, parameterName);
            CimCredential credentials = null;
            if (psCredentials != null)
                NetworkCredential networkCredential = psCredentials.GetNetworkCredential();
                DebugHelper.WriteLog("Domain:{0}; UserName:{1}; Password:{2}.", 1, networkCredential.Domain, networkCredential.UserName, psCredentials.Password);
                credentials = new CimCredential(passwordAuthentication, networkCredential.Domain, networkCredential.UserName, psCredentials.Password);
                ImpersonatedAuthenticationMechanism impersonatedAuthentication;
                switch (passwordAuthentication)
                    case PasswordAuthenticationMechanism.Default:
                        impersonatedAuthentication = ImpersonatedAuthenticationMechanism.None;
                    case PasswordAuthenticationMechanism.Negotiate:
                        impersonatedAuthentication = ImpersonatedAuthenticationMechanism.Negotiate;
                    case PasswordAuthenticationMechanism.Kerberos:
                        impersonatedAuthentication = ImpersonatedAuthenticationMechanism.Kerberos;
                    case PasswordAuthenticationMechanism.NtlmDomain:
                        impersonatedAuthentication = ImpersonatedAuthenticationMechanism.NtlmDomain;
                        ThrowInvalidAuthenticationTypeError(operationName, parameterName, passwordAuthentication);
                credentials = new CimCredential(impersonatedAuthentication);
            DebugHelper.WriteLogEx("return credential {0}", 1, credentials);
            return credentials;
