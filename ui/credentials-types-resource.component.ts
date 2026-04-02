import { ResourceData, MJCredentialTypeEntity, MJCredentialEntity } from '@memberjunction/core-entities';
import { CredentialTypeEditPanelComponent } from '@memberjunction/ng-credentials';
interface FieldSchemaProperty {
    isSecret: boolean;
interface TypeWithStats extends MJCredentialTypeEntity {
@RegisterClass(BaseResourceComponent, 'CredentialsTypesResource')
    selector: 'mj-credentials-types-resource',
    templateUrl: './credentials-types-resource.component.html',
    styleUrls: ['./credentials-types-resource.component.css'],
export class CredentialsTypesResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public types: TypeWithStats[] = [];
    public filteredTypes: TypeWithStats[] = [];
    public selectedType: TypeWithStats | null = null;
    public schemaProperties: FieldSchemaProperty[] = [];
    // Filters
    public selectedCategoryFilter = '';
    @ViewChild('typeEditPanel') typeEditPanel!: CredentialTypeEditPanelComponent;
        return 'Credential Types';
        return 'fa-solid fa-cubes';
        return this.checkEntityPermission('MJ: Credential Types', 'Create');
        return this.checkEntityPermission('MJ: Credential Types', 'Update');
        return this.checkEntityPermission('MJ: Credential Types', 'Delete');
    public get UserCanCreateCredential(): boolean {
            const [typeResult, credResult] = await rv.RunViews([
                const baseTypes = typeResult.Results as MJCredentialTypeEntity[];
                this.credentials = credResult.Success ? credResult.Results as MJCredentialEntity[] : [];
                // Calculate stats for each type
                this.types = baseTypes.map(type => this.enrichTypeWithStats(type));
                // Extract unique categories
                this.categories = [...new Set(this.types.map(t => t.Category))].sort();
            // Apply any navigation config (e.g., category filter from Categories nav item)
            console.error('Error loading credential types:', error);
            MJNotificationService.Instance.CreateSimpleNotification('Error loading credential types', 'error', 3000);
        // Apply category filter from navigation config
        if (config.categoryFilter) {
            this.selectedCategoryFilter = config.categoryFilter as string;
    private enrichTypeWithStats(type: MJCredentialTypeEntity): TypeWithStats {
        // Add stats properties directly to the entity object
        const enrichedType = type as TypeWithStats;
        enrichedType.credentialCount = typeCredentials.length;
        enrichedType.activeCount = typeCredentials.filter(c => c.IsActive).length;
        enrichedType.expiringCount = typeCredentials.filter(c =>
        return enrichedType;
    public createNewType(): void {
        if (this.typeEditPanel) {
            this.typeEditPanel.open(null);
    public editType(type: TypeWithStats, event?: Event): void {
            this.typeEditPanel.open(type);
    public async deleteType(type: TypeWithStats, event?: Event): Promise<void> {
            MJNotificationService.Instance.CreateSimpleNotification('You do not have permission to delete credential types', 'warning', 3000);
        if (type.credentialCount > 0) {
                `Cannot delete "${type.Name}" - it has ${type.credentialCount} credential(s) using it`,
        const confirmed = confirm(`Are you sure you want to delete "${type.Name}"? This action cannot be undone.`);
            const success = await type.Delete();
                MJNotificationService.Instance.CreateSimpleNotification(`Credential type "${type.Name}" deleted successfully`, 'success', 3000);
                this.types = this.types.filter(t => t.ID !== type.ID);
                if (this.selectedType?.ID === type.ID) {
                    this.closeDetail();
                MJNotificationService.Instance.CreateSimpleNotification('Failed to delete credential type', 'error', 3000);
            console.error('Error deleting credential type:', error);
            MJNotificationService.Instance.CreateSimpleNotification('Error deleting credential type', 'error', 3000);
    public createCredentialForType(type: TypeWithStats, event?: Event): void {
        // Navigate to Credentials nav item with the type pre-selected and create panel open
    public onTypeSaved(type: MJCredentialTypeEntity): void {
        const existingIndex = this.types.findIndex(t => t.ID === type.ID);
        const enrichedType = this.enrichTypeWithStats(type);
            this.types[existingIndex] = enrichedType;
            this.types.unshift(enrichedType);
        // Update categories if a new one was added
        if (!this.categories.includes(type.Category)) {
    public onTypeDeleted(typeId: string): void {
        this.types = this.types.filter(t => t.ID !== typeId);
        if (this.selectedType?.ID === typeId) {
    public onCategoryFilterChange(category: string): void {
        this.selectedCategoryFilter = category;
        this.selectedCategoryFilter = '';
        let filtered = [...this.types];
        if (this.selectedCategoryFilter) {
            filtered = filtered.filter(t => t.Category === this.selectedCategoryFilter);
                t.Name.toLowerCase().includes(search) ||
                (t.Description && t.Description.toLowerCase().includes(search)) ||
                t.Category.toLowerCase().includes(search)
        this.filteredTypes = filtered;
    public selectType(type: TypeWithStats): void {
        this.selectedType = type;
        this.parseFieldSchema(type.FieldSchema);
    public closeDetail(): void {
        this.selectedType = null;
        this.schemaProperties = [];
    private parseFieldSchema(schemaJson: string): void {
            const schema = JSON.parse(schemaJson) as { properties?: Record<string, Record<string, unknown>>; required?: string[] };
            const required = schema.required || [];
            this.schemaProperties = Object.entries(properties).map(([name, prop]) => ({
                type: (prop.type as string) || 'string',
                title: (prop.title as string) || name,
                description: (prop.description as string) || '',
                isSecret: prop.isSecret === true,
                required: required.includes(name)
            // Sort by order if available, otherwise by name
            this.schemaProperties.sort((a, b) => {
                const propA = properties[a.name];
                const propB = properties[b.name];
                const orderA = typeof propA.order === 'number' ? propA.order : 999;
                const orderB = typeof propB.order === 'number' ? propB.order : 999;
            console.error('Failed to parse field schema:', e);
    public getCategoryIcon(category: string): string {
    public getCategoryColor(category: string): string {
            'Storage': '#06b6d4',
            'Authentication': '#10b981',
            'Integration': '#ec4899'
        return colorMap[category] || '#6366f1';
    public getTypesByCategory(): Map<string, TypeWithStats[]> {
        const grouped = new Map<string, TypeWithStats[]>();
        for (const type of this.filteredTypes) {
    public getTotalCredentialCount(): number {
        return this.types.reduce((sum, t) => sum + t.credentialCount, 0);
