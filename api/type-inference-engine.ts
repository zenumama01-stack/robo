 * Type Inference Engine - AST-based type inference for component linting
 * This module analyzes JavaScript AST to infer and track types throughout
 * component code. It integrates with TypeContext to provide comprehensive
 * type information for validation rules.
  TypeContext,
  TypeInfo,
  FieldTypeInfo,
  StandardTypes,
  mapSQLTypeToJSType,
  areTypesCompatible,
  describeType
} from './type-context';
 * Result of type inference analysis
export interface TypeInferenceResult {
  /** The type context with all inferred variable types */
  typeContext: TypeContext;
  /** Any type errors or warnings found during inference */
  errors: TypeInferenceError[];
 * A type error or warning found during inference
export interface TypeInferenceError {
  type: 'error' | 'warning';
 * TypeInferenceEngine - Analyzes AST to infer and track types
export class TypeInferenceEngine {
  private typeContext: TypeContext;
  private errors: TypeInferenceError[] = [];
  private functionReturnTypes: Map<string, TypeInfo> = new Map();
  constructor(componentSpec?: ComponentSpec, contextUser?: UserInfo) {
    this.typeContext = new TypeContext(componentSpec);
   * Analyze an AST and build type context
  async analyze(ast: t.File): Promise<TypeInferenceResult> {
    // First pass: collect all variable declarations and their types
    await this.collectVariableTypes(ast);
      typeContext: this.typeContext,
      errors: this.errors
   * Get the type context after analysis
  getTypeContext(): TypeContext {
    return this.typeContext;
   * Get type inference errors found during analysis
  getErrors(): TypeInferenceError[] {
    return this.errors;
   * First pass: collect variable types from declarations and assignments
  private async collectVariableTypes(ast: t.File): Promise<void> {
    const self = this;
    let functionCounter = 0; // Counter for anonymous functions
        self.inferDeclaratorType(path);
        // Also check if it's a function expression assignment
        // const calculateMetrics = () => { return {...} }
        if (t.isIdentifier(node.id) &&
            (t.isArrowFunctionExpression(node.init) || t.isFunctionExpression(node.init))) {
          self.trackFunctionReturnType(node.id.name, node.init);
      // Track assignments to existing variables
      AssignmentExpression(path: NodePath<t.AssignmentExpression>) {
        self.inferAssignmentType(path);
      // Track function parameters and return types with scoping
      FunctionDeclaration: {
        enter(path: NodePath<t.FunctionDeclaration>) {
          const functionName = path.node.id?.name || `anon_func_${functionCounter++}`;
          self.typeContext.enterScope(functionName);
          self.inferFunctionParameterTypes(path);
          // Track return type of named functions
            self.trackFunctionReturnType(path.node.id.name, path.node);
        exit(path: NodePath<t.FunctionDeclaration>) {
          self.typeContext.exitScope();
      // Track arrow function scoping
      ArrowFunctionExpression: {
        enter(path: NodePath<t.ArrowFunctionExpression>) {
          // Find the function name from parent if it's a variable declarator
          let functionName = `anon_arrow_${functionCounter++}`;
            functionName = parent.id.name;
          self.inferArrowFunctionParameterTypes(path);
        exit(path: NodePath<t.ArrowFunctionExpression>) {
      // Track function expression scoping
      FunctionExpression: {
        enter(path: NodePath<t.FunctionExpression>) {
          const functionName = path.node.id?.name || `anon_func_expr_${functionCounter++}`;
        exit(path: NodePath<t.FunctionExpression>) {
   * Infer type from a variable declarator
  private inferDeclaratorType(path: NodePath<t.VariableDeclarator>): void {
    // Get variable name(s)
    if (t.isIdentifier(node.id)) {
      const varName = node.id.name;
      if (node.init) {
        const type = this.inferExpressionType(node.init, path);
        this.typeContext.setVariableType(varName, type);
        // Declared but not initialized
        this.typeContext.setVariableType(varName, { type: 'undefined', nullable: true });
    } else if (t.isObjectPattern(node.id) && node.init) {
      // Destructuring: const { a, b } = obj
      this.inferDestructuringTypes(node.id, node.init, path);
    } else if (t.isArrayPattern(node.id) && node.init) {
      // Array destructuring: const [a, b] = arr
      this.inferArrayDestructuringTypes(node.id, node.init, path);
   * Infer type from an assignment expression
  private inferAssignmentType(path: NodePath<t.AssignmentExpression>): void {
    if (t.isIdentifier(node.left)) {
      const varName = node.left.name;
      const type = this.inferExpressionType(node.right, path);
   * Infer types for function parameters (component props)
  private inferFunctionParameterTypes(path: NodePath<t.FunctionDeclaration>): void {
    const params = path.node.params;
        // Simple parameter - unknown type
        this.typeContext.setVariableType(param.name, StandardTypes.unknown);
        // Destructured props - this is the common component pattern
        this.inferComponentPropsTypes(param);
   * Infer types for arrow function parameters
  private inferArrowFunctionParameterTypes(path: NodePath<t.ArrowFunctionExpression>): void {
   * Infer types for component props from destructuring pattern
  private inferComponentPropsTypes(pattern: t.ObjectPattern): void {
        // Known standard props
        switch (propName) {
          case 'utilities':
            this.typeContext.setVariableType(propName, {
              fields: new Map([
                ['rv', { type: 'object', fromMetadata: true }], // RunView service
                ['rq', { type: 'object', fromMetadata: true }], // RunQuery service
                ['md', { type: 'object', fromMetadata: true }]  // Metadata
          case 'styles':
            this.typeContext.setVariableType(propName, { type: 'object' });
          case 'components':
          case 'callbacks':
          case 'savedUserSettings':
            this.typeContext.setVariableType(propName, { type: 'object', nullable: true });
          case 'onSavedUserSettingsChange':
          case 'onSaveUserSettings':
            this.typeContext.setVariableType(propName, { type: 'function' });
            // Check if it's a prop defined in the component spec
            if (this.componentSpec?.properties) {
              const specProp = this.componentSpec.properties.find(p => p.name === propName);
              if (specProp) {
                  type: this.mapSpecTypeToJSType(specProp.type),
                  nullable: !specProp.required
            // Check if it's an event (on* prefix)
            if (propName.startsWith('on')) {
              this.typeContext.setVariableType(propName, { type: 'function', nullable: true });
              this.typeContext.setVariableType(propName, StandardTypes.unknown);
   * Track function return type by analyzing its body
  private trackFunctionReturnType(functionName: string, func: t.FunctionDeclaration | t.FunctionExpression | t.ArrowFunctionExpression): void {
    const body = func.body;
    // Arrow function with expression body: () => ({...})
    if (t.isExpression(body)) {
      const returnType = this.inferExpressionType(body);
      this.functionReturnTypes.set(functionName, returnType);
    // Function with block body - find return statements
      // Try to analyze the function body for object building patterns
      const returnType = this.analyzeObjectBuildingPattern(body);
      if (returnType) {
      // Fallback: Find the first return statement
          const inferredType = this.inferExpressionType(stmt.argument);
          this.functionReturnTypes.set(functionName, inferredType);
    // No return statement found or void function
    this.functionReturnTypes.set(functionName, StandardTypes.unknown);
   * Analyze common object-building patterns in function bodies
   * Pattern: const obj = {}; forEach(...) { obj[key] = { prop: value } }; return obj;
  private analyzeObjectBuildingPattern(body: t.BlockStatement): TypeInfo | null {
    let objectVarName: string | null = null;
    let objectElementType: TypeInfo | null = null;
    const debugEnabled = false;
    // Look for: const obj = {}; ... obj[key] = { ... }; ... return obj;
      // Find variable declaration: const obj = {};
        for (const decl of stmt.declarations) {
          if (t.isIdentifier(decl.id) && t.isObjectExpression(decl.init) && decl.init.properties.length === 0) {
            objectVarName = decl.id.name;
            if (debugEnabled) console.log('[TypeInference] Found empty object declaration:', objectVarName);
      // Look for forEach or for loops that populate the object
      if (objectVarName && t.isExpressionStatement(stmt) && t.isCallExpression(stmt.expression)) {
        const call = stmt.expression;
            call.callee.property.name === 'forEach' &&
            call.arguments.length > 0) {
          const callback = call.arguments[0];
              t.isBlockStatement(callback.body)) {
            // Look for obj[something] = { ... } pattern inside forEach
            for (const innerStmt of callback.body.body) {
              if (t.isExpressionStatement(innerStmt) &&
                  t.isAssignmentExpression(innerStmt.expression) &&
                  innerStmt.expression.operator === '=') {
                const left = innerStmt.expression.left;
                const right = innerStmt.expression.right;
                // Check if left is obj[...] and right is object literal
                if (t.isMemberExpression(left) &&
                    left.computed &&
                    t.isIdentifier(left.object) &&
                    left.object.name === objectVarName &&
                    t.isObjectExpression(right)) {
                  // Infer the type of the object literal
                  objectElementType = this.inferObjectType(right);
                  if (debugEnabled) console.log('[TypeInference] Found object literal assignment, inferred type:', objectElementType);
      // Check if this function returns the object
      if (objectVarName && objectElementType && t.isReturnStatement(stmt)) {
        if (t.isIdentifier(stmt.argument) && stmt.argument.name === objectVarName) {
          // Function returns an object whose values are of the element type
            fields: new Map(), // Dictionary, not structured object
            objectValueType: objectElementType
          if (debugEnabled) console.log('[TypeInference] Returning object building pattern type:', result);
    if (debugEnabled && objectVarName && objectElementType) {
      console.log('[TypeInference] Had object and element type but no return statement matched');
   * Map component spec type to JavaScript type
  private mapSpecTypeToJSType(specType?: string): string {
    if (!specType) return 'unknown';
    const type = specType.toLowerCase();
    if (type === 'string') return 'string';
    if (type === 'number' || type === 'int' || type === 'integer' || type === 'float' || type === 'decimal') return 'number';
    if (type === 'boolean' || type === 'bool') return 'boolean';
    if (type.startsWith('array') || type.endsWith('[]')) return 'array';
    if (type === 'object') return 'object';
    if (type === 'function') return 'function';
   * Infer types from object destructuring
  private inferDestructuringTypes(pattern: t.ObjectPattern, init: t.Expression, path: NodePath): void {
    const sourceType = this.inferExpressionType(init, path);
        const varName = t.isIdentifier(prop.value) ? prop.value.name : propName;
        // Try to get the property type from the source
        if (sourceType.fields?.has(propName)) {
          const fieldType = sourceType.fields.get(propName)!;
          this.typeContext.setVariableType(varName, { type: fieldType.type, nullable: fieldType.nullable });
          this.typeContext.setVariableType(varName, StandardTypes.unknown);
        // Rest element: const { a, ...rest } = obj
        this.typeContext.setVariableType(prop.argument.name, { type: 'object' });
   * Infer types from array destructuring
  private inferArrayDestructuringTypes(pattern: t.ArrayPattern, init: t.Expression, path: NodePath): void {
    for (let i = 0; i < pattern.elements.length; i++) {
      const element = pattern.elements[i];
      if (t.isIdentifier(element)) {
        // Get element type from array
        if (sourceType.arrayElementType) {
          this.typeContext.setVariableType(element.name, sourceType.arrayElementType);
          this.typeContext.setVariableType(element.name, StandardTypes.unknown);
      } else if (t.isRestElement(element) && t.isIdentifier(element.argument)) {
        // Rest element: const [a, ...rest] = arr
        this.typeContext.setVariableType(element.argument.name, {
          arrayElementType: sourceType.arrayElementType
   * Infer the type of an expression
  inferExpressionType(node: t.Expression | t.SpreadElement | t.JSXNamespacedName | t.ArgumentPlaceholder, path?: NodePath): TypeInfo {
    // Literals - now track actual values for constant analysis
    if (t.isStringLiteral(node)) {
      return { ...StandardTypes.string, literalValue: node.value };
    if (t.isTemplateLiteral(node)) {
      // For template literals, we can't always know the final value
      return StandardTypes.string;
    if (t.isNumericLiteral(node)) {
      return { ...StandardTypes.number, literalValue: node.value };
    if (t.isBooleanLiteral(node)) {
      return { ...StandardTypes.boolean, literalValue: node.value };
    if (t.isNullLiteral(node)) {
      return { ...StandardTypes.null, literalValue: null };
    if (t.isArrayExpression(node)) {
      return this.inferArrayType(node, path);
    if (t.isObjectExpression(node)) {
      return this.inferObjectType(node, path);
    // Identifiers (variable references)
      return this.typeContext.getVariableType(node.name) || StandardTypes.unknown;
    // Function expressions
    if (t.isFunctionExpression(node) || t.isArrowFunctionExpression(node)) {
      return StandardTypes.function;
    // Call expressions
    if (t.isCallExpression(node)) {
      return this.inferCallExpressionType(node, path);
    // New expressions (constructor calls like new Date())
    if (t.isNewExpression(node)) {
      return this.inferNewExpressionType(node, path);
    // Member expressions
      return this.inferMemberExpressionType(node, path);
    // Binary expressions
    if (t.isBinaryExpression(node)) {
      return this.inferBinaryExpressionType(node, path);
    // Unary expressions
    if (t.isUnaryExpression(node)) {
      return this.inferUnaryExpressionType(node);
    // Conditional (ternary) expressions
      // Return the type of the consequent (or alternate if different, return unknown)
      const consequentType = this.inferExpressionType(node.consequent, path);
      const alternateType = this.inferExpressionType(node.alternate, path);
      if (areTypesCompatible(consequentType, alternateType)) {
        return consequentType;
      return StandardTypes.unknown;
    // Logical expressions (&&, ||, ??)
    if (t.isLogicalExpression(node)) {
      // For ||, the result is the first truthy value
      // For &&, the result is the first falsy or last value
      // For ??, the result is the first non-nullish value
      return this.inferExpressionType(node.right, path);
    // Await expressions
    if (t.isAwaitExpression(node)) {
      return this.inferExpressionType(node.argument, path);
   * Infer type for array expressions
  private inferArrayType(node: t.ArrayExpression, path?: NodePath): TypeInfo {
    if (node.elements.length === 0) {
      return { type: 'array', arrayElementType: StandardTypes.unknown };
    // Try to infer element type from first element
    const firstElement = node.elements[0];
    if (firstElement && !t.isSpreadElement(firstElement)) {
      const elementType = this.inferExpressionType(firstElement, path);
      return { type: 'array', arrayElementType: elementType };
   * Infer type for object expressions
  private inferObjectType(node: t.ObjectExpression, path?: NodePath): TypeInfo {
      // Handle spread elements: { ...otherObject, newProp: value }
        const spreadType = this.inferExpressionType(prop.argument, path);
        // If spreading an object, merge its fields into our fields map
        if (spreadType.type === 'object' && spreadType.fields) {
          for (const [fieldName, fieldInfo] of spreadType.fields.entries()) {
            // Only add if not already present (later properties override spread)
            if (!fields.has(fieldName)) {
              fields.set(fieldName, fieldInfo);
      // Handle regular object properties
      else if (t.isObjectProperty(prop) && t.isIdentifier(prop.key)) {
        const propType = this.inferExpressionType(prop.value as t.Expression, path);
        fields.set(propName, {
          type: propType.type,
          fromMetadata: false,
          nullable: propType.nullable
    return { type: 'object', fields };
   * Infer type for call expressions
  private inferCallExpressionType(node: t.CallExpression, path?: NodePath): TypeInfo {
    // Check for user-defined function calls
    if (t.isIdentifier(node.callee)) {
      const functionName = node.callee.name;
      // Check if we've tracked this function's return type
      const returnType = this.functionReturnTypes.get(functionName);
      // Check for React hooks that return computed values
      // useMemo(() => { return {...} }, [deps]) - returns the memoized value
      if (functionName === 'useMemo' && node.arguments.length > 0) {
        const callback = node.arguments[0];
        // Extract the return type from the callback
        if (t.isArrowFunctionExpression(callback) || t.isFunctionExpression(callback)) {
          const body = callback.body;
          // Arrow function with expression body: useMemo(() => ({...}))
            return this.inferExpressionType(body, path);
          // Arrow function with block body: useMemo(() => { return {...} })
                return this.inferExpressionType(stmt.argument, path);
      // useCallback returns a function, but we don't track function return types
      // Other hooks (useState, useEffect, etc.) have specific return types we could handle
    // Check for RunView/RunQuery calls
    if (t.isMemberExpression(node.callee)) {
      const calleeObj = node.callee.object;
      const calleeProp = node.callee.property;
      // utilities.rv.RunView or utilities.rv.RunViews
      if (t.isMemberExpression(calleeObj) &&
          t.isIdentifier(calleeObj.property) &&
          t.isIdentifier(calleeProp)) {
        const serviceName = calleeObj.property.name;
        const methodName = calleeProp.name;
        if (serviceName === 'rv' && (methodName === 'RunView' || methodName === 'RunViews')) {
          return this.inferRunViewResultType(node);
        if (serviceName === 'rq' && methodName === 'RunQuery') {
          return this.inferRunQueryResultType(node);
    // Object.values() - returns array of object's values
    if (t.isMemberExpression(node.callee) &&
        node.callee.property.name === 'values' &&
        node.arguments.length === 1) {
      const objectType = this.inferExpressionType(node.arguments[0], path);
      if (objectType.type === 'object' && objectType.objectValueType) {
        // Return array of the object's value type
          arrayElementType: objectType.objectValueType
      // Fallback: unknown array
    // Array methods that return arrays
      const arrayMethods = ['filter', 'map', 'slice', 'concat', 'flat', 'flatMap', 'sort', 'reverse'];
        const arrayType = this.inferExpressionType(node.callee.object, path);
        if (arrayType.type === 'array') {
          // For map, the element type might change
          if (methodName === 'map') {
          return arrayType;
      // Array methods that return elements
      if (methodName === 'find') {
        if (arrayType.arrayElementType) {
          return { ...arrayType.arrayElementType, nullable: true };
      // Methods that return numbers
      if (['indexOf', 'findIndex', 'length'].includes(methodName)) {
        return StandardTypes.number;
      // Methods that return booleans
      if (['includes', 'some', 'every'].includes(methodName)) {
        return StandardTypes.boolean;
   * Infer type for new expressions (constructor calls)
  private inferNewExpressionType(node: t.NewExpression, path?: NodePath): TypeInfo {
    // Check for common constructors
      const constructorName = node.callee.name;
      // Date constructor
      if (constructorName === 'Date') {
        return { type: 'Date', fromMetadata: false };
      // Array constructor
      if (constructorName === 'Array') {
      // Object constructor
      if (constructorName === 'Object') {
        return { type: 'object', fields: new Map() };
      // Map constructor
      if (constructorName === 'Map') {
        return { type: 'Map', fromMetadata: false };
      // Set constructor
      if (constructorName === 'Set') {
        return { type: 'Set', fromMetadata: false };
      // RegExp constructor
      if (constructorName === 'RegExp') {
        return { type: 'RegExp', fromMetadata: false };
      // Error and related constructors
      if (['Error', 'TypeError', 'RangeError', 'ReferenceError', 'SyntaxError'].includes(constructorName)) {
        return { type: 'Error', fromMetadata: false };
   * Infer type for RunView result
  private inferRunViewResultType(node: t.CallExpression): TypeInfo {
    // Try to extract EntityName from the arguments
    if (node.arguments.length > 0 && t.isObjectExpression(node.arguments[0])) {
      for (const prop of node.arguments[0].properties) {
    return this.typeContext.createRunViewResultType(entityName || 'unknown');
   * Infer type for RunQuery result
  private inferRunQueryResultType(node: t.CallExpression): TypeInfo {
    // Try to extract QueryName and Parameters from the arguments
    let parametersNode: t.ObjectExpression | undefined;
    // Validate query parameters if we have both queryName and parameters
    if (queryName && parametersNode) {
      this.validateQueryParameters(queryName, parametersNode);
    return this.typeContext.createRunQueryResultType(queryName || 'unknown');
   * Validate query parameters, especially date parameters
  private validateQueryParameters(queryName: string, parametersNode: t.ObjectExpression): void {
    // Get the query definition from component spec
    const query = this.componentSpec?.dataRequirements?.queries?.find(
      q => q.name === queryName
    if (!query?.parameters) {
    // Build a map of parameter types with isRequired flag
    const paramTypeMap = new Map<string, { type: string; sqlType: string; isRequired: boolean }>();
      const extParam = param as { name: string; type?: string; isRequired?: boolean };
        paramTypeMap.set(param.name.toLowerCase(), {
          sqlType: extParam.type,
          isRequired: extParam.isRequired === true
    // Validate each parameter value
        const paramTypeInfo = paramTypeMap.get(paramName.toLowerCase());
        if (!paramTypeInfo) {
        // Check for date/datetime types
        const isDateType = ['date', 'datetime', 'datetime2', 'smalldatetime', 'datetimeoffset']
          .includes(paramTypeInfo.sqlType.toLowerCase());
        if (isDateType) {
          // Handle string literals: StartDate: '2024-01-01'
            this.validateDateParameter(paramName, prop.value.value, paramTypeInfo.isRequired, prop.loc);
          // Handle variables: StartDate: effectiveStartDate
          else if (t.isIdentifier(prop.value)) {
            this.validateDateVariable(paramName, prop.value.name, paramTypeInfo.isRequired, prop.loc);
          // Handle logical expressions: StartDate: startDate || appliedStartDate
          else if (t.isLogicalExpression(prop.value)) {
            // For logical expressions, check each operand
            this.validateDateLogicalExpression(paramName, prop.value, paramTypeInfo.isRequired, prop.loc);
   * Validate a date variable used as a parameter
  private validateDateVariable(paramName: string, variableName: string, isRequired: boolean, loc?: t.SourceLocation | null): void {
    // Look up the variable type in the type context
    const varType = this.typeContext.getVariableType(variableName);
      // Variable type unknown - could be a function parameter with no type info
      // We'll allow this for now (no warning)
    // Handle 'unknown' type - common with React useState(null)
    if (varType.type === 'unknown') {
      // For optional parameters, unknown is acceptable (likely React state)
      // For required parameters, emit a warning suggesting validation
      if (isRequired) {
        this.errors.push({
          type: 'warning',
          message: `Parameter "${paramName}" is required but variable "${variableName}" has unknown type. If "${variableName}" comes from React state (useState), ensure it's validated before calling RunQuery. Add a guard: if (!${variableName}) { return; }`,
          line: loc?.start.line || 0,
          column: loc?.start.column || 0,
          code: `// Add validation before RunQuery:\nif (!${variableName}) {\n  return; // or show error to user\n}`
      // For optional parameters with unknown type, allow silently
    // Check if the variable is typed as string (dates are passed as ISO strings)
    if (varType.type !== 'string') {
        message: `Parameter "${paramName}" expects a date string, but variable "${variableName}" has type "${varType.type}". Date parameters must be ISO date strings (e.g., '2024-01-01').`,
        code: `// Ensure ${variableName} contains a valid ISO date string`
      return; // Stop further validation if type is wrong
    // If we know the literal value (constant), validate it as a date string
    if (varType.literalValue !== undefined && typeof varType.literalValue === 'string') {
      this.validateDateParameter(paramName, varType.literalValue, isRequired, loc);
      return; // Already validated the literal value
    // If the variable is nullable and the parameter is required, flag it
    if (isRequired && varType.nullable) {
        message: `Parameter "${paramName}" is required but variable "${variableName}" may be null/undefined. Ensure "${variableName}" always has a valid date value before calling RunQuery.`,
        code: `// Add validation:\nif (!${variableName}) {\n  throw new Error('${paramName} is required');\n}`
   * Validate a logical expression (e.g., startDate || appliedStartDate)
  private validateDateLogicalExpression(paramName: string, node: t.LogicalExpression, isRequired: boolean, loc?: t.SourceLocation | null): void {
    // For || (OR) expressions, validate that at least one operand provides a valid date
    // For && (AND) expressions, validate the right operand (result of the expression)
    // For ?? (nullish coalescing), validate the right operand (fallback value)
    if (operator === '||' || operator === '??') {
      // Validate both sides - if left is null/undefined, right will be used
        this.validateDateVariable(paramName, node.left.name, false, loc); // Left can be nullable
      } else if (t.isStringLiteral(node.left)) {
        this.validateDateParameter(paramName, node.left.value, false, loc);
      if (t.isIdentifier(node.right)) {
        this.validateDateVariable(paramName, node.right.name, isRequired, loc); // Right inherits requirement
      } else if (t.isStringLiteral(node.right)) {
        this.validateDateParameter(paramName, node.right.value, isRequired, loc);
    } else if (operator === '&&') {
      // For AND, only the right side matters as the result
        this.validateDateVariable(paramName, node.right.name, isRequired, loc);
   * Validate a date parameter value
  private validateDateParameter(paramName: string, value: string, isRequired: boolean, loc?: t.SourceLocation | null): void {
    // Empty strings are invalid for date parameters
    if (value === '') {
      let code: string;
        message = `Parameter "${paramName}" is required and must have a valid ISO date value (e.g., '2024-01-01'). Empty strings are not allowed.`;
        code = `${paramName}: '2024-01-01'  // Required parameter`;
        message = `Parameter "${paramName}" is an optional date parameter but has an empty string value. Either provide a valid ISO date (e.g., '2024-01-01') or omit the parameter entirely from the Parameters object.`;
        code = `// Option 1: Provide valid date\n${paramName}: '2024-01-01'\n\n// Option 2: Remove parameter entirely\nParameters: {\n  // ${paramName}: <omitted>\n}`;
    // Validate ISO date format: YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS
    const isoDatePattern = /^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2}(\.\d{3})?(Z|[+-]\d{2}:\d{2})?)?$/;
    if (!isoDatePattern.test(value)) {
        message: `Parameter "${paramName}" is a date parameter but value '${value}' is not a valid ISO date format. Use YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS format.`,
        code: `${paramName}: '2024-01-01'  // or '2024-01-01T00:00:00'`
    // Validate that it's an actual valid date (not 2024-13-45)
    const parsedDate = new Date(value);
    if (isNaN(parsedDate.getTime())) {
        message: `Parameter "${paramName}" has an invalid date value '${value}'. The date format is correct but the date itself is invalid (e.g., month > 12, day > 31).`,
        code: `${paramName}: '2024-01-01'  // Use a valid date`
   * Infer type for member expressions
  private inferMemberExpressionType(node: t.MemberExpression, path?: NodePath): TypeInfo {
    const objectType = this.inferExpressionType(node.object, path);
    // Array index access
    if (node.computed && t.isNumericLiteral(node.property)) {
      if (objectType.arrayElementType) {
        return objectType.arrayElementType;
    // Property access
    if (t.isIdentifier(node.property)) {
      // Check if the object has known fields
      if (objectType.fields?.has(propName)) {
        const field = objectType.fields.get(propName)!;
        return { type: field.type, nullable: field.nullable };
      // Special case: Results property on RunView/RunQuery result
      if (propName === 'Results' && objectType.type === 'object') {
        // This should be an array of entity/query rows
        if (objectType.entityName) {
            arrayElementType: {
              entityName: objectType.entityName
        if (objectType.queryName) {
              queryName: objectType.queryName
      // Array length
      if (propName === 'length' && objectType.type === 'array') {
   * Infer type for binary expressions
  private inferBinaryExpressionType(node: t.BinaryExpression, path?: NodePath): TypeInfo {
    // Comparison operators always return boolean
    // Arithmetic operators return number
    // + can be string or number
    if (operator === '+') {
      // node.left can be PrivateName in some cases, skip type inference for those
      const leftType = this.inferExpressionType(node.left, path);
      const rightType = this.inferExpressionType(node.right, path);
      if (leftType.type === 'string' || rightType.type === 'string') {
      if (leftType.type === 'number' && rightType.type === 'number') {
   * Infer type for unary expressions
  private inferUnaryExpressionType(node: t.UnaryExpression): TypeInfo {
    if (operator === '!') {
    if (operator === 'typeof') {
    if (['+', '-', '~'].includes(operator)) {
    if (operator === 'void') {
      return StandardTypes.undefined;
   * Get inferred type for a variable by name
    return this.typeContext.getVariableType(name);
   * Check if an expression is a specific type
  isType(node: t.Expression, expectedType: string, path?: NodePath): boolean {
    const actualType = this.inferExpressionType(node, path);
    return actualType.type === expectedType;
 * Convenience function to analyze an AST and return type context
export async function analyzeTypes(
): Promise<TypeInferenceResult> {
  const engine = new TypeInferenceEngine(componentSpec, contextUser);
  return engine.analyze(ast);
