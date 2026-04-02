  selector: 'mj-editor-tabs',
    <div class="editor-tabs-container">
      <div class="tab-bar">
        <button class="tab-pill" [class.active]="state.ActiveTab === 0" (click)="SelectTab(0)">
          <i class="fa-solid fa-file-code"></i> Spec
        <button class="tab-pill" [class.active]="state.ActiveTab === 1" (click)="SelectTab(1)">
          <i class="fa-solid fa-code"></i> Code
        <button class="tab-pill" [class.active]="state.ActiveTab === 2" (click)="SelectTab(2)">
          <i class="fa-solid fa-clipboard-list"></i> Requirements
        <button class="tab-pill" [class.active]="state.ActiveTab === 3" (click)="SelectTab(3)">
          <i class="fa-solid fa-drafting-compass"></i> Design
        <button class="tab-pill" [class.active]="state.ActiveTab === 4" (click)="SelectTab(4)">
          <i class="fa-solid fa-database"></i> Data
        <span class="tab-spacer"></span>
        @switch (state.ActiveTab) {
          @case (0) {
            <mj-spec-editor></mj-spec-editor>
          @case (1) {
            <mj-code-editor-panel></mj-code-editor-panel>
          @case (2) {
            <mj-requirements-editor [Field]="'functionalRequirements'" [Title]="'Functional Requirements'"></mj-requirements-editor>
          @case (3) {
            <mj-requirements-editor [Field]="'technicalDesign'" [Title]="'Technical Design'"></mj-requirements-editor>
          @case (4) {
            <mj-data-requirements-editor></mj-data-requirements-editor>
    .editor-tabs-container {
    .tab-bar {
      height: 38px;
    .tab-pill {
    .tab-pill:hover {
    .tab-pill.active {
    .tab-pill i {
    .tab-spacer {
    .tab-content > * {
export class EditorTabsComponent implements OnInit, OnDestroy {
    this.stateChangedSub = this.state.StateChanged.subscribe(() => {
    this.state.ActiveTab = index;
