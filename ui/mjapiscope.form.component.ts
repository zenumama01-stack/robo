import { MJAPIScopeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: API Scopes') // Tell MemberJunction about this class
    selector: 'gen-mjapiscope-form',
    templateUrl: './mjapiscope.form.component.html'
export class MJAPIScopeFormComponent extends BaseFormComponent {
    public record!: MJAPIScopeEntity;
            { sectionKey: 'scopeDefinition', sectionName: 'Scope Definition', isExpanded: true },
            { sectionKey: 'scopeHierarchy', sectionName: 'Scope Hierarchy', isExpanded: true },
            { sectionKey: 'mJAPIScopes', sectionName: 'MJ: API Scopes', isExpanded: false }
