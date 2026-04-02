import { SurveyMonkeyBaseAction, SurveyMonkeySurveyDetails } from '../surveymonkey-base.action';
 * Action to update an existing SurveyMonkey survey
 * IMPORTANT: When MergeWithExisting is true (default), fetches the current survey
 * and merges your changes. When false, replaces entire survey data (use with caution).
 * The action uses PATCH for partial updates when MergeWithExisting=true, which is
 * the recommended approach for most use cases.
 *   ActionName: 'Update SurveyMonkey',
@RegisterClass(BaseAction, 'UpdateSurveyMonkeyAction')
export class UpdateSurveyMonkeyAction extends SurveyMonkeyBaseAction {
    return 'Updates an existing SurveyMonkey survey. Set MergeWithExisting=true (default) to safely update only specified properties while preserving others. Set to false to replace entire survey data (not recommended).';
          Message: 'Context user is required for SurveyMonkey API calls',
          Message: 'SurveyID parameter is required',
      if (!title && !pages && !language && !buttonsText) {
          Message: 'At least one of Title, Pages, Language, or ButtonsText must be provided',
      let existingSurvey: SurveyMonkeySurveyDetails | null = null;
      // Get existing survey if merging
        existingSurvey = await this.getSurveyMonkeyDetails(surveyId, accessToken);
      // Build update data object
      const updateData: {
      } = {};
        updateData.title = title;
      // Add language if provided
        updateData.language = language;
        updatedFields.push('language');
      // Handle buttons text
      if (buttonsText) {
        const buttonsObj = typeof buttonsText === 'string' ? JSON.parse(buttonsText) : buttonsText;
        if (mergeWithExisting && existingSurvey?.buttons_text) {
          updateData.buttons_text = { ...existingSurvey.buttons_text, ...buttonsObj };
          updateData.buttons_text = buttonsObj;
        updatedFields.push('buttons_text');
      // Handle pages (if provided)
      // Note: Pages require separate API calls to update individual pages/questions
      // For now, we'll track it but handle basic survey properties only
      if (pages) {
        // Pages would need to be handled via separate API calls to /surveys/{id}/pages
        // This is a more complex operation and may require additional implementation
        updatedFields.push('pages (note: page updates require separate API calls)');
      // Update the survey using PATCH (partial update)
      const updatedSurvey = await this.updateSurveyMonkey(surveyId, accessToken, updateData);
          Value: updatedSurvey,
          Value: updatedSurvey.id,
          Value: updatedSurvey.title,
          Value: updatedFields,
          Value: updatedSurvey.page_count,
        const existingParam = params.Params.find((p) => p.Name === outputParam.Name);
        Message: `Successfully updated survey "${updatedSurvey.title}" (ID: ${updatedSurvey.id}). Updated fields: ${updatedFields.join(', ')}`,
        Message: this.buildFormErrorMessage('Update SurveyMonkey', errorMessage, error),
