//using System.Runtime.Serialization;
//using System.ComponentModel;
//using System.Runtime.InteropServices;
//using System.Globalization;
//using System.Management.Automation;
//using System.Reflection;
    /// Provides a description of a field for use by <see cref="System.Management.Automation.Host.PSHostUserInterface.Prompt"/>.
    /// It is permitted to subclass <see cref="System.Management.Automation.Host.FieldDescription"/>
    public class
    FieldDescription
        /// Initializes a new instance of FieldDescription and defines the Name value.
        /// The name to identify this field description
        /// <paramref name="name"/> is null or empty.
        FieldDescription(string name)
            // the only required parameter is the name.
                throw PSTraceSource.NewArgumentException(nameof(name), DescriptionsStrings.NullOrEmptyErrorTemplate, "name");
        /// Gets the name of the field.
        /// Sets the ParameterTypeName, ParameterTypeFullName, and ParameterAssemblyFullName as a single operation.
        /// The Type that sets the properties.
        SetParameterType(System.Type parameterType)
            SetParameterTypeName(parameterType.Name);
            SetParameterTypeFullName(parameterType.FullName);
            SetParameterAssemblyFullName(parameterType.AssemblyQualifiedName);
        /// Gets the short name of the parameter's type.
        /// The type name of the parameter
        /// If not already set by a call to <see cref="System.Management.Automation.Host.FieldDescription.SetParameterType"/>,
        /// <see cref="string"/> will be used as the type.
        /// <!--The value of ParameterTypeName is the string value returned.
        /// by System.Type.Name.-->
        ParameterTypeName
                if (string.IsNullOrEmpty(parameterTypeName))
                    // the default if the type name is not specified is 'string'
                    SetParameterType(typeof(string));
                return parameterTypeName;
        /// Gets the full string name of the parameter's type.
        ParameterTypeFullName
                if (string.IsNullOrEmpty(parameterTypeFullName))
                return parameterTypeFullName;
        /// Gets the full name of the assembly containing the type identified by ParameterTypeFullName or ParameterTypeName.
        /// If the assembly is not currently loaded in the hosting application's AppDomain, the hosting application needs
        /// to load the containing assembly to access the type information. AssemblyName is used for this purpose.
        ParameterAssemblyFullName
                if (string.IsNullOrEmpty(parameterAssemblyFullName))
                return parameterAssemblyFullName;
        /// A short, human-presentable message to describe and identify the field.  If supplied, a typical implementation of
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.Prompt"/> will use this value instead of
        /// the field name to identify the field to the user.
        /// set to null.
        /// Note that the special character &amp; (ampersand) may be embedded in the label string to identify the next
        /// character in the label as a "hot key" (aka "keyboard accelerator") that the
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.Prompt"/> implementation may use
        /// to allow the user to quickly set input focus to this field.  The implementation of
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.Prompt"/> is responsible for parsing
        /// the label string for this special character and rendering it accordingly.
        /// For example, a field named "SSN" might have "&amp;Social Security Number" as it's label.
        /// If no label is set, then the empty string is returned.
                Dbg.Assert(label != null, "label should not be null");
                label = value;
                Dbg.Assert(helpMessage != null, "helpMessage should not be null");
                return helpMessage;
                helpMessage = value;
        /// Gets and sets whether a value must be supplied for this field.
        IsMandatory
                return isMandatory;
                isMandatory = value;
        /// Gets and sets the default value, if any, for the implementation of <see cref="System.Management.Automation.Host.PSHostUserInterface.Prompt"/>
        /// to pre-populate its UI with. This is a PSObject instance so that the value can be serialized, converted,
        /// manipulated like any pipeline object.
        /// It is up to the implementer of <see cref="System.Management.Automation.Host.PSHostUserInterface.Prompt"/> to decide if it
        /// can make use of the object in its presentation of the fields prompt.
        PSObject
        DefaultValue
                // null is allowed.
                defaultValue = value;
        /// Gets the Attribute classes that apply to the field. In the case that <see cref="System.Management.Automation.Host.PSHostUserInterface.Prompt"/>
        /// is being called from the engine, this will contain the set of prompting attributes that are attached to a
        /// cmdlet parameter declaration.
        Collection<Attribute>
        Attributes
            get { return metadata ??= new Collection<Attribute>(); }
        /// For use by remoting serialization.
        /// <param name="nameOfType"></param>
        /// If <paramref name="nameOfType"/> is null.
        SetParameterTypeName(string nameOfType)
            if (string.IsNullOrEmpty(nameOfType))
                throw PSTraceSource.NewArgumentException(nameof(nameOfType), DescriptionsStrings.NullOrEmptyErrorTemplate, "nameOfType");
            parameterTypeName = nameOfType;
        /// <param name="fullNameOfType"></param>
        /// If <paramref name="fullNameOfType"/> is null.
        SetParameterTypeFullName(string fullNameOfType)
            if (string.IsNullOrEmpty(fullNameOfType))
                throw PSTraceSource.NewArgumentException(nameof(fullNameOfType), DescriptionsStrings.NullOrEmptyErrorTemplate, "fullNameOfType");
            parameterTypeFullName = fullNameOfType;
        /// <param name="fullNameOfAssembly"></param>
        /// If <paramref name="fullNameOfAssembly"/> is null.
        SetParameterAssemblyFullName(string fullNameOfAssembly)
            if (string.IsNullOrEmpty(fullNameOfAssembly))
                throw PSTraceSource.NewArgumentException(nameof(fullNameOfAssembly), DescriptionsStrings.NullOrEmptyErrorTemplate, "fullNameOfAssembly");
            parameterAssemblyFullName = fullNameOfAssembly;
        /// Indicates if this field description was
        /// modified by the remoting protocol layer.
        /// <remarks>Used by the console host to
        /// determine if this field description was
        /// modified by the remoting protocol layer
        /// and take appropriate actions</remarks>
        internal bool ModifiedByRemotingProtocol
                return modifiedByRemotingProtocol;
                modifiedByRemotingProtocol = value;
        /// Indicates if this field description
        /// is coming from a remote host.
        /// not cast strings to an arbitrary type,
        /// but let the server-side do the type conversion
        internal bool IsFromRemoteHost
                return isFromRemoteHost;
                isFromRemoteHost = value;
        private readonly string name = null;
        private string label = string.Empty;
        private string parameterTypeName = null;
        private string parameterTypeFullName = null;
        private string parameterAssemblyFullName = null;
        private bool isMandatory = true;
        private PSObject defaultValue = null;
        private Collection<Attribute> metadata = new Collection<Attribute>();
        private bool modifiedByRemotingProtocol = false;
        private bool isFromRemoteHost = false;
