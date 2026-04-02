    internal sealed class Registry_Format_Ps1Xml
            var Registry_GroupingFormat = CustomControl.Create()
                            .AddText("    Hive: ")
                            .AddScriptBlockExpressionBinding(@"$_.PSParentPath.Replace(""Microsoft.PowerShell.Core\Registry::"", """")")
                Registry_GroupingFormat
                "Microsoft.PowerShell.Commands.Internal.TransactedRegistryKey",
                ViewsOf_Microsoft_PowerShell_Commands_Internal_TransactedRegistryKey_Microsoft_Win32_RegistryKey_System_Management_Automation_TreatAs_RegistryValue(sharedControls));
            td1.TypeNames.Add("Microsoft.Win32.RegistryKey");
            td1.TypeNames.Add("System.Management.Automation.TreatAs.RegistryValue");
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_Internal_TransactedRegistryKey_Microsoft_Win32_RegistryKey_System_Management_Automation_TreatAs_RegistryValue(CustomControl[] sharedControls)
                    .AddHeader(label: "Name", width: 30)
                        .AddPropertyColumn("PSChildName")
                                  $result = (Get-ItemProperty -LiteralPath $_.PSPath |
                                      Select * -Exclude PSPath,PSParentPath,PSChildName,PSDrive,PsProvider |
                                      Format-List | Out-String | Sort).Trim()
                                  $result = $result.Substring(0, [Math]::Min($result.Length, 5000) )
                                  if($result.Length -eq 5000) { $result += ""`u{2026}"" }
