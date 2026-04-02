  selector: 'app-crm-opportunities',
    <div class="opportunities-container">
      <h2>Opportunities</h2>
      <div class="pipeline-stages">
        <div class="stage" *ngFor="let stage of stages">
          <h3>{{ stage.name }}</h3>
          <p class="stage-value">{{ stage.value }}</p>
          <div class="opportunities-list">
              class="opportunity-card"
              *ngFor="let opp of getOpportunitiesForStage(stage.name)"
              (click)="OpenOpportunityInNewTab(opp)"
              <div class="opp-header">
                <strong>{{ opp.name }}</strong>
                <button class="icon-btn" (click)="OpenOpportunityInNewTab(opp); $event.stopPropagation()">
              <p class="company">{{ opp.company }}</p>
              <div class="opp-footer">
                <span class="value">{{ opp.value }}</span>
                <span class="probability">{{ opp.probability }}%</span>
    .opportunities-container {
    .pipeline-stages {
    .stage {
      .stage-value {
    .opportunities-list {
    .opportunity-card {
    .opp-header {
    .opp-footer {
      .value {
      .probability {
export class OpportunitiesComponent {
  stages = [
    { name: 'Prospecting', value: '$320K' },
    { name: 'Qualification', value: '$580K' },
    { name: 'Proposal', value: '$420K' },
    { name: 'Negotiation', value: '$280K' }
  opportunities = [
      name: 'Enterprise License',
      value: '$50K',
      probability: 75,
      stage: 'Negotiation'
      name: 'Cloud Migration',
      value: '$120K',
      probability: 60,
      stage: 'Proposal'
      name: 'Consulting Package',
      value: '$85K',
      probability: 80,
      name: 'Platform Integration',
      value: '$45K',
      probability: 45,
      stage: 'Qualification'
      id: '5',
      name: 'Training Program',
      value: '$30K',
      probability: 65,
      id: '6',
      name: 'API Development',
      value: '$95K',
      probability: 40,
  getOpportunitiesForStage(stageName: string) {
    return this.opportunities.filter(opp => opp.stage === stageName);
  OpenOpportunityInNewTab(opportunity: any): void {
      `Deal: ${opportunity.name}`,
      `/crm/opportunity/${opportunity.id}`,
      { opportunityId: opportunity.id, opportunity }
