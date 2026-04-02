import { ComponentEntityExtended } from "../custom/ComponentEntityExtended";
    MJComponentDependencyEntity 
export class ComponentMetadataEngine extends BaseEngine<ComponentMetadataEngine> {
    public static get Instance(): ComponentMetadataEngine {
       return super.getInstance<ComponentMetadataEngine>();
    private _components: ComponentEntityExtended[];
    private _componentLibraries: MJComponentLibraryEntity[];
    private _componentLibraryLinks: MJComponentLibraryLinkEntity[];
    private _componentRegistries: MJComponentRegistryEntity[];
    private _componentDependencies: MJComponentDependencyEntity[];
                PropertyName: "_components",
                EntityName: 'MJ: Component Libraries',
                PropertyName: "_componentLibraries",
                EntityName: 'MJ: Component Library Links',
                PropertyName: "_componentLibraryLinks",
                EntityName: 'MJ: Component Registries',
                PropertyName: "_componentRegistries",
                EntityName: 'MJ: Component Dependencies',
                PropertyName: "_componentDependencies",
    public get Components(): ComponentEntityExtended[] {
        return this._components;
    public get ComponentLibraries(): MJComponentLibraryEntity[] {
        return this._componentLibraries;
    public get ComponentLibraryLinks(): MJComponentLibraryLinkEntity[] {
        return this._componentLibraryLinks;
    public get ComponentRegistries(): MJComponentRegistryEntity[] {
        return this._componentRegistries;
    public get ComponentDependencies(): MJComponentDependencyEntity[] {
        return this._componentDependencies;
     * Finds a component on a case-insensitive match of name and optionally, namespace and registry if provided
    public FindComponent(name: string, namespace?: string, registry?: string): ComponentEntityExtended | undefined {
        const match =  this._components.find(c => c.Name.trim().toLowerCase() === name.trim().toLowerCase() && 
                                             c.Namespace?.trim().toLowerCase() === namespace?.trim().toLowerCase() && 
                                            (!registry || c.SourceRegistry?.trim().toLowerCase() === registry?.trim().toLowerCase()));
