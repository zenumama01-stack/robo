// Mock the generated manifest module
vi.mock('../mj-class-registrations.js', () => ({
    CLASS_REGISTRATIONS: [],
    CLASS_REGISTRATIONS_MANIFEST_LOADED: true,
    CLASS_REGISTRATIONS_COUNT: 0,
    CLASS_REGISTRATIONS_PACKAGES: [],
describe('ServerBootstrapLite', () => {
    it('should export CLASS_REGISTRATIONS array', async () => {
        expect(mod.CLASS_REGISTRATIONS).toBeDefined();
        expect(Array.isArray(mod.CLASS_REGISTRATIONS)).toBe(true);
    it('should export CLASS_REGISTRATIONS_MANIFEST_LOADED flag', async () => {
        expect(mod.CLASS_REGISTRATIONS_MANIFEST_LOADED).toBeDefined();
        expect(typeof mod.CLASS_REGISTRATIONS_MANIFEST_LOADED).toBe('boolean');
    it('should export CLASS_REGISTRATIONS_COUNT', async () => {
        expect(mod.CLASS_REGISTRATIONS_COUNT).toBeDefined();
        expect(typeof mod.CLASS_REGISTRATIONS_COUNT).toBe('number');
    it('should export CLASS_REGISTRATIONS_PACKAGES', async () => {
        expect(mod.CLASS_REGISTRATIONS_PACKAGES).toBeDefined();
        expect(Array.isArray(mod.CLASS_REGISTRATIONS_PACKAGES)).toBe(true);
