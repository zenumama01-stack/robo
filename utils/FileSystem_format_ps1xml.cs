    internal sealed class FileSystem_Format_Ps1Xml
            var FileSystemTypes_GroupingFormat = CustomControl.Create()
                                                  $_.PSParentPath.Replace(""Microsoft.PowerShell.Core\FileSystem::"", """")
                FileSystemTypes_GroupingFormat
            var td1 = new ExtendedTypeDefinition(
                "System.IO.DirectoryInfo",
                ViewsOf_FileSystemTypes(sharedControls));
            td1.TypeNames.Add("System.IO.FileInfo");
            yield return td1;
                "System.Security.AccessControl.FileSystemSecurity",
                ViewsOf_System_Security_AccessControl_FileSystemSecurity(sharedControls));
                "Microsoft.PowerShell.Commands.AlternateStreamData",
                ViewsOf_Microsoft_PowerShell_Commands_AlternateStreamData());
        private static IEnumerable<FormatViewDefinition> ViewsOf_FileSystemTypes(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("childrenWithUnixStat",
                    .GroupByProperty("PSParentPath", customControl: sharedControls[0])
                    .AddHeader(Alignment.Left, label: "UnixMode", width: 10)
                    .AddHeader(Alignment.Right, label: "User", width: 10)
                    .AddHeader(Alignment.Left, label: "Group", width: 10)
                    .AddHeader(
                        Alignment.Right,
                        label: "LastWriteTime",
                        width: String.Format(CultureInfo.CurrentCulture, "{0:d} {0:HH}:{0:mm}", CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).Length)
                    .AddHeader(Alignment.Right, label: "Size", width: 12)
                    .AddHeader(Alignment.Left, label: "Name")
                        .AddPropertyColumn("UnixMode")
                        .AddPropertyColumn("Group")
                        .AddScriptBlockColumn(scriptBlock: @"'{0:d} {0:HH}:{0:mm}' -f $_.LastWriteTime")
                        .AddPropertyColumn("NameString")
            yield return new FormatViewDefinition("children",
                    .AddHeader(Alignment.Left, label: "Mode", width: 7)
                    .AddHeader(Alignment.Right, label: "LastWriteTime", width: 26)
                    .AddHeader(Alignment.Right, label: "Length", width: 14)
                        .AddPropertyColumn("ModeWithoutHardLink")
                        .AddPropertyColumn("LastWriteTimeString")
                        .AddPropertyColumn("LengthString")
            yield return new FormatViewDefinition("childrenWithHardlink",
                        .AddPropertyColumn("Mode")
                    .StartEntry(entrySelectedByType: new[] { "System.IO.FileInfo" })
                        .AddItemProperty("LengthString", label: "Length")
                        .AddItemProperty(@"CreationTime")
                        .AddItemProperty(@"LastWriteTime")
                        .AddItemProperty(@"LastAccessTime")
                        .AddItemProperty(@"Mode")
                        .AddItemProperty(@"LinkType")
                        .AddItemProperty(@"Target")
                        .AddItemProperty(@"VersionInfo")
                    .AddPropertyEntry("Name")
                    .AddPropertyEntry("Name", format: "[{0}]", entrySelectedByType: new[] { "System.IO.DirectoryInfo" })
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Security_AccessControl_FileSystemSecurity(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("FileSecurityTable",
                                    split-path $_.Path -leaf
                                    $_.AccessToString
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_AlternateStreamData()
            yield return new FormatViewDefinition("FileSystemStream",
                    .GroupByProperty("Filename")
                    .AddHeader(Alignment.Left, width: 20)
                        .AddPropertyColumn("Stream")
                        .AddPropertyColumn("Length")
