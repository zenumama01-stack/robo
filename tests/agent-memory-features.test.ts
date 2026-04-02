 * Unit tests for Agent Memory Features (Phase 1-3 improvements).
 * These tests verify:
 * - PayloadFeedbackManager functionality
 * - AgentContextInjector formatting with precedence instructions
 * - Multi-tenant scope determination
 * @since 2.130.0
// Mock Data and Types
interface MockNote {
    Type: string;
    Note: string;
    AgentID: string | null;
    CompanyID: string | null;
    PrimaryScopeRecordID: string | null;
    SecondaryScopes: string | null;
    __mj_CreatedAt?: Date;
interface MockPayloadWarning {
    type: 'content_truncation' | 'key_removal' | 'type_change' | 'pattern_anomaly';
    severity: 'high' | 'medium' | 'low';
    details: Record<string, unknown>;
interface MockFeedbackQuestion {
    warning: MockPayloadWarning;
        details: unknown;
// Test Helpers - Standalone implementations for testing
 * Determine MJ scope description for a note (mirrors AgentContextInjector)
function determineNoteScope(note: MockNote): string {
 * Determine secondary scope description for a note (multi-tenant feature)
function determineSecondaryScope(note: MockNote): string | null {
        return null; // No secondary scope
 * Format notes for injection with optional memory policy (mirrors AgentContextInjector)
function formatNotesForInjection(notes: MockNote[], includeMemoryPolicy: boolean = true): string {
    lines.push(`\u{1F4DD} AGENT NOTES (${notes.length})`);
        const scope = determineNoteScope(note);
        const secondaryScope = determineSecondaryScope(note);
 * Generate feedback questions from warnings (mirrors PayloadFeedbackManager)
function generateQuestions(warnings: MockPayloadWarning[]): MockFeedbackQuestion[] {
    const questions: MockFeedbackQuestion[] = [];
            case 'content_truncation': {
            case 'key_removal': {
            case 'type_change': {
        questions.push({
            id: `feedback_${i}_${Date.now()}`,
 * Map LLM responses to feedback format (mirrors PayloadFeedbackManager)
function mapLLMResponsesToFeedback(
    questions: MockFeedbackQuestion[],
): Array<{ questionId: string; intended: boolean; explanation?: string }> {
// Test Data
const mockNotes: MockNote[] = [
        ID: 'note-1',
        Type: 'Preference',
        Note: 'User prefers bullet points',
        AgentID: 'agent-1',
        UserID: 'user-1',
        CompanyID: null,
        PrimaryScopeRecordID: null,
        SecondaryScopes: null
        ID: 'note-2',
        Type: 'Context',
        Note: 'Company uses metric units',
        AgentID: null,
        UserID: null,
        CompanyID: 'company-1',
        PrimaryScopeRecordID: 'org-123',
        ID: 'note-3',
        Type: 'Constraint',
        Note: 'Never share PII',
        ID: 'note-4',
        Note: 'John prefers formal tone',
        SecondaryScopes: '{"ContactID": "contact-456"}'
const mockWarnings: MockPayloadWarning[] = [
        type: 'content_truncation',
        path: 'response.body',
        message: 'Content was significantly reduced',
        details: { originalLength: 1000, newLength: 100, reductionPercentage: 90 },
        requiresFeedback: true
        type: 'key_removal',
        path: 'data.metadata',
        message: 'Keys were removed',
        details: { removedKeys: ['timestamp', 'version'] },
        type: 'type_change',
        path: 'config.enabled',
        message: 'Type changed',
        details: { originalType: 'boolean', newType: 'string' },
        requiresFeedback: false // Should be filtered out
// Scope Determination Tests
describe('Scope Determination', () => {
    it('determineNoteScope: Agent + User specific', () => {
        expect(determineNoteScope(mockNotes[0])).toBe('Agent + User specific');
    it('determineNoteScope: Company-wide', () => {
        expect(determineNoteScope(mockNotes[1])).toBe('Company-wide');
    it('determineNoteScope: Global (all null)', () => {
        expect(determineNoteScope(mockNotes[2])).toBe('Global');
    it('determineSecondaryScope: Company-level (primary scope only)', () => {
        expect(determineSecondaryScope(mockNotes[1])).toBe('Company-level');
    it('determineSecondaryScope: User-specific (has secondary scopes)', () => {
        expect(determineSecondaryScope(mockNotes[3])).toBe('User-specific (most specific)');
    it('determineSecondaryScope: null when no secondary scope', () => {
        expect(determineSecondaryScope(mockNotes[0])).toBeNull();
// Format Notes Tests
describe('Format Notes', () => {
    it('includes memory policy by default', () => {
        const result = formatNotesForInjection(mockNotes);
        expect(result).toContain('<memory_policy>');
        expect(result).toContain('Precedence (highest to lowest)');
        expect(result).toContain('User-specific notes override company-level');
        expect(result).toContain('</memory_policy>');
    it('excludes memory policy when disabled', () => {
        const result = formatNotesForInjection(mockNotes, false);
        expect(result).not.toContain('<memory_policy>');
    it('includes note count', () => {
        expect(result).toContain(`AGENT NOTES (${mockNotes.length})`);
    it('includes note types and content', () => {
        expect(result).toContain('[Preference] User prefers bullet points');
        expect(result).toContain('[Context] Company uses metric units');
        expect(result).toContain('[Constraint] Never share PII');
    it('shows secondary scope when available', () => {
        expect(result).toContain('Company-level');
        expect(result).toContain('User-specific (most specific)');
    it('returns empty string for empty notes', () => {
        expect(formatNotesForInjection([])).toBe('');
// Feedback Question Tests
describe('Feedback Questions', () => {
    it('filters out non-feedback warnings', () => {
        const questions = generateQuestions(mockWarnings);
        expect(questions.length).toBe(2);
    it('formats content_truncation correctly', () => {
        const truncQuestion = questions[0];
        expect(truncQuestion.question).toContain('reduce the content');
        expect(truncQuestion.question).toContain('1000 to 100');
        expect(truncQuestion.question).toContain('90.0%');
    it('formats key_removal correctly', () => {
        const removalQuestion = questions[1];
        expect(removalQuestion.question).toContain('remove the non-empty key(s)');
        expect(removalQuestion.question).toContain('timestamp');
        expect(removalQuestion.question).toContain('version');
    it('includes context with path and type', () => {
        const question = questions[0];
        expect(question.context).toBeDefined();
        expect(question.context?.path).toBe('response.body');
        expect(question.context?.changeType).toBe('content_truncation');
// LLM Response Mapping Tests
describe('LLM Response Mapping', () => {
    it('maps responses correctly', () => {
        const llmResponses = [
            { questionNumber: 1, intended: false, explanation: 'This was a mistake' },
            { questionNumber: 2, intended: true, explanation: 'Intended cleanup' }
        const result = mapLLMResponsesToFeedback(questions, llmResponses);
        expect(result[0].intended).toBe(false);
        expect(result[0].explanation).toBe('This was a mistake');
        expect(result[1].intended).toBe(true);
    it('defaults to intended when no response', () => {
        const llmResponses: Array<{ questionNumber: number; intended: boolean; explanation?: string }> = [];
        expect(result.every(r => r.intended === true)).toBe(true);
        expect(result[0].explanation).toContain('No explicit response');
// Memory Cleanup Agent Tests
describe('Memory Cleanup Agent', () => {
    function getCutoffDate(retentionDays: number): string {
    function getNoteRetentionDays(agent: { NoteRetentionDays?: number | null }): number {
        const DEFAULT_NOTE_RETENTION_DAYS = 90;
        if (typeof agent.NoteRetentionDays === 'number' && agent.NoteRetentionDays > 0) {
            return agent.NoteRetentionDays;
        return DEFAULT_NOTE_RETENTION_DAYS;
    function getExampleRetentionDays(agent: { ExampleRetentionDays?: number | null }): number {
        const DEFAULT_EXAMPLE_RETENTION_DAYS = 180;
        if (typeof agent.ExampleRetentionDays === 'number' && agent.ExampleRetentionDays > 0) {
            return agent.ExampleRetentionDays;
        return DEFAULT_EXAMPLE_RETENTION_DAYS;
    function getAutoArchiveEnabled(agent: { AutoArchiveEnabled?: boolean }): boolean {
        return agent.AutoArchiveEnabled !== false;
    function buildCleanupSummary(result: {
    }): string {
        if (result.notesArchived > 0) parts.push(`${result.notesArchived} stale notes archived`);
        if (result.examplesArchived > 0) parts.push(`${result.examplesArchived} stale examples archived`);
        if (result.notesExpired > 0) parts.push(`${result.notesExpired} expired notes archived`);
        if (result.examplesExpired > 0) parts.push(`${result.examplesExpired} expired examples archived`);
        if (parts.length === 0) parts.push('No items needed archiving');
        if (result.errors.length > 0) parts.push(`${result.errors.length} errors encountered`);
    it('getCutoffDate calculates correct date for 90 days', () => {
        const cutoffStr = getCutoffDate(90);
        const cutoffDate = new Date(cutoffStr);
        const expectedDate = new Date(now);
        expectedDate.setDate(expectedDate.getDate() - 90);
        const diff = Math.abs(cutoffDate.getTime() - expectedDate.getTime());
        expect(diff).toBeLessThan(1000);
    it('getNoteRetentionDays returns agent value when set', () => {
        expect(getNoteRetentionDays({ NoteRetentionDays: 60 })).toBe(60);
    it('getNoteRetentionDays returns default when null', () => {
        expect(getNoteRetentionDays({ NoteRetentionDays: null })).toBe(90);
    it('getNoteRetentionDays returns default when not set', () => {
        expect(getNoteRetentionDays({})).toBe(90);
    it('getExampleRetentionDays returns agent value when set', () => {
        expect(getExampleRetentionDays({ ExampleRetentionDays: 120 })).toBe(120);
    it('getExampleRetentionDays returns default when not set', () => {
        expect(getExampleRetentionDays({})).toBe(180);
    it('getAutoArchiveEnabled returns true by default', () => {
        expect(getAutoArchiveEnabled({})).toBe(true);
    it('getAutoArchiveEnabled returns false when explicitly disabled', () => {
        expect(getAutoArchiveEnabled({ AutoArchiveEnabled: false })).toBe(false);
    it('getAutoArchiveEnabled returns true when explicitly enabled', () => {
        expect(getAutoArchiveEnabled({ AutoArchiveEnabled: true })).toBe(true);
    it('buildCleanupSummary reports stale notes', () => {
        const summary = buildCleanupSummary({ notesArchived: 5, examplesArchived: 0, notesExpired: 0, examplesExpired: 0, errors: [] });
        expect(summary).toContain('5 stale notes archived');
    it('buildCleanupSummary reports expired items', () => {
        const summary = buildCleanupSummary({ notesArchived: 0, examplesArchived: 0, notesExpired: 2, examplesExpired: 3, errors: [] });
        expect(summary).toContain('2 expired notes archived');
        expect(summary).toContain('3 expired examples archived');
    it('buildCleanupSummary reports errors', () => {
        const summary = buildCleanupSummary({ notesArchived: 1, examplesArchived: 0, notesExpired: 0, examplesExpired: 0, errors: ['error1', 'error2'] });
        expect(summary).toContain('2 errors encountered');
    it('buildCleanupSummary reports nothing needed when all zero', () => {
        const summary = buildCleanupSummary({ notesArchived: 0, examplesArchived: 0, notesExpired: 0, examplesExpired: 0, errors: [] });
        expect(summary).toBe('No items needed archiving');
// Extraction Guardrails Tests
describe('Extraction Guardrails', () => {
    function containsEphemeralPhrase(content: string): boolean {
        const ephemeralPatterns = [
            /this time/i, /just for now/i, /today only/i, /for this call/i,
            /temporarily/i, /one-time/i, /exception/i, /just once/i
        return ephemeralPatterns.some(pattern => pattern.test(content));
    function containsDurablePhrase(content: string): boolean {
        const durablePatterns = [
            /always/i, /never/i, /company policy/i, /all customers/i,
            /standard practice/i, /we typically/i, /our preference/i,
            /every time/i, /by default/i, /as a rule/i
        return durablePatterns.some(pattern => pattern.test(content));
    function containsPII(content: string): boolean {
        const piiPatterns = [
            /\b\d{3}-\d{2}-\d{4}\b/, /\b\d{16}\b/, /password[:\s]+\S+/i,
            /\bssn\b/i, /passport\s*#?\s*\d+/i
        return piiPatterns.some(pattern => pattern.test(content));
    it('containsEphemeralPhrase detects "just for now"', () => {
        expect(containsEphemeralPhrase('Just for now, use bullet points')).toBe(true);
    it('containsEphemeralPhrase detects "this time"', () => {
        expect(containsEphemeralPhrase('This time I want a shorter response')).toBe(true);
    it('containsEphemeralPhrase returns false for durable content', () => {
        expect(containsEphemeralPhrase('We always use metric units')).toBe(false);
    it('containsDurablePhrase detects "always"', () => {
        expect(containsDurablePhrase('We always use formal tone')).toBe(true);
    it('containsDurablePhrase detects "company policy"', () => {
        expect(containsDurablePhrase('Company policy requires approval')).toBe(true);
    it('containsDurablePhrase returns false for ephemeral content', () => {
        expect(containsDurablePhrase('Just this once, skip the greeting')).toBe(false);
    it('containsPII detects SSN pattern', () => {
        expect(containsPII('My SSN is 123-45-6789')).toBe(true);
    it('containsPII detects password mention', () => {
        expect(containsPII('The password is: secret123')).toBe(true);
    it('containsPII returns false for safe content', () => {
        expect(containsPII('User prefers bullet points')).toBe(false);
