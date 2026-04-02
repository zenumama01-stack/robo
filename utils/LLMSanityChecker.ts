 * LLM Sanity Checker
 * Uses LLM intelligence to review and reject obviously wrong PK/FK candidates
 * This is a MACRO-LEVEL review that happens once after statistical detection
import { PKCandidate, FKCandidate } from '../types/discovery.js';
import { AIConfig } from '../types/config.js';
import { createLLMInstance } from '../utils/llm-factory.js';
export interface SanityCheckResult {
  invalidPKs: Array<{
  invalidFKs: Array<{
  suggestions: string[];
export class LLMSanityChecker {
  constructor(private aiConfig: AIConfig) {
    // Create LLM instance using shared factory (DRY principle)
    this.llm = createLLMInstance(aiConfig.provider, aiConfig.apiKey);
   * Review all detected PKs and FKs for obvious errors
   * This is a one-time macro review after statistical detection
  public async reviewCandidates(
  ): Promise<SanityCheckResult> {
    console.log(`[LLMSanityChecker] Reviewing ${pkCandidates.length} PK candidates and ${fkCandidates.length} FK candidates`);
    // Build prompt with all candidates
    const prompt = this.buildSanityCheckPrompt(pkCandidates, fkCandidates);
          content: 'You are a database schema expert specializing in identifying incorrect primary key and foreign key  candidates. Your job is to reject obviously wrong candidates based on naming patterns and data types.'
    console.log(`[LLMSanityChecker] Calling LLM for sanity check...`);
      console.warn(`[LLMSanityChecker] LLM call failed: ${chatResult.errorMessage}`);
        invalidPKs: [],
        invalidFKs: [],
        suggestions: [],
        invalidPKs: Array<{ schema: string; table: string; column: string; reason: string }>;
        invalidFKs: Array<{ schema: string; table: string; column: string; reason: string }>;
      console.log(`[LLMSanityChecker] Found ${result.invalidPKs.length} invalid PKs, ${result.invalidFKs.length} invalid FKs`);
        invalidPKs: result.invalidPKs,
        invalidFKs: result.invalidFKs,
        suggestions: result.suggestions || [],
      console.error(`[LLMSanityChecker] Failed to parse LLM response: ${(parseError as Error).message}`);
      console.error(`[LLMSanityChecker] Raw content: ${content}`);
   * Build the sanity check prompt with all candidate information
  private buildSanityCheckPrompt(
    // Group PKs by table
    const pksByTable = new Map<string, PKCandidate[]>();
      if (!pksByTable.has(key)) {
        pksByTable.set(key, []);
      pksByTable.get(key)!.push(pk);
    // Group FKs by table
    const fksByTable = new Map<string, FKCandidate[]>();
      if (!fksByTable.has(key)) {
        fksByTable.set(key, []);
      fksByTable.get(key)!.push(fk);
    let prompt = `You are reviewing database primary key and foreign key candidates for obvious errors.
CRITICAL RULES:
1. Date/time fields are NEVER primary keys (they are not unique per row)
2. Quantity, count, amount fields are NEVER primary keys
3. Text fields (names, descriptions, notes) are NEVER primary keys
4. Boolean/flag fields are NEVER primary keys
5. Each table should have exactly 1 PK (or a composite PK with 2-3 columns max)
6. Foreign keys should reference another table, not point to themselves
DETECTED PRIMARY KEY CANDIDATES:
    // Add PK candidates
    for (const [tableKey, pks] of pksByTable.entries()) {
      prompt += `\nTable: ${tableKey}\n`;
      for (const pk of pks) {
        const uniqueness = pk.evidence?.uniqueness.toFixed(2) || '?';
        const pattern = pk.evidence?.dataPattern || 'unknown';
        prompt += `  - ${pk.columnNames.join(', ')} (pattern: ${pattern}, uniqueness: ${uniqueness}, confidence: ${pk.confidence}%)\n`;
    prompt += `\nDETECTED FOREIGN KEY CANDIDATES:
    // Add FK candidates
    for (const [tableKey, fks] of fksByTable.entries()) {
      for (const fk of fks) {
        prompt += `  - ${fk.sourceColumn} → ${fk.targetSchema}.${fk.targetTable}.${fk.targetColumn} (confidence: ${fk.confidence}%)\n`;
    prompt += `\nTASK:
Identify PKs and FKs that are definitely WRONG based on the rules above.
OUTPUT FORMAT (JSON):
  "invalidPKs": [
    {"schema": "sales", "table": "addr", "column": "ln2", "reason": "VARCHAR field, not a unique identifier"},
    {"schema": "sales", "table": "cst", "column": "lst_ord", "reason": "Date field, cannot be primary key"}
  "invalidFKs": [
    {"schema": "inv", "table": "adj", "column": "adj_id", "reason": "Self-referencing FK to same table, likely misidentified"}
  "suggestions": [
    "sales.addr should have only addr_id as PK",
    "sales.cst should have only cst_id as PK"
