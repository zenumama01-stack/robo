    internal sealed class Certificate_Format_Ps1Xml
        internal static IEnumerable<ExtendedTypeDefinition> GetFormatData()
            var SignatureTypes_GroupingFormat = CustomControl.Create()
                    .StartEntry()
                        .StartFrame()
                            .AddText(FileSystemProviderStrings.DirectoryDisplayGrouping)
                            .AddScriptBlockExpressionBinding(@"split-path $_.Path")
                        .EndFrame()
                    .EndEntry()
                .EndControl();
            var sharedControls = new CustomControl[] {
                SignatureTypes_GroupingFormat
            yield return new ExtendedTypeDefinition(
                "System.Security.Cryptography.X509Certificates.X509Certificate2",
                ViewsOf_System_Security_Cryptography_X509Certificates_X509Certificate2());
            var td2 = new ExtendedTypeDefinition(
                "Microsoft.PowerShell.Commands.X509StoreLocation",
                ViewsOf_CertificateProviderTypes());
            td2.TypeNames.Add("System.Security.Cryptography.X509Certificates.X509Certificate2");
            td2.TypeNames.Add("System.Security.Cryptography.X509Certificates.X509Store");
            yield return td2;
                "System.Management.Automation.Signature",
                ViewsOf_System_Management_Automation_Signature(sharedControls));
                "System.Security.Cryptography.X509Certificates.X509CertificateEx",
                ViewsOf_System_Security_Cryptography_X509Certificates_X509CertificateEx());
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Security_Cryptography_X509Certificates_X509Certificate2()
            yield return new FormatViewDefinition("ThumbprintTable",
                TableControl.Create()
                    .GroupByProperty("PSParentPath")
                    .AddHeader(width: 41)
                    .AddHeader(width: 20)
                    .AddHeader(label: "EnhancedKeyUsageList")
                    .StartRowDefinition()
                        .AddPropertyColumn("Thumbprint")
                        .AddPropertyColumn("Subject")
                        .AddScriptBlockColumn("$_.EnhancedKeyUsageList.FriendlyName")
                    .EndRowDefinition()
                .EndTable());
        private static IEnumerable<FormatViewDefinition> ViewsOf_CertificateProviderTypes()
            yield return new FormatViewDefinition("ThumbprintList",
                ListControl.Create()
                    .StartEntry(entrySelectedByType: new[] { "Microsoft.PowerShell.Commands.X509StoreLocation" })
                        .AddItemProperty(@"Location")
                        .AddItemProperty(@"StoreNames")
                    .StartEntry(entrySelectedByType: new[] { "System.Security.Cryptography.X509Certificates.X509Store" })
                        .AddItemProperty(@"Name")
                        .AddItemScriptBlock(@"$_.SubjectName.Name", label: "Subject")
                        .AddItemScriptBlock(@"$_.IssuerName.Name", label: "Issuer")
                        .AddItemProperty(@"Thumbprint")
                        .AddItemProperty(@"FriendlyName")
                        .AddItemProperty(@"NotBefore")
                        .AddItemProperty(@"NotAfter")
                        .AddItemProperty(@"Extensions")
                .EndList());
            yield return new FormatViewDefinition("ThumbprintWide",
                WideControl.Create()
                    .AddPropertyEntry("Thumbprint")
                .EndWideControl());
            yield return new FormatViewDefinition("PathOnly",
                        .AddItemProperty(@"PSPath")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_Signature(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("PSThumbprintTable",
                    .GroupByScriptBlock("split-path $_.Path", customControl: sharedControls[0])
                    .AddHeader(label: "SignerCertificate", width: 41)
                    .AddHeader()
                    .AddHeader(label: "Path")
                        .AddScriptBlockColumn("$_.SignerCertificate.Thumbprint")
                        .AddPropertyColumn("Status")
                        .AddPropertyColumn("StatusMessage")
                        .AddScriptBlockColumn("split-path $_.Path -leaf")
            yield return new FormatViewDefinition("PSThumbprintWide",
                    .AddScriptBlockEntry(@"""$(split-path $_.Path -leaf): $($_.Status)""")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Security_Cryptography_X509Certificates_X509CertificateEx()
            yield return new FormatViewDefinition("System.Security.Cryptography.X509Certificates.X509CertificateEx",
