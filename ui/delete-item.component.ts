import { DashboardItem } from '../../single-dashboard.component';
  selector: 'app-delete-item-dialog',
  templateUrl: './delete-item.component.html',
  styleUrls: ['./delete-item.component.css']
export class DeleteItemComponent implements OnInit {
  @Output() removeDashboardItem = new EventEmitter<any>();
  @Input() dashboardItem! : DashboardItem | null;
  public confirmDeleteItem(): void {
    if(this.dashboardItem){
      this.removeDashboardItem.emit(this.dashboardItem);
      console.log("item is null");
  public closeDialog(): void {
