import { RunView, Metadata, EntityInfo } from '@memberjunction/core';
    GraphQLDataProvider,
    GraphQLVersionHistoryClient,
    CreateVersionLabelProgress
} from '@memberjunction/graphql-dataprovider';
export interface RecordOption {
    selector: 'mj-label-create',
    templateUrl: './label-create.component.html',
    styleUrls: ['./label-create.component.css'],
export class MjLabelCreateComponent implements OnInit {
     * When set, skips the entity picker step and pre-selects this entity.
    @Input() PreselectedEntity: EntityInfo | null = null;
     * When set along with PreselectedEntity, auto-selects these record IDs
     * and skips directly to the details step.
    @Input() PreselectedRecordIds: string[] = [];
    /** Emitted when labels are successfully created. */
    @Output() Created = new EventEmitter<{ LabelCount: number; ItemCount: number }>();
    /** Emitted when the user cancels. */
    // Wizard state
    public CreateStep: 'entity' | 'records' | 'details' | 'creating' | 'done' = 'entity';
    // Step 1: Entity selection
    public EntitySearchText = '';
    public FilteredEntities: EntityInfo[] = [];
    public SelectedEntity: EntityInfo | null = null;
    // Step 2: Record selection
    public RecordSearchText = '';
    public AvailableRecords: RecordOption[] = [];
    public FilteredRecords: RecordOption[] = [];
    public IsLoadingRecords = false;
    // Step 3: Details
    public LabelName = '';
    public LabelDescription = '';
    // Creating / done state
    public IsCreatingLabel = false;
    public CreateError = '';
    public CreatedLabelCount = 0;
    public CreatedItemCount = 0;
    public CreateProgress: CreateVersionLabelProgress | null = null;
        this.resetCreateDialog();
        if (this.PreselectedEntity) {
            this.SelectedEntity = this.PreselectedEntity;
            if (this.PreselectedRecordIds.length > 0) {
                this.skipToDetailsWithPreselection();
                this.CreateStep = 'records';
                this.loadEntityRecords(this.PreselectedEntity);
            this.FilteredEntities = this.getTrackableEntities();
    // Pre-selection helpers
    private async skipToDetailsWithPreselection(): Promise<void> {
        await this.loadEntityRecords(this.SelectedEntity!);
            this.preselectRecordsByIds(this.PreselectedRecordIds);
            this.CreateStep = 'details';
            this.LabelName = this.suggestLabelName();
    private preselectRecordsByIds(ids: string[]): void {
        const idSet = new Set(ids);
        for (const record of this.AvailableRecords) {
            record.Selected = idSet.has(record.ID);
    // Step 1: Entity picker
    public OnEntitySearchChange(text: string): void {
        this.EntitySearchText = text;
        const search = text.toLowerCase();
        const all = this.getTrackableEntities();
        this.FilteredEntities = search
            ? all.filter(e => e.Name.toLowerCase().includes(search))
            : all;
    public SelectEntity(entity: EntityInfo): void {
        this.SelectedEntity = entity;
        this.loadEntityRecords(entity);
    public OnRecordSearchChange(text: string): void {
        this.RecordSearchText = text;
        this.FilteredRecords = search
            ? this.AvailableRecords.filter(r => r.DisplayName.toLowerCase().includes(search))
            : [...this.AvailableRecords];
    public ToggleRecordSelection(record: RecordOption): void {
        record.Selected = !record.Selected;
    public SelectAllRecords(): void {
        for (const r of this.FilteredRecords) {
            r.Selected = true;
    public DeselectAllRecords(): void {
        for (const r of this.AvailableRecords) {
            r.Selected = false;
    public get SelectedRecordCount(): number {
        return this.AvailableRecords.filter(r => r.Selected).length;
    // Step navigation
    public GoToDetailsStep(): void {
        if (this.SelectedRecordCount === 0) return;
    public GoBackToRecords(): void {
    public GoBackToEntity(): void {
        this.CreateStep = 'entity';
        this.SelectedEntity = null;
        this.AvailableRecords = [];
        this.FilteredRecords = [];
    // Step 3: Create labels
    public async CreateLabels(): Promise<void> {
        if (!this.SelectedEntity || !this.LabelName.trim()) return;
        this.IsCreatingLabel = true;
        this.CreateStep = 'creating';
        this.CreateError = '';
        this.CreatedLabelCount = 0;
        this.CreatedItemCount = 0;
            const selected = this.AvailableRecords.filter(r => r.Selected);
            if (selected.length === 1) {
                await this.createSingleLabel(selected[0]);
                await this.createGroupedLabels(selected);
            this.CreateStep = 'done';
        } catch (e: unknown) {
                this.CreateError = e instanceof Error ? e.message : String(e);
                this.IsCreatingLabel = false;
    public FinishCreate(): void {
            LabelCount: this.CreatedLabelCount,
            ItemCount: this.CreatedItemCount
    // Create helpers
    private async createSingleLabel(record: RecordOption): Promise<void> {
        const vhClient = new GraphQLVersionHistoryClient(GraphQLDataProvider.Instance);
        const result = await vhClient.CreateLabel({
            Name: this.LabelName.trim(),
            Description: this.LabelDescription.trim() || undefined,
            Scope: 'Record',
            EntityName: this.SelectedEntity!.Name,
            RecordKeys: [{ Key: 'ID', Value: record.ID }],
            IncludeDependencies: true,
            OnProgress: (progress) => {
                    this.CreateProgress = progress;
            throw new Error(result.Error ?? 'Failed to create version label');
            this.CreatedLabelCount = 1;
            this.CreatedItemCount = result.ItemsCaptured ?? 0;
    private async createGroupedLabels(records: RecordOption[]): Promise<void> {
        // Create parent container label (no RecordKey -> acts as a grouping label)
        const parentResult = await vhClient.CreateLabel({
            Scope: 'Entity',
            throw new Error(parentResult.Error ?? 'Failed to create parent version label');
            this.CreatedItemCount = parentResult.ItemsCaptured ?? 0;
        // Create child labels for each selected record
            const childResult = await vhClient.CreateLabel({
                Name: `${this.LabelName.trim()} \u2014 ${record.DisplayName}`,
                ParentID: parentResult.LabelID,
            if (!childResult.Success) {
                console.error(`Failed to create child label for record ${record.ID}: ${childResult.Error}`);
                this.CreatedLabelCount++;
                this.CreatedItemCount += childResult.ItemsCaptured ?? 0;
    private async loadEntityRecords(entity: EntityInfo): Promise<void> {
        this.IsLoadingRecords = true;
        this.RecordSearchText = '';
            const nameField = this.findNameField(entity);
            const fields = nameField ? ['ID', nameField] : ['ID'];
                OrderBy: nameField ?? 'ID',
                this.AvailableRecords = result.Results.map(r => ({
                    ID: String(r['ID'] ?? ''),
                    DisplayName: nameField
                        ? String(r[nameField] ?? r['ID'] ?? '')
                        : String(r['ID'] ?? ''),
                    Selected: false
                this.FilteredRecords = [...this.AvailableRecords];
            console.error('Error loading entity records:', error);
                this.IsLoadingRecords = false;
    // Utility helpers
    private findNameField(entity: EntityInfo): string | null {
        const candidates = ['Name', 'Title', 'DisplayName', 'Label', 'Subject', 'Description'];
        for (const name of candidates) {
            const field = entity.Fields.find(f =>
                f.Name.toLowerCase() === name.toLowerCase() && f.TSType === 'string'
    private getTrackableEntities(): EntityInfo[] {
            .filter(e => e.TrackRecordChanges)
    private suggestLabelName(): string {
        const entityName = this.SelectedEntity?.Name ?? '';
            return `${selected[0].DisplayName} v1.0`;
        const date = new Date().toLocaleDateString('en-US', {
        return `${entityName} \u2014 ${date}`;
    private resetCreateDialog(): void {
        this.EntitySearchText = '';
        this.LabelName = '';
        this.LabelDescription = '';
        this.CreateProgress = null;
