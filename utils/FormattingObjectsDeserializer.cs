    /// Class to deserialize property bags into formatting objects
    /// by using ERS functionality.
    internal sealed class FormatObjectDeserializer
        internal TerminatingErrorContext TerminatingErrorContext { get; }
        /// Expansion of TAB character to the following string.
        private const string TabExpansionString = "    ";
        internal FormatObjectDeserializer(TerminatingErrorContext errorContext)
            TerminatingErrorContext = errorContext;
        internal bool IsFormatInfoData(PSObject so)
            if (PSObject.Base(so) is FormatInfoData fid)
                if (fid is FormatStartData ||
                    fid is FormatEndData ||
                    fid is GroupStartData ||
                    fid is GroupEndData ||
                    fid is FormatEntryData)
                // we have an unexpected type (CLSID not matching): this should never happen
                ProcessUnknownInvalidClassId(fid.ClassId2e4f51ef21dd47e99d3c952918aff9cd, so, "FormatObjectDeserializerDeserializeInvalidClassId");
            // check the type of the object by
            // 1) verifying the type name information
            // 2) trying to access the property containing CLSID information
            if (!Deserializer.IsInstanceOfType(so, typeof(FormatInfoData)))
            if (GetProperty(so, FormatInfoData.classidProperty) is not string classId)
                // it's not one of the objects derived from FormatInfoData
            // it's one of ours, get the right class and deserialize it accordingly
            if (IsClass(classId, FormatStartData.CLSID) ||
                IsClass(classId, FormatEndData.CLSID) ||
                IsClass(classId, GroupStartData.CLSID) ||
                IsClass(classId, GroupEndData.CLSID) ||
                IsClass(classId, FormatEntryData.CLSID))
            // we have an unknown type (CLSID not matching): this should never happen
            ProcessUnknownInvalidClassId(classId, so, "FormatObjectDeserializerIsFormatInfoDataInvalidClassId");
        /// Given a raw object out of the pipeline, it deserializes it accordingly to
        /// its type.
        /// If the object is not one of the well known ones (i.e. derived from FormatInfoData)
        /// it just returns the object unchanged.
        /// <param name="so">Object to deserialize.</param>
        /// <returns>Deserialized object or null.</returns>
        internal object Deserialize(PSObject so)
                    return fid;
                return so;
                // it's not one of the objects derived from FormatInfoData,
                // just return it as is
                return DeserializeObject(so);
            ProcessUnknownInvalidClassId(classId, so, "FormatObjectDeserializerDeserializeInvalidClassId");
        private void ProcessUnknownInvalidClassId(string classId, object obj, string errorId)
            string msg = StringUtil.Format(FormatAndOut_format_xxx.FOD_ClassIdInvalid, classId);
                                            PSTraceSource.NewArgumentException(nameof(classId)),
                                            obj);
        private static bool IsClass(string x, string y)
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
#if _UNUSED
        NOTE: this code is commented out because the current schema does not have the need for
        it. We retail it because future schema extensions might require it
        /// ERS helper to reconstitute a string[] out of IEnumerable property.
        /// <param name="rawObject">Object to process.</param>
        /// <param name="propertyName">Property to look up.</param>
        /// <returns>String[] representation of the property.</returns>
        private static string[] ReadStringArrayHelper (object rawObject, string propertyName)
            // throw if the property is not there
            IEnumerable e = (IEnumerable)ERSHelper.GetExtendedProperty (rawObject, propertyName, false);
            // copy to string collection, since, a priori, we do not know the length
            StringCollection temp = new StringCollection ();
            foreach (string s in e)
                temp.Add (s);
            if (temp.Count <= 0)
            // copy to a string[] and return
            string[] retVal = new string[temp.Count];
            temp.CopyTo (retVal, 0);
        internal static object GetProperty(PSObject so, string name)
            PSMemberInfo member = so.Properties[name];
            // NOTE: we do not distinguish between property not there and null property
            // if an exception is thrown, it would be considered an internal failure
            return member.Value;
        // returns null on error
        internal FormatInfoData DeserializeMemberObject(PSObject so, string property)
            object memberRaw = GetProperty(so, property);
            if (memberRaw == null)
            if (so == memberRaw)
                string msg = StringUtil.Format(FormatAndOut_format_xxx.FOD_RecursiveProperty, property);
                                PSTraceSource.NewArgumentException(nameof(property)),
                                "FormatObjectDeserializerRecursiveProperty",
                                so);
            return DeserializeObject(PSObject.AsPSObject(memberRaw));
        internal FormatInfoData DeserializeMandatoryMemberObject(PSObject so, string property)
            FormatInfoData fid = DeserializeMemberObject(so, property);
            VerifyDataNotNull(fid, property);
        private object DeserializeMemberVariable(PSObject so, string property, System.Type t, bool cannotBeNull)
            object objRaw = GetProperty(so, property);
            if (cannotBeNull)
                VerifyDataNotNull(objRaw, property);
            if (objRaw != null && t != objRaw.GetType())
                string msg = StringUtil.Format(FormatAndOut_format_xxx.FOD_InvalidPropertyType, t.Name, property);
                                "FormatObjectDeserializerInvalidPropertyType",
            return objRaw;
        /// Deserialization of string without TAB expansion (RAW)
        /// <param name="so">Object whose the property belongs to.</param>
        /// <param name="property">Name of the string property.</param>
        /// <returns>String out of the MsObject.</returns>
        internal string DeserializeStringMemberVariableRaw(PSObject so, string property)
            return (string)DeserializeMemberVariable(so, property, typeof(string), false /* cannotBeNull */);
        /// Deserialization of string performing TAB expansion.
        internal string DeserializeStringMemberVariable(PSObject so, string property)
            string val = (string)DeserializeMemberVariable(so, property, typeof(string), false /* cannotBeNull */);
            // expand TAB's
            return val.Replace("\t", TabExpansionString);
        internal int DeserializeIntMemberVariable(PSObject so, string property)
            return (int)DeserializeMemberVariable(so, property, typeof(int), true /* cannotBeNull */);
        internal bool DeserializeBoolMemberVariable(PSObject so, string property, bool cannotBeNull = true)
            var val = DeserializeMemberVariable(so, property, typeof(bool), cannotBeNull);
            return val != null && (bool)val;
        internal WriteStreamType DeserializeWriteStreamTypeMemberVariable(PSObject so)
            object wsTypeValue = GetProperty(so, "writeStream");
            if (wsTypeValue == null)
                return WriteStreamType.None;
            WriteStreamType rtnWSType;
            if (wsTypeValue is WriteStreamType)
                rtnWSType = (WriteStreamType)wsTypeValue;
            else if (wsTypeValue is string)
                if (!Enum.TryParse<WriteStreamType>(wsTypeValue as string, true, out rtnWSType))
                    rtnWSType = WriteStreamType.None;
            return rtnWSType;
        internal FormatInfoData DeserializeObject(PSObject so)
            FormatInfoData fid = FormatInfoDataClassFactory.CreateInstance(so, this);
            fid?.Deserialize(so, this);
        internal void VerifyDataNotNull(object obj, string name)
            string msg = StringUtil.Format(FormatAndOut_format_xxx.FOD_NullDataMember, name);
                                            "FormatObjectDeserializerNullDataMember",
    internal static class FormatInfoDataClassFactory
        static FormatInfoDataClassFactory()
            s_constructors = new Dictionary<string, Func<FormatInfoData>>
                {FormatStartData.CLSID,       static () => new FormatStartData()},
                {FormatEndData.CLSID,         static () => new FormatEndData()},
                {GroupStartData.CLSID,        static () => new GroupStartData()},
                {GroupEndData.CLSID,          static () => new GroupEndData()},
                {FormatEntryData.CLSID,       static () => new FormatEntryData()},
                {WideViewHeaderInfo.CLSID,    static () => new WideViewHeaderInfo()},
                {TableHeaderInfo.CLSID,       static () => new TableHeaderInfo()},
                {TableColumnInfo.CLSID,       static () => new TableColumnInfo()},
                {ListViewHeaderInfo.CLSID,    static () => new ListViewHeaderInfo()},
                {ListViewEntry.CLSID,         static () => new ListViewEntry()},
                {ListViewField.CLSID,         static () => new ListViewField()},
                {TableRowEntry.CLSID,         static () => new TableRowEntry()},
                {WideViewEntry.CLSID,         static () => new WideViewEntry()},
                {ComplexViewHeaderInfo.CLSID, static () => new ComplexViewHeaderInfo()},
                {ComplexViewEntry.CLSID,      static () => new ComplexViewEntry()},
                {GroupingEntry.CLSID,         static () => new GroupingEntry()},
                {PageHeaderEntry.CLSID,       static () => new PageHeaderEntry()},
                {PageFooterEntry.CLSID,       static () => new PageFooterEntry()},
                {AutosizeInfo.CLSID,          static () => new AutosizeInfo()},
                {FormatNewLine.CLSID,         static () => new FormatNewLine()},
                {FrameInfo.CLSID,             static () => new FrameInfo()},
                {FormatTextField.CLSID,       static () => new FormatTextField()},
                {FormatPropertyField.CLSID,   static () => new FormatPropertyField()},
                {FormatEntry.CLSID,           static () => new FormatEntry()},
                {RawTextFormatEntry.CLSID,    static () => new RawTextFormatEntry()}
        internal static FormatInfoData CreateInstance(PSObject so, FormatObjectDeserializer deserializer)
            // look for the property that defines the type of object
            string classId = FormatObjectDeserializer.GetProperty(so, FormatInfoData.classidProperty) as string;
            if (classId == null)
                string msg = StringUtil.Format(FormatAndOut_format_xxx.FOD_InvalidClassidProperty);
                                                PSTraceSource.NewArgumentException("classid"),
                                                "FormatObjectDeserializerInvalidClassidProperty",
                deserializer.TerminatingErrorContext.ThrowTerminatingError(errorRecord);
            FormatInfoData fid = CreateInstance(classId, deserializer);
        // returns null on failure
        private static FormatInfoData CreateInstance(string clsid, FormatObjectDeserializer deserializer)
            Func<FormatInfoData> ctor;
            if (!s_constructors.TryGetValue(clsid, out ctor))
                CreateInstanceError(PSTraceSource.NewArgumentException(nameof(clsid)), clsid, deserializer);
                FormatInfoData fid = ctor();
                CreateInstanceError(e, clsid, deserializer);
            catch (MemberAccessException e) // also MethodAccessException and MissingMethodException
            catch (System.Runtime.InteropServices.InvalidComObjectException e)
            catch (TypeLoadException e)
            catch (Exception e) // will rethrow
                    "Unexpected Activator.CreateInstance error in FormatInfoDataClassFactory.CreateInstance: "
                        + e.GetType().FullName);
        private static void CreateInstanceError(Exception e, string clsid, FormatObjectDeserializer deserializer)
            string msg = StringUtil.Format(FormatAndOut_format_xxx.FOD_InvalidClassid, clsid);
                                            "FormatObjectDeserializerInvalidClassid",
        private static readonly Dictionary<string, Func<FormatInfoData>> s_constructors;
    internal static class FormatInfoDataListDeserializer<T> where T : FormatInfoData
        private static void ReadListHelper(IEnumerable en, List<T> lst, FormatObjectDeserializer deserializer)
            deserializer.VerifyDataNotNull(en, "enumerable");
            foreach (object obj in en)
                FormatInfoData fid = deserializer.DeserializeObject(PSObjectHelper.AsPSObject(obj));
                T entry = fid as T;
                deserializer.VerifyDataNotNull(entry, "entry");
                lst.Add(entry);
        internal static void ReadList(PSObject so, string property, List<T> lst, FormatObjectDeserializer deserializer)
            if (lst == null)
                throw PSTraceSource.NewArgumentNullException(nameof(lst));
            object memberRaw = FormatObjectDeserializer.GetProperty(so, property);
            ReadListHelper(PSObjectHelper.GetEnumerable(memberRaw), lst, deserializer);
    #region Formatting Objects Deserializer
        internal virtual void Deserialize(PSObject so, FormatObjectDeserializer deserializer) { }
        internal override void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
            base.Deserialize(so, deserializer);
            // optional
            this.groupingEntry = (GroupingEntry)deserializer.DeserializeMemberObject(so, "groupingEntry");
            this.shapeInfo = (ShapeInfo)deserializer.DeserializeMemberObject(so, "shapeInfo");
            this.objectCount = deserializer.DeserializeIntMemberVariable(so, "objectCount");
            // for the base class the shapeInfo is optional, but it's mandatory for this class
            deserializer.VerifyDataNotNull(this.shapeInfo, "shapeInfo");
            this.pageHeaderEntry = (PageHeaderEntry)deserializer.DeserializeMemberObject(so, "pageHeaderEntry");
            this.pageFooterEntry = (PageFooterEntry)deserializer.DeserializeMemberObject(so, "pageFooterEntry");
            this.autosizeInfo = (AutosizeInfo)deserializer.DeserializeMemberObject(so, "autosizeInfo");
            this.formatEntryInfo = (FormatEntryInfo)deserializer.DeserializeMandatoryMemberObject(so, "formatEntryInfo");
            this.outOfBand = deserializer.DeserializeBoolMemberVariable(so, "outOfBand");
            this.writeStream = deserializer.DeserializeWriteStreamTypeMemberVariable(so);
            this.isHelpObject = so.IsHelpObject;
            this.columns = deserializer.DeserializeIntMemberVariable(so, "columns");
            // The "repeatHeader" property was added later (V5, V6) and presents an incompatibility when remoting to older version PowerShell sessions.
            // When the property is missing from the serialized object, let the deserialized property be false.
            this.repeatHeader = deserializer.DeserializeBoolMemberVariable(so, "repeatHeader", cannotBeNull: false);
            this.hideHeader = deserializer.DeserializeBoolMemberVariable(so, "hideHeader");
            FormatInfoDataListDeserializer<TableColumnInfo>.ReadList(so, "tableColumnInfoList", this.tableColumnInfoList, deserializer);
            this.width = deserializer.DeserializeIntMemberVariable(so, "width");
            this.alignment = deserializer.DeserializeIntMemberVariable(so, "alignment");
            this.label = deserializer.DeserializeStringMemberVariable(so, "label");
            this.propertyName = deserializer.DeserializeStringMemberVariable(so, "propertyName");
            this.text = deserializer.DeserializeStringMemberVariableRaw(so, "text");
            FormatInfoDataListDeserializer<FormatValue>.ReadList(so, "formatValueList", this.formatValueList, deserializer);
            FormatInfoDataListDeserializer<ListViewField>.ReadList(so, "listViewFieldList", this.listViewFieldList, deserializer);
            this.formatPropertyField = (FormatPropertyField)deserializer.DeserializeMandatoryMemberObject(so, "formatPropertyField");
            FormatInfoDataListDeserializer<FormatPropertyField>.ReadList(so, "formatPropertyFieldList", this.formatPropertyFieldList, deserializer);
            this.multiLine = deserializer.DeserializeBoolMemberVariable(so, "multiLine");
            this.text = deserializer.DeserializeStringMemberVariable(so, "text");
            this.propertyValue = deserializer.DeserializeStringMemberVariable(so, "propertyValue");
            this.frameInfo = (FrameInfo)deserializer.DeserializeMemberObject(so, "frameInfo");
            this.leftIndentation = deserializer.DeserializeIntMemberVariable(so, "leftIndentation");
            this.rightIndentation = deserializer.DeserializeIntMemberVariable(so, "rightIndentation");
            this.firstLine = deserializer.DeserializeIntMemberVariable(so, "firstLine");
