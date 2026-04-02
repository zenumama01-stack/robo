 * @fileoverview Runtime utilities for React components providing access to MemberJunction core functionality
 * @module @memberjunction/ng-react/utilities
  RunQuery, 
  RunViewParams, 
  RunQueryParams,
  LogError,
  BaseEntity,
  IEntityDataProvider
import { MJGlobal, RegisterClass } from '@memberjunction/global';
  ComponentUtilities, 
  SimpleAITools, 
  SimpleMetadata, 
  SimpleRunQuery, 
  SimpleRunView,
  SimpleExecutePromptParams,
  SimpleExecutePromptResult,
  SimpleEmbedTextParams,
  SimpleEmbedTextResult
} from '@memberjunction/interactive-component-types';
import { SimpleVectorService } from '@memberjunction/ai-vectors-memory';
 * Base class for providing runtime utilities to React components in Angular.
 * This class can be extended and registered with MJ's ClassFactory
 * to provide custom implementations of data access methods.
@RegisterClass(RuntimeUtilities, 'RuntimeUtilities')
export class RuntimeUtilities {
  private debug: boolean = false;
   * Builds the complete utilities object for React components
   * This is the main method that components will use
  public buildUtilities(debug: boolean = false): ComponentUtilities {
    this.debug = debug;
    return this.SetupUtilities(md);
   * Sets up the utilities object - copied from skip-chat implementation
  private SetupUtilities(md: Metadata): ComponentUtilities {
    const u: ComponentUtilities = {
      md: this.CreateSimpleMetadata(md),
      rv: this.CreateSimpleRunView(rv),
      rq: this.CreateSimpleRunQuery(rq),
      ai: this.CreateSimpleAITools()
  private CreateSimpleAITools(): SimpleAITools {
    // Get the GraphQL provider - it's the same as the BaseEntity provider
    const provider = BaseEntity.Provider;
    // Check if it's a GraphQLDataProvider
    if (!(provider instanceof GraphQLDataProvider)) {
      throw new Error('Current data provider is not a GraphQLDataProvider. AI tools require GraphQL provider.');
    const graphQLProvider = provider as GraphQLDataProvider;
      ExecutePrompt: async (params: SimpleExecutePromptParams): Promise<SimpleExecutePromptResult> => {
          // Use the AI client from GraphQLDataProvider to execute simple prompt
          const result = await graphQLProvider.AI.ExecuteSimplePrompt({
            systemPrompt: params.systemPrompt,
            preferredModels: params.preferredModels,
            modelPower: params.modelPower
          console.log(`🤖  ExecutePrompt succeeded!`);
            console.log('     > params', params);
            console.log('     > result:', result);
            result: result.result || '',
            resultObject: result.resultObject,
            modelName: result.modelName || ''
            result: 'Failed to execute prompt: ' + (error instanceof Error ? error.message : String(error)),
            modelName: ''
      EmbedText: async (params: SimpleEmbedTextParams): Promise<SimpleEmbedTextResult> => {
          // Use the AI client from GraphQLDataProvider to generate embeddings
          const result = await graphQLProvider.AI.EmbedText({
            textToEmbed: params.textToEmbed,
            modelSize: params.modelSize
            throw new Error(result.error || 'Failed to generate embeddings');
          const numEmbeddings: number = Array.isArray(params.textToEmbed) ? result.embeddings?.length : 1;
          console.log(`🤖  EmbedText succeeded! ${numEmbeddings} embeddings returned`);
            result: result.embeddings,
            modelName: result.modelName,
            vectorDimensions: result.vectorDimensions
          throw error; // Re-throw for embeddings as they're critical
      VectorService: new SimpleVectorService()
  private CreateSimpleMetadata(md: Metadata): SimpleMetadata {
      Entities: md.Entities,
      GetEntityObject: (entityName: string) => {
        return md.GetEntityObject(entityName)
  private CreateSimpleRunQuery(rq: RunQuery): SimpleRunQuery {
      RunQuery: async (params: RunQueryParams) => {
        // Run a single query and return the results
          const result = await rq.RunQuery(params);
            console.log(`✅ RunQuery "${params.QueryName}" succeeded: ${result.RowCount} rows returned`);
            console.error(`❌ RunQuery failed: ${result.ErrorMessage}`);
          console.error(`❌ RunQuery threw exception:`, error);
          throw error; // Re-throw to handle it in the caller
  private CreateSimpleRunView(rv: RunView): SimpleRunView {
      RunView: async (params: RunViewParams) => {
        // Run a single view and return the results
            console.log(`✅ RunView succeeded for ${params.EntityName}: ${result.TotalRowCount} rows returned`);
            console.error(`❌ RunView failed for ${params.EntityName}: ${result.ErrorMessage}`);
          console.error(`❌ RunView threw exception:`, error);
      RunViews: async (params: RunViewParams[]) => {
        // Runs multiple views and returns the results
          const results = await rv.RunViews(params);
          const entityNames = params.map(p => p.EntityName).join(', ');
          const totalRows = results.reduce((sum, r) => sum + (r.TotalRowCount || 0), 0);
          console.log(`✅ RunViews succeeded for [${entityNames}]: ${totalRows} total rows returned`);
            console.log('     > results:', results);
          console.error(`❌ RunViews threw exception:`, error);
 * Factory function to create RuntimeUtilities
 * In a Node.js environment, this will use MJ's ClassFactory for runtime substitution
 * In a browser environment, it will use the base class directly
export function createRuntimeUtilities(): RuntimeUtilities {
  // Check if we're in a Node.js environment with MJGlobal available
  if (typeof window === 'undefined') {
      // Use ClassFactory to get the registered class, defaulting to base RuntimeUtilities
      const obj = MJGlobal.Instance.ClassFactory.CreateInstance<RuntimeUtilities>(RuntimeUtilities);
      if (!obj) {
        throw new Error('Failed to create RuntimeUtilities instance');
      // Ensure the object is an instance of RuntimeUtilities
      // Fall through to default
  // Default: just use the base class
  return new RuntimeUtilities();
