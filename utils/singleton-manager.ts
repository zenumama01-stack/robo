 * @fileoverview Singleton manager for shared instances across MetadataSync
 * @module singleton-manager
 * This module ensures that expensive resources like SyncEngine are only
 * initialized once per process, preventing duplicate metadata refreshes
 * and improving performance.
/** Global SyncEngine instance */
let globalSyncEngine: SyncEngine | null = null;
/** Promise to track ongoing SyncEngine initialization */
let syncEngineInitPromise: Promise<SyncEngine> | null = null;
 * Get or create a singleton SyncEngine instance
 * Ensures that only one SyncEngine is created and initialized per process,
 * preventing duplicate metadata refreshes that cause the double GetAllMetadata()
 * console output.
 * @returns Promise resolving to initialized SyncEngine instance
 * const syncEngine = await getSyncEngine(getSystemUser());
 * // Use syncEngine for operations
export async function getSyncEngine(contextUser: UserInfo): Promise<SyncEngine> {
  // Return existing engine if already initialized
  if (globalSyncEngine) {
    return globalSyncEngine;
  if (syncEngineInitPromise) {
    return syncEngineInitPromise;
  syncEngineInitPromise = (async () => {
    globalSyncEngine = new SyncEngine(contextUser);
    await globalSyncEngine.initialize();
 * Reset the singleton SyncEngine instance
 * Should be called when cleaning up resources to ensure a fresh
 * instance is created on the next request.
export function resetSyncEngine(): void {
  globalSyncEngine = null;
  syncEngineInitPromise = null;
