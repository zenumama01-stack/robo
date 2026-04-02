import { NgModule, ModuleWithProviders } from '@angular/core';
import { AngularAuthProviderFactory } from './AngularAuthProviderFactory';
// Import our generic redirect component
import { RedirectComponent } from './redirect.component';
// Export the generic redirect component for backward compatibility
export { RedirectComponent };
 * Extensible authentication module that supports N providers
 * Uses MJGlobal ClassFactory pattern for dynamic provider creation
  imports: [CommonModule],
  declarations: [RedirectComponent],
  exports: [RedirectComponent]
export class AuthServicesModule {
  static forRoot(environment: any): ModuleWithProviders<AuthServicesModule> {
    const providers: any[] = [];
    const authType = environment.AUTH_TYPE?.toLowerCase();
    if (!authType) {
      console.error('No AUTH_TYPE specified in environment');
        ngModule: AuthServicesModule,
        providers: []
    // Use the factory to get provider-specific Angular services
    // This uses the static method on each provider class for extensibility
    const angularServices = AngularAuthProviderFactory.getProviderAngularServices(authType, environment);
    providers.push(...angularServices);
    // Get the provider class from ClassFactory for extensibility
      authType
    const providerClass = registration?.SubClass;
    if (providerClass) {
      // Add the provider itself
      providers.push({
        provide: MJAuthBase,
        useClass: providerClass
      console.error(`No provider class registered for auth type: ${authType}`);
    // Add the factory itself
    providers.push(AngularAuthProviderFactory);
      providers
