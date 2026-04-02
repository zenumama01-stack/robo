import { GuardrailsManager, PhaseType, GuardrailCheckResult } from '../core/GuardrailsManager';
import { AnalysisRun } from '../types/state';
import { GuardrailsConfig } from '../types/config';
describe('GuardrailsManager', () => {
  describe('constructor and defaults', () => {
    it('should create with default empty config', () => {
      const gm = new GuardrailsManager();
      const result = gm.checkGuardrails(run);
      expect(result.canContinue).toBe(true);
    it('should accept config parameter', () => {
      const config: GuardrailsConfig = { maxTokensPerRun: 10000 };
      const gm = new GuardrailsManager(config);
  describe('checkGuardrails - disabled', () => {
    it('should allow continuation when guardrails are disabled', () => {
      const gm = new GuardrailsManager({ enabled: false });
      const run = createMockRun({ totalTokensUsed: 999999 });
  describe('checkGuardrails - token limits', () => {
    it('should stop when maxTokensPerRun is exceeded', () => {
      const gm = new GuardrailsManager({ maxTokensPerRun: 1000 });
      const run = createMockRun({ totalTokensUsed: 1500 });
      expect(result.canContinue).toBe(false);
      expect(result.reason).toContain('Token limit exceeded');
      expect(result.exceedances).toBeDefined();
      expect(result.exceedances![0].type).toBe('tokens_per_run');
    it('should allow when tokens are under limit', () => {
      const gm = new GuardrailsManager({ maxTokensPerRun: 10000 });
      const run = createMockRun({ totalTokensUsed: 500 });
    it('should warn when approaching token limit', () => {
      const gm = new GuardrailsManager({
        maxTokensPerRun: 1000,
        warnThresholds: { tokenPercentage: 80 }
      const run = createMockRun({ totalTokensUsed: 850 });
      expect(result.warnings).toBeDefined();
      expect(result.warnings!.some(w => w.type === 'tokens_per_run')).toBe(true);
  describe('checkGuardrails - cost limits', () => {
    it('should stop when maxCostDollars is exceeded', () => {
      const gm = new GuardrailsManager({ maxCostDollars: 1.0 });
      const run = createMockRun({ estimatedCost: 1.5 });
      expect(result.reason).toContain('Cost limit exceeded');
    it('should warn when approaching cost limit', () => {
        maxCostDollars: 1.0,
        warnThresholds: { costPercentage: 80 }
      const run = createMockRun({ estimatedCost: 0.85 });
      expect(result.warnings!.some(w => w.type === 'cost')).toBe(true);
  describe('checkGuardrails - duration limits', () => {
    it('should stop when maxDurationSeconds is exceeded', () => {
      // Mock Date.now to simulate elapsed time: constructor gets 'now', checkGuardrails gets 'now + 5s'
      vi.spyOn(Date, 'now')
        .mockReturnValueOnce(now)       // constructor startTime
        .mockReturnValueOnce(now + 5000); // checkGuardrails elapsed check
      const gm = new GuardrailsManager({ maxDurationSeconds: 1 });
      expect(result.reason).toContain('Duration limit exceeded');
      vi.restoreAllMocks();
  describe('phase tracking', () => {
    it('should start and end a phase', () => {
      gm.startPhase('analysis');
      gm.endPhase(run, 'analysis');
      expect(run.phaseMetrics).toBeDefined();
      expect(run.phaseMetrics!.analysis).toBeDefined();
      expect(run.phaseMetrics!.analysis!.startedAt).toBeDefined();
      expect(run.phaseMetrics!.analysis!.completedAt).toBeDefined();
    it('should track discovery phase', () => {
      gm.startPhase('discovery');
      gm.endPhase(run, 'discovery');
      expect(run.phaseMetrics!.discovery).toBeDefined();
    it('should track sanityChecks phase', () => {
      gm.startPhase('sanityChecks');
      gm.endPhase(run, 'sanityChecks');
      expect(run.phaseMetrics!.sanityChecks).toBeDefined();
  describe('iteration tracking', () => {
    it('should start and end an iteration', () => {
      gm.startIteration(1);
      gm.endIteration(run, 1);
      expect(run.iterationMetrics).toBeDefined();
      expect(run.iterationMetrics).toHaveLength(1);
      expect(run.iterationMetrics![0].iterationNumber).toBe(1);
      expect(run.iterationMetrics![0].duration).toBeGreaterThanOrEqual(0);
  describe('recordEnforcement', () => {
    it('should record exceedances in run', () => {
      const result: GuardrailCheckResult = {
        canContinue: false,
        reason: 'Token limit exceeded',
        exceedances: [
          { type: 'tokens_per_run', limit: 1000, actual: 1500, unit: 'tokens' }
      gm.recordEnforcement(run, result);
      expect(run.guardrailsEnforced).toBeDefined();
      expect(run.guardrailsEnforced!.stoppedDueToGuardrails).toBe(true);
      expect(run.guardrailsEnforced!.exceedances).toHaveLength(1);
    it('should record warnings in run', () => {
        canContinue: true,
        warnings: [
          { type: 'tokens_per_run', percentage: 85, message: 'Approaching limit' }
      expect(run.guardrailsEnforced!.warnings).toHaveLength(1);
    it('should not record when no warnings or exceedances', () => {
        canContinue: true
      expect(run.guardrailsEnforced).toBeUndefined();
  describe('getElapsedSeconds', () => {
    it('should return elapsed time', () => {
      const elapsed = gm.getElapsedSeconds();
      expect(elapsed).toBeGreaterThanOrEqual(0);
      expect(elapsed).toBeLessThan(5);
  describe('getPhaseTokenBudgetRemaining', () => {
    it('should return undefined when no phase limits configured', () => {
      expect(gm.getPhaseTokenBudgetRemaining('analysis')).toBeUndefined();
    it('should return limit when phase limits are configured', () => {
        maxTokensPerPhase: { analysis: 5000 }
      expect(gm.getPhaseTokenBudgetRemaining('analysis')).toBe(5000);
    it('should return undefined for unconfigured phase', () => {
      expect(gm.getPhaseTokenBudgetRemaining('discovery')).toBeUndefined();
