 * @fileoverview Centralized resource management for React Runtime
 * Handles timers, DOM elements, event listeners, and other resources that need cleanup
// Type alias for timer IDs that works in both browser and Node.js
export type TimerId = number | NodeJS.Timeout;
export interface ManagedResource {
  type: 'timer' | 'interval' | 'animationFrame' | 'eventListener' | 'domElement' | 'observable' | 'reactRoot';
  id: string | TimerId;
  cleanup: () => void;
 * Environment-agnostic timer functions that work in both browser and Node.js
const getTimerFunctions = () => {
  // Check if we're in a browser environment
  if (typeof window !== 'undefined' && window.setTimeout) {
      setTimeout: window.setTimeout.bind(window),
      clearTimeout: window.clearTimeout.bind(window),
      setInterval: window.setInterval.bind(window),
      clearInterval: window.clearInterval.bind(window),
      requestAnimationFrame: window.requestAnimationFrame?.bind(window),
      cancelAnimationFrame: window.cancelAnimationFrame?.bind(window)
  else if (typeof global !== 'undefined' && global.setTimeout) {
      setTimeout: global.setTimeout,
      clearTimeout: global.clearTimeout,
      setInterval: global.setInterval,
      clearInterval: global.clearInterval,
      requestAnimationFrame: undefined, // Not available in Node.js
      cancelAnimationFrame: undefined
  // Fallback - return no-op functions
    const noop = () => {};
    const noopWithReturn = () => 0;
      setTimeout: noopWithReturn,
      clearTimeout: noop,
      setInterval: noopWithReturn,
      clearInterval: noop,
      requestAnimationFrame: undefined,
const timers = getTimerFunctions();
 * ResourceManager provides centralized management of resources that need cleanup.
 * This prevents memory leaks by ensuring all resources are properly disposed.
export class ResourceManager {
  private resources = new Map<string, Set<ManagedResource>>();
  private globalResources = new Set<ManagedResource>();
  private cleanupCallbacks = new Map<string, (() => void)[]>();
   * Register a timeout with automatic cleanup
  setTimeout(
    callback: () => void,
    delay: number,
    metadata?: Record<string, any>
    const id = timers.setTimeout(() => {
      this.removeResource(componentId, 'timer', id);
    this.addResource(componentId, {
      type: 'timer',
      cleanup: () => timers.clearTimeout(id),
    return id as any;
   * Register an interval with automatic cleanup
  setInterval(
    const id = timers.setInterval(callback, delay);
      type: 'interval',
      cleanup: () => timers.clearInterval(id),
   * Register an animation frame with automatic cleanup
  requestAnimationFrame(
    callback: FrameRequestCallback,
  ): TimerId {
    if (!timers.requestAnimationFrame) {
      // Fallback to setTimeout in non-browser environments
      return this.setTimeout(componentId, () => callback(Date.now()), 16, metadata);
    const id = timers.requestAnimationFrame((time) => {
      this.removeResource(componentId, 'animationFrame', id);
      callback(time);
      type: 'animationFrame',
      cleanup: () => timers.cancelAnimationFrame?.(id),
   * Clear a specific timeout
  clearTimeout(componentId: string, id: number): void {
    timers.clearTimeout(id);
   * Clear a specific interval
  clearInterval(componentId: string, id: number): void {
    timers.clearInterval(id);
    this.removeResource(componentId, 'interval', id);
   * Cancel a specific animation frame
  cancelAnimationFrame(componentId: string, id: TimerId): void {
    if (timers.cancelAnimationFrame) {
      timers.cancelAnimationFrame(id as number);
      // If we fell back to setTimeout, use clearTimeout
      timers.clearTimeout(id as any);
   * Register an event listener with automatic cleanup
  addEventListener(
    target: EventTarget,
    listener: EventListener,
    options?: AddEventListenerOptions
    // Only add event listeners if we have a valid EventTarget (browser environment)
    if (target && typeof target.addEventListener === 'function') {
      target.addEventListener(type, listener, options);
      const resourceId = `${type}-${Date.now()}-${Math.random()}`;
        type: 'eventListener',
        id: resourceId,
        cleanup: () => {
          if (target && typeof target.removeEventListener === 'function') {
            target.removeEventListener(type, listener, options);
        metadata: { target, type, options }
   * Register a DOM element that needs cleanup
  registerDOMElement(
    element: any, // Use 'any' to avoid HTMLElement type in Node.js
    cleanup?: () => void
    // Only register if we're in a browser environment with DOM support
    if (typeof document !== 'undefined' && element && element.parentNode) {
      const resourceId = `dom-${Date.now()}-${Math.random()}`;
        type: 'domElement',
          if (cleanup) {
          if (element && element.parentNode && typeof element.parentNode.removeChild === 'function') {
            element.parentNode.removeChild(element);
        metadata: { element }
   * Register a React root for cleanup
  registerReactRoot(
    root: any,
    unmountFn: () => void
      type: 'reactRoot',
      id: `react-root-${componentId}`,
      cleanup: unmountFn,
      metadata: { root }
   * Register a generic cleanup callback for a component
  registerCleanup(componentId: string, cleanup: () => void): void {
    if (!this.cleanupCallbacks.has(componentId)) {
      this.cleanupCallbacks.set(componentId, []);
    this.cleanupCallbacks.get(componentId)!.push(cleanup);
   * Register a global resource (not tied to a specific component)
  registerGlobalResource(resource: ManagedResource): void {
    this.globalResources.add(resource);
   * Add a resource to be managed
  private addResource(componentId: string, resource: ManagedResource): void {
    if (!this.resources.has(componentId)) {
      this.resources.set(componentId, new Set());
    this.resources.get(componentId)!.add(resource);
   * Remove a specific resource
  private removeResource(
    type: ManagedResource['type'],
    id: string | number | NodeJS.Timeout
    const componentResources = this.resources.get(componentId);
    if (componentResources) {
      const toRemove = Array.from(componentResources).find(
        r => r.type === type && r.id === id
      if (toRemove) {
        componentResources.delete(toRemove);
   * Clean up all resources for a specific component
  cleanupComponent(componentId: string): void {
    // Clean up tracked resources
      componentResources.forEach(resource => {
          resource.cleanup();
          console.error(`Error cleaning up ${resource.type} resource:`, error);
      this.resources.delete(componentId);
    // Execute cleanup callbacks
    const callbacks = this.cleanupCallbacks.get(componentId);
      callbacks.forEach(callback => {
          console.error('Error executing cleanup callback:', error);
      this.cleanupCallbacks.delete(componentId);
   * Clean up all global resources
  cleanupGlobal(): void {
    this.globalResources.forEach(resource => {
        console.error(`Error cleaning up global ${resource.type} resource:`, error);
    this.globalResources.clear();
   * Clean up all resources (components and global)
  cleanupAll(): void {
    // Clean up all component resources
    for (const componentId of this.resources.keys()) {
      this.cleanupComponent(componentId);
    // Clean up global resources
    this.cleanupGlobal();
   * Get resource statistics for debugging
    componentCount: number;
    resourceCounts: Record<string, number>;
    globalResourceCount: number;
    const resourceCounts: Record<string, number> = {};
    for (const resources of this.resources.values()) {
      resources.forEach(resource => {
        resourceCounts[resource.type] = (resourceCounts[resource.type] || 0) + 1;
      componentCount: this.resources.size,
      resourceCounts,
      globalResourceCount: this.globalResources.size
export const resourceManager = new ResourceManager();