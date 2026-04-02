export interface EntitySelectorConfig {
    displayField: string;
    descriptionField?: string;
    statusField?: string;
    filters?: string;
    orderBy?: string;
    selector: 'mj-entity-selector-dialog',
        <div class="dialog-wrapper">
            <h3>@if (config.icon) {
              <i [class]="config.icon"></i>
            } {{ config.title }}</h3>
            <div class="search-bar">
                [(ngModel)]="searchText"
                placeholder="Search..."
                (valueChange)="onSearchChange()"
                <p>Loading {{ config.entityName }}...</p>
            <!-- Entity List -->
            @if (!isLoading) {
              <div class="entity-list-container">
                @if (filteredEntities.length === 0) {
                    <p>No {{ config.entityName }} found</p>
                    @for (entity of filteredEntities; track entity.ID) {
                      <div class="entity-item"
                        [class.selected]="selectedEntity?.ID === entity.ID"
                        (click)="selectEntity(entity)">
                          <i [class]="config.icon || 'fa-solid fa-file'"></i>
                          <div class="item-title">{{ entity[config.displayField] || 'Untitled' }}</div>
                          @if (config.descriptionField && entity[config.descriptionField]) {
                            <div class="item-description">{{ entity[config.descriptionField] }}</div>
                          @if (config.statusField && entity[config.statusField]) {
                            <div class="item-status">
                              <span class="status-badge" [class.active]="entity[config.statusField] === 'Active'">
                                {{ entity[config.statusField] }}
            <button kendoButton themeColor="primary" (click)="createNew()">
              <i class="fa-solid fa-plus"></i> Create New
            <button kendoButton (click)="onCancel()">Cancel</button>
            <button kendoButton themeColor="primary" [disabled]="!selectedEntity" (click)="onSelect()">Select</button>
        .dialog-wrapper {
            width: 800px;
        .search-bar {
        .entity-list-container {
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
        .entity-item.selected {
            background: #f0f4f8;
        .item-status {
export class EntitySelectorDialogComponent implements OnInit {
    @Input() config!: EntitySelectorConfig;
    public entities: any[] = [];
    public filteredEntities: any[] = [];
    public selectedEntity: any = null;
    public searchText: string = '';
    public isLoading: boolean = true;
        public dialogRef: DialogRef
        await this.loadEntities();
    async loadEntities() {
                EntityName: this.config.entityName,
                ExtraFilter: this.config.filters,
                OrderBy: this.config.orderBy 
            this.entities = result.Results;
            this.filteredEntities = [...this.entities];
            this.entities = [];
            this.filteredEntities = [];
    onSearchChange() {
        if (!this.searchText) {
            const searchLower = this.searchText.toLowerCase();
            this.filteredEntities = this.entities.filter(entity => {
                const displayValue = entity[this.config.displayField] || '';
                const descriptionValue = this.config.descriptionField ? (entity[this.config.descriptionField] || '') : '';
                return displayValue.toLowerCase().includes(searchLower) || 
                       descriptionValue.toLowerCase().includes(searchLower);
    selectEntity(entity: any) {
        this.selectedEntity = entity;
    onSelect() {
        if (this.selectedEntity) {
            this.dialogRef.close({ entity: this.selectedEntity });
        this.dialogRef.close({ createNew: true });
        this.dialogRef.close(null);
