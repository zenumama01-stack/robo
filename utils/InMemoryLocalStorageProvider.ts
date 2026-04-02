import { ILocalStorageProvider } from './interfaces';
 * In-memory implementation of ILocalStorageProvider.
 * Useful for server-side environments (Node.js) where browser storage APIs like
 * localStorage or indexedDB are not available.
 * Note: Data is not persisted across process restarts. This is suitable for
 * caching scenarios where persistence is not required.
export class InMemoryLocalStorageProvider implements ILocalStorageProvider {
    private static readonly DEFAULT_CATEGORY = 'default';
        const cat = category || InMemoryLocalStorageProvider.DEFAULT_CATEGORY;
        const categoryMap = this.getCategoryMap(category || InMemoryLocalStorageProvider.DEFAULT_CATEGORY);
        const categoryMap = this._storage.get(category || InMemoryLocalStorageProvider.DEFAULT_CATEGORY);
