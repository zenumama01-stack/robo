 * @fileoverview Base engine for test metadata management
 * @module @memberjunction/testing-engine-base
    MJTestSuiteTestEntity
 * Base engine for test framework metadata management.
 * This class handles loading and caching test metadata (types, tests, suites, rubrics).
 * It does NOT contain execution logic - that's in TestEngine.
 * This separation allows the metadata to be safely used in UI contexts.
 * Follows pattern from ActionEngineBase, SchedulingEngineBase.
 * const engine = TestEngineBase.Instance;
 * const types = engine.TestTypes;
 * const tests = engine.Tests;
export class TestEngineBase extends BaseEngine<TestEngineBase> {
    private _testTypes: MJTestTypeEntity[] = [];
    private _tests: MJTestEntity[] = [];
    private _testSuites: MJTestSuiteEntity[] = [];
    private _testSuiteTests: MJTestSuiteTestEntity[] = [];
    private _testRubrics: MJTestRubricEntity[] = [];
     * Singleton instance accessor
    public static get Instance(): TestEngineBase {
        return super.getInstance<TestEngineBase>();
     * All loaded test types
    public get TestTypes(): MJTestTypeEntity[] {
        return this._testTypes;
     * All loaded tests
    public get Tests(): MJTestEntity[] {
        return this._tests;
     * All loaded test suites
    public get TestSuites(): MJTestSuiteEntity[] {
        return this._testSuites;
     * All loaded test suite tests
    public get TestSuiteTests(): MJTestSuiteTestEntity[] {
        return this._testSuiteTests;
     * All loaded test rubrics
    public get TestRubrics(): MJTestRubricEntity[] {
        return this._testRubrics;
     * Configure and load metadata
     * @param forceRefresh - Force reload even if already loaded
                PropertyName: '_testTypes',
                EntityName: 'MJ: Test Types',
                PropertyName: '_tests',
                EntityName: 'MJ: Tests',
                PropertyName: '_testSuites',
                EntityName: 'MJ: Test Suites',
                PropertyName: '_testRubrics',
                EntityName: 'MJ: Test Rubrics',
                PropertyName: '_testSuiteTests',
        return await this.Load(params, provider!, forceRefresh, contextUser);
     * Get test type by ID
    public GetTestTypeByID(id: string): MJTestTypeEntity | undefined {
        return this._testTypes.find(t => t.ID === id);
     * Get test type by name
    public GetTestTypeByName(name: string): MJTestTypeEntity | undefined {
        return this._testTypes.find(t => t.Name === name);
     * Get test by ID
    public GetTestByID(id: string): MJTestEntity | undefined {
        return this._tests.find(t => t.ID === id);
     * Get test by name
    public GetTestByName(name: string): MJTestEntity | undefined {
        return this._tests.find(t => t.Name === name);
     * Get test suite by ID
    public GetTestSuiteByID(id: string): MJTestSuiteEntity | undefined {
        return this._testSuites.find(s => s.ID === id);
     * Get test suite by name
    public GetTestSuiteByName(name: string): MJTestSuiteEntity | undefined {
        return this._testSuites.find(s => s.Name === name);
     * Get test rubric by ID
    public GetTestRubricByID(id: string): MJTestRubricEntity | undefined {
        return this._testRubrics.find(r => r.ID === id);
     * Get test rubric by name
    public GetTestRubricByName(name: string): MJTestRubricEntity | undefined {
        return this._testRubrics.find(r => r.Name === name);
     * Get tests by type
    public GetTestsByType(typeId: string): MJTestEntity[] {
        return this._tests.filter(t => t.TypeID === typeId);
     * Get tests by tag
    public GetTestsByTag(tag: string): MJTestEntity[] {
        return this._tests.filter(t => {
                return tags.includes(tag);
     * Returns all of the tests associated with a given test suite, sorted by their sequence.
     * @param suiteId 
    public GetTestsForSuite(suiteId: string): MJTestEntity[] {
        const suiteTests = this._testSuiteTests.filter(t => t.SuiteID === suiteId);
        const tests: MJTestEntity[] = [];
            const test = this.GetTestByID(st.TestID);
                tests.push(test);
        return tests.sort((a, b) => {
            const aSuiteTest = suiteTests.find(st => st.TestID === a.ID);
            const bSuiteTest = suiteTests.find(st => st.TestID === b.ID);
            if (aSuiteTest && bSuiteTest) {
                return aSuiteTest.Sequence - bSuiteTest.Sequence;
     * Get active tests (Status = 'Active')
    public GetActiveTests(): MJTestEntity[] {
        return this._tests.filter(t => t.Status === 'Active');
     * Get active test suites (Status = 'Active')
    public GetActiveTestSuites(): MJTestSuiteEntity[] {
        return this._testSuites.filter(s => s.Status === 'Active');
