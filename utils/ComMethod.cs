    internal class ComMethodInformation : MethodInformation
        internal readonly Type ReturnType;
        internal readonly int DispId;
        internal readonly COM.INVOKEKIND InvokeKind;
        internal ComMethodInformation(bool hasvarargs, bool hasoptional, ParameterInformation[] arguments, Type returnType, int dispId, COM.INVOKEKIND invokekind)
            : base(hasvarargs, hasoptional, arguments)
            this.ReturnType = returnType;
            this.DispId = dispId;
            this.InvokeKind = invokekind;
    /// Defines a method in the COM object.
    internal class ComMethod
        private readonly Collection<int> _methods = new Collection<int>();
        private readonly COM.ITypeInfo _typeInfo;
        /// Initializes new instance of ComMethod class.
        internal ComMethod(COM.ITypeInfo typeinfo, string name)
            _typeInfo = typeinfo;
        /// Defines the name of the method.
        /// Updates funcdesc for method information.
        /// <param name="index">Index of funcdesc for method in type information.</param>
        internal void AddFuncDesc(int index)
            _methods.Add(index);
        /// Returns the different method overloads signatures.
        internal Collection<string> MethodDefinitions()
            Collection<string> result = new Collection<string>();
            foreach (int index in _methods)
                IntPtr pFuncDesc;
                _typeInfo.GetFuncDesc(index, out pFuncDesc);
                COM.FUNCDESC funcdesc = Marshal.PtrToStructure<COM.FUNCDESC>(pFuncDesc);
                string signature = ComUtil.GetMethodSignatureFromFuncDesc(_typeInfo, funcdesc, false);
                result.Add(signature);
                _typeInfo.ReleaseFuncDesc(pFuncDesc);
        /// Invokes the method on object.
        /// <param name="method">Represents the instance of the method we want to invoke.</param>
        /// <param name="arguments">Parameters to be passed to the method.</param>
        /// <returns>Returns the value of method call.</returns>
        internal object InvokeMethod(PSMethod method, object[] arguments)
                object[] newarguments;
                var methods = ComUtil.GetMethodInformationArray(_typeInfo, _methods, false);
                var bestMethod = (ComMethodInformation)Adapter.GetBestMethodAndArguments(Name, methods, arguments, out newarguments);
                object returnValue = ComInvoker.Invoke(method.baseObject as IDispatch,
                                                       bestMethod.DispId, newarguments,
                                                       ComInvoker.GetByRefArray(bestMethod.parameters,
                                                                                newarguments.Length,
                                                                                isPropertySet: false),
                                                       COM.INVOKEKIND.INVOKE_FUNC);
                Adapter.SetReferences(newarguments, bestMethod, arguments);
                return bestMethod.ReturnType != typeof(void) ? returnValue : AutomationNull.Value;
            catch (TargetInvocationException te)
                // First check if this is a severe exception.
                var innerCom = te.InnerException as COMException;
                if (innerCom == null || innerCom.HResult != ComUtil.DISP_E_MEMBERNOTFOUND)
                    string message = te.InnerException == null ? te.Message : te.InnerException.Message;
                        "ComMethodTargetInvocation",
                        te,
                        method.Name, arguments.Length, message);
                if (ce.HResult != ComUtil.DISP_E_UNKNOWNNAME)
                        "ComMethodCOMException",
                        ce,
                        method.Name, arguments.Length, ce.Message);
