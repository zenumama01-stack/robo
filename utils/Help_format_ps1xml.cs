    internal sealed class Help_Format_Ps1Xml
            var TextPropertyControl = CustomControl.Create()
@"$optional = $_.required -ne 'true'
                        .AddScriptBlockExpressionBinding(@"if ($_.required -ne 'true') { "" [<$_>]"" } else { "" <$_>"" }")
            var MamlTextItem = CustomControl.Create()
                    .StartEntry(entrySelectedByType: new[] { "MamlPreformattedTextItem" })
            var MamlAlertControl = CustomControl.Create()
            var MamlTrueFalseShortControl = CustomControl.Create()
                        .AddScriptBlockExpressionBinding(@";", selectedByScript: @"$_.Equals('true', [System.StringComparison]::OrdinalIgnoreCase)", customControl: control3)
                        .AddScriptBlockExpressionBinding(@";", selectedByScript: @"$_.Equals('false', [System.StringComparison]::OrdinalIgnoreCase)", customControl: control4)
            var control6 = CustomControl.Create()
                        .AddScriptBlockExpressionBinding("' '", selectedByScript: "$_.linkText.Length -ne 0")
            var MamlRelatedLinksControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"navigationLink", enumerateCollection: true, customControl: control6)
            var MamlDetailsControl = CustomControl.Create()
                            .AddPropertyExpressionBinding(@"Description", enumerateCollection: true, customControl: MamlShortDescriptionControl)
                        .AddPropertyExpressionBinding(@"Introduction", enumerateCollection: true, customControl: TextPropertyControl)
                            .AddPropertyExpressionBinding(@"description", enumerateCollection: true, customControl: MamlShortDescriptionControl)
            var control0 = CustomControl.Create()
                        .AddCustomControlExpressionBinding(control0, selectedByScript: "$_.uri")
                        .AddCustomControlExpressionBinding(control1, selectedByScript: "$_.description")
                        .AddPropertyExpressionBinding(@"possibleValue", enumerateCollection: true, customControl: control2)
            var MamlIndentedDescriptionControl = CustomControl.Create()
                            .AddPropertyExpressionBinding(@"description", enumerateCollection: true, customControl: MamlDescriptionControl)
                        .AddPropertyExpressionBinding(@"Parameter", enumerateCollection: true, customControl: MamlParameterControl)
                        .AddText("[" + HelpDisplayStrings.CommonParameters + "]")
                        .AddNewline(2)
            var MamlSyntaxControl = CustomControl.Create()
            var ExamplesControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Example", enumerateCollection: true, customControl: MamlExampleControl)
            var MamlTypeWithDescriptionControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"type", customControl: MamlTypeControl)
            var ErrorControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"errorId")
                        .AddText(HelpDisplayStrings.Category)
                        .AddPropertyExpressionBinding(@"category")
                        .AddText(")")
                        .AddText(HelpDisplayStrings.TypeColon)
                        .AddText(HelpDisplayStrings.TargetObjectTypeColon)
                        .AddPropertyExpressionBinding(@"targetObjectType", customControl: MamlTypeControl)
                        .AddText(HelpDisplayStrings.SuggestedActionColon)
                        .AddPropertyExpressionBinding(@"recommendedAction", enumerateCollection: true, customControl: MamlShortDescriptionControl)
            var MamlPossibleValuesControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"possibleValues", customControl: MamlPossibleValueControl)
            var MamlIndentedSyntaxControl = CustomControl.Create()
                            .AddPropertyExpressionBinding(@"Syntax", customControl: MamlSyntaxControl)
            var MamlFullParameterControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"ParameterValue", customControl: MamlParameterValueControl)
                            .AddPropertyExpressionBinding(@"Description", enumerateCollection: true, customControl: MamlDescriptionControl)
                            .AddCustomControlExpressionBinding(MamlPossibleValuesControl, selectedByScript: "$_.possibleValues -ne $()")
                            .AddPropertyExpressionBinding(@"required", customControl: MamlTrueFalseShortControl)
                            .AddScriptBlockExpressionBinding(@" ", selectedByScript: @"($_.position -eq  $()) -or ($_.position -eq '')", customControl: control7)
                            .AddScriptBlockExpressionBinding(@"$_.position", selectedByScript: "$_.position -ne $()")
                            .AddText(HelpDisplayStrings.ParameterDefaultValue)
                            .AddPropertyExpressionBinding(@"defaultValue")
                            .AddPropertyExpressionBinding(@"globbing", customControl: MamlTrueFalseShortControl)
                        .AddPropertyExpressionBinding(@"Parameter", enumerateCollection: true, customControl: MamlFullParameterControl)
            var zzz = CustomControl.Create()
                        .AddCustomControlExpressionBinding(control8)
                TextPropertyControl,
                MamlDetailsControl,
                MamlIndentedDescriptionControl,
                ExamplesControl,
                MamlTextItem,
                MamlAlertControl,
                MamlPossibleValuesControl,
                MamlTrueFalseShortControl,
                MamlIndentedSyntaxControl,
                MamlSyntaxControl,
                MamlTypeWithDescriptionControl,
                RelatedLinksHelpInfoControl,
                MamlRelatedLinksControl,
                ErrorControl,
                MamlFullParameterControl,
                zzz
                "HelpInfoShort",
                ViewsOf_HelpInfoShort());
                "CmdletHelpInfo",
                ViewsOf_CmdletHelpInfo());
                "MamlCommandHelpInfo",
                ViewsOf_MamlCommandHelpInfo(sharedControls));
                "MamlCommandHelpInfo#DetailedView",
                ViewsOf_MamlCommandHelpInfo_DetailedView(sharedControls));
                "MamlCommandHelpInfo#ExamplesView",
                ViewsOf_MamlCommandHelpInfo_ExamplesView(sharedControls));
                "MamlCommandHelpInfo#FullView",
                ViewsOf_MamlCommandHelpInfo_FullView(sharedControls));
                "ProviderHelpInfo",
                ViewsOf_ProviderHelpInfo(sharedControls));
                "FaqHelpInfo",
                ViewsOf_FaqHelpInfo(sharedControls));
                "GeneralHelpInfo",
                ViewsOf_GeneralHelpInfo(sharedControls));
                "GlossaryHelpInfo",
                ViewsOf_GlossaryHelpInfo(sharedControls));
                "ScriptHelpInfo",
                ViewsOf_ScriptHelpInfo(sharedControls));
                "MamlCommandHelpInfo#Examples",
                ViewsOf_MamlCommandHelpInfo_Examples(sharedControls));
                "MamlCommandHelpInfo#Example",
                ViewsOf_MamlCommandHelpInfo_Example(sharedControls));
                "MamlCommandHelpInfo#commandDetails",
                ViewsOf_MamlCommandHelpInfo_commandDetails(sharedControls));
                "MamlCommandHelpInfo#Parameters",
                ViewsOf_MamlCommandHelpInfo_Parameters(sharedControls));
                "MamlCommandHelpInfo#Parameter",
                ViewsOf_MamlCommandHelpInfo_Parameter(sharedControls));
                "MamlCommandHelpInfo#Syntax",
                ViewsOf_MamlCommandHelpInfo_Syntax(sharedControls));
            var td18 = new ExtendedTypeDefinition(
                "MamlDefinitionTextItem",
                ViewsOf_MamlDefinitionTextItem_MamlOrderedListTextItem_MamlParaTextItem_MamlUnorderedListTextItem(sharedControls));
            td18.TypeNames.Add("MamlOrderedListTextItem");
            td18.TypeNames.Add("MamlParaTextItem");
            td18.TypeNames.Add("MamlUnorderedListTextItem");
            yield return td18;
                "MamlCommandHelpInfo#inputTypes",
                ViewsOf_MamlCommandHelpInfo_inputTypes(sharedControls));
                "MamlCommandHelpInfo#nonTerminatingErrors",
                ViewsOf_MamlCommandHelpInfo_nonTerminatingErrors(sharedControls));
                "MamlCommandHelpInfo#terminatingErrors",
                ViewsOf_MamlCommandHelpInfo_terminatingErrors(sharedControls));
                "MamlCommandHelpInfo#relatedLinks",
                ViewsOf_MamlCommandHelpInfo_relatedLinks(sharedControls));
                "MamlCommandHelpInfo#returnValues",
                ViewsOf_MamlCommandHelpInfo_returnValues(sharedControls));
                "MamlCommandHelpInfo#alertSet",
                ViewsOf_MamlCommandHelpInfo_alertSet(sharedControls));
                "MamlCommandHelpInfo#details",
                ViewsOf_MamlCommandHelpInfo_details(sharedControls));
        private static IEnumerable<FormatViewDefinition> ViewsOf_HelpInfoShort()
            yield return new FormatViewDefinition("help",
                    .AddHeader(Alignment.Left, label: "Name", width: 33)
                    .AddHeader(Alignment.Left, label: "Category", width: 9)
                    .AddHeader(Alignment.Left, label: "Module", width: 25)
                        .AddPropertyColumn("Category")
                        .AddScriptBlockColumn("if ($null -ne $_.ModuleName) { $_.ModuleName } else {$_.PSSnapIn}")
                        .AddPropertyColumn("Synopsis")
        private static IEnumerable<FormatViewDefinition> ViewsOf_CmdletHelpInfo()
            yield return new FormatViewDefinition("CmdletHelp",
                            .AddPropertyExpressionBinding(@"Syntax")
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("DefaultCommandHelp",
                        .AddPropertyExpressionBinding(@"Details", customControl: sharedControls[2])
                        .AddScriptBlockExpressionBinding(@"$_", customControl: sharedControls[15])
                        .AddScriptBlockExpressionBinding(@"$_", customControl: sharedControls[3])
                            .AddPropertyExpressionBinding(@"relatedLinks", customControl: sharedControls[19])
                            .AddText(HelpDisplayStrings.ExampleHelpInfo + "\"")
                            .AddText(HelpDisplayStrings.VerboseHelpInfo + "\"")
                            .AddCustomControlExpressionBinding(sharedControls[18])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_DetailedView(CustomControl[] sharedControls)
                        .AddPropertyExpressionBinding(@"ParameterValue", customControl: sharedControls[6])
                            .AddPropertyExpressionBinding(@"Description", enumerateCollection: true, customControl: sharedControls[4])
                        .AddPropertyExpressionBinding(@"Parameter", enumerateCollection: true, customControl: control10)
            yield return new FormatViewDefinition("VerboseCommandHelp",
                            .AddPropertyExpressionBinding(@"Parameters", customControl: control9)
                            .AddPropertyExpressionBinding(@"Examples", customControl: sharedControls[7])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_ExamplesView(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("ExampleCommandHelp",
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_FullView(CustomControl[] sharedControls)
                            .AddPropertyExpressionBinding(@"title")
                            .AddPropertyExpressionBinding(@"alert", enumerateCollection: true, customControl: sharedControls[11])
            var control15 = CustomControl.Create()
                        .AddText(HelpDisplayStrings.Notes)
                        .AddText(HelpDisplayStrings.NonHyphenTerminatingErrors)
                            .AddPropertyExpressionBinding(@"nonTerminatingError", enumerateCollection: true, customControl: sharedControls[20])
                        .AddText(HelpDisplayStrings.TerminatingErrors)
                            .AddPropertyExpressionBinding(@"terminatingError", enumerateCollection: true, customControl: sharedControls[20])
                        .AddPropertyExpressionBinding(@"ReturnValue", enumerateCollection: true, customControl: sharedControls[17])
            var control11 = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"InputType", enumerateCollection: true, customControl: sharedControls[17])
            yield return new FormatViewDefinition("FullCommandHelp",
                            .AddPropertyExpressionBinding(@"Parameters", customControl: sharedControls[22])
                            .AddPropertyExpressionBinding(@"InputTypes", customControl: control11)
                            .AddPropertyExpressionBinding(@"ReturnValues", customControl: control12)
                        .AddPropertyExpressionBinding(@"terminatingErrors", selectedByScript: @"
                      (($null -ne $_.terminatingErrors) -and
                      ($null -ne $_.terminatingErrors.terminatingError))
                    ", customControl: control13)
                        .AddPropertyExpressionBinding(@"nonTerminatingErrors", selectedByScript: @"
                      (($null -ne $_.nonTerminatingErrors) -and
                      ($null -ne $_.nonTerminatingErrors.nonTerminatingError))
                    ", customControl: control14)
                        .AddPropertyExpressionBinding(@"alertSet", selectedByScript: "$null -ne $_.alertSet", customControl: control15)
                        .AddPropertyExpressionBinding(@"alertSet", enumerateCollection: true, selectedByScript: "$null -ne $_.alertSet", customControl: control16)
        private static IEnumerable<FormatViewDefinition> ViewsOf_ProviderHelpInfo(CustomControl[] sharedControls)
            var TaskExampleControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Introduction", enumerateCollection: true, customControl: sharedControls[0])
                        .AddPropertyExpressionBinding(@"remarks", enumerateCollection: true, customControl: sharedControls[10])
            var DynamicPossibleValues = CustomControl.Create()
                        .StartFrame(leftIndent: 8)
                            .AddPropertyExpressionBinding(@"Description", enumerateCollection: true, customControl: sharedControls[10])
            var TaskExamplesControl = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Example", enumerateCollection: true, customControl: TaskExampleControl)
                        .AddPropertyExpressionBinding(@"PossibleValue", enumerateCollection: true, customControl: DynamicPossibleValues)
                        .AddScriptBlockExpressionBinding(@"""<"" + $_.Name + "">""")
            var DynamicParameterControl = CustomControl.Create()
                            .AddPropertyExpressionBinding(@"Type", customControl: control17)
                            .AddPropertyExpressionBinding(@"Description")
                            .AddPropertyExpressionBinding(@"PossibleValues", customControl: control18)
                            .AddText(HelpDisplayStrings.CmdletsSupported)
                            .AddPropertyExpressionBinding(@"CmdletSupported")
            var Task = CustomControl.Create()
                            .AddText(HelpDisplayStrings.Task)
                            .AddPropertyExpressionBinding(@"Examples", enumerateCollection: true, customControl: TaskExamplesControl)
            var ProviderTasks = CustomControl.Create()
                        .AddPropertyExpressionBinding(@"Task", enumerateCollection: true, customControl: Task)
                        .AddPropertyExpressionBinding(@"DynamicParameter", enumerateCollection: true, customControl: DynamicParameterControl)
            yield return new FormatViewDefinition("ProviderHelpInfo",
                        .AddText(HelpDisplayStrings.ProviderName)
                        .AddText(HelpDisplayStrings.Drives)
                            .AddPropertyExpressionBinding(@"Drives", enumerateCollection: true, customControl: sharedControls[10])
                            .AddPropertyExpressionBinding(@"Synopsis", enumerateCollection: true)
                            .AddPropertyExpressionBinding(@"DetailedDescription", enumerateCollection: true, customControl: sharedControls[10])
                        .AddText(HelpDisplayStrings.Capabilities)
                            .AddPropertyExpressionBinding(@"Capabilities", enumerateCollection: true, customControl: sharedControls[10])
                        .AddText(HelpDisplayStrings.Tasks)
                            .AddPropertyExpressionBinding(@"Tasks", enumerateCollection: true, customControl: ProviderTasks)
                        .AddText(HelpDisplayStrings.DynamicParameters)
                            .AddPropertyExpressionBinding(@"DynamicParameters", customControl: control19)
                            .AddPropertyExpressionBinding(@"Notes")
        private static IEnumerable<FormatViewDefinition> ViewsOf_FaqHelpInfo(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("FaqHelpInfo",
                        .AddText(HelpDisplayStrings.TitleColon)
                        .AddText(HelpDisplayStrings.QuestionColon)
                        .AddPropertyExpressionBinding(@"Question")
                        .AddText(HelpDisplayStrings.Answer)
                            .AddPropertyExpressionBinding(@"Answer", enumerateCollection: true, customControl: sharedControls[10])
        private static IEnumerable<FormatViewDefinition> ViewsOf_GeneralHelpInfo(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("GeneralHelpInfo",
                            .AddPropertyExpressionBinding(@"Content", enumerateCollection: true, customControl: sharedControls[10])
        private static IEnumerable<FormatViewDefinition> ViewsOf_GlossaryHelpInfo(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("GlossaryHelpInfo",
                        .AddText(HelpDisplayStrings.TermColon)
                        .AddText(HelpDisplayStrings.DefinitionColon)
                            .AddPropertyExpressionBinding(@"Definition", enumerateCollection: true, customControl: sharedControls[10])
        private static IEnumerable<FormatViewDefinition> ViewsOf_ScriptHelpInfo(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("ScriptHelpInfo",
                        .AddText(HelpDisplayStrings.ContentColon)
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_Examples(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlCommandExamples",
                        .AddScriptBlockExpressionBinding(@"$_", customControl: sharedControls[7])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_Example(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlCommandExample",
                        .AddPropertyExpressionBinding(@"remarks", enumerateCollection: true, customControl: sharedControls[1])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_commandDetails(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlCommandDetails",
                            .AddPropertyExpressionBinding(@"commandDescription", enumerateCollection: true, customControl: sharedControls[1])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_Parameters(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlCommandParameters",
                            .AddCustomControlExpressionBinding(sharedControls[22])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_Parameter(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlCommandParameterView",
                        .AddScriptBlockExpressionBinding(@"$_", customControl: sharedControls[21])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_Syntax(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlCommandSyntax",
                        .AddScriptBlockExpressionBinding(@"$_", customControl: sharedControls[16])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlDefinitionTextItem_MamlOrderedListTextItem_MamlParaTextItem_MamlUnorderedListTextItem(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlText",
                        .AddScriptBlockExpressionBinding(@"$_", customControl: sharedControls[4])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_inputTypes(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlInputTypes",
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_nonTerminatingErrors(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlNonTerminatingErrors",
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_terminatingErrors(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlTerminatingErrors",
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_relatedLinks(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlRelatedLinks",
                        .AddScriptBlockExpressionBinding(@"$_", customControl: sharedControls[19])
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_returnValues(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlReturnTypes",
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_alertSet(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlAlertSet",
        private static IEnumerable<FormatViewDefinition> ViewsOf_MamlCommandHelpInfo_details(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("MamlDetails",
                        .AddScriptBlockExpressionBinding(@"$_", customControl: sharedControls[2])
