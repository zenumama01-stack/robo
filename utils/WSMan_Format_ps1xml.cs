    internal sealed class WSMan_Format_Ps1Xml
                "System.Xml.XmlElement#http://schemas.dmtf.org/wbem/wsman/identity/1/wsmanidentity.xsd#IdentifyResponse",
                ViewsOf_System_Xml_XmlElement_http___schemas_dmtf_org_wbem_wsman_identity_1_wsmanidentity_xsd_IdentifyResponse());
                "Microsoft.WSMan.Management.WSManConfigElement",
                ViewsOf_Microsoft_WSMan_Management_WSManConfigElement());
                "Microsoft.WSMan.Management.WSManConfigContainerElement",
                ViewsOf_Microsoft_WSMan_Management_WSManConfigContainerElement());
                "Microsoft.WSMan.Management.WSManConfigLeafElement",
                ViewsOf_Microsoft_WSMan_Management_WSManConfigLeafElement());
                "Microsoft.WSMan.Management.WSManConfigLeafElement#InitParams",
                ViewsOf_Microsoft_WSMan_Management_WSManConfigLeafElement_InitParams());
                "Microsoft.WSMan.Management.WSManConfigContainerElement#ComputerLevel",
                ViewsOf_Microsoft_WSMan_Management_WSManConfigContainerElement_ComputerLevel());
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Xml_XmlElement_http___schemas_dmtf_org_wbem_wsman_identity_1_wsmanidentity_xsd_IdentifyResponse()
            yield return new FormatViewDefinition("System.Xml.XmlElement#http://schemas.dmtf.org/wbem/wsman/identity/1/wsmanidentity.xsd#IdentifyResponse",
                        .AddItemProperty(@"wsmid")
                        .AddItemProperty(@"ProtocolVersion")
                        .AddItemProperty(@"ProductVendor")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_WSMan_Management_WSManConfigElement()
            yield return new FormatViewDefinition("Microsoft.WSMan.Management.WSManConfigElement",
                    .GroupByProperty("PSParentPath", label: "WSManConfig")
                    .AddHeader(label: "Type", width: 15)
                        .AddPropertyColumn("TypeNameOfElement")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_WSMan_Management_WSManConfigContainerElement()
            yield return new FormatViewDefinition("Microsoft.WSMan.Management.WSManConfigContainerElement",
                    .AddHeader(label: "Keys", width: 35)
                        .AddPropertyColumn("Keys")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_WSMan_Management_WSManConfigLeafElement()
            yield return new FormatViewDefinition("Microsoft.WSMan.Management.WSManConfigLeafElement",
                    .AddHeader(label: "SourceOfValue", width: 15)
                    .AddHeader(label: "Value")
                        .AddPropertyColumn("SourceOfValue")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_WSMan_Management_WSManConfigLeafElement_InitParams()
            yield return new FormatViewDefinition("Microsoft.WSMan.Management.WSManConfigLeafElement#InitParams",
                    .AddHeader(label: "ParamName", width: 30)
                    .AddHeader(label: "ParamValue", width: 20)
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_WSMan_Management_WSManConfigContainerElement_ComputerLevel()
            yield return new FormatViewDefinition("Microsoft.WSMan.Management.WSManConfigContainerElement#ComputerLevel",
                    .AddHeader(label: "ComputerName", width: 45)
                    .AddHeader(label: "Type", width: 20)
