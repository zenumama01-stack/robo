import { PublicationEntity } from 'mj_generatedentities';
@RegisterClass(BaseFormComponent, 'Publications') // Tell MemberJunction about this class
    selector: 'gen-publication-form',
    templateUrl: './publication.form.component.html'
export class PublicationFormComponent extends BaseFormComponent {
    public record!: PublicationEntity;
            { sectionKey: 'corePublicationInfo', sectionName: 'Core Publication Info', isExpanded: true },
            { sectionKey: 'salesAvailability', sectionName: 'Sales & Availability', isExpanded: true },
