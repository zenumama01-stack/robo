using System.Management;
    /// A command to get WMI Objects.
    [Cmdlet(VerbsCommon.Get, "WmiObject", DefaultParameterSetName = "query",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=113337", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class GetWmiObjectCommand : WmiBaseCmdlet
        /// The WMI class to query.
        [Alias("ClassName")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "query")]
        [Parameter(Position = 1, ParameterSetName = "list")]
        public string Class { get; set; }
        /// To specify whether to get the results recursively.
        [Parameter(ParameterSetName = "list")]
        public SwitchParameter Recurse { get; set; } = false;
        /// The WMI properties to retrieve.
        [Parameter(Position = 1, ParameterSetName = "query")]
            get { return (string[])_property.Clone(); }
            set { _property = value; }
        /// The filter to be used in the search.
        [Parameter(ParameterSetName = "query")]
        public string Filter { get; set; }
        /// If Amended qualifier to use.
        /// If Enumerate Deep flag to use. When 'list' parameter is specified 'EnumerateDeep' parameter is ignored.
        [Parameter(ParameterSetName = "WQLQuery")]
        public SwitchParameter DirectRead { get; set; }
        /// The list of classes.
        public SwitchParameter List { get; set; } = false;
        /// The query string to search for objects.
        [Parameter(Mandatory = true, ParameterSetName = "WQLQuery")]
        public string Query { get; set; }
        private string[] _property = new string[] { "*" };
        /// Uses this.filter, this.wmiClass and this.property to retrieve the filter.
        internal string GetQueryString()
            StringBuilder returnValue = new StringBuilder("select ");
            returnValue.Append(string.Join(", ", _property));
            returnValue.Append(" from ");
            returnValue.Append(Class);
            if (!string.IsNullOrEmpty(Filter))
                returnValue.Append(" where ");
                returnValue.Append(Filter);
        /// Uses filter table to convert the class into WMI understandable language.
        ///            Character   Description Example Match   Comment
        ///             *   Matches zero or more characters starting at the specified position  A*  A,ag,Apple  Supported by PowerShell.
        ///              ?   Matches any character at the specified position ?n  An,in,on (does not match ran)   Supported by PowerShell.
        ///              _   Matches any character at the specified position    _n  An,in,on (does not match ran)   Supported by WMI
        ///             %   Matches zero or more characters starting at the specified position   A%  A,ag,Apple  Supported by WMI
        ///             []  Matches a range of characters  [a-l]ook    Book,cook,look (does not match took)    Supported by WMI and powershell
        ///              []  Matches specified characters   [bc]ook Book,cook, (does not match look)    Supported by WMI and powershell
        ///              ^   Does not Match specified characters. [^bc]ook    Look, took (does not match book, cook)  Supported by WMI.
        internal string GetFilterClassName()
            if (string.IsNullOrEmpty(this.Class))
            string filterClass = string.Copy(this.Class);
            filterClass = filterClass.Replace('*', '%');
            filterClass = filterClass.Replace('?', '_');
            return filterClass;
        internal bool IsLocalizedNamespace(string sNamespace)
            bool toReturn = false;
            if (sNamespace.StartsWith("ms_", StringComparison.OrdinalIgnoreCase))
                toReturn = true;
            return toReturn;
        internal bool ValidateClassFormat()
            string filterClass = this.Class;
            if (string.IsNullOrEmpty(filterClass))
            StringBuilder newClassName = new StringBuilder();
            for (int i = 0; i < filterClass.Length; i++)
                if (char.IsLetterOrDigit(filterClass[i]) ||
                    filterClass[i].Equals('[') || filterClass[i].Equals(']') ||
                    filterClass[i].Equals('*') || filterClass[i].Equals('?') ||
                    filterClass[i].Equals('-'))
                    newClassName.Append(filterClass[i]);
                else if (filterClass[i].Equals('_'))
                    newClassName.Append('[');
                    newClassName.Append(']');
            this.Class = newClassName.ToString();
        /// Gets the ManagementObjectSearcher object.
        internal ManagementObjectSearcher GetObjectList(ManagementScope scope)
            StringBuilder queryStringBuilder = new StringBuilder();
                queryStringBuilder.Append("select * from meta_class");
                string filterClass = GetFilterClassName();
                if (filterClass == null)
                queryStringBuilder.Append("select * from meta_class where __class like '");
                queryStringBuilder.Append(filterClass);
                queryStringBuilder.Append("'");
            ObjectQuery classQuery = new ObjectQuery(queryStringBuilder.ToString());
            EnumerationOptions enumOptions = new EnumerationOptions();
            enumOptions.EnumerateDeep = true;
            enumOptions.UseAmendedQualifiers = this.Amended;
            var searcher = new ManagementObjectSearcher(scope, classQuery, enumOptions);
            return searcher;
            ConnectionOptions options = GetConnectionOption();
            if (this.AsJob)
                RunAsJob("Get-WMIObject");
                if (List.IsPresent)
                    if (!this.ValidateClassFormat())
                        ErrorRecord errorRecord = new ErrorRecord(
                               Thread.CurrentThread.CurrentCulture,
                               "Class", this.Class)),
                       "INVALID_QUERY_IDENTIFIER",
                        errorRecord.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiFilterInvalidClass", this.Class);
                        WriteError(errorRecord);
                        if (this.Recurse.IsPresent)
                            Queue namespaceElement = new Queue();
                            namespaceElement.Enqueue(this.Namespace);
                            while (namespaceElement.Count > 0)
                                string connectNamespace = (string)namespaceElement.Dequeue();
                                ManagementScope scope = new ManagementScope(WMIHelper.GetScopeString(name, connectNamespace), options);
                                    scope.Connect();
                                catch (ManagementException e)
                                         "INVALID_NAMESPACE_IDENTIFIER",
                                    errorRecord.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiNamespaceConnect", connectNamespace, e.Message);
                                catch (System.Runtime.InteropServices.COMException e)
                                catch (System.UnauthorizedAccessException e)
                                ManagementClass namespaceClass = new ManagementClass(scope, new ManagementPath("__Namespace"), new ObjectGetOptions());
                                foreach (ManagementBaseObject obj in namespaceClass.GetInstances())
                                    if (!IsLocalizedNamespace((string)obj["Name"]))
                                        namespaceElement.Enqueue(connectNamespace + "\\" + obj["Name"]);
                                ManagementObjectSearcher searcher = this.GetObjectList(scope);
                                if (searcher == null)
                                foreach (ManagementBaseObject obj in searcher.Get())
                                    WriteObject(obj);
                            ManagementScope scope = new ManagementScope(WMIHelper.GetScopeString(name, this.Namespace), options);
                                errorRecord.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiNamespaceConnect", this.Namespace, e.Message);
                // When -List is not specified and -Recurse is specified, we need the -Class parameter to compose the right query string
                if (this.Recurse.IsPresent && string.IsNullOrEmpty(Class))
                    string errorMsg = string.Format(CultureInfo.InvariantCulture, WmiResources.WmiParameterMissing, "-Class");
                    ErrorRecord er = new ErrorRecord(new InvalidOperationException(errorMsg), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
                string queryString = string.IsNullOrEmpty(this.Query) ? GetQueryString() : this.Query;
                ObjectQuery query = new ObjectQuery(queryString.ToString());
                        enumOptions.UseAmendedQualifiers = Amended;
                        enumOptions.DirectRead = DirectRead;
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query, enumOptions);
                        ErrorRecord errorRecord = null;
                        if (e.ErrorCode.Equals(ManagementStatus.InvalidClass))
                            string className = GetClassNameFromQuery(queryString);
                            string errorMsg = string.Format(CultureInfo.InvariantCulture, WmiResources.WmiQueryFailure,
                                                        e.Message, className);
                            errorRecord = new ErrorRecord(new ManagementException(errorMsg), "GetWMIManagementException", ErrorCategory.InvalidType, null);
                        else if (e.ErrorCode.Equals(ManagementStatus.InvalidQuery))
                                                        e.Message, queryString);
                            errorRecord = new ErrorRecord(new ManagementException(errorMsg), "GetWMIManagementException", ErrorCategory.InvalidArgument, null);
                        else if (e.ErrorCode.Equals(ManagementStatus.InvalidNamespace))
                                                        e.Message, this.Namespace);
                            errorRecord = new ErrorRecord(e, "GetWMIManagementException", ErrorCategory.InvalidOperation, null);
                        ErrorRecord errorRecord = new ErrorRecord(e, "GetWMICOMException", ErrorCategory.InvalidOperation, null);
        /// Get the class name from a query string.
        /// <param name="query"></param>
        private string GetClassNameFromQuery(string query)
            System.Management.Automation.Diagnostics.Assert(query.Contains("from"),
                                                            "Only get called when ErrorCode is InvalidClass, which means the query string contains 'from' and the class name");
            if (Class != null)
                return Class;
            int fromIndex = query.IndexOf(" from ", StringComparison.OrdinalIgnoreCase);
            string subQuery = query.Substring(fromIndex + " from ".Length);
            string className = subQuery.Split(' ')[0];
