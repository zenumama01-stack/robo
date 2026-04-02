import { ResourceData, MJTemplateEntity, MJTemplateContentEntity } from '@memberjunction/core-entities';
interface TemplateCardData {
    Entity: MJTemplateEntity;
    ContentTypes: string[];
    LastUpdated: Date | null;
    CategoryName: string;
@RegisterClass(BaseResourceComponent, 'CommunicationTemplatesResource')
    selector: 'mj-communication-templates-resource',
    <div class="templates-wrapper">
      <!-- HEADER -->
      <div class="templates-header">
          <h2>Communication Templates</h2>
          <p>Manage reusable message templates</p>
            <input type="text" placeholder="Search templates..." (input)="onSearch($event)">
          <button class="tb-btn primary" (click)="addNewTemplate()">
            <i class="fa-solid fa-plus"></i> New Template
      <!-- LOADING -->
          <mj-loading text="Loading templates..."></mj-loading>
      <!-- CATEGORY FILTERS -->
        <div class="category-filters">
          <div class="filter-chip" [class.active]="categoryFilter === ''"
            (click)="onCategoryFilter('')">
            All ({{allTemplates.length}})
            <div class="filter-chip"
              [class.active]="categoryFilter === cat"
              (click)="onCategoryFilter(cat)">
              {{cat}} ({{getCategoryCount(cat)}})
      <!-- TEMPLATES GRID -->
        <div class="templates-grid">
          @for (card of filteredTemplates; track card) {
            <div class="template-card"
              (click)="openTemplate(card.Entity)">
              <div class="template-card-header">
                <div class="template-icon">
                <div class="template-title-area">
                  <div class="template-name">{{card.Entity.Name}}</div>
                  <div class="template-category">{{card.CategoryName}}</div>
              @if (card.Entity.Description) {
                  {{card.Entity.Description}}
              <div class="template-meta">
                <div class="template-content-types">
                  @for (ct of card.ContentTypes; track ct) {
                    <span class="content-type-chip">
                      <i [class]="getContentTypeIcon(ct)"></i> {{ct}}
                  @if (card.ContentTypes.length === 0) {
                    <span class="content-type-chip empty">
                      No content
                @if (card.LastUpdated) {
                  <div class="template-updated">
                    Updated {{card.LastUpdated | date:'mediumDate'}}
          @if (filteredTemplates.length === 0) {
              <p>No templates found matching your criteria</p>
    .templates-wrapper {
    /* HEADER */
    .templates-header {
    .templates-header h2 {
    .templates-header p {
    /* CATEGORY FILTERS */
    .category-filters {
        gap: 4px; padding: 5px 14px;
    .templates-grid {
    .template-card {
    .template-card:hover {
    .template-card-header {
    .template-icon {
        width: 40px; height: 40px;
    .template-title-area { flex: 1; min-width: 0; }
        font-size: 14px; font-weight: 700;
    .template-category {
    .template-meta {
    .template-content-types {
    .content-type-chip {
        gap: 4px; padding: 3px 8px;
        border-radius: 10px; font-size: 10px; font-weight: 500;
    .content-type-chip.empty {
    .template-updated {
        padding: 64px 0; color: var(--mat-sys-on-surface-variant);
export class CommunicationTemplatesResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public allTemplates: TemplateCardData[] = [];
    public filteredTemplates: TemplateCardData[] = [];
    public categories: string[] = [];
    public categoryFilter = '';
            const [templatesResult, contentsResult] = await Promise.all([
                rv.RunView<MJTemplateEntity>({
                rv.RunView<MJTemplateContentEntity>({
            if (templatesResult.Success) {
                const contents = contentsResult.Success ? contentsResult.Results : [];
                this.allTemplates = templatesResult.Results.map(t => this.buildTemplateCard(t, contents));
                this.categories = this.extractCategories(this.allTemplates);
    private buildTemplateCard(template: MJTemplateEntity, allContents: MJTemplateContentEntity[]): TemplateCardData {
        const templateContents = allContents.filter(c => c.TemplateID === template.ID);
        const contentTypes = [...new Set(templateContents.map(c => c.TypeID ? 'Content' : 'Text'))];
        const category = template.Category || 'Uncategorized';
        const lastUpdated = template.Get('__mj_UpdatedAt') as Date | null;
            Entity: template,
            ContentTypes: contentTypes.length > 0 ? contentTypes : [],
            LastUpdated: lastUpdated,
            CategoryName: category
    private extractCategories(cards: TemplateCardData[]): string[] {
        const cats = new Set(cards.map(c => c.CategoryName));
        return Array.from(cats).sort();
    public getCategoryCount(category: string): number {
        return this.allTemplates.filter(t => t.CategoryName === category).length;
    public onCategoryFilter(category: string): void {
        this.categoryFilter = category;
        let filtered = this.allTemplates;
        if (this.categoryFilter) {
            filtered = filtered.filter(t => t.CategoryName === this.categoryFilter);
            filtered = filtered.filter(t =>
                t.Entity.Name?.toLowerCase().includes(this.searchTerm) ||
                t.Entity.Description?.toLowerCase().includes(this.searchTerm) ||
                t.CategoryName.toLowerCase().includes(this.searchTerm)
        this.filteredTemplates = filtered;
    public openTemplate(template: MJTemplateEntity): void {
        pk.LoadFromEntityInfoAndRecord(new Metadata().Entities.find(e => e.Name === 'MJ: Templates')!, template);
        this.navService.OpenEntityRecord('MJ: Templates', pk);
    public addNewTemplate(): void {
        this.navService.OpenEntityRecord('MJ: Templates', new CompositeKey());
    public getContentTypeIcon(type: string): string {
        const t = type.toLowerCase();
        if (t.includes('html')) return 'fa-solid fa-code';
        if (t.includes('text') || t.includes('plain')) return 'fa-solid fa-align-left';
        if (t.includes('sms')) return 'fa-solid fa-comment-sms';
        return 'fa-solid fa-file';
        return 'Templates';
        return 'fa-solid fa-file-lines';
