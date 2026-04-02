import { EntityActionEngineServer } from "@memberjunction/actions";
import { Metadata, UserInfo, BaseEntity, CompositeKey, KeyValuePair, LogError } from "@memberjunction/core";
import { ActionParam, ActionResult } from "@memberjunction/actions-base";
import { KeyValuePairInput } from "../generic/KeyValuePairInput.js";
import { AppContext, ProviderInfo } from "../types.js";
import { CopyScalarsAndArrays } from "@memberjunction/global";
import { GetReadOnlyProvider } from "../util.js";
 * Input type for action parameters
 * Used to pass parameters to actions when invoking them
export class ActionParamInput {
   * The value of the parameter
   * Complex objects should be serialized to JSON strings
   * The data type of the parameter
   * Used for type conversion on the server
 * Input type for running an action
export class RunActionInput {
   * The ID of the action to run
   * Parameters to pass to the action
  @Field(() => [ActionParamInput], { nullable: true })
  Params?: ActionParamInput[];
   * Whether to skip logging the action execution
   * Defaults to false
  SkipActionLog?: boolean;
 * Represents a collection of key-value pairs that make up a composite key
 * Used for both primary keys and foreign keys
export class CompositeKeyInput {
   * The collection of key-value pairs that make up the composite key
  @Field(() => [KeyValuePairInput])
  KeyValuePairs: KeyValuePairInput[];
 * Input type for running entity actions
export class EntityActionInput {
   * The ID of the entity action to run
   * The type of invocation (SingleRecord, View, List, etc.)
   * The name of the entity
   * This is the preferred way to identify an entity as it's more human-readable than EntityID
   * The ID of the entity
   * Use EntityName instead when possible for better code readability
   * @deprecated Use EntityName instead when possible
   * The primary key of the entity record to act on
   * This is used for SingleRecord invocation types
  @Field(() => CompositeKeyInput, { nullable: true })
  PrimaryKey?: CompositeKeyInput;
   * The ID of the list to operate on
   * This is used for List invocation types
   * The ID of the view to operate on
   * This is used for View invocation types
   * Additional parameters to pass to the action
 * Output type for action results
 * Used to return results from actions to clients
export class ActionResultOutput {
   * Whether the action was executed successfully
   * Optional message describing the result of the action
   * Optional result code from the action
   * Optional result data from the action
   * Complex objects are serialized to JSON strings
  ResultData?: string;
 * Resolver for action-related GraphQL operations
 * Handles running actions and entity actions through GraphQL
export class ActionResolver extends ResolverBase {
   * Mutation for running an action
   * @param input The input parameters for running the action
   * @param ctx The GraphQL context containing user authentication information
   * @returns The result of running the action
  @Mutation(() => ActionResultOutput)
  async RunAction(
    @Arg("input") input: RunActionInput,
  ): Promise<ActionResultOutput> {
      // Check API key scope authorization for action execution
      await this.CheckAPIKeyScopeAuthorization('action:execute', input.ActionID, ctx.userPayload);
      // Get the user from context
        throw new Error("User is not authenticated");
      // Initialize the action engine
      await ActionEngineServer.Instance.Config(false, user);
      // Get the action by ID
      const action = this.findActionById(input.ActionID);
      // Parse the parameters
      const params = this.parseActionParameters(input.Params);
      // Run the action
      const result = await this.executeAction(action, user, params, input.SkipActionLog);
      // Return the result
      return this.createActionResult(result);
      return this.handleActionError(e);
   * Finds an action by its ID
   * @param actionID The ID of the action to find
   * @returns The action
   * @throws Error if the action is not found
  private findActionById(actionID: string): any {
    const action = ActionEngineServer.Instance.Actions.find(a => a.ID === actionID);
      throw new Error(`Action with ID ${actionID} not found`);
   * Parses action parameters from the input
   * @param inputParams The input parameters
   * @returns The parsed parameters
  private parseActionParameters(inputParams?: ActionParamInput[]): ActionParam[] {
    if (!inputParams || inputParams.length === 0) {
    return inputParams.map(p => {
      let value: any = p.Value;
      // Try to parse JSON for complex values
        if (p.Value && (p.Type === 'object' || p.Type === 'array')) {
          value = JSON.parse(p.Value);
        // If parsing fails, keep the original value
        LogError(`Failed to parse parameter value as JSON: ${error.message}`);
        Type: 'Input' // Default to Input type since we're sending parameters
   * Executes an action
   * @param action The action to execute
   * @param user The user context
   * @returns The action result
  private async executeAction(
    action: any, 
    skipActionLog?: boolean
    return await ActionEngineServer.Instance.RunAction({
      ContextUser: user,
      SkipActionLog: skipActionLog,
   * Creates an action result from the execution result
   * @param result The execution result
   * @returns The formatted action result
  private createActionResult(result: ActionResult): ActionResultOutput {
    const x =(result.Params || result.RunParams.Params || [])
              .filter(p => p.Type.trim().toLowerCase() === 'output' ||
                           p.Type.trim().toLowerCase() === 'both')     ;
      ResultCode: result.Result?.ResultCode,
      ResultData: x && x.length > 0 ? JSON.stringify(CopyScalarsAndArrays(x)) : undefined
   * Handles errors in the action resolver
  private handleActionError(e: unknown): ActionResultOutput {
    LogError(`Error in RunAction resolver: ${error}`);
      Message: `Error executing action: ${error.message}`
   * Mutation for running an entity action
   * @param input The input parameters for running the entity action
   * @returns The result of running the entity action
  async RunEntityAction(
    @Arg("input") input: EntityActionInput,
      // Check API key scope authorization for entity action execution
      await this.CheckAPIKeyScopeAuthorization('action:execute', input.EntityActionID, ctx.userPayload);
      // Initialize the entity action engine
      await EntityActionEngineServer.Instance.Config(false, user);
      // Get the entity action by ID
      const entityAction = this.getEntityAction(input.EntityActionID);
      // Create the base parameters
      const params = this.createBaseParams(entityAction, input.InvocationType, user);
      // Add entity object if we have entity information and primary key
      if ((input.EntityID || input.EntityName) && input.PrimaryKey && input.PrimaryKey.KeyValuePairs.length > 0) {
        await this.addEntityObject(ctx.providers, params, input, user);
      // Add other parameters
      this.addOptionalParams(params, input);
      // Run the entity action
      const result = await EntityActionEngineServer.Instance.RunEntityAction(params);
        ResultData: JSON.stringify(result)
      return this.handleError(e);
   * Gets an entity action by ID
   * @param actionID The ID of the entity action
   * @returns The entity action
   * @throws Error if entity action is not found
  private getEntityAction(actionID: string): any {
    const entityAction = EntityActionEngineServer.Instance.EntityActions.find(ea => ea.ID === actionID);
    if (!entityAction) {
      throw new Error(`EntityAction with ID ${actionID} not found`);
    return entityAction;
   * Creates the base parameters for the entity action
   * @param entityAction The entity action
   * @param invocationTypeName The invocation type name
   * @param user The authenticated user
   * @returns The base parameters
  private createBaseParams(entityAction: any, invocationTypeName: string, user: UserInfo): any {
      EntityAction: entityAction,
      InvocationType: { Name: invocationTypeName },
   * Adds the entity object to the parameters
   * @param params The parameters to add to
   * @param input The input parameters
  private async addEntityObject(providers: Array<ProviderInfo>, params: any, input: EntityActionInput, user: UserInfo): Promise<void> {
    const md = GetReadOnlyProvider(providers);
    // Find the entity by ID or name
    let entity;
    if (input.EntityName) {
      entity = md.Entities.find(e => e.Name === input.EntityName);
        throw new Error(`Entity with name ${input.EntityName} not found`);
    } else if (input.EntityID) {
      entity = md.Entities.find(e => e.ID === input.EntityID);
        throw new Error(`Entity with ID ${input.EntityID} not found`);
      throw new Error("Entity information is required");
    // Create a composite key and load the entity object
    const compositeKey = this.createCompositeKey(entity, input.PrimaryKey);
    const entityObject = await md.GetEntityObject(entity.Name);
    await entityObject.InnerLoad(compositeKey);
    params['EntityObject'] = entityObject;
   * Creates a composite key from the input
   * @param entity The entity information
   * @param primaryKey The primary key input
   * @returns The composite key
  private createCompositeKey(entity: any, primaryKey: CompositeKeyInput): CompositeKey {
    for (const kvp of primaryKey.KeyValuePairs) {
      // Convert value based on the field type if necessary
      const field = entity.Fields.find(f => f.Name === kvp.Key);
      let value: any = kvp.Value;
      // If the field is found, try to convert to proper type
        value = this.convertValueToProperType(value, field);
      // Add to composite key
      const kvPair = new KeyValuePair();
      kvPair.FieldName = kvp.Key;
      kvPair.Value = value;
      compositeKey.KeyValuePairs.push(kvPair);
   * Converts a value to the proper type based on the field information
   * @param value The value to convert
   * @param field The field information
   * @returns The converted value
  private convertValueToProperType(value: any, field: any): any {
    // Simple conversion, could be enhanced for other types
    if (field.Type.toLowerCase().match(/int|decimal|float|money|numeric|real/) && !isNaN(Number(value))) {
    } else if (field.Type.toLowerCase().includes('date') && !isNaN(Date.parse(value))) {
   * Adds optional parameters to the entity action parameters
  private addOptionalParams(params: any, input: EntityActionInput): void {
    // Add list ID if provided
    if (input.ListID) {
      params['ListID'] = input.ListID;
    // Add view ID if provided
    if (input.ViewID) {
      params['ViewID'] = input.ViewID;
    if (input.Params && input.Params.length > 0) {
      params.Params = input.Params.map(p => this.convertParameterValue(p));
   * Converts a parameter value to the proper format
   * @param p The parameter to convert
   * @returns The converted parameter
  private convertParameterValue(p: ActionParamInput): any {
   * Handles errors in the entity action resolver
  private handleError(e: unknown): ActionResultOutput {
    LogError(`Error in RunEntityAction resolver: ${error}`);
      Message: `Error executing entity action: ${error.message}`
