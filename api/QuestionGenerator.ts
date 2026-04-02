 * QuestionGenerator - Generates business questions for entity groups using AI
 * Uses the Business Question Generator AI prompt to create 1-2 meaningful
 * business questions per entity group that can be answered with SQL.
import { formatEntityGroupForPrompt } from '../utils/entity-helpers';
import { EntityGroup, BusinessQuestion } from '../data/schema';
import { PROMPT_BUSINESS_QUESTION_GENERATOR } from '../prompts/PromptNames';
 * Result structure from Business Question Generator AI prompt
interface QuestionGeneratorResult {
  questions: BusinessQuestion[];
 * QuestionGenerator class
 * Generates domain-specific business questions using AI
export class QuestionGenerator {
   * Generate business questions for an entity group
   * Uses AI to create 1-2 meaningful questions per entity group
   * @param entityGroup - Entity group to generate questions for
   * @returns Array of validated business questions
  async generateQuestions(entityGroup: EntityGroup): Promise<BusinessQuestion[]> {
      // Find the Business Question Generator prompt
      const prompt = this.findPromptByName(aiEngine, PROMPT_BUSINESS_QUESTION_GENERATOR);
      // Format entity group for prompt
      const entityMetadata = formatEntityGroupForPrompt(entityGroup);
      const result = await this.executePrompt(prompt, entityMetadata);
      // Validate and filter questions
      const totalGenerated = result.questions.length;
      const validQuestions = this.validateQuestions(result.questions, entityGroup);
      // Log if questions were filtered out
      if (this.config.verbose && totalGenerated > validQuestions.length) {
          `QuestionGenerator: Filtered out ${totalGenerated - validQuestions.length} of ${totalGenerated} questions for ${entityGroup.primaryEntity.Name}`
      // Warn if no valid questions generated
      if (validQuestions.length === 0 && totalGenerated > 0) {
          `⚠️  QuestionGenerator: All ${totalGenerated} generated questions were filtered out for ${entityGroup.primaryEntity.Name}`
      return validQuestions;
        extractErrorMessage(error, 'QuestionGenerator.generateQuestions')
   * Execute the AI prompt with model/vendor overrides
    entityMetadata: unknown
  ): Promise<QuestionGeneratorResult> {
    const result = await executePromptWithOverrides<QuestionGeneratorResult>(
      { entityGroupMetadata: entityMetadata },
      throw new Error(`AI prompt execution failed: ${result?.errorMessage || 'Unknown error'}`);
   * Validate and filter business questions
   * Removes low-quality or unanswerable questions
   * Corrects entity name casing to match exact entity group names
   * @param questions - Raw questions from AI
   * @param entityGroup - Entity group for validation context
   * @returns Filtered array of valid questions with corrected entity names
  private validateQuestions(
    questions: BusinessQuestion[],
    entityGroup: EntityGroup
  ): BusinessQuestion[] {
    // Create case-insensitive lookup map: lowercase -> exact entity name
    // Also map BaseView names to correct entity names (in case LLM returns view names)
    const entityNameLookup = new Map<string, string>();
    for (const entity of entityGroup.entities) {
      // Map entity name (case-insensitive)
      entityNameLookup.set(entity.Name.toLowerCase(), entity.Name);
      // Map base view name to entity name (case-insensitive) as backup
      if (entity.BaseView) {
        entityNameLookup.set(entity.BaseView.toLowerCase(), entity.Name);
    return questions
      .filter((q) => {
        // Must have required fields
        if (!this.hasRequiredFields(q)) {
            LogStatus(`  ❌ Filtered: Missing required fields - "${q.userQuestion?.substring(0, 60)}..."`);
        // Must reference entities in the group (by name or base view)
        if (!this.referencesGroupEntities(q, entityNameLookup)) {
            LogStatus(`  ❌ Filtered: Doesn't reference group entities - "${q.userQuestion.substring(0, 60)}..." (references: ${q.entities.join(', ')})`);
      .map((q) => {
        // Correct entity name casing to match exact entity group names
        const correctedEntities = q.entities.map((entityName) => {
          const exactName = entityNameLookup.get(entityName.toLowerCase());
          return exactName || entityName; // Use exact name if found, otherwise keep original
          ...q,
          entities: correctedEntities
   * Check if question has all required fields
  private hasRequiredFields(question: BusinessQuestion): boolean {
      question.userQuestion &&
      question.description &&
      question.technicalDescription &&
      question.complexity &&
      Array.isArray(question.entities) &&
      question.entities.length > 0
   * Check if question references entities in the group
   * Uses case-insensitive matching to handle minor casing differences
   * Also accepts base view names as valid references
  private referencesGroupEntities(
    question: BusinessQuestion,
    entityNameLookup: Map<string, string>
    return question.entities.some((entityName) =>
      entityNameLookup.has(entityName.toLowerCase())
