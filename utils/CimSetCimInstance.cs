    internal class CimSetCimInstanceContext : XOperationContextBase
        /// Initializes a new instance of the <see cref="CimSetCimInstanceContext"/> class.
        internal CimSetCimInstanceContext(string theNamespace,
            IDictionary theProperty,
            string theParameterSetName,
            this.Property = theProperty;
            this.ParameterSetName = theParameterSetName;
            this.PassThru = passThru;
        /// <para>property value</para>
        internal IDictionary Property { get; }
        /// <para>parameter set name</para>
        /// <para>PassThru value</para>
        internal bool PassThru { get; }
    /// Implements operations of set-ciminstance cmdlet.
    internal sealed class CimSetCimInstance : CimGetInstance
        /// Initializes a new instance of the <see cref="CimSetCimInstance"/> class.
        public CimSetCimInstance()
        /// Base on parametersetName to set ciminstances
        /// <param name="cmdlet"><see cref="SetCimInstanceCommand"/> object.</param>
        public void SetCimInstance(SetCimInstanceCommand cmdlet)
                        // create CimSessionProxySetCimInstance object internally
                        proxys.Add(CreateSessionProxy(computerName, cmdlet.CimInstance, cmdlet, cmdlet.PassThru));
                        proxys.Add(CreateSessionProxy(session, cmdlet, cmdlet.PassThru));
                    string nameSpace = ConstValue.GetNamespace(GetCimInstanceParameter(cmdlet).CimSystemProperties.Namespace);
                        Exception exception = null;
                        CimInstance instance = cmdlet.CimInstance;
                        // For CimInstance parameter sets, Property is an optional parameter
                        if (cmdlet.Property != null)
                            if (!SetProperty(cmdlet.Property, ref instance, ref exception))
                                cmdlet.ThrowTerminatingError(exception, action);
                        proxy.ModifyInstanceAsync(nameSpace, instance);
        /// Set <see cref="CimInstance"/> with properties specified in cmdlet
        public void SetCimInstance(CimInstance cimInstance, CimSetCimInstanceContext context, CmdletOperationBase cmdlet)
            DebugHelper.WriteLog("CimSetCimInstance::SetCimInstance", 4);
            if (!cmdlet.ShouldProcess(cimInstance.ToString(), action))
            if (!SetProperty(context.Property, ref cimInstance, ref exception))
            CimSessionProxy proxy = CreateCimSessionProxy(context.Proxy, context.PassThru);
            proxy.ModifyInstanceAsync(cimInstance.CimSystemProperties.Namespace, cimInstance);
        /// Set the properties value to be modified to the given
        /// <see cref="CimInstance"/>
        /// <param name="terminationMessage"></param>
        private bool SetProperty(IDictionary properties, ref CimInstance cimInstance, ref Exception exception)
            if (properties.Count == 0)
                // simply ignore if empty properties was provided
                object value = GetBaseObject(enumerator.Value);
                string key = enumerator.Key.ToString();
                DebugHelper.WriteLog("Input property name '{0}' with value '{1}'", 1, key, value);
                    CimProperty property = cimInstance.CimInstanceProperties[key];
                    // modify existing property value if found
                    if (property != null)
                        if ((property.Flags & CimFlags.ReadOnly) == CimFlags.ReadOnly)
                            // can not modify ReadOnly property
                            exception = new CimException(string.Format(CultureInfo.CurrentUICulture,
                                CimCmdletStrings.CouldNotModifyReadonlyProperty, key, cimInstance));
                        // allow modify the key property value as long as it is not readonly,
                        // then the modified ciminstance is stand for a different CimInstance
                        DebugHelper.WriteLog("Set property name '{0}' has old value '{1}'", 4, key, property.Value);
                        property.Value = value;
                    else // For dynamic instance, it is valid to add a new property
                        CimProperty newProperty;
                            newProperty = CimProperty.Create(
                                key,
                                value,
                                CimFlags.Property);
                            object referenceObject = GetReferenceOrReferenceArrayObject(value, ref referenceType);
                        catch (CimException e)
                            if (e.NativeErrorCode == NativeErrorCode.Failed)
                                string errorMessage = string.Format(CultureInfo.CurrentUICulture,
                                    CimCmdletStrings.UnableToAddPropertyToInstance,
                                    newProperty.Name,
                                    cimInstance);
                                exception = new CimException(errorMessage, e);
                                exception = e;
                        DebugHelper.WriteLog("Add non-key property name '{0}' with value '{1}'.", 3, key, value);
                catch (Exception e)
                    DebugHelper.WriteLog("Exception {0}", 4, e);
        private const string action = @"Set-CimInstance";
