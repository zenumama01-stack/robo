using System.Linq;
    /// the parameters of <see cref="NewCimInstanceCommand"/>
    internal class CimNewCimInstanceContext : XOperationContextBase
        /// Initializes a new instance of the <see cref="CimNewCimInstanceContext"/> class.
        internal CimNewCimInstanceContext(
            CimSessionProxy theProxy,
            string theNamespace)
    /// Implements operations of new-ciminstance cmdlet.
    internal sealed class CimNewCimInstance : CimAsyncOperation
        /// Initializes a new instance of the <see cref="CimNewCimInstance"/> class.
        public CimNewCimInstance()
        /// Base on parametersetName to create ciminstances,
        /// either remotely or locally
        public void NewCimInstance(NewCimInstanceCommand cmdlet)
            CimInstance cimInstance = null;
                            cimInstance = CreateCimInstance(cmdlet.ClassName,
                                cmdlet.Key,
                                cmdlet.Property,
                                cmdlet);
                            nameSpace = cmdlet.Namespace; // passing null is ok for resourceUri set
                            cimInstance = CreateCimInstance("DummyClass",
                            cimInstance = CreateCimInstance(cmdlet.CimClass,
            catch (ArgumentNullException e)
                cmdlet.ThrowTerminatingError(e, action);
            catch (ArgumentException e)
            // return if create client only ciminstance
            if (cmdlet.ClientOnly)
                cmdlet.CmdletOperation.WriteObject(cimInstance, null);
            string target = cimInstance.ToString();
            // create ciminstance on server
                        proxys.Add(CreateSessionProxy(session, cmdlet));
                proxy.ContextObject = new CimNewCimInstanceContext(proxy, nameSpace);
                proxy.CreateInstanceAsync(nameSpace, cimInstance);
        #region Get CimInstance after creation (on server)
        /// Get full <see cref="CimInstance"/> from server based on the key
        internal void GetCimInstance(CimInstance cimInstance, XOperationContextBase context)
            if (context is not CimNewCimInstanceContext newCimInstanceContext)
                DebugHelper.WriteLog("Invalid (null) CimNewCimInstanceContext", 1);
            CimSessionProxy proxy = CreateCimSessionProxy(newCimInstanceContext.Proxy);
            string nameSpace = cimInstance.CimSystemProperties.Namespace ?? newCimInstanceContext.Namespace;
            proxy.GetInstanceAsync(nameSpace, cimInstance);
            NewCimInstanceCommand cmdlet)
            CimSessionProxy proxy = new CimSessionProxyNewCimInstance(computerName, this);
            CimSessionProxy proxy = new CimSessionProxyNewCimInstance(session, this);
        /// Create <see cref="CimInstance"/> with given properties.
        /// <param name="className"></param>
        /// <param name="key"></param>
        /// <param name="properties"></param>
        private CimInstance CreateCimInstance(
            string cimNamespace,
            IEnumerable<string> key,
            IDictionary properties,
            CimInstance cimInstance = new(className, cimNamespace);
            if (properties == null)
            List<string> keys = new();
            if (key != null)
                foreach (string keyName in key)
                    keys.Add(keyName);
            IDictionaryEnumerator enumerator = properties.GetEnumerator();
                CimFlags flag = CimFlags.None;
                string propertyName = enumerator.Key.ToString().Trim();
                if (keys.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
                    flag = CimFlags.Key;
                object propertyValue = GetBaseObject(enumerator.Value);
                DebugHelper.WriteLog("Create and add new property to ciminstance: name = {0}; value = {1}; flags = {2}", 5, propertyName, propertyValue, flag);
                if (propertyValue is PSReference cimReference)
                    CimProperty newProperty = CimProperty.Create(propertyName, GetBaseObject(cimReference.Value), CimType.Reference, flag);
                    cimInstance.CimInstanceProperties.Add(newProperty);
                    CimProperty newProperty = CimProperty.Create(
                        propertyName,
                        propertyValue,
                        flag);
            CimInstance cimInstance = new(cimClass);
            List<string> notfoundProperties = new();
            foreach (string property in properties.Keys)
                if (cimInstance.CimInstanceProperties[property] == null)
                    notfoundProperties.Add(property);
                    cmdlet.ThrowInvalidProperty(notfoundProperties, cmdlet.CimClass.CimSystemProperties.ClassName, @"Property", action, properties);
                object propertyValue = GetBaseObject(properties[property]);
                cimInstance.CimInstanceProperties[property].Value = propertyValue;
        private const string action = @"New-CimInstance";
