using System.CommandLine;
namespace Test.Isolated.Nested
    [Cmdlet("Test", "NestedCommand")]
    public class TestNestedCommand : PSCmdlet
        public Foo Param { get; set; }
            WriteObject($"{Param.Name}-{Param.Path}-{typeof(RootCommand).Assembly.FullName}");
    public class Bar
        public Bar(string id)
