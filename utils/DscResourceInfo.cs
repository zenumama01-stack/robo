    /// Enumerated values for DSC resource implementation type.
    public enum ImplementedAsType
        /// DSC resource implementation type not known.
        /// DSC resource is implemented using PowerShell module.
        PowerShell = 1,
        /// DSC resource is implemented using a CIM provider.
        Binary = 2,
        /// DSC resource is a composite and implemented using configuration keyword.
        Composite = 3
    /// Contains a DSC resource information.
    public class DscResourceInfo
        /// Initializes a new instance of the DscResourceInfo class.
        /// <param name="name">Name of the DscResource.</param>
        /// <param name="friendlyName">FriendlyName of the DscResource.</param>
        /// <param name="path">Path of the DscResource.</param>
        /// <param name="parentPath">ParentPath of the DscResource.</param>
        /// <param name="context">The execution context for the DscResource.</param>
        internal DscResourceInfo(string name, string friendlyName, string path, string parentPath, ExecutionContext context)
            this.FriendlyName = friendlyName;
            this.Path = path;
            this.ParentPath = parentPath;
            this.Properties = new ReadOnlyCollection<DscResourcePropertyInfo>(new List<DscResourcePropertyInfo>());
        /// Name of the DSC Resource.
        /// Gets or sets resource type name.
        public string ResourceType { get; set; }
        /// Gets or sets friendly name defined for the resource.
        public string FriendlyName { get; set; }
        /// Gets or sets of the file which implements the resource. For the resources which are defined using
        /// MOF file, this will be path to a module which resides in the same folder where schema.mof file is present.
        /// For composite resources, this will be the module which implements the resource.
        /// Gets or sets parent folder, where the resource is defined
        /// It is the folder containing either the implementing module(=Path) or folder containing ".schema.mof".
        /// For native providers, Path will be null and only ParentPath will be present.
        /// Gets or sets a value which indicate how DSC resource is implemented.
        public ImplementedAsType ImplementedAs { get; set; }
        /// Gets or sets company which owns this resource.
        public string CompanyName { get; set; }
        /// Gets or sets properties of the resource.
        public ReadOnlyCollection<DscResourcePropertyInfo> Properties { get; private set; }
        /// Updates properties of the resource.
        /// <param name="properties">Updated properties.</param>
        public void UpdateProperties(IList<DscResourcePropertyInfo> properties)
            if (properties != null)
                this.Properties = new ReadOnlyCollection<DscResourcePropertyInfo>(properties);
        /// Module in which the DscResource is implemented in.
        public string HelpFile { get; internal set; } = string.Empty;
        // HelpFile
    /// Contains a DSC resource property information.
    public sealed class DscResourcePropertyInfo
        /// Initializes a new instance of the DscResourcePropertyInfo class.
        internal DscResourcePropertyInfo()
            this.Values = new ReadOnlyCollection<string>(new List<string>());
        /// Gets or sets name of the property.
        /// Gets or sets type of the property.
        /// Gets or sets a value indicating whether the property is mandatory or not.
        public bool IsMandatory { get; set; }
        /// Gets Values for a resource property.
        public ReadOnlyCollection<string> Values { get; private set; }
        internal void UpdateValues(IList<string> values)
                this.Values = new ReadOnlyCollection<string>(values);
