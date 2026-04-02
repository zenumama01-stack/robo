namespace ExperimentalFeatureTest
    #region "Replace existing cmdlet"
    [Experimental("ExpTest.FeatureOne", ExperimentAction.Hide)]
    [Cmdlet("Invoke", "AzureFunctionCSharp")]
    public class InvokeAzureFunctionCommand : PSCmdlet
        public string Token { get; set; }
            WriteObject("Invoke-AzureFunction Version ONE");
    [Experimental("ExpTest.FeatureOne", ExperimentAction.Show)]
    public class InvokeAzureFunctionCommandV2 : PSCmdlet
            WriteObject("Invoke-AzureFunction Version TWO");
    #region "Make parameter set experimental"
    [Cmdlet("Get", "GreetingMessageCSharp", DefaultParameterSetName = "Default")]
    public class GetGreetingMessageCommand : PSCmdlet
        [Parameter("ExpTest.FeatureOne", ExperimentAction.Show, ParameterSetName = "SwitchOneSet")]
        public SwitchParameter SwitchOne { get; set; }
        [Parameter("ExpTest.FeatureOne", ExperimentAction.Show, ParameterSetName = "SwitchTwoSet")]
        public SwitchParameter SwitchTwo { get; set; }
            string message = $"Hello World {Name}.";
            if (ExperimentalFeature.IsEnabled("ExpTest.FeatureOne"))
                if (SwitchOne.IsPresent)
                    message += "-SwitchOne is on.";
                if (SwitchTwo.IsPresent)
                    message += "-SwitchTwo is on.";
            WriteObject(message);
    [Cmdlet("Invoke", "MyCommandCSharp")]
    public class InvokeMyCommandCommand : PSCmdlet
        [Parameter(Mandatory = true, ParameterSetName = "ComputerSet")]
        public string UserName { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "VMSet")]
        public string VMName { get; set; }
        // Enable web socket only if the feature is turned on.
        [Parameter("ExpTest.FeatureOne", ExperimentAction.Show, Mandatory = true, ParameterSetName = "WebSocketSet")]
        public string WebSocketUrl { get; set; }
        // Add -ConfigurationName to parameter set "WebSocketSet" only if the feature is turned on.
        [Parameter(ParameterSetName = "ComputerSet")]
        [Parameter("ExpTest.FeatureOne", ExperimentAction.Show, ParameterSetName = "WebSocketSet")]
        // Add -Port to parameter set "WebSocketSet" only if the feature is turned on.
        [Parameter(ParameterSetName = "VMSet")]
        public int ThrottleLimit { get; set; }
                case "ComputerSet": WriteObject("Invoke-MyCommand with ComputerSet"); break;
                case "VMSet": WriteObject("Invoke-MyCommand with VMSet"); break;
                case "WebSocketSet": WriteObject("Invoke-MyCommand with WebSocketSet"); break;
    [Cmdlet("Test", "MyRemotingCSharp")]
    public class TestMyRemotingCommand : PSCmdlet
        // Replace one parameter with another one when the feature is turned on.
        [Parameter("ExpTest.FeatureOne", ExperimentAction.Hide)]
        public string SessionName { get; set; }
        [Parameter("ExpTest.FeatureOne", ExperimentAction.Show)]
        protected override void EndProcessing() { }
    #region "Use 'Experimental' attribute on parameters"
    [Cmdlet("Save", "MyFileCSharp")]
    public class SaveMyFileCommand : PSCmdlet
        [Parameter(ParameterSetName = "UrlSet")]
        public SwitchParameter ByUrl { get; set; }
        [Parameter(ParameterSetName = "RadioSet")]
        public SwitchParameter ByRadio { get; set; }
    #region "Dynamic parameters"
    public class DynamicParamOne
        public string ConfigFile { get; set; }
        public string ConfigName { get; set; }
    [Cmdlet("Test", "MyDynamicParamOneCSharp")]
    public class TestMyDynamicParamOneCommand : PSCmdlet, IDynamicParameters
            return Name == "Joe" ? new DynamicParamOne() : null;
    public class DynamicParamTwo
    [Cmdlet("Test", "MyDynamicParamTwoCSharp")]
    public class TestMyDynamicParamTwoCommand : PSCmdlet, IDynamicParameters
            return Name == "Joe" ? new DynamicParamTwo() : null;
