import { MJContentSourceParamEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Source Params') // Tell MemberJunction about this class
    selector: 'gen-mjcontentsourceparam-form',
    templateUrl: './mjcontentsourceparam.form.component.html'
export class MJContentSourceParamFormComponent extends BaseFormComponent {
    public record!: MJContentSourceParamEntity;
            { sectionKey: 'parameterSettings', sectionName: 'Parameter Settings', isExpanded: true }
