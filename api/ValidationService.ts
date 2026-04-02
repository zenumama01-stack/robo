export class ValidationService {
  async validateAgentInput(agentName?: string, prompt?: string, chatMode?: boolean): Promise<void> {
    // If no agent name provided, we'll show the list (handled in command)
    if (!agentName) {
    // Validate agent name
    if (typeof agentName !== 'string' || agentName.trim().length === 0) {
      throw new Error(`❌ Invalid agent name
Problem: Agent name is empty or invalid
Provided: "${agentName}"
1. Provide a valid agent name with -a or --agent flag
3. Example: mj-ai agents:run -a "Demo Loop Agent" -p "Hello world"`);
    // In chat mode, prompt is optional (can be provided later)
    if (!chatMode && (!prompt || prompt.trim().length === 0)) {
      throw new Error(`❌ Prompt required for agent execution
Problem: No prompt provided for non-interactive mode
1. Provide a prompt with -p or --prompt flag
2. Or use --chat flag for interactive mode
3. Example: mj-ai agents:run -a "${agentName}" -p "Your prompt here"
4. Example: mj-ai agents:run -a "${agentName}" --chat`);
    // Validate prompt length (reasonable limits)
    if (prompt && prompt.length > 10000) {
      throw new Error(`❌ Prompt too long
Problem: Prompt exceeds maximum length (10,000 characters)
Current length: ${prompt.length}
1. Shorten your prompt to under 10,000 characters
2. Break complex requests into multiple interactions
3. Use --chat mode for longer conversations`);
  async validateActionInput(actionName?: string, parameters?: Record<string, any>): Promise<void> {
    // If no action name provided, we'll show the list (handled in command)
    if (!actionName) {
    // Validate action name
    if (typeof actionName !== 'string' || actionName.trim().length === 0) {
      throw new Error(`❌ Invalid action name
Problem: Action name is empty or invalid
Provided: "${actionName}"
1. Provide a valid action name with -n or --name flag
3. Example: mj-ai actions:run -n "Get Weather" --param "Location=Boston"`);
    // Validate parameters format
    if (parameters) {
      for (const [key, value] of Object.entries(parameters)) {
        if (typeof key !== 'string' || key.trim().length === 0) {
          throw new Error(`❌ Invalid parameter name
Problem: Parameter name is empty or invalid
Parameter: "${key}"
1. Ensure parameter names are non-empty strings
2. Use format: --param "ParameterName=value"
3. Example: --param "Location=Boston" --param "Units=metric"`);
        // Check for potentially problematic values
        if (typeof value === 'string' && value.length > 5000) {
          throw new Error(`❌ Parameter value too long
Problem: Parameter "${key}" exceeds maximum length (5,000 characters)
Current length: ${value.length}
1. Shorten the parameter value
2. Consider using a file reference instead of inline data
3. Break large data into smaller parameters`);
  validateTimeout(timeout: number): number {
    if (timeout < 1000) {
      throw new Error(`❌ Invalid timeout value
Problem: Timeout too short (minimum 1 second)
Provided: ${timeout}ms
1. Use a timeout of at least 1000ms (1 second)
2. Recommended: 30000ms (30 seconds) for simple operations
3. For complex operations: 300000ms (5 minutes) or more`);
    if (timeout > 1800000) { // 30 minutes
Problem: Timeout too long (maximum 30 minutes)
1. Use a timeout of at most 1800000ms (30 minutes)
2. For long-running operations, consider breaking into smaller tasks
3. Check if the operation can be optimized`);
    return timeout;
  validateOutputFormat(format: string): 'compact' | 'json' | 'table' {
    const validFormats = ['compact', 'json', 'table'];
    if (!validFormats.includes(format)) {
      throw new Error(`❌ Invalid output format
Problem: Unknown output format "${format}"
Valid formats: ${validFormats.join(', ')}
1. Use one of the supported formats: ${validFormats.join(', ')}
2. Example: --output compact
3. Example: --output json`);
    return format as 'compact' | 'json' | 'table';
  parseParameters(paramStrings: string[]): Record<string, any> {
    const parameters: Record<string, any> = {};
    for (const paramString of paramStrings) {
      const equalIndex = paramString.indexOf('=');
      if (equalIndex === -1) {
        throw new Error(`❌ Invalid parameter format
Problem: Parameter missing '=' separator
Parameter: "${paramString}"
1. Use format: --param "ParameterName=value"
2. Example: --param "Location=Boston"
3. For values with spaces: --param "Message=Hello world"`);
      const key = paramString.substring(0, equalIndex).trim();
      const value = paramString.substring(equalIndex + 1);
      if (key.length === 0) {
Problem: Parameter name is empty
1. Provide a parameter name before the '=' sign
3. Parameter names cannot be empty`);
      // Try to parse JSON values, but fall back to string
        // Check if it looks like JSON (starts with { or [, or is a number/boolean)
        const trimmedValue = value.trim();
        if (trimmedValue.startsWith('{') || trimmedValue.startsWith('[') || 
            trimmedValue === 'true' || trimmedValue === 'false' ||
            trimmedValue === 'null' || !isNaN(Number(trimmedValue))) {
          parameters[key] = JSON.parse(trimmedValue);
          parameters[key] = value;
        // Not JSON, treat as string
  validateDryRun(isDryRun: boolean, hasRequiredParams: boolean): void {
    if (isDryRun && !hasRequiredParams) {
      throw new Error(`❌ Dry run validation failed
Problem: Cannot validate without required parameters
Dry run: true
1. Provide all required parameters for validation
2. Or remove --dry-run flag to see parameter requirements
3. Use 'list' commands to see parameter requirements`);
import { EntityFieldInfo, EntityInfo, Metadata, RunView } from '@memberjunction/core';
import { RecordData } from '../lib/sync-engine';
import { getSystemUser } from '../lib/provider-utils';
// Type aliases for clarity
type EntityData = RecordData;
type EntitySyncConfig = any;
  private errors: ValidationError[] = [];
  private warnings: ValidationWarning[] = [];
  private entityDependencies: Map<string, EntityDependency> = new Map();
  private processedEntities: Set<string> = new Set();
  private options: ValidationOptions;
  private userRoleCache: Map<string, string[]> = new Map();
  constructor(options: Partial<ValidationOptions> = {}) {
   * Validates all metadata files in the specified directory
  public async validateDirectory(dir: string): Promise<ValidationResult> {
    if (this.options.include && this.options.exclude) {
      this.addError({
        file: dir,
        message: 'No .mj-sync.json configuration file found in directory',
      return this.getResult();
    const config = JSON.parse(fs.readFileSync(configPath, 'utf8'));
    // Load user role configuration and cache if enabled
    if (config.userRoleValidation?.enabled) {
      await this.loadUserRoles();
    const directories = await this.getDirectoriesInOrder(dir, config);
    let totalFiles = 0;
    const fileResults = new Map<string, FileValidationResult>();
    for (const subDir of directories) {
      const subDirPath = path.join(dir, subDir);
      const result = await this.validateEntityDirectory(subDirPath);
        totalFiles += result.files;
        totalEntities += result.entities;
        for (const [file, fileResult] of result.fileResults) {
          fileResults.set(file, fileResult);
    // Validate dependency order
    await this.validateDependencyOrder(directories);
      isValid: this.errors.length === 0,
      errors: this.errors,
        totalFiles,
        totalErrors: this.errors.length,
        totalWarnings: this.warnings.length,
        fileResults,
   * Validates a single entity directory
  private async validateEntityDirectory(dir: string): Promise<{ files: number; entities: number; fileResults: Map<string, FileValidationResult> } | null> {
    // Check for .mj-folder.json first (new format)
    let configPath = path.join(dir, '.mj-folder.json');
    let config: any;
    if (fs.existsSync(configPath)) {
      config = JSON.parse(fs.readFileSync(configPath, 'utf8'));
      // .mj-folder.json uses entityName field
      if (!config.entityName) {
          type: 'validation',
          file: configPath,
          message: 'Missing entityName field in .mj-folder.json',
      config.entity = config.entityName; // Normalize to entity field
      // Fall back to .mj-sync.json (legacy format)
      configPath = path.join(dir, '.mj-sync.json');
    // Validate entity name exists
    if (!config.entity || config.entity.trim() === '') {
        message: 'Entity name is empty or missing',
    const entityInfo = this.metadata.EntityByName(config.entity);
        message: `Entity "${config.entity}" not found in metadata`,
    const files = await this.getMatchingFiles(dir, config.filePattern);
      const filePath = path.join(dir, file);
      const result = await this.validateFile(filePath, entityInfo, config);
      totalEntities += result.entityCount;
      fileResults.set(filePath, result);
    return { files: files.length, entities: totalEntities, fileResults };
   * Validates a single metadata file
  private async validateFile(filePath: string, entityInfo: any, config: EntitySyncConfig): Promise<FileValidationResult> {
    const fileErrors: ValidationError[] = [];
    let entityCount = 0;
      const entities = Array.isArray(data) ? data : [data];
      entityCount = entities.length;
      for (const entityData of entities) {
        await this.validateEntityData(entityData, entityInfo, filePath, config);
      fileErrors.push({
        file: filePath,
        message: `Failed to parse JSON: ${error instanceof Error ? error.message : String(error)}`,
    // Collect errors and warnings for this file
    const currentFileErrors = this.errors.filter((e) => e.file === filePath);
    const currentFileWarnings = this.warnings.filter((w) => w.file === filePath);
      entityCount,
      errors: currentFileErrors,
      warnings: currentFileWarnings,
   * Validates a single entity data object
  private async validateEntityData(
    entityData: EntityData,
    config: EntitySyncConfig,
    parentContext?: { entity: string; field: string },
    // Skip validation for deletion records - they don't need field validation or reference checks
    if ((entityData as any).deleteRecord?.delete === true) {
      // Only validate that primaryKey exists for deletion records
      if (!entityData.primaryKey) {
          entity: entityInfo.Name,
          message: 'Deletion record is missing required "primaryKey" property',
          suggestion: 'Add primaryKey to identify the record to delete',
      return; // Skip all other validation for deletion records
    // Check nesting depth
    if (depth > this.options.maxNestingDepth) {
      this.addWarning({
        message: `Nesting depth ${depth} exceeds recommended maximum of ${this.options.maxNestingDepth}`,
        suggestion: 'Consider flattening the data structure or increasing maxNestingDepth',
    // Validate that 'fields' property exists (required)
    if (!entityData.fields) {
      const context = parentContext
        ? `Related entity "${parentContext.field}" in ${parentContext.entity}`
        : `Record`;
        message: `${context} is missing required "fields" property. Did you mean "fields" instead of "field"?`,
        suggestion: 'Each record must have a "fields" object containing the entity field values',
      return; // Can't continue validation without fields
    // Validate fields
    await this.validateFields(entityData.fields, entityInfo, filePath, parentContext);
    // Track dependencies
    this.trackEntityDependencies(entityData, entityInfo.Name, filePath);
    // Validate related entities
    if (entityData.relatedEntities) {
      for (const [relatedEntityName, relatedData] of Object.entries(entityData.relatedEntities)) {
        const relatedEntityInfo = this.metadata.EntityByName(relatedEntityName);
        if (!relatedEntityInfo) {
            message: `Related entity "${relatedEntityName}" not found in metadata`,
        const relatedEntities = Array.isArray(relatedData) ? relatedData : [relatedData];
        for (const relatedEntity of relatedEntities) {
          await this.validateEntityData(relatedEntity, relatedEntityInfo, filePath, config, { entity: entityInfo.Name, field: relatedEntityName }, depth + 1);
   * Validates entity fields
  private async validateFields(
    const entityFields = entityInfo.Fields;
    const fieldMap = new Map(entityFields.map((f) => [f.Name, f]));
      const fieldInfo = fieldMap.get(fieldName);
      if (!fieldInfo) {
        // Check if this might be a virtual property (getter/setter)
          const entityInstance = await this.metadata.GetEntityObject(entityInfo.Name, getSystemUser());
          // we use this approach instead of checking Entity Fields because
          // some sub-classes implement setter properties that allow you to set
          // values that are not physically in the database but are resolved by the sub-class
          // a good example is the sub-class for AI Prompts that has a property called TemplateText
          // that is automatically resolved into a separate record in the Templates/Template Contents entity
          const hasProperty = fieldName in entityInstance;
          if (!hasProperty) {
              message: `Field "${fieldName}" does not exist on entity "${entityInfo.Name}"`,
          // If we can't create an entity instance, fall back to error
      // Check if field is settable (not system field)
      if (fieldInfo.ReadOnly || fieldName.startsWith('__mj_')) {
          message: `Field "${fieldName}" is a read-only or system field and cannot be set`,
          suggestion: 'Remove this field from your metadata file',
      // Validate field value and references
      await this.validateFieldValue(fieldValue, fieldInfo, entityInfo, filePath, parentContext);
    // Check for required fields
    if (this.options.checkBestPractices) {
      for (const field of entityFields) {
        // Skip if field allows null or has a value already (use 'in' to handle falsy values like 0, false, "")
        if (field.AllowsNull || field.Name in fields) {
        // Skip if field has a default value
        if (field.DefaultValue !== null && field.DefaultValue !== undefined) {
        // Skip virtual/computed fields (foreign key reference fields)
        // These are typically named without 'ID' suffix but have a corresponding FK field
        const relatedEntityField = field.RelatedEntity;
        const correspondingFKField = entityFields.find((f: any) => f.Name === field.Name + 'ID' && f.IsForeignKey);
        if (relatedEntityField && correspondingFKField) {
        // Skip fields that are marked as AutoUpdateOnly or ReadOnly
        if (field.AutoIncrement || field.ReadOnly) {
        // Skip if this is a parent context and the field can be inherited
        if (parentContext && (field.Name === parentContext.field || field.Name === parentContext.field + 'ID')) {
        // Special case: Skip TemplateID if TemplateText is provided (virtual property)
        if (field.Name === 'TemplateID' && fields['TemplateText']) {
        // Skip Template field if TemplateText is provided
        if (field.Name === 'Template' && fields['TemplateText']) {
        // Skip Path on Applications - it is auto-calculated by the server on save
        if (field.Name === 'Path' && entityInfo.Name === 'MJ: Applications') {
          message: `Required field "${field.Name}" is missing`,
          suggestion: `Add "${field.Name}" to the fields object`,
   * Validates field values and references
  private async validateFieldValue(
    fieldInfo: EntityFieldInfo,
    if (typeof value === 'string' && this.isValidReference(value)) {
      await this.validateReference(value, fieldInfo, entityInfo, filePath, parentContext);
      // Skip further validation for references as they will be resolved later
    // Validate field value against value list if applicable
    await this.validateFieldValueList(value, fieldInfo, entityInfo, filePath);
    // Validate UserID fields against allowed roles
    if (fieldInfo.Name === 'UserID' && typeof value === 'string' && value.length > 0) {
      // Get the sync config from the file's directory
      // Walk up to find the root sync config with userRoleValidation
      let currentDir = dir;
      let config = null;
      while (currentDir && currentDir !== path.parse(currentDir).root) {
        const currentConfigPath = path.join(currentDir, '.mj-sync.json');
        if (fs.existsSync(currentConfigPath)) {
            const currentConfig = JSON.parse(fs.readFileSync(currentConfigPath, 'utf8'));
            if (currentConfig.userRoleValidation) {
              config = currentConfig;
      if (config?.userRoleValidation?.enabled) {
        await this.validateUserRole(value, entityInfo.Name, fieldInfo.Name, filePath, config);
    // Add other type validation here if needed
   * Validates field value against the field's value list if applicable
  private async validateFieldValueList(
    filePath: string
    // Skip validation if value is null/undefined (handled by required field check)
    // Check if this field has a value list constraint
    if (fieldInfo.ValueListType !== 'List') {
    // Get the allowed values from EntityFieldValues
    const entityFieldValues = fieldInfo.EntityFieldValues;
    if (!entityFieldValues || !Array.isArray(entityFieldValues) || entityFieldValues.length === 0) {
      // No values defined, skip validation
    // Extract the allowed values
    const allowedValues = entityFieldValues.map((efv: any) => efv.Value);
    // Convert value to string for comparison (in case it's a number or boolean)
    // Check if the value is in the allowed list
    if (!allowedValues.includes(stringValue)) {
      // Check case-insensitive match as a warning
      const caseInsensitiveMatch = allowedValues.find((av: string) => 
        av.toLowerCase() === stringValue.toLowerCase()
      if (caseInsensitiveMatch) {
          field: fieldInfo.Name,
          message: `Field "${fieldInfo.Name}" has value "${stringValue}" which differs in case from allowed value "${caseInsensitiveMatch}"`,
          suggestion: `Use "${caseInsensitiveMatch}" for consistency`,
        // Format the allowed values list for display
        const allowedValuesList = allowedValues.length <= 10 
          ? allowedValues.join(', ')
          : allowedValues.slice(0, 10).join(', ') + `, ... (${allowedValues.length - 10} more)`;
          message: `Field "${fieldInfo.Name}" has invalid value "${stringValue}"`,
          suggestion: `Allowed values are: ${allowedValuesList}`,
   * Check if a string is actually a MetadataSync reference (not just any @ string)
  private isValidReference(value: string): boolean {
   * Validates special references (@file:, @lookup:, etc.)
  private async validateReference(
    reference: string,
    const parsed = this.parseReference(reference);
        message: `Invalid reference format: "${reference}"`,
    switch (parsed.type) {
      case METADATA_KEYWORDS.FILE:
        await this.validateFileReference(parsed.value, filePath, entityInfo.Name, fieldInfo.Name);
      case METADATA_KEYWORDS.LOOKUP:
        await this.validateLookupReference(parsed, filePath, entityInfo.Name, fieldInfo.Name);
      case METADATA_KEYWORDS.TEMPLATE:
        await this.validateTemplateReference(parsed.value, filePath, entityInfo.Name, fieldInfo.Name);
      case METADATA_KEYWORDS.PARENT:
        this.validateParentReference(parsed.value, parentContext, filePath, entityInfo.Name, fieldInfo.Name);
      case METADATA_KEYWORDS.ROOT:
        this.validateRootReference(parsed.value, parentContext, filePath, entityInfo.Name, fieldInfo.Name);
   * Parses a reference string
  private parseReference(reference: string): ParsedReference | null {
    const patterns: [ReferenceType, RegExp][] = [
      [METADATA_KEYWORDS.FILE, /^@file:(.+)$/],
      [METADATA_KEYWORDS.LOOKUP, /^@lookup:([^.]+)\.(.+)$/],
      [METADATA_KEYWORDS.TEMPLATE, /^@template:(.+)$/],
      [METADATA_KEYWORDS.PARENT, /^@parent:(.+)$/],
      [METADATA_KEYWORDS.ROOT, /^@root:(.+)$/],
      [METADATA_KEYWORDS.ENV, /^@env:(.+)$/],
    for (const [type, pattern] of patterns) {
      const match = reference.match(pattern);
        if (type === METADATA_KEYWORDS.LOOKUP) {
          const [, entity, remaining] = match;
          // Check if this has ?create syntax
          const fields: Array<{field: string, value: string}> = [];
          // For backward compatibility, use the first field as primary
          const primaryField = fields.length > 0 ? fields[0] : { field: '', value: '' };
          const additionalFields: Record<string, any> = {};
            value: primaryField.value, 
            field: primaryField.field,
            fields, // Include all fields for validation
            createIfMissing: hasCreate, 
            additionalFields 
        return { type, value: match[1] };
   * Validates @file: references
  private async validateFileReference(filePath: string, sourceFile: string, entityName: string, fieldName: string, visitedFiles?: Set<string>): Promise<void> {
    const dir = path.dirname(sourceFile);
    const resolvedPath = path.resolve(dir, filePath);
    // Initialize visited files set if not provided (for circular reference detection)
    const visited = visitedFiles || new Set<string>();
    // Check for circular references
    if (visited.has(resolvedPath)) {
        file: sourceFile,
        message: `Circular @file reference detected: "${filePath}"`,
        details: `Path ${resolvedPath} is already being processed`,
        suggestion: 'Restructure your file references to avoid circular dependencies',
    if (!fs.existsSync(resolvedPath)) {
        message: `File reference not found: "${filePath}"`,
        suggestion: `Create file at: ${resolvedPath}`,
    // Add to visited set
    visited.add(resolvedPath);
    // Read the file and check for references
      const content = fs.readFileSync(resolvedPath, 'utf-8');
      // Check for {@include} references in all file types
      await this.validateIncludeReferences(content, resolvedPath, new Set([resolvedPath]));
      // If it's a JSON file, parse and validate nested @ references
      if (resolvedPath.endsWith('.json')) {
          const jsonContent = JSON.parse(content);
          // Check if JSON contains @include directives that need preprocessing
            await this.validateJsonIncludes(jsonContent, resolvedPath);
          // Recursively validate all @ references in the JSON structure
          await this.validateJsonReferences(jsonContent, resolvedPath, entityName, visited);
          // Not valid JSON or error parsing, treat as text file (already validated {@include} above)
          if (this.options.verbose) {
            console.log(`File ${resolvedPath} is not valid JSON, treating as text file`);
        message: `Failed to read file reference: "${filePath}"`,
        details: error instanceof Error ? error.message : String(error),
   * Validates @lookup: references
  private async validateLookupReference(parsed: ParsedReference, sourceFile: string, entityName: string, fieldName: string): Promise<void> {
    const lookupEntity = this.metadata.EntityByName(parsed.entity!);
    if (!lookupEntity) {
        message: `Lookup entity "${parsed.entity}" not found`,
        suggestion: 'Check entity name spelling and case',
    // For multi-field lookups, validate all fields
    if (parsed.fields && parsed.fields.length > 0) {
      for (const {field} of parsed.fields) {
        const lookupField = lookupEntity.Fields.find((f: any) => f.Name === field);
        if (!lookupField) {
            message: `Lookup field "${field}" not found on entity "${parsed.entity}"`,
            suggestion: `Available fields: ${lookupEntity.Fields.map((f: any) => f.Name).join(', ')}`,
    } else if (parsed.field) {
      // Fallback for single field lookup (backward compatibility)
      const lookupField = lookupEntity.Fields.find((f: any) => f.Name === parsed.field);
          message: `Lookup field "${parsed.field}" not found on entity "${parsed.entity}"`,
    // Track dependency
    this.addEntityDependency(entityName, parsed.entity!);
   * Validates @template: references
  private async validateTemplateReference(templatePath: string, sourceFile: string, entityName: string, fieldName: string): Promise<void> {
    const resolvedPath = path.resolve(dir, templatePath);
        message: `Template file not found: "${templatePath}"`,
        suggestion: `Create template at: ${resolvedPath}`,
    // Validate template is valid JSON
      JSON.parse(fs.readFileSync(resolvedPath, 'utf8'));
        message: `Template file is not valid JSON: "${templatePath}"`,
   * Validates @parent: references
  private validateParentReference(
    _fieldName: string,
    parentContext: { entity: string; field: string } | undefined,
    sourceFile: string,
    currentFieldName: string,
    if (!parentContext) {
        field: currentFieldName,
        message: `@parent: reference used but no parent context exists`,
        suggestion: '@parent: can only be used in nested/related entities',
   * Validates @root: references
  private validateRootReference(
        message: `@root: reference used but no root context exists`,
        suggestion: '@root: can only be used in nested/related entities',
   * Track entity dependencies
  private trackEntityDependencies(entityData: EntityData, entityName: string, filePath: string): void {
    if (!this.entityDependencies.has(entityName)) {
      this.entityDependencies.set(entityName, {
        dependsOn: new Set(),
    // Track dependencies from lookups in fields
    if (entityData.fields) {
      for (const value of Object.values(entityData.fields)) {
        if (typeof value === 'string' && value.startsWith(METADATA_KEYWORDS.LOOKUP)) {
          const parsed = this.parseReference(value);
          if (parsed?.entity) {
            this.addEntityDependency(entityName, parsed.entity);
   * Add an entity dependency
  private addEntityDependency(from: string, to: string): void {
    // Don't add self-references as dependencies (e.g., ParentID in hierarchical structures)
    if (from === to) {
    if (!this.entityDependencies.has(from)) {
      this.entityDependencies.set(from, {
        entityName: from,
        file: '',
    this.entityDependencies.get(from)!.dependsOn.add(to);
   * Validates dependency order
  private async validateDependencyOrder(directoryOrder: string[]): Promise<void> {
    // Build a map of entity to directory
    const entityToDirectory = new Map<string, string>();
      // This is simplified - in reality we'd need to read the .mj-sync.json
      // to get the actual entity name
      entityToDirectory.set(dir, dir);
    // Check for circular dependencies
    for (const [entity] of this.entityDependencies) {
        this.checkCircularDependency(entity, visited, recursionStack);
    // Check if current order satisfies dependencies
    const orderViolations = this.checkDependencyOrder(directoryOrder);
    if (orderViolations.length > 0) {
      // Suggest a corrected order
      const suggestedOrder = this.topologicalSort();
      for (const violation of orderViolations) {
          type: 'dependency',
          entity: violation.entity,
          file: violation.file,
          message: `Entity '${violation.entity}' depends on '${violation.dependency}' but is processed before it`,
          suggestion: `Reorder directories to: [${suggestedOrder.join(', ')}]`,
   * Check for circular dependencies
  private checkCircularDependency(entity: string, visited: Set<string>, recursionStack: Set<string>, path: string[] = []): boolean {
    recursionStack.add(entity);
    path.push(entity);
    const deps = this.entityDependencies.get(entity);
        if (!visited.has(dep)) {
          if (this.checkCircularDependency(dep, visited, recursionStack, [...path])) {
        } else if (recursionStack.has(dep)) {
          // Found circular dependency
          const cycle = [...path, dep];
          const cycleStart = cycle.indexOf(dep);
          const cyclePath = cycle.slice(cycleStart).join(' → ');
            type: 'circular',
            message: `Circular dependency detected: ${cyclePath}`,
            suggestion: 'Restructure your entities to avoid circular references',
    recursionStack.delete(entity);
   * Get directories in order based on config
  private async getDirectoriesInOrder(rootDir: string, config: any): Promise<string[]> {
    const allDirs = fs
      .readdirSync(rootDir)
      .filter((f) => fs.statSync(path.join(rootDir, f)).isDirectory())
      .filter((d) => !d.startsWith('.'));
    let orderedDirs: string[];
    if (config.directoryOrder && Array.isArray(config.directoryOrder)) {
      const ordered = config.directoryOrder.filter((d: string) => allDirs.includes(d));
      const remaining = allDirs.filter((d) => !ordered.includes(d)).sort();
      orderedDirs = [...ordered, ...remaining];
      orderedDirs = allDirs.sort();
    // Apply include/exclude filters if specified
    return this.applyDirectoryFilters(orderedDirs);
   * Apply include/exclude filters to directory list
  private applyDirectoryFilters(directories: string[]): string[] {
    if (this.options.include && this.options.include.length > 0) {
      filteredDirs = directories.filter(dirName => {
        return this.options.include!.some(pattern =>
    if (this.options.exclude && this.options.exclude.length > 0) {
      filteredDirs = filteredDirs.filter(dirName => {
        return !this.options.exclude!.some(pattern =>
   * Get files matching pattern
  private async getMatchingFiles(dir: string, pattern: string): Promise<string[]> {
    const files = fs.readdirSync(dir).filter((f) => fs.statSync(path.join(dir, f)).isFile());
    // Strip leading **/ from glob patterns (we only match in current directory)
    const normalizedPattern = pattern.replace(/^\*\*\//, '');
    // Simple glob pattern matching
    if (normalizedPattern === '*.json') {
      return files.filter((f) => f.endsWith('.json') && !f.startsWith('.mj-'));
    } else if (normalizedPattern === '.*.json') {
      return files.filter((f) => f.startsWith('.') && f.endsWith('.json') && !f.startsWith('.mj-'));
   * Add an error
  private addError(error: ValidationError): void {
    this.errors.push(error);
   * Add a warning
  private addWarning(warning: ValidationWarning): void {
   * Check if current directory order satisfies dependencies
  private checkDependencyOrder(directoryOrder: string[]): Array<{ entity: string; dependency: string; file: string }> {
      // In real implementation, we'd read .mj-sync.json to get entity name
      const entityName = dir; // Simplified for now
      const deps = this.entityDependencies.get(entityName);
   * Perform topological sort on entity dependencies
  private topologicalSort(): string[] {
        // Circular dependency, already handled by checkCircularDependency
      if (visited.has(entity)) {
          if (!visit(dep)) {
    // Visit all entities
    for (const entity of this.entityDependencies.keys()) {
   * Reset validation state
    this.errors = [];
    this.entityDependencies.clear();
    this.processedEntities.clear();
    this.userRoleCache.clear();
   * Get validation result
  private getResult(): ValidationResult {
        totalFiles: 0,
   * Load user roles from the database into cache
  private async loadUserRoles(): Promise<void> {
      // Load all user roles with role names
          OrderBy: 'UserID',
          MaxRows: 10000,
          file: 'system',
          message: 'Failed to load user roles for validation',
          details: result.ErrorMessage,
      // Group roles by UserID
      for (const userRole of result.Results || []) {
        const roleName = userRole.Role;
        if (!this.userRoleCache.has(userId)) {
          this.userRoleCache.set(userId, []);
        this.userRoleCache.get(userId)!.push(roleName);
        console.log(`Loaded roles for ${this.userRoleCache.size} users`);
        message: 'Error loading user roles for validation',
   * Validate a UserID field value against allowed roles
  private async validateUserRole(userId: string, entityName: string, fieldName: string, filePath: string, config: any): Promise<void> {
    // Skip if user role validation is not enabled
    if (!config.userRoleValidation?.enabled) {
    const userRoles = this.userRoleCache.get(userId);
    const allowedRoles = config.userRoleValidation.allowedRoles || [];
    const allowUsersWithoutRoles = config.userRoleValidation.allowUsersWithoutRoles || false;
    if (!userRoles || userRoles.length === 0) {
      if (!allowUsersWithoutRoles) {
          message: `UserID '${userId}' does not have any assigned roles`,
          suggestion:
            allowedRoles.length > 0
              ? `User must have one of these roles: ${allowedRoles.join(', ')}`
              : 'Assign appropriate roles to this user or set allowUsersWithoutRoles: true',
    // Check if user has at least one allowed role
    if (allowedRoles.length > 0) {
      const hasAllowedRole = userRoles.some((role) => allowedRoles.includes(role));
      if (!hasAllowedRole) {
          message: `UserID '${userId}' has roles [${userRoles.join(', ')}] but none are in allowed list`,
          suggestion: `Allowed roles: ${allowedRoles.join(', ')}`,
   * Validates {@include} references within file content
   * Recursively checks all {@include path} references in file content to ensure:
   * - Referenced files exist
   * - No circular references occur
   * - Include paths are valid
   * @param content - The file content to validate
   * @param filePath - Path of the file being validated
   * @param visitedPaths - Set of already visited paths for circular reference detection
  private async validateIncludeReferences(content: string, filePath: string, visitedPaths: Set<string>): Promise<void> {
      // Check for circular reference
      if (visitedPaths.has(resolvedPath)) {
          message: `Circular {@include} reference detected: "${trimmedPath}"`,
          suggestion: 'Restructure your includes to avoid circular references',
          message: `{@include} file not found: "${trimmedPath}"`,
      // Recursively validate the included file
        const includedContent = fs.readFileSync(resolvedPath, 'utf-8');
        const newVisitedPaths = new Set(visitedPaths);
        newVisitedPaths.add(resolvedPath);
        await this.validateIncludeReferences(includedContent, resolvedPath, newVisitedPaths);
          message: `Failed to read {@include} file: "${trimmedPath}"`,
   * Validates @include directives in JSON files
  private async validateJsonIncludes(jsonContent: any, sourceFile: string): Promise<void> {
    if (Array.isArray(jsonContent)) {
      for (const item of jsonContent) {
        if (typeof item === 'string' && item.startsWith(`${METADATA_KEYWORDS.INCLUDE}:`)) {
          const includePath = extractKeywordValue(item) as string;
          await this.validateIncludeFile(includePath.trim(), sourceFile);
          await this.validateJsonIncludes(item, sourceFile);
    } else if (jsonContent && typeof jsonContent === 'object') {
      for (const [key, value] of Object.entries(jsonContent)) {
          let includeFile: string;
            includeFile = value;
          } else if (value && typeof value === 'object' && 'file' in value) {
            includeFile = (value as any).file;
              message: `Invalid @include directive format for key "${key}"`,
              suggestion: 'Use either a string path or an object with a "file" property',
          await this.validateIncludeFile(includeFile, sourceFile);
          await this.validateJsonIncludes(value, sourceFile);
   * Validates a single include file path
  private async validateIncludeFile(includePath: string, sourceFile: string): Promise<void> {
    const resolvedPath = path.resolve(dir, includePath);
        message: `@include file not found: "${includePath}"`,
   * Recursively validates all @ references in a JSON structure
  private async validateJsonReferences(
    visitedFiles: Set<string>,
    parentContext?: { entity: string; field: string }
      for (const item of obj) {
        await this.validateJsonReferences(item, sourceFile, entityName, visitedFiles, parentContext);
    } else if (typeof obj === 'object') {
          // Process different reference types
            // Recursively validate the file reference (with circular detection)
            await this.validateFileReference(filePath, sourceFile, entityName, key, visitedFiles);
          } else if (value.startsWith(METADATA_KEYWORDS.LOOKUP)) {
            // Parse and validate lookup reference
              await this.validateLookupReference(parsed, sourceFile, entityName, key);
          } else if (value.startsWith(METADATA_KEYWORDS.TEMPLATE)) {
            const templatePath = extractKeywordValue(value) as string;
            await this.validateTemplateReference(templatePath, sourceFile, entityName, key);
          } else if (value.startsWith(METADATA_KEYWORDS.PARENT)) {
              this.validateParentReference(parsed.value, parentContext, sourceFile, entityName, key);
          } else if (value.startsWith(METADATA_KEYWORDS.ROOT)) {
              this.validateRootReference(parsed.value, parentContext, sourceFile, entityName, key);
          } else if (value.startsWith(METADATA_KEYWORDS.ENV)) {
            if (!process.env[envVar]) {
                message: `Environment variable "${envVar}" is not currently set`,
                suggestion: `Ensure this variable is set before running push operations`,
          await this.validateJsonReferences(value, sourceFile, entityName, visitedFiles, parentContext);
