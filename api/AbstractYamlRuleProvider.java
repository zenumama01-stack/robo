package org.openhab.core.model.yaml.internal.rules;
 * {@link AbstractYamlRuleProvider} is an abstract class that contains some methods common for {@link YamlRuleProvider}
 * and {@link YamlRuleTemplateProvider}.
 * @param <E> the type of the provided elements
public abstract class AbstractYamlRuleProvider<@NonNull E> extends AbstractProvider<E> {
     * Extracts and returns the module IDs from any number of {@link Collection}s of either {@link Module}s or
     * {@link YamlModuleDTO}s.
     * @param moduleCollections the collections of {@link Module}s or {@link YamlModuleDTO}s.
     * @return The resulting {@link Set} of IDs.
    protected Set<String> extractModuleIds(@Nullable Collection<?> @Nullable... moduleCollections) {
        if (moduleCollections == null || moduleCollections.length == 0) {
        for (@Nullable
        Collection<?> modules : moduleCollections) {
                for (Object object : modules) {
                    id = null;
                    if (object instanceof Module module) {
                        id = module.getId();
                    } else if (object instanceof YamlModuleDTO moduleDto) {
                        id = moduleDto.id;
                    if (id != null && !id.isBlank()) {
                        result.add(id);
     * Creates a {@link List} of modules of the specified class from a {@link List} of module DTOs. Modules without an
     * ID will be assigned a unique generated ID.
     * @param <T> the type of the resulting {@link Module}s.
     * @param <D> the type of the supplied DTOs.
     * @param dtos the {@link List} of DTOs.
     * @param otherModuleIds the {@link Set} of {@link String}s containing already in-use module IDs.
     * @param targetClazz the {@link Module} class to create.
     * @return The {@link List} of created {@link Module}s.
     * @throws SerializationException If the mapping fails.
    protected <T extends Module, D extends YamlModuleDTO> List<T> mapModules(List<D> dtos, Set<String> otherModuleIds,
            Class<T> targetClazz) throws SerializationException {
        List<T> modules = new ArrayList<>(dtos.size());
        int id = 0;
        boolean generateIds = dtos.stream().anyMatch(m -> m.id == null || m.id.isBlank());
        while (generateIds) {
                String ids = Integer.toString(++id);
                if (!dtos.stream().anyMatch(m -> ids.equals(m.id))
                        && (otherModuleIds == null || !otherModuleIds.stream().anyMatch(i -> ids.equals(i)))) {
            final String ids2 = Integer.toString(id);
            dtos.stream().filter(m -> m.id == null || m.id.isBlank()).findFirst().ifPresent(m -> m.id = ids2);
            generateIds = dtos.stream().anyMatch(m -> m.id == null || m.id.isBlank());
        for (D dto : dtos) {
                if (targetClazz.isAssignableFrom(Condition.class) && dto instanceof YamlConditionDTO cDto) {
                    modules.add((T) ModuleBuilder.createCondition().withId(dto.id).withTypeUID(dto.type)
                            .withConfiguration(new Configuration(dto.config)).withInputs(cDto.inputs)
                            .withLabel(dto.label).withDescription(dto.description).build());
                } else if (targetClazz.isAssignableFrom(Action.class) && dto instanceof YamlActionDTO aDto) {
                    modules.add((T) ModuleBuilder.createAction().withId(dto.id).withTypeUID(dto.type)
                            .withConfiguration(new Configuration(dto.config)).withInputs(aDto.inputs)
                } else if (targetClazz.isAssignableFrom(Trigger.class)) {
                    modules.add((T) ModuleBuilder.createTrigger().withId(dto.id).withTypeUID(dto.type)
                            .withConfiguration(new Configuration(dto.config)).withLabel(dto.label)
                            .withDescription(dto.description).build());
                    throw new IllegalArgumentException("Invalid combination of target and dto classes: "
                            + targetClazz.getSimpleName() + " <-> " + dto.getClass().getSimpleName());
                throw new SerializationException("Failed to process YAML rule or rule template "
                        + targetClazz.getSimpleName() + ": \"" + dto + "\": " + e.getMessage(), e);
