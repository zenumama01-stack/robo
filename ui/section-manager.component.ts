  OnChanges, SimpleChanges
import { PanelVariant } from '../types/form-types';
 * Info about a section, passed from the container to the Section Manager drawer.
export interface SectionManagerItem {
  SectionKey: string;
  SectionName: string;
  Variant: PanelVariant;
 * Slide-in drawer for managing section order.
 * Provides an accessible, keyboard-friendly alternative to drag-and-drop
 * for reordering form sections. Each section is displayed as a row with
 * up/down arrow buttons to change position.
 * <mj-section-manager
 *   [Sections]="sectionList"
 *   [SectionOrder]="currentOrder"
 *   [Visible]="showDrawer"
 *   (SectionOrderChange)="onOrderChange($event)"
 *   (Closed)="showDrawer = false">
 * </mj-section-manager>
  selector: 'mj-section-manager',
  templateUrl: './section-manager.component.html',
  styleUrls: ['./section-manager.component.css']
export class MjSectionManagerComponent implements OnChanges {
  /** All available sections (unordered) */
  @Input() Sections: SectionManagerItem[] = [];
  /** Current section order (array of section keys) */
  @Input() SectionOrder: string[] = [];
  /** Whether the drawer is visible */
  /** Emits the new section order when the user reorders */
  @Output() SectionOrderChange = new EventEmitter<string[]>();
  /** Emits when the drawer is closed */
  /** Emits when the user resets to default order */
  @Output() ResetRequested = new EventEmitter<void>();
  /** Ordered list of sections for display */
  OrderedSections: SectionManagerItem[] = [];
    if (changes['Sections'] || changes['SectionOrder']) {
      this.RebuildOrderedSections();
  OnClose(): void {
  OnOverlayClick(event: MouseEvent): void {
    // Only close if clicking the overlay itself, not the drawer
    if (event.target === event.currentTarget) {
  OnMoveUp(index: number): void {
    if (index <= 0) return;
    const newOrder = this.OrderedSections.map(s => s.SectionKey);
    [newOrder[index - 1], newOrder[index]] = [newOrder[index], newOrder[index - 1]];
    this.SectionOrderChange.emit(newOrder);
    this.ApplyOrder(newOrder);
  OnMoveDown(index: number): void {
    if (index >= this.OrderedSections.length - 1) return;
    [newOrder[index], newOrder[index + 1]] = [newOrder[index + 1], newOrder[index]];
  OnReset(): void {
    this.ResetRequested.emit();
  GetVariantIcon(variant: PanelVariant): string {
    switch (variant) {
      case 'related-entity': return 'fa-solid fa-link';
      case 'inherited': return 'fa-solid fa-arrow-up';
      default: return 'fa-solid fa-table-cells';
  GetVariantLabel(variant: PanelVariant): string {
      case 'related-entity': return 'Related';
      case 'inherited': return 'Inherited';
      default: return 'Fields';
  private RebuildOrderedSections(): void {
    if (!this.Sections || this.Sections.length === 0) {
      this.OrderedSections = [];
    const sectionMap = new Map<string, SectionManagerItem>();
    for (const s of this.Sections) {
      sectionMap.set(s.SectionKey, s);
    const ordered: SectionManagerItem[] = [];
    // Add sections in persisted order first
    if (this.SectionOrder && this.SectionOrder.length > 0) {
      for (const key of this.SectionOrder) {
        const section = sectionMap.get(key);
          ordered.push(section);
          sectionMap.delete(key);
    // Append any sections not in the persisted order (new sections)
    for (const section of sectionMap.values()) {
    this.OrderedSections = ordered;
  private ApplyOrder(newOrder: string[]): void {
    for (const s of this.OrderedSections) {
    this.OrderedSections = newOrder
      .map(key => sectionMap.get(key))
      .filter((s): s is SectionManagerItem => s !== undefined);
