type CategoryType = 'AI' | 'Authentication' | 'Communication' | 'Database' | 'Integration' | 'Storage';
interface SchemaField {
    selector: 'mj-credential-type-edit-panel',
    templateUrl: './credential-type-edit-panel.component.html',
    styleUrls: ['./credential-type-edit-panel.component.css'],
export class CredentialTypeEditPanelComponent implements OnInit {
    @Input() credentialType: MJCredentialTypeEntity | null = null;
    @Output() saved = new EventEmitter<MJCredentialTypeEntity>();
    public category: CategoryType = 'Integration';
    public validationEndpoint = '';
    public schemaFields: SchemaField[] = [];
    // Available categories
    public categories: CategoryType[] = ['AI', 'Authentication', 'Communication', 'Database', 'Integration', 'Storage'];
        return this.isNew ? 'Create Credential Type' : 'Edit Credential Type';
        return this.name.trim().length > 0 && this.category.length > 0;
    public async open(credentialType: MJCredentialTypeEntity | null): Promise<void> {
        this.credentialType = credentialType;
        this.isNew = !credentialType || !credentialType.ID;
        if (credentialType && credentialType.ID) {
            this.populateFromType(credentialType);
        this.category = 'Integration';
        this.validationEndpoint = '';
    private populateFromType(credentialType: MJCredentialTypeEntity): void {
        this.name = credentialType.Name || '';
        this.description = credentialType.Description || '';
        this.category = credentialType.Category || 'Integration';
        this.iconClass = credentialType.IconClass || '';
        this.validationEndpoint = credentialType.ValidationEndpoint || '';
        // Parse field schema
        this.parseFieldSchema(credentialType.FieldSchema);
            if (!schemaJson) {
            const schema = JSON.parse(schemaJson) as {
            this.schemaFields = Object.entries(properties).map(([name, prop]) => ({
    private buildFieldSchema(): string {
        if (this.schemaFields.length === 0) {
            return JSON.stringify({ type: 'object', properties: {}, required: [] });
        const properties: Record<string, Record<string, unknown>> = {};
        const required: string[] = [];
        for (let i = 0; i < this.schemaFields.length; i++) {
            const field = this.schemaFields[i];
            properties[field.name] = {
                type: field.type,
                title: field.title,
                description: field.description,
                isSecret: field.isSecret,
                order: i
            if (field.required) {
                required.push(field.name);
            $schema: 'http://json-schema.org/draft-07/schema#',
            properties,
    // Schema field management
    public addSchemaField(): void {
        this.schemaFields.push({
            isSecret: false,
            required: false,
            order: this.schemaFields.length
    public removeSchemaField(index: number): void {
        this.schemaFields.splice(index, 1);
    public moveFieldUp(index: number): void {
            const temp = this.schemaFields[index];
            this.schemaFields[index] = this.schemaFields[index - 1];
            this.schemaFields[index - 1] = temp;
    public moveFieldDown(index: number): void {
        if (index < this.schemaFields.length - 1) {
            this.schemaFields[index] = this.schemaFields[index + 1];
            this.schemaFields[index + 1] = temp;
            let entity: MJCredentialTypeEntity;
                entity = await this._metadata.GetEntityObject<MJCredentialTypeEntity>('MJ: Credential Types');
                entity = this.credentialType!;
            entity.Category = this.category;
            entity.ValidationEndpoint = this.validationEndpoint.trim() || null;
            entity.FieldSchema = this.buildFieldSchema();
                    `Credential type "${entity.Name}" ${action} successfully`,
                    `Failed to save credential type: ${errorMessage}`,
            console.error('Error saving credential type:', error);
                'Error saving credential type',
    public async deleteType(): Promise<void> {
        if (this.isNew || !this.credentialType) return;
        const confirmed = confirm(`Are you sure you want to delete "${this.credentialType.Name}"? This action cannot be undone.`);
            const success = await this.credentialType.Delete();
                    `Credential type "${this.credentialType.Name}" deleted successfully`,
                this.deleted.emit(this.credentialType.ID);
                const errorMessage = this.credentialType.LatestResult?.Message || 'Unknown error';
                    `Failed to delete credential type: ${errorMessage}`,
                'Error deleting credential type',
        this.credentialType = null;
