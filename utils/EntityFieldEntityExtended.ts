import { BaseEntity, ValidationResult, ValidationErrorInfo, ValidationErrorType } from '@memberjunction/core';
import { MJEntityFieldEntity } from '../generated/entity_subclasses';
 * Extended MJEntityFieldEntity class that provides safeguards against modifying database-reflected properties.
 * These properties should only be updated by the CodeGen system when reflecting changes from the database schema.
@RegisterClass(BaseEntity, 'MJ: Entity Fields')
export class EntityFieldEntityExtended extends MJEntityFieldEntity {
     * Properties that are reflected from the database schema and should not be modified directly
    private static readonly DATABASE_REFLECTED_PROPERTIES = [
        'Name',              // Column name in database
        'Type',              // SQL data type
        'Length',            // Column length
        'Precision',         // Numeric precision
        'Scale',             // Numeric scale
        'IsPrimaryKey',      // Primary key status
        'IsUnique',          // Unique constraint status
        'AllowsNull',        // Nullable status
        'IsVirtual',         // Whether field exists in database
        'DefaultValue',      // Default value from database
        'AutoIncrement',     // Auto-increment status
        'RelatedEntityID',   // Foreign key relationships
        'RelatedEntityFieldName' // Foreign key field reference
                console.warn(`Setting AutoUpdateDescription to true for Entity Field "${this.Name}" because Description is being manually updated. This will prevent CodeGen from overwriting this description.`);
     * Validates the entity field before saving. Extends the base validation to prevent modifications
     * to database-reflected properties.
        // First run the base validation
        // If we're updating an existing record, check for restricted field modifications
        if (!this.NewRecord) {
            // Check each database-reflected property
            for (const prop of EntityFieldEntityExtended.DATABASE_REFLECTED_PROPERTIES) {
                const field = this.GetFieldByName(prop);
                if (field && field.Dirty) {
                        prop,
                        `Cannot modify ${prop} - this property is reflected from the database schema and can only be updated by CodeGen`,
                        field.Value,
