import { MJApplicationEntityEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Application Entities') // Tell MemberJunction about this class
    selector: 'gen-mjapplicationentity-form',
    templateUrl: './mjapplicationentity.form.component.html'
export class MJApplicationEntityFormComponent extends BaseFormComponent {
    public record!: MJApplicationEntityEntity;
            { sectionKey: 'applicationLinkage', sectionName: 'Application Linkage', isExpanded: true },
            { sectionKey: 'entityDefinition', sectionName: 'Entity Definition', isExpanded: true },
