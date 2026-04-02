import { TestEngineBase, TestVariableDefinition, TestTypeVariablesSchema, TestVariablesConfig } from '@memberjunction/testing-engine-base';
import { GraphQLTestingClient, GraphQLDataProvider } from '@memberjunction/graphql-dataprovider';
import { MJTestEntity, MJTestSuiteEntity, MJTestSuiteTestEntity, MJTestTypeEntity } from '@memberjunction/core-entities';
interface VariableInput {
  definition: TestVariableDefinition;
  value: string | number | Date | boolean | null;
  stringValue: string; // For input binding (converted on submit)
interface ProgressUpdate {
  step: string;
  testName?: string;
  driverType?: string;
  selector: 'app-test-run-dialog',
      <div class="test-run-dialog">
        @if (!isRunning && !hasCompleted) {
          <!-- Pre-selected Mode - Compact Header -->
          @if (isPreselected) {
            <div class="dialog-scroll-content">
            <div class="preselected-header">
              <div class="preselected-info">
                <div class="preselected-icon">
                  <i class="fa-solid" [class.fa-flask]="runMode === 'test'" [class.fa-layer-group]="runMode === 'suite'"></i>
                <div class="preselected-content">
                  <div class="preselected-label">{{ runMode === 'test' ? 'Test' : 'Test Suite' }}</div>
                  <div class="preselected-name">{{ preselectedName }}</div>
              <div class="options-compact">
                  <input type="checkbox" [(ngModel)]="verbose" />
                  <span>Verbose</span>
                @if (runMode === 'suite') {
                    <input type="checkbox" [(ngModel)]="parallel" />
                    <span>Parallel</span>
            <!-- Tags Section for Preselected Mode -->
            <div class="tags-section">
              <div class="tags-header">
                <span>Run Tags</span>
                <span class="tags-hint">(optional)</span>
              <div class="tags-container">
                <div class="tags-chips">
                    <span class="tag-chip">
                      <button class="tag-remove" (click)="removeTag(tag)">
                <div class="tag-input-row">
                    placeholder="Add tag (e.g., opus-4.5, v2.1.0)"
                    class="tag-input"
                  <button class="tag-add-btn" (click)="addTag()" [disabled]="!newTag.trim()">
            <!-- Variables Section for Preselected Mode -->
            @if (availableVariables.length > 0) {
              <div class="variables-section">
                <button class="variables-toggle" (click)="showVariablesSection = !showVariablesSection">
                  <i class="fa-solid" [class.fa-chevron-right]="!showVariablesSection" [class.fa-chevron-down]="showVariablesSection"></i>
                  <span>Test Variables</span>
                  <span class="variables-count-badge">{{ availableVariables.length }}</span>
                @if (showVariablesSection) {
                  <div class="variables-content">
                    @for (variable of availableVariables; track variable.definition.name) {
                      <div class="variable-row">
                        <div class="variable-info">
                          <label class="variable-label">{{ variable.definition.displayName }}</label>
                          @if (variable.definition.description) {
                            <span class="variable-description">{{ variable.definition.description }}</span>
                        <div class="variable-input">
                          @if (variable.definition.valueSource === 'static' && variable.definition.possibleValues) {
                            <select [(ngModel)]="variable.stringValue" class="variable-select">
                              <option value="">-- Select --</option>
                              @for (option of variable.definition.possibleValues; track option.value) {
                                <option [value]="option.value">{{ option.label || option.value }}</option>
                          } @else if (variable.definition.dataType === 'boolean') {
                              <option value="true">Yes</option>
                              <option value="false">No</option>
                          } @else if (variable.definition.dataType === 'number') {
                              [(ngModel)]="variable.stringValue"
                              class="variable-input-field"
                              [placeholder]="variable.definition.defaultValue?.toString() || 'Enter value'"
                              [step]="variable.definition.name.toLowerCase().includes('temperature') ? 0.1 : 1"
            <!-- Advanced Options for Preselected Suite Mode -->
            @if (runMode === 'suite' && suiteTests.length > 0) {
              <div class="advanced-options-section preselected-advanced">
                <button class="advanced-toggle" (click)="toggleAdvancedOptions()">
                  <i class="fa-solid" [class.fa-chevron-right]="!showAdvancedOptions" [class.fa-chevron-down]="showAdvancedOptions"></i>
                  <span>Advanced Options</span>
                  <span class="test-count-badge">{{ suiteTests.length }} tests</span>
                @if (showAdvancedOptions) {
                  <div class="advanced-content">
                    <!-- Selection Mode Tabs -->
                    <div class="selection-mode-tabs">
                        class="selection-tab"
                        [class.active]="!useSequenceRange"
                        (click)="useSequenceRange = false"
                        <i class="fa-solid fa-check-square"></i>
                        Select Tests
                        [class.active]="useSequenceRange"
                        (click)="useSequenceRange = true"
                        Sequence Range
                    <!-- Select Individual Tests Mode -->
                    @if (!useSequenceRange) {
                      <div class="test-selection-panel">
                        <div class="selection-header">
                          <label class="checkbox-label select-all">
                              [checked]="allTestsSelected"
                              [indeterminate]="someTestsSelected"
                              (change)="toggleAllTests($any($event.target).checked)"
                            <span>Select All</span>
                          <span class="selection-count">{{ selectedTestCount }} of {{ suiteTests.length }} selected</span>
                        <div class="test-list">
                          @for (test of suiteTests; track test.testId) {
                            <label class="test-item" [class.selected]="test.selected">
                                [checked]="test.selected"
                                (change)="toggleTest(test.testId)"
                              <span class="test-sequence">#{{ test.sequence }}</span>
                              <span class="test-name">{{ test.testName }}</span>
                    <!-- Sequence Range Mode -->
                    @if (useSequenceRange) {
                      <div class="sequence-range-panel">
                        <div class="range-inputs">
                          <div class="range-field">
                            <label>Start at sequence</label>
                              [(ngModel)]="sequenceStart"
                              [max]="suiteTests.length"
                              class="sequence-input"
                          <div class="range-separator">
                            <label>End at sequence</label>
                              [(ngModel)]="sequenceEnd"
                        @if (sequenceRangeValid) {
                          <div class="range-summary">
                            Will run {{ testsInSequenceRange }} test(s) from sequence {{ sequenceStart }} to {{ sequenceEnd }}
                          <div class="range-error">
                            Invalid range: start must be less than or equal to end
          <!-- Selection Mode - Full UI -->
          @if (!isPreselected) {
            <div class="selection-mode">
              <div class="mode-tabs">
                  class="mode-tab"
                  [class.active]="runMode === 'test'"
                  (click)="setRunMode('test')"
                  <span>Single Test</span>
                  [class.active]="runMode === 'suite'"
                  (click)="setRunMode('suite')"
                  <span>Test Suite</span>
            @if (runMode === 'test') {
                    (input)="filterItems()"
                    placeholder="Search tests..."
                <div class="items-list">
                  @for (test of filteredTests; track test.ID) {
                      class="item"
                      [class.selected]="selectedTestId === test.ID"
                      (click)="selectTest(test.ID)"
                        <div class="item-name">{{ test.Name }}</div>
                        <div class="item-meta">{{ test.Type }} • {{ test.Description || 'No description' }}</div>
                      @if (selectedTestId === test.ID) {
                        <div class="item-check">
                  @empty {
                    <div class="no-items">
                      <p>No tests found</p>
                    placeholder="Search test suites..."
                  @for (suite of filteredSuites; track suite.ID) {
                      [class.selected]="selectedSuiteId === suite.ID"
                      (click)="selectSuite(suite.ID)"
                      <div class="item-icon suite">
                        <div class="item-name">{{ suite.Name }}</div>
                        <div class="item-meta">{{ suite.Description || 'No description' }}</div>
                      @if (selectedSuiteId === suite.ID) {
              <!-- Advanced Options - Progressive Disclosure -->
              @if (selectedSuiteId && suiteTests.length > 0) {
                <div class="advanced-options-section">
              <div class="options-panel">
                  <span>Verbose logging</span>
                    <span>Run tests in parallel</span>
              <!-- Tags Section for Selection Mode -->
              <div class="tags-section selection-mode-tags">
        @if (isRunning || hasCompleted) {
          <div class="execution-mode">
            <div class="execution-header">
                <i class="fa-solid" [class.fa-spinner]="isRunning" [class.fa-spin]="isRunning" [class.fa-check-circle]="hasCompleted && !hasError" [class.fa-exclamation-circle]="hasCompleted && hasError"></i>
                <span>{{ executionTitle }}</span>
              <div class="execution-status" [class.running]="isRunning" [class.success]="hasCompleted && !hasError" [class.error]="hasCompleted && hasError">
                {{ executionStatus }}
            <div class="progress-container">
                <div class="progress-fill" [style.width.%]="progress"></div>
              <div class="progress-text">{{ progress }}%</div>
            <div class="progress-steps">
              @for (step of progressSteps; track step.step) {
                <div class="step" [class.active]="step.active" [class.completed]="step.completed">
                  <div class="step-icon">
                    @if (step.completed) {
                    } @else if (step.active) {
                    <div class="step-label">{{ step.label }}</div>
                    @if (step.message) {
                      <div class="step-message">{{ step.message }}</div>
            @if (executionLog.length > 0) {
              <div class="execution-log">
                  <i class="fa-solid fa-terminal"></i>
                  <span>Execution Log</span>
                  @for (entry of executionLog; track $index) {
                    <div class="log-entry" [class]="entry.type">
                      <span class="log-time">{{ entry.timestamp | date:'HH:mm:ss' }}</span>
                      <span class="log-message">{{ entry.message }}</span>
            @if (hasCompleted && result) {
              <div class="result-summary" [class.success]="!hasError" [class.error]="hasError">
                  <i class="fa-solid" [class.fa-check-circle]="!hasError" [class.fa-exclamation-circle]="hasError"></i>
                  <span>{{ hasError ? 'Execution Failed' : 'Execution Completed' }}</span>
                  @if (!hasError) {
                    <div class="result-item">
                      <span class="result-label">Status:</span>
                      <span class="result-value">{{ result.result?.status || 'Unknown' }}</span>
                      <span class="result-label">Score:</span>
                      <span class="result-value">{{ (result.result?.score || 0) | number:'1.4-4' }}</span>
                      <span class="result-label">Duration:</span>
                      <span class="result-value">{{ result.executionTimeMs }}ms</span>
                    @if (result.result?.totalCost) {
                        <span class="result-label">Cost:</span>
                        <span class="result-value">\${{ result.result.totalCost | number:'1.6-6' }}</span>
                      <span>{{ result.errorMessage }}</span>
            <button class="action-btn cancel-btn" (click)="onClose()">Cancel</button>
            <button class="action-btn run-btn"
                    [disabled]="!canRun()"
                    (click)="runTest()">
              Run {{ runMode === 'test' ? 'Test' : 'Suite' }}
          } @else if (hasCompleted) {
            <button class="action-btn cancel-btn" (click)="onClose()">Close</button>
            <button class="action-btn run-btn" (click)="resetDialog()">
              Run Another
            <button class="action-btn run-btn" [disabled]="true">
    .test-run-dialog {
    .dialog-scroll-content {
    .run-btn:hover:not(:disabled) {
    .run-btn:disabled {
    /* Preselected Mode - Compact Header */
    .preselected-header {
    .preselected-info {
    .preselected-icon {
      background: linear-gradient(135deg, #2196f3, #21cbf3);
    .preselected-content {
    .preselected-label {
    .preselected-name {
    .options-compact {
    /* Selection Mode */
    .selection-mode {
    .mode-tabs {
    .mode-tab {
    .mode-tab:hover {
    .mode-tab.active {
    .mode-tab i {
      border: 2px solid #e0e4e8;
      background: rgba(0,0,0,0.05);
    .items-list {
    .item {
    .item:hover {
    .item.selected {
    .item-icon.suite {
    .item-check {
    .no-items {
    .no-items i {
    .no-items p {
    .options-panel {
    .checkbox-label span {
    /* Execution Mode */
    .execution-title i {
    .execution-title i.fa-check-circle {
    .execution-title i.fa-exclamation-circle {
    .execution-status.running {
    .execution-status.success {
    .execution-status.error {
      background: linear-gradient(90deg, #2196f3, #21cbf3);
      min-width: 45px;
    .progress-steps {
    .step.active {
    .step-icon {
    .step.active .step-icon {
    .step.completed .step-icon {
    .step-message {
    .execution-log {
      background: #2d2d2d;
      border-bottom: 1px solid #3d3d3d;
    .log-header i {
    .log-entry {
    .log-entry.error {
    .log-entry.success {
    .log-entry.info {
    .log-time {
    .log-message {
    .result-summary {
    .result-summary.success {
      border: 2px solid #4caf50;
    .result-summary.error {
      border: 2px solid #f44336;
    .result-summary.success .result-header {
    .result-summary.error .result-header {
    .result-header i {
      background: rgba(255,255,255,0.5);
    .result-value {
      background: rgba(255,255,255,0.7);
    .error-message span {
    /* Tags Section Styles */
    .tags-section {
    .tags-section.selection-mode-tags {
    .tags-header {
    .tags-header i {
    .tags-hint {
    .tags-container {
    .tags-chips {
    .tag-remove {
    .tag-remove:hover {
      background: rgba(0,0,0,0.2);
    .tag-remove i {
    .tag-input-row {
    .tag-input {
      border: 1px solid #e0e4e8;
    .tag-input:focus {
    .tag-input::placeholder {
    .tag-add-btn {
    .tag-add-btn:hover:not(:disabled) {
    .tag-add-btn:disabled {
    .tag-add-btn i {
    /* Advanced Options - Progressive Disclosure */
    .advanced-options-section {
    .advanced-toggle {
    .advanced-toggle:hover {
    .advanced-toggle i {
    .test-count-badge {
    .advanced-content {
      padding: 0 12px 12px 12px;
    .selection-mode-tabs {
    .selection-tab {
    .selection-tab:hover {
    .selection-tab.active {
    .selection-tab i {
    /* Test Selection Panel */
    .test-selection-panel {
    .selection-header {
      border-bottom: 1px solid #e0e4e8;
    .select-all {
    .test-list {
    .test-item {
    .test-item:last-child {
    .test-item:hover {
    .test-item.selected {
    .test-item input[type="checkbox"] {
    /* Sequence Range Panel */
    .sequence-range-panel {
    .range-inputs {
    .range-field {
    .range-field label {
    .sequence-input {
    .sequence-input:focus {
    .range-separator {
    .range-summary {
    .range-error {
    .range-error i {
    .preselected-advanced {
      /* No extra margin - handled by dialog-scroll-content gap */
    /* Variables Section Styles */
    .variables-section {
    .variables-toggle {
    .variables-toggle:hover {
    .variables-toggle .fa-sliders {
    .variables-count-badge {
    .variables-content {
    .variable-row {
    .variable-info {
    .variable-label {
    .variable-description {
    .variable-input {
    .variable-select,
    .variable-input-field {
    .variable-select:focus,
    .variable-input-field:focus {
      border-color: #9c27b0;
    .variable-input-field::placeholder {
export class TestRunDialogComponent implements OnInit, OnDestroy {
  private testingClient!: GraphQLTestingClient;
  private engine!: TestEngineBase;
  runMode: 'test' | 'suite' = 'test';
  searchText = '';
  selectedTestId: string | null = null;
  selectedSuiteId: string | null = null;
  verbose = true;
  parallel = false;
  // Tags for test/suite runs
  // Pre-selection mode - when launched from a specific test/suite
  isPreselected = false;
  preselectedName = '';
  allTests: MJTestEntity[] = [];
  allSuites: MJTestSuiteEntity[] = [];
  filteredTests: MJTestEntity[] = [];
  filteredSuites: MJTestSuiteEntity[] = [];
  // Selective test execution for suites (progressive disclosure)
  showAdvancedOptions = false;
  suiteTests: SuiteTestItem[] = [];
  useSequenceRange = false;
  sequenceStart: number | null = null;
  sequenceEnd: number | null = null;
  // Variables for parameterized tests
  availableVariables: VariableInput[] = [];
  showVariablesSection = false;
  // Execution state
  isRunning = false;
  hasCompleted = false;
  hasError = false;
  progress = 0;
  executionTitle = '';
  executionStatus = '';
  result: any = null;
  progressSteps = [
    { step: 'loading_test', label: 'Loading Configuration', message: '', active: false, completed: false },
    { step: 'initializing_driver', label: 'Initializing Driver', message: '', active: false, completed: false },
    { step: 'executing_test', label: 'Executing Test', message: '', active: false, completed: false },
    { step: 'evaluating_oracles', label: 'Evaluating Oracles', message: '', active: false, completed: false },
    { step: 'complete', label: 'Complete', message: '', active: false, completed: false }
  executionLog: Array<{ timestamp: Date; message: string; type: 'info' | 'success' | 'error' }> = [];
  get dialogTitle(): string {
    if (this.isRunning || this.hasCompleted) {
      return 'Test Execution';
    return 'Run Test';
    // Get GraphQLDataProvider from Metadata.Provider (it's already configured in the Angular app)
    this.testingClient = new GraphQLTestingClient(dataProvider);
    // Get engine instance and ensure it's configured
    this.engine = TestEngineBase.Instance;
    // Ensure the engine is configured
    if (!this.engine.Loaded) {
      await this.engine.Config(false);
    // Load tests and suites from cache
    this.allTests = this.engine.Tests.filter(t => t.Status === 'Active');
    this.allSuites = this.engine.TestSuites.filter(s => s.Status === 'Active');
    // Check if we have a pre-selected test or suite
    if (this.selectedTestId) {
      this.isPreselected = true;
      this.runMode = 'test';
      const test = this.allTests.find(t => t.ID === this.selectedTestId);
      this.preselectedName = test ? test.Name : 'Test';
      // Load variables for the selected test
      if (test) {
        this.loadVariablesForTest(test);
    } else if (this.selectedSuiteId) {
      this.runMode = 'suite';
      const suite = this.allSuites.find(s => s.ID === this.selectedSuiteId);
      this.preselectedName = suite ? suite.Name : 'Test Suite';
      // Load suite tests for selective execution
      this.loadSuiteTests(this.selectedSuiteId);
    this.filterItems();
  setRunMode(mode: 'test' | 'suite'): void {
    this.runMode = mode;
    this.selectedTestId = null;
    this.selectedSuiteId = null;
  filterItems(): void {
    const search = this.searchText.toLowerCase();
    if (this.runMode === 'test') {
      this.filteredTests = this.allTests.filter(t =>
        (t.Description && t.Description.toLowerCase().includes(search))
      this.filteredSuites = this.allSuites.filter(s =>
        (s.Description && s.Description.toLowerCase().includes(search))
  selectTest(testId: string): void {
    this.selectedTestId = testId;
    const test = this.allTests.find(t => t.ID === testId);
  selectSuite(suiteId: string): void {
    this.selectedSuiteId = suiteId;
    this.loadSuiteTests(suiteId);
   * Load tests for a selected suite to enable selective execution
  private loadSuiteTests(suiteId: string): void {
    const suiteTestLinks = this.engine.TestSuiteTests.filter(st => st.SuiteID === suiteId);
    // Build list of tests with their sequence numbers
    this.suiteTests = suiteTestLinks
        if (!test) return null;
          testId: st.TestID,
          testName: test.Name,
          sequence: st.Sequence,
          selected: true // All selected by default
      .filter((item): item is SuiteTestItem => item !== null)
    // Reset sequence range
    this.useSequenceRange = false;
    this.sequenceStart = this.suiteTests.length > 0 ? this.suiteTests[0].sequence : null;
    this.sequenceEnd = this.suiteTests.length > 0 ? this.suiteTests[this.suiteTests.length - 1].sequence : null;
  toggleAdvancedOptions(): void {
    this.showAdvancedOptions = !this.showAdvancedOptions;
   * Load available variables for a test based on its TestType's VariablesSchema
  private loadVariablesForTest(test: MJTestEntity): void {
    this.availableVariables = [];
    this.showVariablesSection = false;
    // Get the TestType to access VariablesSchema
    const testType = this.engine.TestTypes.find(tt => tt.ID === test.TypeID);
    if (!testType) {
    // Parse the type's VariablesSchema
    const variablesSchemaJson = (testType as MJTestTypeEntity & { VariablesSchema?: string }).VariablesSchema;
    if (!variablesSchemaJson) {
    const typeSchema = SafeJSONParse(variablesSchemaJson) as TestTypeVariablesSchema | null;
    if (!typeSchema || !typeSchema.variables || typeSchema.variables.length === 0) {
    // Parse the test's Variables config to check which are exposed
    const testVariablesJson = (test as MJTestEntity & { Variables?: string }).Variables;
    const testConfig = testVariablesJson ? SafeJSONParse(testVariablesJson) as TestVariablesConfig | null : null;
    // Build the available variables list
    for (const varDef of typeSchema.variables) {
      // Check if test explicitly hides this variable
      const testOverride = testConfig?.variables?.[varDef.name];
      if (testOverride?.exposed === false) {
        continue; // Variable not exposed by this test
      // Determine the default value to show
      const defaultValue = testOverride?.defaultValue ?? varDef.defaultValue;
      this.availableVariables.push({
        definition: varDef,
        value: defaultValue ?? null,
        stringValue: defaultValue != null ? String(defaultValue) : ''
    // Auto-expand variables section if there are required variables
    if (this.availableVariables.some(v => v.definition.required)) {
      this.showVariablesSection = true;
   * Collect variable values for test execution
  private getVariablesForExecution(): Record<string, unknown> | undefined {
    if (this.availableVariables.length === 0) {
    const variables: Record<string, unknown> = {};
    let hasValues = false;
    for (const variable of this.availableVariables) {
      if (variable.stringValue !== '' && variable.stringValue != null) {
        hasValues = true;
        // Convert string value to appropriate type
        switch (variable.definition.dataType) {
            variables[variable.definition.name] = parseFloat(variable.stringValue);
            variables[variable.definition.name] = variable.stringValue.toLowerCase() === 'true';
            variables[variable.definition.name] = variable.stringValue;
    return hasValues ? variables : undefined;
  toggleAllTests(selectAll: boolean): void {
    this.suiteTests.forEach(t => t.selected = selectAll);
  toggleTest(testId: string): void {
    const test = this.suiteTests.find(t => t.testId === testId);
      test.selected = !test.selected;
  get selectedTestCount(): number {
    return this.suiteTests.filter(t => t.selected).length;
  get allTestsSelected(): boolean {
    return this.suiteTests.length > 0 && this.suiteTests.every(t => t.selected);
  get someTestsSelected(): boolean {
    const selected = this.selectedTestCount;
    return selected > 0 && selected < this.suiteTests.length;
  get sequenceRangeValid(): boolean {
    if (!this.useSequenceRange) return true;
    if (this.sequenceStart == null || this.sequenceEnd == null) return false;
    return this.sequenceStart <= this.sequenceEnd;
  get testsInSequenceRange(): number {
    if (!this.useSequenceRange || this.sequenceStart == null || this.sequenceEnd == null) {
      return this.suiteTests.length;
    return this.suiteTests.filter(t =>
      t.sequence >= this.sequenceStart! && t.sequence <= this.sequenceEnd!
   * Get IDs of selected tests for execution
  private getSelectedTestIds(): string[] {
    if (!this.showAdvancedOptions) {
      // If advanced options not shown, run all tests
      return this.suiteTests.map(t => t.testId);
    return this.suiteTests.filter(t => t.selected).map(t => t.testId);
   * Get sequence range parameters if enabled
  private getSequenceRangeParams(): { start: number | undefined; end: number | undefined } {
    if (!this.showAdvancedOptions || !this.useSequenceRange) {
      return { start: undefined, end: undefined };
      start: this.sequenceStart ?? undefined,
      end: this.sequenceEnd ?? undefined
  canRun(): boolean {
    return (this.runMode === 'test' && this.selectedTestId != null) ||
           (this.runMode === 'suite' && this.selectedSuiteId != null);
  async runTest(): Promise<void> {
    if (!this.canRun()) return;
    this.hasCompleted = false;
    this.hasError = false;
    this.progress = 0;
    this.executionLog = [];
    this.resetProgressSteps();
      this.executionTitle = test ? test.Name : 'Running Test...';
      this.executionStatus = 'Running';
      this.addLogEntry(`Starting test: ${test?.Name}`, 'info');
      await this.executeTest();
      this.executionTitle = suite ? suite.Name : 'Running Suite...';
      this.addLogEntry(`Starting suite: ${suite?.Name}`, 'info');
      await this.executeSuite();
  private async executeTest(): Promise<void> {
      // Collect variable values for execution
      const variables = this.getVariablesForExecution();
      const result = await this.testingClient.RunTest({
        testId: this.selectedTestId!,
        verbose: this.verbose,
        tags: this.tags.length > 0 ? this.tags : undefined,
        onProgress: (progress) => {
          // Update progress percentage (fallback to 0 if not provided)
          this.progress = progress.percentage ?? 0;
          // Update progress steps based on current step
          this.updateProgressStep(progress.currentStep);
          // Add log entry for this progress update
          this.addLogEntry(progress.message, 'info');
      this.progress = 100;
      this.hasCompleted = true;
      this.hasError = !result.success;
      this.executionStatus = result.success ? 'Completed' : 'Failed';
      this.completeAllSteps();
        this.addLogEntry('Test completed successfully', 'success');
        this.addLogEntry(`Test failed: ${result.errorMessage}`, 'error');
      this.hasError = true;
      this.executionStatus = 'Error';
      this.result = {
        errorMessage: (error as Error).message
      this.addLogEntry(`Error: ${(error as Error).message}`, 'error');
  private async executeSuite(): Promise<void> {
      // Build selective execution parameters
      const selectedTestIds = this.getSelectedTestIds();
      const sequenceParams = this.getSequenceRangeParams();
      // Collect variable values for execution (applies to all tests in suite)
      const result = await this.testingClient.RunTestSuite({
        suiteId: this.selectedSuiteId!,
        parallel: this.parallel,
        selectedTestIds: selectedTestIds.length < this.suiteTests.length ? selectedTestIds : undefined,
        sequenceStart: sequenceParams.start,
        sequenceEnd: sequenceParams.end,
        this.addLogEntry('Suite completed successfully', 'success');
        this.addLogEntry(`Suite failed: ${result.errorMessage}`, 'error');
  private updateProgress(update: ProgressUpdate): void {
    this.progress = update.percentage;
    // Update step states
    const stepIndex = this.progressSteps.findIndex(s => s.step === update.step);
    if (stepIndex >= 0) {
      // Mark previous steps as completed
      for (let i = 0; i < stepIndex; i++) {
        this.progressSteps[i].completed = true;
        this.progressSteps[i].active = false;
      // Mark current step as active
      this.progressSteps[stepIndex].active = true;
      this.progressSteps[stepIndex].message = update.message;
    this.addLogEntry(update.message, 'info');
  private updateProgressStep(currentStep: string): void {
    // Map test engine steps to our UI steps
    const stepMapping: Record<string, string> = {
      'loading_test': 'loading_test',
      'initializing': 'initializing_driver',
      'executing': 'executing_test',
      'evaluating': 'evaluating_oracles',
      'complete': 'complete'
    const mappedStep = stepMapping[currentStep] || currentStep;
    const stepIndex = this.progressSteps.findIndex(s => s.step === mappedStep);
      this.progressSteps[stepIndex].completed = false;
  private resetProgressSteps(): void {
    this.progressSteps.forEach(step => {
      step.active = false;
      step.completed = false;
      step.message = '';
  private completeAllSteps(): void {
      step.completed = true;
  private addLogEntry(message: string, type: 'info' | 'success' | 'error'): void {
    this.executionLog.push({
    // Keep log manageable
    if (this.executionLog.length > 100) {
      this.executionLog = this.executionLog.slice(-100);
  resetDialog(): void {
    this.executionTitle = '';
    this.executionStatus = '';
    this.result = null;
    this.tags = [];
    if (!this.isRunning) {
  // Tag management methods
