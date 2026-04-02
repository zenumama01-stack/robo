  selector: 'mj-action-list-item',
  templateUrl: './action-list-item.component.html',
  styleUrls: ['./action-list-item.component.css'],
export class ActionListItemComponent {
  @Input() IsCompact = false;
  public onRowClick(): void {
    return this.Categories.get(this.Action.CategoryID)?.Name || 'Unknown';
  public formatDate(date: Date | null | undefined): string {
