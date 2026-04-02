using System.Management.Automation.Host;
    /// This class implements get-member command.
    [Cmdlet(VerbsCommon.Add, "Member", DefaultParameterSetName = "TypeNameSet",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097109", RemotingCapability = RemotingCapability.None)]
    public class AddMemberCommand : PSCmdlet
        private static readonly object s_notSpecified = new();
        private static bool HasBeenSpecified(object obj)
            return !System.Object.ReferenceEquals(obj, s_notSpecified);
        private PSObject _inputObject;
        /// The object to add a member to.
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "MemberSet")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "TypeNameSet")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = NotePropertySingleMemberSet)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = NotePropertyMultiMemberSet)]
        public PSObject InputObject
        private PSMemberTypes _memberType;
        /// The member type of to be added.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "MemberSet")]
        public PSMemberTypes MemberType
            get { return _memberType; }
            set { _memberType = value; }
        private string _memberName;
        /// The name of the new member.
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "MemberSet")]
            get { return _memberName; }
            set { _memberName = value; }
        private object _value1 = s_notSpecified;
        /// First value of the new member. The meaning of this value changes according to the member type.
        [Parameter(Position = 2, ParameterSetName = "MemberSet")]
            get { return _value1; }
            set { _value1 = value; }
        private object _value2 = s_notSpecified;
        /// Second value of the new member. The meaning of this value changes according to the member type.
        [Parameter(Position = 3, ParameterSetName = "MemberSet")]
        public object SecondValue
            get { return _value2; }
            set { _value2 = value; }
        private string _typeName;
        /// Add new type name to the specified object for TypeNameSet.
        [Parameter(Mandatory = true, ParameterSetName = "TypeNameSet")]
        [Parameter(ParameterSetName = "MemberSet")]
        [Parameter(ParameterSetName = NotePropertySingleMemberSet)]
        [Parameter(ParameterSetName = NotePropertyMultiMemberSet)]
        public string TypeName
            get { return _typeName; }
            set { _typeName = value; }
        /// True if we should overwrite a possibly existing member.
        private bool _passThru /* = false */;
        /// Gets or sets the parameter -passThru which states output from the command should be placed in the pipeline.
        [Parameter(ParameterSetName = "TypeNameSet")]
        #region Simplifying NoteProperty Declaration
        private const string NotePropertySingleMemberSet = "NotePropertySingleMemberSet";
        private const string NotePropertyMultiMemberSet = "NotePropertyMultiMemberSet";
        private string _notePropertyName;
        /// The name of the new NoteProperty member.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = NotePropertySingleMemberSet)]
        [ValidateNotePropertyName]
        [NotePropertyTransformation]
        public string NotePropertyName
            get { return _notePropertyName; }
            set { _notePropertyName = value; }
        private object _notePropertyValue;
        /// The value of the new NoteProperty member.
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = NotePropertySingleMemberSet)]
        public object NotePropertyValue
            get { return _notePropertyValue; }
            set { _notePropertyValue = value; }
        // Use IDictionary to support both Hashtable and OrderedHashtable
        private IDictionary _property;
        /// The NoteProperty members to be set.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = NotePropertyMultiMemberSet)]
        public IDictionary NotePropertyMembers
        #endregion Simplifying NoteProperty Declaration
        private static object GetParameterType(object sourceValue, Type destinationType)
            return LanguagePrimitives.ConvertTo(sourceValue, destinationType, CultureInfo.InvariantCulture);
        private void EnsureValue1AndValue2AreNotBothNull()
            if (_value1 == null &&
               (_value2 == null || !HasBeenSpecified(_value2)))
                ThrowTerminatingError(NewError("Value1AndValue2AreNotBothNull", "Value1AndValue2AreNotBothNull", null, _memberType));
        private void EnsureValue1IsNotNull()
            if (_value1 == null)
                ThrowTerminatingError(NewError("Value1ShouldNotBeNull", "Value1ShouldNotBeNull", null, _memberType));
        private void EnsureValue2IsNotNull()
            if (_value2 == null)
                ThrowTerminatingError(NewError("Value2ShouldNotBeNull", "Value2ShouldNotBeNull", null, _memberType));
        private void EnsureValue1HasBeenSpecified()
            if (!HasBeenSpecified(_value1))
                Collection<FieldDescription> fdc = new();
                fdc.Add(new FieldDescription("Value"));
                string prompt = StringUtil.Format(AddMember.Value1Prompt, _memberType);
                Dictionary<string, PSObject> result = this.Host.UI.Prompt(prompt, null, fdc);
                    _value1 = result["Value"].BaseObject;
        private void EnsureValue2HasNotBeenSpecified()
            if (HasBeenSpecified(_value2))
                ThrowTerminatingError(NewError("Value2ShouldNotBeSpecified", "Value2ShouldNotBeSpecified", null, _memberType));
        private PSMemberInfo GetAliasProperty()
            EnsureValue1HasBeenSpecified();
            EnsureValue1IsNotNull();
            string value1Str = (string)GetParameterType(_value1, typeof(string));
                EnsureValue2IsNotNull();
                Type value2Type = (Type)GetParameterType(_value2, typeof(Type));
                return new PSAliasProperty(_memberName, value1Str, value2Type);
            return new PSAliasProperty(_memberName, value1Str);
        private PSMemberInfo GetCodeMethod()
            EnsureValue2HasNotBeenSpecified();
            MethodInfo value1MethodInfo = (MethodInfo)GetParameterType(_value1, typeof(MethodInfo));
            return new PSCodeMethod(_memberName, value1MethodInfo);
        private PSMemberInfo GetCodeProperty()
            EnsureValue1AndValue2AreNotBothNull();
            MethodInfo value1MethodInfo = null;
            if (HasBeenSpecified(_value1))
                value1MethodInfo = (MethodInfo)GetParameterType(_value1, typeof(MethodInfo));
            MethodInfo value2MethodInfo = null;
                value2MethodInfo = (MethodInfo)GetParameterType(_value2, typeof(MethodInfo));
            return new PSCodeProperty(_memberName, value1MethodInfo, value2MethodInfo);
        private PSMemberInfo GetMemberSet()
            if (_value1 == null || !HasBeenSpecified(_value1))
                return new PSMemberSet(_memberName);
            Collection<PSMemberInfo> value1Collection =
                (Collection<PSMemberInfo>)GetParameterType(_value1, typeof(Collection<PSMemberInfo>));
            return new PSMemberSet(_memberName, value1Collection);
        private PSMemberInfo GetNoteProperty()
            return new PSNoteProperty(_memberName, _value1);
        private PSMemberInfo GetPropertySet()
            Collection<string> value1Collection =
                (Collection<string>)GetParameterType(_value1, typeof(Collection<string>));
            return new PSPropertySet(_memberName, value1Collection);
        private PSMemberInfo GetScriptMethod()
            ScriptBlock value1ScriptBlock = (ScriptBlock)GetParameterType(_value1, typeof(ScriptBlock));
            return new PSScriptMethod(_memberName, value1ScriptBlock);
        private PSMemberInfo GetScriptProperty()
            ScriptBlock value1ScriptBlock = null;
                value1ScriptBlock = (ScriptBlock)GetParameterType(_value1, typeof(ScriptBlock));
            ScriptBlock value2ScriptBlock = null;
                value2ScriptBlock = (ScriptBlock)GetParameterType(_value2, typeof(ScriptBlock));
            return new PSScriptProperty(_memberName, value1ScriptBlock, value2ScriptBlock);
        /// This method implements the ProcessRecord method for add-member command.
            if (_typeName != null && string.IsNullOrWhiteSpace(_typeName))
                ThrowTerminatingError(NewError("TypeNameShouldNotBeEmpty", "TypeNameShouldNotBeEmpty", _typeName));
            if (ParameterSetName == "TypeNameSet")
                UpdateTypeNames();
                if (_passThru)
                    WriteObject(_inputObject);
            if (ParameterSetName == NotePropertyMultiMemberSet)
                ProcessNotePropertyMultiMemberSet();
            PSMemberInfo member = null;
            if (ParameterSetName == NotePropertySingleMemberSet)
                member = new PSNoteProperty(_notePropertyName, _notePropertyValue);
                int memberCountHelper = (int)_memberType;
                int memberCount = 0;
                while (memberCountHelper != 0)
                    if ((memberCountHelper & 1) != 0)
                        memberCount++;
                    memberCountHelper >>= 1;
                if (memberCount != 1)
                    ThrowTerminatingError(NewError("WrongMemberCount", "WrongMemberCount", null, _memberType.ToString()));
                switch (_memberType)
                    case PSMemberTypes.AliasProperty:
                        member = GetAliasProperty();
                    case PSMemberTypes.CodeMethod:
                        member = GetCodeMethod();
                    case PSMemberTypes.CodeProperty:
                        member = GetCodeProperty();
                    case PSMemberTypes.MemberSet:
                        member = GetMemberSet();
                    case PSMemberTypes.NoteProperty:
                        member = GetNoteProperty();
                    case PSMemberTypes.PropertySet:
                        member = GetPropertySet();
                    case PSMemberTypes.ScriptMethod:
                        member = GetScriptMethod();
                    case PSMemberTypes.ScriptProperty:
                        member = GetScriptProperty();
                        ThrowTerminatingError(NewError("CannotAddMemberType", "CannotAddMemberType", null, _memberType.ToString()));
            if (!AddMemberToTarget(member))
            if (_typeName != null)
        /// Add the member to the target object.
        /// <param name="member"></param>
        private bool AddMemberToTarget(PSMemberInfo member)
            PSMemberInfo previousMember = _inputObject.Members[member.Name];
            if (previousMember != null)
                if (!_force)
                    WriteError(NewError("MemberAlreadyExists",
                        "MemberAlreadyExists",
                        _inputObject, member.Name));
                    if (previousMember.IsInstance)
                        _inputObject.Members.Remove(member.Name);
                        WriteError(NewError("CannotRemoveTypeDataMember",
                            "CannotRemoveTypeDataMember",
                            _inputObject, member.Name, previousMember.MemberType));
            _inputObject.Members.Add(member);
        /// Process the 'NotePropertyMultiMemberSet' parameter set.
        private void ProcessNotePropertyMultiMemberSet()
            foreach (DictionaryEntry prop in _property)
                string noteName = PSObject.ToStringParser(this.Context, prop.Key);
                object noteValue = prop.Value;
                if (string.IsNullOrEmpty(noteName))
                    WriteError(NewError("NotePropertyNameShouldNotBeNull",
                        "NotePropertyNameShouldNotBeNull", noteName));
                PSMemberInfo member = new PSNoteProperty(noteName, noteValue);
                if (AddMemberToTarget(member) && !result)
            if (result && _typeName != null)
            if (result && _passThru)
        private void UpdateTypeNames()
            // Respect the type shortcut
            Type type;
            string typeNameInUse = _typeName;
            if (LanguagePrimitives.TryConvertTo(_typeName, out type))
                typeNameInUse = type.FullName;
            _inputObject.TypeNames.Insert(0, typeNameInUse);
        private ErrorRecord NewError(string errorId, string resourceId, object targetObject, params object[] args)
            ErrorDetails details = new(this.GetType().GetTypeInfo().Assembly,
                "Microsoft.PowerShell.Commands.Utility.resources.AddMember", resourceId, args);
                new InvalidOperationException(details.Message),
        /// This ValidateArgumentsAttribute is used to guarantee the argument to be bound to
        /// -NotePropertyName parameter cannot be converted to the enum type PSMemberTypes.
        /// So when given a string or a number that can be converted, we make sure it gets
        /// bound to -MemberType, instead of -NotePropertyName.
        /// This exception will be hidden in the positional binding phase. So we make sure
        /// if the argument can be converted to PSMemberTypes, it gets bound to the -MemberType
        /// parameter. We are sure that when this exception is thrown, the current positional
        /// argument can be successfully bound to.
        private sealed class ValidateNotePropertyNameAttribute : ValidateArgumentsAttribute
            protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
                PSMemberTypes memberType;
                if (arguments is string notePropertyName && LanguagePrimitives.TryConvertTo<PSMemberTypes>(notePropertyName, out memberType))
                    switch (memberType)
                            string errMsg = StringUtil.Format(AddMember.InvalidValueForNotePropertyName, typeof(PSMemberTypes).FullName);
                            throw new ValidationMetadataException(errMsg, true);
        /// Transform the integer arguments to strings for the parameter NotePropertyName.
        internal sealed class NotePropertyTransformationAttribute : ArgumentTransformationAttribute
            public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
                object target = PSObject.Base(inputData);
                if (target != null && target.GetType().IsNumeric())
                    var result = LanguagePrimitives.ConvertTo<string>(target);
                return inputData;
