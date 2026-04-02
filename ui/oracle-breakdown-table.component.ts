import { Component, Input } from '@angular/core';
export interface OracleResult {
  status: 'Passed' | 'Failed' | 'Skipped' | 'Error';
  selector: 'app-oracle-breakdown-table',
    <div class="oracle-breakdown">
          <i class="fa-solid fa-balance-scale"></i>
          Oracle Results
        @if (results && results.length > 0) {
          <div class="aggregate-score">
            <span class="label">Aggregate:</span>
            <app-score-indicator [score]="getAggregateScore()" [showBar]="false"></app-score-indicator>
          <div class="oracle-table">
              <div class="header-cell">Oracle</div>
              <div class="header-cell">Score</div>
            @for (oracle of results; track oracle.name) {
              <div class="table-row" [class.has-error]="oracle.errorMessage">
                  <div class="oracle-name">
                    @if (oracle.status === 'Passed') {
                      <i class="fa-solid fa-check-circle oracle-icon"></i>
                    @if (oracle.status === 'Failed') {
                      <i class="fa-solid fa-times-circle oracle-icon"></i>
                    @if (oracle.status === 'Error') {
                      <i class="fa-solid fa-exclamation-triangle oracle-icon"></i>
                    @if (oracle.status === 'Skipped') {
                      <i class="fa-solid fa-forward oracle-icon"></i>
                    <span>{{ oracle.name }}</span>
                  <app-test-status-badge [status]="oracle.status" [showIcon]="false"></app-test-status-badge>
                  <app-score-indicator [score]="oracle.score" [showBar]="true"></app-score-indicator>
                  <app-cost-display [cost]="oracle.cost" [showIcon]="true" [decimals]="6"></app-cost-display>
                  {{ formatDuration(oracle.duration) }}
              @if (oracle.errorMessage) {
                <div class="error-row">
                    {{ oracle.errorMessage }}
          <div class="breakdown-summary">
            <div class="summary-item">
              <span class="summary-label">Total Cost:</span>
              <app-cost-display [cost]="getTotalCost()" [showIcon]="true"></app-cost-display>
              <span class="summary-label">Total Duration:</span>
              <span class="summary-value">{{ formatDuration(getTotalDuration()) }}</span>
              <span class="summary-label">Oracles Run:</span>
              <span class="summary-value">{{ results.length }}</span>
            <p>No oracle results available</p>
    .oracle-breakdown {
    .aggregate-score {
    .aggregate-score .label {
    .oracle-table {
      grid-template-columns: 2fr 120px 150px 120px 100px;
    .table-row.has-error {
      border-left: 3px solid #f44336;
    .oracle-name {
    .oracle-icon {
    .oracle-icon.fa-check-circle {
    .oracle-icon.fa-times-circle {
    .oracle-icon.fa-exclamation-triangle {
    .oracle-icon.fa-forward {
    .error-row {
      border-left: 3px solid #ff9800;
    .breakdown-summary {
export class OracleBreakdownTableComponent {
  @Input() results: OracleResult[] = [];
  getAggregateScore(): number {
    if (!this.results || this.results.length === 0) return 0;
    const total = this.results.reduce((sum, r) => sum + r.score, 0);
    return total / this.results.length;
    return this.results.reduce((sum, r) => sum + r.cost, 0);
  getTotalDuration(): number {
    return this.results.reduce((sum, r) => sum + r.duration, 0);
