    [TestCaseOrderer("TestOrder.TestCaseOrdering.PriorityOrderer", "powershell-tests")]
    public class PowerShellPolicyFixture : IDisposable
        private readonly string systemWideConfigFile;
        private readonly string currentUserConfigFile;
        private readonly string systemWideConfigBackupFile;
        private readonly string currentUserConfigBackupFile;
        private readonly string systemWideConfigDirectory;
        private readonly string currentUserConfigDirectory;
        private readonly PowerShellPolicies systemWidePolicies;
        private readonly PowerShellPolicies currentUserPolicies;
        private readonly bool originalTestHookValue;
        public PowerShellPolicyFixture()
            currentUserConfigDirectory = Platform.ConfigDirectory;
            if (!Directory.Exists(currentUserConfigDirectory))
                // Create the CurrentUser config directory if it doesn't exist
                Directory.CreateDirectory(currentUserConfigDirectory);
            currentUserConfigFile = Path.Combine(currentUserConfigDirectory, ConfigFileName);
            if (File.Exists(systemWideConfigFile))
                systemWideConfigBackupFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                File.Move(systemWideConfigFile, systemWideConfigBackupFile);
            if (File.Exists(currentUserConfigFile))
                currentUserConfigBackupFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                File.Move(currentUserConfigFile, currentUserConfigBackupFile);
            var settings = new JsonSerializerSettings()
                MaxDepth = 10,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            serializer = JsonSerializer.Create(settings);
            systemWidePolicies = new PowerShellPolicies()
                ScriptExecution = new ScriptExecution() { ExecutionPolicy = "RemoteSigned", EnableScripts = true },
                ScriptBlockLogging = new ScriptBlockLogging() { EnableScriptBlockInvocationLogging = true, EnableScriptBlockLogging = false },
                ModuleLogging = new ModuleLogging() { EnableModuleLogging = false, ModuleNames = new string[] { "PSReadline", "PowerShellGet" } },
                ProtectedEventLogging = new ProtectedEventLogging() { EnableProtectedEventLogging = false, EncryptionCertificate = new string[] { "Joe" } },
                Transcription = new Transcription() { EnableInvocationHeader = true, EnableTranscripting = true, OutputDirectory = @"c:\tmp" },
                UpdatableHelp = new UpdatableHelp() { DefaultSourcePath = @"f:\temp" },
                ConsoleSessionConfiguration = new ConsoleSessionConfiguration() { EnableConsoleSessionConfiguration = true, ConsoleSessionConfigurationName = "name" }
            currentUserPolicies = new PowerShellPolicies()
                ScriptExecution = new ScriptExecution() { ExecutionPolicy = "RemoteSigned" },
                ScriptBlockLogging = new ScriptBlockLogging() { EnableScriptBlockLogging = false },
                ModuleLogging = new ModuleLogging() { EnableModuleLogging = false },
                ProtectedEventLogging = new ProtectedEventLogging() { EncryptionCertificate = new string[] { "Joe" } }
            // Set the test hook to disable policy caching
            originalTestHookValue = InternalTestHooks.BypassGroupPolicyCaching;
            InternalTestHooks.BypassGroupPolicyCaching = true;
                CleanupConfigFiles();
                if (systemWideConfigBackupFile != null)
                    File.Move(systemWideConfigBackupFile, systemWideConfigFile);
                if (currentUserConfigBackupFile != null)
                    File.Move(currentUserConfigBackupFile, currentUserConfigFile);
                InternalTestHooks.BypassGroupPolicyCaching = originalTestHookValue;
        internal PowerShellPolicies SystemWidePolicies
            get { return systemWidePolicies; }
        internal PowerShellPolicies CurrentUserPolicies
            get { return currentUserPolicies; }
        #region Compare_Policy_Settings
        internal void CompareScriptExecution(ScriptExecution a, ScriptExecution b)
                Assert.Null(b);
                Assert.Equal(a.EnableScripts, b.EnableScripts);
                Assert.Equal(a.ExecutionPolicy, b.ExecutionPolicy);
        internal void CompareScriptBlockLogging(ScriptBlockLogging a, ScriptBlockLogging b)
                Assert.Equal(a.EnableScriptBlockInvocationLogging, b.EnableScriptBlockInvocationLogging);
                Assert.Equal(a.EnableScriptBlockLogging, b.EnableScriptBlockLogging);
        internal void CompareModuleLogging(ModuleLogging a, ModuleLogging b)
                Assert.Equal(a.EnableModuleLogging, b.EnableModuleLogging);
                if (a.ModuleNames == null)
                    Assert.Null(b.ModuleNames);
                    Assert.Equal(a.ModuleNames.Length, b.ModuleNames.Length);
                    for (int i = 0; i < a.ModuleNames.Length; i++)
                        Assert.Equal(a.ModuleNames[i], b.ModuleNames[i]);
        internal void CompareProtectedEventLogging(ProtectedEventLogging a, ProtectedEventLogging b)
                Assert.Equal(a.EnableProtectedEventLogging, b.EnableProtectedEventLogging);
                if (a.EncryptionCertificate == null)
                    Assert.Null(b.EncryptionCertificate);
                    Assert.Equal(a.EncryptionCertificate.Length, b.EncryptionCertificate.Length);
                    for (int i = 0; i < a.EncryptionCertificate.Length; i++)
                        Assert.Equal(a.EncryptionCertificate[i], b.EncryptionCertificate[i]);
        internal void CompareTranscription(Transcription a, Transcription b)
                Assert.Equal(a.EnableTranscripting, b.EnableTranscripting);
                Assert.Equal(a.EnableInvocationHeader, b.EnableInvocationHeader);
                Assert.Equal(a.OutputDirectory, b.OutputDirectory);
        internal void CompareUpdatableHelp(UpdatableHelp a, UpdatableHelp b)
                Assert.Equal(a.DefaultSourcePath, b.DefaultSourcePath);
        internal void CompareConsoleSessionConfiguration(ConsoleSessionConfiguration a, ConsoleSessionConfiguration b)
                Assert.Equal(a.EnableConsoleSessionConfiguration, b.EnableConsoleSessionConfiguration);
                Assert.Equal(a.ConsoleSessionConfigurationName, b.ConsoleSessionConfigurationName);
        internal void CompareTwoPolicies(PowerShellPolicies a, PowerShellPolicies b)
            // Compare 'ScriptExecution' settings
            CompareScriptExecution(a.ScriptExecution, b.ScriptExecution);
            // Compare 'ScriptBlockLogging' settings
            CompareScriptBlockLogging(a.ScriptBlockLogging, b.ScriptBlockLogging);
            // Compare 'ModuleLogging' settings
            CompareModuleLogging(a.ModuleLogging, b.ModuleLogging);
            // Compare 'ProtectedEventLogging' settings
            CompareProtectedEventLogging(a.ProtectedEventLogging, b.ProtectedEventLogging);
            // Compare 'Transcription' settings
            CompareTranscription(a.Transcription, b.Transcription);
            // Compare 'UpdatableHelp' settings
            CompareUpdatableHelp(a.UpdatableHelp, b.UpdatableHelp);
            // Compare 'ConsoleSessionConfiguration' settings
            CompareConsoleSessionConfiguration(a.ConsoleSessionConfiguration, b.ConsoleSessionConfiguration);
        #region Configuration_File_Setup
        public void CleanupConfigFiles()
            var maxPause = 10;
            while (maxPause-- != 0 && (File.Exists(systemWideConfigFile) || File.Exists(currentUserConfigFile)))
                var pause = false;
                    File.Delete(systemWideConfigFile);
                    pause = true;
                    File.Delete(currentUserConfigFile);
                if (pause)
                    Thread.Sleep(5);
        public void SetupConfigFile1()
            // System wide config file has all policy settings
            var systemWideConfig = new { ConsolePrompting = true, PowerShellPolicies = systemWidePolicies };
            using (var streamWriter = new StreamWriter(systemWideConfigFile))
                serializer.Serialize(streamWriter, systemWideConfig);
            // Current user config file has partial policy settings
            var currentUserConfig = new { DisablePromptToUpdateHelp = false, PowerShellPolicies = currentUserPolicies };
            using (var streamWriter = new StreamWriter(currentUserConfigFile))
                serializer.Serialize(streamWriter, currentUserConfig);
        public void SetupConfigFile2()
            // Current user config file is empty
            CreateEmptyFile(currentUserConfigFile);
        public void SetupConfigFile3()
            // System wide config file is empty
            CreateEmptyFile(systemWideConfigFile);
        public void SetupConfigFile4()
        private static void CreateEmptyFile(string fileName)
            File.Create(fileName).Dispose();
        public void SetupConfigFile5()
            // System wide config file is broken
            CreateBrokenConfigFile(systemWideConfigFile);
            // Current user config file is broken
            CreateBrokenConfigFile(currentUserConfigFile);
        private static void CreateBrokenConfigFile(string fileName)
            File.WriteAllText(fileName, "[abbra");
        internal void ForceReadingFromFile()
            // Reset the cached roots.
            FieldInfo roots = typeof(PowerShellConfig).GetField("configRoots", BindingFlags.NonPublic | BindingFlags.Instance);
            JObject[] value = (JObject[])roots.GetValue(PowerShellConfig.Instance);
            value[0] = null;
            value[1] = null;
    public class PowerShellPolicyTests : IClassFixture<PowerShellPolicyFixture>
        private readonly PowerShellPolicyFixture fixture;
        public PowerShellPolicyTests(PowerShellPolicyFixture fixture)
            this.fixture = fixture;
        [Fact, Priority(1)]
        public void PowerShellConfig_GetPowerShellPolicies_BothConfigFilesNotEmpty()
            fixture.SetupConfigFile1();
            fixture.ForceReadingFromFile();
            var sysPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.AllUsers);
            var userPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.CurrentUser);
            Assert.NotNull(sysPolicies);
            Assert.NotNull(userPolicies);
            fixture.CompareTwoPolicies(sysPolicies, fixture.SystemWidePolicies);
            fixture.CompareTwoPolicies(userPolicies, fixture.CurrentUserPolicies);
        [Fact, Priority(2)]
        public void PowerShellConfig_GetPowerShellPolicies_EmptyUserConfig()
            fixture.SetupConfigFile2();
            Assert.Null(userPolicies);
        [Fact, Priority(3)]
        public void PowerShellConfig_GetPowerShellPolicies_EmptySystemConfig()
            fixture.SetupConfigFile3();
            Assert.Null(sysPolicies);
        [Fact, Priority(4)]
        public void PowerShellConfig_GetPowerShellPolicies_BothConfigFilesEmpty()
            fixture.SetupConfigFile4();
        [Fact, Priority(5)]
        public void PowerShellConfig_GetPowerShellPolicies_BothConfigFilesNotExist()
            fixture.CleanupConfigFiles();
        [Fact, Priority(6)]
        public void Utils_GetPolicySetting_BothConfigFilesNotEmpty()
            ScriptExecution scriptExecution;
            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.SystemWidePolicies.ScriptExecution);
            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.CurrentUserPolicies.ScriptExecution);
            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideThenCurrentUserConfig);
            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserThenSystemWideConfig);
            ScriptBlockLogging scriptBlockLogging;
            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.SystemWidePolicies.ScriptBlockLogging);
            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.CurrentUserPolicies.ScriptBlockLogging);
            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideThenCurrentUserConfig);
            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserThenSystemWideConfig);
            ModuleLogging moduleLogging;
            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.SystemWidePolicies.ModuleLogging);
            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.CurrentUserPolicies.ModuleLogging);
            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideThenCurrentUserConfig);
            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserThenSystemWideConfig);
            ProtectedEventLogging protectedEventLogging;
            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.SystemWidePolicies.ProtectedEventLogging);
            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.CurrentUserPolicies.ProtectedEventLogging);
            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideThenCurrentUserConfig);
            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserThenSystemWideConfig);
            // The CurrentUser config doesn't contain any settings for 'Transcription', 'UpdatableHelp' and 'ConsoleSessionConfiguration'
            Transcription transcription;
            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideOnlyConfig);
            fixture.CompareTranscription(transcription, fixture.SystemWidePolicies.Transcription);
            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserOnlyConfig);
            fixture.CompareTranscription(transcription, null);
            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideThenCurrentUserConfig);
            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserThenSystemWideConfig);
            UpdatableHelp updatableHelp;
            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, fixture.SystemWidePolicies.UpdatableHelp);
            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);
            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideThenCurrentUserConfig);
            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserThenSystemWideConfig);
            ConsoleSessionConfiguration consoleSessionConfiguration;
            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, fixture.SystemWidePolicies.ConsoleSessionConfiguration);
            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);
            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideThenCurrentUserConfig);
            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserThenSystemWideConfig);
        [Fact, Priority(7)]
        public void Utils_GetPolicySetting_EmptyUserConfig()
            // The CurrentUser config is empty
            fixture.CompareScriptExecution(scriptExecution, null);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);
            fixture.CompareModuleLogging(moduleLogging, null);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);
        [Fact, Priority(8)]
        public void Utils_GetPolicySetting_EmptySystemConfig()
            // The SystemWide config is empty
        [Fact, Priority(9)]
        public void Utils_GetPolicySetting_BothConfigFilesEmpty()
            // Both config files are empty
        [Fact, Priority(10)]
        public void Utils_GetPolicySetting_BothConfigFilesNotExist()
            // Both config files don't exist
        [Fact, Priority(11)]
        public void PowerShellConfig_GetPowerShellPolicies_BrokenSystemConfig()
            fixture.SetupConfigFile5();
            Assert.Throws<System.Management.Automation.PSInvalidOperationException>(() => PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.AllUsers));
            Assert.Throws<System.Management.Automation.PSInvalidOperationException>(() => PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.CurrentUser));
