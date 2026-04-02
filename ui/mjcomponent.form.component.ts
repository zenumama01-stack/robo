import { MJComponentEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Components') // Tell MemberJunction about this class
    selector: 'gen-mjcomponent-form',
    templateUrl: './mjcomponent.form.component.html'
export class MJComponentFormComponent extends BaseFormComponent {
    public record!: MJComponentEntity;
            { sectionKey: 'identificationVersioning', sectionName: 'Identification & Versioning', isExpanded: true },
            { sectionKey: 'specificationDesign', sectionName: 'Specification & Design', isExpanded: true },
            { sectionKey: 'developerOwnership', sectionName: 'Developer & Ownership', isExpanded: false },
            { sectionKey: 'registrySynchronization', sectionName: 'Registry & Synchronization', isExpanded: false },
            { sectionKey: 'mJComponentDependencies', sectionName: 'MJ: Component Dependencies', isExpanded: false },
            { sectionKey: 'mJComponentLibraryLinks', sectionName: 'MJ: Component Library Links', isExpanded: false },
            { sectionKey: 'mJComponentDependencies1', sectionName: 'MJ: Component Dependencies', isExpanded: false }
