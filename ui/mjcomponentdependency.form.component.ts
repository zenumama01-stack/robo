import { MJComponentDependencyEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Component Dependencies') // Tell MemberJunction about this class
    selector: 'gen-mjcomponentdependency-form',
    templateUrl: './mjcomponentdependency.form.component.html'
export class MJComponentDependencyFormComponent extends BaseFormComponent {
    public record!: MJComponentDependencyEntity;
            { sectionKey: 'componentRelationships', sectionName: 'Component Relationships', isExpanded: true },
