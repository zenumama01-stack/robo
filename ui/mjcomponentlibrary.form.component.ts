import { MJComponentLibraryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Component Libraries') // Tell MemberJunction about this class
    selector: 'gen-mjcomponentlibrary-form',
    templateUrl: './mjcomponentlibrary.form.component.html'
export class MJComponentLibraryFormComponent extends BaseFormComponent {
    public record!: MJComponentLibraryEntity;
            { sectionKey: 'libraryIdentification', sectionName: 'Library Identification', isExpanded: true },
            { sectionKey: 'distributionAssets', sectionName: 'Distribution & Assets', isExpanded: true },
            { sectionKey: 'governanceDependencies', sectionName: 'Governance & Dependencies', isExpanded: false },
            { sectionKey: 'mJComponentLibraryLinks', sectionName: 'MJ: Component Library Links', isExpanded: false }
