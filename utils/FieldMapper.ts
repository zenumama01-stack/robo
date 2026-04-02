 * FieldMapper is used to map fields from one name to another. This is useful when we need to map
 * fields from one system to another, or when we need to map fields from one version of a system
 * to another. Uses an internal field mapping but may be overridden or extended as needed.
export class FieldMapper {
  private _fieldMap: Record<string, string> = {
    __mj_CreatedAt: '_mj__CreatedAt',
    __mj_UpdatedAt: '_mj__UpdatedAt',
    __mj_DeletedAt: '_mj__DeletedAt',
   * Creates a new FieldMapper instance.
   * @param fieldMap An optional field map to use for mapping fields. If not provided, the default field map will be used.
   * Maps fields from one name to another mutating the object in place.
   * @param obj The object to mutate
  public MapFields(obj?: Record<string, unknown>) {
    if (obj) {
      for (const k in obj) {
        if (k in this._fieldMap) {
          obj[this._fieldMap[k]] = obj[k];
          delete obj[k];
   * Maps a field name from one name to another.
   * @param fieldName The field name to map.
   * @returns The mapped field name, or the original field name if no mapping is found.
  public MapFieldName(fieldName: string): string {
    return this._fieldMap[fieldName] ?? fieldName;
   * Maps a field name from one name to another using the reverse mapping.
  public ReverseMapFieldName(fieldName: string): string {
    return Object.entries(this._fieldMap).find(([k, v]) => v === fieldName)?.[0] ?? fieldName;
   * Maps fields from one name to another mutating the object in place using the reverse mapping.
  public ReverseMapFields(obj: Record<string, unknown>) {
    const reversed = Object.fromEntries(Object.entries(this._fieldMap).map(([k, v]) => [v, k]));
      if (k in reversed) {
        obj[reversed[k]] = obj[k];
