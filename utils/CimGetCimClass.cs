    /// Containing all information originated from
    /// the parameters of <see cref="GetCimClassCommand"/>
    internal class CimGetCimClassContext : XOperationContextBase
        /// Initializes a new instance of the <see cref="CimGetCimClassContext"/> class.
        /// <param name="methodName"></param>
        /// <param name="propertyName"></param>
        /// <param name="qualifierName"></param>
        internal CimGetCimClassContext(
            string theClassName,
            string theMethodName,
            string thePropertyName,
            string theQualifierName)
            this.ClassName = theClassName;
            this.MethodName = theMethodName;
            this.PropertyName = thePropertyName;
            this.QualifierName = theQualifierName;
        /// The following is the definition of the input parameter "ClassName".
        /// Wildcard expansion should be allowed.
        public string ClassName { get; set; }
        /// The following is the definition of the input parameter "MethodName",
        /// Which may contains wildchar.
        /// Then Filter the <see cref="CimClass"/> by given methodname
        internal string MethodName { get; }
        /// The following is the definition of the input parameter "PropertyName",
        /// Filter the <see cref="CimClass"/> by given property name.
        internal string PropertyName { get; }
        /// The following is the definition of the input parameter "QualifierName",
        /// Filter the <see cref="CimClass"/> by given methodname
        internal string QualifierName { get; }
    /// Implements operations of get-cimclass cmdlet.
    internal sealed class CimGetCimClass : CimAsyncOperation
        /// Initializes a new instance of the <see cref="CimGetCimClass"/> class.
        public CimGetCimClass()
        /// Base on parametersetName to retrieve <see cref="CimClass"/>
        /// <param name="cmdlet"><see cref="GetCimClassCommand"/> object.</param>
        public void GetCimClass(GetCimClassCommand cmdlet)
            string nameSpace = ConstValue.GetNamespace(cmdlet.Namespace);
            string className = cmdlet.ClassName ?? @"*";
            CimGetCimClassContext context = new(
                cmdlet.ClassName,
                cmdlet.MethodName,
                cmdlet.PropertyName,
                cmdlet.QualifierName);
                        IEnumerable<string> computerNames = ConstValue.GetComputerNames(
                                cmdlet.ComputerName);
                            CimSessionProxy proxy = CreateSessionProxy(computerName, cmdlet);
                            proxy.ContextObject = context;
            if (WildcardPattern.ContainsWildcardCharacters(className))
                // retrieve all classes and then filter based on
                // classname, propertyname, methodname, and qualifiername
                    proxy.EnumerateClassesAsync(nameSpace);
                    proxy.GetClassAsync(nameSpace, className);
            GetCimClassCommand cmdlet)
            proxy.Amended = cmdlet.Amended;
            CimSessionProxy proxy = new CimSessionProxyGetCimClass(computerName);
            CimSessionProxy proxy = new CimSessionProxyGetCimClass(session);
