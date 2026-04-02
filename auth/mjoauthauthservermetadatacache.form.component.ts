import { MJOAuthAuthServerMetadataCacheEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: O Auth Auth Server Metadata Caches') // Tell MemberJunction about this class
    selector: 'gen-mjoauthauthservermetadatacache-form',
    templateUrl: './mjoauthauthservermetadatacache.form.component.html'
export class MJOAuthAuthServerMetadataCacheFormComponent extends BaseFormComponent {
    public record!: MJOAuthAuthServerMetadataCacheEntity;
