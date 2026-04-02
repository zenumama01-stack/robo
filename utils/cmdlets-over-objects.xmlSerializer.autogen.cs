#if _DYNAMIC_XMLSERIALIZER_COMPILATION
    [System.CodeDom.Compiler.GeneratedCodeAttribute("sgen", "4.0")]
    internal class XmlSerializationWriter1 : System.Xml.Serialization.XmlSerializationWriter
        public void Write50_PowerShellMetadata(object o)
            WriteStartDocument();
                WriteEmptyTag(@"PowerShellMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            TopLevelElement();
            Write39_PowerShellMetadata(@"PowerShellMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata)o), false, false);
        public void Write51_ClassMetadata(object o)
                WriteNullTagLiteral(@"ClassMetadata", string.Empty);
            Write36_ClassMetadata(@"ClassMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata)o), true, false);
        public void Write52_ClassMetadataInstanceCmdlets(object o)
                WriteNullTagLiteral(@"ClassMetadataInstanceCmdlets", string.Empty);
            Write40_ClassMetadataInstanceCmdlets(@"ClassMetadataInstanceCmdlets", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets)o), true, false);
        public void Write53_GetCmdletParameters(object o)
                WriteNullTagLiteral(@"GetCmdletParameters", string.Empty);
            Write19_GetCmdletParameters(@"GetCmdletParameters", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters)o), true, false);
        public void Write54_PropertyMetadata(object o)
                WriteNullTagLiteral(@"PropertyMetadata", string.Empty);
            Write15_PropertyMetadata(@"PropertyMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata)o), true, false);
        public void Write55_TypeMetadata(object o)
                WriteNullTagLiteral(@"TypeMetadata", string.Empty);
            Write2_TypeMetadata(@"TypeMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata)o), true, false);
        public void Write56_Association(object o)
                WriteNullTagLiteral(@"Association", string.Empty);
            Write17_Association(@"Association", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.Association)o), true, false);
        public void Write57_AssociationAssociatedInstance(object o)
                WriteNullTagLiteral(@"AssociationAssociatedInstance", string.Empty);
            Write41_AssociationAssociatedInstance(@"AssociationAssociatedInstance", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance)o), true, false);
        public void Write58_CmdletParameterMetadata(object o)
                WriteNullTagLiteral(@"CmdletParameterMetadata", string.Empty);
            Write10_CmdletParameterMetadata(@"CmdletParameterMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata)o), true, false);
        public void Write59_Item(object o)
                WriteNullTagLiteral(@"CmdletParameterMetadataForGetCmdletParameter", string.Empty);
            Write11_Item(@"CmdletParameterMetadataForGetCmdletParameter", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter)o), true, false);
        public void Write60_Item(object o)
                WriteNullTagLiteral(@"CmdletParameterMetadataForGetCmdletFilteringParameter", string.Empty);
            Write12_Item(@"CmdletParameterMetadataForGetCmdletFilteringParameter", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter)o), true, false);
        public void Write61_Item(object o)
                WriteNullTagLiteral(@"CmdletParameterMetadataValidateCount", string.Empty);
            Write42_Item(@"CmdletParameterMetadataValidateCount", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount)o), true, false);
        public void Write62_Item(object o)
                WriteNullTagLiteral(@"CmdletParameterMetadataValidateLength", string.Empty);
            Write43_Item(@"CmdletParameterMetadataValidateLength", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength)o), true, false);
        public void Write63_Item(object o)
                WriteNullTagLiteral(@"CmdletParameterMetadataValidateRange", string.Empty);
            Write44_Item(@"CmdletParameterMetadataValidateRange", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange)o), true, false);
        public void Write64_ObsoleteAttributeMetadata(object o)
                WriteNullTagLiteral(@"ObsoleteAttributeMetadata", string.Empty);
            Write7_ObsoleteAttributeMetadata(@"ObsoleteAttributeMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata)o), true, false);
        public void Write65_Item(object o)
                WriteNullTagLiteral(@"CmdletParameterMetadataForInstanceMethodParameter", string.Empty);
            Write9_Item(@"CmdletParameterMetadataForInstanceMethodParameter", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter)o), true, false);
        public void Write66_Item(object o)
                WriteNullTagLiteral(@"CmdletParameterMetadataForStaticMethodParameter", string.Empty);
            Write8_Item(@"CmdletParameterMetadataForStaticMethodParameter", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter)o), true, false);
        public void Write67_QueryOption(object o)
                WriteNullTagLiteral(@"QueryOption", string.Empty);
            Write18_QueryOption(@"QueryOption", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption)o), true, false);
        public void Write68_GetCmdletMetadata(object o)
                WriteNullTagLiteral(@"GetCmdletMetadata", string.Empty);
            Write22_GetCmdletMetadata(@"GetCmdletMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata)o), true, false);
        public void Write69_CommonCmdletMetadata(object o)
                WriteNullTagLiteral(@"CommonCmdletMetadata", string.Empty);
            Write21_CommonCmdletMetadata(@"CommonCmdletMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata)o), true, false);
        public void Write70_ConfirmImpact(object o)
                WriteEmptyTag(@"ConfirmImpact", string.Empty);
            WriteElementString(@"ConfirmImpact", @"", Write20_ConfirmImpact(((global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact)o)));
        public void Write71_StaticCmdletMetadata(object o)
                WriteNullTagLiteral(@"StaticCmdletMetadata", string.Empty);
            Write34_StaticCmdletMetadata(@"StaticCmdletMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata)o), true, false);
        public void Write72_Item(object o)
                WriteNullTagLiteral(@"StaticCmdletMetadataCmdletMetadata", string.Empty);
            Write45_Item(@"StaticCmdletMetadataCmdletMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata)o), true, false);
        public void Write73_CommonMethodMetadata(object o)
                WriteNullTagLiteral(@"CommonMethodMetadata", string.Empty);
            Write29_CommonMethodMetadata(@"CommonMethodMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata)o), true, false);
        public void Write74_StaticMethodMetadata(object o)
                WriteNullTagLiteral(@"StaticMethodMetadata", string.Empty);
            Write28_StaticMethodMetadata(@"StaticMethodMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata)o), true, false);
        public void Write75_CommonMethodParameterMetadata(object o)
                WriteNullTagLiteral(@"CommonMethodParameterMetadata", string.Empty);
            Write26_CommonMethodParameterMetadata(@"CommonMethodParameterMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata)o), true, false);
        public void Write76_StaticMethodParameterMetadata(object o)
                WriteNullTagLiteral(@"StaticMethodParameterMetadata", string.Empty);
            Write27_StaticMethodParameterMetadata(@"StaticMethodParameterMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata)o), true, false);
        public void Write77_CmdletOutputMetadata(object o)
                WriteNullTagLiteral(@"CmdletOutputMetadata", string.Empty);
            Write23_CmdletOutputMetadata(@"CmdletOutputMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata)o), true, false);
        public void Write78_Item(object o)
                WriteNullTagLiteral(@"InstanceMethodParameterMetadata", string.Empty);
            Write25_Item(@"InstanceMethodParameterMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata)o), true, false);
        public void Write79_Item(object o)
                WriteNullTagLiteral(@"CommonMethodMetadataReturnValue", string.Empty);
            Write46_Item(@"CommonMethodMetadataReturnValue", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue)o), true, false);
        public void Write80_InstanceMethodMetadata(object o)
                WriteNullTagLiteral(@"InstanceMethodMetadata", string.Empty);
            Write30_InstanceMethodMetadata(@"InstanceMethodMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata)o), true, false);
        public void Write81_InstanceCmdletMetadata(object o)
                WriteNullTagLiteral(@"InstanceCmdletMetadata", string.Empty);
            Write31_InstanceCmdletMetadata(@"InstanceCmdletMetadata", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata)o), true, false);
        public void Write82_PropertyQuery(object o)
                WriteNullTagLiteral(@"PropertyQuery", string.Empty);
            Write14_PropertyQuery(@"PropertyQuery", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery)o), true, false);
        public void Write83_WildcardablePropertyQuery(object o)
                WriteNullTagLiteral(@"WildcardablePropertyQuery", string.Empty);
            Write13_WildcardablePropertyQuery(@"WildcardablePropertyQuery", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery)o), true, false);
        public void Write84_ItemsChoiceType(object o)
                WriteEmptyTag(@"ItemsChoiceType", string.Empty);
            WriteElementString(@"ItemsChoiceType", @"", Write3_ItemsChoiceType(((global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType)o)));
        public void Write85_ClassMetadataData(object o)
                WriteNullTagLiteral(@"ClassMetadataData", string.Empty);
            Write47_ClassMetadataData(@"ClassMetadataData", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData)o), true, false);
        public void Write86_EnumMetadataEnum(object o)
                WriteNullTagLiteral(@"EnumMetadataEnum", string.Empty);
            Write48_EnumMetadataEnum(@"EnumMetadataEnum", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum)o), true, false);
        public void Write87_EnumMetadataEnumValue(object o)
                WriteNullTagLiteral(@"EnumMetadataEnumValue", string.Empty);
            Write49_EnumMetadataEnumValue(@"EnumMetadataEnumValue", @"", ((global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue)o), true, false);
        private void Write49_EnumMetadataEnumValue(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue o, bool isNullable, bool needType)
            if ((object)o == null)
                if (isNullable)
                    WriteNullTagLiteral(n, ns);
            if (!needType)
                System.Type t = o.GetType();
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue))
                    throw CreateUnknownTypeException(o);
            WriteStartElement(n, ns, o, false, null);
            if (needType)
                WriteXsiType(@"EnumMetadataEnumValue", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"Name", @"", ((global::System.String)o.@Name));
            WriteAttribute(@"Value", @"", ((global::System.String)o.@Value));
            WriteEndElement(o);
        private void Write48_EnumMetadataEnum(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum))
                WriteXsiType(@"EnumMetadataEnum", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"EnumName", @"", ((global::System.String)o.@EnumName));
            WriteAttribute(@"UnderlyingType", @"", ((global::System.String)o.@UnderlyingType));
            if (o.@BitwiseFlagsSpecified)
                WriteAttribute(@"BitwiseFlags", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@BitwiseFlags)));
                global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue[])o.@Value;
                    for (int ia = 0; ia < a.Length; ia++)
                        Write37_EnumMetadataEnumValue(@"Value", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue)a[ia]), false, false);
        private void Write37_EnumMetadataEnumValue(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue o, bool isNullable, bool needType)
                WriteXsiType(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write47_ClassMetadataData(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData))
                WriteXsiType(@"ClassMetadataData", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            if ((object)(o.@Value) != null)
                WriteValue(((global::System.String)o.@Value));
        private string Write3_ItemsChoiceType(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType v)
            string s = null;
            switch (v)
                case global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@ExcludeQuery: s = @"ExcludeQuery"; break;
                case global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MaxValueQuery: s = @"MaxValueQuery"; break;
                case global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MinValueQuery: s = @"MinValueQuery"; break;
                case global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@RegularQuery: s = @"RegularQuery"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType");
        private void Write13_WildcardablePropertyQuery(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery))
                WriteXsiType(@"WildcardablePropertyQuery", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            if (o.@AllowGlobbingSpecified)
                WriteAttribute(@"AllowGlobbing", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@AllowGlobbing)));
            Write12_Item(@"CmdletParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter)o.@CmdletParameterMetadata), false, false);
        private void Write12_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter))
                WriteXsiType(@"CmdletParameterMetadataForGetCmdletFilteringParameter", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            if (o.@IsMandatorySpecified)
                WriteAttribute(@"IsMandatory", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@IsMandatory)));
                global::System.String[] a = (global::System.String[])o.@Aliases;
                    Writer.WriteStartAttribute(null, @"Aliases", string.Empty);
                    for (int i = 0; i < a.Length; i++)
                        global::System.String ai = (global::System.String)a[i];
                            Writer.WriteString(" ");
                        WriteValue(ai);
                    Writer.WriteEndAttribute();
            WriteAttribute(@"PSName", @"", ((global::System.String)o.@PSName));
            WriteAttribute(@"Position", @"", ((global::System.String)o.@Position));
            if (o.@ValueFromPipelineSpecified)
                WriteAttribute(@"ValueFromPipeline", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@ValueFromPipeline)));
            if (o.@ValueFromPipelineByPropertyNameSpecified)
                WriteAttribute(@"ValueFromPipelineByPropertyName", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@ValueFromPipelineByPropertyName)));
                global::System.String[] a = (global::System.String[])o.@CmdletParameterSets;
                    Writer.WriteStartAttribute(null, @"CmdletParameterSets", string.Empty);
            if (o.@ErrorOnNoMatchSpecified)
                WriteAttribute(@"ErrorOnNoMatch", @"", System.Xml.XmlConvert.ToString((global::System.Boolean)((global::System.Boolean)o.@ErrorOnNoMatch)));
            Write1_Object(@"AllowEmptyCollection", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::System.Object)o.@AllowEmptyCollection), false, false);
            Write1_Object(@"AllowEmptyString", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::System.Object)o.@AllowEmptyString), false, false);
            Write1_Object(@"AllowNull", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::System.Object)o.@AllowNull), false, false);
            Write1_Object(@"ValidateNotNull", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::System.Object)o.@ValidateNotNull), false, false);
            Write1_Object(@"ValidateNotNullOrEmpty", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::System.Object)o.@ValidateNotNullOrEmpty), false, false);
            Write4_Item(@"ValidateCount", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount)o.@ValidateCount), false, false);
            Write5_Item(@"ValidateLength", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength)o.@ValidateLength), false, false);
            Write6_Item(@"ValidateRange", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange)o.@ValidateRange), false, false);
                global::System.String[] a = (global::System.String[])((global::System.String[])o.@ValidateSet);
                    WriteStartElement(@"ValidateSet", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                        WriteElementString(@"AllowedValue", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::System.String)a[ia]));
                    WriteEndElement();
            Write7_ObsoleteAttributeMetadata(@"Obsolete", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata)o.@Obsolete), false, false);
        private void Write7_ObsoleteAttributeMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata))
                WriteXsiType(@"ObsoleteAttributeMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"Message", @"", ((global::System.String)o.@Message));
        private void Write6_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange))
            WriteAttribute(@"Min", @"", ((global::System.String)o.@Min));
            WriteAttribute(@"Max", @"", ((global::System.String)o.@Max));
        private void Write5_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength))
        private void Write4_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount))
        private void Write1_Object(string n, string ns, global::System.Object o, bool isNullable, bool needType)
                if (isNullable) WriteNullTagLiteral(n, ns);
                if (t == typeof(global::System.Object))
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue))
                    Write49_EnumMetadataEnumValue(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum))
                    Write48_EnumMetadataEnum(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData))
                    Write47_ClassMetadataData(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue))
                    Write46_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange))
                    Write44_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength))
                    Write43_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount))
                    Write42_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance))
                    Write41_AssociationAssociatedInstance(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets))
                    Write40_ClassMetadataInstanceCmdlets(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata))
                    Write36_ClassMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata))
                    Write34_StaticCmdletMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata))
                    Write31_InstanceCmdletMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata))
                    Write26_CommonMethodParameterMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata))
                    Write27_StaticMethodParameterMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata))
                    Write25_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata))
                    Write29_CommonMethodMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata))
                    Write30_InstanceMethodMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata))
                    Write28_StaticMethodMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata))
                    Write23_CmdletOutputMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata))
                    Write22_GetCmdletMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata))
                    Write21_CommonCmdletMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata))
                    Write45_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters))
                    Write19_GetCmdletParameters(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption))
                    Write18_QueryOption(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.Association))
                    Write17_Association(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.Association)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata))
                    Write15_PropertyMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery))
                    Write14_PropertyQuery(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery))
                    Write13_WildcardablePropertyQuery(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata))
                    Write10_CmdletParameterMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter))
                    Write11_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter))
                    Write12_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter))
                    Write9_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter))
                    Write8_Item(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata))
                    Write7_ObsoleteAttributeMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata))
                    Write2_TypeMetadata(n, ns, (global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata)o, isNullable, true);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType))
                    Writer.WriteStartElement(n, ns);
                    WriteXsiType(@"ItemsChoiceType", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                    Writer.WriteString(Write3_ItemsChoiceType((global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType)o));
                    Writer.WriteEndElement();
                else if (t == typeof(global::System.String[]))
                    WriteXsiType(@"ArrayOfString", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        global::System.String[] a = (global::System.String[])o;
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[]))
                    WriteXsiType(@"ArrayOfPropertyMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[])o;
                                Write15_PropertyMetadata(@"Property", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata)a[ia]), false, false);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.Association[]))
                    WriteXsiType(@"ArrayOfAssociation", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        global::Microsoft.PowerShell.Cmdletization.Xml.Association[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.Association[])o;
                                Write17_Association(@"Association", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.Association)a[ia]), false, false);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[]))
                    WriteXsiType(@"ArrayOfQueryOption", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[])o;
                                Write18_QueryOption(@"Option", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption)a[ia]), false, false);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact))
                    WriteXsiType(@"ConfirmImpact", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                    Writer.WriteString(Write20_ConfirmImpact((global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact)o));
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[]))
                    WriteXsiType(@"ArrayOfStaticMethodParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[])o;
                                Write27_StaticMethodParameterMetadata(@"Parameter", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata)a[ia]), false, false);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[]))
                    WriteXsiType(@"ArrayOfInstanceMethodParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[])o;
                                Write25_Item(@"Parameter", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata)a[ia]), false, false);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[]))
                    WriteXsiType(@"ArrayOfStaticCmdletMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[])o;
                                Write34_StaticCmdletMetadata(@"Cmdlet", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata)a[ia]), false, false);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[]))
                    WriteXsiType(@"ArrayOfClassMetadataData", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[])o;
                                Write35_ClassMetadataData(@"Data", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData)a[ia]), false, false);
                else if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[]))
                    WriteXsiType(@"ArrayOfEnumMetadataEnum", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[])o;
                                Write38_EnumMetadataEnum(@"Enum", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum)a[ia]), false, false);
                    WriteTypedPrimitive(n, ns, o, true);
        private void Write38_EnumMetadataEnum(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum o, bool isNullable, bool needType)
            if (needType) WriteXsiType(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write35_ClassMetadataData(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData o, bool isNullable, bool needType)
        private void Write34_StaticCmdletMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata))
            if (needType) WriteXsiType(@"StaticCmdletMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            Write33_Item(@"CmdletMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata)o.@CmdletMetadata), false, false);
                global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata[])o.@Method;
                        Write28_StaticMethodMetadata(@"Method", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata)a[ia]), false, false);
        private void Write28_StaticMethodMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata))
            if (needType) WriteXsiType(@"StaticMethodMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"MethodName", @"", ((global::System.String)o.@MethodName));
            WriteAttribute(@"CmdletParameterSet", @"", ((global::System.String)o.@CmdletParameterSet));
            Write24_Item(@"ReturnValue", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue)o.@ReturnValue), false, false);
                global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[])((global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[])o.@Parameters);
                    WriteStartElement(@"Parameters", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
        private void Write27_StaticMethodParameterMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata))
            if (needType) WriteXsiType(@"StaticMethodParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"ParameterName", @"", ((global::System.String)o.@ParameterName));
            WriteAttribute(@"DefaultValue", @"", ((global::System.String)o.@DefaultValue));
            Write2_TypeMetadata(@"Type", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata)o.@Type), false, false);
            Write8_Item(@"CmdletParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter)o.@CmdletParameterMetadata), false, false);
            Write23_CmdletOutputMetadata(@"CmdletOutputMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata)o.@CmdletOutputMetadata), false, false);
        private void Write23_CmdletOutputMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata))
            if (needType) WriteXsiType(@"CmdletOutputMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            Write1_Object(@"ErrorCode", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::System.Object)o.@ErrorCode), false, false);
        private void Write8_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter))
            if (needType) WriteXsiType(@"CmdletParameterMetadataForStaticMethodParameter", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                        if (i != 0) Writer.WriteString(" ");
        private void Write2_TypeMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata))
            if (needType) WriteXsiType(@"TypeMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"PSType", @"", ((global::System.String)o.@PSType));
            WriteAttribute(@"ETSType", @"", ((global::System.String)o.@ETSType));
        private void Write24_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue))
        private void Write33_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata))
            WriteAttribute(@"Verb", @"", ((global::System.String)o.@Verb));
            WriteAttribute(@"Noun", @"", ((global::System.String)o.@Noun));
            if (o.@ConfirmImpactSpecified)
                WriteAttribute(@"ConfirmImpact", @"", Write20_ConfirmImpact(((global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact)o.@ConfirmImpact)));
            WriteAttribute(@"HelpUri", @"", ((global::System.String)o.@HelpUri));
            WriteAttribute(@"DefaultCmdletParameterSet", @"", ((global::System.String)o.@DefaultCmdletParameterSet));
        private string Write20_ConfirmImpact(global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact v)
                case global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact.@None: s = @"None"; break;
                case global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact.@Low: s = @"Low"; break;
                case global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact.@Medium: s = @"Medium"; break;
                case global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact.@High: s = @"High"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact");
        private void Write25_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata))
            if (needType) WriteXsiType(@"InstanceMethodParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            Write9_Item(@"CmdletParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter)o.@CmdletParameterMetadata), false, false);
        private void Write9_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter))
            if (needType) WriteXsiType(@"CmdletParameterMetadataForInstanceMethodParameter", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write18_QueryOption(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption))
            if (needType) WriteXsiType(@"QueryOption", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"OptionName", @"", ((global::System.String)o.@OptionName));
            Write11_Item(@"CmdletParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter)o.@CmdletParameterMetadata), false, false);
        private void Write11_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter))
            if (needType) WriteXsiType(@"CmdletParameterMetadataForGetCmdletParameter", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write17_Association(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.Association o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.Association))
            if (needType) WriteXsiType(@"Association", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"Association", @"", ((global::System.String)o.@Association1));
            WriteAttribute(@"SourceRole", @"", ((global::System.String)o.@SourceRole));
            WriteAttribute(@"ResultRole", @"", ((global::System.String)o.@ResultRole));
            Write16_AssociationAssociatedInstance(@"AssociatedInstance", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance)o.@AssociatedInstance), false, false);
        private void Write16_AssociationAssociatedInstance(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance))
        private void Write15_PropertyMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata))
            if (needType) WriteXsiType(@"PropertyMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"PropertyName", @"", ((global::System.String)o.@PropertyName));
                global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery[])o.@Items;
                    global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType[] c = (global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType[])o.@ItemsElementName;
                    if (c == null || c.Length < a.Length)
                        throw CreateInvalidChoiceIdentifierValueException(@"Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType", @"ItemsElementName");
                        global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery ai = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery)a[ia];
                        global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType ci = (global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType)c[ia];
                            if (ci == Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@RegularQuery && ((object)(ai) != null))
                                if (((object)ai) != null && ai is not global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery) throw CreateMismatchChoiceException(@"Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery", @"ItemsElementName", @"Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@RegularQuery");
                                Write13_WildcardablePropertyQuery(@"RegularQuery", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery)ai), false, false);
                            else if (ci == Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@ExcludeQuery && ((object)(ai) != null))
                                if (((object)ai) != null && ai is not global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery) throw CreateMismatchChoiceException(@"Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery", @"ItemsElementName", @"Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@ExcludeQuery");
                                Write13_WildcardablePropertyQuery(@"ExcludeQuery", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery)ai), false, false);
                            else if (ci == Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MaxValueQuery && ((object)(ai) != null))
                                if (((object)ai) != null && ai is not global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery) throw CreateMismatchChoiceException(@"Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery", @"ItemsElementName", @"Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MaxValueQuery");
                                Write14_PropertyQuery(@"MaxValueQuery", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery)ai), false, false);
                            else if (ci == Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MinValueQuery && ((object)(ai) != null))
                                if (((object)ai) != null && ai is not global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery) throw CreateMismatchChoiceException(@"Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery", @"ItemsElementName", @"Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MinValueQuery");
                                Write14_PropertyQuery(@"MinValueQuery", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery)ai), false, false);
                            else if ((object)(ai) != null)
                                throw CreateUnknownTypeException(ai);
        private void Write14_PropertyQuery(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery))
            if (needType) WriteXsiType(@"PropertyQuery", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write10_CmdletParameterMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata))
            if (needType) WriteXsiType(@"CmdletParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write19_GetCmdletParameters(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters))
            if (needType) WriteXsiType(@"GetCmdletParameters", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[])((global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[])o.@QueryableProperties);
                    WriteStartElement(@"QueryableProperties", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                global::Microsoft.PowerShell.Cmdletization.Xml.Association[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.Association[])((global::Microsoft.PowerShell.Cmdletization.Xml.Association[])o.@QueryableAssociations);
                    WriteStartElement(@"QueryableAssociations", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[])((global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[])o.@QueryOptions);
                    WriteStartElement(@"QueryOptions", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
        private void Write45_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata o, bool isNullable, bool needType)
            if (needType) WriteXsiType(@"StaticCmdletMetadataCmdletMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write21_CommonCmdletMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata))
            if (needType) WriteXsiType(@"CommonCmdletMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write22_GetCmdletMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata))
            if (needType) WriteXsiType(@"GetCmdletMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            Write21_CommonCmdletMetadata(@"CmdletMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata)o.@CmdletMetadata), false, false);
            Write19_GetCmdletParameters(@"GetCmdletParameters", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters)o.@GetCmdletParameters), false, false);
        private void Write30_InstanceMethodMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata))
            if (needType) WriteXsiType(@"InstanceMethodMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[])((global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[])o.@Parameters);
        private void Write29_CommonMethodMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata))
            if (needType) WriteXsiType(@"CommonMethodMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write26_CommonMethodParameterMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata))
            if (needType) WriteXsiType(@"CommonMethodParameterMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write31_InstanceCmdletMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata))
            if (needType) WriteXsiType(@"InstanceCmdletMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            Write30_InstanceMethodMetadata(@"Method", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata)o.@Method), false, false);
        private void Write36_ClassMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata))
            if (needType) WriteXsiType(@"ClassMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            WriteAttribute(@"CmdletAdapter", @"", ((global::System.String)o.@CmdletAdapter));
            WriteAttribute(@"ClassName", @"", ((global::System.String)o.@ClassName));
            WriteAttribute(@"ClassVersion", @"", ((global::System.String)o.@ClassVersion));
            WriteElementString(@"Version", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::System.String)o.@Version));
            WriteElementString(@"DefaultNoun", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::System.String)o.@DefaultNoun));
            Write32_ClassMetadataInstanceCmdlets(@"InstanceCmdlets", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets)o.@InstanceCmdlets), false, false);
                global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[])((global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[])o.@StaticCmdlets);
                    WriteStartElement(@"StaticCmdlets", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[])((global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[])o.@CmdletAdapterPrivateData);
                    WriteStartElement(@"CmdletAdapterPrivateData", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
        private void Write32_ClassMetadataInstanceCmdlets(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets))
            Write22_GetCmdletMetadata(@"GetCmdlet", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata)o.@GetCmdlet), false, false);
                global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata[])o.@Cmdlet;
                        Write31_InstanceCmdletMetadata(@"Cmdlet", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata)a[ia]), false, false);
        private void Write40_ClassMetadataInstanceCmdlets(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets o, bool isNullable, bool needType)
            if (needType) WriteXsiType(@"ClassMetadataInstanceCmdlets", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write41_AssociationAssociatedInstance(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance o, bool isNullable, bool needType)
            if (needType) WriteXsiType(@"AssociationAssociatedInstance", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write42_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount o, bool isNullable, bool needType)
            if (needType) WriteXsiType(@"CmdletParameterMetadataValidateCount", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write43_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength o, bool isNullable, bool needType)
            if (needType) WriteXsiType(@"CmdletParameterMetadataValidateLength", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write44_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange o, bool isNullable, bool needType)
            if (needType) WriteXsiType(@"CmdletParameterMetadataValidateRange", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write46_Item(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue o, bool isNullable, bool needType)
            if (needType) WriteXsiType(@"CommonMethodMetadataReturnValue", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        private void Write39_PowerShellMetadata(string n, string ns, global::Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata o, bool isNullable, bool needType)
                if (t == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata))
            Write36_ClassMetadata(@"Class", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", ((global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata)o.@Class), false, false);
                global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[] a = (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[])((global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[])o.@Enums);
                    WriteStartElement(@"Enums", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
        protected override void InitCallbacks()
    internal class XmlSerializationReader1 : System.Xml.Serialization.XmlSerializationReader
        public object Read50_PowerShellMetadata()
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element)
                if (((object)Reader.LocalName == (object)_id1_PowerShellMetadata && (object)Reader.NamespaceURI == (object)_id2_Item))
                    o = Read39_PowerShellMetadata(false, true);
                    throw CreateUnknownNodeException();
                UnknownNode(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:PowerShellMetadata");
            return (object)o;
        public object Read51_ClassMetadata()
                if (((object)Reader.LocalName == (object)_id3_ClassMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read36_ClassMetadata(true, true);
                UnknownNode(null, @":ClassMetadata");
        public object Read52_ClassMetadataInstanceCmdlets()
                if (((object)Reader.LocalName == (object)_id5_ClassMetadataInstanceCmdlets && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read40_ClassMetadataInstanceCmdlets(true, true);
                UnknownNode(null, @":ClassMetadataInstanceCmdlets");
        public object Read53_GetCmdletParameters()
                if (((object)Reader.LocalName == (object)_id6_GetCmdletParameters && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read19_GetCmdletParameters(true, true);
                UnknownNode(null, @":GetCmdletParameters");
        public object Read54_PropertyMetadata()
                if (((object)Reader.LocalName == (object)_id7_PropertyMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read15_PropertyMetadata(true, true);
                UnknownNode(null, @":PropertyMetadata");
        public object Read55_TypeMetadata()
                if (((object)Reader.LocalName == (object)_id8_TypeMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read2_TypeMetadata(true, true);
                UnknownNode(null, @":TypeMetadata");
        public object Read56_Association()
                if (((object)Reader.LocalName == (object)_id9_Association && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read17_Association(true, true);
                UnknownNode(null, @":Association");
        public object Read57_AssociationAssociatedInstance()
                if (((object)Reader.LocalName == (object)_id10_AssociationAssociatedInstance && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read41_AssociationAssociatedInstance(true, true);
                UnknownNode(null, @":AssociationAssociatedInstance");
        public object Read58_CmdletParameterMetadata()
                if (((object)Reader.LocalName == (object)_id11_CmdletParameterMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read10_CmdletParameterMetadata(true, true);
                UnknownNode(null, @":CmdletParameterMetadata");
        public object Read59_Item()
                if (((object)Reader.LocalName == (object)_id12_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read11_Item(true, true);
                UnknownNode(null, @":CmdletParameterMetadataForGetCmdletParameter");
        public object Read60_Item()
                if (((object)Reader.LocalName == (object)_id13_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read12_Item(true, true);
                UnknownNode(null, @":CmdletParameterMetadataForGetCmdletFilteringParameter");
        public object Read61_Item()
                if (((object)Reader.LocalName == (object)_id14_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read42_Item(true, true);
                UnknownNode(null, @":CmdletParameterMetadataValidateCount");
        public object Read62_Item()
                if (((object)Reader.LocalName == (object)_id15_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read43_Item(true, true);
                UnknownNode(null, @":CmdletParameterMetadataValidateLength");
        public object Read63_Item()
                if (((object)Reader.LocalName == (object)_id16_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read44_Item(true, true);
                UnknownNode(null, @":CmdletParameterMetadataValidateRange");
        public object Read64_ObsoleteAttributeMetadata()
                if (((object)Reader.LocalName == (object)_id17_ObsoleteAttributeMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read7_ObsoleteAttributeMetadata(true, true);
                UnknownNode(null, @":ObsoleteAttributeMetadata");
        public object Read65_Item()
                if (((object)Reader.LocalName == (object)_id18_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read9_Item(true, true);
                UnknownNode(null, @":CmdletParameterMetadataForInstanceMethodParameter");
        public object Read66_Item()
                if (((object)Reader.LocalName == (object)_id19_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read8_Item(true, true);
                UnknownNode(null, @":CmdletParameterMetadataForStaticMethodParameter");
        public object Read67_QueryOption()
                if (((object)Reader.LocalName == (object)_id20_QueryOption && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read18_QueryOption(true, true);
                UnknownNode(null, @":QueryOption");
        public object Read68_GetCmdletMetadata()
                if (((object)Reader.LocalName == (object)_id21_GetCmdletMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read22_GetCmdletMetadata(true, true);
                UnknownNode(null, @":GetCmdletMetadata");
        public object Read69_CommonCmdletMetadata()
                if (((object)Reader.LocalName == (object)_id22_CommonCmdletMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read21_CommonCmdletMetadata(true, true);
                UnknownNode(null, @":CommonCmdletMetadata");
        public object Read70_ConfirmImpact()
                if (((object)Reader.LocalName == (object)_id23_ConfirmImpact && (object)Reader.NamespaceURI == (object)_id4_Item))
                        o = Read20_ConfirmImpact(Reader.ReadElementString());
                UnknownNode(null, @":ConfirmImpact");
        public object Read71_StaticCmdletMetadata()
                if (((object)Reader.LocalName == (object)_id24_StaticCmdletMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read34_StaticCmdletMetadata(true, true);
                UnknownNode(null, @":StaticCmdletMetadata");
        public object Read72_Item()
                if (((object)Reader.LocalName == (object)_id25_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read45_Item(true, true);
                UnknownNode(null, @":StaticCmdletMetadataCmdletMetadata");
        public object Read73_CommonMethodMetadata()
                if (((object)Reader.LocalName == (object)_id26_CommonMethodMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read29_CommonMethodMetadata(true, true);
                UnknownNode(null, @":CommonMethodMetadata");
        public object Read74_StaticMethodMetadata()
                if (((object)Reader.LocalName == (object)_id27_StaticMethodMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read28_StaticMethodMetadata(true, true);
                UnknownNode(null, @":StaticMethodMetadata");
        public object Read75_CommonMethodParameterMetadata()
                if (((object)Reader.LocalName == (object)_id28_CommonMethodParameterMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read26_CommonMethodParameterMetadata(true, true);
                UnknownNode(null, @":CommonMethodParameterMetadata");
        public object Read76_StaticMethodParameterMetadata()
                if (((object)Reader.LocalName == (object)_id29_StaticMethodParameterMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read27_StaticMethodParameterMetadata(true, true);
                UnknownNode(null, @":StaticMethodParameterMetadata");
        public object Read77_CmdletOutputMetadata()
                if (((object)Reader.LocalName == (object)_id30_CmdletOutputMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read23_CmdletOutputMetadata(true, true);
                UnknownNode(null, @":CmdletOutputMetadata");
        public object Read78_Item()
                if (((object)Reader.LocalName == (object)_id31_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read25_Item(true, true);
                UnknownNode(null, @":InstanceMethodParameterMetadata");
        public object Read79_Item()
                if (((object)Reader.LocalName == (object)_id32_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read46_Item(true, true);
                UnknownNode(null, @":CommonMethodMetadataReturnValue");
        public object Read80_InstanceMethodMetadata()
                if (((object)Reader.LocalName == (object)_id33_InstanceMethodMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read30_InstanceMethodMetadata(true, true);
                UnknownNode(null, @":InstanceMethodMetadata");
        public object Read81_InstanceCmdletMetadata()
                if (((object)Reader.LocalName == (object)_id34_InstanceCmdletMetadata && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read31_InstanceCmdletMetadata(true, true);
                UnknownNode(null, @":InstanceCmdletMetadata");
        public object Read82_PropertyQuery()
                if (((object)Reader.LocalName == (object)_id35_PropertyQuery && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read14_PropertyQuery(true, true);
                UnknownNode(null, @":PropertyQuery");
        public object Read83_WildcardablePropertyQuery()
                if (((object)Reader.LocalName == (object)_id36_WildcardablePropertyQuery && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read13_WildcardablePropertyQuery(true, true);
                UnknownNode(null, @":WildcardablePropertyQuery");
        public object Read84_ItemsChoiceType()
                if (((object)Reader.LocalName == (object)_id37_ItemsChoiceType && (object)Reader.NamespaceURI == (object)_id4_Item))
                        o = Read3_ItemsChoiceType(Reader.ReadElementString());
                UnknownNode(null, @":ItemsChoiceType");
        public object Read85_ClassMetadataData()
                if (((object)Reader.LocalName == (object)_id38_ClassMetadataData && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read47_ClassMetadataData(true, true);
                UnknownNode(null, @":ClassMetadataData");
        public object Read86_EnumMetadataEnum()
                if (((object)Reader.LocalName == (object)_id39_EnumMetadataEnum && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read48_EnumMetadataEnum(true, true);
                UnknownNode(null, @":EnumMetadataEnum");
        public object Read87_EnumMetadataEnumValue()
                if (((object)Reader.LocalName == (object)_id40_EnumMetadataEnumValue && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o = Read49_EnumMetadataEnumValue(true, true);
                UnknownNode(null, @":EnumMetadataEnumValue");
        private global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue Read49_EnumMetadataEnumValue(bool isNullable, bool checkType)
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id40_EnumMetadataEnumValue && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            if (isNull) return null;
            global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue();
            bool[] paramsRead = new bool[2];
            while (Reader.MoveToNextAttribute())
                if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id41_Name && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@Name = Reader.Value;
                    paramsRead[0] = true;
                else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id42_Value && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@Value = CollapseWhitespace(Reader.Value);
                    paramsRead[1] = true;
                else if (!IsXmlnsAttribute(Reader.Name))
                    UnknownNode((object)o, @":Name, :Value");
            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
                Reader.Skip();
            Reader.ReadStartElement();
            int whileIterations0 = 0;
            int readerCount0 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None)
                    UnknownNode((object)o, string.Empty);
                CheckReaderCount(ref whileIterations0, ref readerCount0);
            ReadEndElement();
        private global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum Read48_EnumMetadataEnum(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id39_EnumMetadataEnum && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum();
            global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue[] a_0 = null;
            int ca_0 = 0;
            bool[] paramsRead = new bool[4];
                if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id43_EnumName && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@EnumName = Reader.Value;
                else if (!paramsRead[2] && ((object)Reader.LocalName == (object)_id44_UnderlyingType && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@UnderlyingType = Reader.Value;
                    paramsRead[2] = true;
                else if (!paramsRead[3] && ((object)Reader.LocalName == (object)_id45_BitwiseFlags && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@BitwiseFlags = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@BitwiseFlagsSpecified = true;
                    paramsRead[3] = true;
                    UnknownNode((object)o, @":EnumName, :UnderlyingType, :BitwiseFlags");
                o.@Value = (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue[])ShrinkArray(a_0, ca_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue), true);
            int whileIterations1 = 0;
            int readerCount1 = ReaderCount;
                    if (((object)Reader.LocalName == (object)_id42_Value && (object)Reader.NamespaceURI == (object)_id2_Item))
                        a_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue[])EnsureArrayIndex(a_0, ca_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue)); a_0[ca_0++] = Read37_EnumMetadataEnumValue(false, true);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Value");
                CheckReaderCount(ref whileIterations1, ref readerCount1);
        private global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue Read37_EnumMetadataEnumValue(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id4_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            int whileIterations2 = 0;
            int readerCount2 = ReaderCount;
                CheckReaderCount(ref whileIterations2, ref readerCount2);
        private global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData Read47_ClassMetadataData(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id38_ClassMetadataData && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData();
                    UnknownNode((object)o, @":Name");
            int whileIterations3 = 0;
            int readerCount3 = ReaderCount;
                string tmp = null;
                else if (Reader.NodeType == System.Xml.XmlNodeType.Text ||
                Reader.NodeType == System.Xml.XmlNodeType.CDATA ||
                Reader.NodeType == System.Xml.XmlNodeType.Whitespace ||
                Reader.NodeType == System.Xml.XmlNodeType.SignificantWhitespace)
                    tmp = ReadString(tmp, false);
                    o.@Value = tmp;
                CheckReaderCount(ref whileIterations3, ref readerCount3);
        private global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType Read3_ItemsChoiceType(string s)
            switch (s)
                case @"ExcludeQuery": return global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@ExcludeQuery;
                case @"MaxValueQuery": return global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MaxValueQuery;
                case @"MinValueQuery": return global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MinValueQuery;
                case @"RegularQuery": return global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@RegularQuery;
                default: throw CreateUnknownConstantException(s, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType));
        private global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery Read13_WildcardablePropertyQuery(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id36_WildcardablePropertyQuery && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery();
                if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id46_AllowGlobbing && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@AllowGlobbing = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@AllowGlobbingSpecified = true;
                    UnknownNode((object)o, @":AllowGlobbing");
            int whileIterations4 = 0;
            int readerCount4 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id11_CmdletParameterMetadata && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@CmdletParameterMetadata = Read12_Item(false, true);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                CheckReaderCount(ref whileIterations4, ref readerCount4);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter Read12_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id13_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter();
            global::System.String[] a_8 = null;
            int ca_8 = 0;
            global::System.String[] a_11 = null;
            int ca_11 = 0;
            global::System.String[] a_16 = null;
            int ca_16 = 0;
            bool[] paramsRead = new bool[18];
                if (!paramsRead[10] && ((object)Reader.LocalName == (object)_id47_IsMandatory && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@IsMandatory = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@IsMandatorySpecified = true;
                    paramsRead[10] = true;
                else if (((object)Reader.LocalName == (object)_id48_Aliases && (object)Reader.NamespaceURI == (object)_id4_Item))
                    string listValues = Reader.Value;
                    string[] vals = listValues.Split(null);
                    for (int i = 0; i < vals.Length; i++)
                        a_11 = (global::System.String[])EnsureArrayIndex(a_11, ca_11, typeof(global::System.String)); a_11[ca_11++] = vals[i];
                else if (!paramsRead[12] && ((object)Reader.LocalName == (object)_id49_PSName && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@PSName = Reader.Value;
                    paramsRead[12] = true;
                else if (!paramsRead[13] && ((object)Reader.LocalName == (object)_id50_Position && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@Position = CollapseWhitespace(Reader.Value);
                    paramsRead[13] = true;
                else if (!paramsRead[14] && ((object)Reader.LocalName == (object)_id51_ValueFromPipeline && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@ValueFromPipeline = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@ValueFromPipelineSpecified = true;
                    paramsRead[14] = true;
                else if (!paramsRead[15] && ((object)Reader.LocalName == (object)_id52_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@ValueFromPipelineByPropertyName = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@ValueFromPipelineByPropertyNameSpecified = true;
                    paramsRead[15] = true;
                else if (((object)Reader.LocalName == (object)_id53_CmdletParameterSets && (object)Reader.NamespaceURI == (object)_id4_Item))
                        a_16 = (global::System.String[])EnsureArrayIndex(a_16, ca_16, typeof(global::System.String)); a_16[ca_16++] = vals[i];
                else if (!paramsRead[17] && ((object)Reader.LocalName == (object)_id54_ErrorOnNoMatch && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@ErrorOnNoMatch = System.Xml.XmlConvert.ToBoolean(Reader.Value);
                    o.@ErrorOnNoMatchSpecified = true;
                    paramsRead[17] = true;
                    UnknownNode((object)o, @":IsMandatory, :Aliases, :PSName, :Position, :ValueFromPipeline, :ValueFromPipelineByPropertyName, :CmdletParameterSets, :ErrorOnNoMatch");
                o.@Aliases = (global::System.String[])ShrinkArray(a_11, ca_11, typeof(global::System.String), true);
                o.@CmdletParameterSets = (global::System.String[])ShrinkArray(a_16, ca_16, typeof(global::System.String), true);
            int whileIterations5 = 0;
            int readerCount5 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id55_AllowEmptyCollection && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@AllowEmptyCollection = Read1_Object(false, true);
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id56_AllowEmptyString && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@AllowEmptyString = Read1_Object(false, true);
                    else if (!paramsRead[2] && ((object)Reader.LocalName == (object)_id57_AllowNull && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@AllowNull = Read1_Object(false, true);
                    else if (!paramsRead[3] && ((object)Reader.LocalName == (object)_id58_ValidateNotNull && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@ValidateNotNull = Read1_Object(false, true);
                    else if (!paramsRead[4] && ((object)Reader.LocalName == (object)_id59_ValidateNotNullOrEmpty && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@ValidateNotNullOrEmpty = Read1_Object(false, true);
                        paramsRead[4] = true;
                    else if (!paramsRead[5] && ((object)Reader.LocalName == (object)_id60_ValidateCount && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@ValidateCount = Read4_Item(false, true);
                        paramsRead[5] = true;
                    else if (!paramsRead[6] && ((object)Reader.LocalName == (object)_id61_ValidateLength && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@ValidateLength = Read5_Item(false, true);
                        paramsRead[6] = true;
                    else if (!paramsRead[7] && ((object)Reader.LocalName == (object)_id62_ValidateRange && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@ValidateRange = Read6_Item(false, true);
                        paramsRead[7] = true;
                    else if (((object)Reader.LocalName == (object)_id63_ValidateSet && (object)Reader.NamespaceURI == (object)_id2_Item))
                        if (!ReadNull())
                            global::System.String[] a_8_0 = null;
                            int ca_8_0 = 0;
                            if ((Reader.IsEmptyElement))
                                int whileIterations6 = 0;
                                int readerCount6 = ReaderCount;
                                        if (((object)Reader.LocalName == (object)_id64_AllowedValue && (object)Reader.NamespaceURI == (object)_id2_Item))
                                                a_8_0 = (global::System.String[])EnsureArrayIndex(a_8_0, ca_8_0, typeof(global::System.String)); a_8_0[ca_8_0++] = Reader.ReadElementString();
                                            UnknownNode(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                    CheckReaderCount(ref whileIterations6, ref readerCount6);
                            o.@ValidateSet = (global::System.String[])ShrinkArray(a_8_0, ca_8_0, typeof(global::System.String), false);
                    else if (!paramsRead[9] && ((object)Reader.LocalName == (object)_id65_Obsolete && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@Obsolete = Read7_ObsoleteAttributeMetadata(false, true);
                        paramsRead[9] = true;
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Obsolete");
                CheckReaderCount(ref whileIterations5, ref readerCount5);
        private global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata Read7_ObsoleteAttributeMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id17_ObsoleteAttributeMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata();
            bool[] paramsRead = new bool[1];
                if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id66_Message && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@Message = Reader.Value;
                    UnknownNode((object)o, @":Message");
            int whileIterations7 = 0;
            int readerCount7 = ReaderCount;
                CheckReaderCount(ref whileIterations7, ref readerCount7);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange Read6_Item(bool isNullable, bool checkType)
            global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange();
                if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id67_Min && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@Min = CollapseWhitespace(Reader.Value);
                else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id68_Max && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@Max = CollapseWhitespace(Reader.Value);
                    UnknownNode((object)o, @":Min, :Max");
            int whileIterations8 = 0;
            int readerCount8 = ReaderCount;
                CheckReaderCount(ref whileIterations8, ref readerCount8);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength Read5_Item(bool isNullable, bool checkType)
            global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength();
            int whileIterations9 = 0;
            int readerCount9 = ReaderCount;
                CheckReaderCount(ref whileIterations9, ref readerCount9);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount Read4_Item(bool isNullable, bool checkType)
            global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount();
            int whileIterations10 = 0;
            int readerCount10 = ReaderCount;
                CheckReaderCount(ref whileIterations10, ref readerCount10);
        private global::System.Object Read1_Object(bool isNullable, bool checkType)
                if (isNull)
                    if (xsiType != null) return (global::System.Object)ReadTypedNull(xsiType);
                    else return null;
                if (xsiType == null)
                    return ReadTypedPrimitive(new System.Xml.XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema"));
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id40_EnumMetadataEnumValue && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read49_EnumMetadataEnumValue(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id39_EnumMetadataEnum && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read48_EnumMetadataEnum(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id38_ClassMetadataData && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read47_ClassMetadataData(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id32_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read46_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id16_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read44_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id15_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read43_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id14_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read42_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id10_AssociationAssociatedInstance && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read41_AssociationAssociatedInstance(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id5_ClassMetadataInstanceCmdlets && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read40_ClassMetadataInstanceCmdlets(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id3_ClassMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read36_ClassMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id24_StaticCmdletMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read34_StaticCmdletMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id34_InstanceCmdletMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read31_InstanceCmdletMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id28_CommonMethodParameterMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read26_CommonMethodParameterMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id29_StaticMethodParameterMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read27_StaticMethodParameterMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id31_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read25_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id26_CommonMethodMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read29_CommonMethodMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id33_InstanceMethodMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read30_InstanceMethodMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id27_StaticMethodMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read28_StaticMethodMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id30_CmdletOutputMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read23_CmdletOutputMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id21_GetCmdletMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read22_GetCmdletMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id22_CommonCmdletMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read21_CommonCmdletMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id25_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read45_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id6_GetCmdletParameters && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read19_GetCmdletParameters(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id20_QueryOption && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read18_QueryOption(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id9_Association && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read17_Association(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id7_PropertyMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read15_PropertyMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id35_PropertyQuery && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read14_PropertyQuery(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id36_WildcardablePropertyQuery && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read13_WildcardablePropertyQuery(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id11_CmdletParameterMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read10_CmdletParameterMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id12_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read11_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id13_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read12_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id18_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read9_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id19_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read8_Item(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id17_ObsoleteAttributeMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read7_ObsoleteAttributeMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id8_TypeMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    return Read2_TypeMetadata(isNullable, false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id37_ItemsChoiceType && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    object e = Read3_ItemsChoiceType(CollapseWhitespace(Reader.ReadString()));
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id69_ArrayOfString && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    global::System.String[] a = null;
                        global::System.String[] z_0_0 = null;
                        int cz_0_0 = 0;
                            int whileIterations11 = 0;
                            int readerCount11 = ReaderCount;
                                            z_0_0 = (global::System.String[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::System.String)); z_0_0[cz_0_0++] = Reader.ReadElementString();
                                CheckReaderCount(ref whileIterations11, ref readerCount11);
                        a = (global::System.String[])ShrinkArray(z_0_0, cz_0_0, typeof(global::System.String), false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id70_ArrayOfPropertyMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[] a = null;
                        global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[] z_0_0 = null;
                            int whileIterations12 = 0;
                            int readerCount12 = ReaderCount;
                                    if (((object)Reader.LocalName == (object)_id71_Property && (object)Reader.NamespaceURI == (object)_id2_Item))
                                        z_0_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata)); z_0_0[cz_0_0++] = Read15_PropertyMetadata(false, true);
                                        UnknownNode(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Property");
                                CheckReaderCount(ref whileIterations12, ref readerCount12);
                        a = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[])ShrinkArray(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata), false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id72_ArrayOfAssociation && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    global::Microsoft.PowerShell.Cmdletization.Xml.Association[] a = null;
                        global::Microsoft.PowerShell.Cmdletization.Xml.Association[] z_0_0 = null;
                            int whileIterations13 = 0;
                            int readerCount13 = ReaderCount;
                                    if (((object)Reader.LocalName == (object)_id9_Association && (object)Reader.NamespaceURI == (object)_id2_Item))
                                        z_0_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.Association[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.Association)); z_0_0[cz_0_0++] = Read17_Association(false, true);
                                        UnknownNode(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Association");
                                CheckReaderCount(ref whileIterations13, ref readerCount13);
                        a = (global::Microsoft.PowerShell.Cmdletization.Xml.Association[])ShrinkArray(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.Association), false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id73_ArrayOfQueryOption && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[] a = null;
                        global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[] z_0_0 = null;
                            int whileIterations14 = 0;
                            int readerCount14 = ReaderCount;
                                    if (((object)Reader.LocalName == (object)_id74_Option && (object)Reader.NamespaceURI == (object)_id2_Item))
                                        z_0_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption)); z_0_0[cz_0_0++] = Read18_QueryOption(false, true);
                                        UnknownNode(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Option");
                                CheckReaderCount(ref whileIterations14, ref readerCount14);
                        a = (global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[])ShrinkArray(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption), false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id23_ConfirmImpact && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    object e = Read20_ConfirmImpact(CollapseWhitespace(Reader.ReadString()));
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id75_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[] a = null;
                        global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[] z_0_0 = null;
                            int whileIterations15 = 0;
                            int readerCount15 = ReaderCount;
                                    if (((object)Reader.LocalName == (object)_id76_Parameter && (object)Reader.NamespaceURI == (object)_id2_Item))
                                        z_0_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata)); z_0_0[cz_0_0++] = Read27_StaticMethodParameterMetadata(false, true);
                                        UnknownNode(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameter");
                                CheckReaderCount(ref whileIterations15, ref readerCount15);
                        a = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[])ShrinkArray(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata), false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id77_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[] a = null;
                        global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[] z_0_0 = null;
                            int whileIterations16 = 0;
                            int readerCount16 = ReaderCount;
                                        z_0_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata)); z_0_0[cz_0_0++] = Read25_Item(false, true);
                                CheckReaderCount(ref whileIterations16, ref readerCount16);
                        a = (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[])ShrinkArray(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata), false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id78_ArrayOfStaticCmdletMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[] a = null;
                        global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[] z_0_0 = null;
                            int whileIterations17 = 0;
                            int readerCount17 = ReaderCount;
                                    if (((object)Reader.LocalName == (object)_id79_Cmdlet && (object)Reader.NamespaceURI == (object)_id2_Item))
                                        z_0_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata)); z_0_0[cz_0_0++] = Read34_StaticCmdletMetadata(false, true);
                                        UnknownNode(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                                CheckReaderCount(ref whileIterations17, ref readerCount17);
                        a = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[])ShrinkArray(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata), false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id80_ArrayOfClassMetadataData && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[] a = null;
                        global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[] z_0_0 = null;
                            int whileIterations18 = 0;
                            int readerCount18 = ReaderCount;
                                    if (((object)Reader.LocalName == (object)_id81_Data && (object)Reader.NamespaceURI == (object)_id2_Item))
                                        z_0_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData)); z_0_0[cz_0_0++] = Read35_ClassMetadataData(false, true);
                                        UnknownNode(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Data");
                                CheckReaderCount(ref whileIterations18, ref readerCount18);
                        a = (global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[])ShrinkArray(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData), false);
                else if (((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id82_ArrayOfEnumMetadataEnum && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
                    global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[] a = null;
                        global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[] z_0_0 = null;
                            int whileIterations19 = 0;
                            int readerCount19 = ReaderCount;
                                    if (((object)Reader.LocalName == (object)_id83_Enum && (object)Reader.NamespaceURI == (object)_id2_Item))
                                        z_0_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum)); z_0_0[cz_0_0++] = Read38_EnumMetadataEnum(false, true);
                                        UnknownNode(null, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Enum");
                                CheckReaderCount(ref whileIterations19, ref readerCount19);
                        a = (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[])ShrinkArray(z_0_0, cz_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum), false);
                    return ReadTypedPrimitive((System.Xml.XmlQualifiedName)xsiType);
            global::System.Object o;
            o = new global::System.Object();
            bool[] paramsRead = Array.Empty<bool>();
                if (!IsXmlnsAttribute(Reader.Name))
                    UnknownNode((object)o);
            int whileIterations20 = 0;
            int readerCount20 = ReaderCount;
                CheckReaderCount(ref whileIterations20, ref readerCount20);
        private global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum Read38_EnumMetadataEnum(bool isNullable, bool checkType)
            int whileIterations21 = 0;
            int readerCount21 = ReaderCount;
                CheckReaderCount(ref whileIterations21, ref readerCount21);
        private global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData Read35_ClassMetadataData(bool isNullable, bool checkType)
            int whileIterations22 = 0;
            int readerCount22 = ReaderCount;
                CheckReaderCount(ref whileIterations22, ref readerCount22);
        private global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata Read34_StaticCmdletMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id24_StaticCmdletMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata();
            global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata[] a_1 = null;
            int ca_1 = 0;
                o.@Method = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata[])ShrinkArray(a_1, ca_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata), true);
            int whileIterations23 = 0;
            int readerCount23 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id84_CmdletMetadata && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@CmdletMetadata = Read33_Item(false, true);
                    else if (((object)Reader.LocalName == (object)_id85_Method && (object)Reader.NamespaceURI == (object)_id2_Item))
                        a_1 = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata[])EnsureArrayIndex(a_1, ca_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata)); a_1[ca_1++] = Read28_StaticMethodMetadata(false, true);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Method");
                CheckReaderCount(ref whileIterations23, ref readerCount23);
        private global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata Read28_StaticMethodMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id27_StaticMethodMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata();
            global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[] a_2 = null;
            int ca_2 = 0;
                if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id86_MethodName && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@MethodName = Reader.Value;
                else if (!paramsRead[3] && ((object)Reader.LocalName == (object)_id87_CmdletParameterSet && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@CmdletParameterSet = Reader.Value;
                    UnknownNode((object)o, @":MethodName, :CmdletParameterSet");
            int whileIterations24 = 0;
            int readerCount24 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id88_ReturnValue && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@ReturnValue = Read24_Item(false, true);
                    else if (((object)Reader.LocalName == (object)_id89_Parameters && (object)Reader.NamespaceURI == (object)_id2_Item))
                            global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[] a_2_0 = null;
                            int ca_2_0 = 0;
                                int whileIterations25 = 0;
                                int readerCount25 = ReaderCount;
                                            a_2_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[])EnsureArrayIndex(a_2_0, ca_2_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata)); a_2_0[ca_2_0++] = Read27_StaticMethodParameterMetadata(false, true);
                                    CheckReaderCount(ref whileIterations25, ref readerCount25);
                            o.@Parameters = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata[])ShrinkArray(a_2_0, ca_2_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata), false);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ReturnValue, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameters");
                CheckReaderCount(ref whileIterations24, ref readerCount24);
        private global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata Read27_StaticMethodParameterMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id29_StaticMethodParameterMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata();
            bool[] paramsRead = new bool[5];
                if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id90_ParameterName && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@ParameterName = Reader.Value;
                else if (!paramsRead[2] && ((object)Reader.LocalName == (object)_id91_DefaultValue && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@DefaultValue = Reader.Value;
                    UnknownNode((object)o, @":ParameterName, :DefaultValue");
            int whileIterations26 = 0;
            int readerCount26 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id92_Type && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@Type = Read2_TypeMetadata(false, true);
                    else if (!paramsRead[3] && ((object)Reader.LocalName == (object)_id11_CmdletParameterMetadata && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@CmdletParameterMetadata = Read8_Item(false, true);
                    else if (!paramsRead[4] && ((object)Reader.LocalName == (object)_id30_CmdletOutputMetadata && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@CmdletOutputMetadata = Read23_CmdletOutputMetadata(false, true);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                CheckReaderCount(ref whileIterations26, ref readerCount26);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata Read23_CmdletOutputMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id30_CmdletOutputMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata();
                if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id49_PSName && (object)Reader.NamespaceURI == (object)_id4_Item))
                    UnknownNode((object)o, @":PSName");
            int whileIterations27 = 0;
            int readerCount27 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id93_ErrorCode && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@ErrorCode = Read1_Object(false, true);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ErrorCode");
                CheckReaderCount(ref whileIterations27, ref readerCount27);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter Read8_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id19_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter();
            bool[] paramsRead = new bool[16];
                    UnknownNode((object)o, @":IsMandatory, :Aliases, :PSName, :Position, :ValueFromPipeline, :ValueFromPipelineByPropertyName");
            int whileIterations28 = 0;
            int readerCount28 = ReaderCount;
                                int whileIterations29 = 0;
                                int readerCount29 = ReaderCount;
                                    CheckReaderCount(ref whileIterations29, ref readerCount29);
                CheckReaderCount(ref whileIterations28, ref readerCount28);
        private global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata Read2_TypeMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id8_TypeMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata();
                if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id94_PSType && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@PSType = Reader.Value;
                else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id95_ETSType && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@ETSType = Reader.Value;
                    UnknownNode((object)o, @":PSType, :ETSType");
            int whileIterations30 = 0;
            int readerCount30 = ReaderCount;
                CheckReaderCount(ref whileIterations30, ref readerCount30);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue Read24_Item(bool isNullable, bool checkType)
            global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue();
            int whileIterations31 = 0;
            int readerCount31 = ReaderCount;
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id30_CmdletOutputMetadata && (object)Reader.NamespaceURI == (object)_id2_Item))
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                CheckReaderCount(ref whileIterations31, ref readerCount31);
        private global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata Read33_Item(bool isNullable, bool checkType)
            global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata();
            global::System.String[] a_3 = null;
            int ca_3 = 0;
            bool[] paramsRead = new bool[7];
                if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id96_Verb && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@Verb = Reader.Value;
                else if (!paramsRead[2] && ((object)Reader.LocalName == (object)_id97_Noun && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@Noun = Reader.Value;
                        a_3 = (global::System.String[])EnsureArrayIndex(a_3, ca_3, typeof(global::System.String)); a_3[ca_3++] = vals[i];
                else if (!paramsRead[4] && ((object)Reader.LocalName == (object)_id23_ConfirmImpact && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@ConfirmImpact = Read20_ConfirmImpact(Reader.Value);
                    o.@ConfirmImpactSpecified = true;
                else if (!paramsRead[5] && ((object)Reader.LocalName == (object)_id98_HelpUri && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@HelpUri = CollapseWhitespace(Reader.Value);
                else if (!paramsRead[6] && ((object)Reader.LocalName == (object)_id99_DefaultCmdletParameterSet && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@DefaultCmdletParameterSet = Reader.Value;
                    UnknownNode((object)o, @":Verb, :Noun, :Aliases, :ConfirmImpact, :HelpUri, :DefaultCmdletParameterSet");
                o.@Aliases = (global::System.String[])ShrinkArray(a_3, ca_3, typeof(global::System.String), true);
            int whileIterations32 = 0;
            int readerCount32 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id65_Obsolete && (object)Reader.NamespaceURI == (object)_id2_Item))
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Obsolete");
                CheckReaderCount(ref whileIterations32, ref readerCount32);
        private global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact Read20_ConfirmImpact(string s)
                case @"None": return global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact.@None;
                case @"Low": return global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact.@Low;
                case @"Medium": return global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact.@Medium;
                case @"High": return global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact.@High;
                default: throw CreateUnknownConstantException(s, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact));
        private global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata Read25_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id31_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata();
            int whileIterations33 = 0;
            int readerCount33 = ReaderCount;
                        o.@CmdletParameterMetadata = Read9_Item(false, true);
                CheckReaderCount(ref whileIterations33, ref readerCount33);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter Read9_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id18_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter();
            bool[] paramsRead = new bool[15];
                else if (!paramsRead[14] && ((object)Reader.LocalName == (object)_id52_Item && (object)Reader.NamespaceURI == (object)_id4_Item))
                    UnknownNode((object)o, @":IsMandatory, :Aliases, :PSName, :Position, :ValueFromPipelineByPropertyName");
            int whileIterations34 = 0;
            int readerCount34 = ReaderCount;
                                int whileIterations35 = 0;
                                int readerCount35 = ReaderCount;
                                    CheckReaderCount(ref whileIterations35, ref readerCount35);
                CheckReaderCount(ref whileIterations34, ref readerCount34);
        private global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption Read18_QueryOption(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id20_QueryOption && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption();
            bool[] paramsRead = new bool[3];
                if (!paramsRead[2] && ((object)Reader.LocalName == (object)_id100_OptionName && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@OptionName = Reader.Value;
                    UnknownNode((object)o, @":OptionName");
            int whileIterations36 = 0;
            int readerCount36 = ReaderCount;
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id11_CmdletParameterMetadata && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@CmdletParameterMetadata = Read11_Item(false, true);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                CheckReaderCount(ref whileIterations36, ref readerCount36);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter Read11_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id12_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter();
            bool[] paramsRead = new bool[17];
                    UnknownNode((object)o, @":IsMandatory, :Aliases, :PSName, :Position, :ValueFromPipeline, :ValueFromPipelineByPropertyName, :CmdletParameterSets");
            int whileIterations37 = 0;
            int readerCount37 = ReaderCount;
                                int whileIterations38 = 0;
                                int readerCount38 = ReaderCount;
                                    CheckReaderCount(ref whileIterations38, ref readerCount38);
                CheckReaderCount(ref whileIterations37, ref readerCount37);
        private global::Microsoft.PowerShell.Cmdletization.Xml.Association Read17_Association(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id9_Association && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.Association o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.Association();
                if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id9_Association && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@Association1 = Reader.Value;
                else if (!paramsRead[2] && ((object)Reader.LocalName == (object)_id101_SourceRole && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@SourceRole = Reader.Value;
                else if (!paramsRead[3] && ((object)Reader.LocalName == (object)_id102_ResultRole && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@ResultRole = Reader.Value;
                    UnknownNode((object)o, @":Association, :SourceRole, :ResultRole");
            int whileIterations39 = 0;
            int readerCount39 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id103_AssociatedInstance && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@AssociatedInstance = Read16_AssociationAssociatedInstance(false, true);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AssociatedInstance");
                CheckReaderCount(ref whileIterations39, ref readerCount39);
        private global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance Read16_AssociationAssociatedInstance(bool isNullable, bool checkType)
            global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance();
            int whileIterations40 = 0;
            int readerCount40 = ReaderCount;
                CheckReaderCount(ref whileIterations40, ref readerCount40);
        private global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata Read15_PropertyMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id7_PropertyMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata();
            global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery[] a_1 = null;
            global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType[] choice_a_1 = null;
            int cchoice_a_1 = 0;
                if (!paramsRead[2] && ((object)Reader.LocalName == (object)_id104_PropertyName && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@PropertyName = Reader.Value;
                    UnknownNode((object)o, @":PropertyName");
                o.@Items = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery[])ShrinkArray(a_1, ca_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery), true);
                o.@ItemsElementName = (global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType[])ShrinkArray(choice_a_1, cchoice_a_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType), true);
            int whileIterations41 = 0;
            int readerCount41 = ReaderCount;
                    else if (((object)Reader.LocalName == (object)_id105_MaxValueQuery && (object)Reader.NamespaceURI == (object)_id2_Item))
                        a_1 = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery[])EnsureArrayIndex(a_1, ca_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery)); a_1[ca_1++] = Read14_PropertyQuery(false, true);
                        choice_a_1 = (global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType[])EnsureArrayIndex(choice_a_1, cchoice_a_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType)); choice_a_1[cchoice_a_1++] = global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MaxValueQuery;
                    else if (((object)Reader.LocalName == (object)_id106_RegularQuery && (object)Reader.NamespaceURI == (object)_id2_Item))
                        a_1 = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery[])EnsureArrayIndex(a_1, ca_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery)); a_1[ca_1++] = Read13_WildcardablePropertyQuery(false, true);
                        choice_a_1 = (global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType[])EnsureArrayIndex(choice_a_1, cchoice_a_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType)); choice_a_1[cchoice_a_1++] = global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@RegularQuery;
                    else if (((object)Reader.LocalName == (object)_id107_ExcludeQuery && (object)Reader.NamespaceURI == (object)_id2_Item))
                        choice_a_1 = (global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType[])EnsureArrayIndex(choice_a_1, cchoice_a_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType)); choice_a_1[cchoice_a_1++] = global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@ExcludeQuery;
                    else if (((object)Reader.LocalName == (object)_id108_MinValueQuery && (object)Reader.NamespaceURI == (object)_id2_Item))
                        choice_a_1 = (global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType[])EnsureArrayIndex(choice_a_1, cchoice_a_1, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType)); choice_a_1[cchoice_a_1++] = global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MinValueQuery;
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:MaxValueQuery, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:RegularQuery, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ExcludeQuery, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:MinValueQuery");
                CheckReaderCount(ref whileIterations41, ref readerCount41);
        private global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery Read14_PropertyQuery(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id35_PropertyQuery && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery();
            int whileIterations42 = 0;
            int readerCount42 = ReaderCount;
                CheckReaderCount(ref whileIterations42, ref readerCount42);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata Read10_CmdletParameterMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id11_CmdletParameterMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata();
            bool[] paramsRead = new bool[14];
                    UnknownNode((object)o, @":IsMandatory, :Aliases, :PSName, :Position");
            int whileIterations43 = 0;
            int readerCount43 = ReaderCount;
                                int whileIterations44 = 0;
                                int readerCount44 = ReaderCount;
                                    CheckReaderCount(ref whileIterations44, ref readerCount44);
                CheckReaderCount(ref whileIterations43, ref readerCount43);
        private global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters Read19_GetCmdletParameters(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id6_GetCmdletParameters && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters();
            global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[] a_0 = null;
            global::Microsoft.PowerShell.Cmdletization.Xml.Association[] a_1 = null;
            global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[] a_2 = null;
                if (!paramsRead[3] && ((object)Reader.LocalName == (object)_id99_DefaultCmdletParameterSet && (object)Reader.NamespaceURI == (object)_id4_Item))
                    UnknownNode((object)o, @":DefaultCmdletParameterSet");
            int whileIterations45 = 0;
            int readerCount45 = ReaderCount;
                    if (((object)Reader.LocalName == (object)_id109_QueryableProperties && (object)Reader.NamespaceURI == (object)_id2_Item))
                            global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[] a_0_0 = null;
                            int ca_0_0 = 0;
                                int whileIterations46 = 0;
                                int readerCount46 = ReaderCount;
                                            a_0_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[])EnsureArrayIndex(a_0_0, ca_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata)); a_0_0[ca_0_0++] = Read15_PropertyMetadata(false, true);
                                    CheckReaderCount(ref whileIterations46, ref readerCount46);
                            o.@QueryableProperties = (global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata[])ShrinkArray(a_0_0, ca_0_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata), false);
                    else if (((object)Reader.LocalName == (object)_id110_QueryableAssociations && (object)Reader.NamespaceURI == (object)_id2_Item))
                            global::Microsoft.PowerShell.Cmdletization.Xml.Association[] a_1_0 = null;
                            int ca_1_0 = 0;
                                int whileIterations47 = 0;
                                int readerCount47 = ReaderCount;
                                            a_1_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.Association[])EnsureArrayIndex(a_1_0, ca_1_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.Association)); a_1_0[ca_1_0++] = Read17_Association(false, true);
                                    CheckReaderCount(ref whileIterations47, ref readerCount47);
                            o.@QueryableAssociations = (global::Microsoft.PowerShell.Cmdletization.Xml.Association[])ShrinkArray(a_1_0, ca_1_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.Association), false);
                    else if (((object)Reader.LocalName == (object)_id111_QueryOptions && (object)Reader.NamespaceURI == (object)_id2_Item))
                            global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[] a_2_0 = null;
                                int whileIterations48 = 0;
                                int readerCount48 = ReaderCount;
                                            a_2_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[])EnsureArrayIndex(a_2_0, ca_2_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption)); a_2_0[ca_2_0++] = Read18_QueryOption(false, true);
                                    CheckReaderCount(ref whileIterations48, ref readerCount48);
                            o.@QueryOptions = (global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption[])ShrinkArray(a_2_0, ca_2_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption), false);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:QueryableProperties, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:QueryableAssociations, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:QueryOptions");
                CheckReaderCount(ref whileIterations45, ref readerCount45);
        private global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata Read45_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id25_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            int whileIterations49 = 0;
            int readerCount49 = ReaderCount;
                CheckReaderCount(ref whileIterations49, ref readerCount49);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata Read21_CommonCmdletMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id22_CommonCmdletMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata();
            bool[] paramsRead = new bool[6];
                    UnknownNode((object)o, @":Verb, :Noun, :Aliases, :ConfirmImpact, :HelpUri");
            int whileIterations50 = 0;
            int readerCount50 = ReaderCount;
                CheckReaderCount(ref whileIterations50, ref readerCount50);
        private global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata Read22_GetCmdletMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id21_GetCmdletMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata();
            int whileIterations51 = 0;
            int readerCount51 = ReaderCount;
                        o.@CmdletMetadata = Read21_CommonCmdletMetadata(false, true);
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id6_GetCmdletParameters && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@GetCmdletParameters = Read19_GetCmdletParameters(false, true);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters");
                CheckReaderCount(ref whileIterations51, ref readerCount51);
        private global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata Read30_InstanceMethodMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id33_InstanceMethodMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata();
            global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[] a_2 = null;
                    UnknownNode((object)o, @":MethodName");
            int whileIterations52 = 0;
            int readerCount52 = ReaderCount;
                            global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[] a_2_0 = null;
                                int whileIterations53 = 0;
                                int readerCount53 = ReaderCount;
                                            a_2_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[])EnsureArrayIndex(a_2_0, ca_2_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata)); a_2_0[ca_2_0++] = Read25_Item(false, true);
                                    CheckReaderCount(ref whileIterations53, ref readerCount53);
                            o.@Parameters = (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata[])ShrinkArray(a_2_0, ca_2_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata), false);
                CheckReaderCount(ref whileIterations52, ref readerCount52);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata Read29_CommonMethodMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id26_CommonMethodMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata();
            int whileIterations54 = 0;
            int readerCount54 = ReaderCount;
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ReturnValue");
                CheckReaderCount(ref whileIterations54, ref readerCount54);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata Read26_CommonMethodParameterMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id28_CommonMethodParameterMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata();
            int whileIterations55 = 0;
            int readerCount55 = ReaderCount;
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type");
                CheckReaderCount(ref whileIterations55, ref readerCount55);
        private global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata Read31_InstanceCmdletMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id34_InstanceCmdletMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata();
            int whileIterations56 = 0;
            int readerCount56 = ReaderCount;
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id85_Method && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@Method = Read30_InstanceMethodMetadata(false, true);
                    else if (!paramsRead[2] && ((object)Reader.LocalName == (object)_id6_GetCmdletParameters && (object)Reader.NamespaceURI == (object)_id2_Item))
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Method, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters");
                CheckReaderCount(ref whileIterations56, ref readerCount56);
        private global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata Read36_ClassMetadata(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id3_ClassMetadata && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata();
            global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[] a_3 = null;
            global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[] a_4 = null;
            int ca_4 = 0;
            bool[] paramsRead = new bool[8];
                if (!paramsRead[5] && ((object)Reader.LocalName == (object)_id112_CmdletAdapter && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@CmdletAdapter = Reader.Value;
                else if (!paramsRead[6] && ((object)Reader.LocalName == (object)_id113_ClassName && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@ClassName = Reader.Value;
                else if (!paramsRead[7] && ((object)Reader.LocalName == (object)_id114_ClassVersion && (object)Reader.NamespaceURI == (object)_id4_Item))
                    o.@ClassVersion = Reader.Value;
                    UnknownNode((object)o, @":CmdletAdapter, :ClassName, :ClassVersion");
            int whileIterations57 = 0;
            int readerCount57 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id115_Version && (object)Reader.NamespaceURI == (object)_id2_Item))
                            o.@Version = Reader.ReadElementString();
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id116_DefaultNoun && (object)Reader.NamespaceURI == (object)_id2_Item))
                            o.@DefaultNoun = Reader.ReadElementString();
                    else if (!paramsRead[2] && ((object)Reader.LocalName == (object)_id117_InstanceCmdlets && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@InstanceCmdlets = Read32_ClassMetadataInstanceCmdlets(false, true);
                    else if (((object)Reader.LocalName == (object)_id118_StaticCmdlets && (object)Reader.NamespaceURI == (object)_id2_Item))
                            global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[] a_3_0 = null;
                            int ca_3_0 = 0;
                                int whileIterations58 = 0;
                                int readerCount58 = ReaderCount;
                                            a_3_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[])EnsureArrayIndex(a_3_0, ca_3_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata)); a_3_0[ca_3_0++] = Read34_StaticCmdletMetadata(false, true);
                                    CheckReaderCount(ref whileIterations58, ref readerCount58);
                            o.@StaticCmdlets = (global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata[])ShrinkArray(a_3_0, ca_3_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata), false);
                    else if (((object)Reader.LocalName == (object)_id119_CmdletAdapterPrivateData && (object)Reader.NamespaceURI == (object)_id2_Item))
                            global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[] a_4_0 = null;
                            int ca_4_0 = 0;
                                int whileIterations59 = 0;
                                int readerCount59 = ReaderCount;
                                            a_4_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[])EnsureArrayIndex(a_4_0, ca_4_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData)); a_4_0[ca_4_0++] = Read35_ClassMetadataData(false, true);
                                    CheckReaderCount(ref whileIterations59, ref readerCount59);
                            o.@CmdletAdapterPrivateData = (global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData[])ShrinkArray(a_4_0, ca_4_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData), false);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Version, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:DefaultNoun, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:InstanceCmdlets, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:StaticCmdlets, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletAdapterPrivateData");
                CheckReaderCount(ref whileIterations57, ref readerCount57);
        private global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets Read32_ClassMetadataInstanceCmdlets(bool isNullable, bool checkType)
            global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets();
            global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata[] a_2 = null;
                o.@Cmdlet = (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata[])ShrinkArray(a_2, ca_2, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata), true);
            int whileIterations60 = 0;
            int readerCount60 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id6_GetCmdletParameters && (object)Reader.NamespaceURI == (object)_id2_Item))
                    else if (!paramsRead[1] && ((object)Reader.LocalName == (object)_id120_GetCmdlet && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@GetCmdlet = Read22_GetCmdletMetadata(false, true);
                    else if (((object)Reader.LocalName == (object)_id79_Cmdlet && (object)Reader.NamespaceURI == (object)_id2_Item))
                        a_2 = (global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata[])EnsureArrayIndex(a_2, ca_2, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata)); a_2[ca_2++] = Read31_InstanceCmdletMetadata(false, true);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdlet, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                CheckReaderCount(ref whileIterations60, ref readerCount60);
        private global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets Read40_ClassMetadataInstanceCmdlets(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id5_ClassMetadataInstanceCmdlets && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            int whileIterations61 = 0;
            int readerCount61 = ReaderCount;
                CheckReaderCount(ref whileIterations61, ref readerCount61);
        private global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance Read41_AssociationAssociatedInstance(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id10_AssociationAssociatedInstance && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            int whileIterations62 = 0;
            int readerCount62 = ReaderCount;
                CheckReaderCount(ref whileIterations62, ref readerCount62);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount Read42_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id14_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            int whileIterations63 = 0;
            int readerCount63 = ReaderCount;
                CheckReaderCount(ref whileIterations63, ref readerCount63);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength Read43_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id15_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            int whileIterations64 = 0;
            int readerCount64 = ReaderCount;
                CheckReaderCount(ref whileIterations64, ref readerCount64);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange Read44_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id16_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            int whileIterations65 = 0;
            int readerCount65 = ReaderCount;
                CheckReaderCount(ref whileIterations65, ref readerCount65);
        private global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue Read46_Item(bool isNullable, bool checkType)
                if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)_id32_Item && (object)((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)_id2_Item))
            int whileIterations66 = 0;
            int readerCount66 = ReaderCount;
                CheckReaderCount(ref whileIterations66, ref readerCount66);
        private global::Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata Read39_PowerShellMetadata(bool isNullable, bool checkType)
            global::Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata o;
            o = new global::Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata();
            global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[] a_1 = null;
            int whileIterations67 = 0;
            int readerCount67 = ReaderCount;
                    if (!paramsRead[0] && ((object)Reader.LocalName == (object)_id121_Class && (object)Reader.NamespaceURI == (object)_id2_Item))
                        o.@Class = Read36_ClassMetadata(false, true);
                    else if (((object)Reader.LocalName == (object)_id122_Enums && (object)Reader.NamespaceURI == (object)_id2_Item))
                            global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[] a_1_0 = null;
                                int whileIterations68 = 0;
                                int readerCount68 = ReaderCount;
                                            a_1_0 = (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[])EnsureArrayIndex(a_1_0, ca_1_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum)); a_1_0[ca_1_0++] = Read38_EnumMetadataEnum(false, true);
                                    CheckReaderCount(ref whileIterations68, ref readerCount68);
                            o.@Enums = (global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum[])ShrinkArray(a_1_0, ca_1_0, typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum), false);
                        UnknownNode((object)o, @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Class, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Enums");
                CheckReaderCount(ref whileIterations67, ref readerCount67);
        private string _id72_ArrayOfAssociation;
        private string _id46_AllowGlobbing;
        private string _id6_GetCmdletParameters;
        private string _id25_Item;
        private string _id62_ValidateRange;
        private string _id118_StaticCmdlets;
        private string _id58_ValidateNotNull;
        private string _id17_ObsoleteAttributeMetadata;
        private string _id49_PSName;
        private string _id116_DefaultNoun;
        private string _id38_ClassMetadataData;
        private string _id114_ClassVersion;
        private string _id66_Message;
        private string _id65_Obsolete;
        private string _id51_ValueFromPipeline;
        private string _id108_MinValueQuery;
        private string _id119_CmdletAdapterPrivateData;
        private string _id21_GetCmdletMetadata;
        private string _id120_GetCmdlet;
        private string _id67_Min;
        private string _id56_AllowEmptyString;
        private string _id30_CmdletOutputMetadata;
        private string _id106_RegularQuery;
        private string _id74_Option;
        private string _id75_Item;
        private string _id23_ConfirmImpact;
        private string _id117_InstanceCmdlets;
        private string _id83_Enum;
        private string _id40_EnumMetadataEnumValue;
        private string _id111_QueryOptions;
        private string _id34_InstanceCmdletMetadata;
        private string _id60_ValidateCount;
        private string _id45_BitwiseFlags;
        private string _id81_Data;
        private string _id31_Item;
        private string _id1_PowerShellMetadata;
        private string _id98_HelpUri;
        private string _id91_DefaultValue;
        private string _id4_Item;
        private string _id32_Item;
        private string _id43_EnumName;
        private string _id122_Enums;
        private string _id82_ArrayOfEnumMetadataEnum;
        private string _id14_Item;
        private string _id48_Aliases;
        private string _id115_Version;
        private string _id11_CmdletParameterMetadata;
        private string _id70_ArrayOfPropertyMetadata;
        private string _id9_Association;
        private string _id102_ResultRole;
        private string _id29_StaticMethodParameterMetadata;
        private string _id97_Noun;
        private string _id47_IsMandatory;
        private string _id35_PropertyQuery;
        private string _id54_ErrorOnNoMatch;
        private string _id3_ClassMetadata;
        private string _id77_Item;
        private string _id2_Item;
        private string _id22_CommonCmdletMetadata;
        private string _id37_ItemsChoiceType;
        private string _id36_WildcardablePropertyQuery;
        private string _id113_ClassName;
        private string _id64_AllowedValue;
        private string _id52_Item;
        private string _id55_AllowEmptyCollection;
        private string _id13_Item;
        private string _id76_Parameter;
        private string _id19_Item;
        private string _id105_MaxValueQuery;
        private string _id101_SourceRole;
        private string _id5_ClassMetadataInstanceCmdlets;
        private string _id112_CmdletAdapter;
        private string _id10_AssociationAssociatedInstance;
        private string _id93_ErrorCode;
        private string _id41_Name;
        private string _id68_Max;
        private string _id50_Position;
        private string _id100_OptionName;
        private string _id84_CmdletMetadata;
        private string _id87_CmdletParameterSet;
        private string _id104_PropertyName;
        private string _id28_CommonMethodParameterMetadata;
        private string _id107_ExcludeQuery;
        private string _id92_Type;
        private string _id33_InstanceMethodMetadata;
        private string _id63_ValidateSet;
        private string _id53_CmdletParameterSets;
        private string _id15_Item;
        private string _id109_QueryableProperties;
        private string _id57_AllowNull;
        private string _id80_ArrayOfClassMetadataData;
        private string _id99_DefaultCmdletParameterSet;
        private string _id20_QueryOption;
        private string _id89_Parameters;
        private string _id90_ParameterName;
        private string _id61_ValidateLength;
        private string _id78_ArrayOfStaticCmdletMetadata;
        private string _id16_Item;
        private string _id39_EnumMetadataEnum;
        private string _id7_PropertyMetadata;
        private string _id110_QueryableAssociations;
        private string _id86_MethodName;
        private string _id8_TypeMetadata;
        private string _id71_Property;
        private string _id27_StaticMethodMetadata;
        private string _id94_PSType;
        private string _id44_UnderlyingType;
        private string _id103_AssociatedInstance;
        private string _id79_Cmdlet;
        private string _id18_Item;
        private string _id85_Method;
        private string _id95_ETSType;
        private string _id26_CommonMethodMetadata;
        private string _id88_ReturnValue;
        private string _id69_ArrayOfString;
        private string _id24_StaticCmdletMetadata;
        private string _id59_ValidateNotNullOrEmpty;
        private string _id96_Verb;
        private string _id121_Class;
        private string _id73_ArrayOfQueryOption;
        private string _id12_Item;
        private string _id42_Value;
        protected override void InitIDs()
            _id72_ArrayOfAssociation = Reader.NameTable.Add(@"ArrayOfAssociation");
            _id46_AllowGlobbing = Reader.NameTable.Add(@"AllowGlobbing");
            _id6_GetCmdletParameters = Reader.NameTable.Add(@"GetCmdletParameters");
            _id25_Item = Reader.NameTable.Add(@"StaticCmdletMetadataCmdletMetadata");
            _id62_ValidateRange = Reader.NameTable.Add(@"ValidateRange");
            _id118_StaticCmdlets = Reader.NameTable.Add(@"StaticCmdlets");
            _id58_ValidateNotNull = Reader.NameTable.Add(@"ValidateNotNull");
            _id17_ObsoleteAttributeMetadata = Reader.NameTable.Add(@"ObsoleteAttributeMetadata");
            _id49_PSName = Reader.NameTable.Add(@"PSName");
            _id116_DefaultNoun = Reader.NameTable.Add(@"DefaultNoun");
            _id38_ClassMetadataData = Reader.NameTable.Add(@"ClassMetadataData");
            _id114_ClassVersion = Reader.NameTable.Add(@"ClassVersion");
            _id66_Message = Reader.NameTable.Add(@"Message");
            _id65_Obsolete = Reader.NameTable.Add(@"Obsolete");
            _id51_ValueFromPipeline = Reader.NameTable.Add(@"ValueFromPipeline");
            _id108_MinValueQuery = Reader.NameTable.Add(@"MinValueQuery");
            _id119_CmdletAdapterPrivateData = Reader.NameTable.Add(@"CmdletAdapterPrivateData");
            _id21_GetCmdletMetadata = Reader.NameTable.Add(@"GetCmdletMetadata");
            _id120_GetCmdlet = Reader.NameTable.Add(@"GetCmdlet");
            _id67_Min = Reader.NameTable.Add(@"Min");
            _id56_AllowEmptyString = Reader.NameTable.Add(@"AllowEmptyString");
            _id30_CmdletOutputMetadata = Reader.NameTable.Add(@"CmdletOutputMetadata");
            _id106_RegularQuery = Reader.NameTable.Add(@"RegularQuery");
            _id74_Option = Reader.NameTable.Add(@"Option");
            _id75_Item = Reader.NameTable.Add(@"ArrayOfStaticMethodParameterMetadata");
            _id23_ConfirmImpact = Reader.NameTable.Add(@"ConfirmImpact");
            _id117_InstanceCmdlets = Reader.NameTable.Add(@"InstanceCmdlets");
            _id83_Enum = Reader.NameTable.Add(@"Enum");
            _id40_EnumMetadataEnumValue = Reader.NameTable.Add(@"EnumMetadataEnumValue");
            _id111_QueryOptions = Reader.NameTable.Add(@"QueryOptions");
            _id34_InstanceCmdletMetadata = Reader.NameTable.Add(@"InstanceCmdletMetadata");
            _id60_ValidateCount = Reader.NameTable.Add(@"ValidateCount");
            _id45_BitwiseFlags = Reader.NameTable.Add(@"BitwiseFlags");
            _id81_Data = Reader.NameTable.Add(@"Data");
            _id31_Item = Reader.NameTable.Add(@"InstanceMethodParameterMetadata");
            _id1_PowerShellMetadata = Reader.NameTable.Add(@"PowerShellMetadata");
            _id98_HelpUri = Reader.NameTable.Add(@"HelpUri");
            _id91_DefaultValue = Reader.NameTable.Add(@"DefaultValue");
            _id4_Item = Reader.NameTable.Add(string.Empty);
            _id32_Item = Reader.NameTable.Add(@"CommonMethodMetadataReturnValue");
            _id43_EnumName = Reader.NameTable.Add(@"EnumName");
            _id122_Enums = Reader.NameTable.Add(@"Enums");
            _id82_ArrayOfEnumMetadataEnum = Reader.NameTable.Add(@"ArrayOfEnumMetadataEnum");
            _id14_Item = Reader.NameTable.Add(@"CmdletParameterMetadataValidateCount");
            _id48_Aliases = Reader.NameTable.Add(@"Aliases");
            _id115_Version = Reader.NameTable.Add(@"Version");
            _id11_CmdletParameterMetadata = Reader.NameTable.Add(@"CmdletParameterMetadata");
            _id70_ArrayOfPropertyMetadata = Reader.NameTable.Add(@"ArrayOfPropertyMetadata");
            _id9_Association = Reader.NameTable.Add(@"Association");
            _id102_ResultRole = Reader.NameTable.Add(@"ResultRole");
            _id29_StaticMethodParameterMetadata = Reader.NameTable.Add(@"StaticMethodParameterMetadata");
            _id97_Noun = Reader.NameTable.Add(@"Noun");
            _id47_IsMandatory = Reader.NameTable.Add(@"IsMandatory");
            _id35_PropertyQuery = Reader.NameTable.Add(@"PropertyQuery");
            _id54_ErrorOnNoMatch = Reader.NameTable.Add(@"ErrorOnNoMatch");
            _id3_ClassMetadata = Reader.NameTable.Add(@"ClassMetadata");
            _id77_Item = Reader.NameTable.Add(@"ArrayOfInstanceMethodParameterMetadata");
            _id2_Item = Reader.NameTable.Add(@"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            _id22_CommonCmdletMetadata = Reader.NameTable.Add(@"CommonCmdletMetadata");
            _id37_ItemsChoiceType = Reader.NameTable.Add(@"ItemsChoiceType");
            _id36_WildcardablePropertyQuery = Reader.NameTable.Add(@"WildcardablePropertyQuery");
            _id113_ClassName = Reader.NameTable.Add(@"ClassName");
            _id64_AllowedValue = Reader.NameTable.Add(@"AllowedValue");
            _id52_Item = Reader.NameTable.Add(@"ValueFromPipelineByPropertyName");
            _id55_AllowEmptyCollection = Reader.NameTable.Add(@"AllowEmptyCollection");
            _id13_Item = Reader.NameTable.Add(@"CmdletParameterMetadataForGetCmdletFilteringParameter");
            _id76_Parameter = Reader.NameTable.Add(@"Parameter");
            _id19_Item = Reader.NameTable.Add(@"CmdletParameterMetadataForStaticMethodParameter");
            _id105_MaxValueQuery = Reader.NameTable.Add(@"MaxValueQuery");
            _id101_SourceRole = Reader.NameTable.Add(@"SourceRole");
            _id5_ClassMetadataInstanceCmdlets = Reader.NameTable.Add(@"ClassMetadataInstanceCmdlets");
            _id112_CmdletAdapter = Reader.NameTable.Add(@"CmdletAdapter");
            _id10_AssociationAssociatedInstance = Reader.NameTable.Add(@"AssociationAssociatedInstance");
            _id93_ErrorCode = Reader.NameTable.Add(@"ErrorCode");
            _id41_Name = Reader.NameTable.Add(@"Name");
            _id68_Max = Reader.NameTable.Add(@"Max");
            _id50_Position = Reader.NameTable.Add(@"Position");
            _id100_OptionName = Reader.NameTable.Add(@"OptionName");
            _id84_CmdletMetadata = Reader.NameTable.Add(@"CmdletMetadata");
            _id87_CmdletParameterSet = Reader.NameTable.Add(@"CmdletParameterSet");
            _id104_PropertyName = Reader.NameTable.Add(@"PropertyName");
            _id28_CommonMethodParameterMetadata = Reader.NameTable.Add(@"CommonMethodParameterMetadata");
            _id107_ExcludeQuery = Reader.NameTable.Add(@"ExcludeQuery");
            _id92_Type = Reader.NameTable.Add(@"Type");
            _id33_InstanceMethodMetadata = Reader.NameTable.Add(@"InstanceMethodMetadata");
            _id63_ValidateSet = Reader.NameTable.Add(@"ValidateSet");
            _id53_CmdletParameterSets = Reader.NameTable.Add(@"CmdletParameterSets");
            _id15_Item = Reader.NameTable.Add(@"CmdletParameterMetadataValidateLength");
            _id109_QueryableProperties = Reader.NameTable.Add(@"QueryableProperties");
            _id57_AllowNull = Reader.NameTable.Add(@"AllowNull");
            _id80_ArrayOfClassMetadataData = Reader.NameTable.Add(@"ArrayOfClassMetadataData");
            _id99_DefaultCmdletParameterSet = Reader.NameTable.Add(@"DefaultCmdletParameterSet");
            _id20_QueryOption = Reader.NameTable.Add(@"QueryOption");
            _id89_Parameters = Reader.NameTable.Add(@"Parameters");
            _id90_ParameterName = Reader.NameTable.Add(@"ParameterName");
            _id61_ValidateLength = Reader.NameTable.Add(@"ValidateLength");
            _id78_ArrayOfStaticCmdletMetadata = Reader.NameTable.Add(@"ArrayOfStaticCmdletMetadata");
            _id16_Item = Reader.NameTable.Add(@"CmdletParameterMetadataValidateRange");
            _id39_EnumMetadataEnum = Reader.NameTable.Add(@"EnumMetadataEnum");
            _id7_PropertyMetadata = Reader.NameTable.Add(@"PropertyMetadata");
            _id110_QueryableAssociations = Reader.NameTable.Add(@"QueryableAssociations");
            _id86_MethodName = Reader.NameTable.Add(@"MethodName");
            _id8_TypeMetadata = Reader.NameTable.Add(@"TypeMetadata");
            _id71_Property = Reader.NameTable.Add(@"Property");
            _id27_StaticMethodMetadata = Reader.NameTable.Add(@"StaticMethodMetadata");
            _id94_PSType = Reader.NameTable.Add(@"PSType");
            _id44_UnderlyingType = Reader.NameTable.Add(@"UnderlyingType");
            _id103_AssociatedInstance = Reader.NameTable.Add(@"AssociatedInstance");
            _id79_Cmdlet = Reader.NameTable.Add(@"Cmdlet");
            _id18_Item = Reader.NameTable.Add(@"CmdletParameterMetadataForInstanceMethodParameter");
            _id85_Method = Reader.NameTable.Add(@"Method");
            _id95_ETSType = Reader.NameTable.Add(@"ETSType");
            _id26_CommonMethodMetadata = Reader.NameTable.Add(@"CommonMethodMetadata");
            _id88_ReturnValue = Reader.NameTable.Add(@"ReturnValue");
            _id69_ArrayOfString = Reader.NameTable.Add(@"ArrayOfString");
            _id24_StaticCmdletMetadata = Reader.NameTable.Add(@"StaticCmdletMetadata");
            _id59_ValidateNotNullOrEmpty = Reader.NameTable.Add(@"ValidateNotNullOrEmpty");
            _id96_Verb = Reader.NameTable.Add(@"Verb");
            _id121_Class = Reader.NameTable.Add(@"Class");
            _id73_ArrayOfQueryOption = Reader.NameTable.Add(@"ArrayOfQueryOption");
            _id12_Item = Reader.NameTable.Add(@"CmdletParameterMetadataForGetCmdletParameter");
            _id42_Value = Reader.NameTable.Add(@"Value");
    internal abstract class XmlSerializer1 : System.Xml.Serialization.XmlSerializer
        protected override System.Xml.Serialization.XmlSerializationReader CreateReader()
            return new XmlSerializationReader1();
        protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter()
            return new XmlSerializationWriter1();
    internal sealed class PowerShellMetadataSerializer : XmlSerializer1
        public override bool CanDeserialize(System.Xml.XmlReader xmlReader)
            return xmlReader.IsStartElement(@"PowerShellMetadata", @"http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
        protected override void Serialize(object objectToSerialize, System.Xml.Serialization.XmlSerializationWriter writer)
            ((XmlSerializationWriter1)writer).Write50_PowerShellMetadata(objectToSerialize);
        protected override object Deserialize(System.Xml.Serialization.XmlSerializationReader reader)
            return ((XmlSerializationReader1)reader).Read50_PowerShellMetadata();
    internal sealed class ClassMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"ClassMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write51_ClassMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read51_ClassMetadata();
    internal sealed class ClassMetadataInstanceCmdletsSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"ClassMetadataInstanceCmdlets", string.Empty);
            ((XmlSerializationWriter1)writer).Write52_ClassMetadataInstanceCmdlets(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read52_ClassMetadataInstanceCmdlets();
    internal sealed class GetCmdletParametersSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"GetCmdletParameters", string.Empty);
            ((XmlSerializationWriter1)writer).Write53_GetCmdletParameters(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read53_GetCmdletParameters();
    internal sealed class PropertyMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"PropertyMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write54_PropertyMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read54_PropertyMetadata();
    internal sealed class TypeMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"TypeMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write55_TypeMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read55_TypeMetadata();
    internal sealed class AssociationSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"Association", string.Empty);
            ((XmlSerializationWriter1)writer).Write56_Association(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read56_Association();
    internal sealed class AssociationAssociatedInstanceSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"AssociationAssociatedInstance", string.Empty);
            ((XmlSerializationWriter1)writer).Write57_AssociationAssociatedInstance(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read57_AssociationAssociatedInstance();
    internal sealed class CmdletParameterMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CmdletParameterMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write58_CmdletParameterMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read58_CmdletParameterMetadata();
    internal sealed class CmdletParameterMetadataForGetCmdletParameterSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CmdletParameterMetadataForGetCmdletParameter", string.Empty);
            ((XmlSerializationWriter1)writer).Write59_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read59_Item();
    internal sealed class CmdletParameterMetadataForGetCmdletFilteringParameterSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CmdletParameterMetadataForGetCmdletFilteringParameter", string.Empty);
            ((XmlSerializationWriter1)writer).Write60_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read60_Item();
    internal sealed class CmdletParameterMetadataValidateCountSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CmdletParameterMetadataValidateCount", string.Empty);
            ((XmlSerializationWriter1)writer).Write61_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read61_Item();
    internal sealed class CmdletParameterMetadataValidateLengthSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CmdletParameterMetadataValidateLength", string.Empty);
            ((XmlSerializationWriter1)writer).Write62_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read62_Item();
    internal sealed class CmdletParameterMetadataValidateRangeSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CmdletParameterMetadataValidateRange", string.Empty);
            ((XmlSerializationWriter1)writer).Write63_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read63_Item();
    internal sealed class ObsoleteAttributeMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"ObsoleteAttributeMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write64_ObsoleteAttributeMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read64_ObsoleteAttributeMetadata();
    internal sealed class CmdletParameterMetadataForInstanceMethodParameterSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CmdletParameterMetadataForInstanceMethodParameter", string.Empty);
            ((XmlSerializationWriter1)writer).Write65_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read65_Item();
    internal sealed class CmdletParameterMetadataForStaticMethodParameterSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CmdletParameterMetadataForStaticMethodParameter", string.Empty);
            ((XmlSerializationWriter1)writer).Write66_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read66_Item();
    internal sealed class QueryOptionSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"QueryOption", string.Empty);
            ((XmlSerializationWriter1)writer).Write67_QueryOption(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read67_QueryOption();
    internal sealed class GetCmdletMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"GetCmdletMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write68_GetCmdletMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read68_GetCmdletMetadata();
    internal sealed class CommonCmdletMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CommonCmdletMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write69_CommonCmdletMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read69_CommonCmdletMetadata();
    internal sealed class ConfirmImpactSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"ConfirmImpact", string.Empty);
            ((XmlSerializationWriter1)writer).Write70_ConfirmImpact(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read70_ConfirmImpact();
    internal sealed class StaticCmdletMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"StaticCmdletMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write71_StaticCmdletMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read71_StaticCmdletMetadata();
    internal sealed class StaticCmdletMetadataCmdletMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"StaticCmdletMetadataCmdletMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write72_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read72_Item();
    internal sealed class CommonMethodMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CommonMethodMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write73_CommonMethodMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read73_CommonMethodMetadata();
    internal sealed class StaticMethodMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"StaticMethodMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write74_StaticMethodMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read74_StaticMethodMetadata();
    internal sealed class CommonMethodParameterMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CommonMethodParameterMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write75_CommonMethodParameterMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read75_CommonMethodParameterMetadata();
    internal sealed class StaticMethodParameterMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"StaticMethodParameterMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write76_StaticMethodParameterMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read76_StaticMethodParameterMetadata();
    internal sealed class CmdletOutputMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CmdletOutputMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write77_CmdletOutputMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read77_CmdletOutputMetadata();
    internal sealed class InstanceMethodParameterMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"InstanceMethodParameterMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write78_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read78_Item();
    internal sealed class CommonMethodMetadataReturnValueSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"CommonMethodMetadataReturnValue", string.Empty);
            ((XmlSerializationWriter1)writer).Write79_Item(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read79_Item();
    internal sealed class InstanceMethodMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"InstanceMethodMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write80_InstanceMethodMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read80_InstanceMethodMetadata();
    internal sealed class InstanceCmdletMetadataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"InstanceCmdletMetadata", string.Empty);
            ((XmlSerializationWriter1)writer).Write81_InstanceCmdletMetadata(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read81_InstanceCmdletMetadata();
    internal sealed class PropertyQuerySerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"PropertyQuery", string.Empty);
            ((XmlSerializationWriter1)writer).Write82_PropertyQuery(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read82_PropertyQuery();
    internal sealed class WildcardablePropertyQuerySerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"WildcardablePropertyQuery", string.Empty);
            ((XmlSerializationWriter1)writer).Write83_WildcardablePropertyQuery(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read83_WildcardablePropertyQuery();
    internal sealed class ItemsChoiceTypeSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"ItemsChoiceType", string.Empty);
            ((XmlSerializationWriter1)writer).Write84_ItemsChoiceType(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read84_ItemsChoiceType();
    internal sealed class ClassMetadataDataSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"ClassMetadataData", string.Empty);
            ((XmlSerializationWriter1)writer).Write85_ClassMetadataData(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read85_ClassMetadataData();
    internal sealed class EnumMetadataEnumSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"EnumMetadataEnum", string.Empty);
            ((XmlSerializationWriter1)writer).Write86_EnumMetadataEnum(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read86_EnumMetadataEnum();
    internal sealed class EnumMetadataEnumValueSerializer : XmlSerializer1
            return xmlReader.IsStartElement(@"EnumMetadataEnumValue", string.Empty);
            ((XmlSerializationWriter1)writer).Write87_EnumMetadataEnumValue(objectToSerialize);
            return ((XmlSerializationReader1)reader).Read87_EnumMetadataEnumValue();
    internal class XmlSerializerContract : global::System.Xml.Serialization.XmlSerializerImplementation
        public override global::System.Xml.Serialization.XmlSerializationReader Reader { get { return new XmlSerializationReader1(); } }
        public override global::System.Xml.Serialization.XmlSerializationWriter Writer { get { return new XmlSerializationWriter1(); } }
        private System.Collections.Hashtable _readMethods = null;
        public override System.Collections.Hashtable ReadMethods
                if (_readMethods == null)
                    System.Collections.Hashtable _tmp = new System.Collections.Hashtable();
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata:http://schemas.microsoft.com/cmdlets-over-objects/2009/11::False:"] = @"Read50_PowerShellMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata::"] = @"Read51_ClassMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets::"] = @"Read52_ClassMetadataInstanceCmdlets";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters::"] = @"Read53_GetCmdletParameters";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata::"] = @"Read54_PropertyMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata::"] = @"Read55_TypeMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.Association::"] = @"Read56_Association";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance::"] = @"Read57_AssociationAssociatedInstance";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata::"] = @"Read58_CmdletParameterMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter::"] = @"Read59_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter::"] = @"Read60_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount::"] = @"Read61_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength::"] = @"Read62_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange::"] = @"Read63_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata::"] = @"Read64_ObsoleteAttributeMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter::"] = @"Read65_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter::"] = @"Read66_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.QueryOption::"] = @"Read67_QueryOption";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata::"] = @"Read68_GetCmdletMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata::"] = @"Read69_CommonCmdletMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact::"] = @"Read70_ConfirmImpact";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata::"] = @"Read71_StaticCmdletMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata::"] = @"Read72_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata::"] = @"Read73_CommonMethodMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata::"] = @"Read74_StaticMethodMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata::"] = @"Read75_CommonMethodParameterMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata::"] = @"Read76_StaticMethodParameterMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata::"] = @"Read77_CmdletOutputMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata::"] = @"Read78_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue::"] = @"Read79_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata::"] = @"Read80_InstanceMethodMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata::"] = @"Read81_InstanceCmdletMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery::"] = @"Read82_PropertyQuery";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery::"] = @"Read83_WildcardablePropertyQuery";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType::"] = @"Read84_ItemsChoiceType";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData::"] = @"Read85_ClassMetadataData";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum::"] = @"Read86_EnumMetadataEnum";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue::"] = @"Read87_EnumMetadataEnumValue";
                    if (_readMethods == null) _readMethods = _tmp;
                return _readMethods;
        private System.Collections.Hashtable _writeMethods = null;
        public override System.Collections.Hashtable WriteMethods
                if (_writeMethods == null)
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata:http://schemas.microsoft.com/cmdlets-over-objects/2009/11::False:"] = @"Write50_PowerShellMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata::"] = @"Write51_ClassMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets::"] = @"Write52_ClassMetadataInstanceCmdlets";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters::"] = @"Write53_GetCmdletParameters";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata::"] = @"Write54_PropertyMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata::"] = @"Write55_TypeMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.Association::"] = @"Write56_Association";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance::"] = @"Write57_AssociationAssociatedInstance";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata::"] = @"Write58_CmdletParameterMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter::"] = @"Write59_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter::"] = @"Write60_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount::"] = @"Write61_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength::"] = @"Write62_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange::"] = @"Write63_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata::"] = @"Write64_ObsoleteAttributeMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter::"] = @"Write65_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter::"] = @"Write66_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.QueryOption::"] = @"Write67_QueryOption";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata::"] = @"Write68_GetCmdletMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata::"] = @"Write69_CommonCmdletMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact::"] = @"Write70_ConfirmImpact";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata::"] = @"Write71_StaticCmdletMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata::"] = @"Write72_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata::"] = @"Write73_CommonMethodMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata::"] = @"Write74_StaticMethodMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata::"] = @"Write75_CommonMethodParameterMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata::"] = @"Write76_StaticMethodParameterMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata::"] = @"Write77_CmdletOutputMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata::"] = @"Write78_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue::"] = @"Write79_Item";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata::"] = @"Write80_InstanceMethodMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata::"] = @"Write81_InstanceCmdletMetadata";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery::"] = @"Write82_PropertyQuery";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery::"] = @"Write83_WildcardablePropertyQuery";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType::"] = @"Write84_ItemsChoiceType";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData::"] = @"Write85_ClassMetadataData";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum::"] = @"Write86_EnumMetadataEnum";
                    _tmp[@"Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue::"] = @"Write87_EnumMetadataEnumValue";
                    if (_writeMethods == null) _writeMethods = _tmp;
                return _writeMethods;
        private System.Collections.Hashtable _typedSerializers = null;
        public override System.Collections.Hashtable TypedSerializers
                if (_typedSerializers == null)
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance::", new AssociationAssociatedInstanceSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.Association::", new AssociationSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets::", new ClassMetadataInstanceCmdletsSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata:http://schemas.microsoft.com/cmdlets-over-objects/2009/11::False:", new PowerShellMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue::", new EnumMetadataEnumValueSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata::", new StaticCmdletMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType::", new ItemsChoiceTypeSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery::", new PropertyQuerySerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata::", new CmdletParameterMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata::", new CommonMethodParameterMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata::", new StaticMethodMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata::", new ObsoleteAttributeMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata::", new InstanceCmdletMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue::", new CommonMethodMetadataReturnValueSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata::", new PropertyMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter::", new CmdletParameterMetadataForGetCmdletParameterSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata::", new CmdletOutputMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum::", new EnumMetadataEnumSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.QueryOption::", new QueryOptionSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata::", new InstanceMethodParameterMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange::", new CmdletParameterMetadataValidateRangeSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData::", new ClassMetadataDataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact::", new ConfirmImpactSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata::", new StaticCmdletMetadataCmdletMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata::", new GetCmdletMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength::", new CmdletParameterMetadataValidateLengthSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata::", new InstanceMethodMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata::", new CommonMethodMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount::", new CmdletParameterMetadataValidateCountSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters::", new GetCmdletParametersSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter::", new CmdletParameterMetadataForInstanceMethodParameterSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata::", new CommonCmdletMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata::", new TypeMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter::", new CmdletParameterMetadataForGetCmdletFilteringParameterSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata::", new StaticMethodParameterMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter::", new CmdletParameterMetadataForStaticMethodParameterSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata::", new ClassMetadataSerializer());
                    _tmp.Add(@"Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery::", new WildcardablePropertyQuerySerializer());
                    if (_typedSerializers == null) _typedSerializers = _tmp;
                return _typedSerializers;
        public override bool CanSerialize(System.Type type)
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.Association)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum)) return true;
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue)) return true;
        public override System.Xml.Serialization.XmlSerializer GetSerializer(System.Type type)
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata)) return new PowerShellMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata)) return new ClassMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets)) return new ClassMetadataInstanceCmdletsSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters)) return new GetCmdletParametersSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata)) return new PropertyMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata)) return new TypeMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.Association)) return new AssociationSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance)) return new AssociationAssociatedInstanceSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata)) return new CmdletParameterMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter)) return new CmdletParameterMetadataForGetCmdletParameterSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter)) return new CmdletParameterMetadataForGetCmdletFilteringParameterSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount)) return new CmdletParameterMetadataValidateCountSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength)) return new CmdletParameterMetadataValidateLengthSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange)) return new CmdletParameterMetadataValidateRangeSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata)) return new ObsoleteAttributeMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter)) return new CmdletParameterMetadataForInstanceMethodParameterSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter)) return new CmdletParameterMetadataForStaticMethodParameterSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption)) return new QueryOptionSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata)) return new GetCmdletMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata)) return new CommonCmdletMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact)) return new ConfirmImpactSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata)) return new StaticCmdletMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata)) return new StaticCmdletMetadataCmdletMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata)) return new CommonMethodMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata)) return new StaticMethodMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata)) return new CommonMethodParameterMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata)) return new StaticMethodParameterMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata)) return new CmdletOutputMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata)) return new InstanceMethodParameterMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue)) return new CommonMethodMetadataReturnValueSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata)) return new InstanceMethodMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata)) return new InstanceCmdletMetadataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery)) return new PropertyQuerySerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery)) return new WildcardablePropertyQuerySerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType)) return new ItemsChoiceTypeSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData)) return new ClassMetadataDataSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum)) return new EnumMetadataEnumSerializer();
            if (type == typeof(global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue)) return new EnumMetadataEnumValueSerializer();
    internal class XmlSerializationReader1
        #region Copy_From_XmlSerializationReader
        // The fields, properties and methods in this section are copied from XmlSerializationReader with
        // some necessary adjustment:
        //  1. XmlReader.ReadString() and XmlReader.ReadElementString() are not in CoreCLR. They are replaced by
        //     XmlReader.ReadElementContentAsString() as suggested in MSDN.
        //  2. GetXsiType(). In the context of CDXML deserialization, GetXsiType() will always return null, as all
        //     CDXML files are under the namespace "http://schemas.microsoft.com/cmdlets-over-objects/2009/11".
        //  3. ReadTypedPrimitive(XmlQualifiedName type) and ReadTypedNull(XmlQualifiedName type). See the comments
        //     in them for more information.
        #region "Constructor"
        internal XmlSerializationReader1(XmlReader reader)
            _r = reader;
            _d = null;
            _schemaNsID = _r.NameTable.Add("http://www.w3.org/2001/XMLSchema");
            _schemaNs2000ID = _r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema");
            _schemaNs1999ID = _r.NameTable.Add("http://www.w3.org/1999/XMLSchema");
            _schemaNonXsdTypesNsID = _r.NameTable.Add("http://microsoft.com/wsdl/types/");
            _instanceNsID = _r.NameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
            _instanceNs2000ID = _r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema-instance");
            _instanceNs1999ID = _r.NameTable.Add("http://www.w3.org/1999/XMLSchema-instance");
            _soapNsID = _r.NameTable.Add("http://schemas.xmlsoap.org/soap/encoding/");
            _soap12NsID = _r.NameTable.Add("http://www.w3.org/2003/05/soap-encoding");
            _schemaID = _r.NameTable.Add("schema");
            _wsdlNsID = _r.NameTable.Add("http://schemas.xmlsoap.org/wsdl/");
            _wsdlArrayTypeID = _r.NameTable.Add("arrayType");
            _nullID = _r.NameTable.Add("null");
            _nilID = _r.NameTable.Add("nil");
            _typeID = _r.NameTable.Add("type");
            _arrayTypeID = _r.NameTable.Add("arrayType");
            _itemTypeID = _r.NameTable.Add("itemType");
            _arraySizeID = _r.NameTable.Add("arraySize");
            _arrayID = _r.NameTable.Add("Array");
            _urTypeID = _r.NameTable.Add("anyType");
            InitIDs();
        #endregion "Constructor"
        #region "Field Definition"
        XmlReader _r;
        XmlDocument _d;
        bool _soap12;
        bool _isReturnValue;
        bool _decodeName = true;
        string _schemaNsID;
        string _schemaNs1999ID;
        string _schemaNs2000ID;
        string _schemaNonXsdTypesNsID;
        string _instanceNsID;
        string _instanceNs2000ID;
        string _instanceNs1999ID;
        string _soapNsID;
        string _soap12NsID;
        string _schemaID;
        string _wsdlNsID;
        string _wsdlArrayTypeID;
        string _nullID;
        string _nilID;
        string _typeID;
        string _arrayTypeID;
        string _itemTypeID;
        string _arraySizeID;
        string _arrayID;
        string _urTypeID;
        string _stringID;
        string _intID;
        string _booleanID;
        string _shortID;
        string _longID;
        string _floatID;
        string _doubleID;
        string _decimalID;
        string _dateTimeID;
        string _qnameID;
        string _dateID;
        string _timeID;
        string _hexBinaryID;
        string _base64BinaryID;
        string _base64ID;
        string _unsignedByteID;
        string _byteID;
        string _unsignedShortID;
        string _unsignedIntID;
        string _unsignedLongID;
        string _oldDecimalID;
        string _oldTimeInstantID;
        string _anyURIID;
        string _durationID;
        string _ENTITYID;
        string _ENTITIESID;
        string _gDayID;
        string _gMonthID;
        string _gMonthDayID;
        string _gYearID;
        string _gYearMonthID;
        string _IDID;
        string _IDREFID;
        string _IDREFSID;
        string _integerID;
        string _languageID;
        string _nameID;
        string _NCNameID;
        string _NMTOKENID;
        string _NMTOKENSID;
        string _negativeIntegerID;
        string _nonPositiveIntegerID;
        string _nonNegativeIntegerID;
        string _normalizedStringID;
        string _NOTATIONID;
        string _positiveIntegerID;
        string _tokenID;
        string _charID;
        string _guidID;
        static object s_primitiveTypedObject = new object();
        #endregion "Field Definition"
        #region "Property Definition"
        internal XmlReader Reader
                return _r;
        internal int ReaderCount
                // XmlSerializationReader implementation is:
                //    return checkDeserializeAdvances ? countingReader.AdvanceCount : 0;
                // and checkDeserializeAdvances is set in the static constructor:
                //    XmlSerializerSection configSection = ConfigurationManager.GetSection(ConfigurationStrings.XmlSerializerSectionPath) as XmlSerializerSection;
                //    checkDeserializeAdvances = (configSection == null) ? false : configSection.CheckDeserializeAdvances;
                // When XmlSerializationReader is used in powershell, there is no configuration file defined for it, so 'checkDeserializeAdvances' will actually
                // always be 'false'. Therefore, here we directly return 0 for 'ReaderCount'.
        internal bool DecodeName
                return _decodeName;
                _decodeName = value;
        protected XmlDocument Document
                if (_d == null)
                    _d = new XmlDocument(_r.NameTable);
                return _d;
        #endregion "Property Definition"
        #region "Method Definition"
        internal void InitPrimitiveIDs()
            if (_tokenID != null) return;
            object ns = _r.NameTable.Add("http://www.w3.org/2001/XMLSchema");
            object ns2 = _r.NameTable.Add("http://microsoft.com/wsdl/types/");
            _stringID = _r.NameTable.Add("string");
            _intID = _r.NameTable.Add("int");
            _booleanID = _r.NameTable.Add("boolean");
            _shortID = _r.NameTable.Add("short");
            _longID = _r.NameTable.Add("long");
            _floatID = _r.NameTable.Add("float");
            _doubleID = _r.NameTable.Add("double");
            _decimalID = _r.NameTable.Add("decimal");
            _dateTimeID = _r.NameTable.Add("dateTime");
            _qnameID = _r.NameTable.Add("QName");
            _dateID = _r.NameTable.Add("date");
            _timeID = _r.NameTable.Add("time");
            _hexBinaryID = _r.NameTable.Add("hexBinary");
            _base64BinaryID = _r.NameTable.Add("base64Binary");
            _unsignedByteID = _r.NameTable.Add("unsignedByte");
            _byteID = _r.NameTable.Add("byte");
            _unsignedShortID = _r.NameTable.Add("unsignedShort");
            _unsignedIntID = _r.NameTable.Add("unsignedInt");
            _unsignedLongID = _r.NameTable.Add("unsignedLong");
            _oldDecimalID = _r.NameTable.Add("decimal");
            _oldTimeInstantID = _r.NameTable.Add("timeInstant");
            _charID = _r.NameTable.Add("char");
            _guidID = _r.NameTable.Add("guid");
            _base64ID = _r.NameTable.Add("base64");
            _anyURIID = _r.NameTable.Add("anyURI");
            _durationID = _r.NameTable.Add("duration");
            _ENTITYID = _r.NameTable.Add("ENTITY");
            _ENTITIESID = _r.NameTable.Add("ENTITIES");
            _gDayID = _r.NameTable.Add("gDay");
            _gMonthID = _r.NameTable.Add("gMonth");
            _gMonthDayID = _r.NameTable.Add("gMonthDay");
            _gYearID = _r.NameTable.Add("gYear");
            _gYearMonthID = _r.NameTable.Add("gYearMonth");
            _IDID = _r.NameTable.Add("ID");
            _IDREFID = _r.NameTable.Add("IDREF");
            _IDREFSID = _r.NameTable.Add("IDREFS");
            _integerID = _r.NameTable.Add("integer");
            _languageID = _r.NameTable.Add("language");
            _nameID = _r.NameTable.Add("Name");
            _NCNameID = _r.NameTable.Add("NCName");
            _NMTOKENID = _r.NameTable.Add("NMTOKEN");
            _NMTOKENSID = _r.NameTable.Add("NMTOKENS");
            _negativeIntegerID = _r.NameTable.Add("negativeInteger");
            _nonNegativeIntegerID = _r.NameTable.Add("nonNegativeInteger");
            _nonPositiveIntegerID = _r.NameTable.Add("nonPositiveInteger");
            _normalizedStringID = _r.NameTable.Add("normalizedString");
            _NOTATIONID = _r.NameTable.Add("NOTATION");
            _positiveIntegerID = _r.NameTable.Add("positiveInteger");
            _tokenID = _r.NameTable.Add("token");
        protected void CheckReaderCount(ref int whileIterations, ref int readerCount)
        private string CurrentTag()
            switch (_r.NodeType)
                case XmlNodeType.Element:
                    return "<" + _r.LocalName + " xmlns='" + _r.NamespaceURI + "'>";
                case XmlNodeType.EndElement:
                    return ">";
                case XmlNodeType.Text:
                    return _r.Value;
                case XmlNodeType.CDATA:
                    return "CDATA";
                case XmlNodeType.Comment:
                    return "<--";
                case XmlNodeType.ProcessingInstruction:
                    return "<?";
                    return "(unknown)";
        protected Exception CreateUnknownNodeException()
            return new InvalidOperationException("XmlUnknownNode: " + CurrentTag());
        protected Exception CreateUnknownTypeException(XmlQualifiedName type)
            return new InvalidOperationException(string.Create(CultureInfo.CurrentCulture, $"XmlUnknownType. Name: {type.Name}, Namespace: {type.Namespace}, CurrentTag: {CurrentTag()}"));
        protected Exception CreateUnknownConstantException(string value, Type enumType)
            return new InvalidOperationException(string.Create(CultureInfo.CurrentCulture, $"XmlUnknownConstant. Value: {value}, EnumType: {enumType.Name}"));
        protected Array ShrinkArray(Array a, int length, Type elementType, bool isNullable)
            if (a == null)
                if (isNullable) return null;
                return Array.CreateInstance(elementType, 0);
            if (a.Length == length) return a;
            Array b = Array.CreateInstance(elementType, length);
            Array.Copy(a, b, length);
            return b;
        protected Array EnsureArrayIndex(Array a, int index, Type elementType)
            if (a == null) return Array.CreateInstance(elementType, 32);
            if (index < a.Length) return a;
            Array b = Array.CreateInstance(elementType, a.Length * 2);
            Array.Copy(a, b, index);
        protected string CollapseWhitespace(string value)
        protected bool IsXmlnsAttribute(string name)
            if (!name.StartsWith("xmlns", StringComparison.Ordinal)) return false;
            if (name.Length == 5) return true;
            return name[5] == ':';
        protected void UnknownNode(object o)
            UnknownNode(o, null);
        protected void UnknownNode(object o, string qnames)
            if (_r.NodeType == XmlNodeType.None || _r.NodeType == XmlNodeType.Whitespace)
                _r.Read();
            if (_r.NodeType == XmlNodeType.EndElement)
            if (_r.NodeType == XmlNodeType.Attribute)
            else if (_r.NodeType == XmlNodeType.Element)
                _r.Skip();
                UnknownNode(Document.ReadNode(_r), o, qnames);
        private void UnknownNode(XmlNode unknownNode, object o, string qnames)
            if (unknownNode == null)
            // No XmlDeserializationEvents in CoreCLR. The events like 'onUnknownNode', 'onUnknownAttribute' and
            // 'onUnknownElement' are not used in powershell code, so it's safe to not perform extra operations here.
        protected void ReadEndElement()
            while (_r.NodeType == XmlNodeType.Whitespace) _r.Skip();
            if (_r.NodeType == XmlNodeType.None) _r.Skip();
            else _r.ReadEndElement();
        protected string ReadString(string value, bool trim)
            // This method is used only in Read47_ClassMetadataData and Read35_ClassMetadataData when the current XmlNodeType
            // is one of the following:
            //   XmlNodeType.Text
            //   XmlNodeType.CDATA
            //   XmlNodeType.Whitespace
            //   XmlNodeType.SignificantWhitespace
            // In this case, we use 'ReadContentAsString()' to read the text content at the current position.
            // We cannot use 'ReadElementContentAsString()'. It will fail because the XmlReader is not positioned on an Element start node.
            string str = _r.ReadContentAsString();
            if (str != null && trim)
                str = str.Trim();
            return value + str;
        protected XmlQualifiedName ToXmlQualifiedName(string value)
            return ToXmlQualifiedName(value, DecodeName);
        internal XmlQualifiedName ToXmlQualifiedName(string value, bool decodeName)
            int colon = value == null ? -1 : value.LastIndexOf(':');
            string prefix = colon < 0 ? null : value.Substring(0, colon);
            string localName = value.Substring(colon + 1);
            if (decodeName)
                prefix = XmlConvert.DecodeName(prefix);
                localName = XmlConvert.DecodeName(localName);
            if (prefix == null || prefix.Length == 0)
                return new XmlQualifiedName(_r.NameTable.Add(value), _r.LookupNamespace(string.Empty));
                string ns = _r.LookupNamespace(prefix);
                if (ns == null)
                    // Namespace prefix '{0}' is not defined.
                    throw new InvalidOperationException(string.Create(CultureInfo.CurrentCulture, $"XmlUndefinedAlias. Prefix: {prefix}"));
                return new XmlQualifiedName(_r.NameTable.Add(localName), ns);
        /// In the context of CDXML deserialization, GetXsiType() will
        /// always return null, as all CDXML files are under the namespace
        /// "http://schemas.microsoft.com/cmdlets-over-objects/2009/11",
        /// so the GetAttribute(..) operation here will always return null.
        protected XmlQualifiedName GetXsiType()
            string type = _r.GetAttribute(_typeID, _instanceNsID);
                type = _r.GetAttribute(_typeID, _instanceNs2000ID);
                    type = _r.GetAttribute(_typeID, _instanceNs1999ID);
            return ToXmlQualifiedName(type, false);
        protected bool GetNullAttr()
            string isNull = _r.GetAttribute(_nilID, _instanceNsID);
            if (isNull == null)
                isNull = _r.GetAttribute(_nullID, _instanceNsID);
                isNull = _r.GetAttribute(_nullID, _instanceNs2000ID);
                    isNull = _r.GetAttribute(_nullID, _instanceNs1999ID);
            if (isNull == null || !XmlConvert.ToBoolean(isNull)) return false;
        protected bool ReadNull()
            if (!GetNullAttr()) return false;
            if (_r.IsEmptyElement)
            _r.ReadStartElement();
            int whileIterations = 0;
            int readerCount = ReaderCount;
            while (_r.NodeType != XmlNodeType.EndElement)
                UnknownNode(null);
                CheckReaderCount(ref whileIterations, ref readerCount);
        bool IsPrimitiveNamespace(string ns)
            return (object)ns == (object)_schemaNsID ||
                   (object)ns == (object)_schemaNonXsdTypesNsID ||
                   (object)ns == (object)_soapNsID ||
                   (object)ns == (object)_soap12NsID ||
                   (object)ns == (object)_schemaNs2000ID ||
                   (object)ns == (object)_schemaNs1999ID;
        protected object ReadTypedPrimitive(XmlQualifiedName type)
            InitPrimitiveIDs();
            // This method is only used in Read1_Object(bool isNullable, bool checkType).
            // This method is called only when we want to get a value for tag elements that don't take values, such as
            // ValidateNotNull, AllowNull, AllEmptyString, ErrorCode and etc. e.g. <ValidateNotNullOrEmpty />, <ErrorCode />.
            // We don't actually use the value, only check if the value is null, so as to decide whether the tag element
            // is specified in CDXML file.
            if (!IsPrimitiveNamespace(type.Namespace) || (object)type.Name == (object)_urTypeID)
                return s_primitiveTypedObject;
            // CDXML files are all under the namespace 'http://schemas.microsoft.com/cmdlets-over-objects/2009/11', so
            // they will never fall into the following namespaces:
            //     schemaNsID, soapNsID, soap12NsID, schemaNs2000ID, schemaNs1999ID, schemaNonXsdTypesNsID
            // Actually, in the context of CDXML deserialization, GetXsiType() will always return null, so
            // the only possible 'type' passed in this method should be like this:
            //     type.Name = "anyType"; type.Namespace = "http://www.w3.org/2001/XMLSchema"
            // Therefore, execution of this method should always fall in the above IF block.
            throw new InvalidOperationException("ReadTypedPrimitive - code should be unreachable for its usage in CDXML.");
        protected object ReadTypedNull(XmlQualifiedName type)
            // This method is invoked only if GetXsiType() returns a value that is not null. Actually, in the context of
            // CDXML deserialization, GetXsiType() will always return null, so this method will never be called in runtime.
        #endregion "Method Definition"
        #endregion Copy_From_XmlSerializationReader
                        o = Read20_ConfirmImpact(Reader.ReadElementContentAsString());
                        o = Read3_ItemsChoiceType(Reader.ReadElementContentAsString());
        global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue Read49_EnumMetadataEnumValue(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum Read48_EnumMetadataEnum(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue Read37_EnumMetadataEnumValue(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData Read47_ClassMetadataData(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType Read3_ItemsChoiceType(string s)
        global::Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery Read13_WildcardablePropertyQuery(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter Read12_Item(bool isNullable, bool checkType)
                                                a_8_0 = (global::System.String[])EnsureArrayIndex(a_8_0, ca_8_0, typeof(global::System.String)); a_8_0[ca_8_0++] = Reader.ReadElementContentAsString();
        global::Microsoft.PowerShell.Cmdletization.Xml.ObsoleteAttributeMetadata Read7_ObsoleteAttributeMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange Read6_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength Read5_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount Read4_Item(bool isNullable, bool checkType)
        global::System.Object Read1_Object(bool isNullable, bool checkType)
                    object e = Read3_ItemsChoiceType(CollapseWhitespace(Reader.ReadElementContentAsString()));
                                            z_0_0 = (global::System.String[])EnsureArrayIndex(z_0_0, cz_0_0, typeof(global::System.String)); z_0_0[cz_0_0++] = Reader.ReadElementContentAsString();
                    object e = Read20_ConfirmImpact(CollapseWhitespace(Reader.ReadElementContentAsString()));
        global::Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum Read38_EnumMetadataEnum(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData Read35_ClassMetadataData(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata Read34_StaticCmdletMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata Read28_StaticMethodMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata Read27_StaticMethodParameterMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata Read23_CmdletOutputMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter Read8_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata Read2_TypeMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue Read24_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata Read33_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact Read20_ConfirmImpact(string s)
        global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata Read25_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter Read9_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.QueryOption Read18_QueryOption(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter Read11_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.Association Read17_Association(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance Read16_AssociationAssociatedInstance(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata Read15_PropertyMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery Read14_PropertyQuery(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata Read10_CmdletParameterMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters Read19_GetCmdletParameters(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata Read45_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata Read21_CommonCmdletMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata Read22_GetCmdletMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata Read30_InstanceMethodMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata Read29_CommonMethodMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata Read26_CommonMethodParameterMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata Read31_InstanceCmdletMetadata(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata Read36_ClassMetadata(bool isNullable, bool checkType)
                            o.@Version = Reader.ReadElementContentAsString();
                            o.@DefaultNoun = Reader.ReadElementContentAsString();
        global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets Read32_ClassMetadataInstanceCmdlets(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets Read40_ClassMetadataInstanceCmdlets(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance Read41_AssociationAssociatedInstance(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount Read42_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength Read43_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange Read44_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue Read46_Item(bool isNullable, bool checkType)
        global::Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata Read39_PowerShellMetadata(bool isNullable, bool checkType)
        string _id72_ArrayOfAssociation;
        string _id46_AllowGlobbing;
        string _id6_GetCmdletParameters;
        string _id25_Item;
        string _id62_ValidateRange;
        string _id118_StaticCmdlets;
        string _id58_ValidateNotNull;
        string _id17_ObsoleteAttributeMetadata;
        string _id49_PSName;
        string _id116_DefaultNoun;
        string _id38_ClassMetadataData;
        string _id114_ClassVersion;
        string _id66_Message;
        string _id65_Obsolete;
        string _id51_ValueFromPipeline;
        string _id108_MinValueQuery;
        string _id119_CmdletAdapterPrivateData;
        string _id21_GetCmdletMetadata;
        string _id120_GetCmdlet;
        string _id67_Min;
        string _id56_AllowEmptyString;
        string _id30_CmdletOutputMetadata;
        string _id106_RegularQuery;
        string _id74_Option;
        string _id75_Item;
        string _id23_ConfirmImpact;
        string _id117_InstanceCmdlets;
        string _id83_Enum;
        string _id40_EnumMetadataEnumValue;
        string _id111_QueryOptions;
        string _id34_InstanceCmdletMetadata;
        string _id60_ValidateCount;
        string _id45_BitwiseFlags;
        string _id81_Data;
        string _id31_Item;
        string _id1_PowerShellMetadata;
        string _id98_HelpUri;
        string _id91_DefaultValue;
        string _id4_Item;
        string _id32_Item;
        string _id43_EnumName;
        string _id122_Enums;
        string _id82_ArrayOfEnumMetadataEnum;
        string _id14_Item;
        string _id48_Aliases;
        string _id115_Version;
        string _id11_CmdletParameterMetadata;
        string _id70_ArrayOfPropertyMetadata;
        string _id9_Association;
        string _id102_ResultRole;
        string _id29_StaticMethodParameterMetadata;
        string _id97_Noun;
        string _id47_IsMandatory;
        string _id35_PropertyQuery;
        string _id54_ErrorOnNoMatch;
        string _id3_ClassMetadata;
        string _id77_Item;
        string _id2_Item;
        string _id22_CommonCmdletMetadata;
        string _id37_ItemsChoiceType;
        string _id36_WildcardablePropertyQuery;
        string _id113_ClassName;
        string _id64_AllowedValue;
        string _id52_Item;
        string _id55_AllowEmptyCollection;
        string _id13_Item;
        string _id76_Parameter;
        string _id19_Item;
        string _id105_MaxValueQuery;
        string _id101_SourceRole;
        string _id5_ClassMetadataInstanceCmdlets;
        string _id112_CmdletAdapter;
        string _id10_AssociationAssociatedInstance;
        string _id93_ErrorCode;
        string _id41_Name;
        string _id68_Max;
        string _id50_Position;
        string _id100_OptionName;
        string _id84_CmdletMetadata;
        string _id87_CmdletParameterSet;
        string _id104_PropertyName;
        string _id28_CommonMethodParameterMetadata;
        string _id107_ExcludeQuery;
        string _id92_Type;
        string _id33_InstanceMethodMetadata;
        string _id63_ValidateSet;
        string _id53_CmdletParameterSets;
        string _id15_Item;
        string _id109_QueryableProperties;
        string _id57_AllowNull;
        string _id80_ArrayOfClassMetadataData;
        string _id99_DefaultCmdletParameterSet;
        string _id20_QueryOption;
        string _id89_Parameters;
        string _id90_ParameterName;
        string _id61_ValidateLength;
        string _id78_ArrayOfStaticCmdletMetadata;
        string _id16_Item;
        string _id39_EnumMetadataEnum;
        string _id7_PropertyMetadata;
        string _id110_QueryableAssociations;
        string _id86_MethodName;
        string _id8_TypeMetadata;
        string _id71_Property;
        string _id27_StaticMethodMetadata;
        string _id94_PSType;
        string _id44_UnderlyingType;
        string _id103_AssociatedInstance;
        string _id79_Cmdlet;
        string _id18_Item;
        string _id85_Method;
        string _id95_ETSType;
        string _id26_CommonMethodMetadata;
        string _id88_ReturnValue;
        string _id69_ArrayOfString;
        string _id24_StaticCmdletMetadata;
        string _id59_ValidateNotNullOrEmpty;
        string _id96_Verb;
        string _id121_Class;
        string _id73_ArrayOfQueryOption;
        string _id12_Item;
        string _id42_Value;
        private void InitIDs()
    internal sealed class PowerShellMetadataSerializer
        internal object Deserialize(XmlReader reader)
            ArgumentNullException.ThrowIfNull(reader);
            XmlSerializationReader1 cdxmlSerializationReader = new XmlSerializationReader1(reader);
            return cdxmlSerializationReader.Read50_PowerShellMetadata();
