import { SharedService } from "./shared.service";
import { BaseNavigationComponent } from "./base-navigation-component";
export abstract class BaseResourceComponent extends BaseNavigationComponent {
    private _data: ResourceData = new ResourceData();
    public get Data(): ResourceData {
        return this._data;
    public set Data(value: ResourceData) {
        this._data = value;
    private _loadCompleteEvent: any = null;
    public get LoadCompleteEvent(): any {
        return this._loadCompleteEvent
    public set LoadCompleteEvent(value: any) {
        this._loadCompleteEvent = value;
    private _loadStartedEvent: any = null;
    public get LoadStartedEvent(): any {
        return this._loadStartedEvent
    public set LoadStartedEvent(value: any) {
        this._loadStartedEvent = value;
    private _resourceRecordSavedEvent: any = null;
    public get ResourceRecordSavedEvent(): any {
        return this._resourceRecordSavedEvent
    public set ResourceRecordSavedEvent(value: any) {
        this._resourceRecordSavedEvent = value;
    protected NotifyLoadComplete() {
        if (this._loadCompleteEvent) {
            this._loadCompleteEvent();
    protected NotifyLoadStarted() {
        if (this._loadStartedEvent) {
            this._loadStartedEvent();
    protected ResourceRecordSaved(resourceRecordEntity: BaseEntity) {
        this.Data.ResourceRecordID = resourceRecordEntity.PrimaryKey.ToString();
        if (this._resourceRecordSavedEvent) {
            this._resourceRecordSavedEvent(resourceRecordEntity);
    abstract GetResourceDisplayName(data: ResourceData): Promise<string>
    abstract GetResourceIconClass(data: ResourceData): Promise<string>
