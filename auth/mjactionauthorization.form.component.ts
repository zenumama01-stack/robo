import { MJActionAuthorizationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Action Authorizations') // Tell MemberJunction about this class
    selector: 'gen-mjactionauthorization-form',
    templateUrl: './mjactionauthorization.form.component.html'
export class MJActionAuthorizationFormComponent extends BaseFormComponent {
    public record!: MJActionAuthorizationEntity;
            { sectionKey: 'authorizationMapping', sectionName: 'Authorization Mapping', isExpanded: true },
            { sectionKey: 'actionAuthorizationDetails', sectionName: 'Action & Authorization Details', isExpanded: true },
