 * Prompt execution helpers with model/vendor override support
 * Execute a prompt with optional model/vendor overrides from QueryGenConfig
 * Uses AIPromptParams.override parameter to apply runtime model/vendor overrides.
 * If config specifies modelOverride or vendorOverride, looks up their IDs from
 * the AIEngine cache and passes them to the prompt execution.
 * @param prompt - The AI prompt to execute (from AIEngine.Instance.Prompts)
 * @param data - Data to pass to the prompt template
 * @param config - QueryGen configuration (for model/vendor overrides)
 * @returns Promise resolving to the prompt result
export async function executePromptWithOverrides<T>(
): Promise<{ success: boolean; result?: T; errorMessage?: string }> {
  promptParams.data = data;
  promptParams.skipValidation = false;
  // Apply model/vendor overrides using built-in AIPromptParams.override
    const overrideIds = resolveModelVendorOverrides(config);
    if (overrideIds.modelId || overrideIds.vendorId) {
      promptParams.override = overrideIds;
  return await runner.ExecutePrompt<T>(promptParams);
 * Resolve model/vendor names to IDs for AIPromptParams.override
 * Looks up model and vendor by name in the AIEngine cache (already loaded).
 * @param config - QueryGen configuration with modelOverride/vendorOverride names
 * @returns Object with modelId and/or vendorId, or empty object if none found
function resolveModelVendorOverrides(
): { modelId?: string; vendorId?: string } {
  const result: { modelId?: string; vendorId?: string } = {};
  // Look up model ID from AIEngine cache if modelOverride is set
  if (config.modelOverride) {
    const model = AIEngine.Instance.Models.find(m => m.Name === config.modelOverride);
    if (model && model.ID) {
      result.modelId = model.ID;
  // Look up vendor ID from AIEngine cache if vendorOverride is set
  if (config.vendorOverride) {
    const vendor = AIEngine.Instance.Vendors.find(v => v.Name === config.vendorOverride);
    if (vendor && vendor.ID) {
      result.vendorId = vendor.ID;
