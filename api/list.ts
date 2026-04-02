import { Command, Flags } from '@oclif/core';
import ora from 'ora-classic';
import { ActionService } from '../../services/ActionService';
import { OutputFormatter } from '../../lib/output-formatter';
import { ValidationService } from '../../services/ValidationService';
export default class ActionsList extends Command {
  static description = 'List available actions';
  static examples = [
    '<%= config.bin %> <%= command.id %>',
    '<%= config.bin %> <%= command.id %> --output=json',
    '<%= config.bin %> <%= command.id %> --output=table',
  static flags = {
    output: Flags.string({
      description: 'Output format',
      options: ['compact', 'json', 'table'],
      default: 'table'
  async run(): Promise<void> {
    const { flags } = await this.parse(ActionsList);
    const spinner = ora('Loading available actions...');
      const validator = new ValidationService();
      const outputFormat = validator.validateOutputFormat(flags.output);
      spinner.start();
      const actionService = new ActionService();
      await actionService.initialize();
      const actions = await actionService.listActions();
      spinner.stop();
      const formatter = new OutputFormatter(outputFormat);
      this.log(formatter.formatActionList(actions));
      // Clean up resources
      await this.cleanup();
      // Force exit after successful completion
      setImmediate(() => process.exit(0));
    } catch (error: any) {
      if (spinner.isSpinning) {
        spinner.fail('Failed to load actions');
      // If it's a formatted error message, show it cleanly
      if (error.message.startsWith('❌')) {
        this.log(error.message);
        this.exit(1);
        this.error(error.message);
  private async cleanup(): Promise<void> {
      const { closeMJProvider } = await import('../../lib/mj-provider');
      await closeMJProvider();
      // Ignore cleanup errors to not interfere with main execution
import { AgentService } from '../../services/AgentService';
export default class AgentsList extends Command {
  static description = 'List available AI agents';
    const { flags } = await this.parse(AgentsList);
    const spinner = ora('Loading available agents...');
      const agentService = new AgentService();
      await agentService.initialize();
      const agents = await agentService.listAgents();
      this.log(formatter.formatAgentList(agents));
        spinner.fail('Failed to load agents');
import { PromptService } from '../../services/PromptService';
export default class PromptsList extends Command {
  static description = 'List available models for prompt execution';
      description: 'Show detailed model information',
    const { flags } = await this.parse(PromptsList);
      const service = new PromptService();
      const models = await service.listAvailableModels();
      this.log(chalk.bold(`\nAvailable AI Models (${models.length}):\n`));
      for (const model of models) {
        this.log(`${chalk.cyan(model.name)} ${chalk.gray(`(${model.vendor})`)}`);
        if (flags.verbose && model.description) {
          this.log(`  ${chalk.dim(model.description)}`);
      this.log(chalk.gray('\nUse any of these models with the --model flag when running prompts'));
      this.error(error as Error);
  static description = 'List available AI actions';
    const { ActionService, OutputFormatter } = await import('@memberjunction/ai-cli');
      const service = new ActionService();
      const actions = await service.listActions();
      // Force exit after completion
    const { AgentService, OutputFormatter } = await import('@memberjunction/ai-cli');
      const service = new AgentService();
      const agents = await service.listAgents();
    const { PromptService } = await import('@memberjunction/ai-cli');
      spinner.start('Loading available models...');
      spinner.fail('Failed to load models');
export default class TestList extends Command {
  static description = 'List available tests, suites, and types';
    '<%= config.bin %> <%= command.id %> --suites',
    '<%= config.bin %> <%= command.id %> --types',
    '<%= config.bin %> <%= command.id %> --type=agent-eval',
    '<%= config.bin %> <%= command.id %> --tag=smoke',
    '<%= config.bin %> <%= command.id %> --status=active --verbose',
    suites: Flags.boolean({
      description: 'List test suites instead of tests',
    types: Flags.boolean({
      description: 'List test types',
    type: Flags.string({
      description: 'Filter by test type',
    tag: Flags.string({
      description: 'Filter by tag',
    const { ListCommand } = await import('@memberjunction/testing-cli');
    const { flags } = await this.parse(TestList);
      // Create ListCommand instance and execute
      const listCommand = new ListCommand();
      await listCommand.execute({
        suites: flags.suites,
        types: flags.types,
        type: flags.type,
        tag: flags.tag,
 * @fileoverview List command implementation
import { TestEngine, VariableResolver } from '@memberjunction/testing-engine';
import { TestVariableDefinition } from '@memberjunction/testing-engine-base';
import { MJTestEntity, MJTestSuiteEntity, MJTestTypeEntity } from '@memberjunction/core-entities';
import { ListFlags } from '../types';
import { initializeMJProvider, closeMJProvider, getContextUser } from '../lib/mj-provider';
 * List command - List available tests, suites, and types
export class ListCommand {
     * Execute the list command
    async execute(flags: ListFlags, contextUser?: UserInfo): Promise<void> {
            // Initialize MJ provider (database connection and metadata)
            // Get context user after initialization if not provided
            if (flags.types) {
                this.listTestTypes(engine, flags);
            } else if (flags.suites) {
                this.listTestSuites(engine, flags);
                this.listTests(engine, flags);
            console.error(OutputFormatter.formatError('Failed to list tests', error as Error));
            // Clean up resources before exit
                // Ignore cleanup errors
     * List test types
    private listTestTypes(engine: TestEngine, flags?: ListFlags): void {
        const types = engine.TestTypes;
        console.log(chalk.bold(`\nTest Types (${types.length}):\n`));
            console.log(chalk.cyan(`  ${type.Name}`));
            if (type.Description) {
                console.log(chalk.gray(`    ${type.Description}`));
            // Show variables if flag is set
            if (flags?.showVariables && type.VariablesSchema) {
                const variables = this.parseVariablesSchema(type.VariablesSchema);
                if (variables.length > 0) {
                    console.log(chalk.yellow(`    Variables:`));
                    for (const variable of variables) {
                        this.displayVariable(variable, 6);
     * List test suites
    private listTestSuites(engine: TestEngine, flags: ListFlags): void {
        let suites = engine.TestSuites;
        if (flags.status) {
            suites = suites.filter(s => s.Status?.toLowerCase() === flags.status?.toLowerCase());
        console.log(chalk.bold(`\nTest Suites (${suites.length}):\n`));
        for (const suite of suites) {
            const testCount = this.getTestCountForSuite(engine, suite);
            console.log(chalk.cyan(`  ${suite.Name}`) + chalk.gray(` (${testCount} tests)`));
            if (suite.Description) {
                console.log(chalk.gray(`    ${suite.Description}`));
            if (flags.verbose && suite.Configuration) {
                console.log(chalk.gray(`    Config: ${suite.Configuration}`));
     * List tests
    private listTests(engine: TestEngine, flags: ListFlags): void {
        let tests = engine.Tests;
        if (flags.type) {
            const type = engine.GetTestTypeByName(flags.type);
                tests = tests.filter(t => t.TypeID === type.ID);
            tests = tests.filter(t => {
                if (!t.Tags) return false;
                    const tags = JSON.parse(t.Tags) as string[];
                    return tags.includes(flags.tag!);
            tests = tests.filter(t => t.Status?.toLowerCase() === flags.status?.toLowerCase());
        // Group by type
        const typeMap = new Map<string, MJTestEntity[]>();
            const type = types.find(t => t.ID === test.TypeID);
            const typeName = type?.Name || 'Unknown';
            if (!typeMap.has(typeName)) {
                typeMap.set(typeName, []);
            typeMap.get(typeName)!.push(test);
        console.log(chalk.bold(`\nAvailable Tests (${tests.length}):\n`));
        const resolver = new VariableResolver();
        for (const [typeName, testsInType] of typeMap) {
            console.log(chalk.bold.cyan(`${typeName} (${testsInType.length}):`));
            for (const test of testsInType) {
                const tags = this.formatTags(test.Tags);
                console.log(`  ${chalk.white(test.Name)} ${tags}`);
                if (flags.verbose && test.Description) {
                    console.log(chalk.gray(`    ${test.Description}`));
                if (flags.showVariables) {
                    const testType = types.find(t => t.ID === test.TypeID);
                    if (testType?.VariablesSchema) {
                        const variables = resolver.getAvailableVariables(
                            testType.VariablesSchema,
                            test.Variables
     * Get test count for a suite
    private getTestCountForSuite(engine: TestEngine, suite: MJTestSuiteEntity): number {
        if (!suite.Configuration) return 0;
            const config = JSON.parse(suite.Configuration) as { testIds?: string[] };
            return config.testIds?.length || 0;
     * Format tags for display
    private formatTags(tagsJson: string | null): string {
        if (!tagsJson) return '';
            const tags = JSON.parse(tagsJson) as string[];
            return chalk.gray(`Tags: ${tags.join(', ')}`);
     * Parse variables schema JSON
    private parseVariablesSchema(schemaJson: string): TestVariableDefinition[] {
            const schema = resolver.parseTypeSchema(schemaJson);
            return schema?.variables || [];
     * Display a single variable definition
    private displayVariable(variable: TestVariableDefinition, indent: number): void {
        const prefix = ' '.repeat(indent);
        const required = variable.required ? chalk.red('*') : '';
        const defaultVal = variable.defaultValue !== undefined
            ? chalk.gray(` (default: ${variable.defaultValue})`)
        console.log(`${prefix}${chalk.white(variable.name)}${required}: ${chalk.cyan(variable.dataType)}${defaultVal}`);
        if (variable.description) {
            console.log(`${prefix}  ${chalk.gray(variable.description)}`);
        if (variable.possibleValues && variable.possibleValues.length > 0) {
            const values = variable.possibleValues.map(pv => pv.value).join(', ');
            console.log(`${prefix}  ${chalk.gray(`Values: [${values}]`)}`);
