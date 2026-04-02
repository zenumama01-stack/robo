  selector: 'app-crm-contacts',
    <div class="contacts-container">
      <div class="contacts-header">
        <h2>Contacts</h2>
          Add Contact
      <div class="contacts-table">
          <div class="col-name">Name</div>
          <div class="col-company">Company</div>
          <div class="col-email">Email</div>
          <div class="col-phone">Phone</div>
          <div class="col-actions">Actions</div>
          *ngFor="let contact of contacts"
          (click)="OpenContactInNewTab(contact)"
          <div class="col-name">
            <div class="contact-avatar">{{ contact.initials }}</div>
            <strong>{{ contact.name }}</strong>
          <div class="col-company">{{ contact.company }}</div>
          <div class="col-email">{{ contact.email }}</div>
          <div class="col-phone">{{ contact.phone }}</div>
          <div class="col-actions">
            <button class="icon-btn" (click)="OpenContactInNewTab(contact); $event.stopPropagation()">
    .contacts-container {
    .contacts-header {
    .contacts-table {
      grid-template-columns: 2fr 1.5fr 2fr 1.5fr 100px;
    .contact-avatar {
    .col-email,
    .col-phone {
export class ContactsComponent {
  contacts = [
      phone: '(555) 123-4567'
      phone: '(555) 234-5678'
      phone: '(555) 345-6789'
      phone: '(555) 456-7890'
  OpenContactInNewTab(contact: any): void {
      `Contact: ${contact.name}`,
      `/crm/contact/${contact.id}`,
      { contactId: contact.id, contact }
