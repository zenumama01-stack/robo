export class CrmApp implements IApp {
  Id = 'crm';
  Name = 'CRM';
  Icon = 'fa-solid fa-briefcase';
  Route = '/crm';
  Color = '#2e7d32'; // Green - growth, relationships
        Label: 'Dashboard',
        Route: '/crm/dashboard',
        Icon: 'fa-solid fa-chart-line'
        Label: 'Contacts',
        Route: '/crm/contacts',
        Icon: 'fa-solid fa-user',
        Badge: 42
        Label: 'Companies',
        Route: '/crm/companies',
        Icon: 'fa-solid fa-building',
        Badge: 15
        Label: 'Opportunities',
        Route: '/crm/opportunities',
        Icon: 'fa-solid fa-handshake',
        Badge: 8
    console.log('CRM app searching for:', query);
    // Custom search within CRM data
    console.log('CRM handling route:', segments);
