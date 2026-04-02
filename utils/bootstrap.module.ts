 * MemberJunction Bootstrap Module
 * Provides the MJBootstrapComponent and all necessary services for MemberJunction Angular applications.
 * This module encapsulates all the authentication and initialization logic that was previously
 * spread across app.component.ts and app.module.ts.
 * @NgModule({
 *   imports: [
 *     BrowserModule,
 *     BrowserAnimationsModule,
 *     MJBootstrapModule.forRoot(environment),
 *     // ... other MJ modules
 *   ],
 *   bootstrap: [AppComponent]
 * })
 * export class AppModule {}
import { NgModule, ModuleWithProviders, APP_INITIALIZER } from '@angular/core';
import { MJBootstrapComponent } from './bootstrap.component';
import { MJAuthShellComponent } from './components/auth-shell.component';
import { MJInitializationService } from './services/initialization.service';
import { MJAuthBase } from '@memberjunction/ng-auth-services';
 * Initialize auth provider before Angular routing starts
 * This ensures auth providers can process OAuth redirect responses before Angular's router
 * consumes the URL hash
export function initializeAuth(authService: MJAuthBase): () => Promise<void> {
  return () => authService.initialize();
    MJBootstrapComponent,
    MJAuthShellComponent
    CommonModule
export class MJBootstrapModule {
   * Configure the bootstrap module with environment settings
   * @param environment - Environment configuration for the application
   * @returns ModuleWithProviders with environment configuration and all services
   * import { environment } from '../environments/environment';
   *     MJBootstrapModule.forRoot(environment)
  static forRoot(environment: MJEnvironmentConfig): ModuleWithProviders<MJBootstrapModule> {
      ngModule: MJBootstrapModule,
        { provide: MJ_ENVIRONMENT, useValue: environment },
        // MJInitializationService uses providedIn: 'root'
          provide: APP_INITIALIZER,
          useFactory: initializeAuth,
          deps: [MJAuthBase],
          multi: true
