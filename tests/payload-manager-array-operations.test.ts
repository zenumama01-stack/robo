 * PayloadManager Array Operations Tests
 * Tests for array handling in applyAgentChangeRequest:
 * 1. Empty array + updateElements should add all items
 * 2. Existing array + updateElements should do positional replacement + add extras
 * 3. Existing array + newElements should append
 * 4. Nested arrays (e.g., citations within facts) should be handled correctly
import { PayloadManager } from '../src/PayloadManager';
// Helper to run test and report
function runTest(name: string, fn: () => { passed: boolean; details: string }) {
    const result = fn();
    console.log(`${result.passed ? '✅' : '❌'} ${name}`);
    if (!result.passed) {
        console.log(`   ${result.details}`);
    return result.passed;
console.log('=== PayloadManager Array Operations Tests ===\n');
let allPassed = true;
// Test 1: Empty array + updateElements (original bug)
allPassed = runTest('Empty array + updateElements should add all items', () => {
    const payload = {
        knowledge: { facts: [] }
    const changeRequest = {
        updateElements: {
            knowledge: {
                facts: [
                    { inquiry: "Q1", text: "A1" },
                    { inquiry: "Q2", text: "A2" },
                    { inquiry: "Q3", text: "A3" }
    const result = pm.applyAgentChangeRequest(payload, changeRequest);
    const facts = (result.result as any).knowledge.facts;
    if (facts.length !== 3) {
        return { passed: false, details: `Expected 3 facts, got ${facts.length}` };
    return { passed: true, details: '' };
}) && allPassed;
// Test 2: Existing array + updateElements (positional replacement + extras)
allPassed = runTest('Existing array + updateElements should do positional replacement and add extras', () => {
                { inquiry: "Old Q1", text: "Old A1" }
                    { inquiry: "New Q1", text: "New A1" },
                    { inquiry: "New Q2", text: "New A2" }
    if (facts.length !== 2) {
        return { passed: false, details: `Expected 2 facts, got ${facts.length}` };
    if (facts[0].inquiry !== "New Q1") {
        return { passed: false, details: `Expected facts[0].inquiry to be "New Q1", got "${facts[0].inquiry}"` };
    if (facts[1].inquiry !== "New Q2") {
        return { passed: false, details: `Expected facts[1].inquiry to be "New Q2", got "${facts[1].inquiry}"` };
// Test 3: Existing array + newElements (append)
allPassed = runTest('Existing array + newElements should append items', () => {
                { inquiry: "Existing Q1", text: "Existing A1" },
                { inquiry: "Existing Q2", text: "Existing A2" }
        newElements: {
                    { inquiry: "Appended Q1", text: "Appended A1" },
                    { inquiry: "Appended Q2", text: "Appended A2" }
    if (facts.length !== 4) {
        return { passed: false, details: `Expected 4 facts, got ${facts.length}` };
    if (facts[0].inquiry !== "Existing Q1") {
        return { passed: false, details: `Expected facts[0] to be existing, got "${facts[0].inquiry}"` };
    if (facts[2].inquiry !== "Appended Q1") {
        return { passed: false, details: `Expected facts[2] to be appended, got "${facts[2].inquiry}"` };
// Test 4: Nested arrays - citations within facts (the real-world bug scenario)
allPassed = runTest('Nested arrays (citations) should be fully replaced on positional update', () => {
                    inquiry: "AAiP question",
                    text: "AAiP answer",
                    citations: [
                        { source: "FAQ", excerpt: "Original citation" }
                        inquiry: "Vectors question",
                        text: "Vectors answer",
                            { source: "Betty", excerpt: "Citation 1" },
                            { source: "Betty", excerpt: "Citation 2" },
                            { source: "Betty", excerpt: "Citation 3" },
                            { source: "Betty", excerpt: "Citation 4" },
                            { source: "Betty", excerpt: "Citation 5" }
                        inquiry: "AAiP question updated",
                        text: "AAiP answer updated",
                            { source: "FAQ", excerpt: "Updated citation" }
    if (facts[0].citations?.length !== 5) {
        return { passed: false, details: `Expected 5 citations in facts[0], got ${facts[0].citations?.length}` };
    if (facts[1].inquiry !== "AAiP question updated") {
        return { passed: false, details: `Expected facts[1] to be the AAiP update, got "${facts[1].inquiry}"` };
// Test 5: Real-world scenario from bug report
allPassed = runTest('Real-world scenario: 1 existing fact, update with 2 facts', () => {
    const startingPayload = {
            faqSearched: true,
                    inquiry: "Why is ongoing education required?",
                    text: "To maintain your AAiP certification...",
                    citations: [{ source: "FAQ", excerpt: "Original" }],
                    toolUsed: "FAQ",
                    confidence: 0.95
                complete: true,
                        inquiry: "Why are vectors difficult to match?",
                        text: "Vectors represent data in high-dimensional space...",
                        toolUsed: "Betty",
                        confidence: 0.9
                        text: "Updated AAiP answer...",
                        citations: [{ source: "FAQ", excerpt: "Updated" }],
    const result = pm.applyAgentChangeRequest(startingPayload, changeRequest);
    const knowledge = (result.result as any).knowledge;
    if (knowledge.facts.length !== 2) {
        return { passed: false, details: `Expected 2 facts, got ${knowledge.facts.length}` };
    if (knowledge.facts[0].citations?.length !== 5) {
        return { passed: false, details: `Expected 5 citations in facts[0], got ${knowledge.facts[0].citations?.length}` };
    if (knowledge.facts[1].inquiry !== "Why is ongoing education required?") {
        return { passed: false, details: `Expected facts[1] to be AAiP question, got "${knowledge.facts[1].inquiry}"` };
    if (knowledge.complete !== true) {
        return { passed: false, details: `Expected knowledge.complete to be true` };
console.log(`\n=== ${allPassed ? 'ALL TESTS PASSED' : 'SOME TESTS FAILED'} ===`);
process.exit(allPassed ? 0 : 1);
