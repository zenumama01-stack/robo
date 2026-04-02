 * The Bundle package is a no-op retained for backward compatibility.
 * The index.ts is effectively empty (just a JSDoc comment).
 * This test verifies the module can be imported without errors.
describe('AI Provider Bundle', () => {
  it('should import the module without errors', async () => {
    const mod = await import('../index');
    expect(mod).toBeDefined();
  it('should export an empty module (no-op for backward compatibility)', async () => {
    // The module is empty – it only contains a comment
    expect(Object.keys(mod)).toHaveLength(0);
