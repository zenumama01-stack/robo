  selector: 'app-crm-dashboard',
      <h2>CRM Dashboard</h2>
          <h3>42</h3>
          <p>Active Contacts</p>
          <h3>15</h3>
          <p>Companies</p>
          <h3>8</h3>
          <p>Open Deals</p>
          <h3>$2.4M</h3>
          <p>Pipeline Value</p>
          <div class="activity-item" *ngFor="let activity of recentActivity">
            <i [class]="activity.icon"></i>
      transition: box-shadow 0.15s;
export class CrmDashboardComponent {
  recentActivity = [
      icon: 'fa-solid fa-user-plus',
      title: 'New Contact Added',
      description: 'Sarah Johnson from Acme Corp',
      icon: 'fa-solid fa-handshake',
      title: 'Deal Won',
      description: 'Enterprise License - $50,000',
      time: '5 hours ago'
      icon: 'fa-solid fa-calendar-check',
      title: 'Meeting Scheduled',
      description: 'Product demo with TechStart Inc',
