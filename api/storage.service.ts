 * Mock ORM layer using localStorage for prototype
 * Will be replaced with real MJ entity system later
export class StorageService {
  private readonly PREFIX = 'mj_prototype_';
  // Generic CRUD operations
  Save<T>(key: string, data: T): void {
      localStorage.setItem(this.PREFIX + key, JSON.stringify(data));
      console.error('Failed to save to localStorage:', error);
  Load<T>(key: string): T | null {
      const item = localStorage.getItem(this.PREFIX + key);
      return item ? JSON.parse(item) : null;
      console.error('Failed to load from localStorage:', error);
  Delete(key: string): void {
    localStorage.removeItem(this.PREFIX + key);
    Object.keys(localStorage)
      .filter(key => key.startsWith(this.PREFIX))
      .forEach(key => localStorage.removeItem(key));
  // Collection operations
  SaveCollection<T>(collectionName: string, items: T[]): void {
    this.Save(collectionName, items);
  LoadCollection<T>(collectionName: string): T[] {
    return this.Load<T[]>(collectionName) || [];
  AddToCollection<T extends { Id: string }>(collectionName: string, item: T): void {
    const items = this.LoadCollection<T>(collectionName);
    const index = items.findIndex(i => i.Id === item.Id);
      items[index] = item; // Update
      items.push(item); // Insert
    this.SaveCollection(collectionName, items);
  RemoveFromCollection<T extends { Id: string }>(collectionName: string, id: string): void {
    const filtered = items.filter(i => i.Id !== id);
    this.SaveCollection(collectionName, filtered);
  FindInCollection<T extends { Id: string }>(collectionName: string, id: string): T | null {
    return items.find(i => i.Id === id) || null;
