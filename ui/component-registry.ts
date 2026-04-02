 * @fileoverview Platform-agnostic component registry for managing compiled React components.
 * Provides storage, retrieval, and lifecycle management for components with namespace support.
  RegistryEntry, 
  ComponentMetadata, 
  RegistryConfig 
import { ComponentObject } from '@memberjunction/interactive-component-types';
import { resourceManager } from '../utilities/resource-manager';
 * Default registry configuration
const DEFAULT_REGISTRY_CONFIG: RegistryConfig = {
  cleanupInterval: 60000, // 1 minute
 * Platform-agnostic component registry.
 * Manages compiled React components with namespace isolation and lifecycle management.
export class ComponentRegistry {
  private registry: Map<string, RegistryEntry>;
  private config: RegistryConfig;
  private cleanupTimer?: NodeJS.Timeout | number;
  public readonly registryId: string;
   * Creates a new ComponentRegistry instance
   * @param config - Optional registry configuration
  constructor(config?: Partial<RegistryConfig>) {
    this.config = { ...DEFAULT_REGISTRY_CONFIG, ...config };
    this.registry = new Map();
    this.registryId = `component-registry-${Date.now()}`;
    // Start cleanup timer if configured
    if (this.config.cleanupInterval > 0) {
   * Registers a compiled component
   * @param component - Compiled component object
   * @param namespace - Component namespace (default: 'Global')
   * @param version - Component version (default: 'v1')
   * @param tags - Optional tags for categorization
   * @returns The registered component's metadata
  register(
    component: ComponentObject,
    version: string = 'v1',
    tags?: string[]
  ): ComponentMetadata {
    const id = this.generateRegistryKey(name, namespace, version);
    // Create metadata
    const metadata: ComponentMetadata = {
    // Create registry entry
    const entry: RegistryEntry = {
      component,
      refCount: 0
    // Check capacity
    if (this.registry.size >= this.config.maxComponents && this.config.useLRU) {
      this.evictLRU();
    // Store in registry
    this.registry.set(id, entry);
   * Gets a component from the registry
   * @returns The component object if found, undefined otherwise
  get(name: string, namespace: string = 'Global', version?: string): ComponentObject | undefined {
    const id = version 
      ? this.generateRegistryKey(name, namespace, version)
      : this.findLatestVersion(name, namespace);
    if (!id) return undefined;
    const entry = this.registry.get(id);
      // Update access time and increment ref count
      entry.lastAccessed = new Date();
      entry.refCount++;
      return entry.component;
   * Checks if a component exists in the registry
   * @returns true if the component exists
  has(name: string, namespace: string = 'Global', version?: string): boolean {
    return id ? this.registry.has(id) : false;
   * Removes a component from the registry
   * @returns true if the component was removed
  unregister(name: string, namespace: string = 'Global', version?: string): boolean {
    return this.registry.delete(id);
   * Gets all components in a namespace
   * @param namespace - Namespace to query
   * @returns Array of components in the namespace
  getNamespace(namespace: string): ComponentMetadata[] {
    const components: ComponentMetadata[] = [];
    for (const entry of this.registry.values()) {
      if (entry.metadata.namespace === namespace) {
        components.push(entry.metadata);
    return components;
   * Gets all components in a namespace and version as a map
   * @param namespace - Namespace to query (default: 'Global')
   * @param version - Version to query (default: 'v1')
   * @returns Object mapping component names to component objects
  getAll(namespace: string = 'Global', version: string = 'v1'): Record<string, ComponentObject> {
      if (entry.metadata.namespace === namespace && entry.metadata.version === version) {
        components[entry.metadata.name] = entry.component;
   * Gets all registered namespaces
   * @returns Array of unique namespace names
  getNamespaces(): string[] {
    const namespaces = new Set<string>();
      namespaces.add(entry.metadata.namespace);
    return Array.from(namespaces);
   * Gets components by tags
   * @param tags - Tags to search for
   * @returns Array of components matching any of the tags
  getByTags(tags: string[]): ComponentMetadata[] {
      if (entry.metadata.tags?.some(tag => tags.includes(tag))) {
   * Decrements reference count for a component
  release(name: string, namespace: string = 'Global', version?: string): void {
    if (!id) return;
    if (entry && entry.refCount > 0) {
      entry.refCount--;
   * Clears all components from the registry
    this.registry.clear();
   * Clear all components in a specific namespace
   * @param namespace - Namespace to clear (default: 'Global')
   * @returns Number of components removed
  clearNamespace(namespace: string = 'Global'): number {
    for (const [key, entry] of this.registry) {
      this.registry.delete(key);
    return toRemove.length;
   * Force clear all components and reset registry
   * Used for development/testing scenarios
  forceClear(): void {
    this.stopCleanupTimer();
    console.log('🧹 Registry force cleared - all components removed');
   * Gets the current size of the registry
   * @returns Number of registered components
  size(): number {
    return this.registry.size;
   * Performs cleanup of unused components
   * @param force - Force cleanup regardless of reference count
  cleanup(force: boolean = false): number {
    for (const [id, entry] of this.registry) {
      // Remove if no references and hasn't been accessed recently
      const timeSinceAccess = now - entry.lastAccessed.getTime();
      const isUnused = entry.refCount === 0 && timeSinceAccess > this.config.cleanupInterval;
      if (force || isUnused) {
        toRemove.push(id);
    for (const id of toRemove) {
      this.registry.delete(id);
   * Gets registry statistics
   * @returns Object containing registry stats
    totalComponents: number;
    namespaces: number;
    totalRefCount: number;
    oldestComponent?: Date;
    newestComponent?: Date;
    let totalRefCount = 0;
    let oldest: Date | undefined;
    let newest: Date | undefined;
      totalRefCount += entry.refCount;
      if (!oldest || entry.metadata.registeredAt < oldest) {
        oldest = entry.metadata.registeredAt;
      if (!newest || entry.metadata.registeredAt > newest) {
        newest = entry.metadata.registeredAt;
      totalComponents: this.registry.size,
      namespaces: this.getNamespaces().length,
      totalRefCount,
      oldestComponent: oldest,
      newestComponent: newest
   * Destroys the registry and cleans up resources
    // Clean up any resources associated with this registry
    resourceManager.cleanupComponent(this.registryId);
   * Generates a unique registry key
   * @returns Registry key
  private generateRegistryKey(name: string, namespace: string, version: string): string {
    if (this.config.enableNamespaces) {
      return `${namespace}::${name}@${version}`;
    return `${name}@${version}`;
   * Finds the latest version of a component
   * @returns Registry key of latest version or undefined
  private findLatestVersion(name: string, namespace: string): string | undefined {
    let latestKey: string | undefined;
    let latestDate: Date | undefined;
      if (entry.metadata.name === name && 
          entry.metadata.namespace === namespace) {
        if (!latestDate || entry.metadata.registeredAt > latestDate) {
          latestDate = entry.metadata.registeredAt;
          latestKey = key;
    return latestKey;
   * Evicts the least recently used component
  private evictLRU(): void {
    let lruKey: string | undefined;
    let lruTime: Date | undefined;
      // Skip components with active references
      if (entry.refCount > 0) continue;
      if (!lruTime || entry.lastAccessed < lruTime) {
        lruTime = entry.lastAccessed;
        lruKey = key;
      this.registry.delete(lruKey);
   * Starts the automatic cleanup timer
    this.cleanupTimer = resourceManager.setInterval(
      this.registryId,
      this.config.cleanupInterval,
      { purpose: 'component-registry-cleanup' }
   * Stops the automatic cleanup timer
  private stopCleanupTimer(): void {
      resourceManager.clearInterval(this.registryId, this.cleanupTimer as number);
      this.cleanupTimer = undefined;
