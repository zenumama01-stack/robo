import { MJEntitySettingEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Settings') // Tell MemberJunction about this class
    selector: 'gen-mjentitysetting-form',
    templateUrl: './mjentitysetting.form.component.html'
export class MJEntitySettingFormComponent extends BaseFormComponent {
    public record!: MJEntitySettingEntity;
            { sectionKey: 'entityConfiguration', sectionName: 'Entity Configuration', isExpanded: true },
