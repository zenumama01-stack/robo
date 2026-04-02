import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { EvaluationPreferencesService } from '../../services/evaluation-preferences.service';
import { EvaluationPreferences } from '../../models/evaluation.types';
 * Toggle component for selecting which evaluation metrics to display.
 * Automatically saves preferences to user settings.
 * <app-evaluation-mode-toggle></app-evaluation-mode-toggle>
  selector: 'app-evaluation-mode-toggle',
    <div class="eval-toggle">
      <span class="toggle-label">Show:</span>
      <div class="toggle-options">
          [class.active]="preferences.showExecution"
          (click)="toggle('showExecution')"
          title="Execution status (Passed, Failed, Error, Timeout, etc.)">
          [class.active]="preferences.showHuman"
          (click)="toggle('showHuman')"
          title="Human evaluation ratings (1-10 scale)">
          <span>Human</span>
          [class.active]="preferences.showAuto"
          (click)="toggle('showAuto')"
          title="Automated evaluation scores (0-100%)">
          <span>Auto</span>
      @if (showHint) {
        <span class="toggle-hint">
          At least one must be enabled
    .eval-toggle {
    .toggle-options {
      box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
    .toggle-btn.active:hover {
    .toggle-btn i {
    .toggle-hint {
    .toggle-hint i {
      .toggle-btn span {
export class EvaluationModeToggleComponent implements OnInit, OnDestroy {
  preferences: EvaluationPreferences = {
  showHint = false;
    private prefsService: EvaluationPreferencesService,
    this.prefsService.preferences$
        this.preferences = prefs;
  async toggle(key: keyof EvaluationPreferences): Promise<void> {
    const newValue = !this.preferences[key];
    // Check if this would disable all
    const updated = { ...this.preferences, [key]: newValue };
    if (!updated.showExecution && !updated.showHuman && !updated.showAuto) {
      // Show hint briefly
      this.showHint = true;
        this.showHint = false;
    await this.prefsService.toggle(key);
