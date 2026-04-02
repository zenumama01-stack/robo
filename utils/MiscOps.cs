    internal static class PipelineOps
        private static CommandProcessorBase AddCommand(PipelineProcessor pipe,
                                                       CommandParameterInternal[] commandElements,
                                                       CommandBaseAst commandBaseAst,
                                                       CommandRedirection[] redirections,
            var commandAst = commandBaseAst as CommandAst;
            var invocationToken = commandAst != null ? commandAst.InvocationOperator : TokenKind.Unknown;
            bool dotSource = invocationToken == TokenKind.Dot;
            SessionStateInternal commandSessionState = null;
            Diagnostics.Assert(commandElements[0].ArgumentSpecified && !commandElements[0].ParameterNameSpecified,
                "Compiler will pass first parameter as an argument.");
            var mi = PSObject.Base(commandElements[0].ArgumentValue) as PSModuleInfo;
                if (mi.ModuleType == ModuleType.Binary && mi.SessionState == null)
                        null, "CantInvokeInBinaryModule", ParserStrings.CantInvokeInBinaryModule, mi.Name);
                else if (mi.SessionState == null)
                        null, "CantInvokeInNonImportedModule", ParserStrings.CantInvokeInNonImportedModule, mi.Name);
                else if ((invocationToken == TokenKind.Ampersand || invocationToken == TokenKind.Dot) && mi.LanguageMode != context.LanguageMode)
                        // Disallow FullLanguage "& (Get-Module MyModule) MyPrivateFn" from ConstrainedLanguage because it always
                        // runs "internal" origin and so has access to all functions, including non-exported functions.
                        // Otherwise we end up leaking non-exported functions that run in FullLanguage.
                        throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null,
                            "CantInvokeCallOperatorAcrossLanguageBoundaries", ParserStrings.CantInvokeCallOperatorAcrossLanguageBoundaries);
                        title: ParserStrings.WDACParserModuleScopeCallOperatorLogTitle,
                        message: ParserStrings.WDACParserModuleScopeCallOperatorLogMessage,
                        fqid: "ModuleScopeCallOperatorNotAllowed",
                commandSessionState = mi.SessionState.Internal;
                commandIndex += 1;
            object command;
            IScriptExtent commandExtent;
            var cpiCommand = commandElements[commandIndex];
            if (cpiCommand.ParameterNameSpecified)
                command = cpiCommand.ParameterText;
                commandExtent = cpiCommand.ParameterExtent;
                if (cpiCommand.ArgumentSpecified)
                    // BUG: we've seen something like:
                    //     & (gmo Module) -foo: bar
                    // The command is -foo:, and bar is an argument, but both are in commandElements[commandIndex],
                    // so we won't add 'bar' as an argument.
                command = PSObject.Base(cpiCommand.ArgumentValue);
                commandExtent = cpiCommand.ArgumentExtent;
            string invocationName = (dotSource) ? "." : invocationToken == TokenKind.Ampersand ? "&" : null;
            CommandProcessorBase commandProcessor;
            var scriptBlock = command as ScriptBlock;
                commandProcessor = CommandDiscovery.CreateCommandProcessorForScript(scriptBlock, context, !dotSource, commandSessionState);
                    commandProcessor = context.CommandDiscovery.LookupCommandProcessor(commandInfo, context.EngineSessionState.CurrentScope.ScopeOrigin, !dotSource, commandSessionState);
                    var commandName = command as string ?? PSObject.ToStringParser(context, command);
                    invocationName ??= commandName;
                            commandExtent,
                            "BadExpression",
                            ParserStrings.BadExpression,
                            dotSource ? "." : "&");
                        // See if we need to resolve the command in a different session state
                        // as will be the case with modules...
                        // BUGBUG - this can be cleaned up by fixing the overload on execution context (but not easily.)
                        if (commandSessionState != null)
                                context.EngineSessionState = commandSessionState;
                                commandProcessor = context.CreateCommand(commandName, dotSource);
                        // CreateCommand doesn't have the context to set InvocationInfo properly
                        // so we'll do it here instead...
                            InvocationInfo invocationInfo = new InvocationInfo(null, commandExtent, context)
                            { InvocationName = invocationName };
                            rte.ErrorRecord.SetInvocationInfo(invocationInfo);
            InternalCommand cmd = commandProcessor.Command;
            commandProcessor.UseLocalScope = !dotSource &&
                                             (cmd is ScriptCommand || cmd is PSScriptCmdlet);
            bool isNativeCommand = commandProcessor is NativeCommandProcessor;
            for (int i = commandIndex + 1; i < commandElements.Length; ++i)
                var cpi = commandElements[i];
                if (cpi.ParameterNameSpecified)
                    // Skip adding the special -- parameter unless we're invoking a native command.
                    if (cpi.ParameterName.Equals("-", StringComparison.OrdinalIgnoreCase) && !isNativeCommand)
                if (cpi.ArgumentToBeSplatted)
                    foreach (var splattedCpi in Splat(cpi.ArgumentValue, cpi.ArgumentAst))
                        commandProcessor.AddParameter(splattedCpi);
                    commandProcessor.AddParameter(cpi);
            if (commandProcessor.IsHelpRequested(out helpTarget, out helpCategory))
                commandProcessor = CommandProcessorBase.CreateGetHelpCommandProcessor(context, helpTarget, helpCategory);
            commandProcessor.Command.InvocationExtent = commandBaseAst.Extent;
            commandProcessor.Command.MyInvocation.ScriptPosition = commandBaseAst.Extent;
            commandProcessor.Command.MyInvocation.InvocationName = invocationName;
            pipe.Add(commandProcessor);
            bool redirectedError = false;
            bool redirectedWarning = false;
            bool redirectedVerbose = false;
            bool redirectedDebug = false;
            bool redirectedInformation = false;
                if (isNativeCommand)
                    foreach (CommandRedirection redirection in redirections)
                        if (redirection is MergingRedirection)
                            redirection.Bind(pipe, commandProcessor, context);
                    if (!isNativeCommand || redirection is not MergingRedirection)
                    switch (redirection.FromStream)
                            redirectedError = true;
                            redirectedWarning = true;
                            redirectedVerbose = true;
                            redirectedDebug = true;
                            redirectedInformation = true;
            // Pipe redirection can also be specified via the ExecutionContext pipes.
            if (!redirectedError)
                if (context.ShellFunctionErrorOutputPipe != null)
                    commandProcessor.CommandRuntime.ErrorOutputPipe = context.ShellFunctionErrorOutputPipe;
                    commandProcessor.CommandRuntime.ErrorOutputPipe.ExternalWriter = context.ExternalErrorOutput;
            if (!redirectedWarning && (context.ExpressionWarningOutputPipe != null))
                commandProcessor.CommandRuntime.WarningOutputPipe = context.ExpressionWarningOutputPipe;
            if (!redirectedVerbose && (context.ExpressionVerboseOutputPipe != null))
                commandProcessor.CommandRuntime.VerboseOutputPipe = context.ExpressionVerboseOutputPipe;
            if (!redirectedDebug && (context.ExpressionDebugOutputPipe != null))
                commandProcessor.CommandRuntime.DebugOutputPipe = context.ExpressionDebugOutputPipe;
            if (!redirectedInformation && (context.ExpressionInformationOutputPipe != null))
                commandProcessor.CommandRuntime.InformationOutputPipe = context.ExpressionInformationOutputPipe;
            // Warning, Verbose, Debug should pick up any redirection information from its parent command runtime object.
            if (context.CurrentCommandProcessor != null && context.CurrentCommandProcessor.CommandRuntime != null)
                if (!redirectedWarning &&
                    context.CurrentCommandProcessor.CommandRuntime.WarningOutputPipe != null)
                    commandProcessor.CommandRuntime.WarningOutputPipe = context.CurrentCommandProcessor.CommandRuntime.WarningOutputPipe;
                if (!redirectedVerbose &&
                    context.CurrentCommandProcessor.CommandRuntime.VerboseOutputPipe != null)
                    commandProcessor.CommandRuntime.VerboseOutputPipe = context.CurrentCommandProcessor.CommandRuntime.VerboseOutputPipe;
                if (!redirectedDebug &&
                    context.CurrentCommandProcessor.CommandRuntime.DebugOutputPipe != null)
                    commandProcessor.CommandRuntime.DebugOutputPipe = context.CurrentCommandProcessor.CommandRuntime.DebugOutputPipe;
                if (!redirectedInformation &&
                    context.CurrentCommandProcessor.CommandRuntime.InformationOutputPipe != null)
                    commandProcessor.CommandRuntime.InformationOutputPipe = context.CurrentCommandProcessor.CommandRuntime.InformationOutputPipe;
        internal static IEnumerable<CommandParameterInternal> Splat(object splattedValue, Ast splatAst)
            splattedValue = PSObject.Base(splattedValue);
            var markUntrustedData = false;
                // If the value to be splatted is untrusted, then make sure sub-values held by it are
                // also marked as untrusted.
                markUntrustedData = ExecutionContext.IsMarkedAsUntrusted(splattedValue);
            IDictionary splattedTable = splattedValue as IDictionary;
            if (splattedTable != null)
                foreach (DictionaryEntry de in splattedTable)
                    string parameterName = de.Key.ToString();
                    object parameterValue = de.Value;
                    string parameterText = GetParameterText(parameterName);
                    if (markUntrustedData)
                        ExecutionContext.MarkObjectAsUntrusted(parameterValue);
                    yield return CommandParameterInternal.CreateParameterWithArgument(
                        parameterAst: splatAst,
                        parameterName: parameterName,
                        argumentAst: splatAst,
                        value: parameterValue,
                        spaceAfterParameter: false,
                        fromSplatting: true);
                IEnumerable enumerableValue = splattedValue as IEnumerable;
                if (enumerableValue != null)
                    foreach (object obj in enumerableValue)
                            ExecutionContext.MarkObjectAsUntrusted(obj);
                        yield return SplatEnumerableElement(obj, splatAst);
                    yield return SplatEnumerableElement(splattedValue, splatAst);
        private static CommandParameterInternal SplatEnumerableElement(object splattedArgument, Ast splatAst)
            var psObject = splattedArgument as PSObject;
            if (psObject != null)
                var prop = psObject.Properties[ScriptParameterBinderController.NotePropertyNameForSplattingParametersInArgs];
                var baseObj = psObject.BaseObject;
                if (prop != null && prop.Value is string && baseObj is string)
                    return CommandParameterInternal.CreateParameter((string)prop.Value, (string)baseObj, splatAst);
            return CommandParameterInternal.CreateArgument(splattedArgument, splatAst);
        private static string GetParameterText(string parameterName)
            Diagnostics.Assert(parameterName != null, "caller makes sure the parameterName is not null");
            int endPosition = parameterName.Length;
            while ((endPosition > 0) && char.IsWhiteSpace(parameterName[endPosition - 1]))
            if (endPosition == 0 || parameterName[endPosition - 1] == ':')
                return "-" + parameterName;
            if (endPosition == parameterName.Length)
                parameterText = "-" + parameterName + ":";
                string whitespaces = parameterName.Substring(endPosition);
                parameterText = string.Concat("-", parameterName.AsSpan(0, endPosition), ":", whitespaces);
            return parameterText;
        internal static void InvokePipeline(object input,
                                            bool ignoreInput,
                                            CommandParameterInternal[][] pipeElements,
                                            CommandBaseAst[] pipeElementAsts,
                                            CommandRedirection[][] commandRedirections,
                                            FunctionContext funcContext)
            ExecutionContext context = funcContext._executionContext;
            Pipe outputPipe = funcContext._outputPipe;
                context.Events?.ProcessPendingActions();
                if (input == AutomationNull.Value && !ignoreInput)
                    // We have seen something like:
                    //    $e | measure-object
                    // And $e is AutomationNull.Value.  We want to ensure
                    // measure-object runs w/o sending anything through the pipe,
                    // so we'll turn the pipe into Out-Null | ...
                    // This cleanly avoids any problems with the pipeline processing
                    // code dealing with null/AutomationNull input going directly to
                    // the first command (e.g. Measure-Object).
                    AddNoopCommandProcessor(pipelineProcessor, context);
                CommandRedirection[] commandRedirection = null;
                for (int i = 0; i < pipeElements.Length; i++)
                    commandRedirection = commandRedirections?[i];
                    commandProcessor = AddCommand(pipelineProcessor, pipeElements[i], pipeElementAsts[i],
                                                  commandRedirection, context);
                var cmdletInfo = commandProcessor?.CommandInfo as CmdletInfo;
                if (cmdletInfo?.ImplementingType == typeof(OutNullCommand))
                    var commandsCount = pipelineProcessor.Commands.Count;
                    if (commandsCount == 1)
                        // Out-Null is the only command, bail without running anything
                    // Out-Null is the last command, rewrite command before Out-Null to a null pipe, but
                    // only if it didn't redirect anything, e.g. `Get-Stuff > o.txt | Out-Null`
                    var nextToLastCommand = pipelineProcessor.Commands[commandsCount - 2];
                    if (!nextToLastCommand.CommandRuntime.OutputPipe.IsRedirected)
                        pipelineProcessor.Commands.RemoveAt(commandsCount - 1);
                        commandProcessor = nextToLastCommand;
                        nextToLastCommand.CommandRuntime.OutputPipe = new Pipe { NullPipe = true };
                if (commandProcessor != null && !commandProcessor.CommandRuntime.OutputPipe.IsRedirected)
                    pipelineProcessor.LinkPipelineSuccessOutput(outputPipe ?? new Pipe(new List<object>()));
                    // Fix up merge redirection bindings on last command processor.
                    if (commandRedirection != null)
                        foreach (CommandRedirection redirection in commandRedirection)
                                redirection.Bind(pipelineProcessor, commandProcessor, context);
                context.PushPipelineProcessor(pipelineProcessor);
                    pipelineProcessor.SynchronousExecuteEnumerate(input);
                    context.PopPipelineProcessor(false);
                context.QuestionMarkVariableValue = !pipelineProcessor.ExecutionFailed;
        internal static void InvokePipelineInBackground(
                                            PipelineBaseAst pipelineAst,
                // For background jobs rewrite the pipeline as a Start-Job command
                var scriptblockBodyString = pipelineAst.Extent.Text;
                var pipelineOffset = pipelineAst.Extent.StartOffset;
                var variables = pipelineAst.FindAll(static x => x is VariableExpressionAst, true);
                // Minimize allocations by initializing the stringbuilder to the size of the source string + space for ${using:} * 2
                System.Text.StringBuilder updatedScriptblock = new System.Text.StringBuilder(scriptblockBodyString.Length + 18);
                // Prefix variables in the scriptblock with $using:
                foreach (var v in variables)
                    var variableName = ((VariableExpressionAst)v).VariablePath.UserPath;
                    // Skip variables that don't exist
                    if (funcContext._executionContext.EngineSessionState.GetVariable(variableName) == null)
                    // Skip PowerShell magic variables
                    if (!Regex.Match(
                            variableName,
                            "^(global:){0,1}(PID|PSVersionTable|PSEdition|PSHOME|HOST|TRUE|FALSE|NULL)$",
                            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Success)
                        updatedScriptblock.Append(scriptblockBodyString.AsSpan(position, v.Extent.StartOffset - pipelineOffset - position));
                        updatedScriptblock.Append("${using:");
                        updatedScriptblock.Append(CodeGeneration.EscapeVariableName(variableName));
                        updatedScriptblock.Append('}');
                        position = v.Extent.EndOffset - pipelineOffset;
                updatedScriptblock.Append(scriptblockBodyString.AsSpan(position));
                var sb = ScriptBlock.Create(updatedScriptblock.ToString());
                var commandInfo = new CmdletInfo("Start-Job", typeof(StartJobCommand));
                commandProcessor = context.CommandDiscovery.LookupCommandProcessor(commandInfo, CommandOrigin.Internal, false, context.EngineSessionState);
                var workingDirectoryParameter = CommandParameterInternal.CreateParameterWithArgument(
                    parameterAst: pipelineAst,
                    parameterName: "WorkingDirectory",
                    parameterText: null,
                    argumentAst: pipelineAst,
                    value: context.SessionState.Path.CurrentLocation.Path,
                var scriptBlockParameter = CommandParameterInternal.CreateParameterWithArgument(
                    parameterName: "ScriptBlock",
                    value: sb,
                commandProcessor.AddParameter(workingDirectoryParameter);
                commandProcessor.AddParameter(scriptBlockParameter);
                pipelineProcessor.Add(commandProcessor);
        private static void AddNoopCommandProcessor(PipelineProcessor pipelineProcessor, ExecutionContext context)
            var commandInfo = new CmdletInfo("Out-Null", typeof(OutNullCommand));
            var commandProcessor = context.CommandDiscovery.LookupCommandProcessor(commandInfo,
                                                                                   context.EngineSessionState.CurrentScope.ScopeOrigin,
                                                                                   sessionState: null);
        internal static object CheckAutomationNullInCommandArgument(object obj)
            var objAsArray = obj as object[];
            return objAsArray != null ? CheckAutomationNullInCommandArgumentArray(objAsArray) : obj;
        internal static object[] CheckAutomationNullInCommandArgumentArray(object[] objArray)
                for (int i = 0; i < objArray.Length; ++i)
                    if (objArray[i] == AutomationNull.Value)
                        objArray[i] = null;
            return objArray;
        internal static SteppablePipeline GetSteppablePipeline(PipelineAst pipelineAst, CommandOrigin commandOrigin, ScriptBlock scriptBlock, object[] args)
            var pipelineProcessor = new PipelineProcessor();
            var commandTuples = new List<Tuple<CommandAst, List<CommandParameterInternal>, List<CommandRedirection>>>();
                string scriptText = scriptBlock.ToString();
                    ParserStrings.GetSteppablePipelineFromWrongThread,
                e.SetErrorId("GetSteppablePipelineFromWrongThread");
            // We try binding the parameter when following conditions are satisfied:
            //   * Arguments are provided
            //   * The ScriptBlock has parameters
            // If the script block has no parameter, RuntimeDefinedParameters.Data will be set to RuntimeDefinedParameterDictionary.EmptyParameterArray
            bool useParameter = (args != null && args.Length > 0) &&
                                scriptBlock.RuntimeDefinedParameters.Data !=
                                RuntimeDefinedParameterDictionary.EmptyParameterArray;
                if (useParameter)
                    var locals = MutableTuple.MakeTuple(Compiler.DottedLocalsTupleType,
                                                        Compiler.DottedLocalsNameIndexMap);
                    object[] remainingArgs =
                        ScriptBlock.BindArgumentsForScriptblockInvoke(
                            (RuntimeDefinedParameter[])scriptBlock.RuntimeDefinedParameters.Data,
                            args, context, false, null, locals);
                // GetSteppablePipeline() is called on an arbitrary script block with the intention
                // of invoking it. So the trustworthiness is defined by the trustworthiness of the
                // script block's language mode.
                bool isTrusted = scriptBlock.LanguageMode == PSLanguageMode.FullLanguage;
                if (scriptBlock.LanguageMode == PSLanguageMode.ConstrainedLanguage
                    isTrusted = true;
                        title: ParserStrings.WDACGetSteppablePipelineLogTitle,
                        message: ParserStrings.WDACGetSteppablePipelineLogMessage,
                        fqid: "GetSteppablePipelineMayFail",
                foreach (var commandAst in pipelineAst.PipelineElements.Cast<CommandAst>())
                    var commandParameters = new List<CommandParameterInternal>();
                    foreach (var commandElement in commandAst.CommandElements)
                        var commandParameterAst = commandElement as CommandParameterAst;
                        if (commandParameterAst != null)
                            commandParameters.Add(GetCommandParameter(commandParameterAst, isTrusted, context));
                        var exprAst = (ExpressionAst)commandElement;
                        var argument = Compiler.GetExpressionValue(exprAst, isTrusted, context);
                        var splatting = exprAst is VariableExpressionAst && ((VariableExpressionAst)exprAst).Splatted;
                        commandParameters.Add(CommandParameterInternal.CreateArgument(argument, exprAst, splatting));
                    var redirections = new List<CommandRedirection>();
                    foreach (var redirection in commandAst.Redirections)
                        redirections.Add(GetCommandRedirection(redirection, isTrusted, context));
                    commandTuples.Add(Tuple.Create(commandAst, commandParameters, redirections));
            foreach (var commandTuple in commandTuples)
                var commandProcessor = AddCommand(pipelineProcessor, commandTuple.Item2.ToArray(), commandTuple.Item1, commandTuple.Item3.ToArray(), context);
                commandProcessor.Command.CommandOriginInternal = commandOrigin;
                commandProcessor.CommandScope.ScopeOrigin = commandOrigin;
                commandProcessor.Command.MyInvocation.CommandOrigin = commandOrigin;
                // For nicer error reporting, we want to make it look like errors in the steppable pipeline point back to
                // the caller of the proxy.  We don't want errors pointing to the script block created in the proxy.
                // Here we assume (in a safe way) that GetSteppablePipeline is called from script.  If that isn't the case,
                // we won't crash, but the error reporting might be a little misleading.
                var callStack = context.Debugger.GetCallStack().ToArray();
                if (callStack.Length > 0 && Regex.IsMatch(callStack[0].Position.Text, "GetSteppablePipeline", RegexOptions.IgnoreCase))
                    var myInvocation = commandProcessor.Command.MyInvocation;
                    myInvocation.InvocationName = callStack[0].InvocationInfo.InvocationName;
                    if (callStack.Length > 1)
                        var displayPosition = callStack[1].Position;
                        if (displayPosition != null && displayPosition != PositionUtilities.EmptyExtent)
                            myInvocation.DisplayScriptPosition = displayPosition;
                // Set the data stream merge properties based on ExecutionContext.
                    commandProcessor.CommandRuntime.SetMergeFromRuntime(context.CurrentCommandProcessor.CommandRuntime);
        private static CommandParameterInternal GetCommandParameter(CommandParameterAst commandParameterAst, bool isTrusted, ExecutionContext context)
            var argumentAst = commandParameterAst.Argument;
                return CommandParameterInternal.CreateParameter(commandParameterAst.ParameterName, errorPos.Text, commandParameterAst);
            object argumentValue = Compiler.GetExpressionValue(argumentAst, isTrusted, context);
            bool spaceAfterParameter = errorPos.EndLineNumber != argumentAst.Extent.StartLineNumber ||
                                       errorPos.EndColumnNumber != argumentAst.Extent.StartColumnNumber;
            return CommandParameterInternal.CreateParameterWithArgument(commandParameterAst, commandParameterAst.ParameterName,
                                                                        errorPos.Text, argumentAst, argumentValue,
                                                                        spaceAfterParameter);
        private static CommandRedirection GetCommandRedirection(RedirectionAst redirectionAst, bool isTrusted, ExecutionContext context)
            var fileRedirection = redirectionAst as FileRedirectionAst;
            if (fileRedirection != null)
                object fileName = Compiler.GetExpressionValue(fileRedirection.Location, isTrusted, context);
                return new FileRedirection(fileRedirection.FromStream, fileRedirection.Append, fileName.ToString());
            var mergingRedirectionAst = (MergingRedirectionAst)redirectionAst;
        internal static object PipelineResult(List<object> resultList)
            var resultCount = resultList.Count;
            if (resultCount == 0)
            var result = resultCount == 1 ? resultList[0] : resultList.ToArray();
            // Clear the array list so that we don't write the results of the pipe when flushing the pipe.
            resultList.Clear();
        internal static void FlushPipe(Pipe oldPipe, List<object> resultList)
            for (int i = 0; i < resultList.Count; i++)
                oldPipe.Add(resultList[i]);
        internal static void ClearPipe(List<object> resultList)
        internal static ExitException GetExitException(object exitCodeObj)
                if (!LanguagePrimitives.IsNull(exitCodeObj))
                    exitCode = ParserOps.ConvertTo<int>(exitCodeObj, PositionUtilities.EmptyExtent);
            catch (Exception) // ignore non-severe exceptions
            return new ExitException(exitCode);
        internal static void CheckForInterrupts(ExecutionContext context)
        // This is to work around a DLR problem with gotos in try/catch to the end of a lambda.
        internal static void Nop() { }
    #region Redirections
    internal abstract class CommandRedirection
        protected CommandRedirection(RedirectionStream from)
        internal RedirectionStream FromStream { get; }
        internal abstract void Bind(PipelineProcessor pipelineProcessor, CommandProcessorBase commandProcessor, ExecutionContext context);
        internal void UnbindForExpression(FunctionContext funcContext, Pipe[] pipes)
            if (pipes == null)
                // The pipes can be null if there was an exception (ideally we'd just call unbind
                // from a fault, but that isn't supported in a clr dynamic method.
            var context = funcContext._executionContext;
            switch (FromStream)
                    funcContext._outputPipe = pipes[(int)RedirectionStream.Output];
                    context.ShellFunctionErrorOutputPipe = pipes[(int)RedirectionStream.Error];
                    context.ExpressionWarningOutputPipe = pipes[(int)RedirectionStream.Warning];
                    context.ExpressionVerboseOutputPipe = pipes[(int)RedirectionStream.Verbose];
                    context.ExpressionDebugOutputPipe = pipes[(int)RedirectionStream.Debug];
                    context.ExpressionInformationOutputPipe = pipes[(int)RedirectionStream.Information];
                case RedirectionStream.Output:
                    context.ShellFunctionErrorOutputPipe = pipes[(int)FromStream];
                    context.ExpressionWarningOutputPipe = pipes[(int)FromStream];
                    context.ExpressionVerboseOutputPipe = pipes[(int)FromStream];
                    context.ExpressionDebugOutputPipe = pipes[(int)FromStream];
                    context.ExpressionInformationOutputPipe = pipes[(int)FromStream];
    internal class MergingRedirection : CommandRedirection
        internal MergingRedirection(RedirectionStream from, RedirectionStream to)
            : base(from)
            if (to != RedirectionStream.Output)
                throw InterpreterError.NewInterpreterException(to, typeof(ArgumentException),
                                               null, "RedirectionStreamCanOnlyMergeToOutputStream",
                                               ParserStrings.RedirectionStreamCanOnlyMergeToOutputStream);
            // this.ToStream = to;
            return FromStream == RedirectionStream.All
                       ? "*>&1"
                       : string.Create(CultureInfo.InvariantCulture, $"{(int)FromStream}>&1");
        // private RedirectionStream ToStream { get; set; }
        // Handle merging redirections for commands, like:
        //   dir 2>&1
        // A more realistic example:
        //   dir 2>&1 > out
        internal override void Bind(PipelineProcessor pipelineProcessor, CommandProcessorBase commandProcessor, ExecutionContext context)
            Pipe pipe = commandProcessor.CommandRuntime.OutputPipe;
                    commandProcessor.CommandRuntime.ErrorMergeTo = MshCommandRuntime.MergeDataStream.Output;
                    commandProcessor.CommandRuntime.WarningOutputPipe = pipe;
                    commandProcessor.CommandRuntime.VerboseOutputPipe = pipe;
                    commandProcessor.CommandRuntime.DebugOutputPipe = pipe;
                    commandProcessor.CommandRuntime.InformationOutputPipe = pipe;
        // Handle merging redirections for expressions, like:
        //   $(write-error) 2>&1
        //   $(write-error) 2>&1 > out
        internal Pipe[] BindForExpression(ExecutionContext context, FunctionContext funcContext)
            Pipe[] oldPipes = new Pipe[(int)RedirectionStream.Information + 1];
            Pipe pipe = funcContext._outputPipe;
            // We set the redirection pipe directly in Context because there is no command processor
            // (which indirectly does the same thing as this code.)
                    oldPipes[(int)RedirectionStream.Output] = funcContext._outputPipe;
                    oldPipes[(int)RedirectionStream.Error] = context.ShellFunctionErrorOutputPipe;
                    context.ShellFunctionErrorOutputPipe = pipe;
                    oldPipes[(int)RedirectionStream.Warning] = context.ExpressionWarningOutputPipe;
                    context.ExpressionWarningOutputPipe = pipe;
                    oldPipes[(int)RedirectionStream.Verbose] = context.ExpressionVerboseOutputPipe;
                    context.ExpressionVerboseOutputPipe = pipe;
                    oldPipes[(int)RedirectionStream.Debug] = context.ExpressionDebugOutputPipe;
                    context.ExpressionDebugOutputPipe = pipe;
                    oldPipes[(int)RedirectionStream.Information] = context.ExpressionInformationOutputPipe;
                    context.ExpressionInformationOutputPipe = pipe;
                    oldPipes[(int)FromStream] = context.ShellFunctionErrorOutputPipe;
                    oldPipes[(int)FromStream] = context.ExpressionWarningOutputPipe;
                    oldPipes[(int)FromStream] = context.ExpressionVerboseOutputPipe;
                    oldPipes[(int)FromStream] = context.ExpressionDebugOutputPipe;
                    oldPipes[(int)FromStream] = context.ExpressionInformationOutputPipe;
            return oldPipes;
    internal class FileRedirection : CommandRedirection, IDisposable
        internal FileRedirection(RedirectionStream from, bool appending, string file)
            this.File = file;
            this.Appending = appending;
                "{0}> {1}",
                FromStream == RedirectionStream.All
                    ? "*"
                    : ((int)FromStream).ToString(CultureInfo.InvariantCulture),
                File);
        internal string File { get; }
        internal bool Appending { get; }
        private PipelineProcessor PipelineProcessor { get; set; }
        // Handle binding file redirection for commands, like:
        //    dir > out
            // Check first to see if File is a variable path. If so, we'll not create the FileBytePipe
            bool redirectToVariable = false;
            context.SessionState.Path.GetUnresolvedProviderPathFromPSPath(File, out ProviderInfo p, out _);
            if (p != null && p.NameEquals(context.ProviderNames.Variable))
                redirectToVariable = true;
            if (commandProcessor is NativeCommandProcessor nativeCommand
                && nativeCommand.CommandRuntime.ErrorMergeTo is not MshCommandRuntime.MergeDataStream.Output
                && FromStream is RedirectionStream.Output
                && !string.IsNullOrWhiteSpace(File)
                && !redirectToVariable)
                nativeCommand.StdOutDestination = FileBytePipe.Create(File, Appending);
            Pipe pipe = GetRedirectionPipe(context, pipelineProcessor);
                    // Since a temp output pipe is going to be used, we should pass along the error and warning variable list.
                    // Normally, context.CurrentCommandProcessor will not be null. But in legacy DRTs from ParserTest.cs,
                    // a scriptblock may be invoked through 'DoInvokeReturnAsIs' using .NET reflection. In that case,
                    // context.CurrentCommandProcessor will be null. We don't try passing along variable lists in such case.
                    context.CurrentCommandProcessor?.CommandRuntime.OutputPipe.SetVariableListForTemporaryPipe(pipe);
                    commandProcessor.CommandRuntime.OutputPipe = pipe;
                    commandProcessor.CommandRuntime.ErrorOutputPipe = pipe;
        // Handle binding file redirections for expressions, like:
        //     $(write-error blah) 2> out
        internal Pipe[] BindForExpression(FunctionContext funcContext)
            // GetRedirectionPipe can throw if the filename specified can't be written to.  In that case,
            // oldPipes is null, and when unbinding, there is nothing to do.
            Pipe pipe = GetRedirectionPipe(context, null);
            var oldPipes = new Pipe[(int)RedirectionStream.Information + 1];
                    funcContext._outputPipe.SetVariableListForTemporaryPipe(pipe);
                    funcContext._outputPipe = pipe;
        internal Pipe GetRedirectionPipe(ExecutionContext context, PipelineProcessor parentPipelineProcessor)
            if (string.IsNullOrWhiteSpace(File))
                return new Pipe { NullPipe = true };
            // determine whether we're trying to set a variable by inspecting the file path
            // if we can determine that it's a variable, we'll use Set-Variable rather than Out-File
            var name = context.SessionState.Path.GetUnresolvedProviderPathFromPSPath(File, out ProviderInfo p, out _);
                commandProcessor = context.CreateCommand("Set-Variable", false);
                Diagnostics.Assert(commandProcessor != null, "CreateCommand returned null");
                    /*argumentAst*/null, name,
                if (this.Appending)
                    commandProcessor.AddParameter(CommandParameterInternal.CreateParameter("Append", "-Append", null));
                commandProcessor = context.CreateCommand("out-file", false);
                // Previously, we mandated Unicode encoding here
                // Now, We can take what ever has been set if PSDefaultParameterValues
                // Unicode is still the default, but now may be overridden
                    /*parameterAst*/null, "Filepath", "-Filepath:",
                    /*argumentAst*/null, File,
                        /*parameterAst*/null, "Append", "-Append:",
                        /*argumentAst*/null, true,
            PipelineProcessor = new PipelineProcessor();
            PipelineProcessor.Add(commandProcessor);
                PipelineProcessor.StartStepping(true);
                // If it's just wrapping an argument exception, build a new exception that
                // is more specific tp the redirection operation...
                if (rte.ErrorRecord.Exception is System.ArgumentException)
                        "RedirectionFailed",
                        ParserStrings.RedirectionFailed,
                        rte.ErrorRecord.Exception,
                        rte.ErrorRecord.Exception.Message);
            // I think this is only necessary for calling Dispose on the commands in the redirection pipe.
            parentPipelineProcessor?.AddRedirectionPipe(PipelineProcessor);
            return new Pipe(context, PipelineProcessor);
        /// After file redirection is done, we need to call 'DoComplete' on the pipeline processor,
        /// so that 'EndProcessing' of Out-File can be called to wrap up the file write operation.
        /// 'StartStepping' is called after creating the pipeline processor.
        /// 'Step' is called when an object is added to the pipe created with the pipeline processor.
        internal void CallDoCompleteForExpression()
            // The pipe returned from 'GetRedirectionPipe' could be a NullPipe
            PipelineProcessor?.DoComplete();
                PipelineProcessor?.Dispose();
    #endregion Redirections
    internal static class FunctionOps
        internal static void DefineFunction(ExecutionContext context,
                                            FunctionDefinitionAst functionDefinitionAst,
                                            ScriptBlockExpressionWrapper scriptBlockExpressionWrapper)
                ScriptBlock scriptBlock = scriptBlockExpressionWrapper.GetScriptBlock(
                    context, functionDefinitionAst.IsFilter);
                var expAttribute = scriptBlock.ExperimentalAttribute;
                if (expAttribute == null || expAttribute.ToShow)
                    context.EngineSessionState.SetFunctionRaw(functionDefinitionAst.Name,
                        scriptBlock, context.EngineSessionState.CurrentScope.ScopeOrigin);
                if (exception is not RuntimeException rte)
                    throw ExceptionHandlingOps.ConvertToRuntimeException(exception, functionDefinitionAst.Extent);
                InterpreterError.UpdateExceptionErrorRecordPosition(rte, functionDefinitionAst.Extent);
    internal class ScriptBlockExpressionWrapper
        internal ScriptBlockExpressionWrapper(IParameterMetadataProvider ast)
        internal ScriptBlock GetScriptBlock(ExecutionContext context, bool isFilter)
            // We always clone the result, even when creating a new script block, so that the cached
            // value doesn't hold on to any session state.
            Diagnostics.Assert(_scriptBlock == null || _scriptBlock.SessionStateInternal == null,
                "Cached script block should not hold on to session state");
            var result = (_scriptBlock ??= new ScriptBlock(_ast, isFilter)).Clone();
            result.SessionStateInternal = context.EngineSessionState;
    internal static class ByRefOps
        /// There is no way to directly work with ByRef type in the expression tree, so we turn to reflection in this case.
        internal static object GetByRefPropertyValue(object target, PropertyInfo property)
            return property.GetValue(target);
    internal static class HashtableOps
        internal static void AddKeyValuePair(IDictionary hashtable, object key, object value, IScriptExtent errorExtent)
                throw InterpreterError.NewInterpreterException(hashtable, typeof(RuntimeException), errorExtent,
                                                               "InvalidNullKey", ParserStrings.InvalidNullKey);
            if (hashtable.Contains(key))
                // convert the key to a string for the error message, trimming if it's to long...
                // we pass a null context here because we're not too interested in $OFS.
                string errorKeyString = PSObject.ToStringParser(null, key);
                if (errorKeyString.Length > 40)
                    errorKeyString = errorKeyString.Substring(0, 40) + PSObjectHelper.Ellipsis;
                    "DuplicateKeyInHashLiteral", ParserStrings.DuplicateKeyInHashLiteral, errorKeyString);
        internal static object Add(IDictionary lvalDict, IDictionary rvalDict)
            IDictionary newDictionary;
            if (lvalDict is OrderedDictionary)
                // If the left is ordered, assume they want orderedness preserved.
                newDictionary = new OrderedDictionary(StringComparer.CurrentCultureIgnoreCase);
                newDictionary = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            // Add key and values from left hand side...
            foreach (object key in lvalDict.Keys)
                newDictionary.Add(key, lvalDict[key]);
            // and the right-hand side
            foreach (object key in rvalDict.Keys)
                newDictionary.Add(key, rvalDict[key]);
            return newDictionary;
    internal static class ExceptionHandlingOps
        internal class CatchAll { }
        /// Represent a handler search result.
        private sealed class HandlerSearchResult
            internal HandlerSearchResult()
                Handler = -1;
                Rank = int.MaxValue;
                ExceptionToPass = null;
                ErrorRecordToPass = null;
            internal int Handler;
            internal int Rank;
            internal Exception ExceptionToPass;
            internal ErrorRecord ErrorRecordToPass;
        /// Rank the exception types based on how specific they are.
        /// Smaller ranking number indicates more specific exception type.
        /// The ranking number for each type represent how many other
        /// types from the array derive from it.
        /// For example, 0 means no other types in the array derive from
        /// the corresponding type, while 3 means there are 3 other types
        /// in the array actually derive from the corresponding type.
        /// 'CatchAll' is considered to be derived by all exception types.
        private static int[] RankExceptionTypes(Type[] types)
            int[] ranks = new int[types.Length];
            int length = types.Length;
            // If 'CatchAll' is specified, it must be the last catch block.
            // Handle it specially. This can save a few iterations in the
            // 'for' loop below, and also avoid some type comparisons.
            if (types[length - 1].Equals(typeof(CatchAll)))
                ranks[length - 1] = length - 1;
                length -= 1;
            // For each type check if it's a sub-class of any types after it.
            // The ordering of the type array guarantees the more specific type comes first.
            for (int i = 0; i < length - 1; i++)
                for (int j = i + 1; j < length; j++)
                    if (types[i].IsSubclassOf(types[j]))
                        ranks[j]++;
            return ranks;
        /// Search for handler by the exception type and process the found result.
        private static void FindAndProcessHandler(Type[] types, int[] ranks,
                                                  HandlerSearchResult current,
                                                  ErrorRecord errorRecord)
            Diagnostics.Assert(current != null, "Caller makes sure 'current' is not null.");
            int handler = FindMatchingHandlerByType(exception.GetType(), types);
            // If no handler was found, return without changing the current result.
            if (handler == -1)
            // New handler was found.
            //  - If new-rank is less than current-rank -- meaning the new handler is more specific,
            //    then we update the current result with it.
            //  - If new-rank is more than current-rank -- meaning the new handler is less specific,
            //    then we do NOT change the current result.
            //  - If new-rank is equal to current-rank, we do NOT change the current result UNLESS the
            //    current handler is catch-all. (This is to keep the original behavior -- prefer to use
            //    the later found exception as the exception-to-pass-in if all exceptions result in the
            //    catch-all handler.
            int rank = ranks[handler];
            if (rank < current.Rank ||
                (rank == current.Rank && types[current.Handler].Equals(typeof(CatchAll)))
                current.Handler = handler;
                current.Rank = rank;
                current.ExceptionToPass = exception;
                current.ErrorRecordToPass = errorRecord;
        /// Find the matching handler for the caught exception.
        internal static int FindMatchingHandler(MutableTuple tuple, RuntimeException rte, Type[] types, ExecutionContext context)
            bool continueToSearch = false;
            int[] ranks = RankExceptionTypes(types);
            var current = new HandlerSearchResult();
                // Always assume no need to repeat the search for another iteration
                continueToSearch = false;
                // The 'ErrorRecord' of the current RuntimeException would be passed to $_
                ErrorRecord errorRecordToPass = rte.ErrorRecord;
                Exception inner = rte.InnerException;
                if (inner != null)
                    FindAndProcessHandler(types, ranks, current, inner, errorRecordToPass);
                // If no handler was found (rank = int.MaxValue), or if the handler we found was not
                // the most specific one, then look again, this time using the outer exception.
                // If we found a handler, but not one of the most specific ones (rank != 0), there may
                // be a more specific handler that catches outer but not inner exception.
                if (current.Rank > 0)
                    FindAndProcessHandler(types, ranks, current, rte, errorRecordToPass);
                // If we still didn't find one of the most specific handlers (rank != 0), we'll try unwrapping a few other of our exceptions:
                //     ActionPreferenceStopException - to cover '-ea stop'
                //         try { gci nosuchfile -ea stop } catch [System.Management.Automation.ItemNotFoundException] { 'caught' }
                //     CmdletInvocationException - to cover cmdlets like Invoke-Expression
                    var apse = rte as ActionPreferenceStopException;
                    if (apse != null)
                        var exceptionToPass = apse.ErrorRecord.Exception;
                        // If it's again a RuntimeException, we repeat the search using it
                        rte = exceptionToPass as RuntimeException;
                        if (rte != null)
                            continueToSearch = true;
                        else if (exceptionToPass != null)
                            FindAndProcessHandler(types, ranks, current, exceptionToPass, errorRecordToPass);
                    else if (rte is CmdletInvocationException && inner != null)
                        if (inner.InnerException != null)
                            FindAndProcessHandler(types, ranks, current, inner.InnerException, errorRecordToPass);
            } while (continueToSearch);
            if (current.Handler != -1)
                var errorRecord = new ErrorRecord(current.ErrorRecordToPass, current.ExceptionToPass);
                tuple.SetAutomaticVariable(AutomaticVariable.Underbar, errorRecord, context);
            return current.Handler;
        /// Find the matching handler by the exception type.
        private static int FindMatchingHandlerByType(Type exceptionType, Type[] types)
            // pass 1 - exact match (this pass isn't needed for catch handlers because the ordering
            // guarantees more specific handlers come first.)
            for (i = 0; i < types.Length; ++i)
                if (exceptionType.Equals(types[i]))
            // pass 2 - subclass
                if (exceptionType.IsSubclassOf(types[i]))
            // pass 3 - untyped catchall handler...
            //   if there is more than one (can only happen with traps), return the first.
            //   it might be nice to enforce a single default in strict mode.
                if (types[i].Equals(typeof(CatchAll)))
        internal static bool SuspendStoppingPipeline(ExecutionContext context)
            var localPipeline = (LocalPipeline)context.CurrentRunspace.GetCurrentlyRunningPipeline();
            return SuspendStoppingPipelineImpl(localPipeline);
        internal static void RestoreStoppingPipeline(ExecutionContext context, bool oldIsStopping)
            RestoreStoppingPipelineImpl(localPipeline, oldIsStopping);
        internal static bool SuspendStoppingPipelineImpl(LocalPipeline localPipeline)
            if (localPipeline is not null)
                bool oldIsStopping = localPipeline.Stopper.IsStopping;
                localPipeline.Stopper.IsStopping = false;
                return oldIsStopping;
        internal static void RestoreStoppingPipelineImpl(LocalPipeline localPipeline, bool oldIsStopping)
                localPipeline.Stopper.IsStopping = oldIsStopping;
        internal static void CheckActionPreference(FunctionContext funcContext, Exception exception)
            if (exception is TargetInvocationException)
                // Always unwrap TargetInvocationException.
            var rte = exception as RuntimeException;
            if (rte == null)
                rte = ConvertToRuntimeException(exception, funcContext.CurrentPosition);
                InterpreterError.UpdateExceptionErrorRecordPosition(rte, funcContext.CurrentPosition);
            // Update the history id if needed to associate the exception with the right history item.
            InterpreterError.UpdateExceptionErrorRecordHistoryId(rte, funcContext._executionContext);
            var outputPipe = funcContext._outputPipe;
            var extent = rte.ErrorRecord.InvocationInfo.ScriptPosition;
            SetErrorVariables(extent, rte, context, outputPipe);
            // set $? to false indicating an error
            context.QuestionMarkVariableValue = false;
            ActionPreference preference = GetErrorActionPreference(context);
            // If the exception was not rethrown and we are not currently
            // handling an exception, then the exception is new, and we
            // can break on it if requested.
            if (!rte.WasRethrown &&
                context.CurrentExceptionBeingHandled == null &&
                preference == ActionPreference.Break)
                context.Debugger?.Break(rte);
            // Item2 in the trap tuples is the action (script) for the trap.
            // A null action script is only used to indicate when exceptions
            // should be thrown up to a higher level, and doesn't count as an
            // actual trap handler in the function context.
            bool anyTrapHandlers = funcContext._traps.Count > 0 && funcContext._traps[funcContext._traps.Count - 1].Item2 != null;
            if (anyTrapHandlers)
                // update the action preference according to how the exception is
                // handled in the trap statement(s).
                preference = ProcessTraps(funcContext, rte);
            else if (ExceptionCannotBeStoppedContinuedOrIgnored(rte, context))
            else if (preference == ActionPreference.Inquire && !rte.SuppressPromptInInterpreter)
                preference = InquireForActionPreference(rte.Message, context);
            if ((preference == ActionPreference.SilentlyContinue) ||
                (preference == ActionPreference.Ignore))
            if (preference == ActionPreference.Stop)
                // The interpreter prompt CommandBaseStrings:InquireHalt
                // should be suppressed when this flag is set.  This will be set
                // when this prompt has already occurred and Break was chosen,
                // or for ActionPreferenceStopException in all cases.
                rte.SuppressPromptInInterpreter = true;
            if (!anyTrapHandlers && rte.WasThrownFromThrowStatement)
            if (!ReportErrorRecord(extent, rte, context))
        private static ActionPreference ProcessTraps(FunctionContext funcContext,
                                                     RuntimeException rte)
            int handler = -1;
            var types = funcContext._traps.Last().Item1;
            var handlers = funcContext._traps.Last().Item2;
                handler = FindMatchingHandlerByType(inner.GetType(), types);
                exception = inner;
            // If no handler was found, or if the handler we found was the catch all handler,
            // then look again, this time using the outer exception.  If, when looking with the inner,
            // we found the catch all, there may be a handler that catches outer but not inner.
            if (handler == -1 || types[handler].Equals(typeof(CatchAll)))
                int outerHandler = FindMatchingHandlerByType(rte.GetType(), types);
                if (outerHandler != handler)
                    handler = outerHandler;
                    exception = rte;
            if (handler != -1)
                Diagnostics.Assert(exception != null, "Exception object can't be null.");
                    ErrorRecord err = rte.ErrorRecord;
                    // CurrentCommandProcessor is normally not null, but it is null
                    // when executing some unit tests through reflection.
                    context.CurrentCommandProcessor?.ForgetScriptException();
                        // Invoke the trap statement body, passing in the exception...
                        var locals = MutableTuple.MakeTuple(funcContext._traps.Last().Item3[handler], Compiler.DottedLocalsNameIndexMap);
                        // Copy automatic variables into the new scope (not necessarily required because dynamic scoping
                        // would find them in the parent scope, but internal code might avoid dynamic lookup so copy
                        // to be safe.
                        Diagnostics.Assert(AutomaticVariable.Underbar == 0, "Code below relies on this assertion being true.");
                        locals.SetAutomaticVariable(AutomaticVariable.Underbar, new ErrorRecord(err, exception), context);
                        for (int i = 1; i < (int)AutomaticVariable.NumberOfAutomaticVariables; ++i)
                            locals.SetValue(i, funcContext._localsTuple.GetValue(i));
                        var trapFuncContext = new FunctionContext
                            _file = funcContext._file,
                            _scriptBlock = funcContext._scriptBlock,
                            _sequencePoints = funcContext._sequencePoints,
                            _debuggerHidden = funcContext._debuggerHidden,
                            _debuggerStepThrough = funcContext._debuggerStepThrough,
                            _executionContext = funcContext._executionContext,
                            _boundBreakpoints = funcContext._boundBreakpoints,
                            _outputPipe = funcContext._outputPipe,
                            _breakPoints = funcContext._breakPoints,
                        handlers[handler](trapFuncContext);
                    return ExceptionHandlingOps.QueryForAction(rte, exception.Message, context);
                catch (ContinueException)
                    // Just continue on to the next statement.
                    // Terminate this block of statements.
                    return ActionPreference.Stop;
                    // The questionmark variable will always be false when we process a trap, so
                    // set it to false to ensure it didn't change as a result of anything done
                    // inside the trap
        /// Gets the current error action preference value.
        /// <param name="context">The execution context.</param>
        /// <returns>The preference the user selected.</returns>
        /// Error action is decided by error action preference. If preference is inquire, we will
        /// prompt user for their preference.
        internal static ActionPreference GetErrorActionPreference(ExecutionContext context)
            return context.GetEnumPreference(
                ActionPreference.Continue,
        /// Determine if we should continue or not after an error or exception.
        /// <param name="rte">The RuntimeException which was reported.</param>
        /// <param name="message">The message to display.</param>
        internal static ActionPreference QueryForAction(RuntimeException rte, string message, ExecutionContext context)
            // 906264 "$ErrorActionPreference="Inquire" prevents original non-terminating error from being reported to $error"
            ActionPreference preference =
                context.GetEnumPreference(
            if (preference != ActionPreference.Inquire || rte.SuppressPromptInInterpreter)
            return InquireForActionPreference(message, context);
        /// This is a helper function for prompting for user preference.
        /// This method will allow user to enter suspend mode.
        internal static ActionPreference InquireForActionPreference(string message, ExecutionContext context)
            InternalHostUserInterface ui = (InternalHostUserInterface)context.EngineHostInterface.UI;
            string continueLabel = ParserStrings.ContinueLabel;
            string continueHelpMsg = ParserStrings.ContinueHelpMessage;
            string silentlyContinueLabel = ParserStrings.SilentlyContinueLabel;
            string silentlyContinueHelpMsg = ParserStrings.SilentlyContinueHelpMessage;
            string breakLabel = ParserStrings.BreakLabel;
            string breakHelpMsg = ParserStrings.BreakHelpMessage;
            string suspendLabel = ParserStrings.SuspendLabel;
            string suspendHelpMsg = StringUtil.Format(ParserStrings.SuspendHelpMessage);
            choices.Add(new ChoiceDescription(continueLabel, continueHelpMsg));
            choices.Add(new ChoiceDescription(silentlyContinueLabel, silentlyContinueHelpMsg));
            choices.Add(new ChoiceDescription(breakLabel, breakHelpMsg));
            choices.Add(new ChoiceDescription(suspendLabel, suspendHelpMsg));
            string caption = ParserStrings.ExceptionActionPromptCaption;
            bool oldQuestionMarkVariableValue = context.QuestionMarkVariableValue;
            int choice;
            while ((choice = ui.PromptForChoice(caption, message, choices, 0)) == 3)
                context.EngineHostInterface.EnterNestedPrompt();
            context.QuestionMarkVariableValue = oldQuestionMarkVariableValue;
            if (choice == 0)
            if (choice == 1)
        /// Set error variables like $error and $stacktrace.
        /// <param name="rte"></param>
        /// <param name="outputPipe">The output pipe of the statement.</param>
        internal static void SetErrorVariables(IScriptExtent extent, RuntimeException rte, ExecutionContext context, Pipe outputPipe)
            string stack = null;
            Exception e = rte;
            while (e != null && i++ < 10)
                if (!string.IsNullOrEmpty(e.StackTrace))
                    stack = e.StackTrace;
            context.SetVariable(SpecialVariables.StackTraceVarPath, stack);
            Diagnostics.Assert(rte.ErrorRecord != null, "The runtime exception's error record was null");
            InterpreterError.UpdateExceptionErrorRecordPosition(rte, extent);
            ErrorRecord errRec = rte.ErrorRecord.WrapException(rte);
            if (rte is not PipelineStoppedException)
                outputPipe?.AppendVariableList(VariableStreamKind.Error, errRec);
                context.AppendDollarError(errRec);
        internal static bool ExceptionCannotBeStoppedContinuedOrIgnored(RuntimeException rte, ExecutionContext context)
            return context.PropagateExceptionsToEnclosingStatementBlock
                   || context.ShellFunctionErrorOutputPipe == null
                   || context.CurrentPipelineStopping
                   || rte.SuppressPromptInInterpreter
                   || rte is PipelineStoppedException;
        /// Report error into error pipe.
        /// <param name="rte">The runtime error to report.</param>
        /// <returns>True if it was able to report the error.</returns>
        internal static bool ReportErrorRecord(IScriptExtent extent, RuntimeException rte, ExecutionContext context)
            if (context.ShellFunctionErrorOutputPipe == null)
            if (rte.ErrorRecord.InvocationInfo == null && extent != null && extent != PositionUtilities.EmptyExtent)
                rte.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, extent, context));
            PSObject errorWrap = PSObject.AsPSObject(new ErrorRecord(rte.ErrorRecord, rte));
            // If this is an error pipe for a hosting application (i.e.: no downstream cmdlet),
            // and we are logging, then create a temporary PowerShell to log the error.
            if (context.InternalHost.UI.IsTranscribing)
                context.InternalHost.UI.TranscribeError(context, rte.ErrorRecord.InvocationInfo, errorWrap);
            context.ShellFunctionErrorOutputPipe.Add(errorWrap);
            // set the value of $? here in case it is reset in error reporting.
        internal static RuntimeException ConvertToException(object result, IScriptExtent extent, bool rethrow)
            result = PSObject.Base(result);
            RuntimeException runtimeException = result as RuntimeException;
            if (runtimeException != null)
                InterpreterError.UpdateExceptionErrorRecordPosition(runtimeException, extent);
                runtimeException.WasRethrown = rethrow;
            ErrorRecord er = result as ErrorRecord;
                runtimeException = new RuntimeException(er.ToString(), er.Exception, er) { WasThrownFromThrowStatement = true, WasRethrown = rethrow };
            Exception exception = result as Exception;
                er = new ErrorRecord(exception, exception.Message, ErrorCategory.OperationStopped, null);
                runtimeException = new RuntimeException(exception.Message, exception, er) { WasThrownFromThrowStatement = true, WasRethrown = rethrow };
            string message = LanguagePrimitives.IsNull(result)
                ? "ScriptHalted"
                : ParserOps.ConvertTo<string>(result, PositionUtilities.EmptyExtent);
            exception = new RuntimeException(message, null);
            er = new ErrorRecord(exception, message, ErrorCategory.OperationStopped, null);
            runtimeException = new RuntimeException(message, exception, er) { WasThrownFromThrowStatement = true, WasRethrown = rethrow };
            runtimeException.SetTargetObject(result);
        internal static RuntimeException ConvertToRuntimeException(Exception exception, IScriptExtent extent)
            RuntimeException runtimeException = exception as RuntimeException;
                var er = icer != null
                             : new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.OperationStopped, null);
                runtimeException = new RuntimeException(exception.Message, exception, er);
        internal static void ConvertToArgumentConversionException(Exception exception, string parameterName, object argument, string method, Type toType)
                "MethodArgumentConversionInvalidCastArgument", exception,
                ExtendedTypeSystem.MethodArgumentConversionException, parameterName, argument, method, toType, exception.Message);
        internal static void ConvertToMethodInvocationException(Exception exception, Type typeToThrow, string methodName, int numArgs, MemberInfo memberInfo = null)
            // Win8: 178063. Allow flow control related exceptions for PowerShell hosting API
            if ((exception is FlowControlException ||
                exception is ScriptCallDepthException ||
                exception is PipelineStoppedException) &&
                ((memberInfo == null) || ((memberInfo.DeclaringType != typeof(PowerShell)) && (memberInfo.DeclaringType != typeof(Pipeline)))))
            if (typeToThrow == typeof(MethodException))
                if (exception is MethodException)
                        exception.GetType().Name,
                        methodName, numArgs, exception.Message);
            if (methodName.StartsWith("set_", StringComparison.Ordinal) || methodName.StartsWith("get_", StringComparison.Ordinal))
                methodName = methodName.Substring(4);
            if (typeToThrow == typeof(GetValueInvocationException))
                if (exception is GetValueException)
                    "ExceptionWhenGetting",
                    methodName, exception.Message);
            Diagnostics.Assert(typeToThrow == typeof(SetValueInvocationException),
                               "caller to verify exception is expected type");
            if (exception is SetValueException)
                "ExceptionWhenSetting",
    internal static class TypeOps
        internal static Type ResolveTypeName(ITypeName typeName, IScriptExtent errorPos)
            var result = TypeResolver.ResolveITypeName(typeName, out exception);
                    if (exception is InvalidCastException &&
                        exception.InnerException != null &&
                        exception.InnerException is TypeResolver.AmbiguousTypeException)
                    throw InterpreterError.NewInterpreterException(typeName, typeof(RuntimeException), errorPos,
                                                                   "TypeNotFoundWithMessage",
                                                                   ParserStrings.TypeNotFoundWithMessage,
                                                                   typeName.FullName, exception.Message);
                // For better error messages, figure out exactly which type we couldn't resolve.
                // We recurse and relying on one of the recursive calls to throw, or if none do,
                // then we just throw on the top level typeName.
                var genericTypeName = typeName as GenericTypeName;
                    var generic = genericTypeName.GetGenericType(ResolveTypeName(genericTypeName.TypeName, errorPos));
                    var typeArgs = (from arg in genericTypeName.GenericArguments select ResolveTypeName(arg, errorPos)).ToArray();
                            generic.MakeGenericType(typeArgs);
                                                                       typeName.FullName, e.Message);
                var arrayTypeName = typeName as ArrayTypeName;
                    ResolveTypeName(arrayTypeName.ElementType, errorPos);
                                                               "TypeNotFound", ParserStrings.TypeNotFound,
                                                               typeName.FullName);
        internal static bool IsInstance(object left, object right)
                rType = ParserOps.ConvertTo<Type>(rval, null);
                        null, "IsOperatorRequiresType", ParserStrings.IsOperatorRequiresType);
            return rType.IsInstanceOfType(lval);
        internal static object AsOperator(object left, Type type)
                                                               "AsOperatorRequiresType", ParserStrings.AsOperatorRequiresType);
            // We figure out the exception instead of just executing a conversion because we can avoid an exception which is quite expensive,
            // and people using -as don't expect it to be expensive.
            // ConstrainedLanguage note - Calls to this conversion are done at runtime, so conversions are not cached.
            var conversion = LanguagePrimitives.FigureConversion(left, type, out debase);
                    return conversion.Invoke(PSObject.Base(left), type, false, (PSObject)left,
                        NumberFormatInfo.InvariantInfo, null);
                return conversion.Invoke(left, type, false, null, NumberFormatInfo.InvariantInfo, null);
        internal static string[] GetNamespacesForTypeResolutionState(IEnumerable<UsingStatementAst> usingAsts)
            var usedSystem = false;
            var namespaces = new List<string>();
                if (usingStmt.UsingStatementKind == UsingStatementKind.Namespace)
                    if (!usedSystem && usingStmt.Name.Value.Equals("System", StringComparison.OrdinalIgnoreCase))
                        usedSystem = true;
                    namespaces.Add(usingStmt.Name.Value);
            if (!usedSystem)
                namespaces.Insert(0, "System");
            return namespaces.ToArray();
        /// Add types to the current scope.
        /// This method called at runtime after types are created at compile time.
        /// This method should be called for every ScriptBlockAst that defines types.
        /// I.e.
        /// class C1 {}
        /// function foo { class C2 {} }
        /// 1..10 | ForEach-Object { foo }
        /// DefinePowerShellTypes() would be called for two TypeDefinitionAsts at the same time and Types for C1 and C2 would be created at the same assembly.
        /// AddPowerShellTypesToTheScope() would be called for root script first and then for foo\C2, once we call function foo.
        /// Note that AddPowerShellTypesToTheScope() would be call on every foo call, 10 times.
        /// This method also should be called for 'using module' statements. Then added types would have a different name.
        internal static void AddPowerShellTypesToTheScope(Dictionary<string, TypeDefinitionAst> types, ExecutionContext context)
            var trs = context.EngineSessionState.CurrentScope.TypeResolutionState;
                Diagnostics.Assert(t.Value.Type != null, "TypeDefinitionAst.Type cannot be null");
                context.EngineSessionState.CurrentScope.AddType(t.Key, t.Value.Type);
            context.EngineSessionState.CurrentScope.TypeResolutionState = trs.CloneWithAddTypesDefined(types.Keys);
        /// Capture session state for methods defined in PowerShell types, so they know what context to use.
        internal static void InitPowerShellTypesAtRuntime(TypeDefinitionAst[] types)
                Diagnostics.Assert(t.Type != null, "TypeDefinitionAst.Type cannot be null");
                if (t.IsClass)
                    if (t.Type.IsDefined(typeof(NoRunspaceAffinityAttribute), inherit: true))
                        // Skip the initialization for session state affinity.
                    var helperType = t.Type.Assembly.GetType(t.Type.FullName + "_<staticHelpers>");
                    Diagnostics.Assert(helperType != null, "no corresponding " + t.Type.FullName + "_<staticHelpers> type found");
                    foreach (var p in helperType.GetFields(BindingFlags.Static | BindingFlags.NonPublic))
                        var field = p.GetValue(null);
                        // field can be one of two types: SessionStateKeeper or ScriptBlockMemberMethodWrapper
                        var methodWrapper = field as ScriptBlockMemberMethodWrapper;
                        if (methodWrapper != null)
                            methodWrapper.InitAtRuntime();
                            ((SessionStateKeeper)field).RegisterRunspace();
        internal static void SetCurrentTypeResolutionState(TypeResolutionState trs, ExecutionContext context)
            context.EngineSessionState.CurrentScope.TypeResolutionState = trs;
        internal static void SetAssemblyDefiningPSTypes(FunctionContext functionContext, Assembly assembly)
            functionContext._scriptBlock.AssemblyDefiningPSTypes = assembly;
    internal static class SwitchOps
        internal static bool ConditionSatisfiedWildcard(bool caseSensitive,
                                                        object condition,
                                                        string str,
            WildcardPattern wildcard = condition as WildcardPattern;
            if (wildcard != null)
                // If case sensitivity doesn't agree between the existing wildcard pattern and the switch mode,
                // make a new wildcard pattern that agrees with the switch.
                if (((wildcard.Options & WildcardOptions.IgnoreCase) == 0) != caseSensitive)
                    WildcardOptions options = caseSensitive ? WildcardOptions.None : WildcardOptions.IgnoreCase;
                    wildcard = WildcardPattern.Get(wildcard.Pattern, options);
                wildcard = WildcardPattern.Get(PSObject.ToStringParser(context, condition), options);
            return wildcard.IsMatch(str);
        internal static bool ConditionSatisfiedRegex(bool caseSensitive,
            RegexOptions options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                Match m;
                Regex regex = condition as Regex;
                // Check if the regex agrees with the switch w.r.t. case sensitivity, if not,
                // we must build a new regex.
                if (regex != null && (((regex.Options & RegexOptions.IgnoreCase) != 0) != caseSensitive))
                    m = regex.Match(str);
                    pattern = PSObject.ToStringParser(context, condition);
                    m = Regex.Match(str, pattern, options);
                    if (m.Success && m.Groups.Count > 0)
                        // We used the static regex method for it's caching ability, but
                        // we need the group names now.  Fortunately constructing another regex
                        // isn't slow because it should be in the cache still.
                        regex = new Regex(pattern, options);
                        Diagnostics.Assert(regex != null, "Logic above ensures regex is not null.");
                        foreach (string groupName in regex.GetGroupNames())
                return m.Success;
                // ErrorSkipping: Add this error to parser
                    errorPosition, "InvalidRegularExpression", ParserStrings.InvalidRegularExpression, ae, pattern);
        internal static string ResolveFilePath(IScriptExtent errorExtent, object obj, ExecutionContext context)
                FileInfo file = obj as FileInfo;
                string filePath = file != null ? file.FullName : PSObject.ToStringParser(context, obj);
                    throw InterpreterError.NewInterpreterException(filePath,
                        typeof(RuntimeException), errorExtent, "InvalidFilenameOption", ParserStrings.InvalidFilenameOption);
                SessionState sessionState = new SessionState(context.EngineSessionState);
                    throw InterpreterError.NewInterpreterException(filePath, typeof(RuntimeException), errorExtent,
                                                                   "FileOpenError", ParserStrings.FileOpenError,
                    // "No files matching '{0}' were found.."
                                                                   "FileNotFound", ParserStrings.FileNotFound, filePath);
                    throw InterpreterError.NewInterpreterException(filePaths, typeof(RuntimeException), errorExtent,
                                                                   "AmbiguousPath", ParserStrings.AmbiguousPath);
                // Add the invocation info to this command...
                if (rte.ErrorRecord != null && rte.ErrorRecord.InvocationInfo == null)
                    rte.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorExtent, context));
    /// Controls the matching behaviour of the Where() operator.
    public enum WhereOperatorSelectionMode
        /// Return all matches.
        /// Stop processing after the first match.
        First = 1,
        /// Return the last matching element.
        Last = 2,       // return last match
        /// Skip until the condition is true, then return the rest.
        SkipUntil = 3,
        /// Return elements until the condition is true then skip the rest.
        Until = 4,
        /// Return an array of two elements, first index is matched elements, second index is the remaining elements.
        Split = 5,
    internal static class EnumerableOps
        /// Implements the Where(expression) operation on collections.
        /// <param name="enumerator">The enumerator over the collection to search.</param>
        /// <param name="expressionSB">
        /// A ScriptBlock where its result is treated as a boolean, or null to
        /// return all collection objects with WhereOperatorSelectionMode.
        /// <param name="selectionMode">
        /// Sets the WhereOperatorSelectionMode for operator, defaults to All.
        /// This is of type object to allow either enum values or strings to be passed.
        /// <param name="numberToReturn">The number of elements to return.</param>
        internal static object Where(IEnumerator enumerator, ScriptBlock expressionSB, WhereOperatorSelectionMode selectionMode, int numberToReturn)
            Diagnostics.Assert(enumerator != null, "The Where() operator should never receive a null enumerator value from the runtime.");
            if (numberToReturn < 0)
                throw new ArgumentOutOfRangeException(nameof(numberToReturn), numberToReturn, ParserStrings.NumberToReturnMustBeGreaterThanZero);
            // Optimization to speed up the case where there is no condition expression
            // Useful when using selection mode and number to return to do fast list
            // slicing.
            if (expressionSB == null)
                if (selectionMode == WhereOperatorSelectionMode.Default)
                    throw new InvalidOperationException(ParserStrings.EmptyExpressionRequiresANonDefaultMode);
                var rest = new List<object>();
                object current = null;
                if (numberToReturn == 0)
                    numberToReturn = 1;
                // Skip the first N elements and return the rest
                if (selectionMode == WhereOperatorSelectionMode.SkipUntil)
                    while (index < numberToReturn && MoveNext(null, enumerator))
                    while (MoveNext(context, enumerator))
                        rest.Add(Current(enumerator));
                    return rest.ToArray();
                // Return the last N elements
                if (selectionMode == WhereOperatorSelectionMode.Last)
                        current = Current(enumerator);
                        if (numberToReturn > 1)
                            rest.Add(current);
                            if (rest.Count > numberToReturn)
                                rest.RemoveAt(0);
                    if (numberToReturn == 1)
                        return new object[] { current };
                object[] first = new object[numberToReturn];
                    first[index++] = current;
                    if (index >= numberToReturn)
                        // Return the first N elements
                        if (selectionMode == WhereOperatorSelectionMode.First || selectionMode == WhereOperatorSelectionMode.Until)
                // Return a array of two elements, the first element is the first N elements,
                // the second element is the remainder of the input
                if (selectionMode == WhereOperatorSelectionMode.Split)
                        var e = Current(enumerator);
                        rest.Add(e);
                    return new object[] { first, rest.ToArray() };
            Collection<PSObject> matches = new Collection<PSObject>();
            Collection<PSObject> notMatched = null;
                notMatched = new Collection<PSObject>();
            var resultCollection = new List<object>();
            Pipe outputPipe = new Pipe(resultCollection);
            bool returnTheRest = false;
                var ie = Current(enumerator);
                if (returnTheRest)
                    matches.Add(ie == null ? null : PSObject.AsPSObject(ie));
                    if (numberToReturn > 0 && matches.Count >= numberToReturn)
                resultCollection.Clear();
                expressionSB.InvokeWithPipeImpl(false, null, null, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, ie, AutomationNull.Value, AutomationNull.Value, outputPipe, null);
                bool elementMatched = LanguagePrimitives.IsTrue(resultCollection);
                if (elementMatched)
                    if (selectionMode == WhereOperatorSelectionMode.Until)
                    else if (selectionMode == WhereOperatorSelectionMode.Last)
                        if (matches.Count < numberToReturn)
                                matches[0] = ie == null ? null : PSObject.AsPSObject(ie);
                                // Maintains a sliding window
                                matches.RemoveAt(0);
                    else if (selectionMode == WhereOperatorSelectionMode.SkipUntil)
                        returnTheRest = true;
                    if (selectionMode != WhereOperatorSelectionMode.Last)
                        if (numberToReturn == 0 && selectionMode == WhereOperatorSelectionMode.First)
                        // If number to return is not 0, First and Any have identical behaviour
                        if (numberToReturn != 0 && numberToReturn == matches.Count)
                else if (selectionMode == WhereOperatorSelectionMode.Until)
                    // no match so in the until case, we add the value until the count is reached
                else if (selectionMode == WhereOperatorSelectionMode.Split)
                    // If in split mode, record both matched and noteMatched elements.
                    notMatched.Add(ie == null ? null : PSObject.AsPSObject(ie));
            // If split was specified, return both sets of objects
                // We may have stopped looping before processing the whole collection because
                // reached the max number of matching elements to return. In that case,
                // add remaining elements to the notMatched collection.
                return new object[] { matches, notMatched };
        /// Implements the ForEach() operator.
        /// <param name="enumerator">The collection to operate over.</param>
        /// <returns>An object array containing the results of the expression evaluation.</returns>
        internal static object ForEach(IEnumerator enumerator, object expression, object[] arguments)
            Diagnostics.Assert(enumerator != null, "The ForEach() operator should never receive a null enumerator value from the runtime.");
            Diagnostics.Assert(arguments != null, "The ForEach() operator should never receive a null value for the 'arguments' parameter from the runtime.");
            ArgumentNullException.ThrowIfNull(expression);
            // If expression argument is a .Net type then convert the collection to that type
            // if the target type is a collection or array, then the result will be a collection of exactly
            // that type. If the target type is not a collection type then return a generic collection of that type.
            Type targetType = expression as Type;
                dynamic resultCollection = null;
                if (targetType.GetInterface("System.Collections.ICollection") != null)
                    // If the target type is an array, accumulate all the elements
                    // then use the PowerShell type converter to turn it into an array
                    // of the correct type.
                    if (targetType.IsArray)
                        var list = new List<object>();
                        while (MoveNext(null, enumerator))
                            object current = Current(enumerator);
                            list.Add(current);
                        return LanguagePrimitives.ConvertTo(list, targetType, CultureInfo.InvariantCulture);
                    // If it's a generic type then make sure it only has one type argument
                    if (targetType.IsGenericType)
                        Type[] ta = targetType.GetGenericArguments();
                        if (ta.Length != 1)
                                null, "ForEachBadGenericConversionTypeSpecified", ParserStrings.ForEachBadGenericConversionTypeSpecified, ParserOps.ConvertTo<string>(targetType, null));
                        resultCollection = PSObject.AsPSObject(Activator.CreateInstance(targetType));
                            // Let the PSObject method invocation mechanism take care of
                            // any required conversions, etc.
                            resultCollection.Add(current);
                    // Target is not a collection so return a Collection<targetType>
                    Type resultCollectionType = typeof(Collection<>).MakeGenericType(targetType);
                    resultCollection = PSObject.AsPSObject(Activator.CreateInstance(resultCollectionType));
                if (resultCollection == null)
                        null, "ForEachTypeConversionFailed", ParserStrings.ForEachTypeConversionFailed, ParserOps.ConvertTo<string>(targetType, null));
                return resultCollection;
            // If the expression is a script block, it will be executed in the current scope
            // once on each element.
            var result = new Collection<PSObject>();
            ScriptBlock sb = expression as ScriptBlock;
                if (sb.HasCleanBlock)
                    throw new PSNotSupportedException(ParserStrings.ForEachNotSupportCleanBlock);
                if (sb.HasBeginBlock)
                    sb.InvokeWithPipeImpl(ScriptBlockClauseToInvoke.Begin, false, null, null, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, AutomationNull.Value, AutomationNull.Value, outputPipe, null, arguments);
                ScriptBlockClauseToInvoke processClause = (sb.HasProcessBlock) ? ScriptBlockClauseToInvoke.Process : ScriptBlockClauseToInvoke.End;
                object ie = null;
                    ie = Current(enumerator);
                    if (ie != AutomationNull.Value)
                        sb.InvokeWithPipeImpl(processClause, false, null, null, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, ie, AutomationNull.Value, AutomationNull.Value, outputPipe, null, arguments);
                if (processClause == ScriptBlockClauseToInvoke.Process && sb.HasEndBlock)
                    // $_ has the same value as it did in the last iteration of the process loop
                    sb.InvokeWithPipeImpl(ScriptBlockClauseToInvoke.End, false, null, null, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, ie, AutomationNull.Value, AutomationNull.Value, outputPipe, null, arguments);
                // Deal with member gets, sets and invokes
                string name = ParserOps.ConvertTo<string>(expression, null);
                var numArgs = arguments.Length;
                    object basedCurrent = PSObject.Base(current);
                    Hashtable ht = basedCurrent as Hashtable;
                    if (ht != null)
                        // special case hashtables since we don't want to hit a method name
                        switch (numArgs)
                                // No args so do a set
                                object element = ht[name];
                                result.Add(element != null ? PSObject.AsPSObject(element) : null);
                                // 1 args so set as a scalar
                                ht[name] = arguments[0];
                                // more than one arg, just assign as is
                                ht[name] = arguments;
                        // handle the null case with PowerShell semantics:
                        // - retrieving a property on null adds a null to the result set
                        // - setting a property on null or trying to invoke a method is an error
                            if (arguments.Length == 0)
                                result.Add(null);
                                var nullRefException = new NullReferenceException();
                                    nullRefException.GetType().Name,
                                    nullRefException,
                                    name, arguments.Length, nullRefException.Message);
                        var ie = PSObject.AsPSObject(current);
                            PSMemberInfo member = ie.Members[name];
                            // If the property was not found, check strict mode...
                                        null, "PropertyNotFoundStrict", ParserStrings.PropertyNotFoundStrict, name);
                                if (numArgs == 0)
                                    throw InterpreterError.NewInterpreterException(ie, typeof(NullReferenceException), null, "ForEachNonexistentMemberReference",
                                        ParserStrings.ForEachNonexistentMemberReference, name);
                            var method = member as PSMethodInfo;
                                // It's a method so check language modes to see if it's allowed.
                                if (languageMode == PSLanguageMode.RestrictedLanguage)
                                    throw InterpreterError.NewInterpreterException(current, typeof(PSInvalidOperationException),
                                        null, "NoMethodInvocationInRestrictedLanguageMode", InternalCommandStrings.NoMethodInvocationInRestrictedLanguageMode);
                                // In constrained language mode, can only execute methods on certain types.
                                if (languageMode == PSLanguageMode.ConstrainedLanguage)
                                    if (!CoreTypes.Contains(basedCurrent.GetType()))
                                                null, "MethodInvocationNotSupportedInConstrainedLanguage", ParserStrings.InvokeMethodConstrainedLanguage);
                                            title: ParserStrings.WDACParserForEachOperatorLogTitle,
                                            message: StringUtil.Format(ParserStrings.WDACParserForEachOperatorLogMessage, method.Name ?? string.Empty),
                                            fqid: "ForEachOperatorMethodInvocationNotAllowed",
                                result.Add(PSObject.AsPSObject(method.Invoke(arguments)));
                                var property = member as PSPropertyInfo;
                                        // No args: do a get
                                        result.Add(PSObject.AsPSObject(property.Value));
                                        // 1 arg: set as a scalar
                                        property.Value = arguments[0];
                                        property.Value = arguments;
        internal static object SlicingIndex(object target, IEnumerator indexes, Func<object, object, object> indexer)
            var fakeEnumerator = indexes as NonEnumerableObjectEnumerator;
            if (fakeEnumerator != null)
                // We have a non-enumerable object, we're trying to slice index with it.  It really should have
                // been a single index, so we don't want to return an array, we just want to return the indexed value.
                return indexer(target, fakeEnumerator.GetNonEnumerableObject());
            while (MoveNext(null, indexes))
                var value = indexer(target, Current(indexes));
                    result.Add(value);
        private static void FlattenResults(object o, List<object> result)
            var e = LanguagePrimitives.GetEnumerator(o);
                    o = e.Current;
                    if (o != AutomationNull.Value)
                        result.Add(o);
        private static void PropertyGetterWorker(CallSite<Func<CallSite, object, object>> getMemberBinderSite,
                                                 IEnumerator enumerator,
                                                 List<object> result)
                var current = Current(enumerator);
                var o = getMemberBinderSite.Target.Invoke(getMemberBinderSite, current);
                    FlattenResults(o, result);
                    // Recurse through collections if current didn't have the property.
                    var nestedEnumerator = LanguagePrimitives.GetEnumerator(current);
                    if (nestedEnumerator != null)
                        PropertyGetterWorker(getMemberBinderSite, nestedEnumerator, context, result);
        internal static object PropertyGetter(PSGetMemberBinder binder, IEnumerator enumerator)
            var getMemberBinderSite = CallSite<Func<CallSite, object, object>>.Create(binder);
            PropertyGetterWorker(getMemberBinderSite, enumerator, context, result);
                return result[0];
                        null, "PropertyNotFoundStrict", ParserStrings.PropertyNotFoundStrict, binder.Name);
        private static void MethodInvokerWorker(CallSite invokeMemberSite,
                                                List<object> result,
                                                ref bool foundMethod)
                    // The following 2 lines contain quite a bit of magic.  We know that invokeMemberSite is a CallSite,
                    // but we don't know the exact delegate type so we can't use the usual code site.Target.Invoke.
                    // The Target could be an unbounded number of different types - but we do know is that it will have
                    // a delegate member named Target and we want to invoke that delegate.
                    // We do know it will have a signature like:
                    //     Func<CallSite, object, <unknown number of argument types>, object>
                    // Because we don't know the number of arguments, we can use DynamicInvoke to call the delegate.
                    dynamic site = invokeMemberSite;
                    object o = site.Target.DynamicInvoke(args.Prepend(current).Prepend(invokeMemberSite).ToArray());
                    // If we get here, we successfully called one method, so set the flag so we don't report a MissingMethodException.
                    // If there was a method, but it raised an exception, it doesn't matter that we aren't setting this flag, we'll
                    // be reporting the method's exception anyway, not a MissingMethodException.
                    foundMethod = true;
                    // void methods return AutomationNull.Value, so don't add it
                    // If we tried to invoke a method that didn't exist, then we'll try enumerating the object and call the method on it's members.
                    RuntimeException rte = tie.InnerException as RuntimeException;
                    if (rte != null && rte.ErrorRecord.FullyQualifiedErrorId.Equals(ParserOps.MethodNotFoundErrorId, StringComparison.Ordinal))
                            MethodInvokerWorker(invokeMemberSite, nestedEnumerator, args, context, result, ref foundMethod);
                    // Always unwrap the TargetInvocationException - we are called via a delegate already and anything we throw
                    // will get wrapped in a new TargetInvocationException.
        // Call some method(s) named by binder on all objects from enumerator - applied recursively if an object is itself enumerable
        // and doesn't have the method.
        // We don't necessarily call the same method on each object, just the same named method.
        internal static object MethodInvoker(PSInvokeMemberBinder binder,
                                             Type delegateType,
                                             Type typeForMessage)
            var invokeMemberSite = CallSite.Create(delegateType, binder);
            bool foundMethod = false;
            MethodInvokerWorker(invokeMemberSite, enumerator, args, context, result, ref foundMethod);
            if (!foundMethod)
                // We must have had an empty collection - throw an error.
                                                               ParserOps.MethodNotFoundErrorId,
                                                               ParserStrings.MethodNotFound, typeForMessage.FullName,
                                                               binder.Name);
                // All void methods - don't return a value.
        internal static object Multiply(IEnumerator enumerator, uint times)
            var fakeEnumerator = enumerator as NonEnumerableObjectEnumerator;
                // We have a non-enumerable object, we're trying to multiply something to it.  Generate an error
                // (or on the off chance that there is an implicit op, call that).
                return ParserOps.ImplicitOp(fakeEnumerator.GetNonEnumerableObject(),
                                            times,
                                            "op_Multiply", null, "*");
            var originalList = new List<object>();
                originalList.Add(Current(enumerator));
            if (originalList.Count == 0)
                // Don't use Array.Empty<object>(); always return a new instance.
                return new object[0];
            return ArrayOps.Multiply(originalList.ToArray(), times);
        internal static IEnumerator GetEnumerator(IEnumerable enumerable)
                return enumerable.GetEnumerator();
                // Just rethrow runtime exceptions...
        // Sometimes we need to pretend something is enumerable when it isn't.  So we wrap the object in a collection and enumerate that.
        // But sometimes we need to behave differently when an object is enumerable or not.  For example:
        //     $o -eq $o
        // If $o is enumerable, this expression will always return $null because we search for values in the LHS that match the RHS.
        // If $o is not enumerable, this expression returns $true.
        // The solution is to pretend the object is enumerable, return a real but custom enumerator.  In places that don't care
        // about semantics (e.g. when writing to the pipe), the enumerator will work just fine.  In places where we care about
        // language semantics, we can check the type of the enumerator and use the non-enumerable semantics instead.
        internal class NonEnumerableObjectEnumerator : IEnumerator
            internal static IEnumerator Create(object obj)
                return new NonEnumerableObjectEnumerator
                    _obj = obj,
                    _realEnumerator = (new[] { obj }).GetEnumerator()
            private object _obj;
            private IEnumerator _realEnumerator;
            bool IEnumerator.MoveNext()
                return _realEnumerator.MoveNext();
                _realEnumerator.Reset();
                get { return _realEnumerator.Current; }
            internal object GetNonEnumerableObject()
                return _obj;
        internal static IEnumerator GetCOMEnumerator(object obj)
            object targetValue = PSObject.Base(obj);
                var enumerator = (targetValue as IEnumerable)?.GetEnumerator();
            return targetValue as IEnumerator ?? NonEnumerableObjectEnumerator.Create(obj);
        internal static IEnumerator GetGenericEnumerator<T>(IEnumerable<T> enumerable)
        internal static bool MoveNext(ExecutionContext context, IEnumerator enumerator)
                    null, "BadEnumeration", ParserStrings.BadEnumeration, e, e.Message);
        /// Wrapper caller for enumerator.Current - handles and republishes errors...
        internal static object Current(IEnumerator enumerator)
        internal static object AddFakeEnumerable(NonEnumerableObjectEnumerator fakeEnumerator, object rhs)
            // We have a non-enumerable object, we're trying to add something to it.  Generate an error
            var fakeEnumerator2 = rhs as NonEnumerableObjectEnumerator;
                                        fakeEnumerator2 != null ? fakeEnumerator2.GetNonEnumerableObject() : rhs,
                                        "op_Addition", null, "+");
        internal static object AddEnumerable(ExecutionContext context, IEnumerator lhs, IEnumerator rhs)
            var fakeEnumerator = lhs as NonEnumerableObjectEnumerator;
                return AddFakeEnumerable(fakeEnumerator, rhs);
            while (MoveNext(context, lhs))
                result.Add(Current(lhs));
            while (MoveNext(context, rhs))
                result.Add(Current(rhs));
        internal static object AddObject(ExecutionContext context, IEnumerator lhs, object rhs)
            result.Add(rhs);
        internal static object Compare(IEnumerator enumerator, object valueToCompareTo, Func<object, object, bool> compareDelegate)
                return compareDelegate(fakeEnumerator.GetNonEnumerableObject(), valueToCompareTo) ? Boxed.True : Boxed.False;
            var resultArray = new List<object>();
                object val = Current(enumerator);
                if (compareDelegate(val, valueToCompareTo))
                    resultArray.Add(val);
            return resultArray.ToArray();
        internal static void WriteEnumerableToPipe(IEnumerator enumerator, Pipe pipe, ExecutionContext context, bool dispose)
                    pipe.Add(Current(enumerator));
                if (dispose)
                    var disposable = enumerator as IDisposable;
        internal static object[] ToArray(IEnumerator enumerator)
                result.Add(Current(enumerator));
        internal static object[] GetSlice(IList list, int startIndex)
            int countElements = list.Count - startIndex;
            object[] result = new object[countElements];
            while (j < countElements)
                result[j++] = list[i++];
    internal static class MemberInvocationLoggingOps
        private static readonly Lazy<bool> DumpLogAMSIContent = new Lazy<bool>(
            () => {
                object result = Environment.GetEnvironmentVariable("__PSDumpAMSILogContent");
                if (result != null && LanguagePrimitives.TryConvertTo(result, out int value))
                    return value == 1;
        private static string ArgumentToString(object arg)
            object baseObj = PSObject.Base(arg);
            if (baseObj is null)
                // The argument is null or AutomationNull.Value.
            // The comparisons below are ordered by the likelihood of arguments being of those types.
            if (baseObj is string str)
            // Special case some types to call 'ToString' on the object. For the rest, we return its
            // full type name to avoid calling a potentially expensive 'ToString' implementation.
            Type baseType = baseObj.GetType();
            if (baseType.IsEnum || baseType.IsPrimitive
                || baseType == typeof(Guid)
                || baseType == typeof(Uri)
                || baseType == typeof(Version)
                || baseType == typeof(SemanticVersion)
                || baseType == typeof(BigInteger)
                || baseType == typeof(decimal))
                return baseObj.ToString();
            return baseType.FullName;
        internal static void LogMemberInvocation(string targetName, string name, object[] args)
                var contentName = "PowerShellMemberInvocation";
                var argsBuilder = new Text.StringBuilder();
                    string value = ArgumentToString(args[i]);
                        argsBuilder.Append(", ");
                    argsBuilder.Append($"<{value}>");
                string content = $"<{targetName}>.{name}({argsBuilder})";
                if (DumpLogAMSIContent.Value)
                    Console.WriteLine("\n=== Amsi notification report content ===");
                    Console.WriteLine(content);
                var success = AmsiUtils.ReportContent(
                    name: contentName,
                    content: content);
                    Console.WriteLine($"=== Amsi notification report success: {success} ===");
                // ReportContent() will throw PSSecurityException if AMSI detects malware, which
                // must be propagated.
#pragma warning disable CS0168 // variable declared but never used
#pragma warning restore CS0168
                    Console.WriteLine($"!!! Amsi notification report exception: {ex} !!!");
