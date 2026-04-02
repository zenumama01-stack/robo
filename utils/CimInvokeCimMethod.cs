    /// Implements operations of invoke-cimmethod cmdlet.
    internal sealed class CimInvokeCimMethod : CimAsyncOperation
        /// Containing all necessary information originated from
        /// the parameters of <see cref="InvokeCimMethodCommand"/>
        internal class CimInvokeCimMethodContext : XOperationContextBase
            /// Initializes a new instance of the <see cref="CimInvokeCimMethodContext"/> class.
            /// <param name="theNamespace"></param>
            /// <param name="theCollection"></param>
            /// <param name="theProxy"></param>
            internal CimInvokeCimMethodContext(string theNamespace,
                CimMethodParametersCollection theCollection,
                CimSessionProxy theProxy)
                this.proxy = theProxy;
                this.ParametersCollection = theCollection;
                this.nameSpace = theNamespace;
            /// <para>namespace</para>
            /// <para>parameters collection</para>
            internal CimMethodParametersCollection ParametersCollection { get; }
        /// Initializes a new instance of the <see cref="CimInvokeCimMethod"/> class.
        public CimInvokeCimMethod()
        public void InvokeCimMethod(InvokeCimMethodCommand cmdlet)
            string action = string.Format(CultureInfo.CurrentUICulture, actionTemplate, cmdlet.MethodName);
                        proxys.Add(CreateSessionProxy(computerName, cmdlet.CimInstance, cmdlet));
                case CimBaseCommand.CimClassComputerSet:
                        proxys.Add(CreateSessionProxy(computerName, cmdlet));
                case CimBaseCommand.CimClassSessionSet:
            CimMethodParametersCollection paramsCollection =
                CreateParametersCollection(cmdlet.Arguments, cmdlet.CimClass, cmdlet.CimInstance, cmdlet.MethodName);
            // Invoke methods
                        string target = string.Format(CultureInfo.CurrentUICulture, targetClass, cmdlet.ClassName);
                            nameSpace = cmdlet.Namespace;
                            nameSpace = ConstValue.GetNamespace(cmdlet.Namespace);
                            if (!cmdlet.ShouldProcess(target, action))
                            proxy.InvokeMethodAsync(
                                paramsCollection);
                        string target = string.Format(CultureInfo.CurrentUICulture, targetClass, cmdlet.CimClass.CimSystemProperties.ClassName);
                        nameSpace = ConstValue.GetNamespace(cmdlet.CimClass.CimSystemProperties.Namespace);
                                cmdlet.CimClass.CimSystemProperties.ClassName,
                        // create context object
                        CimInvokeCimMethodContext context = new(
                            paramsCollection,
                        // firstly query instance and then invoke method upon returned instances
                        proxy.QueryInstancesAsync(nameSpace, ConstValue.GetQueryDialectWithDefault(cmdlet.QueryDialect), cmdlet.Query);
                        string target = cmdlet.CimInstance.ToString();
        /// Invoke cimmethod on given <see cref="CimInstance"/>
        public void InvokeCimMethodOnCimInstance(CimInstance cimInstance, XOperationContextBase context, CmdletOperationBase operation)
            CimInvokeCimMethodContext cimInvokeCimMethodContext = context as CimInvokeCimMethodContext;
            Debug.Assert(cimInvokeCimMethodContext != null, "CimInvokeCimMethod::InvokeCimMethodOnCimInstance should has CimInvokeCimMethodContext != NULL.");
            string action = string.Format(CultureInfo.CurrentUICulture, actionTemplate, cimInvokeCimMethodContext.MethodName);
            if (!operation.ShouldProcess(cimInstance.ToString(), action))
            CimSessionProxy proxy = CreateCimSessionProxy(cimInvokeCimMethodContext.Proxy);
                cimInvokeCimMethodContext.Namespace,
                cimInstance,
                cimInvokeCimMethodContext.MethodName,
                cimInvokeCimMethodContext.ParametersCollection);
            InvokeCimMethodCommand cmdlet)
        /// Create <see cref="CimMethodParametersCollection"/> with given key properties.
        /// And/or <see cref="CimClass"/> object.
        /// <param name="cimClass"></param>
        /// <exception cref="ArgumentNullException">See CimProperty.Create.</exception>
        /// <exception cref="ArgumentException">CimProperty.Create.</exception>
        private CimMethodParametersCollection CreateParametersCollection(
            IDictionary parameters,
            CimClass cimClass,
            string methodName)
            CimMethodParametersCollection collection = null;
            if (parameters == null)
                return collection;
            else if (parameters.Count == 0)
            collection = new CimMethodParametersCollection();
            IDictionaryEnumerator enumerator = parameters.GetEnumerator();
            while (enumerator.MoveNext())
                string parameterName = enumerator.Key.ToString();
                const CimFlags parameterFlags = CimFlags.In;
                object parameterValue = GetBaseObject(enumerator.Value);
                DebugHelper.WriteLog(@"Create parameter name= {0}, value= {1}, flags= {2}.", 4,
                    parameterName,
                    parameterValue,
                    parameterFlags);
                CimMethodParameter parameter = null;
                CimMethodDeclaration declaration = null;
                string className = null;
                if (cimClass != null)
                    className = cimClass.CimSystemProperties.ClassName;
                    declaration = cimClass.CimClassMethods[methodName];
                    if (declaration == null)
                        throw new ArgumentException(string.Format(
                                CultureInfo.CurrentUICulture, CimCmdletStrings.InvalidMethod, methodName, className));
                else if (cimInstance != null)
                    className = cimInstance.CimClass.CimSystemProperties.ClassName;
                    declaration = cimInstance.CimClass.CimClassMethods[methodName];
                if (declaration != null)
                    CimMethodParameterDeclaration paramDeclaration = declaration.Parameters[parameterName];
                    if (paramDeclaration == null)
                            CultureInfo.CurrentUICulture, CimCmdletStrings.InvalidMethodParameter, parameterName, methodName, className));
                    parameter = CimMethodParameter.Create(
                        paramDeclaration.CimType,
                    // FIXME: check in/out qualifier
                    // parameterFlags = paramDeclaration.Qualifiers;
                    if (parameterValue == null)
                        // try the best to get the type while value is null
                            CimType.String,
                        CimType referenceType = CimType.Unknown;
                        object referenceObject = GetReferenceOrReferenceArrayObject(parameterValue, ref referenceType);
                        if (referenceObject != null)
                                referenceObject,
                                referenceType,
                if (parameter != null)
                    collection.Add(parameter);
        /// Operation target.
        private const string targetClass = @"{0}";
        /// Action.
        private const string actionTemplate = @"Invoke-CimMethod: {0}";
