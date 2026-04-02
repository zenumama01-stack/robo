@RegisterClass(BaseResourceComponent, 'QueryResource')
    selector: 'mj-query-resource',
    template: `<mj-single-query [queryId]="Data.ResourceRecordID" (loadComplete)="NotifyLoadComplete()" (loadStarted)="NotifyLoadStarted()"></mj-single-query>`
export class QueryResource extends BaseResourceComponent implements OnInit {
        const name = await md.GetEntityRecordName('Queries', compositeKey);
        return `${name ? name : 'Query ID: ' + data.ResourceRecordID}`;
