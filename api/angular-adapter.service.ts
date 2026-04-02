 * @fileoverview Angular adapter service that bridges the React runtime with Angular.
 * Provides Angular-specific functionality for the platform-agnostic React runtime.
  ComponentCompiler,
  ComponentRegistry,
  ComponentResolver,
  ComponentManager,
  createReactRuntime,
  CompileOptions,
  RuntimeContext,
  ExternalLibraryConfig,
  LibraryConfiguration,
  SetupStyles
import { ScriptLoaderService } from './script-loader.service';
import { ComponentStyles } from '@memberjunction/interactive-component-types';
 * Angular-specific adapter for the React runtime.
 * Manages the integration between Angular services and the platform-agnostic React runtime.
export class AngularAdapterService {
  private runtime?: {
    compiler: ComponentCompiler;
    registry: ComponentRegistry;
    resolver: ComponentResolver;
    manager: ComponentManager;
  private runtimeContext?: RuntimeContext;
  private initializationPromise: Promise<void> | undefined;
  constructor(private scriptLoader: ScriptLoaderService) {}
   * Initialize the React runtime with Angular-specific configuration
   * @param config Optional library configuration
   * @param additionalLibraries Optional additional libraries to merge
   * @param options Optional options including debug flag
   * @returns Promise resolving when runtime is ready
  async initialize(
    config?: LibraryConfiguration,
    additionalLibraries?: ExternalLibraryConfig[],
    options?: { debug?: boolean }
    if (this.runtime) {
      return this.initializationPromise; // in progress
    // Start initialization and store the promise immediately
    this.initializationPromise = this.doInitialize(config, additionalLibraries, options);
        // Clear the promise on error so it can be retried
        this.initializationPromise = undefined;
  private async doInitialize(
    // Load React ecosystem with optional additional libraries
    const ecosystem = await this.scriptLoader.loadReactEcosystem(config, additionalLibraries, options);
    // Create runtime context
    this.runtimeContext = {
      React: ecosystem.React,
      ReactDOM: ecosystem.ReactDOM,
      libraries: ecosystem.libraries,
      utilities: {
        // Add any Angular-specific utilities here
    // Create the React runtime with runtime context for registry support
    this.runtime = createReactRuntime(ecosystem.Babel, {
      compiler: {
        maxCacheSize: 100,
        debug: options?.debug
      registry: {
        maxComponents: 1000,
        cleanupInterval: 60000,
        useLRU: true,
        enableNamespaces: true,
    }, this.runtimeContext, options?.debug);
   * Get the component compiler
   * @returns Component compiler instance
  getCompiler(): ComponentCompiler {
    if (!this.runtime) {
      throw new Error('React runtime not initialized. Call initialize() first.');
    return this.runtime.compiler;
   * Get the component registry
   * @returns Component registry instance
  getRegistry(): ComponentRegistry {
    return this.runtime.registry;
   * Get the component resolver
   * @returns Component resolver instance
  getResolver(): ComponentResolver {
    return this.runtime.resolver;
   * Get the runtime context
   * @returns Runtime context with React and libraries
  getRuntimeContext(): RuntimeContext {
    if (!this.runtimeContext) {
    return this.runtimeContext;
   * Get the unified component manager
   * @returns Component manager instance
  getComponentManager(): ComponentManager {
    return this.runtime.manager;
   * Compile a component with Angular-specific defaults
   * @param options - Compilation options
   * @returns Promise resolving to compilation result
  async compileComponent(options: CompileOptions) {
    // Validate options before initialization
    if (!options) {
        'Angular adapter error: No compilation options provided.\n' +
        'This usually means the component spec is null or undefined.\n' +
        'Please check that:\n' +
        '1. Your component data is loaded properly\n' +
        '2. The component spec has "name" and "code" properties\n' +
        '3. The component input is not undefined'
    if (!options.componentName || options.componentName.trim() === '') {
        'Angular adapter error: Component name is missing or empty.\n' +
        `Received options: ${JSON.stringify(options, null, 2)}\n` +
        'Make sure your component spec includes a "name" property.'
    if (!options.componentCode || options.componentCode.trim() === '') {
        `Angular adapter error: Component code is missing or empty for component "${options.componentName}".\n` +
        'Make sure your component spec includes a "code" property with the React component source.'
    // Apply default styles if not provided
    const optionsWithDefaults = {
      styles: options.styles || SetupStyles()
    return this.runtime!.compiler.compile(optionsWithDefaults);
   * Register a component in the registry
   * @param name - Component name
   * @param component - Compiled component
   * @param namespace - Component namespace
   * @param version - Component version
   * @returns Component metadata
  registerComponent(
    component: any,
    namespace: string = 'Global',
    version: string = 'v1'
    return this.runtime.registry.register(name, component, namespace, version);
   * Get a component from the registry
   * @returns Component if found
  getComponent(name: string, namespace: string = 'Global', version?: string) {
    return this.runtime.registry.get(name, namespace, version);
   * Check if runtime is initialized
   * @returns true if initialized
  isInitialized(): boolean {
    return !!this.runtime && !!this.runtimeContext;
   * Get runtime version
   * @returns Runtime version string
  getVersion(): string {
    return this.runtime?.version || 'unknown';
      this.runtime.registry.destroy();
      this.runtime = undefined;
      this.runtimeContext = undefined;
   * Get Babel instance for direct use
   * @returns Babel instance
  getBabel(): any {
    return this.runtimeContext?.libraries?.Babel || (window as any).Babel;
   * Transpile JSX code directly
   * @param code - JSX code to transpile
   * @param filename - Optional filename for better error messages
   * @returns Transpiled JavaScript code
  transpileJSX(code: string, filename?: string): string {
    const babel = this.getBabel();
    if (!babel) {
      throw new Error('Babel not loaded. Initialize the runtime first.');
      const result = babel.transform(code, {
        presets: ['react'],
        filename: filename || 'component.jsx'
      return result.code;
      throw new Error(`Failed to transpile JSX: ${error.message}`);
