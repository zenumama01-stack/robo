  OnChanges, SimpleChanges, OnInit, AfterContentInit, AfterViewInit, OnDestroy,
  ContentChildren, QueryList, HostBinding, HostListener, ElementRef,
  ViewChild, NgZone, ViewEncapsulation
import { FormContext, PanelVariant, PanelDragStartEvent, PanelDropEvent } from '../types/form-types';
import { MjFormFieldComponent } from '../field/form-field.component';
 * Reusable collapsible panel for form sections.
 * Supports three visual variants:
 * - **default**: White card with accent border (standard field sections)
 * - **related-entity**: Blue-accented card with row count badge (related entity grids)
 * - **inherited**: Purple-accented card with "Inherited from X" badge (IS-A parent field sections)
 * - Expand/collapse with smooth animation
 * - Search filtering (hides non-matching panels, highlights matched names)
 * - Drag-to-reorder sections
 * - Inheritable "Inherited from X" badge with navigation event
 * - Row count badge for related entity sections
 * <mj-collapsible-panel
 *   SectionKey="productDetails"
 *   SectionName="Product Details"
 *   Icon="fa fa-box"
 *   Variant="inherited"
 *   InheritedFromEntity="Products"
 *   [Form]="formComponent"
 *   (Navigate)="onNavigate($event)">
 *   <mj-form-field ...></mj-form-field>
 * </mj-collapsible-panel>
  selector: 'mj-collapsible-panel',
  encapsulation: ViewEncapsulation.None, // Required to style projected content (grids, etc.)
  templateUrl: './collapsible-panel.component.html',
  styleUrls: ['./collapsible-panel.component.css']
