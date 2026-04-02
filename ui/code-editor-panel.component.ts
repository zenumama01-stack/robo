import { Component, ChangeDetectorRef, ViewChild } from '@angular/core';
import { Extension } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import { unifiedMergeView, getChunks, goToNextChunk, goToPreviousChunk } from '@codemirror/merge';
import { ComponentStudioStateService, CodeSection } from '../../services/component-studio-state.service';
type CodeViewMode = 'current' | 'original' | 'diff';
  selector: 'mj-code-editor-panel',
    <div class="code-editor-panel">
      <!-- Tab bar: one tab per code section -->
      <div class="code-tab-bar">
        @for (section of State.CodeSections; track section.title; let i = $index) {
          <button class="code-tab"
                  [class.active]="ActiveTabIndex === i"
                  (click)="SelectTab(i)">
            @if (section.isDependency) {
              <i class="fa-solid fa-puzzle-piece dep-icon"></i>
            {{ section.title }}
            @if (HasChanges(section)) {
              <span class="modified-dot" title="Modified"></span>
      <!-- Toolbar: view mode toggle + actions -->
          <button class="mode-btn" [class.active]="ViewMode === 'current'" (click)="SetViewMode('current')">
            Current
          <button class="mode-btn" [class.active]="ViewMode === 'original'" (click)="SetViewMode('original')">
            Original
          <button class="mode-btn" [class.active]="ViewMode === 'diff'" (click)="SetViewMode('diff')">
            <i class="fa-solid fa-code-compare"></i> Diff
        @if (ViewMode === 'diff') {
          <div class="diff-nav">
            <span class="diff-count">{{ DiffChunkCount }} change{{ DiffChunkCount !== 1 ? 's' : '' }}</span>
            <button class="icon-btn" (click)="GoToPreviousChunk()" title="Previous change (↑)" [disabled]="DiffChunkCount === 0">
            <button class="icon-btn" (click)="GoToNextChunk()" title="Next change (↓)" [disabled]="DiffChunkCount === 0">
        <span class="toolbar-spacer"></span>
        <button class="icon-btn" (click)="CopyCode()" title="Copy code to clipboard">
        @if (State.IsEditingCode) {
            <button kendoButton [themeColor]="'primary'" (click)="ApplyChanges()">
              <i class="fa-solid fa-check"></i> Apply
            <button kendoButton [themeColor]="'base'" (click)="CancelChanges()">
      <!-- Editor body -->
      <div class="code-editor-body">
        @if (ActiveSection) {
          @switch (ViewMode) {
            @case ('current') {
              <div class="editor-pane">
                  [(ngModel)]="ActiveSection.code"
                  [language]="'javascript'"
                  [placeholder]="'Enter component code...'"
                  (ngModelChange)="OnCodeChanged()">
            @case ('original') {
                  [ngModel]="ActiveSection.originalCode"
                  [readonly]="true">
            @case ('diff') {
              <div class="editor-pane diff-pane">
                  #diffEditor
                  [ngModel]="ActiveSection.code"
                  [extensions]="DiffExtensions">
            <p>No code sections available. Select a component to view its code.</p>
    .code-editor-panel {
    /* ---- Tab bar ---- */
    .code-tab-bar {
    .code-tab {
    .code-tab:hover {
    .code-tab.active {
    .dep-icon {
      color: var(--mat-sys-tertiary, #7c3aed);
    .modified-dot {
      background: var(--mat-sys-error, #dc2626);
    /* ---- Toolbar ---- */
    .mode-btn {
    .mode-btn:not(:last-child) {
    .mode-btn:hover {
    .mode-btn.active {
    .mode-btn i {
    .icon-btn {
    .icon-btn:hover {
    .diff-nav {
    .diff-count {
    .icon-btn:disabled {
    /* ---- Editor body ---- */
    .code-editor-body {
    .editor-pane {
    .editor-pane mj-code-editor {
    /* ---- Diff-specific styles ---- */
    :host ::ng-deep .diff-pane .cm-mergeView .cm-changedLine {
      background: rgba(var(--mat-sys-primary-rgb, 99, 102, 241), 0.08);
    :host ::ng-deep .diff-pane .cm-mergeView .cm-deletedChunk {
      background: rgba(220, 38, 38, 0.08);
    /* ---- Empty state ---- */
export class CodeEditorPanelComponent {
  ActiveTabIndex = 0;
  ViewMode: CodeViewMode = 'current';
  DiffExtensions: Extension[] = [];
  DiffChunkCount = 0;
  @ViewChild('diffEditor') set DiffEditorRef(ref: { view?: EditorView } | undefined) {
    if (ref?.view) {
      this.diffView = ref.view;
      this.updateChunkCount();
  private diffView: EditorView | null = null;
  get ActiveSection(): CodeSection | null {
    const sections = this.State.CodeSections;
    if (sections.length === 0) return null;
    if (this.ActiveTabIndex >= sections.length) {
      this.ActiveTabIndex = 0;
    return sections[this.ActiveTabIndex];
  HasChanges(section: CodeSection): boolean {
    return section.code !== section.originalCode;
  SelectTab(index: number): void {
    this.ActiveTabIndex = index;
    this.ViewMode = 'current';
  SetViewMode(mode: CodeViewMode): void {
    if (mode === 'diff') {
      this.buildDiffExtensions();
  OnCodeChanged(): void {
    this.State.IsEditingCode = true;
  ApplyChanges(): void {
    this.State.ApplyCodeChanges();
  CancelChanges(): void {
    this.State.InitializeEditors();
  CopyCode(): void {
    const section = this.ActiveSection;
    if (!section) return;
    const text = this.ViewMode === 'original' ? section.originalCode : section.code;
    navigator.clipboard.writeText(text).catch(err => {
      console.error('Failed to copy code to clipboard:', err);
  GoToNextChunk(): void {
    if (this.diffView) {
      goToNextChunk(this.diffView);
  GoToPreviousChunk(): void {
      goToPreviousChunk(this.diffView);
  private updateChunkCount(): void {
    if (!this.diffView) {
      this.DiffChunkCount = 0;
    // Schedule after the merge view has initialized
      if (!this.diffView) return;
      const result = getChunks(this.diffView.state);
      this.DiffChunkCount = result ? result.chunks.length : 0;
  private buildDiffExtensions(): void {
    if (!section) {
      this.DiffExtensions = [];
    this.DiffExtensions = unifiedMergeView({
      original: section.originalCode,
      highlightChanges: true,
      gutter: true
