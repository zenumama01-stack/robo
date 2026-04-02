import chalk from 'chalk';
export default class ActionsRun extends Command {
  static description = 'Execute individual actions directly';
    '<%= config.bin %> <%= command.id %> -n "Get Weather" --param "Location=Boston"',
    '<%= config.bin %> <%= command.id %> -n "Send Email" --param "To=user@example.com" --param "Subject=Test"',
    '<%= config.bin %> <%= command.id %> -n "Calculate" --param "Numbers=[1,2,3]" --verbose',
    name: Flags.string({
      char: 'n',
      description: 'Name of the action to execute',
      required: false
    param: Flags.string({
      description: 'Action parameter (key=value format, can be used multiple times)',
      multiple: true,
      default: []
    verbose: Flags.boolean({
      char: 'v',
      description: 'Show detailed execution information'
    timeout: Flags.integer({
      description: 'Override default timeout (milliseconds)',
      default: 300000
    'dry-run': Flags.boolean({
      description: 'Validate without executing'
      default: 'compact'
    const { flags } = await this.parse(ActionsRun);
    const spinner = ora();
      // Validate inputs
      const timeout = validator.validateTimeout(flags.timeout);
      const parameters = validator.parseParameters(flags.param);
      await validator.validateActionInput(flags.name, parameters);
      // Show available actions if none specified
      if (!flags.name) {
        spinner.start('Loading available actions...');
      if (flags['dry-run']) {
        spinner.start('Validating action and parameters...');
        // Try to find the action to validate it exists
        const action = await actionService.findAction(flags.name);
        if (!action) {
          this.error(`Action "${flags.name}" not found. Use 'mj-ai actions:list' to see available actions.`);
        this.log(chalk.green('✓ Validation passed - dry run complete'));
        this.log(chalk.dim(`Action: ${flags.name}`));
        if (Object.keys(parameters).length > 0) {
          this.log(chalk.dim(`Parameters:`));
          Object.entries(parameters).forEach(([key, value]) => {
            const valueStr = typeof value === 'object' ? JSON.stringify(value) : String(value);
            const displayValue = valueStr.length > 50 ? valueStr.substring(0, 50) + '...' : valueStr;
            this.log(chalk.dim(`  ${key}: ${displayValue}`));
          this.log(chalk.dim(`Parameters: None`));
        this.log(chalk.dim(`Timeout: ${timeout}ms`));
      // Execute action
      spinner.start(`Executing action: ${flags.name}...`);
      const result = await actionService.executeAction(flags.name, {
        verbose: flags.verbose,
        timeout: timeout,
        parameters: parameters
      this.log(formatter.formatActionResult(result));
      // Exit with error code if execution failed
      if (!result.success) {
        spinner.fail('Operation failed');
import { ConversationService } from '../../services/ConversationService';
export default class AgentsRun extends Command {
  static description = 'Execute AI agents with custom prompts';
    '<%= config.bin %> <%= command.id %> -a "Demo Loop Agent" -p "Hello world"',
    '<%= config.bin %> <%= command.id %> -a "Demo Loop Agent" --chat',
    '<%= config.bin %> <%= command.id %> -a "Demo Loop Agent" -p "Get weather" --verbose',
    agent: Flags.string({
      char: 'a',
      description: 'Name of the AI agent to execute',
    prompt: Flags.string({
      char: 'p', 
      description: 'Prompt text to send to the agent'
    chat: Flags.boolean({
      char: 'c',
      description: 'Start interactive conversation mode'
    const { flags } = await this.parse(AgentsRun);
      await validator.validateAgentInput(flags.agent, flags.prompt, flags.chat);
      // Show available agents if none specified
      if (!flags.agent) {
        spinner.start('Loading available agents...');
        spinner.start('Validating agent and configuration...');
        // Try to find the agent to validate it exists
        const agent = await agentService.findAgent(flags.agent);
          this.error(`Agent "${flags.agent}" not found. Use 'mj-ai agents:list' to see available agents.`);
        this.log(chalk.dim(`Agent: ${flags.agent}`));
        if (flags.prompt) {
          this.log(chalk.dim(`Prompt: ${flags.prompt.substring(0, 100)}${flags.prompt.length > 100 ? '...' : ''}`));
        this.log(chalk.dim(`Mode: ${flags.chat ? 'Interactive chat' : 'Single execution'}`));
      // Execute agent in chat mode
      if (flags.chat) {
        const conversationService = new ConversationService();
        await conversationService.startChat(flags.agent, flags.prompt, {
          timeout: timeout
      // Execute agent with single prompt
      if (!flags.prompt) {
        this.error('Prompt is required for non-interactive mode. Use --chat for interactive mode or provide --prompt.');
      spinner.start(`Executing agent: ${flags.agent}...`);
      const result = await agentService.executeAgent(flags.agent, flags.prompt, {
      this.log(formatter.formatAgentResult(result));
        if (flags.verbose && error.stack) {
          this.log('\nStack trace:');
          this.log(error.stack);
        // Show stack trace in verbose mode
          this.error(error.stack);
import ora from 'ora';
export default class PromptsRun extends Command {
  static description = 'Execute a direct prompt with an AI model';
    '<%= config.bin %> <%= command.id %> -p "Explain quantum computing in simple terms"',
    '<%= config.bin %> <%= command.id %> -p "Write a Python function to sort a list" --model "gpt-4"',
    '<%= config.bin %> <%= command.id %> -p "Translate to French: Hello world" --temperature 0.3',
    '<%= config.bin %> <%= command.id %> -p "Generate a haiku" --system "You are a poet" --max-tokens 100',
      description: 'The prompt to execute',
      required: true,
    model: Flags.string({
      char: 'm',
      description: 'AI model to use (e.g., gpt-4, claude-3-opus)',
    system: Flags.string({
      char: 's',
      description: 'System prompt to set context',
    temperature: Flags.string({
      char: 't',
      description: 'Temperature for response creativity (0.0-2.0)',
    'max-tokens': Flags.integer({
      description: 'Maximum tokens for the response',
    configuration: Flags.string({
      description: 'AI Configuration ID to use',
      char: 'o',
      default: 'compact',
      description: 'Show detailed execution information',
      description: 'Execution timeout in milliseconds',
      default: 300000, // 5 minutes
    const { flags } = await this.parse(PromptsRun);
    const formatter = new OutputFormatter(flags.output as 'compact' | 'json' | 'table');
      // Show what model will be used
      if (flags.model) {
        spinner.start(`Executing prompt with model: ${flags.model}`);
        spinner.start('Executing prompt with default model');
      // Parse temperature if provided
      let temperature: number | undefined;
      if (flags.temperature) {
        temperature = parseFloat(flags.temperature);
        if (isNaN(temperature) || temperature < 0 || temperature > 2) {
          spinner.fail();
          this.error('Temperature must be a number between 0.0 and 2.0');
      const result = await service.executePrompt(flags.prompt, {
        timeout: flags.timeout,
        model: flags.model,
        temperature,
        maxTokens: flags['max-tokens'],
        systemPrompt: flags.system,
        configurationId: flags.configuration,
      this.log(formatter.formatPromptResult(result));
  static description = 'Execute an AI action with parameters';
    '<%= config.bin %> <%= command.id %> -n "Get Stock Price" --param "Ticker=AAPL"',
    '<%= config.bin %> <%= command.id %> -n "Send Single Message" --param "To=user@example.com" --param "Subject=Test" --param "Body=Hello" --param "MessageType=Email" --param "Provider=SendGrid"',
    '<%= config.bin %> <%= command.id %> -n "Calculate Expression" --param "Expression=2+2*3" --dry-run',
      description: 'Action name',
      description: 'Action parameters in key=value format',
      description: 'Validate without executing',
    // Parse parameters
    const params: Record<string, string> = {};
    if (flags.param) {
      for (const param of flags.param) {
        const [key, ...valueParts] = param.split('=');
        if (!key || valueParts.length === 0) {
          this.error(`Invalid parameter format: "${param}". Use key=value format.`);
        params[key] = valueParts.join('='); // Handle values with = in them
        // For dry-run, just show what would be executed
        this.log(chalk.yellow('Dry-run mode: Action would be executed with these parameters:'));
        this.log(chalk.cyan(`Action: ${flags.name}`));
        if (Object.keys(params).length > 0) {
          this.log(chalk.cyan('Parameters:'));
            this.log(`  ${key}: ${value}`);
          this.log(chalk.gray('No parameters provided'));
        spinner.start(`Executing action: ${flags.name}`);
        const result = await service.executeAction(flags.name, {
          parameters: params,
          timeout: flags.timeout
  static description = 'Execute an AI agent with a prompt or start interactive chat';
    '<%= config.bin %> <%= command.id %> -a "Skip: Requirements Expert" -p "Create a dashboard for sales metrics"',
    '<%= config.bin %> <%= command.id %> -a "Child Component Generator Sub-agent" --chat',
    '<%= config.bin %> <%= command.id %> -a "Skip: Technical Design Expert" -p "Build a React component" --verbose --timeout=600000',
      description: 'Agent name',
      description: 'Prompt to execute',
      exclusive: ['chat'],
      description: 'Start interactive chat mode',
      exclusive: ['prompt'],
    const { AgentService, OutputFormatter, ConversationService } = await import('@memberjunction/ai-cli');
    if (!flags.prompt && !flags.chat) {
      this.error('Either --prompt or --chat flag is required');
        // Interactive chat mode
        await conversationService.startChat(flags.agent, undefined, {
        // Single prompt execution
        spinner.start(`Executing agent: ${flags.agent}`);
        const result = await service.executeAgent(flags.agent, flags.prompt!, {
    const { PromptService, OutputFormatter } = await import('@memberjunction/ai-cli');
export default class TestRun extends Command {
  static description = 'Execute a single test by ID or name';
    '<%= config.bin %> <%= command.id %> <test-id>',
    '<%= config.bin %> <%= command.id %> --name="Active Members Count"',
    '<%= config.bin %> <%= command.id %> <test-id> --environment=staging',
    '<%= config.bin %> <%= command.id %> <test-id> --format=json --output=results.json',
    '<%= config.bin %> <%= command.id %> <test-id> --dry-run',
    testId: Args.string({
      description: 'Test ID to execute',
      description: 'Test name to execute',
    environment: Flags.string({
      description: 'Environment context (dev, staging, prod)',
    const { RunCommand } = await import('@memberjunction/testing-cli');
    const { args, flags } = await this.parse(TestRun);
      // Create RunCommand instance and execute
      const runCommand = new RunCommand();
      await runCommand.execute(args.testId, {
        name: flags.name,
        environment: flags.environment,
 * @fileoverview Run command implementation
import { RunFlags } from '../types';
import { SpinnerManager } from '../utils/spinner-manager';
import { parseVariableFlags, getTestVariablesSchema } from '../utils/variable-parser';
 * Run command - Execute a single test or filtered set of tests
export class RunCommand {
    private spinner = new SpinnerManager();
     * Execute the run command
     * @param testId - Optional test ID to run
    async execute(testId: string | undefined, flags: RunFlags, contextUser?: UserInfo): Promise<void> {
            const format = flags.format || config.defaultFormat || 'console';
            const environment = flags.environment || config.defaultEnvironment;
            // Get engine instance
            let test;
            if (testId) {
                // Run specific test by ID
                test = engine.GetTestByID(testId);
                if (!test) {
                    console.error(OutputFormatter.formatError(`Test not found: ${testId}`));
            } else if (flags.name) {
                // Run test by name
                test = engine.GetTestByName(flags.name);
                    console.error(OutputFormatter.formatError(`Test not found: ${flags.name}`));
            } else if (flags.suite) {
                // Run test suite (delegate to suite command)
                console.error(OutputFormatter.formatError('Use "mj test suite" command to run test suites'));
            } else if (flags.tag || flags.category || flags.difficulty) {
                // Run tests by filter
                console.error(OutputFormatter.formatError('Filtered test execution not yet implemented'));
            } else if (flags.all) {
                console.error(OutputFormatter.formatError('Use "mj test suite" command to run all tests'));
                console.error(OutputFormatter.formatError('Must specify test ID, --name, or other filter'));
            // Parse variables from --var flags
            const variablesSchema = getTestVariablesSchema(engine, test.ID);
            const variables = parseVariableFlags(flags.var, variablesSchema);
            // Dry run mode
            if (flags.dryRun) {
                console.log(OutputFormatter.formatInfo(`Would run test: ${test.Name}`));
                console.log(OutputFormatter.formatInfo(`Type: ${test.Type}`));
                console.log(OutputFormatter.formatInfo(`Environment: ${environment}`));
                    console.log(OutputFormatter.formatInfo(`Variables: ${JSON.stringify(variables)}`));
            // Execute test
            this.spinner.start(`Running test: ${test.Name}...`);
            const result = await engine.RunTest(test.ID, {
            this.spinner.stop();
                // Multiple iterations - display summary and all results
                console.log(OutputFormatter.formatInfo(`Ran ${result.length} iterations`));
                    const output = OutputFormatter.formatTestResult(iterationResult, format);
                    console.log(output);
                    if (iterationResult.status !== 'Passed') {
                // Write to file if requested (write all results as JSON array)
                    OutputFormatter.writeToFile(JSON.stringify(result, null, 2), flags.output);
                // Exit with appropriate code (pass only if all iterations passed)
                const output = OutputFormatter.formatTestResult(result, format);
                // Write to file if requested
                OutputFormatter.writeToFile(output, flags.output);
                // Exit with appropriate code
                process.exit(result.status === 'Passed' ? 0 : 1);
            this.spinner.fail();
            console.error(OutputFormatter.formatError('Failed to run test', error as Error));
