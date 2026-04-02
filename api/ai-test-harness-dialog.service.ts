import { TestHarnessWindowManagerService } from './test-harness-window-manager.service';
import { TestResult } from './test-harness-window.service';
 * Service for opening AI test harness windows for agents and prompts.
 * This service wraps the TestHarnessWindowManagerService to provide a simpler API
 * that matches the expected interface in consuming components.
export class AITestHarnessDialogService {
  constructor(private testHarnessService: TestHarnessWindowManagerService) {}
   * Opens the test harness window for an AI Agent
  openForAgent(agentId: string, viewContainerRef?: ViewContainerRef): Observable<TestResult> {
    return this.testHarnessService.openAgentTestHarness({
      agentId: agentId,
      // Don't pass title - let the window component generate it with the agent name
      viewContainerRef: viewContainerRef
   * Opens the test harness window for an AI Prompt
  openForPrompt(promptId: string, viewContainerRef?: ViewContainerRef): Observable<TestResult> {
    return this.testHarnessService.openPromptTestHarness({
      // Don't pass title - let the window component generate it with the prompt name
