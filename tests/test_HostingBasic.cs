using Xunit;
namespace PowerShell.Hosting.SDK.Tests
    public static class HostingTests
        [Fact]
        public static void TestCommandFromUtility()
                var results = ps.AddScript("Get-Verb -Verb get").Invoke();
                foreach (dynamic item in results)
                    Assert.Equal("Get", item.Verb);
        public static void TestCommandFromManagement()
                var path = Environment.CurrentDirectory;
                var results = ps.AddCommand("Test-Path").AddParameter("Path", path).Invoke<bool>();
                    Assert.True(item);
        public static void TestCommandFromCore()
                var results = ps.AddScript(@"$i = 0 ; 1..3 | ForEach-Object { $i += $_} ; $i").Invoke<int>();
                    Assert.Equal(6, item);
        [SkippableFact]
        public static void TestCommandFromMMI()
            // Test is disabled since we do not have a CimCmdlets module released in the SDK.
            Skip.IfNot(Platform.IsWindows);
                var results = ps.AddScript("[Microsoft.Management.Infrastructure.CimInstance]::new('Win32_Process')").Invoke();
                Assert.True(results.Count > 0);
        public static void TestCommandFromDiagnostics()
                var results = ps.AddScript("Get-WinEvent -ListLog Application").Invoke();
                    Assert.Equal("Application", item.LogName);
        public static void TestCommandFromSecurity()
                var results = ps.AddScript("ConvertTo-SecureString -String test -AsPlainText -Force").Invoke<SecureString>();
                Assert.IsType<SecureString>(results[0]);
        public static void TestCommandFromWSMan()
                var results = ps.AddScript("Test-WSMan").Invoke();
                    Assert.Equal("Microsoft Corporation", item.ProductVendor);
        public static void TestCommandFromNative()
            var fs = File.Create(Path.GetTempFileName());
            fs.Close();
            string target = fs.Name;
            string path = Path.GetTempFileName();
                // New-Item -ItemType SymbolicLink uses libpsl-native, hence using it for validating native dependencies.
                string command = $"New-Item -ItemType SymbolicLink -Path {path} -Target {target}";
                var results = ps.AddScript(command).Invoke<FileInfo>();
                foreach (var item in results)
                    Assert.Equal(path, item.FullName);
            if (File.Exists(target))
                File.Delete(target);
        /// Reference assemblies should be handled correctly so that Add-Type works in the hosting scenario.
        public static void TestAddTypeCmdletInHostScenario()
            string code = @"
                public class Foo
                    public Foo(string name, string path)
                    public string Path;
                ps.AddCommand("Add-Type").AddParameter("TypeDefinition", code).Invoke();
                var results = ps.AddScript("[Foo]::new('Joe', 'Unknown')").Invoke();
                Assert.Single(results);
                dynamic foo = results[0];
                Assert.Equal("Joe", foo.Name);
                Assert.Equal("Unknown", foo.Path);
        public static void TestConsoleShellScenario()
            int ret = ConsoleShell.Start("Hello", string.Empty, new string[] { "-noprofile", "-c", "exit 42" });
            Assert.Equal(42, ret);
        /* Test disabled because CommandLineParser is static and can only be initialized once (above in TestConsoleShellScenario)
        /// ConsoleShell cannot start with both InitialSessionState and -ConfigurationFile argument configurations specified.
        public static void TestConsoleShellConfigConflictError()
            var iss = System.Management.Automation.Runspaces.InitialSessionState.CreateDefault2();
            int ret = ConsoleShell.Start(iss, "BannerText", string.Empty, new string[] { @"-ConfigurationFile ""noneSuch""" });
            Assert.Equal(70, ret);  // ExitCodeInitFailure.
        public static void TestBuiltInModules()
            if (System.Management.Automation.Platform.IsWindows)
                iss.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.RemoteSigned;
            using var runspace = System.Management.Automation.Runspaces.RunspaceFactory.CreateRunspace(iss);
            using var ps = System.Management.Automation.PowerShell.Create(runspace);
            var results_1 = ps.AddScript("Write-Output Hello > $null; Get-Module").Invoke<System.Management.Automation.PSModuleInfo>();
            Assert.Single(results_1);
            var module = results_1[0];
            Assert.Equal("Microsoft.PowerShell.Utility", module.Name);
            var results_2 = ps.AddScript("Join-Path $PSHOME 'Modules' 'Microsoft.PowerShell.Utility' 'Microsoft.PowerShell.Utility.psd1'").Invoke<string>();
            var moduleManifestPath = results_2[0];
            Assert.Equal(moduleManifestPath, module.Path, ignoreCase: true);
