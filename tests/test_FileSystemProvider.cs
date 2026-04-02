using Microsoft.CodeAnalysis.CSharp.Syntax;
    public class FileSystemProviderTests : IDisposable
        private readonly string testPath;
        private readonly string testContent;
        public FileSystemProviderTests()
            testPath = Path.GetTempFileName();
            testContent = "test content!";
            if (File.Exists(testPath))
                File.Delete(testPath);
            File.AppendAllText(testPath, testContent);
        private static ExecutionContext GetExecutionContext()
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            PSHost hostInterface = new DefaultHost(currentCulture, currentCulture);
            AutomationEngine engine = new AutomationEngine(hostInterface, iss);
            ExecutionContext executionContext = new ExecutionContext(engine, hostInterface, iss);
            return executionContext;
        private static ProviderInfo GetProvider()
            ExecutionContext executionContext = GetExecutionContext();
            SessionStateInternal sessionState = new SessionStateInternal(executionContext);
            SessionStateProviderEntry providerEntry = new SessionStateProviderEntry("FileSystem", typeof(FileSystemProvider), null);
            sessionState.AddSessionStateEntry(providerEntry);
            ProviderInfo matchingProvider = sessionState.ProviderList.ToList()[0];
            return matchingProvider;
        public void TestCreateJunctionFails()
                Assert.False(InternalSymbolicLinkLinkCodeMethods.CreateJunction(string.Empty, string.Empty));
                Assert.Throws<System.ArgumentException>(delegate { InternalSymbolicLinkLinkCodeMethods.CreateJunction(string.Empty, string.Empty); });
        public void TestGetHelpMaml()
            FileSystemProvider fileSystemProvider = new FileSystemProvider();
            Assert.Equal(fileSystemProvider.GetHelpMaml(string.Empty, string.Empty), string.Empty);
            Assert.Equal(fileSystemProvider.GetHelpMaml("helpItemName", string.Empty), string.Empty);
            Assert.Equal(fileSystemProvider.GetHelpMaml(string.Empty, "path"), string.Empty);
        public void TestMode()
            Assert.Equal(FileSystemProvider.Mode(null), string.Empty);
            FileSystemInfo directoryObject = null;
            FileSystemInfo fileObject = null;
            FileSystemInfo executableObject = null;
                directoryObject = new DirectoryInfo(@"/");
                fileObject = new FileInfo(@"/etc/hosts");
                executableObject = new FileInfo(@"/bin/echo");
                directoryObject = new DirectoryInfo(System.Environment.CurrentDirectory);
                fileObject = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                executableObject = new FileInfo(Environment.ProcessPath);
            Assert.Equal("d----", FileSystemProvider.Mode(PSObject.AsPSObject(directoryObject)).Replace("r", "-"));
            Assert.Equal("-----", FileSystemProvider.Mode(PSObject.AsPSObject(fileObject)).Replace("r", "-").Replace("a", "-"));
            Assert.Equal("-----", FileSystemProvider.Mode(PSObject.AsPSObject(executableObject)).Replace("r", "-").Replace("a", "-"));
        public void TestGetProperty()
            ProviderInfo providerInfoToSet = GetProvider();
            fileSystemProvider.SetProviderInformation(providerInfoToSet);
            fileSystemProvider.Context = new CmdletProviderContext(GetExecutionContext());
            pso.AddOrSetProperty("IsReadOnly", false);
            fileSystemProvider.SetProperty(testPath, pso);
            fileSystemProvider.GetProperty(testPath, new Collection<string>() { "IsReadOnly" });
            FileInfo fileSystemObject1 = new FileInfo(testPath);
            PSObject psobject1 = PSObject.AsPSObject(fileSystemObject1);
            PSPropertyInfo property = psobject1.Properties["IsReadOnly"];
            Assert.False((bool)property.Value);
        public void TestSetProperty()
            fileSystemProvider.GetProperty(testPath, new Collection<string>() { "Name" });
            PSPropertyInfo property = psobject1.Properties["FullName"];
            Assert.Equal(testPath, property.Value);
        public void TestClearProperty()
            fileSystemProvider.ClearProperty(testPath, new Collection<string>() { "Attributes" });
        public void TestGetContentReader()
            IContentReader contentReader = fileSystemProvider.GetContentReader(testPath);
            Assert.Equal(contentReader.Read(1)[0], testContent);
            contentReader.Close();
        public void TestGetContentWriter()
            IContentWriter contentWriter = fileSystemProvider.GetContentWriter(testPath);
            contentWriter.Write(new List<string>() { "contentWriterTestContent" });
            contentWriter.Close();
            Assert.Equal(File.ReadAllText(testPath), testContent + @"contentWriterTestContent" + System.Environment.NewLine);
        public void TestClearContent()
            fileSystemProvider.ClearContent(testPath);
            Assert.Empty(File.ReadAllText(testPath));
