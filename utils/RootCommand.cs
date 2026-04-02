namespace Test.Isolated.Root
    [Cmdlet("Test", "RootCommand")]
    public class TestRootCommand : PSCmdlet
        public Red Param { get; set; }
            WriteObject(Param.Name);
    public class Red
        public Red(string name)
    public class Yellow
        public Yellow(string id)
