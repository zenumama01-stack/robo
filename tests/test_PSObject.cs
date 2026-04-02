    public static class PSObjectTests
        public static void TestEmptyObjectHasNoProperty()
            var pso = new PSObject();
            var actual = pso.GetFirstPropertyOrDefault(name => true);
            Assert.Null(actual);
        public static void TestWrappedDateTimeHasReflectedMember()
            var pso = new PSObject(DateTime.Now);
            var member = pso.GetFirstPropertyOrDefault(name => name == "DayOfWeek");
            Assert.NotNull(member);
            Assert.Equal("DayOfWeek", member.Name);
        public static void TestAdaptedMember()
            pso.Members.Add(new PSNoteProperty("NewMember", "AValue"));
            var member = pso.GetFirstPropertyOrDefault(name => name == "NewMember");
            Assert.Equal("NewMember", member.Name);
        public static void TestShadowedMember()
            pso.Members.Add(new PSNoteProperty("DayOfWeek", "AValue"));
            Assert.Equal("AValue", member.Value);
        public static void TestMemberSetIsNotProperty()
            var psNoteProperty = new PSNoteProperty("NewMember", "AValue");
            pso.Members.Add(psNoteProperty);
            pso.Members.Add(new PSMemberSet("NewMemberSet", new[] { psNoteProperty }));
            var member = pso.GetFirstPropertyOrDefault(name => name == "NewMemberSet");
            Assert.Null(member);
        public static void TestMemberSet()
            var member = pso.Members.FirstOrDefault(name => name == "NewMemberSet");
            Assert.Equal("NewMemberSet", member.Name);
        public static void TextXmlElementMember()
            var doc = new XmlDocument();
            var root = doc.CreateElement("root");
            doc.AppendChild(root);
            var firstChild = doc.CreateElement("elem1");
            root.AppendChild(firstChild);
            root.InsertAfter(doc.CreateElement("elem2"), firstChild);
            var pso = new PSObject(root);
            var member = pso.GetFirstPropertyOrDefault(name => name.StartsWith("elem"));
            Assert.Equal("elem1", member.Name);
        public static void TextXmlAttributeMember()
            root.SetAttribute("attr", "value");
            root.AppendChild(doc.CreateElement("elem"));
            var member = pso.GetFirstPropertyOrDefault(name => name.StartsWith("attr"));
            Assert.Equal("attr", member.Name);
        public static void TestCimInstanceProperty()
            iss.Commands.Add(new SessionStateCmdletEntry("Get-CimInstance", typeof(Microsoft.Management.Infrastructure.CimCmdlets.GetCimInstanceCommand), null));
            using (var ps = PowerShell.Create(iss))
                ps.AddCommand("Get-CimInstance").AddParameter("ClassName", "Win32_BIOS");
                var res = ps.Invoke().FirstOrDefault();
                Assert.NotNull(res);
                var member = res.GetFirstPropertyOrDefault(name => name == "Name");
