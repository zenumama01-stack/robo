import { Component,  EventEmitter,  Input, OnDestroy, OnInit, Output } from '@angular/core';
import { BaseEntity, EntityFieldInfo, EntityInfo, LogError, Metadata, RunView } from '@memberjunction/core';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
  selector: 'mj-find-record',
  templateUrl: './find-record.component.html',
  styleUrls: ['./find-record.component.css']
export class FindRecordComponent implements OnInit, OnDestroy {
  @Input() public DisplayFields: EntityFieldInfo[] = []; // Fields to display in the grid
   * Optional, the number of milliseconds to wait after the last keypress before triggering a search. Default is 300ms
  @Input() public SearchDebounceTime: number = 300; // Debounce time for search
  public searchTerm: string = ''; // User search term
  public records: any[] = []; // Store search results
  public loading = false; // Loading state for search
  public searchHasRun: boolean = false; // has a search been run
  private entityInfo: EntityInfo | undefined; // Entity metadata
  private searchSubject = new Subject<string>(); // Subject to emit search term changes
  private searchSubscription: any;
    // Fetch the entity metadata based on EntityName
    this.entityInfo = md.EntityByName(this.EntityName);
      LogError(`Entity ${this.EntityName} not found`);
    // Set display fields (you can adjust the logic based on your entity structure)
    if (this.DisplayFields.length === 0) {
      this.DisplayFields = this.entityInfo.Fields.filter(
        (field: EntityFieldInfo) => field.DefaultInView || field.IsPrimaryKey || field.IsNameField || field.IncludeInUserSearchAPI
    // Subscribe to the searchSubject with debounce
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(this.SearchDebounceTime), // Delay search execution by 300ms
      distinctUntilChanged(), // Only proceed if the search term has changed
      switchMap(term => {
        return this.doSearch(term);
      next: (results: any[]) => {
        this.records = results;
        this.searchHasRun = true;
        LogError(error.message);
    // Cleanup the subscription
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
  onFind() {
    this.searchSubject.next(this.searchTerm); // Trigger the debounced search
  onSearchTermChange(term: string) {
    this.searchTerm = term;
    this.searchSubject.next(term); // Emit the new search term
  public onSelectionChange(event: any) {
    // Emit the selected record
    this.OnRecordSelected.emit(event.selectedRows[0].dataItem);
  // Stub function for simulating a database search (replace with actual search logic)
  protected async doSearch(searchTerm: string): Promise<BaseEntity[]> {
    else  {
      const errorMessage = `Error searching for ${this.EntityName}: ${result.ErrorMessage}`;
      LogError(errorMessage);
