    public class Scripting
        private Runspace runspace;
        private ScriptBlock scriptBlock;
        private void SetupRunspace()
            // Unless you want to run commands from any built-in modules, using 'CreateDefault2' is enough.
            runspace = RunspaceFactory.CreateRunspace(InitialSessionState.CreateDefault2());
        #region Invoke-Method
        [ParamsSource(nameof(ValuesForScript))]
        public string InvokeMethodScript { get; set; }
        public IEnumerable<string> ValuesForScript()
            yield return @"'String'.GetType()";
            yield return @"[System.IO.Path]::HasExtension('')";
            // Test on COM method invocation.
                yield return @"$sh=New-Object -ComObject Shell.Application; $sh.Namespace('c:\')";
                yield return @"$fs=New-Object -ComObject scripting.filesystemobject; $fs.Drives";
        [GlobalSetup(Target = nameof(InvokeMethod))]
            SetupRunspace();
            scriptBlock = ScriptBlock.Create(InvokeMethodScript);
            // Run it once to get the C# code jitted and the script compiled.
            scriptBlock.Invoke();
        public Collection<PSObject> InvokeMethod()
            return scriptBlock.Invoke();
        [GlobalCleanup]
        public void GlobalCleanup()
