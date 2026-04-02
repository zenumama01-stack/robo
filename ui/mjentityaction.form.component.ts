@RegisterClass(BaseFormComponent, 'MJ: Entity Actions') // Tell MemberJunction about this class
    selector: 'gen-mjentityaction-form',
    templateUrl: './mjentityaction.form.component.html'
export class MJEntityActionFormComponent extends BaseFormComponent {
            { sectionKey: 'relationshipKeys', sectionName: 'Relationship Keys', isExpanded: true },
            { sectionKey: 'actionConfiguration', sectionName: 'Action Configuration', isExpanded: true },
            { sectionKey: 'entityActionFilters', sectionName: 'Entity Action Filters', isExpanded: false },
            { sectionKey: 'entityActionInvocations', sectionName: 'Entity Action Invocations', isExpanded: false },
            { sectionKey: 'entityActionParams', sectionName: 'Entity Action Params', isExpanded: false }
