    #region Module Specification class
    /// Represents module specification written in a module manifest (i.e. in RequiredModules member/field).
    /// Module manifest allows 2 forms of module specification:
    /// 1. string - module name
    /// 2. hashtable - [string]ModuleName (required) + [Version]ModuleVersion/RequiredVersion (required) + [Guid]GUID (optional)
    /// so we have a constructor that takes a string and a constructor that takes a hashtable
    /// (so that LanguagePrimitives.ConvertTo can cast a string or a hashtable to this type)
    public class ModuleSpecification
        public ModuleSpecification()
        /// Construct a module specification from the module name.
        /// <param name="moduleName">The module name.</param>
        public ModuleSpecification(string moduleName)
            ArgumentException.ThrowIfNullOrEmpty(moduleName);
            this.Name = moduleName;
            // Alias name of miniumVersion
            this.Version = null;
            this.RequiredVersion = null;
            this.MaximumVersion = null;
            this.Guid = null;
        /// Construct a module specification from a hashtable.
        /// Keys can be ModuleName, ModuleVersion, and Guid.
        /// ModuleName must be convertible to <see cref="string"/>.
        /// ModuleVersion must be convertible to <see cref="Version"/>.
        /// Guid must be convertible to <see cref="Guid"/>.
        /// <param name="moduleSpecification">The module specification as a hashtable.</param>
        public ModuleSpecification(Hashtable moduleSpecification)
            ArgumentNullException.ThrowIfNull(moduleSpecification);
            var exception = ModuleSpecificationInitHelper(this, moduleSpecification);
        /// Initialize moduleSpecification from hashtable. Return exception object, if hashtable cannot be converted.
        /// Return null, in the success case.
        /// <param name="moduleSpecification">Object to initialize.</param>
        /// <param name="hashtable">Contains info about object to initialize.</param>
        internal static Exception ModuleSpecificationInitHelper(ModuleSpecification moduleSpecification, Hashtable hashtable)
                    string field = entry.Key.ToString();
                    if (field.Equals("ModuleName", StringComparison.OrdinalIgnoreCase))
                        moduleSpecification.Name = LanguagePrimitives.ConvertTo<string>(entry.Value);
                    else if (field.Equals("ModuleVersion", StringComparison.OrdinalIgnoreCase))
                        moduleSpecification.Version = LanguagePrimitives.ConvertTo<Version>(entry.Value);
                    else if (field.Equals("RequiredVersion", StringComparison.OrdinalIgnoreCase))
                        moduleSpecification.RequiredVersion = LanguagePrimitives.ConvertTo<Version>(entry.Value);
                    else if (field.Equals("MaximumVersion", StringComparison.OrdinalIgnoreCase))
                        moduleSpecification.MaximumVersion = LanguagePrimitives.ConvertTo<string>(entry.Value);
                        ModuleCmdletBase.GetMaximumVersion(moduleSpecification.MaximumVersion);
                    else if (field.Equals("GUID", StringComparison.OrdinalIgnoreCase))
                        moduleSpecification.Guid = LanguagePrimitives.ConvertTo<Guid?>(entry.Value);
                        badKeys.Append(entry.Key.ToString());
            // catch all exceptions here, we are going to report them via return value.
            // Example of caught exception: one of conversions to Version failed.
            if (badKeys.Length != 0)
                message = StringUtil.Format(Modules.InvalidModuleSpecificationMember, "ModuleName, ModuleVersion, RequiredVersion, GUID", badKeys);
                return new ArgumentException(message);
            if (string.IsNullOrEmpty(moduleSpecification.Name))
                message = StringUtil.Format(Modules.RequiredModuleMissingModuleName);
                return new MissingMemberException(message);
            if (moduleSpecification.RequiredVersion == null && moduleSpecification.Version == null && moduleSpecification.MaximumVersion == null)
                message = StringUtil.Format(Modules.RequiredModuleMissingModuleVersion);
            if (moduleSpecification.RequiredVersion != null && moduleSpecification.Version != null)
                message = StringUtil.Format(SessionStateStrings.GetContent_TailAndHeadCannotCoexist, "ModuleVersion", "RequiredVersion");
            if (moduleSpecification.RequiredVersion != null && moduleSpecification.MaximumVersion != null)
                message = StringUtil.Format(SessionStateStrings.GetContent_TailAndHeadCannotCoexist, "MaximumVersion", "RequiredVersion");
        internal string GetRequiredModuleNotFoundVersionMessage()
            if (RequiredVersion is not null)
                return StringUtil.Format(
                    Modules.RequiredModuleNotFoundRequiredVersion,
                    RequiredVersion);
            bool hasVersion = Version is not null;
            bool hasMaximumVersion = MaximumVersion is not null;
            if (hasVersion && hasMaximumVersion)
                    Modules.RequiredModuleNotFoundModuleAndMaximumVersion,
                    Version,
                    MaximumVersion);
            if (hasVersion)
                    Modules.RequiredModuleNotFoundModuleVersion,
                    Version);
            if (hasMaximumVersion)
                    Modules.RequiredModuleNotFoundMaximumVersion,
                Modules.RequiredModuleNotFoundWithoutVersion,
        internal ModuleSpecification(PSModuleInfo moduleInfo)
            ArgumentNullException.ThrowIfNull(moduleInfo);
            this.Name = moduleInfo.Name;
            this.Version = moduleInfo.Version;
            this.Guid = moduleInfo.Guid;
        /// Implements ToString() for a module specification. If the specification
        /// just contains a Name, then that is returned as is. Otherwise, the object is
        /// formatted as a PowerSHell hashtable.
            if (Guid == null && Version == null && RequiredVersion == null && MaximumVersion == null)
            var moduleSpecBuilder = new StringBuilder();
            moduleSpecBuilder.Append("@{ ModuleName = '").Append(Name).Append('\'');
            if (Guid != null)
                moduleSpecBuilder.Append("; Guid = '{").Append(Guid).Append("}' ");
            if (RequiredVersion != null)
                moduleSpecBuilder.Append("; RequiredVersion = '").Append(RequiredVersion).Append('\'');
                if (Version != null)
                    moduleSpecBuilder.Append("; ModuleVersion = '").Append(Version).Append('\'');
                if (MaximumVersion != null)
                    moduleSpecBuilder.Append("; MaximumVersion = '").Append(MaximumVersion).Append('\'');
            moduleSpecBuilder.Append(" }");
            return moduleSpecBuilder.ToString();
        /// Parse the specified string into a ModuleSpecification object.
        /// <param name="input">The module specification string.</param>
        /// <param name="result">The ModuleSpecification object.</param>
        public static bool TryParse(string input, out ModuleSpecification result)
                Hashtable hashtable;
                if (Parser.TryParseAsConstantHashtable(input, out hashtable))
                    result = new ModuleSpecification(hashtable);
                // Ignoring the exceptions to return false
        /// Copy the module specification while normalizing the name
        /// so that paths become absolute and use the right directory separators.
        /// <param name="context">The current execution context. Used for path normalization.</param>
        /// <param name="basePath">The base path where a relative path should be interpreted with respect to.</param>
        /// <returns>A fresh module specification object with the name normalized for use internally.</returns>
        internal ModuleSpecification WithNormalizedName(ExecutionContext context, string basePath)
            // Save allocating a new module spec if we don't need to change anything
            if (!ModuleIntrinsics.IsModuleNamePath(Name))
            return new ModuleSpecification()
                Guid = Guid,
                MaximumVersion = MaximumVersion,
                Version = Version,
                RequiredVersion = RequiredVersion,
                Name = ModuleIntrinsics.NormalizeModuleName(Name, basePath, context)
        /// The module name.
        /// The module GUID, if specified.
        public Guid? Guid { get; internal set; }
        /// The module version number if specified, otherwise null.
        /// The module maxVersion number if specified, otherwise null.
        public string MaximumVersion { get; internal set; }
        /// The exact version of the module if specified, otherwise null.
        public Version RequiredVersion { get; internal set; }
    /// Compares two ModuleSpecification objects for equality.
    internal class ModuleSpecificationComparer : IEqualityComparer<ModuleSpecification>
        /// Check if two module specifications are property-wise equal.
        /// <returns>True if the specifications are equal, false otherwise.</returns>
        public bool Equals(ModuleSpecification x, ModuleSpecification y)
            if (x == y)
            return x != null && y != null
                && string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)
                && Guid.Equals(x.Guid, y.Guid)
                && Version.Equals(x.RequiredVersion, y.RequiredVersion)
                && Version.Equals(x.Version, y.Version)
                && string.Equals(x.MaximumVersion, y.MaximumVersion);
        /// Get a property-based hashcode for a ModuleSpecification object.
        /// <param name="obj">The module specification for the object.</param>
        /// <returns>A hashcode that is always the same for any module specification with the same properties.</returns>
        public int GetHashCode(ModuleSpecification obj)
            return HashCode.Combine(obj.Name, obj.Guid, obj.RequiredVersion, obj.Version, obj.MaximumVersion);
