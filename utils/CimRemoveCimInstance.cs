    /// the parameters of <see cref="RemoveCimInstanceCommand"/>
    internal class CimRemoveCimInstanceContext : XOperationContextBase
        /// Initializes a new instance of the <see cref="CimRemoveCimInstanceContext"/> class.
        internal CimRemoveCimInstanceContext(string theNamespace,
    /// Implements operations of remove-ciminstance cmdlet.
    internal sealed class CimRemoveCimInstance : CimGetInstance
        /// Initializes a new instance of the <see cref="CimRemoveCimInstance"/> class.
        public CimRemoveCimInstance()
        public void RemoveCimInstance(RemoveCimInstanceCommand cmdlet)
                    string nameSpace = null;
                        nameSpace = GetCimInstanceParameter(cmdlet).CimSystemProperties.Namespace;
                        nameSpace = ConstValue.GetNamespace(GetCimInstanceParameter(cmdlet).CimSystemProperties.Namespace);
                        proxy.DeleteInstanceAsync(nameSpace, cmdlet.CimInstance);
        /// Remove <see cref="CimInstance"/> from namespace specified in cmdlet
        internal void RemoveCimInstance(CimInstance cimInstance, XOperationContextBase context, CmdletOperationBase cmdlet)
            CimRemoveCimInstanceContext removeContext = context as CimRemoveCimInstanceContext;
            Debug.Assert(removeContext != null, "CimRemoveCimInstance::RemoveCimInstance should has CimRemoveCimInstanceContext != NULL.");
            CimSessionProxy proxy = CreateCimSessionProxy(removeContext.Proxy);
            proxy.DeleteInstanceAsync(removeContext.Namespace, cimInstance);
        private const string action = @"Remove-CimInstance";
