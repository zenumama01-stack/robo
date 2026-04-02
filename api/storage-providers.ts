import { ILocalStorageProvider, LogError, LogErrorEx } from '@memberjunction/core';
import { openDB, DBSchema, IDBPDatabase } from '@tempfix/idb';
// Default category used when no category is specified
const DEFAULT_CATEGORY = 'default';
// IN-MEMORY STORAGE PROVIDER (Map of Maps)
 * In-memory storage provider using nested Map structure for category isolation.
 * Used as a fallback when browser storage is not available.
 * Storage structure: Map<category, Map<key, value>>
export class BrowserStorageProviderBase implements ILocalStorageProvider {
    private _storage: Map<string, Map<string, string>> = new Map();
     * Gets or creates a category map
    private getCategoryMap(category: string): Map<string, string> {
        const cat = category || DEFAULT_CATEGORY;
        let categoryMap = this._storage.get(cat);
        if (!categoryMap) {
            categoryMap = new Map();
            this._storage.set(cat, categoryMap);
        return categoryMap;
    public async GetItem(key: string, category?: string): Promise<string | null> {
        const categoryMap = this.getCategoryMap(category || DEFAULT_CATEGORY);
        return categoryMap.get(key) ?? null;
    public async SetItem(key: string, value: string, category?: string): Promise<void> {
        categoryMap.set(key, value);
    public async Remove(key: string, category?: string): Promise<void> {
        categoryMap.delete(key);
    public async ClearCategory(category: string): Promise<void> {
        this._storage.delete(cat);
    public async GetCategoryKeys(category: string): Promise<string[]> {
        const categoryMap = this._storage.get(category || DEFAULT_CATEGORY);
        return categoryMap ? Array.from(categoryMap.keys()) : [];
// BROWSER LOCAL STORAGE PROVIDER (Key Prefix)
 * Browser localStorage provider with category support via key prefixing.
 * Key format: [mj]:[category]:[key]
 * Example: [mj]:[RunViewCache]:[Users|Active=1|Name ASC]
 * Falls back to in-memory storage if localStorage is not available.
class BrowserLocalStorageProvider extends BrowserStorageProviderBase {
     * Builds a prefixed key for localStorage
     * Format: [mj]:[category]:[key]
    private buildKey(key: string, category?: string): string {
        return `[mj]:[${cat}]:[${key}]`;
     * Parses a prefixed key to extract category and original key
    private parseKey(prefixedKey: string): { category: string; key: string } | null {
        const match = prefixedKey.match(/^\[mj\]:\[([^\]]*)\]:\[(.+)\]$/);
            return { category: match[1], key: match[2] };
    public override async GetItem(key: string, category?: string): Promise<string | null> {
        if (typeof localStorage !== 'undefined') {
            return localStorage.getItem(this.buildKey(key, category));
        return await super.GetItem(key, category);
    public override async SetItem(key: string, value: string, category?: string): Promise<void> {
            localStorage.setItem(this.buildKey(key, category), value);
            await super.SetItem(key, value, category);
    public override async Remove(key: string, category?: string): Promise<void> {
            localStorage.removeItem(this.buildKey(key, category));
            await super.Remove(key, category);
    public override async ClearCategory(category: string): Promise<void> {
            const prefix = `[mj]:[${cat}]:`;
            for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key && key.startsWith(prefix)) {
            for (const key of keysToRemove) {
                localStorage.removeItem(key);
            await super.ClearCategory(category);
    public override async GetCategoryKeys(category: string): Promise<string[]> {
            const keys: string[] = [];
                const prefixedKey = localStorage.key(i);
                if (prefixedKey && prefixedKey.startsWith(prefix)) {
                    const parsed = this.parseKey(prefixedKey);
                        keys.push(parsed.key);
        return await super.GetCategoryKeys(category);
// INDEXED DB STORAGE PROVIDER (Object Stores per Category)
const IDB_DB_NAME = 'MJ_Metadata';
const IDB_DB_VERSION = 3; // v3: Remove legacy Metadata_KVPairs store
// Known object store names as a const tuple for type safety
const KNOWN_OBJECT_STORES = [
    'mj:default',       // Default category
    'mj:Metadata',      // Metadata cache
    'mj:RunViewCache',  // RunView results cache
    'mj:RunQueryCache', // RunQuery results cache
    'mj:DatasetCache',  // Dataset cache
// Type for known store names
type KnownStoreName = typeof KNOWN_OBJECT_STORES[number];
// Legacy store name - kept for cleanup during upgrade
const LEGACY_STORE_NAME = 'Metadata_KVPairs';
 * IndexedDB schema with dynamic object stores per category.
 * Each category gets its own object store: mj:CategoryName
export interface MJ_MetadataDB extends DBSchema {
    // Default category store
    'mj:default': {
    // Metadata store
    'mj:Metadata': {
    // RunView cache store
    'mj:RunViewCache': {
    // RunQuery cache store
    'mj:RunQueryCache': {
    // Dataset cache store
    'mj:DatasetCache': {
 * IndexedDB storage provider with category support via separate object stores.
 * Known categories (mj:Metadata, mj:RunViewCache, etc.) get dedicated object stores.
 * Unknown categories fall back to the default store with prefixed keys.
export class BrowserIndexedDBStorageProvider extends BrowserStorageProviderBase {
    private dbPromise: Promise<IDBPDatabase<MJ_MetadataDB>>;
    private _dbReady: boolean = false;
        this.dbPromise = openDB<MJ_MetadataDB>(IDB_DB_NAME, IDB_DB_VERSION, {
            upgrade(db) {
                    // Remove legacy store if it exists (cleanup from v1/v2)
                    // Cast needed because LEGACY_STORE_NAME is not in current schema (it's being removed)
                    if (db.objectStoreNames.contains(LEGACY_STORE_NAME as KnownStoreName)) {
                        db.deleteObjectStore(LEGACY_STORE_NAME as KnownStoreName);
                    // Create known category stores
                    for (const storeName of KNOWN_OBJECT_STORES) {
                        if (!db.objectStoreNames.contains(storeName)) {
                            db.createObjectStore(storeName);
                        error: e,
                        message: (e as Error)?.message
        this.dbPromise.then(() => {
            this._dbReady = true;
        }).catch(e => {
                message: 'IndexedDB initialization failed: ' + (e as Error)?.message
     * Checks if a category has a dedicated object store
    private isKnownCategory(category: string): boolean {
        const storeName = `mj:${category}`;
        return (KNOWN_OBJECT_STORES as readonly string[]).includes(storeName);
     * Gets the object store name for a category.
     * Returns the dedicated store if it exists, otherwise returns the default store.
    private getStoreName(category?: string): KnownStoreName {
        if (this.isKnownCategory(cat)) {
            return `mj:${cat}` as KnownStoreName;
        return 'mj:default';
     * Gets the key to use in the store.
     * For known stores, use the key as-is.
     * For unknown categories using the default store, prefix with category.
    private getStoreKey(key: string, category?: string): string {
        // If using default store for unknown category, prefix the key
        return `[${cat}]:${key}`;
            const db = await this.dbPromise;
            const storeName = this.getStoreName(category);
            const storeKey = this.getStoreKey(key, category);
            const tx = db.transaction(storeName, 'readwrite');
            await tx.objectStore(storeName).put(value, storeKey);
            await tx.done;
            // Fall back to in-memory
            const value = await db.transaction(storeName).objectStore(storeName).get(storeKey);
            return value ?? null;
            await tx.objectStore(storeName).delete(storeKey);
            // If it's a dedicated store, clear the entire store
                await tx.objectStore(storeName).clear();
                // For unknown categories using default store, clear only prefixed keys
                const prefix = `[${cat}]:`;
                const tx = db.transaction('mj:default', 'readwrite');
                const store = tx.objectStore('mj:default');
                const allKeys = await store.getAllKeys();
                for (const storeKey of allKeys) {
                    if (typeof storeKey === 'string' && storeKey.startsWith(prefix)) {
                        await store.delete(storeKey);
            const tx = db.transaction(storeName, 'readonly');
            const store = tx.objectStore(storeName);
            // If it's a dedicated store, return keys as-is
                return allKeys.map(k => String(k));
            // For unknown categories, filter and strip prefix
            return allKeys
                .map(k => String(k))
                .filter(k => k.startsWith(prefix))
                .map(k => k.slice(prefix.length));
