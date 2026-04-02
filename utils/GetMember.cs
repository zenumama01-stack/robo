    /// Class with member information that this cmdlet writes to the pipeline.
    public class MemberDefinition
        /// Returns the member definition.
            return Definition;
        /// Initializes a new instance of the <see cref="MemberDefinition"/> class.
        public MemberDefinition(string typeName, string name, PSMemberTypes memberType, string definition)
            Definition = definition;
            MemberType = memberType;
        /// Type name.
        public string TypeName { get; }
        /// Member name.
        /// Member type.
        public PSMemberTypes MemberType { get; }
        /// Member definition.
        public string Definition { get; }
    [Cmdlet(VerbsCommon.Get, "Member", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096704", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(MemberDefinition))]
    public class GetMemberCommand : PSCmdlet
        /// The object to retrieve properties from.
        /// The member names to be retrieved.
        public string[] Name { get; set; } = new string[] { "*" };
        /// The member types to be retrieved.
        public PSMemberTypes MemberType { get; set; } = PSMemberTypes.All;
        /// View from which the members are retrieved.
        public PSMemberViewTypes View { get; set; } = PSMemberViewTypes.Adapted | PSMemberViewTypes.Extended;
        private bool _staticParameter = false;
        /// True if we should return static members.
        public SwitchParameter Static
            get { return _staticParameter; }
            set { _staticParameter = value; }
        /// Gives the Member matcher guidance on how vigorous the Match should be.
        /// If set to true all members in a given view + membertype are displayed.
        /// This parameter is added to hide Get/Set property accessor methods by default.
        /// If a user wants to see these methods, -force should be set to true.
                return (_matchOptions == MshMemberMatchOptions.IncludeHidden);
                    // Include hidden members if force parameter is set
                    _matchOptions = MshMemberMatchOptions.IncludeHidden;
                    _matchOptions = MshMemberMatchOptions.None;
        private MshMemberMatchOptions _matchOptions = MshMemberMatchOptions.None;
        private readonly HybridDictionary _typesAlreadyDisplayed = new();
        /// This method implements the ProcessRecord method for get-member command.
            if (this.InputObject == null || this.InputObject == AutomationNull.Value)
            Type baseObjectAsType = null;
            string typeName;
            Adapter staticAdapter = null;
            if (this.Static == true)
                staticAdapter = PSObject.DotNetStaticAdapter;
                object baseObject = this.InputObject.BaseObject;
                baseObjectAsType = baseObject as System.Type ?? baseObject.GetType();
                typeName = baseObjectAsType.FullName;
                var typeNames = this.InputObject.InternalTypeNames;
                if (typeNames.Count != 0)
                    typeName = typeNames[0];
                    // This is never used for display.  It is used only as a key to typesAlreadyDisplayed
                    typeName = "<null>";
            if (_typesAlreadyDisplayed.Contains(typeName))
                _typesAlreadyDisplayed.Add(typeName, string.Empty);
            PSMemberTypes memberTypeToSearch = MemberType;
            PSMemberViewTypes viewToSearch = View;
            if (((View & PSMemberViewTypes.Extended) == 0) &&
                (!typeof(PSMemberSet).ToString().Equals(typeName, StringComparison.OrdinalIgnoreCase)))
                // PSMemberSet is an internal memberset and its properties/methods are populated differently.
                // PSMemberSet instance is created to represent PSExtended, PSAdapted, PSBase, PSObject hidden
                // properties. We should honor extended properties for such case.
                // request is to search dotnet or adapted or both members.
                // dotnet,adapted members cannot be Script*,Note*,Code*
                memberTypeToSearch ^= (PSMemberTypes.AliasProperty | PSMemberTypes.CodeMethod | PSMemberTypes.CodeProperty
                | PSMemberTypes.MemberSet | PSMemberTypes.NoteProperty | PSMemberTypes.PropertySet | PSMemberTypes.ScriptMethod
                | PSMemberTypes.ScriptProperty);
            if (((View & PSMemberViewTypes.Adapted) == 0) && (View & PSMemberViewTypes.Base) == 0)
                // base and adapted are not mentioned in the view so ignore respective properties
                memberTypeToSearch ^= (PSMemberTypes.Property | PSMemberTypes.ParameterizedProperty | PSMemberTypes.Method);
            if (((View & PSMemberViewTypes.Base) == PSMemberViewTypes.Base) &&
                (InputObject.InternalBaseDotNetAdapter == null))
                // the input object don't have a custom adapter..
                // for this case adapted view and base view are the same.
                viewToSearch |= PSMemberViewTypes.Adapted;
            PSMemberInfoCollection<PSMemberInfo> membersToSearch;
                membersToSearch = staticAdapter.BaseGetMembers<PSMemberInfo>(baseObjectAsType);
                Collection<CollectionEntry<PSMemberInfo>> memberCollection = PSObject.GetMemberCollection(viewToSearch);
                membersToSearch = new PSMemberInfoIntegratingCollection<PSMemberInfo>(this.InputObject, memberCollection);
            foreach (string nameElement in this.Name)
                ReadOnlyPSMemberInfoCollection<PSMemberInfo> readOnlyMembers;
                readOnlyMembers = membersToSearch.Match(nameElement, memberTypeToSearch, _matchOptions);
                MemberDefinition[] members = new MemberDefinition[readOnlyMembers.Count];
                int resultCount = 0;
                foreach (PSMemberInfo member in readOnlyMembers)
                        if ((member is PSMethod memberAsPSMethod) && (memberAsPSMethod.IsSpecial))
                    members[resultCount] = new MemberDefinition(typeName, member.Name, member.MemberType, member.ToString());
                    resultCount++;
                Array.Sort<MemberDefinition>(members, 0, resultCount, new MemberComparer());
                for (int index = 0; index < resultCount; index++)
                    this.WriteObject(members[index]);
        private sealed class MemberComparer : System.Collections.Generic.IComparer<MemberDefinition>
            public int Compare(MemberDefinition first, MemberDefinition second)
                int result = string.Compare(first.MemberType.ToString(), second.MemberType.ToString(),
                if (result != 0)
                return string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);
        /// This method implements the End method for get-member command.
            if (_typesAlreadyDisplayed.Count == 0)
                    new InvalidOperationException(GetMember.NoObjectSpecified),
                    "NoObjectInGetMember",
                    ErrorCategory.CloseError,
