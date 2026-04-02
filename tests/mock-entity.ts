 * Options for creating mock entity objects
export interface MockEntityOptions {
  /** Whether the entity should report as saved (existing in DB) */
  isSaved?: boolean;
  /** Whether the entity should report as dirty */
 * Creates a mock object that behaves like a BaseEntity with getter/setter properties.
 * Since BaseEntity uses getter/setters, the spread operator doesn't work on real entities.
 * This creates a Proxy-based mock that supports Get(), Set(), GetAll(), and property access.
 * @param data Initial field values for the mock entity
 * @param options Configuration options for the mock's state
 * @returns A proxy object that behaves like a BaseEntity
export function createMockEntity<T extends Record<string, unknown>>(
  data: T,
  options: MockEntityOptions = {}
): T & MockEntityMethods {
  const { isSaved = true, isDirty = false } = options;
  const fields = new Map<string, unknown>(Object.entries(data));
  const oldValues = new Map<string, unknown>(Object.entries(data));
  let dirty = isDirty;
  const methods: MockEntityMethods = {
    Get(fieldName: string): unknown {
      const lowerKey = [...fields.keys()].find(k => k.toLowerCase() === fieldName.toLowerCase());
      return lowerKey ? fields.get(lowerKey) : undefined;
    Set(fieldName: string, value: unknown): void {
      fields.set(fieldName, value);
      for (const [key, value] of fields) {
      return dirty;
      return isSaved;
    get PrimaryKey() {
        KeyValuePairs: [{ FieldName: 'ID', Value: fields.get('ID') }],
        Values: () => String(fields.get('ID') ?? ''),
      dirty = false;
    async Delete(): Promise<boolean> {
  return new Proxy(methods, {
    get(target, prop: string) {
      // Check methods first
      if (prop in target) {
        const val = target[prop as keyof MockEntityMethods];
      // Then check data fields
      const lowerProp = prop.toLowerCase();
        if (key.toLowerCase() === lowerProp) {
    set(_target, prop: string, value) {
      fields.set(prop, value);
  }) as T & MockEntityMethods;
export interface MockEntityMethods {
  Get(fieldName: string): unknown;
  Set(fieldName: string, value: unknown): void;
  GetAll(): Record<string, unknown>;
  readonly Dirty: boolean;
  readonly IsSaved: boolean;
  readonly PrimaryKey: { KeyValuePairs: Array<{ FieldName: string; Value: unknown }>; Values: () => string };
  Save(): Promise<boolean>;
  Delete(): Promise<boolean>;
