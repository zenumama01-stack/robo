 * Subset Of Entity Fields Validator
 * Validates that array elements are valid field names for a specified entity.
 * // ❌ BROKEN - FullName, Status, StartDate don't exist on Members
 *   entityName="Members"
 *   fields={['FullName', 'Status', 'StartDate']}
 * // ✅ FIXED - FirstName, LastName, Email exist on Members
 *   fields={['FirstName', 'LastName', 'Email']}
 *     "allowWildcard": false,
 * Validates that array elements are valid field names for an entity
 * **Validates**: Array of field name strings
 * - EntityDataGrid fields prop
 * - DataGrid columns (when using entity binding)
 * - Custom components with entity field arrays
@RegisterClass(BaseConstraintValidator, 'subset-of-entity-fields')
export class SubsetOfEntityFieldsValidator extends BaseConstraintValidator {
   * Validate that array elements are valid entity field names
      // Entity doesn't exist - not this validator's responsibility to report
      // (should be caught by valid-entity-reference validator)
    const allowWildcard = config.allowWildcard === true;
    // Validate the property value is an array
    if (!Array.isArray(context.propertyValue)) {
          'invalid-type',
            `Property '${context.propertyName}' must be an array of field names`,
            { entityName }
          `Use an array: ${context.propertyName}={['${fieldNames.slice(0, 3).join("', '")}']}`
    // Validate each element is a string or object with 'field' property
    for (let i = 0; i < context.propertyValue.length; i++) {
      const element = context.propertyValue[i];
      // Skip dynamic values (identifiers, expressions)
      if (this.isDynamicValue(element)) {
      // Extract field name from element
      let fieldName: string | null = null;
      if (typeof element === 'string') {
        // Simple string field name
        fieldName = element;
      } else if (typeof element === 'object' && element !== null) {
        // Object with 'field' property (e.g., ColumnDef, FieldDefinition)
        const fieldProp = (element as any).field;
        if (typeof fieldProp === 'string') {
          fieldName = fieldProp;
        } else if (this.isDynamicValue(fieldProp)) {
          // Dynamic field property - skip validation
          // Object without valid 'field' property
              'invalid-element-type',
                `Element at index ${i} in '${context.propertyName}' must be a string or object with 'field' property`,
                { entityName, index: i, elementType: typeof element }
              'high',
              `Use string field names or objects with 'field' property from entity '${entityName}'`
        // Invalid type (not string or object)
              `Element at index ${i} in '${context.propertyName}' must be a string or object, got ${typeof element}`,
            `Use string field names from entity '${entityName}'`
      // At this point, fieldName should be a string
        continue; // Should not reach here, but safety check
      // Check for wildcard
      if (allowWildcard && fieldName === '*') {
        continue; // Wildcard is allowed
        fieldExists = fieldNames.includes(fieldName);
        const index = fieldNamesLower.indexOf(fieldName.toLowerCase());
        const similarFields = this.findSimilar(fieldName, fieldNames, 3, 3);
            'invalid-field',
              `Field '${fieldName}' does not exist on entity '${entityName}'`,
      } else if (!caseSensitive && correctCaseName && correctCaseName !== fieldName) {
              `Field '${fieldName}' case doesn't match schema. Expected '${correctCaseName}' on entity '${entityName}'`,
            `Use '${correctCaseName}' instead of '${fieldName}'`,
    return 'Validates that array elements are valid field names for the specified entity';
