 * Scheduling Dashboard Resource - displays the overview dashboard with KPIs, health, and alerts
@RegisterClass(BaseResourceComponent, 'SchedulingDashboardResource')
  selector: 'mj-scheduling-dashboard-resource',
      <app-scheduling-overview></app-scheduling-overview>
export class SchedulingOverviewResourceComponent extends BaseResourceComponent implements OnInit {
    return 'Dashboard';
