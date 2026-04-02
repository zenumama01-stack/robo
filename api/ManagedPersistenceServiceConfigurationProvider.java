package org.openhab.core.persistence.registry;
 * The {@link ManagedPersistenceServiceConfigurationProvider} implements a
 * {@link PersistenceServiceConfigurationProvider} for managed configurations which are stored in a JSON database
@Component(immediate = true, service = { PersistenceServiceConfigurationProvider.class,
        ManagedPersistenceServiceConfigurationProvider.class })
public class ManagedPersistenceServiceConfigurationProvider
        extends AbstractManagedProvider<PersistenceServiceConfiguration, String, PersistenceServiceConfigurationDTO>
        implements PersistenceServiceConfigurationProvider {
    private static final String STORAGE_NAME = "org.openhab.core.persistence.PersistenceServiceConfiguration";
    public ManagedPersistenceServiceConfigurationProvider(@Reference StorageService storageService) {
        return STORAGE_NAME;
    protected @Nullable PersistenceServiceConfiguration toElement(String key,
            PersistenceServiceConfigurationDTO persistableElement) {
        return PersistenceServiceConfigurationDTOMapper.map(persistableElement);
    protected PersistenceServiceConfigurationDTO toPersistableElement(PersistenceServiceConfiguration element) {
        return PersistenceServiceConfigurationDTOMapper.map(element);
