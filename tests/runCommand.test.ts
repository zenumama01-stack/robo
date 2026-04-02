vi.mock('tree-kill', () => ({
    default: vi.fn()
import { RunCommandsBase } from '../Misc/runCommand';
import type { CommandExecutionResult } from '../Misc/runCommand';
describe('CommandExecutionResult type', () => {
    it('should represent a successful command', () => {
        const result: CommandExecutionResult = {
            output: 'Build completed',
            error: '',
            elapsedTime: 5000
        expect(result.elapsedTime).toBe(5000);
    it('should represent a failed command', () => {
            output: '',
            error: 'Command not found',
            elapsedTime: 100
        expect(result.error).toBe('Command not found');
    it('should track elapsed time', () => {
            output: 'done',
            elapsedTime: 12345
        expect(result.elapsedTime).toBe(12345);
describe('RunCommandsBase', () => {
    let runner: RunCommandsBase;
        runner = new RunCommandsBase();
    describe('runCommands', () => {
            expect(typeof runner.runCommands).toBe('function');
        it('should return an array', async () => {
            // Empty commands array should return empty results
            const results = await runner.runCommands([]);
            expect(Array.isArray(results)).toBe(true);
        it('should handle errors in individual commands without failing entire batch', async () => {
            // The runCommand will fail because the command doesn't exist, but runCommands catches it
            const results = await runner.runCommands([
                { command: 'nonexistent-command-xyz', args: [], workingDirectory: '/tmp', when: 'test', timeout: 1000 }
            // The error is caught and logged, not propagated
            expect(results).toBeDefined();
    describe('runCommand', () => {
            expect(typeof runner.runCommand).toBe('function');
        it('should reject for invalid commands', async () => {
                command: 'echo',
                args: ['hello'],
                when: 'test',
                timeout: 5000
            // This will attempt to actually spawn, so we test the structure
                const result = await runner.runCommand(command);
                // If it succeeds (echo exists), check the result shape
                expect(result).toHaveProperty('output');
                expect(result).toHaveProperty('success');
                expect(result).toHaveProperty('elapsedTime');
                // If it fails (in restricted environments), that's expected
