 * Entity Integration Resource - displays entity-action mappings
@RegisterClass(BaseResourceComponent, 'ActionsEntitiesResource')
  selector: 'mj-entity-integration',
    <div class="entity-integration-placeholder" >
        <h3>Entity Integration</h3>
        <p>Entity-action mapping and relationship management coming soon...</p>
    .entity-integration-placeholder {
export class EntityIntegrationComponent extends BaseResourceComponent implements OnInit {
    return 'Entity Integration';
    return 'fa-solid fa-sitemap';