export class MjCollapsiblePanelComponent implements OnInit, OnChanges, AfterContentInit, AfterViewInit, OnDestroy {
  private elementRef = inject(ElementRef);
  /** Unique key for state persistence */
  @Input() SectionKey = '';
  /** Display name shown in the panel header */
  @Input() SectionName = '';
  /** Font Awesome icon class for the panel header */
  @Input() Icon = 'fa fa-folder';
   * Reference to the parent form component for state delegation.
   * Expected to have IsSectionExpanded, SetSectionExpanded, getSectionDisplayOrder methods.
  @Input() Form: unknown;
  /** Panel visual variant */
  @Input() Variant: PanelVariant = 'default';
  /** Row count badge for related entity sections */
  @Input() BadgeCount: number | undefined;
  /** Default expanded state when no persisted state exists */
  @Input() DefaultExpanded: boolean | undefined;
   * For 'inherited' variant: the parent entity name this section's fields come from.
   * Displayed as "Inherited from X" badge and used for navigation.
  @Input() InheritedFromEntity = '';
   * For 'inherited' variant: the primary key to navigate to when clicking the badge.
   * (Shared PK in IS-A relationships.)
  @Input() InheritedRecordPrimaryKey?: CompositeKey;
  /** @deprecated Use [SectionKey] instead */
  @Input('sectionKey') set _deprecatedSectionKey(value: string) { this.SectionKey = value; }
  /** @deprecated Use [SectionName] instead */
  @Input('sectionName') set _deprecatedSectionName(value: string) { this.SectionName = value; }
  /** @deprecated Use [Icon] instead */
  @Input('icon') set _deprecatedIcon(value: string) { this.Icon = value; }
  /** @deprecated Use [Form] instead */
  @Input('form') set _deprecatedForm(value: unknown) { this.Form = value; }
  @Output() DragStarted = new EventEmitter<PanelDragStartEvent>();
  @Output() DragEnded = new EventEmitter<void>();
  @Output() PanelDrop = new EventEmitter<PanelDropEvent>();
  @ContentChildren(MjFormFieldComponent, { descendants: true }) FieldComponents!: QueryList<MjFormFieldComponent>;
  // ---- State ----
  DisplayName = '';
  FieldNames = '';
  IsVisible = true;
  @HostBinding('class')
  get HostClass(): string {
    const classes = [`mj-panel--${this.Variant}`];
    if (!this.IsVisible) classes.push('mj-search-hidden');
    if (this.IsDragging) classes.push('mj-dragging');
    if (this.IsDragOver) classes.push('mj-drag-over');
  @HostBinding('style.order')
  get CssOrder(): number {
    const formRef = this.Form as { getSectionDisplayOrder?: (key: string) => number };
    return formRef?.getSectionDisplayOrder ? formRef.getSectionDisplayOrder(this.SectionKey) : 0;
  IsDragging = false;
  IsDragOver = false;
  // ---- Event relay ----
  /** Signals re-subscription when ContentChildren change */
  private fieldNavReset$ = new Subject<void>();
  // ---- Panel content resize ----
  @ViewChild('panelContent') private panelContentRef?: ElementRef<HTMLElement>;
  private resizeObserver?: ResizeObserver;
  private resizeDebounceTimer?: ReturnType<typeof setTimeout>;
   * Persisted panel height for related-entity panels.
   * Returns undefined to let CSS default (400px) apply.
  get PanelContentHeight(): number | undefined {
    if (this.Variant !== 'related-entity') return undefined;
    const formRef = this.Form as { GetSectionPanelHeight?: (key: string) => number | undefined };
    return formRef?.GetSectionPanelHeight?.(this.SectionKey);
  /** Whether drag-to-reorder is allowed (from FormContext) */
  get ReorderAllowed(): boolean {
    return this.FormContext?.allowSectionReorder !== false;
  /** Whether the panel is expanded (delegates to form state) */
  get Expanded(): boolean {
    const formRef = this.Form as { IsSectionExpanded?: (key: string, defaultExpanded?: boolean) => boolean };
    return formRef?.IsSectionExpanded ? formRef.IsSectionExpanded(this.SectionKey, this.DefaultExpanded) : true;
    this.DisplayName = this.SectionName;
    this.UpdateFieldNames();
    this.SubscribeToFieldNavigateEvents();
    this.FieldComponents.changes.subscribe(() => {
    if (changes['SectionName']) {
    if (changes['SectionName'] || changes['FormContext']) {
      this.UpdateVisibilityAndHighlighting();
    if (changes['FormContext'] && this.FieldComponents) {
      this.FieldComponents.forEach(field => {
        field.FormContext = this.FormContext;
    this.SetupResizeObserver();
    this.fieldNavReset$.next();
    this.fieldNavReset$.complete();
    this.resizeObserver?.disconnect();
    if (this.resizeDebounceTimer) {
      clearTimeout(this.resizeDebounceTimer);
  // ---- Actions ----
  Toggle(): void {
    const formRef = this.Form as { SetSectionExpanded?: (key: string, expanded: boolean) => void };
    if (formRef?.SetSectionExpanded) {
      formRef.SetSectionExpanded(this.SectionKey, !this.Expanded);
   * Navigate to the parent entity when clicking the "Inherited from X" badge.
   * Derives the PrimaryKey from the Form's record when InheritedRecordPrimaryKey
   * is not explicitly provided (which is the common case in generated templates).
  OnInheritedBadgeClick(event: MouseEvent): void {
    if (!this.InheritedFromEntity) return;
    const primaryKey = this.InheritedRecordPrimaryKey
      ?? (this.Form as { record?: { PrimaryKey: CompositeKey } })?.record?.PrimaryKey
      ?? new CompositeKey([]);
      EntityName: this.InheritedFromEntity,
      PrimaryKey: primaryKey,
      Direction: 'parent'
  // ---- Drag and Drop ----
  @HostListener('dragover', ['$event'])
    if (!this.ReorderAllowed) return;
    if (event.dataTransfer?.types.includes('text/plain')) {
      this.IsDragOver = true;
  @HostListener('dragleave', ['$event'])
  OnDragLeave(event: DragEvent): void {
    this.IsDragOver = false;
  @HostListener('drop', ['$event'])
    const sourceSectionKey = event.dataTransfer?.getData('text/plain');
    if (sourceSectionKey && sourceSectionKey !== this.SectionKey) {
      this.PanelDrop.emit({
        SourceSectionKey: sourceSectionKey,
        TargetSectionKey: this.SectionKey
      this.ReorderSections(sourceSectionKey, this.SectionKey);
  OnDragStart(event: DragEvent): void {
    this.IsDragging = true;
    event.dataTransfer?.setData('text/plain', this.SectionKey);
    this.DragStarted.emit({ SectionKey: this.SectionKey, Event: event });
    this.IsDragging = false;
    this.DragEnded.emit();
  // ---- Private Methods ----
  private ReorderSections(sourceSectionKey: string, targetSectionKey: string): void {
    const formRef = this.Form as {
      getSectionOrder?: () => string[];
      setSectionOrder?: (order: string[]) => void;
    if (!formRef?.getSectionOrder || !formRef?.setSectionOrder) return;
    const currentOrder = formRef.getSectionOrder();
    const sourceIndex = currentOrder.indexOf(sourceSectionKey);
    const targetIndex = currentOrder.indexOf(targetSectionKey);
    if (sourceIndex === -1 || targetIndex === -1) return;
    const newOrder = [...currentOrder];
    newOrder.splice(sourceIndex, 1);
    newOrder.splice(targetIndex, 0, sourceSectionKey);
    formRef.setSectionOrder(newOrder);
  private UpdateFieldNames(): void {
    if (this.FieldComponents) {
      const names: string[] = [];
        if (field.DisplayName) {
          names.push(field.DisplayName.toLowerCase());
      this.FieldNames = names.join(' ');
   * Subscribes to Navigate events from all child form-field components
   * and relays them through this panel's Navigate output.
  private SubscribeToFieldNavigateEvents(): void {
    this.fieldNavReset$.next(); // tear down previous subscriptions
      field.Navigate.pipe(takeUntil(this.fieldNavReset$)).subscribe((event: FormNavigationEvent) => {
   * Sets up a ResizeObserver on the panel content div for related-entity panels.
   * When the user drags the CSS resize handle, we persist the new height.
  private SetupResizeObserver(): void {
    if (this.Variant !== 'related-entity' || !this.panelContentRef) return;
    const el = this.panelContentRef.nativeElement;
    // Run outside Angular zone to avoid triggering change detection on every resize frame
      this.resizeObserver = new ResizeObserver((entries) => {
        const entry = entries[0];
        if (!entry) return;
        const newHeight = Math.round(entry.contentRect.height);
        this.DebouncePersistHeight(newHeight);
      this.resizeObserver.observe(el);
   * Debounces height persistence so we don't write to DB on every resize frame.
  private DebouncePersistHeight(height: number): void {
    this.resizeDebounceTimer = setTimeout(() => {
      const formRef = this.Form as { SetSectionPanelHeight?: (key: string, height: number) => void };
      formRef?.SetSectionPanelHeight?.(this.SectionKey, height);
  private UpdateVisibilityAndHighlighting(): void {
    const searchTerm = (this.FormContext?.sectionFilter || '').toLowerCase().trim();
    const sectionMatches = this.SectionName.toLowerCase().includes(searchTerm);
    const fieldsMatch = this.FieldNames.includes(searchTerm);
    this.IsVisible = sectionMatches || fieldsMatch;
    if (this.IsVisible && sectionMatches) {
      const escaped = searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
      this.DisplayName = this.SectionName.replace(regex, '<mark class="mj-forms-search-highlight">$&</mark>');
