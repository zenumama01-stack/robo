import { MJComponentRegistryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Component Registries') // Tell MemberJunction about this class
    selector: 'gen-mjcomponentregistry-form',
    templateUrl: './mjcomponentregistry.form.component.html'
export class MJComponentRegistryFormComponent extends BaseFormComponent {
    public record!: MJComponentRegistryEntity;
            { sectionKey: 'registryCoreInfo', sectionName: 'Registry Core Info', isExpanded: true },
            { sectionKey: 'accessDetails', sectionName: 'Access Details', isExpanded: true },
            { sectionKey: 'mJComponents', sectionName: 'MJ: Components', isExpanded: false }
