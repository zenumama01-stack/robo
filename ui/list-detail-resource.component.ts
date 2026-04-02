@RegisterClass(BaseResourceComponent, 'ListDetailResource')
    selector: 'mj-list-detail-resource',
    template: `<mj-list-detail [ListID]="Data.ResourceRecordID"/>`
export class ListDetailResource extends BaseResourceComponent {
            let compositeKey: CompositeKey = new CompositeKey([{FieldName: "ID", Value: data.ResourceRecordID}]);
            const name = await md.GetEntityRecordName('Lists', compositeKey);
            return name ? name : `List: ${data.ResourceRecordID}`;
            return 'List [Error]';
