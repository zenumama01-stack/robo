 * Configuration settings for each instance's use of the JoinGrid via the related entity. 
 * Defines the shape of the JSON that should be used to configure the JoinGrid component.
 * This component enables many-to-many relationships by displaying a grid that shows
 * available rows and allows joining/unjoining entities.
export class JoinGridConfigInfo extends ComponentConfigBase {
     * The name of the entity that provides the rows in the grid (the "many" side of the relationship)
    public RowsEntityName: string;
     * Optional additional filter to apply to the rows entity query
    public RowsExtraFilter?: string;
     * The field name from the rows entity to display as the primary identifier
    public RowsEntityDisplayField: string;
     * Optional display name for the rows entity (if different from RowsEntityDisplayField)
    public RowsEntityDisplayName?: string;
     * Optional ORDER BY clause for sorting the rows
    public RowsOrderBy?: string;
     * Optional array of column names to display from the join entity
    public JoinEntityDisplayColumns?: string[];
     * Constructs a new JoinGridConfigInfo with default values
        this.RowsEntityName = "";
        this.RowsEntityDisplayField = "";
 * Generator class for creating JoinGrid components that handle many-to-many relationships.
 * This component generates Angular templates that use the MemberJunction JoinGrid component
 * to display and manage join relationships between entities.
 * The JoinGrid allows users to:
 * - View available entities that can be joined
 * - Add new join relationships
 * - Remove existing join relationships
 * - Display additional columns from the join entity
@RegisterClass(RelatedEntityDisplayComponentGeneratorBase, "JoinGrid")
export class JoinGridRelatedEntityGenerator extends RelatedEntityDisplayComponentGeneratorBase {
     * Returns the configuration type class used for this generator
     * @returns The JoinGridConfigInfo class type
    public get ConfigType(): typeof JoinGridConfigInfo {
        return JoinGridConfigInfo;
     * Returns the NPM package path for importing the JoinGrid Angular component
     * @returns The import path for the ng-join-grid module
        return "@memberjunction/ng-join-grid";
     * @returns Array containing the JoinGridComponent import details
                ClassName: "JoinGridComponent",  
                AngularSelectorName: "mj-join-grid",
                ModuleName: "JoinGridModule" 
     * Generates the Angular template for a JoinGrid component based on the relationship configuration
     * @returns Promise resolving to the generation result with the Angular template
     * @throws Error if the DisplayComponentConfiguration is invalid JSON
        const config = SafeJSONParse<JoinGridConfigInfo>(input.RelationshipInfo!.DisplayComponentConfiguration);
        if (!config)
            throw new Error("Invalid configuration for JoinGrid component for relationship " + input.RelationshipInfo!.ID);
        const template = `<mj-join-grid
    RowsEntityName="${config.RowsEntityName}"
    RowsEntityDisplayField="${config.RowsEntityDisplayField}"
    RowsExtraFilter="${config.RowsExtraFilter ? config.RowsExtraFilter : ''}"
    RowsOrderBy="${config.RowsOrderBy ? config.RowsOrderBy : ''}"
    RowsEntityDisplayName="${config.RowsEntityDisplayName ? config.RowsEntityDisplayName : config.RowsEntityDisplayField}"
    JoinEntityName="${input.RelationshipInfo!.RelatedEntity}"
    JoinEntityRowForeignKey="${this.GetForeignKeyName(input.RelationshipInfo!.RelatedEntity, config.RowsEntityName)}"
    [JoinEntityExtraFilter]="'${this.GetForeignKeyName(input.RelationshipInfo!.RelatedEntity, input.Entity!.Name)}=' + record.${input.Entity!.FirstPrimaryKey.Name}"
    [JoinEntityDisplayColumns]="${config.JoinEntityDisplayColumns ? `[${config.JoinEntityDisplayColumns.map(c => `'${c}'`).join(',')}]` : '[]'}"
    [NewRecordDefaultValues]="{${this.GetForeignKeyName(input.RelationshipInfo!.RelatedEntity, input.Entity!.Name)}: record.${input.Entity!.FirstPrimaryKey.Name}}"
