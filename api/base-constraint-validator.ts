 * Base Constraint Validator
 * Abstract base class for all constraint validators.
 * Validators implement business logic for validating prop values at lint-time.
 * - **Validators = Code**: Implemented as TypeScript classes
 * - **Constraints = Metadata**: Declared in component spec JSON
 * - **Separation**: Validator logic is separate from constraint definitions
 * // Validator implementation (code)
 * class SubsetOfEntityFieldsValidator extends BaseConstraintValidator {
 *   async validate(context, constraint) {
 *     // Implementation...
 * // Constraint definition (metadata in spec JSON)
 *   "type": "subset-of-entity-fields",
 *   "dependsOn": "entityName"
  PropertyConstraint,
  ConstraintViolation,
import { ValidationContext } from './validation-context';
import { PropValueExtractor, DynamicValue } from '../prop-value-extractor';
 * Abstract base class for constraint validators
 * All constraint validators must extend this class and implement the validate() method.
export abstract class BaseConstraintValidator {
   * Validate a prop value against a constraint
   * This is the main method that subclasses must implement.
   * It receives the validation context and constraint definition,
   * and returns an array of violations (empty if valid).
   * Note: Changed from async to sync since validators perform no actual I/O.
   * This allows seamless integration with the synchronous linter architecture.
   * @param context - Validation context with prop value, component spec, entities, etc.
   * @param constraint - The constraint definition from the component spec
   * @returns Array of violations (empty if valid)
   * class MyValidator extends BaseConstraintValidator {
   *   validate(context: ValidationContext, constraint: PropertyConstraint): ConstraintViolation[] {
   *     const violations: ConstraintViolation[] = [];
   *     // Check if value can be validated statically
   *     if (PropValueExtractor.isDynamicValue(context.propertyValue)) {
   *       return violations; // Skip - can't validate dynamic values
   *     // Perform validation logic
   *     // ...
   *     return violations;
  abstract validate(
    context: ValidationContext,
    constraint: PropertyConstraint
  ): ConstraintViolation[];
   * Get the name of this validator
   * Used for debugging and error messages.
   * Defaults to the class name, but can be overridden.
   * @returns Validator name
  getName(): string {
   * Get a description of what this validator checks
   * Used for documentation and error messages.
   * Should be a brief, user-friendly description.
   * @returns Validator description
   * getDescription() {
   *   return "Validates that array elements are valid field names for the specified entity";
  abstract getDescription(): string;
  // Protected Helper Methods
   * Check if a value is dynamic (can't be validated statically)
   * Dynamic values include identifiers (variables) and expressions (function calls, etc.)
   * that can't be resolved at lint-time.
   * @param value - The extracted value to check
   * if (this.isDynamicValue(context.propertyValue)) {
   *   return []; // Skip validation
  protected isDynamicValue(value: any): value is DynamicValue {
    return PropValueExtractor.isDynamicValue(value);
   * Create a violation with consistent formatting
   * Helper method to create ConstraintViolation objects with common fields populated.
   * @param type - Violation type (e.g., 'invalid-field', 'type-mismatch')
   * @param message - Error message
   * @param severity - Severity level (default: 'high')
   * @param suggestion - Optional fix suggestion
   * @param metadata - Optional additional metadata
   * @returns ConstraintViolation object
   * return [
   *   this.createViolation(
   *     'invalid-field',
   *     `Field '${field}' does not exist on entity '${entityName}'`,
   *     'critical',
   *     `Use one of: ${validFields.join(', ')}`,
   *     { field, entityName, validFields }
   *   )
  protected createViolation(
    severity: 'critical' | 'high' | 'medium' | 'low' = 'high',
    suggestion?: string,
  ): ConstraintViolation {
      suggestion,
   * Get the dependent prop value
   * If the constraint has a `dependsOn` field, this method retrieves
   * the value of that dependent prop from sibling props.
   * Returns null if:
   * - No dependency is specified
   * - The dependent prop is not found
   * - The dependent prop has a dynamic value
   * @param context - Validation context
   * @param constraint - Constraint definition
   * @returns The dependent prop value, or null if not available
   * // Constraint: { type: 'subset-of-entity-fields', dependsOn: 'entityName' }
   * const entityName = this.getDependentPropValue(context, constraint);
   * if (!entityName) {
   *   // Can't validate - entityName not provided or is dynamic
   *   return [];
  protected getDependentPropValue(
    if (!constraint.dependsOn) {
    const dependentValue = context.siblingProps.get(constraint.dependsOn);
    if (!dependentValue) {
    if (this.isDynamicValue(dependentValue)) {
    return dependentValue;
   * Format a list of values for error messages
   * Truncates long lists with "..." and formats values nicely.
   * @param values - Array of values
   * @param maxItems - Maximum items to show (default: 10)
   * @returns Formatted string
   * const fields = ['FirstName', 'LastName', 'Email', ...]; // 50 fields
   * const formatted = this.formatValueList(fields, 5);
   * // => "FirstName, LastName, Email, Status, JoinDate... (45 more)"
  protected formatValueList(values: any[], maxItems: number = 10): string {
    if (values.length === 0) {
      return '(none)';
    if (values.length <= maxItems) {
      return values.map((v) => `'${v}'`).join(', ');
    const shown = values.slice(0, maxItems).map((v) => `'${v}'`);
    const remaining = values.length - maxItems;
    return `${shown.join(', ')}... (${remaining} more)`;
   * Apply custom error template
   * If the constraint has an `errorTemplate`, applies it by replacing
   * template variables with actual values.
   * Template variables:
   * - {property} - Property name from context
   * - {value} - Property value from context
   * - {constraint} - Constraint type
   * - Custom variables from templateVars parameter
   * @param defaultMessage - Default message to use if no template
   * @param templateVars - Additional template variables
   * @returns Formatted message
   * // Constraint: { errorTemplate: "Field '{field}' not found on {entityName}" }
   * const message = this.applyErrorTemplate(
   *   constraint,
   *   context,
   *   `Field '${field}' does not exist`,
   *   { field: 'FullName', entityName: 'Members' }
   * // => "Field 'FullName' not found on Members"
  protected applyErrorTemplate(
    constraint: PropertyConstraint,
    defaultMessage: string,
    templateVars: Record<string, any> = {}
    if (!constraint.errorTemplate) {
      return defaultMessage;
    let message = constraint.errorTemplate;
    // Standard variables
    const vars: Record<string, any> = {
      property: context.propertyName,
      value: PropValueExtractor.describe(context.propertyValue),
      constraint: constraint.type,
      ...templateVars,
    for (const [key, value] of Object.entries(vars)) {
      const regex = new RegExp(`\\{${key}\\}`, 'g');
      message = message.replace(regex, String(value));
   * Used for finding similar field names for "Did you mean?" suggestions.
   * @param a - First string
   * @param b - Second string
   * @returns Edit distance
  protected levenshteinDistance(a: string, b: string): number {
    // Initialize first column
    // Initialize first row
    // Fill matrix
            matrix[i - 1][j - 1] + 1, // substitution
            matrix[i][j - 1] + 1, // insertion
            matrix[i - 1][j] + 1 // deletion
   * Find similar strings using Levenshtein distance
   * Returns strings sorted by similarity (most similar first).
   * @param target - The target string to find matches for
   * @param candidates - Array of candidate strings
   * @param maxResults - Maximum number of results (default: 3)
   * @param maxDistance - Maximum edit distance to consider (default: 3)
   * @returns Array of similar strings, sorted by distance
   * const fields = ['FirstName', 'LastName', 'Email', 'Status'];
   * const similar = this.findSimilar('FristName', fields, 2);
   * // => ['FirstName'] (edit distance 1)
  protected findSimilar(
    target: string,
    candidates: string[],
    maxResults: number = 3,
    maxDistance: number = 3
    const withDistances = candidates
      .map((candidate) => ({
        value: candidate,
        distance: this.levenshteinDistance(target.toLowerCase(), candidate.toLowerCase()),
      .filter((item) => item.distance <= maxDistance)
      .sort((a, b) => a.distance - b.distance);
    return withDistances.slice(0, maxResults).map((item) => item.value);
   * Case-insensitive string comparison
   * @returns True if strings are equal (case-insensitive)
  protected equalsCaseInsensitive(a: string, b: string): boolean {
    return a.toLowerCase() === b.toLowerCase();
   * Check if array contains value (case-insensitive for strings)
   * @param arr - Array to search
   * @param value - Value to find
   * @returns True if array contains the value
  protected includesCaseInsensitive(arr: string[], value: string): boolean {
    return arr.some((item) => this.equalsCaseInsensitive(item, value));
