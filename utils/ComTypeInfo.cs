    /// A Wrapper class for COM object's Type Information.
    internal class ComTypeInfo
        /// A member with a DISPID equal to –4 is found on a collection interface.
        /// This special member, often called '_NewEnum', returns an interface that enables clients to enumerate objects in a collection.
        internal const int DISPID_NEWENUM = -4;
        /// A member with a DISPID equal to 0 is considered a default member.
        /// Default members in COM can be transformed to default members in .NET (indexers in C#).
        internal const int DISPID_DEFAULTMEMBER = 0;
        /// Member variables.
        private readonly Dictionary<string, ComProperty> _properties = null;
        private readonly Dictionary<string, ComMethod> _methods = null;
        private readonly COM.ITypeInfo _typeinfo = null;
        private Guid _guid = Guid.Empty;
        /// <param name="info">ITypeInfo object being wrapped by this object.</param>
        internal ComTypeInfo(COM.ITypeInfo info)
            _typeinfo = info;
            _properties = new Dictionary<string, ComProperty>(StringComparer.OrdinalIgnoreCase);
            _methods = new Dictionary<string, ComMethod>(StringComparer.OrdinalIgnoreCase);
            if (_typeinfo != null)
        /// Collection of properties in the COM object.
        internal Dictionary<string, ComProperty> Properties
        /// Collection of methods in the COM object.
        internal Dictionary<string, ComMethod> Methods
        /// Returns the string of the GUID for the type information.
        internal string Clsid
                return _guid.ToString();
        /// If 'DISPID_NEWENUM' member is present, return the InvokeKind;
        /// otherwise, return null.
        internal COM.INVOKEKIND? NewEnumInvokeKind { get; private set; }
        /// Initializes the typeinfo object.
        private void Initialize()
                COM.TYPEATTR typeattr = GetTypeAttr(_typeinfo);
                // Initialize the type information guid
                _guid = typeattr.guid;
                for (int i = 0; i < typeattr.cFuncs; i++)
                    COM.FUNCDESC funcdesc = GetFuncDesc(_typeinfo, i);
                    if (funcdesc.memid == DISPID_NEWENUM)
                        NewEnumInvokeKind = funcdesc.invkind;
                    if ((funcdesc.wFuncFlags & 0x1) == 0x1)
                        // https://msdn.microsoft.com/library/ee488948.aspx
                        // FUNCFLAGS -- FUNCFLAG_FRESTRICTED = 0x1:
                        //     Indicates that the function should not be accessible from macro languages.
                        //     This flag is intended for system-level functions or functions that type browsers should not display.
                        // For IUnknown methods (AddRef, QueryInterface and Release) and IDispatch methods (GetTypeInfoCount, GetTypeInfo, GetIDsOfNames and Invoke)
                        // FUNCFLAG_FRESTRICTED (0x1) is set for the 'wFuncFlags' field
                    string strName = ComUtil.GetNameFromFuncDesc(_typeinfo, funcdesc);
                    switch (funcdesc.invkind)
                            AddProperty(strName, funcdesc, i);
                        case COM.INVOKEKIND.INVOKE_FUNC:
                            AddMethod(strName, i);
        /// Get the typeinfo interface for the given comobject.
        /// <param name="comObject">Reference to com object for which we are getting type information.</param>
        /// <returns>ComTypeInfo object which wraps the ITypeInfo interface of the given COM object.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Code uses the out parameter of 'GetTypeInfo' to check if the call succeeded.")]
        internal static ComTypeInfo GetDispatchTypeInfo(object comObject)
            ComTypeInfo result = null;
            IDispatch disp = comObject as IDispatch;
            if (disp != null)
                COM.ITypeInfo typeinfo = null;
                disp.GetTypeInfo(0, 0, out typeinfo);
                if (typeinfo != null)
                    COM.TYPEATTR typeattr = GetTypeAttr(typeinfo);
                    if ((typeattr.typekind == COM.TYPEKIND.TKIND_INTERFACE))
                        // We have typeinfo for custom interface. Get typeinfo for Dispatch interface.
                        typeinfo = GetDispatchTypeInfoFromCustomInterfaceTypeInfo(typeinfo);
                    if ((typeattr.typekind == COM.TYPEKIND.TKIND_COCLASS))
                        // We have typeinfo for the COClass.  Find the default interface and get typeinfo for default interface.
                        typeinfo = GetDispatchTypeInfoFromCoClassTypeInfo(typeinfo);
                    result = new ComTypeInfo(typeinfo);
        private void AddProperty(string strName, COM.FUNCDESC funcdesc, int index)
            if (!_properties.TryGetValue(strName, out prop))
                prop = new ComProperty(_typeinfo, strName);
                _properties[strName] = prop;
            prop?.UpdateFuncDesc(funcdesc, index);
        private void AddMethod(string strName, int index)
            if (!_methods.TryGetValue(strName, out method))
                method = new ComMethod(_typeinfo, strName);
                _methods[strName] = method;
            method?.AddFuncDesc(index);
        /// Get TypeAttr for the given type information.
        /// <param name="typeinfo">Reference to ITypeInfo from which to get TypeAttr.</param>
        internal static COM.TYPEATTR GetTypeAttr(COM.ITypeInfo typeinfo)
            IntPtr pTypeAttr;
            typeinfo.GetTypeAttr(out pTypeAttr);
            COM.TYPEATTR typeattr = Marshal.PtrToStructure<COM.TYPEATTR>(pTypeAttr);
            typeinfo.ReleaseTypeAttr(pTypeAttr);
            return typeattr;
        /// <param name="typeinfo"></param>
        internal static COM.FUNCDESC GetFuncDesc(COM.ITypeInfo typeinfo, int index)
            typeinfo.GetFuncDesc(index, out pFuncDesc);
            typeinfo.ReleaseFuncDesc(pFuncDesc);
            return funcdesc;
        internal static COM.ITypeInfo GetDispatchTypeInfoFromCustomInterfaceTypeInfo(COM.ITypeInfo typeinfo)
            int href;
            COM.ITypeInfo dispinfo = null;
                // We need the typeinfo for Dispatch Interface
                typeinfo.GetRefTypeOfImplType(-1, out href);
                typeinfo.GetRefTypeInfo(href, out dispinfo);
                // check if the error code is TYPE_E_ELEMENTNOTFOUND.
                // This error code is thrown when we can't IDispatch interface.
                if (ce.HResult != ComUtil.TYPE_E_ELEMENTNOTFOUND)
                    // For other codes, rethrow the exception.
            return dispinfo;
        /// Get the IDispatch Typeinfo from CoClass typeinfo.
        /// <param name="typeinfo">Reference to the type info to which the type descriptor belongs.</param>
        /// <returns>ITypeInfo reference to the Dispatch interface.</returns>
        internal static COM.ITypeInfo GetDispatchTypeInfoFromCoClassTypeInfo(COM.ITypeInfo typeinfo)
            // Get the number of interfaces implemented by this CoClass.
            int count = typeattr.cImplTypes;
            COM.ITypeInfo interfaceinfo = null;
            // For each interface implemented by this coclass
            for (int i = 0; i < count; i++)
                // Get the type information?
                typeinfo.GetRefTypeOfImplType(i, out href);
                typeinfo.GetRefTypeInfo(href, out interfaceinfo);
                typeattr = GetTypeAttr(interfaceinfo);
                // Is this interface IDispatch compatible interface?
                if (typeattr.typekind == COM.TYPEKIND.TKIND_DISPATCH)
                    return interfaceinfo;
                // Nope. Is this a dual interface
                if ((typeattr.wTypeFlags & COM.TYPEFLAGS.TYPEFLAG_FDUAL) != 0)
                    interfaceinfo = GetDispatchTypeInfoFromCustomInterfaceTypeInfo(interfaceinfo);
