@RegisterClass(BaseResourceComponent, 'RecordResource')
    selector: 'mj-record-resource',
    template: `<mj-single-record [PrimaryKey]="this.PrimaryKey" [entityName]="Data.Configuration.Entity" [newRecordValues]="Data.Configuration.NewRecordValues" (loadComplete)="NotifyLoadComplete()" (recordSaved)="ResourceRecordSaved($event)" ></mj-single-record>`
export class EntityRecordResource extends BaseResourceComponent {
    public get PrimaryKey(): CompositeKey {
        return EntityRecordResource.GetPrimaryKey(this.Data);
    public static GetPrimaryKey(data: ResourceData): CompositeKey {
        const e = md.Entities.find(e => e.Name.trim().toLowerCase() === data.Configuration.Entity.trim().toLowerCase());
        if (!e){
            throw new Error(`Entity ${data.Configuration.Entity} not found in metadata`);
        compositeKey.LoadFromURLSegment(e, data.ResourceRecordID);
        if (!data.Configuration.Entity) {
        const e = md.EntityByName(data.Configuration.Entity);
        if (!e) {
        const pk: CompositeKey = EntityRecordResource.GetPrimaryKey(data);
        if (pk.HasValue) {
            const name = await md.GetEntityRecordName(data.Configuration.Entity, pk);
            return name ? name : e.DisplayNameOrName;
            return `New ${e.DisplayNameOrName} Record`;
        if (!data.Configuration.Entity){
            return ''
            if (e)
                return e?.Icon;
