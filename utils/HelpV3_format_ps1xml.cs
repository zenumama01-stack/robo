    internal sealed class HelpV3_Format_Ps1Xml
            var MamlTypeControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"name", enumerateCollection: true)
            var MamlParameterValueControl = CustomControl.Create()
                        .AddText(" ")
                        .AddScriptBlockExpressionBinding(@"""[""", selectedByScript: @"$_.required -ne ""true""")
                        .AddScriptBlockExpressionBinding(@"""<"" + $_ + "">""")
                        .AddScriptBlockExpressionBinding(@"""]""", selectedByScript: @"$_.required -ne ""true""")
            var control1 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Value")
                        .AddNewline()
            var MamlPossibleValueControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"possibleValue", enumerateCollection: true, customControl: control1)
            var MamlShortDescriptionControl = CustomControl.Create()
                    .StartEntry(entrySelectedByType: new[] { "MamlParaTextItem" })
                        .AddPropertyExpressionBinding(@"Text")
            var MamlDescriptionControl = CustomControl.Create()
                    .StartEntry(entrySelectedByType: new[] { "MamlOrderedListTextItem" })
                        .StartFrame(firstLineHanging: 4)
                            .AddPropertyExpressionBinding(@"Tag")
                    .StartEntry(entrySelectedByType: new[] { "MamlUnorderedListTextItem" })
                        .StartFrame(firstLineHanging: 2)
                    .StartEntry(entrySelectedByType: new[] { "MamlDefinitionTextItem" })
                        .AddPropertyExpressionBinding(@"Term")
                        .StartFrame(leftIndent: 4)
                            .AddPropertyExpressionBinding(@"Definition")
            var MamlFieldCustomControl = CustomControl.Create()
                    .StartEntry(entrySelectedByType: new[] { "MamlPSClassHelpInfo#field" })
                        .AddScriptBlockExpressionBinding(@"""["" + $_.fieldData.type.name + ""] $"" + $_.fieldData.name")
                        .AddScriptBlockExpressionBinding(@"$_.introduction.text")
            var MamlMethodCustomControl = CustomControl.Create()
                    .StartEntry(entrySelectedByType: new[] { "MamlPSClassHelpInfo#method" })
                            .AddScriptBlockExpressionBinding(@"function GetParam
    if(-not $_.Parameters) { return $null }
    $_.Parameters.Parameter | ForEach-Object {
        if($_.type) { $param = ""[$($_.type.name)] `$$($_.name), "" }
        else { $param = ""[object] `$$($_.name), "" }
        $params += $param
    $params = $params.Remove($params.Length - 2)
    return $params
$paramOutput = GetParam
""["" + $_.returnValue.type.name + ""] "" + $_.title + ""("" + $($paramOutput) + "")""")
            var RelatedLinksHelpInfoControl = CustomControl.Create()
                            .AddScriptBlockExpressionBinding(StringUtil.Format(@"Set-StrictMode -Off
if (($_.relatedLinks -ne $()) -and ($_.relatedLinks.navigationLink -ne $()) -and ($_.relatedLinks.navigationLink.Length -ne 0))
    ""    {0}`""Get-Help $($_.Details.Name) -Online`""""
}}", HelpDisplayStrings.RelatedLinksHelpInfo))
            var MamlParameterControl = CustomControl.Create()
                        .AddScriptBlockExpressionBinding(
@"Set-StrictMode -Off
$optional = $_.required -ne 'true'
$positional = (($_.position -ne $()) -and ($_.position -ne '') -and ($_.position -notmatch 'named') -and ([int]$_.position -ne $()))
$parameterValue = if ($null -ne $_.psobject.Members['ParameterValueGroup']) {
    "" {$($_.ParameterValueGroup.ParameterValue -join ' | ')}""
} elseif ($null -ne $_.psobject.Members['ParameterValue']) {
    "" <$($_.ParameterValue)>""
    ''
$(if ($optional -and $positional) { '[[-{0}]{1}] ' }
elseif ($optional)   { '[-{0}{1}] ' }
elseif ($positional) { '[-{0}]{1} ' }
else                 { '-{0}{1} ' }) -f $_.Name, $parameterValue")
            var control2 = CustomControl.Create()
            var MamlExampleControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Title")
                        .AddPropertyExpressionBinding(@"Introduction", enumerateCollection: true, customControl: control2)
                        .AddPropertyExpressionBinding(@"Code", enumerateCollection: true)
                        .AddPropertyExpressionBinding(@"results")
                        .AddPropertyExpressionBinding(@"remarks", enumerateCollection: true, customControl: MamlShortDescriptionControl)
                null, //MamlParameterValueGroupControl,
                MamlParameterControl,
                MamlTypeControl,
                MamlParameterValueControl,
                MamlPossibleValueControl,
                MamlShortDescriptionControl,
                MamlDescriptionControl,
                MamlExampleControl,
                MamlFieldCustomControl,
                MamlMethodCustomControl,
                RelatedLinksHelpInfoControl
                "ExtendedCmdletHelpInfo",
                ViewsOf_ExtendedCmdletHelpInfo(sharedControls));
                "ExtendedCmdletHelpInfo#DetailedView",
                ViewsOf_ExtendedCmdletHelpInfo_DetailedView(sharedControls));
                "ExtendedCmdletHelpInfo#FullView",
                ViewsOf_ExtendedCmdletHelpInfo_FullView(sharedControls));
                "ExtendedCmdletHelpInfo#ExamplesView",
                ViewsOf_ExtendedCmdletHelpInfo_ExamplesView());
                "ExtendedCmdletHelpInfo#parameter",
                ViewsOf_ExtendedCmdletHelpInfo_parameter(sharedControls));
                "System.Management.Automation.VerboseRecord",
                ViewsOf_System_Management_Automation_VerboseRecord());
                "Deserialized.System.Management.Automation.VerboseRecord",
                ViewsOf_Deserialized_System_Management_Automation_VerboseRecord());
                "System.Management.Automation.DebugRecord",
                ViewsOf_System_Management_Automation_DebugRecord());
                "Deserialized.System.Management.Automation.DebugRecord",
                ViewsOf_Deserialized_System_Management_Automation_DebugRecord());
                "DscResourceHelpInfo#FullView",
                ViewsOf_DscResourceHelpInfo_FullView(sharedControls));
                "DscResourceHelpInfo#DetailedView",
                ViewsOf_DscResourceHelpInfo_DetailedView(sharedControls));
                "DscResourceHelpInfo",
                ViewsOf_DscResourceHelpInfo(sharedControls));
                "PSClassHelpInfo#FullView",
                ViewsOf_PSClassHelpInfo_FullView(sharedControls));
                "PSClassHelpInfo#DetailedView",
                ViewsOf_PSClassHelpInfo_DetailedView(sharedControls));
                "PSClassHelpInfo",
                ViewsOf_PSClassHelpInfo(sharedControls));
        private static IEnumerable<FormatViewDefinition> ViewsOf_ExtendedCmdletHelpInfo(CustomControl[] sharedControls)
            var control7 = CustomControl.Create()
                        .AddText(StringUtil.Format("[{0}]", HelpDisplayStrings.CommonParameters))
            var control5 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"name")
                        .AddPropertyExpressionBinding(@"Parameter", enumerateCollection: true, customControl: sharedControls[1])
                        .AddScriptBlockExpressionBinding(@" ", selectedByScript: "$_.CommonParameters -eq $true", customControl: control7)
            var control4 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"SyntaxItem", enumerateCollection: true, customControl: control5)
            var control3 = CustomControl.Create()
                        .AddText(HelpDisplayStrings.Name)
                            .AddPropertyExpressionBinding(@"Name")
            yield return new FormatViewDefinition("ReducedDefaultCommandHelp",
                        .AddPropertyExpressionBinding(@"Details", customControl: control3)
                        .AddText(HelpDisplayStrings.Syntax)
                            .AddPropertyExpressionBinding(@"Syntax", customControl: control4)
                        .AddText(HelpDisplayStrings.AliasesSection)
                            .AddPropertyExpressionBinding(@"Aliases")
                        .AddText(HelpDisplayStrings.RemarksSection)
                            .AddPropertyExpressionBinding(@"Remarks")
        private static IEnumerable<FormatViewDefinition> ViewsOf_ExtendedCmdletHelpInfo_DetailedView(CustomControl[] sharedControls)
            var control17 = CustomControl.Create()
                        .AddText(HelpDisplayStrings.None)
            var control16 = CustomControl.Create()
                        .AddText(HelpDisplayStrings.CommonParameters)
                            .AddText(HelpDisplayStrings.BaseCmdletInformation)
            var control14 = CustomControl.Create()
                        .AddText("-")
                        .AddPropertyExpressionBinding(@"ParameterValue", customControl: sharedControls[3])
            var control13 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Parameter", enumerateCollection: true, customControl: control14)
            var control12 = CustomControl.Create()
                        .AddText("[")
                        .AddText("]")
            var control10 = CustomControl.Create()
                        .AddScriptBlockExpressionBinding(@" ", selectedByScript: "$_.CommonParameters -eq $true", customControl: control12)
            var control9 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"SyntaxItem", enumerateCollection: true, customControl: control10)
            var control8 = CustomControl.Create()
            yield return new FormatViewDefinition("ReducedVerboseCommandHelp",
                        .AddPropertyExpressionBinding(@"Details", customControl: control8)
                            .AddPropertyExpressionBinding(@"Syntax", customControl: control9)
                        .AddText(HelpDisplayStrings.Parameters)
                            .AddPropertyExpressionBinding(@"Parameters", customControl: control13)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: "$_.CommonParameters -eq $true", customControl: control16)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: "($_.CommonParameters -eq $false) -and ($_.parameters.parameter.count -eq 0)", customControl: control17)
        private static IEnumerable<FormatViewDefinition> ViewsOf_ExtendedCmdletHelpInfo_FullView(CustomControl[] sharedControls)
            var control35 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"type", customControl: sharedControls[2])
            var control34 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"ReturnValue", enumerateCollection: true, customControl: control35)
            var control33 = CustomControl.Create()
            var control32 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"InputType", enumerateCollection: true, customControl: control33)
            var control31 = CustomControl.Create()
            var control30 = CustomControl.Create()
            var control28 = CustomControl.Create()
                        .AddText(HelpDisplayStrings.NamedParameter)
            var control27 = CustomControl.Create()
                        .AddText(HelpDisplayStrings.FalseShort)
            var control26 = CustomControl.Create()
                        .AddText(HelpDisplayStrings.TrueShort)
            var control25 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"possibleValues", customControl: sharedControls[4])
            var control24 = CustomControl.Create()
                            .AddPropertyExpressionBinding(@"description", selectedByScript: "$_.description -ne $()")
                            .AddCustomControlExpressionBinding(control25, selectedByScript: "$_.possibleValues -ne $()")
                            .AddText(HelpDisplayStrings.ParameterRequired)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: @"$_.required.ToLower().Equals(""true"")", customControl: control26)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: @"$_.required.ToLower().Equals(""false"")", customControl: control27)
                            .AddText(HelpDisplayStrings.ParameterPosition)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: @"($_.position -eq  $()) -or ($_.position -eq """")", customControl: control28)
                            .AddScriptBlockExpressionBinding(@"$_.position", selectedByScript: "$_.position  -ne  $()")
                            .AddText(HelpDisplayStrings.AcceptsPipelineInput)
                            .AddPropertyExpressionBinding(@"pipelineInput")
                            .AddText(HelpDisplayStrings.ParameterSetName)
                            .AddPropertyExpressionBinding(@"parameterSetName")
                            .AddText(HelpDisplayStrings.ParameterAliases)
                            .AddPropertyExpressionBinding(@"aliases")
                            .AddText(HelpDisplayStrings.ParameterIsDynamic)
                            .AddPropertyExpressionBinding(@"isDynamic")
                            .AddText(HelpDisplayStrings.AcceptsWildCardCharacters)
                            .AddPropertyExpressionBinding(@"globbing")
            var control23 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Parameter", enumerateCollection: true, customControl: control24)
            var control22 = CustomControl.Create()
            var control20 = CustomControl.Create()
                        .AddScriptBlockExpressionBinding(@" ", selectedByScript: "$_.CommonParameters -eq $true", customControl: control22)
            var control19 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"SyntaxItem", enumerateCollection: true, customControl: control20)
            var control18 = CustomControl.Create()
            yield return new FormatViewDefinition("ReducedFullCommandHelp",
                        .AddPropertyExpressionBinding(@"Details", customControl: control18)
                            .AddPropertyExpressionBinding(@"Syntax", customControl: control19)
                            .AddPropertyExpressionBinding(@"Parameters", customControl: control23)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: "$_.CommonParameters -eq $true", customControl: control30)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: "($_.CommonParameters -eq $false) -and ($_.parameters.parameter.count -eq 0)", customControl: control31)
                        .AddText(HelpDisplayStrings.InputType)
                            .AddPropertyExpressionBinding(@"InputTypes", customControl: control32)
                        .AddText(HelpDisplayStrings.ReturnType)
                            .AddPropertyExpressionBinding(@"ReturnValues", customControl: control34)
        private static IEnumerable<FormatViewDefinition> ViewsOf_ExtendedCmdletHelpInfo_ExamplesView()
            var control36 = CustomControl.Create()
            yield return new FormatViewDefinition("ReducedExamplesCommandHelp",
                        .AddPropertyExpressionBinding(@"Details", customControl: control36)
        private static IEnumerable<FormatViewDefinition> ViewsOf_ExtendedCmdletHelpInfo_parameter(CustomControl[] sharedControls)
            var control41 = CustomControl.Create()
            var control40 = CustomControl.Create()
            var control39 = CustomControl.Create()
            var control38 = CustomControl.Create()
            var control37 = CustomControl.Create()
                            .AddCustomControlExpressionBinding(control38, selectedByScript: "$_.possibleValues -ne $()")
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: @"$_.required.ToLower().Equals(""true"")", customControl: control39)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: @"$_.required.ToLower().Equals(""false"")", customControl: control40)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: @"($_.position -eq  $()) -or ($_.position -eq """")", customControl: control41)
            yield return new FormatViewDefinition("ReducedCommandHelpParameterView",
                        .AddCustomControlExpressionBinding(control37, enumerateCollection: true)
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_VerboseRecord()
            yield return new FormatViewDefinition("VerboseRecord",
                CustomControl.Create(outOfBand: true)
                        .AddPropertyExpressionBinding(@"Message")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Deserialized_System_Management_Automation_VerboseRecord()
            yield return new FormatViewDefinition("DeserializedVerboseRecord",
                        .AddPropertyExpressionBinding(@"InformationalRecord_Message")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_DebugRecord()
            yield return new FormatViewDefinition("DebugRecord",
        private static IEnumerable<FormatViewDefinition> ViewsOf_Deserialized_System_Management_Automation_DebugRecord()
            yield return new FormatViewDefinition("DeserializedDebugRecord",
        private static IEnumerable<FormatViewDefinition> ViewsOf_DscResourceHelpInfo_FullView(CustomControl[] sharedControls)
            var control50 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"linkText")
                        .AddScriptBlockExpressionBinding(@""" """, selectedByScript: "$_.linkText.Length -ne 0")
                        .AddPropertyExpressionBinding(@"uri")
            var control49 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"navigationLink", enumerateCollection: true, customControl: control50)
            var control48 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Example", enumerateCollection: true, customControl: sharedControls[7])
            var control47 = CustomControl.Create()
            var control46 = CustomControl.Create()
            var control45 = CustomControl.Create()
            var control44 = CustomControl.Create()
                            .AddPropertyExpressionBinding(@"Description", enumerateCollection: true, customControl: sharedControls[6])
                            .AddCustomControlExpressionBinding(control45, selectedByScript: "$_.possibleValues -ne $()")
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: @"$_.required.ToLower().Equals(""true"")", customControl: control46)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: @"$_.required.ToLower().Equals(""false"")", customControl: control47)
            var control43 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Parameter", enumerateCollection: true, customControl: control44)
            var control42 = CustomControl.Create()
                        .AddText(HelpDisplayStrings.Synopsis)
                            .AddPropertyExpressionBinding(@"Description", enumerateCollection: true, customControl: sharedControls[5])
            yield return new FormatViewDefinition("DscResourceHelp",
                        .AddPropertyExpressionBinding(@"Details", customControl: control42)
                        .AddText(HelpDisplayStrings.DetailedDescription)
                            .AddPropertyExpressionBinding(@"description", enumerateCollection: true, customControl: sharedControls[6])
                        .AddText(HelpDisplayStrings.Properties)
                            .AddPropertyExpressionBinding(@"Properties", customControl: control43)
                        .AddText(HelpDisplayStrings.Examples)
                            .AddPropertyExpressionBinding(@"Examples", customControl: control48)
                        .AddText(HelpDisplayStrings.RelatedLinks)
                            .AddPropertyExpressionBinding(@"relatedLinks", customControl: control49)
                            .AddText(HelpDisplayStrings.ExampleHelpInfo)
                            .AddText(@"""")
                            .AddScriptBlockExpressionBinding(@"""Get-Help "" + $_.Details.Name + "" -Examples""")
                            .AddText(HelpDisplayStrings.VerboseHelpInfo)
                            .AddScriptBlockExpressionBinding(@"""Get-Help "" + $_.Details.Name + "" -Detailed""")
                            .AddText(HelpDisplayStrings.FullHelpInfo)
                            .AddScriptBlockExpressionBinding(@"""Get-Help "" + $_.Details.Name + "" -Full""")
                            .AddCustomControlExpressionBinding(sharedControls[10])
        private static IEnumerable<FormatViewDefinition> ViewsOf_DscResourceHelpInfo_DetailedView(CustomControl[] sharedControls)
            var control56 = CustomControl.Create()
            var control55 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"navigationLink", enumerateCollection: true, customControl: control56)
            var control54 = CustomControl.Create()
            var control53 = CustomControl.Create()
            var control52 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Parameter", enumerateCollection: true, customControl: control53)
            var control51 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Details", customControl: control51)
                            .AddPropertyExpressionBinding(@"Properties", customControl: control52)
                            .AddPropertyExpressionBinding(@"Examples", customControl: control54)
                            .AddPropertyExpressionBinding(@"relatedLinks", customControl: control55)
        private static IEnumerable<FormatViewDefinition> ViewsOf_DscResourceHelpInfo(CustomControl[] sharedControls)
            var control59 = CustomControl.Create()
            var control58 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"navigationLink", enumerateCollection: true, customControl: control59)
            var control57 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Details", customControl: control57)
                            .AddPropertyExpressionBinding(@"relatedLinks", customControl: control58)
        private static IEnumerable<FormatViewDefinition> ViewsOf_PSClassHelpInfo_FullView(CustomControl[] sharedControls)
            var control68 = CustomControl.Create()
            var control67 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"navigationLink", enumerateCollection: true, customControl: control68)
            var control66 = CustomControl.Create()
            var control65 = CustomControl.Create()
                        .AddCustomControlExpressionBinding(sharedControls[9])
            var control64 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"member", enumerateCollection: true, customControl: control65)
            var control63 = CustomControl.Create()
                        .AddCustomControlExpressionBinding(sharedControls[8])
            var control62 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"member", enumerateCollection: true, customControl: control63)
            var control61 = CustomControl.Create()
                            .AddPropertyExpressionBinding(@"Introduction", enumerateCollection: true, customControl: sharedControls[5])
                            .AddPropertyExpressionBinding(@"members", customControl: control62)
                        .AddText(HelpDisplayStrings.Methods)
                            .AddPropertyExpressionBinding(@"members", customControl: control64)
                            .AddPropertyExpressionBinding(@"Examples", customControl: control66)
                            .AddPropertyExpressionBinding(@"relatedLinks", customControl: control67)
            yield return new FormatViewDefinition("PSClassHelp",
                        .AddCustomControlExpressionBinding(control61)
        private static IEnumerable<FormatViewDefinition> ViewsOf_PSClassHelpInfo_DetailedView(CustomControl[] sharedControls)
            var control77 = CustomControl.Create()
            var control76 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"navigationLink", enumerateCollection: true, customControl: control77)
            var control75 = CustomControl.Create()
            var control74 = CustomControl.Create()
            var control73 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"member", enumerateCollection: true, customControl: control74)
            var control72 = CustomControl.Create()
            var control71 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"member", enumerateCollection: true, customControl: control72)
            var control70 = CustomControl.Create()
                            .AddPropertyExpressionBinding(@"members", customControl: control71)
                            .AddPropertyExpressionBinding(@"members", customControl: control73)
                            .AddPropertyExpressionBinding(@"Examples", customControl: control75)
                            .AddPropertyExpressionBinding(@"relatedLinks", customControl: control76)
                        .AddCustomControlExpressionBinding(control70)
        private static IEnumerable<FormatViewDefinition> ViewsOf_PSClassHelpInfo(CustomControl[] sharedControls)
            var control81 = CustomControl.Create()
            var control80 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"navigationLink", enumerateCollection: true, customControl: control81)
            var control79 = CustomControl.Create()
                        .AddCustomControlExpressionBinding(control79)
                            .AddPropertyExpressionBinding(@"relatedLinks", customControl: control80)
                            .AddScriptBlockExpressionBinding(@"""Get-Help "" + $_.Name + "" -Examples""")
                            .AddScriptBlockExpressionBinding(@"""Get-Help "" + $_.Name + "" -Detailed""")
                            .AddScriptBlockExpressionBinding(@"""Get-Help "" + $_.Name + "" -Full""")
