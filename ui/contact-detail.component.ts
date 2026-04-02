  selector: 'app-contact-detail',
    <div class="contact-detail-container" *ngIf="contact">
      <div class="contact-header">
        <div class="contact-avatar-large">{{ contact.initials }}</div>
        <div class="contact-info">
          <h2>{{ contact.name }}</h2>
          <p class="company">{{ contact.company }}</p>
      <div class="contact-sections">
          <h3>Contact Information</h3>
              <label>Email</label>
              <span>{{ contact.email }}</span>
              <label>Phone</label>
              <span>{{ contact.phone }}</span>
              <label>Company</label>
              <span>{{ contact.company }}</span>
              <label>Title</label>
              <span>{{ contact.title }}</span>
          <h3>Recent Activity</h3>
            <div class="timeline-item" *ngFor="let activity of contact.activities">
              <div class="timeline-dot"></div>
                <strong>{{ activity.title }}</strong>
                <p>{{ activity.description }}</p>
                <span class="time">{{ activity.time }}</span>
    .contact-detail-container {
    .contact-header {
    .contact-avatar-large {
    .contact-info {
      .company {
    .contact-sections {
      span {
      &:not(:last-child)::before {
        left: 7px;
        bottom: -20px;
      .time {
export class ContactDetailComponent implements OnInit {
  contactId: string | null = null;
  contact: any = null;
  // Mock contact database
  private contacts: any = {
    '1': {
      name: 'Sarah Johnson',
      initials: 'SJ',
      company: 'Acme Corp',
      title: 'VP of Sales',
      email: 'sarah.j@acme.com',
      phone: '(555) 123-4567',
      activities: [
          title: 'Email Sent',
          description: 'Product demo follow-up',
          time: '2 hours ago'
          title: 'Meeting Completed',
          description: 'Initial discovery call',
          time: '3 days ago'
          title: 'Contact Created',
          description: 'Added to CRM',
          time: '1 week ago'
    '2': {
      name: 'Michael Chen',
      initials: 'MC',
      company: 'TechStart Inc',
      title: 'CTO',
      email: 'mchen@techstart.io',
      phone: '(555) 234-5678',
          title: 'Demo Scheduled',
          description: 'Technical evaluation meeting',
          time: '1 day ago'
    '3': {
      name: 'Emily Rodriguez',
      initials: 'ER',
      company: 'Global Solutions',
      title: 'Director of Operations',
      email: 'emily.r@global.com',
      phone: '(555) 345-6789',
      activities: []
    '4': {
      name: 'David Kim',
      initials: 'DK',
      company: 'Innovation Labs',
      title: 'Product Manager',
      email: 'dkim@innovationlabs.com',
      phone: '(555) 456-7890',
  constructor(private route: ActivatedRoute) {}
    this.route.params.subscribe(params => {
      this.contactId = params['id'];
      if (this.contactId) {
        this.contact = this.contacts[this.contactId];
