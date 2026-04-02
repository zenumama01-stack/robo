import { AuthProviderConfig, LogError, LogStatus } from '@memberjunction/core';
 * Initialize authentication providers from configuration
export function initializeAuthProviders(): void {
  // Clear any existing providers
  factory.clear();
  // Initialize providers from authProviders config
  if (configInfo.authProviders && configInfo.authProviders.length > 0) {
    for (const providerConfig of configInfo.authProviders) {
        const provider = AuthProviderFactory.createProvider(providerConfig as AuthProviderConfig);
        factory.register(provider);
        LogStatus(`Registered auth provider: ${provider.name} (type: ${providerConfig.type})`);
        LogError(`Failed to initialize auth provider ${providerConfig.name}: ${error}`);
  // Validate we have at least one provider
    LogError('No authentication providers configured. Please configure authProviders array in mj.config.cjs');
