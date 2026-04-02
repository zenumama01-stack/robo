import { CrmApp } from '../crm.app';
  selector: 'app-crm-companies',
    <div class="companies-container">
      <h2>Companies</h2>
      <div class="companies-grid">
          class="company-card"
          *ngFor="let company of companies"
          (click)="OpenCompanyInNewTab(company)"
          <div class="company-header">
            <div class="company-logo">{{ company.initials }}</div>
            <button class="icon-btn" (click)="OpenCompanyInNewTab(company); $event.stopPropagation()">
          <h3>{{ company.name }}</h3>
          <p class="industry">{{ company.industry }}</p>
              <span>{{ company.contacts }} contacts</span>
              <span>{{ company.revenue }}</span>
    .companies-container {
    .companies-grid {
    .company-card {
    .company-header {
    .company-logo {
    .industry {
export class CompaniesComponent {
  companies = [
      name: 'Acme Corp',
      initials: 'AC',
      industry: 'Manufacturing',
      contacts: 12,
      revenue: '$5M'
      id: '2',
      name: 'TechStart Inc',
      initials: 'TS',
      industry: 'Technology',
      contacts: 8,
      revenue: '$2.5M'
      id: '3',
      name: 'Global Solutions',
      initials: 'GS',
      industry: 'Consulting',
      contacts: 15,
      revenue: '$8M'
      id: '4',
      name: 'Innovation Labs',
      initials: 'IL',
      industry: 'Research',
      contacts: 6,
      revenue: '$1.2M'
  constructor(private crmApp: CrmApp) {}
  OpenCompanyInNewTab(company: any): void {
    this.crmApp.RequestNewTab(
      `Company: ${company.name}`,
      `/crm/company/${company.id}`,
      { companyId: company.id, company }
