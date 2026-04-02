    /// A class used to add pstypename to partial ciminstance
    /// for <see cref="GetCimInstanceCommand"/>, if -KeyOnly
    /// or -SelectProperties is been specified, then add a pstypename:
    /// "Microsoft.Management.Infrastructure.CimInstance#__PartialCIMInstance"
    internal class FormatPartialCimInstance : IObjectPreProcess
        /// Partial ciminstance pstypename.
        internal const string PartialPSTypeName = @"Microsoft.Management.Infrastructure.CimInstance#__PartialCIMInstance";
        /// Add pstypename to the resultobject if necessary.
        /// <param name="resultObject"></param>
        public object Process(object resultObject)
            if (resultObject is CimInstance)
                PSObject obj = PSObject.AsPSObject(resultObject);
                obj.TypeNames.Insert(0, PartialPSTypeName);
            return resultObject;
    /// Implements operations of get-ciminstance cmdlet.
    internal class CimGetInstance : CimAsyncOperation
        /// Initializes a new instance of the <see cref="CimGetInstance"/> class.
        /// Constructor
        public CimGetInstance() : base()
        /// Base on parametersetName to retrieve ciminstances
        public void GetCimInstance(GetCimInstanceCommand cmdlet)
            GetCimInstanceInternal(cmdlet);
        /// Refactor to be reused by Get-CimInstance;Remove-CimInstance;Set-CimInstance
        protected void GetCimInstanceInternal(CimBaseCommand cmdlet)
                GetComputerName(cmdlet));
            string nameSpace;
            bool isGetCimInstanceCommand = cmdlet is GetCimInstanceCommand;
            CimInstance targetCimInstance = null;
                case CimBaseCommand.CimInstanceComputerSet:
                        targetCimInstance = GetCimInstanceParameter(cmdlet);
                        CimSessionProxy proxy = CreateSessionProxy(computerName, targetCimInstance, cmdlet);
                        if (isGetCimInstanceCommand)
                            SetPreProcess(proxy, cmdlet as GetCimInstanceCommand);
                case CimBaseCommand.ClassNameComputerSet:
                case CimBaseCommand.QueryComputerSet:
                case CimBaseCommand.ResourceUriComputerSet:
                case CimBaseCommand.ClassNameSessionSet:
                case CimBaseCommand.CimInstanceSessionSet:
                case CimBaseCommand.QuerySessionSet:
                case CimBaseCommand.ResourceUriSessionSet:
                    foreach (CimSession session in GetCimSession(cmdlet))
                    nameSpace = ConstValue.GetNamespace(GetNamespace(cmdlet));
                    if (IsClassNameQuerySet(cmdlet))
                        string query = CreateQuery(cmdlet);
                        DebugHelper.WriteLogEx(@"Query = {0}", 1, query);
                            proxy.QueryInstancesAsync(nameSpace,
                                ConstValue.GetQueryDialectWithDefault(GetQueryDialect(cmdlet)),
                                query);
                            proxy.EnumerateInstancesAsync(nameSpace, GetClassName(cmdlet));
                        CimInstance instance = GetCimInstanceParameter(cmdlet);
                        nameSpace = ConstValue.GetNamespace(instance.CimSystemProperties.Namespace);
                            proxy.GetInstanceAsync(nameSpace, instance);
                            GetQuery(cmdlet));
                        proxy.EnumerateInstancesAsync(GetNamespace(cmdlet), GetClassName(cmdlet));
        #region bridge methods to read properties from cmdlet
        protected static string[] GetComputerName(CimBaseCommand cmdlet)
            if (cmdlet is GetCimInstanceCommand)
                return (cmdlet as GetCimInstanceCommand).ComputerName;
            else if (cmdlet is RemoveCimInstanceCommand)
                return (cmdlet as RemoveCimInstanceCommand).ComputerName;
            else if (cmdlet is SetCimInstanceCommand)
                return (cmdlet as SetCimInstanceCommand).ComputerName;
        protected static string GetNamespace(CimBaseCommand cmdlet)
                return (cmdlet as GetCimInstanceCommand).Namespace;
                return (cmdlet as RemoveCimInstanceCommand).Namespace;
                return (cmdlet as SetCimInstanceCommand).Namespace;
        protected static CimSession[] GetCimSession(CimBaseCommand cmdlet)
                return (cmdlet as GetCimInstanceCommand).CimSession;
                return (cmdlet as RemoveCimInstanceCommand).CimSession;
                return (cmdlet as SetCimInstanceCommand).CimSession;
        protected static string GetClassName(CimBaseCommand cmdlet)
                return (cmdlet as GetCimInstanceCommand).ClassName;
        protected static string GetQuery(CimBaseCommand cmdlet)
                return (cmdlet as GetCimInstanceCommand).Query;
                return (cmdlet as RemoveCimInstanceCommand).Query;
                return (cmdlet as SetCimInstanceCommand).Query;
        internal static bool IsClassNameQuerySet(CimBaseCommand cmdlet)
            if (cmdlet is GetCimInstanceCommand cmd)
                if (cmd.QueryDialect != null || cmd.SelectProperties != null || cmd.Filter != null)
        protected static string CreateQuery(CimBaseCommand cmdlet)
                StringBuilder propertyList = new();
                if (cmd.SelectProperties == null)
                    propertyList.Append('*');
                    foreach (string property in cmd.SelectProperties)
                        if (propertyList.Length > 0)
                            propertyList.Append(',');
                        propertyList.Append(property);
                return (cmd.Filter == null) ?
                    string.Format(CultureInfo.CurrentUICulture, queryWithoutWhere, propertyList, cmd.ClassName) :
                    string.Format(CultureInfo.CurrentUICulture, queryWithWhere, propertyList, cmd.ClassName, cmd.Filter);
        protected static string GetQueryDialect(CimBaseCommand cmdlet)
                return (cmdlet as GetCimInstanceCommand).QueryDialect;
                return (cmdlet as RemoveCimInstanceCommand).QueryDialect;
                return (cmdlet as SetCimInstanceCommand).QueryDialect;
        protected static CimInstance GetCimInstanceParameter(CimBaseCommand cmdlet)
                return (cmdlet as GetCimInstanceCommand).CimInstance;
                return (cmdlet as RemoveCimInstanceCommand).CimInstance;
                return (cmdlet as SetCimInstanceCommand).CimInstance;
        #region help methods
            CimBaseCommand cmdlet)
                GetCimInstanceCommand getCimInstance = cmdlet as GetCimInstanceCommand;
                proxy.KeyOnly = getCimInstance.KeyOnly;
                proxy.Shallow = getCimInstance.Shallow;
                proxy.OperationTimeout = getCimInstance.OperationTimeoutSec;
                if (getCimInstance.ResourceUri != null)
                    proxy.ResourceUri = getCimInstance.ResourceUri;
                RemoveCimInstanceCommand removeCimInstance = cmdlet as RemoveCimInstanceCommand;
                proxy.OperationTimeout = removeCimInstance.OperationTimeoutSec;
                if (removeCimInstance.ResourceUri != null)
                    proxy.ResourceUri = removeCimInstance.ResourceUri;
                CimRemoveCimInstanceContext context = new(
                    ConstValue.GetNamespace(removeCimInstance.Namespace),
                    proxy);
                SetCimInstanceCommand setCimInstance = cmdlet as SetCimInstanceCommand;
                proxy.OperationTimeout = setCimInstance.OperationTimeoutSec;
                if (setCimInstance.ResourceUri != null)
                    proxy.ResourceUri = setCimInstance.ResourceUri;
                CimSetCimInstanceContext context = new(
                    ConstValue.GetNamespace(setCimInstance.Namespace),
                    setCimInstance.Property,
                    proxy,
                    cmdlet.ParameterSetName,
                    setCimInstance.PassThru);
        protected CimSessionProxy CreateSessionProxy(
            CimSessionProxy proxy = CreateCimSessionProxy(computerName);
            CimBaseCommand cmdlet,
            bool passThru)
            CimSessionProxy proxy = CreateCimSessionProxy(computerName, cimInstance, passThru);
            CimSessionProxy proxy = CreateCimSessionProxy(session, passThru);
        /// Set <see cref="IObjectPreProcess"/> object to proxy to pre-process
        /// the result object if necessary.
        private static void SetPreProcess(CimSessionProxy proxy, GetCimInstanceCommand cmdlet)
            if (cmdlet.KeyOnly || (cmdlet.SelectProperties != null))
                proxy.ObjectPreProcess = new FormatPartialCimInstance();
        /// Wql query format with where clause.
        private const string queryWithWhere = @"SELECT {0} FROM {1} WHERE {2}";
        /// Wql query format without where clause.
        private const string queryWithoutWhere = @"SELECT {0} FROM {1}";
