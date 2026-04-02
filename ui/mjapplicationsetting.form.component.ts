import { MJApplicationSettingEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Application Settings') // Tell MemberJunction about this class
    selector: 'gen-mjapplicationsetting-form',
    templateUrl: './mjapplicationsetting.form.component.html'
export class MJApplicationSettingFormComponent extends BaseFormComponent {
    public record!: MJApplicationSettingEntity;
            { sectionKey: 'settingDefinition', sectionName: 'Setting Definition', isExpanded: true },
