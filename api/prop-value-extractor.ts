 * Prop Value Extractor Utility
 * Extracts static values from JSX attribute AST nodes for validation purposes.
 * This utility is foundational for constraint validation - it converts AST nodes
 * into analyzable values that constraint validators can work with.
 * Supported Extractions:
 * - Literal values: strings, numbers, booleans, null
 * - Arrays of literals
 * - Object literals (nested)
 * - Identifies dynamic values (identifiers/expressions) that can't be extracted
 * const value = PropValueExtractor.extract(jsxAttribute);
 * if (value._type === 'identifier') {
 *   // Can't validate statically - skip with warning
 *   // Can validate - proceed with constraint checking
 * Represents a value that couldn't be extracted statically
export interface DynamicValue {
  _type: 'identifier' | 'expression';
 * Union type for extracted values
export type ExtractedValue =
  | string
  | number
  | boolean
  | null
  | undefined
  | ExtractedValue[]
  | { [key: string]: ExtractedValue }
  | DynamicValue;
 * Utility class for extracting static values from JSX attribute AST nodes
export class PropValueExtractor {
   * Extract a value from a JSX attribute
   * @param attr - The JSX attribute node
   * @returns The extracted value, or a DynamicValue object if it can't be extracted statically
   * // Boolean shorthand: <Component show />
   * PropValueExtractor.extract(attr) // => true
   * // String literal: <Component name="test" />
   * PropValueExtractor.extract(attr) // => "test"
   * // Expression: <Component count={42} />
   * PropValueExtractor.extract(attr) // => 42
   * // Array: <Component items={['a', 'b']} />
   * PropValueExtractor.extract(attr) // => ['a', 'b']
   * // Dynamic: <Component name={userName} />
   * PropValueExtractor.extract(attr) // => { _type: 'identifier', name: 'userName' }
  static extract(attr: t.JSXAttribute): ExtractedValue {
    // Boolean shorthand: <Component show />
    if (!attr.value) {
    // String literal: <Component name="test" />
    if (t.isStringLiteral(attr.value)) {
      return attr.value.value;
    // Expression container: <Component prop={...} />
    if (t.isJSXExpressionContainer(attr.value)) {
      const expr = attr.value.expression;
      // Handle JSXEmptyExpression (e.g., {/* comment */})
      if (t.isJSXEmptyExpression(expr)) {
      return this.extractExpression(expr);
    // JSX element or fragment (rare but possible)
    if (t.isJSXElement(attr.value) || t.isJSXFragment(attr.value)) {
        _type: 'expression',
        description: 'JSX element as prop value',
   * Extract a value from an expression node
   * @param expr - The expression node
  private static extractExpression(expr: t.Expression | t.JSXEmptyExpression): ExtractedValue {
    // Simple literals
    if (t.isStringLiteral(expr)) return expr.value;
    if (t.isNumericLiteral(expr)) return expr.value;
    if (t.isBooleanLiteral(expr)) return expr.value;
    if (t.isNullLiteral(expr)) return null;
    // Template literals (can be static)
    if (t.isTemplateLiteral(expr)) {
      return this.extractTemplateLiteral(expr);
    if (t.isArrayExpression(expr)) {
      return this.extractArray(expr);
    // Objects
      return this.extractObjectLiteral(expr);
    // Unary expressions (e.g., -5, !true)
    if (t.isUnaryExpression(expr)) {
      return this.extractUnaryExpression(expr);
    // Binary expressions (e.g., 1 + 2, 'hello' + 'world')
    if (t.isBinaryExpression(expr)) {
      return this.extractBinaryExpression(expr);
    // Identifiers - can't extract statically
        _type: 'identifier',
        name: expr.name,
    // Member expressions (e.g., obj.prop)
    if (t.isMemberExpression(expr)) {
        description: 'member expression',
    // Call expressions (e.g., func())
        description: 'function call',
    // Arrow functions, function expressions
    if (t.isArrowFunctionExpression(expr) || t.isFunctionExpression(expr)) {
        description: 'function',
    // Conditional expressions (ternary)
    if (t.isConditionalExpression(expr)) {
      return this.extractConditionalExpression(expr);
    // All other expression types
      description: expr.type,
   * Extract template literal if it's static
   * @param node - Template literal node
   * @returns Concatenated string if all expressions are literals, otherwise DynamicValue
  private static extractTemplateLiteral(node: t.TemplateLiteral): ExtractedValue {
    // Check if all expressions are static literals
    for (let i = 0; i < node.quasis.length; i++) {
      parts.push(node.quasis[i].value.cooked || node.quasis[i].value.raw);
      if (i < node.expressions.length) {
        const expr = node.expressions[i];
        // Handle TSType nodes (TypeScript types in template literals)
        if (!t.isExpression(expr) && !t.isJSXEmptyExpression(expr)) {
            description: 'template literal with type annotation',
        const value = this.extractExpression(expr);
        // If any expression is dynamic, the whole template is dynamic
        if (this.isDynamicValue(value)) {
            description: 'template literal with dynamic expressions',
        parts.push(String(value));
    return parts.join('');
   * Extract array expression
   * @param node - Array expression node
   * @returns Array of extracted values
  private static extractArray(node: t.ArrayExpression): ExtractedValue {
    const result: ExtractedValue[] = [];
    for (const element of node.elements) {
      if (!element) {
        // Sparse array: [1, , 3]
        result.push(undefined);
      if (t.isSpreadElement(element)) {
        // Spread element: [...items] - can't extract statically
          description: 'spread element',
      result.push(this.extractExpression(element));
   * Extract object literal
   * @param node - Object expression node
   * @returns Object with extracted property values
  private static extractObjectLiteral(node: t.ObjectExpression): ExtractedValue {
    const result: { [key: string]: ExtractedValue } = {};
    for (const prop of node.properties) {
      // Spread properties: { ...obj }
        result['...'] = {
          description: 'spread property',
      // Object method: { foo() {} }
      if (t.isObjectMethod(prop)) {
        const key = this.getPropertyKey(prop.key, prop.computed);
            description: 'object method',
      // Regular property: { foo: 'bar' }
          result[key] = this.extractExpression(prop.value as t.Expression);
   * Get property key as string
   * @param key - Property key node
   * @param computed - Whether the property is computed
   * @returns String key or null if can't be extracted
  private static getPropertyKey(
    key: t.Expression | t.Identifier | t.PrivateName | t.StringLiteral | t.NumericLiteral | t.BigIntLiteral,
    computed: boolean
    if (t.isIdentifier(key)) {
      return key.name;
    if (t.isStringLiteral(key)) {
      return key.value;
    if (t.isNumericLiteral(key)) {
      return String(key.value);
    if (computed && t.isExpression(key)) {
      // Computed property: { [expr]: value }
      const value = this.extractExpression(key);
      if (!this.isDynamicValue(value)) {
   * Extract unary expression if operand is static
   * @param node - Unary expression node
   * @returns Computed value or DynamicValue
  private static extractUnaryExpression(node: t.UnaryExpression): ExtractedValue {
    const operand = this.extractExpression(node.argument);
    if (this.isDynamicValue(operand)) {
    // Apply unary operator
    switch (node.operator) {
        return typeof operand === 'number' ? -operand : operand;
        return typeof operand === 'number' ? +operand : operand;
        return !operand;
        return typeof operand === 'number' ? ~operand : operand;
      case 'typeof':
        return typeof operand;
      case 'void':
          description: `unary ${node.operator}`,
   * Extract binary expression if both operands are static
   * @param node - Binary expression node
  private static extractBinaryExpression(node: t.BinaryExpression): ExtractedValue {
    // Handle PrivateName in left operand (shouldn't happen in normal code but TypeScript allows it)
    if (t.isPrivateName(node.left)) {
        description: 'binary expression with private name',
    const left = this.extractExpression(node.left);
    const right = this.extractExpression(node.right);
    // If either side is dynamic, can't compute
    if (this.isDynamicValue(left) || this.isDynamicValue(right)) {
        description: `binary ${node.operator}`,
    // Try to compute the result
          return (left as any) + (right as any);
          return (left as any) - (right as any);
          return (left as any) * (right as any);
          return (left as any) / (right as any);
          return (left as any) % (right as any);
        case '**':
          return (left as any) ** (right as any);
        case '==':
          return left == right;
        case '!=':
          return left != right;
        case '===':
          return left === right;
        case '!==':
          return left !== right;
          return (left as any) < (right as any);
        case '<=':
          return (left as any) <= (right as any);
          return (left as any) > (right as any);
        case '>=':
          return (left as any) >= (right as any);
          return (left as any) & (right as any);
          return (left as any) | (right as any);
          return (left as any) ^ (right as any);
        case '<<':
          return (left as any) << (right as any);
        case '>>':
          return (left as any) >> (right as any);
        case '>>>':
          return (left as any) >>> (right as any);
      // Computation failed (e.g., type error)
   * Extract conditional expression (ternary) if all parts are static
   * @param node - Conditional expression node
   * @returns Value of the taken branch or DynamicValue
  private static extractConditionalExpression(node: t.ConditionalExpression): ExtractedValue {
    const test = this.extractExpression(node.test);
    // If test is dynamic, we can't determine which branch is taken
    if (this.isDynamicValue(test)) {
        description: 'conditional expression',
    // Evaluate which branch is taken
    const consequent = this.extractExpression(node.consequent);
    const alternate = this.extractExpression(node.alternate);
    return test ? consequent : alternate;
   * Check if a value is a DynamicValue (can't be validated statically)
   * @param value - The extracted value
   * @returns True if the value is dynamic
  static isDynamicValue(value: ExtractedValue): value is DynamicValue {
      '_type' in value &&
      (value._type === 'identifier' || value._type === 'expression')
   * Check if an array contains any dynamic values
   * @param arr - The array to check
   * @returns True if any element is dynamic
  static hasAnyDynamicValue(arr: ExtractedValue[]): boolean {
    return arr.some((val) => {
      if (this.isDynamicValue(val)) return true;
      if (Array.isArray(val)) return this.hasAnyDynamicValue(val);
      if (typeof val === 'object' && val !== null && !('_type' in val)) {
        return Object.values(val).some((v) => this.isDynamicValue(v));
   * Get a human-readable description of the value
   * @returns Description string
  static describe(value: ExtractedValue): string {
      if (value._type === 'identifier' && value.name) {
        return `identifier '${value.name}'`;
      return value.description || 'dynamic expression';
      return `array with ${value.length} element${value.length !== 1 ? 's' : ''}`;
      return `object with ${keys.length} propert${keys.length !== 1 ? 'ies' : 'y'}: {${keys.slice(0, 3).join(', ')}${keys.length > 3 ? '...' : ''}}`;
      return `"${value.length > 50 ? value.substring(0, 50) + '...' : value}"`;
