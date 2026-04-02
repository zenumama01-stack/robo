import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { EvaluationPreferences, TestRunWithFeedback, getQualityColor } from '../../models/evaluation.types';
 * Display mode for the evaluation badge
export type EvaluationBadgeMode = 'compact' | 'expanded' | 'inline';
 * Evaluation badge component that displays test run evaluation data
 * based on user preferences (execution, human, auto).
 * <app-evaluation-badge
 *   [executionStatus]="'Completed'"
 *   [originalStatus]="'Passed'"
 *   [autoScore]="0.85"
 *   [humanRating]="8"
 *   [humanIsCorrect]="true"
 *   [hasHumanFeedback]="true"
 *   [preferences]="evalPrefs"
 *   [mode]="'compact'"
 * ></app-evaluation-badge>
  selector: 'app-evaluation-badge',
    <!-- Compact mode: inline icons and values -->
    @if (mode === 'compact') {
      <div class="eval-badge compact">
        <!-- Execution status -->
        @if (preferences?.showExecution) {
          <span class="eval-item exec" [class]="getExecClass()">
            <i [class]="getExecIcon()"></i>
        <!-- Human rating -->
        @if (preferences?.showHuman && hasHumanFeedback && humanRating != null) {
          <span class="eval-item human">
            <span class="value">{{ humanRating }}</span>
            @if (humanIsCorrect === true) {
              <i class="fa-solid fa-check correctness-icon"></i>
            @if (humanIsCorrect === false) {
              <i class="fa-solid fa-xmark correctness-icon incorrect"></i>
        <!-- Human pending indicator -->
        @if (preferences?.showHuman && !hasHumanFeedback) {
          <span class="eval-item human pending" title="Needs review">
            <i class="fa-solid fa-user-clock"></i>
        <!-- Auto score -->
        @if (preferences?.showAuto && autoScore != null) {
          <span class="eval-item auto" [class]="getAutoClass()">
            <span class="value">{{ formatAutoScore() }}</span>
    <!-- Expanded mode: stacked with labels -->
    @if (mode === 'expanded') {
      <div class="eval-badge expanded">
          <div class="eval-row">
            <span class="label">Status</span>
            <span class="value-wrap" [class]="getExecClass()">
              <span class="text">{{ getExecText() }}</span>
        @if (preferences?.showHuman) {
            <span class="label">Human</span>
            @if (hasHumanFeedback && humanRating != null) {
              <span class="value-wrap" [class]="getHumanClass()">
                <span class="rating-stars">{{ getRatingStars() }}</span>
                <span class="rating-num">{{ humanRating }}/10</span>
                  <span class="correctness"><i class="fa-solid fa-check"></i> Correct</span>
                  <span class="correctness incorrect"><i class="fa-solid fa-xmark"></i> Incorrect</span>
            @if (!hasHumanFeedback) {
              <span class="value-wrap pending">
                <span class="text">Needs review</span>
        @if (preferences?.showAuto) {
            <span class="label">Auto</span>
            @if (autoScore != null) {
              <span class="value-wrap" [class]="getAutoClass()">
                <div class="score-bar">
                  <div class="score-fill" [style.width.%]="(autoScore || 0) * 100"></div>
                <span class="score-text">{{ formatAutoScore() }}</span>
                @if (totalChecks) {
                  <span class="checks">{{ passedChecks }}/{{ totalChecks }} checks</span>
            @if (autoScore == null) {
              <span class="value-wrap na">
                <span class="text">Not evaluated</span>
    <!-- Inline mode: single primary value -->
    @if (mode === 'inline') {
      <span class="eval-badge inline" [class]="getQualityColorClass()">
        {{ getPrimaryValue() }}
    .eval-badge {
    /* Compact mode */
    .eval-badge.compact {
    .eval-item {
    .eval-item.exec {
    .eval-item.exec.success { color: #22c55e; }
    .eval-item.exec.error { color: #ef4444; }
    .eval-item.exec.timeout { color: #f97316; }
    .eval-item.exec.running { color: #3b82f6; }
    .eval-item.exec.pending { color: #9ca3af; }
    .eval-item.exec.skipped { color: #a1a1aa; }
    .eval-item.human {
    .eval-item.human.pending {
    .eval-item.human .value {
    .correctness-icon {
    .correctness-icon.incorrect {
    .eval-item.auto {
    .eval-item.auto.high {
    .eval-item.auto.low {
    .eval-item.auto .value {
    /* Expanded mode */
    .eval-badge.expanded {
    .eval-row {
    .eval-row .label {
    .eval-row .value-wrap {
    .eval-row .value-wrap.success { color: #22c55e; }
    .eval-row .value-wrap.error { color: #ef4444; }
    .eval-row .value-wrap.timeout { color: #f97316; }
    .eval-row .value-wrap.running { color: #3b82f6; }
    .eval-row .value-wrap.pending { color: #9ca3af; }
    .eval-row .value-wrap.na { color: #9ca3af; }
    .eval-row .text {
    .rating-num {
    .correctness {
    .correctness.incorrect {
      background: linear-gradient(90deg, #3b82f6 0%, #22c55e 100%);
    .checks {
    /* Inline mode */
    .eval-badge.inline {
    .eval-badge.inline.success {
    .eval-badge.inline.warning {
    .eval-badge.inline.danger {
    .eval-badge.inline.neutral {
export class EvaluationBadgeComponent {
  @Input() executionStatus: string = 'Completed';
  @Input() originalStatus: string = 'Passed';
  @Input() autoScore: number | null = null;
  @Input() passedChecks: number | null = null;
  @Input() failedChecks: number | null = null;
  @Input() totalChecks: number | null = null;
  @Input() humanRating: number | null = null;
  @Input() humanIsCorrect: boolean | null = null;
  @Input() hasHumanFeedback: boolean = false;
  @Input() preferences: EvaluationPreferences | null = null;
  @Input() mode: EvaluationBadgeMode = 'compact';
  getExecIcon(): string {
    switch (this.executionStatus) {
      case 'Completed':
      case 'Passed':
        return 'fa-solid fa-circle-check';
        return 'fa-solid fa-circle-xmark';
        return 'fa-solid fa-triangle-exclamation';
      case 'Timeout':
      case 'Running':
        return 'fa-solid fa-circle-dot';
      case 'Skipped':
        return 'fa-solid fa-forward';
        return 'fa-solid fa-circle-question';
  getExecClass(): string {
        return 'success';
        return 'error';
        return 'timeout';
        return 'running';
        return 'skipped';
  getExecText(): string {
    return this.originalStatus || this.executionStatus;
  getHumanClass(): string {
    if (this.humanRating == null) return '';
    if (this.humanRating >= 8) return 'success';
    if (this.humanRating >= 5) return 'warning';
    return 'danger';
  getAutoClass(): string {
    if (this.autoScore == null) return '';
    if (this.autoScore >= 0.8) return 'high';
    if (this.autoScore >= 0.5) return '';
  formatAutoScore(): string {
    if (this.autoScore == null) return '—';
    return `${Math.round(this.autoScore * 100)}%`;
  getRatingStars(): string {
    const filled = Math.round(this.humanRating / 2);
    const empty = 5 - filled;
    return '★'.repeat(filled) + '☆'.repeat(empty);
  getQualityColorClass(): string {
    if (!this.preferences) return 'neutral';
    const run: TestRunWithFeedback = {
      id: '',
      testId: '',
      testName: '',
      executionStatus: this.executionStatus as TestRunWithFeedback['executionStatus'],
      originalStatus: this.originalStatus,
      autoScore: this.autoScore,
      passedChecks: this.passedChecks,
      failedChecks: this.failedChecks,
      totalChecks: this.totalChecks,
      humanRating: this.humanRating,
      humanIsCorrect: this.humanIsCorrect,
      hasHumanFeedback: this.hasHumanFeedback,
    return getQualityColor(run, this.preferences);
  getPrimaryValue(): string {
    // Priority: Human > Auto > Execution
    if (this.preferences?.showHuman && this.hasHumanFeedback && this.humanRating != null) {
      return `${this.humanRating}/10`;
    if (this.preferences?.showAuto && this.autoScore != null) {
      return this.formatAutoScore();
    if (this.preferences?.showExecution) {
      return this.originalStatus;
    return '—';
