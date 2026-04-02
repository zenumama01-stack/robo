 * MemberJunction Explorer Application Module
 * Provides the complete branded Explorer application shell.
 * Use forRoot() to configure with environment settings.
 *     MJExplorerAppModule.forRoot(environment)
import { RouterModule } from '@angular/router';
import { MJExplorerAppComponent } from './explorer-app.component';
import { MJEnvironmentConfig, MJ_ENVIRONMENT, MJ_STARTUP_VALIDATION } from '@memberjunction/ng-bootstrap';
import { ShellModule, StartupValidationService, SystemValidationBannerComponent } from '@memberjunction/ng-explorer-core';
    MJExplorerAppComponent
    RouterModule,
    ShellModule,
    SystemValidationBannerComponent  // Standalone component
export class MJExplorerAppModule {
   * Configure the Explorer App module with environment settings.
   * Should be called once in the root application module.
  static forRoot(environment: MJEnvironmentConfig): ModuleWithProviders<MJExplorerAppModule> {
      ngModule: MJExplorerAppModule,
          provide: MJ_ENVIRONMENT,
          useValue: environment
          provide: MJ_STARTUP_VALIDATION,
          useClass: StartupValidationService
