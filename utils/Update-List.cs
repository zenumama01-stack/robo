    /// This cmdlet updates the property of incoming objects and passes them to the
    /// pipeline. This cmdlet also returns a .NET object with properties that
    /// defines the update action on a list.
    /// This cmdlet is most helpful when the cmdlet author wants the user to do
    /// update action on object list that are not directly exposed through
    /// cmdlet parameter. One wants to update a property value which is a list
    /// (multi-valued parameter for a cmdlet), without exposing the list.
    [Cmdlet(VerbsData.Update, "List", DefaultParameterSetName = "AddRemoveSet",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2109383", RemotingCapability = RemotingCapability.None)]
    public class UpdateListCommand : PSCmdlet
        /// The following is the definition of the input parameter "Add".
        /// Objects to add to the list.
        [Parameter(ParameterSetName = "AddRemoveSet")]
        public object[] Add { get; set; }
        /// The following is the definition of the input parameter "Remove".
        /// Objects to be removed from the list.
        public object[] Remove { get; set; }
        /// The following is the definition of the input parameter "Replace".
        /// Objects in this list replace the objects in the target list.
        [Parameter(Mandatory = true, ParameterSetName = "ReplaceSet")]
        public object[] Replace { get; set; }
        /// List of InputObjects where the updates needs to applied to the specific property.
        // [Parameter(ValueFromPipeline = true, ParameterSetName = "AddRemoveSet")]
        // [Parameter(ValueFromPipeline = true, ParameterSetName = "ReplaceSet")]
        /// Defines which property of the input object should be updated with Add and Remove actions.
        // [Parameter(Position = 0, ParameterSetName = "AddRemoveSet")]
        // [Parameter(Position = 0, ParameterSetName = "ReplaceSet")]
        private PSListModifier _listModifier;
                    WriteError(NewError("MissingInputObjectParameter", "MissingInputObjectParameter", null));
                    _listModifier ??= CreatePSListModifier();
                    PSMemberInfo memberInfo = InputObject.Members[Property];
                    if (memberInfo != null)
                            _listModifier.ApplyTo(memberInfo.Value);
                            WriteError(new ErrorRecord(e, "ApplyFailed", ErrorCategory.InvalidOperation, null));
                        WriteError(NewError("MemberDoesntExist", "MemberDoesntExist", InputObject, Property));
                    ThrowTerminatingError(NewError("MissingPropertyParameter", "MissingPropertyParameter", null));
                    WriteObject(CreateHashtable());
        private Hashtable CreateHashtable()
            Hashtable hash = new(2);
            if (Add != null)
                hash.Add("Add", Add);
            if (Remove != null)
                hash.Add("Remove", Remove);
            if (Replace != null)
                hash.Add("Replace", Replace);
        private PSListModifier CreatePSListModifier()
            PSListModifier listModifier = new();
                foreach (object obj in Add)
                    listModifier.Add.Add(obj);
                foreach (object obj in Remove)
                    listModifier.Remove.Add(obj);
                foreach (object obj in Replace)
                    listModifier.Replace.Add(obj);
            return listModifier;
            ErrorDetails details = new(this.GetType().Assembly, "UpdateListStrings", resourceId, args);
