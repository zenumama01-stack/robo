import { BaseEntity, ValidationResult } from "@memberjunction/core";
import { MJTemplateContentEntity, MJTemplateEntity, MJTemplateParamEntity } from "../generated/entity_subclasses";
@RegisterClass(BaseEntity, 'MJ: Templates')
export class TemplateEntityExtended extends MJTemplateEntity {
    private _Content: MJTemplateContentEntity[] = [];
    public get Content(): MJTemplateContentEntity[] {
        return this._Content;
    public set Content(value: MJTemplateContentEntity[]) {
        this._Content = value;
    private _Params: MJTemplateParamEntity[] = [];
    public get Params(): MJTemplateParamEntity[] {
    public set Params(value: MJTemplateParamEntity[]) {
        this._Params = value;
     * Returns all content for a given type for the template
    public GetContentByType(type: string): MJTemplateContentEntity[] {
        return this.Content.filter(c => c.Type.trim().toLowerCase() === type.trim().toLowerCase());
     * Returns the highest priority content for the template
     * @param type If provided, returns the highest priority content of the specified type
    public GetHighestPriorityContent(type?: string): MJTemplateContentEntity {
        if (type) {
            return this.Content.filter(c => c.Type.trim().toLowerCase() === type.trim().toLowerCase())
                .sort((a, b) => a.Priority - b.Priority)[0];
            return this.Content.sort((a, b) => a.Priority - b.Priority)[0];
     * Returns all parameters that apply to a specific template content.
     * This includes both global parameters (where TemplateContentID is NULL)
     * and content-specific parameters for the given contentId.
     * @param contentId - The ID of the template content. If not provided, returns only global parameters.
     * @returns Array of MJTemplateParamEntity objects that apply to the specified content
     * // Get all parameters for a specific content
     * const params = template.GetParametersForContent('content-uuid');
     * // Get only global parameters (that apply to all contents)
     * const globalParams = template.GetParametersForContent();
    public GetParametersForContent(contentId?: string): MJTemplateParamEntity[] {
        if (!contentId) {
            // Return only global parameters (TemplateContentID is null)
            return this.Params.filter(p => !p.TemplateContentID);
        // Return both global parameters and content-specific parameters
        return this.Params.filter(p =>
            !p.TemplateContentID || // Global param (applies to all contents)
            p.TemplateContentID === contentId // Content-specific param
     * This method is different from the Validate() method which validates the state of the Template itself.
     * This method validates the data object provided meets the requirements for the template's parameter definitions.
     * @param contentId - Optional: The ID of the template content to validate against.
     *                    If provided, validates against parameters specific to that content.
     *                    If not provided, validates against all parameters.
     * @returns ValidationResult with success status and any validation errors
     * // Validate against all parameters
     * const result = template.ValidateTemplateInput(inputData);
     * // Validate against parameters for a specific content
     * const result = template.ValidateTemplateInput(inputData, 'content-uuid');
    public ValidateTemplateInput(data: any, contentId?: string): ValidationResult {
        // Get the relevant parameters based on contentId
        const paramsToValidate = contentId ?
            this.GetParametersForContent(contentId) :
            this.Params;
        paramsToValidate.forEach((p) => {
                if (!data ||
                    data[p.Name] === undefined ||
                    data[p.Name] === null ||
                    (typeof data[p.Name] === 'string' && data[p.Name].toString().trim() === ''))
        // now set result's top level success flag based on the existence of ANY failure record within the errors collection
