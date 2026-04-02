 * Fixture-Based Linter Tests
 * Test suite that runs linter validation against real component specs loaded from fixtures.
 * This approach allows testing against actual bugs found in production components.
import { describe, it, expect } from '../infrastructure/test-runner';
import { loadFixture, loadFixturesByCategory, getFixtureStats } from '../fixtures/fixture-loader';
 * Set the context user for tests
export function setContextUser(user: UserInfo) {
 * Test all broken components - each gets its own test case
export async function registerBulkBrokenTests() {
  const fixtures = await loadFixturesByCategory('broken');
  describe('Broken Components', () => {
    for (const fixture of fixtures) {
      it(`${fixture.metadata.name} - should have violations`, async () => {
        if (lintResult.violations.length > 0) {
          console.log(`      ✓ ${lintResult.violations.length} violation(s)`);
          console.log(`      ❌ 0 violations (expected >0)`);
        // Broken components MUST have violations
        expect(lintResult.violations.length).toBeGreaterThan(0);
 * Test all fixed components - each gets its own test case
export async function registerBulkFixedTests() {
  const fixtures = await loadFixturesByCategory('fixed');
  describe('Fixed Components', () => {
      it(`${fixture.metadata.name} - should have NO violations`, async () => {
        // Check for invalid property violations (the specific bug we fixed)
        const invalidPropertyViolations = lintResult.violations.filter((v: any) =>
        if (invalidPropertyViolations.length === 0) {
          console.log(`      ✓ 0 violations`);
          console.log(`      ❌ ${invalidPropertyViolations.length} violation(s) (expected 0)`);
        // Fixed components MUST have ZERO invalid property violations
        expect(invalidPropertyViolations.length).toBe(0);
 * Test all valid components - each gets its own test case
export async function registerBulkValidTests() {
  const fixtures = await loadFixturesByCategory('valid');
  describe('Valid Components', () => {
          console.log(`      ❌ ${lintResult.violations.length} violation(s) (expected 0)`);
            // Show full message for better debugging
            const fullMessage = v.message.replace(/\n/g, ' ').trim();
            console.log(`         ${idx + 1}. [${v.rule}] ${fullMessage}`);
        // Valid components MUST have ZERO violations
        expect(lintResult.violations.length).toBe(0);
 * Display fixture statistics
export async function displayFixtureStats() {
  const stats = await getFixtureStats();
  console.log('\n📊 Fixture Statistics:');
  console.log(`   Total Fixtures: ${stats.total}`);
  console.log(`   Broken:  ${stats.broken}`);
  console.log(`   Fixed:   ${stats.fixed}`);
  console.log(`   Valid:   ${stats.valid}`);
