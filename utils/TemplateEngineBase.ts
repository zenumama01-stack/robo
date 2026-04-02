import { MJTemplateCategoryEntity, MJTemplateContentEntity, MJTemplateContentTypeEntity, TemplateEntityExtended, MJTemplateParamEntity } from "@memberjunction/core-entities";
 * TemplateEngine is used for accessing template metadata/caching it, and rendering templates
export class TemplateEngineBase extends BaseEngine<TemplateEngineBase> {
    public static get Instance(): TemplateEngineBase {
       return super.getInstance<TemplateEngineBase>();
        TemplateContentTypes: MJTemplateContentTypeEntity[],
        TemplateCategories: MJTemplateCategoryEntity[],
        Templates: TemplateEntityExtended[],
        TemplateContents: MJTemplateContentEntity[],
        TemplateParams: MJTemplateParamEntity[]
                DatasetName: 'Template_Metadata',
        // post-process the template content and params to associate them with a template
        this.Templates.forEach((t) => {
            t.Content = this.TemplateContents.filter((tc) => tc.TemplateID === t.ID);
            t.Params = this.TemplateParams.filter((tp) => tp.TemplateID === t.ID);
    public get Templates(): TemplateEntityExtended[] {
        return this._Metadata.Templates;
    public get TemplateContentTypes(): MJTemplateContentTypeEntity[] {
        return this._Metadata.TemplateContentTypes;
    public get TemplateCategories(): MJTemplateCategoryEntity[] {
        return this._Metadata.TemplateCategories;
    public get TemplateContents(): MJTemplateContentEntity[] {
        return this._Metadata.TemplateContents;
        return this._Metadata.TemplateParams;
     * Convenience method to find a template by name, case-insensitive
     * @param templateName 
    public FindTemplate(templateName: string): TemplateEntityExtended {
        return this.Templates.find((t) => t.Name.trim().toLowerCase() === templateName.trim().toLowerCase())
