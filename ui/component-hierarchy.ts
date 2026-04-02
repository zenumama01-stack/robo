 * @fileoverview Component hierarchy registration utilities for MemberJunction React Runtime.
 * Provides functionality to register a hierarchy of components from Skip component specifications.
 * @module @memberjunction/react-runtime/hierarchy
  CompiledComponent
import { ComponentSpec, ComponentStyles } from '@memberjunction/interactive-component-types';
import { UserInfo, Metadata, LogStatus, GetProductionStatus } from '@memberjunction/core';
 * Result of a hierarchy registration operation
export interface HierarchyRegistrationResult {
  registeredComponents: string[];
  errors: ComponentRegistrationError[];
  /** The fully resolved component specification with all dependencies and libraries */
 * Error information for component registration
export interface ComponentRegistrationError {
  phase: 'compilation' | 'registration' | 'validation';
 * Options for hierarchy registration
export interface HierarchyRegistrationOptions {
  /** Component styles to apply to all components */
  styles?: ComponentStyles;
  /** Namespace for component registration */
  /** Version for component registration */
  /** Whether to continue on errors */
  /** Whether to override existing components */
  allowOverride?: boolean;
   * Required, metadata for all possible libraries allowed by the system
  allLibraries: MJComponentLibraryEntity[];
  /** Optional user context for fetching from external registries */
 * Utility class for registering component hierarchies
export class ComponentHierarchyRegistrar {
    private compiler: ComponentCompiler,
    private registry: ComponentRegistry,
    private runtimeContext: RuntimeContext
   * Fetches a component specification from an external registry
  private async fetchExternalComponent(
  ): Promise<ComponentSpec | null> {
      if (!provider || !(provider as any).ExecuteGQL) {
        console.warn('⚠️ [ComponentHierarchyRegistrar] No GraphQL provider available for external registry fetch');
      // Dynamically import the GraphQL client to avoid circular dependencies
      const graphQLClient = new GraphQLComponentRegistryClient(provider as any);
      const fullSpec = await graphQLClient.GetRegistryComponent({
        registryName: spec.registry!,
      if (fullSpec && fullSpec.code) {
        if (!GetProductionStatus()) {
          LogStatus(`✅ [ComponentHierarchyRegistrar] Fetched external component ${spec.name} with code (${fullSpec.code.length} chars)`);
        console.warn(`⚠️ [ComponentHierarchyRegistrar] Failed to fetch external component ${spec.name} or no code`);
      console.error(`❌ [ComponentHierarchyRegistrar] Error fetching external component ${spec.name}:`, error);
   * Registers a complete component hierarchy from a root specification
   * @param rootSpec - The root component specification
   * @returns Registration result with details about success/failures
  async registerHierarchy(
    options: HierarchyRegistrationOptions
  ): Promise<HierarchyRegistrationResult> {
    // If this is an external registry component without code, fetch it first
    let resolvedRootSpec = rootSpec;
    if (rootSpec.location === 'registry' && rootSpec.registry && !rootSpec.code) {
        LogStatus(`🌐 [ComponentHierarchyRegistrar] Fetching external registry component: ${rootSpec.registry}/${rootSpec.name}`);
      resolvedRootSpec = await this.fetchExternalComponent(rootSpec, options.contextUser) || rootSpec;
      namespace = 'Global',
      version = 'v1',
      continueOnError = true,
      allowOverride = true
    const registeredComponents: string[] = [];
    const errors: ComponentRegistrationError[] = [];
      LogStatus('🌳 ComponentHierarchyRegistrar.registerHierarchy:', undefined, {
        rootComponent: resolvedRootSpec.name,
        hasLibraries: !!(resolvedRootSpec.libraries && resolvedRootSpec.libraries.length > 0),
        libraryCount: resolvedRootSpec.libraries?.length || 0
    // PHASE 1: Compile all components first (but defer factory execution)
    const compiledMap = new Map<string, CompiledComponent>();
    const specMap = new Map<string, ComponentSpec>();
    const allLoadedLibraries = new Map<string, any>(); // Track all loaded libraries
    // Helper to compile a component without calling its factory
    const compileOnly = async (spec: ComponentSpec): Promise<{ success: boolean; error?: ComponentRegistrationError }> => {
      if (!spec.code) return { success: true };
        // Filter out invalid library entries before compilation
        const validLibraries = spec.libraries?.filter(lib => {
          if (!lib || typeof lib !== 'object') return false;
          if (!lib.name || lib.name === 'unknown' || lib.name === 'null' || lib.name === 'undefined') return false;
          if (!lib.globalVariable || lib.globalVariable === 'undefined' || lib.globalVariable === 'null') return false;
        const compileOptions: CompileOptions = {
          allLibraries: options.allLibraries
        const result = await this.compiler.compile(compileOptions);
          compiledMap.set(spec.name, result.component);
          specMap.set(spec.name, spec);
          // Extract and accumulate loaded libraries from the compilation
          if (result.loadedLibraries) {
              if (!allLoadedLibraries.has(key)) {
                allLoadedLibraries.set(key, value);
                  LogStatus(`📚 [registerHierarchy] Added library ${key} to accumulated libraries`);
              error: result.error?.message || 'Unknown compilation error',
              phase: 'compilation'
    // Compile all components in hierarchy
    const compileQueue = [resolvedRootSpec];
    while (compileQueue.length > 0) {
      let spec = compileQueue.shift()!;
      if (visited.has(spec.name)) continue;
      if (spec.location === 'registry' && spec.registry && !spec.code) {
        const fetched = await this.fetchExternalComponent(spec, options.contextUser);
        if (fetched) {
          spec = fetched;
          console.warn(`⚠️ [ComponentHierarchyRegistrar] Could not fetch external component ${spec.name}, skipping`);
      const result = await compileOnly(spec);
        errors.push(result.error!);
          return { success: false, registeredComponents, errors, warnings, resolvedSpec: resolvedRootSpec };
      if (spec.dependencies) {
        compileQueue.push(...spec.dependencies);
    // Add all accumulated libraries to runtime context
    if (allLoadedLibraries.size > 0) {
      allLoadedLibraries.forEach((value, key) => {
          LogStatus(`✅ [registerHierarchy] Added ${key} to runtime context libraries`);
    // PHASE 2: Execute all factories with components available
    for (const [name, compiled] of compiledMap) {
      const spec = specMap.get(name)!;
      // Build components object from all registered components
      const components: Record<string, any> = {};
      for (const [depName, depCompiled] of compiledMap) {
        // Call factory to get ComponentObject, then extract React component
        const depObject = depCompiled.factory(this.runtimeContext, styles);
        components[depName] = depObject.component;
      // Now call factory with components available
      const componentObject = compiled.factory(this.runtimeContext, styles, components);
      // Register in registry
        componentObject,
      registeredComponents.push(spec.name);
      registeredComponents,
      resolvedSpec: resolvedRootSpec
   * Registers a single component from a specification
   * @returns Registration result for this component
  async registerSingleComponent(
  ): Promise<{ success: boolean; error?: ComponentRegistrationError }> {
    const { styles, namespace = 'Global', version = 'v1', allowOverride = true } = options;
      // Skip if no component code
      if (!spec.code) {
      const existingComponent = this.registry.get(spec.name, namespace, version);
      if (existingComponent && !allowOverride) {
            error: `Component already registered in ${namespace}/${version}`,
            phase: 'registration'
        LogStatus(`🔧 Compiling component ${spec.name} with libraries:`, undefined, {
          originalCount: spec.libraries?.length || 0,
          filteredCount: validLibraries?.length || 0,
          libraries: validLibraries?.map(l => l.name) || []
        libraries: validLibraries, // Pass along filtered library dependencies
        dependencies: spec.dependencies, // Pass along child component dependencies
      const compilationResult = await this.compiler.compile(compileOptions);
            error: compilationResult.error?.message || 'Unknown compilation error',
      if (compilationResult.loadedLibraries && compilationResult.loadedLibraries.size > 0) {
        compilationResult.loadedLibraries.forEach((value, key) => {
            LogStatus(`✅ [registerSingleComponent] Added ${key} to runtime context libraries`);
      // Call the factory to create the ComponentObject
      // IMPORTANT: We don't pass components here because child components may not be registered yet
      // Components are resolved later when the component is actually rendered
        LogStatus(`🏭 Calling factory for ${spec.name} with runtime context:`, undefined, {
          hasReact: !!this.runtimeContext.React,
          hasReactDOM: !!this.runtimeContext.ReactDOM,
          libraryCount: Object.keys(this.runtimeContext.libraries || {}).length
      const componentObject = compilationResult.component!.factory(this.runtimeContext, styles);
      // Register the full ComponentObject (not just the React component)
   * Recursively registers child components
   * @param children - Array of child component specifications
   * @param registeredComponents - Array to track registered components
   * @param errors - Array to collect errors
   * @param warnings - Array to collect warnings
  private async registerChildComponents(
    children: ComponentSpec[],
    options: HierarchyRegistrationOptions,
    registeredComponents: string[],
    errors: ComponentRegistrationError[],
      // Register this child
      const childResult = await this.registerSingleComponent(child, {
        styles: options.styles,
        namespace: options.namespace,
        version: options.version,
        allowOverride: options.allowOverride,
      if (childResult.success) {
        if (child.code) {
          registeredComponents.push(child.name);
        errors.push(childResult.error!);
        if (!options.continueOnError) {
      // Register nested children recursively
      const nestedChildren = child.dependencies || [];
      if (nestedChildren.length > 0) {
        await this.registerChildComponents(
          nestedChildren,
 * Convenience function to register a component hierarchy
 * @param compiler - Component compiler instance
 * @param registry - Component registry instance
 * @param runtimeContext - Runtime context with React and other libraries
 * @returns Registration result
export async function registerComponentHierarchy(
  const registrar = new ComponentHierarchyRegistrar(compiler, registry, runtimeContext);
  return registrar.registerHierarchy(rootSpec, options);
 * Validates a component specification before registration
 * @returns Array of validation errors (empty if valid)
export function validateComponentSpec(spec: ComponentSpec): string[] {
  if (!spec.name) {
    errors.push('Component specification must have a name');
  // If componentCode is provided, do basic validation
  if (spec.code) {
    if (typeof spec.code !== 'string') {
      errors.push(`Component code for ${spec.name} must be a string`);
    if (spec.code.trim().length === 0) {
      errors.push(`Component code for ${spec.name} cannot be empty`);
  // Validate child components recursively
  children.forEach((child, index) => {
    const childErrors = validateComponentSpec(child);
    childErrors.forEach(error => {
      errors.push(`Child ${index} (${child.name || 'unnamed'}): ${error}`);
 * Flattens a component hierarchy into a list of all components
 * @returns Array of all component specifications in the hierarchy
export function flattenComponentHierarchy(rootSpec: ComponentSpec): ComponentSpec[] {
  const components: ComponentSpec[] = [rootSpec];
  const children = rootSpec.dependencies || [];
    components.push(...flattenComponentHierarchy(child));
 * Counts the total number of components in a hierarchy
 * @param includeEmpty - Whether to include components without code
 * @returns Total component count
export function countComponentsInHierarchy(
  includeEmpty: boolean = false
  if (includeEmpty || rootSpec.code) {
    count += countComponentsInHierarchy(child, includeEmpty);
