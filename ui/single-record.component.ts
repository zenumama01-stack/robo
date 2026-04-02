import { AfterViewInit, Component, ComponentRef, EventEmitter, inject, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router'
import { Metadata, KeyValuePair, CompositeKey, BaseEntity, BaseEntityEvent, FieldValueCollection, EntityFieldTSType } from '@memberjunction/core';
import { BaseFormComponent, FormNavigationEvent, FormNotificationEvent } from '@memberjunction/ng-base-forms';
import { NavigationService, RecentAccessService, SharedService } from '@memberjunction/ng-shared';
  selector: 'mj-single-record',
  templateUrl: './single-record.component.html',
  styleUrls: ['./single-record.component.css']
export class SingleRecordComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild(Container, {static: true}) formContainer!: Container;
  @Input() public PrimaryKey: CompositeKey = new CompositeKey();
  @Input() public entityName: string | null = '';
  @Input() public newRecordValues: string | Record<string, unknown> | null = '';
  @Output() public recordSaved: EventEmitter<BaseEntity> = new EventEmitter<BaseEntity>();
  constructor (private route: ActivatedRoute) {
  public appDescription: string = ''
  public useGenericForm: boolean = false;
  // Track dynamically created components and entities for cleanup
  private _formComponentRef: ComponentRef<BaseFormComponent> | null = null;
  private _currentRecord: BaseEntity | null = null;
  private _eventHandlerSubscription: Subscription | null = null;
  private _formEventSubscriptions: Subscription[] = [];
    this.LoadForm(this.PrimaryKey, <string>this.entityName)
  public async LoadForm(primaryKey: CompositeKey, entityName: string) {
    // Perform any necessary actions with the ViewID, such as fetching data
    if (!entityName || entityName.trim().length === 0) {
      return; // not ready to load
    this.entityName = entityName;
    if (primaryKey.HasValue) {
      // we have an existing record to load up
      this.PrimaryKey = primaryKey;
      // new record, no existing primary key
      this.PrimaryKey = new CompositeKey();
    const formReg = MJGlobal.Instance.ClassFactory.GetRegistration(BaseFormComponent, entityName);
    if (formReg) {
      const record = await md.GetEntityObject<BaseEntity>(entityName);
          await record.InnerLoad(primaryKey);
          // Log access to existing record (fire-and-forget, don't await)
          this.recentAccessService.logAccess(entityName, primaryKey, 'record');
          record.NewRecord();
          this.SetNewRecordValues(record);
        // CRITICAL: Track the event handler subscription for cleanup
        this._eventHandlerSubscription = record.RegisterEventHandler((eventType: BaseEntityEvent) => {
          if (eventType.type === 'save')
            this.recordSaved.emit(record);
        const viewContainerRef = this.formContainer.viewContainerRef;
        const componentRef = viewContainerRef.createComponent<typeof formReg.SubClass>(formReg.SubClass);
        // Track component and record for cleanup
        this._formComponentRef = componentRef;
        this._currentRecord = record;
        componentRef.instance.record = record
        componentRef.instance.userPermissions = permissions
        componentRef.instance.EditMode = !primaryKey.HasValue; // for new records go direct into edit mode
        // Subscribe to form @Output events and map them to Explorer services
        this.subscribeToFormEvents(componentRef.instance);
        this.useGenericForm = false;
        throw new Error(`Unable to load entity ${entityName} with primary key values: ${primaryKey.ToString()}`);
  protected SetNewRecordValues(record: BaseEntity) {
    if (!this.newRecordValues) {
    // Handle both object and string (URL segment) formats
    if (typeof this.newRecordValues === 'string') {
      if (this.newRecordValues.length === 0) {
      // we have a URL segment string format: "field1|value1||field2|value2"
      const fv = new FieldValueCollection();
      fv.SimpleLoadFromURLSegment(this.newRecordValues);
      // now apply the values to the record
      fv.KeyValuePairs.filter(kvp => kvp.Value !== null && kvp.Value !== undefined).forEach(kvp => {
        const f = record.Fields.find(f => f.Name.trim().toLowerCase() === kvp.FieldName.trim().toLowerCase());
        if (f) {
          // make sure we set the value to the right type based on the f.TSType property
          switch (f.EntityFieldInfo.TSType) {
            case EntityFieldTSType.String:
              record.Set(kvp.FieldName, kvp.Value);
            case EntityFieldTSType.Number:
              record.Set(kvp.FieldName, parseFloat(kvp.Value));
            case EntityFieldTSType.Boolean:
              if (kvp.Value === 'false' || kvp.Value === '0' || kvp.Value.toString().trim().length === 0 )
                record.Set(kvp.FieldName, false);
                record.Set(kvp.FieldName, true);
            case EntityFieldTSType.Date:
              record.Set(kvp.FieldName, new Date(kvp.Value));
      // we have a plain object format: { field1: value1, field2: value2 }
      const recordValues = this.newRecordValues as Record<string, unknown>;
      Object.keys(recordValues)
        .filter(key => recordValues[key] !== null && recordValues[key] !== undefined)
        .forEach(key => {
          const f = record.Fields.find(f => f.Name.trim().toLowerCase() === key.trim().toLowerCase());
            const value = recordValues[key];
            // Set the value with proper type conversion
                record.Set(key, value?.toString() || '');
                record.Set(key, typeof value === 'number' ? value : parseFloat(value?.toString() || '0'));
                if (typeof value === 'boolean') {
                  record.Set(key, value);
                else if (typeof value === 'string') {
                  record.Set(key, value !== 'false' && value !== '0' && value.trim().length > 0);
                  record.Set(key, !!value);
                record.Set(key, value instanceof Date ? value : new Date(value?.toString() || ''));
   * Subscribe to BaseFormComponent @Output events and map them to Explorer services.
  private subscribeToFormEvents(form: BaseFormComponent): void {
    this.cleanupFormSubscriptions();
    this._formEventSubscriptions.push(
      form.Navigate.subscribe((event: FormNavigationEvent) => this.handleNavigation(event)),
      form.Notification.subscribe((event: FormNotificationEvent) => {
        this.sharedService.CreateSimpleNotification(event.Message, event.Type, event.Duration);
  private handleNavigation(event: FormNavigationEvent): void {
    switch (event.Kind) {
        this.navigationService.OpenEntityRecord(event.EntityName, event.PrimaryKey, { forceNewTab: event.OpenInNewTab });
      case 'new-record':
        this.navigationService.OpenNewEntityRecord(event.EntityName, { newRecordValues: event.DefaultValues });
      case 'entity-hierarchy':
        this.navigationService.OpenEntityRecord(event.EntityName, event.PrimaryKey);
      case 'external-link':
        window.open(event.Url, '_blank');
        window.open(`mailto:${event.EmailAddress}`, '_self');
  private cleanupFormSubscriptions(): void {
    for (const sub of this._formEventSubscriptions) {
      sub.unsubscribe();
    this._formEventSubscriptions = [];
    // CRITICAL: Clean up form event subscriptions first
    // CRITICAL: Clean up dynamically created form component to prevent zombie components
    if (this._formComponentRef) {
      this._formComponentRef.destroy();
      this._formComponentRef = null;
    // CRITICAL: Unsubscribe from event handler to prevent memory leaks
    if (this._eventHandlerSubscription) {
      this._eventHandlerSubscription.unsubscribe();
      this._eventHandlerSubscription = null;
    // Clean up record reference
    if (this._currentRecord) {
      this._currentRecord = null;
    if (this.formContainer?.viewContainerRef) {
      this.formContainer.viewContainerRef.clear();
