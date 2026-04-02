package org.openhab.core.model.yaml.internal.config;
import com.fasterxml.jackson.annotation.JsonAlias;
import com.fasterxml.jackson.annotation.JsonProperty;
 * This is a data transfer object used to serialize a parameter of a configuration description in a YAML configuration.
public class YamlConfigDescriptionParameterDTO {
    @JsonProperty("default")
    @JsonAlias("defaultValue")
    @JsonAlias({ "stepsize" })
    public YamlConfigDescriptionParameterDTO() {
     * Creates a new instance based on the specified {@link ConfigDescriptionParameter}.
     * @param parameter the {@link ConfigDescriptionParameter}.
    public YamlConfigDescriptionParameterDTO(@NonNull ConfigDescriptionParameter parameter) {
        this.type = parameter.getType();
        this.min = parameter.getMinimum();
        this.max = parameter.getMaximum();
        this.step = parameter.getStepSize();
        this.pattern = parameter.getPattern();
        this.readOnly = parameter.isReadOnly();
        this.multiple = parameter.isMultiple();
        this.context = parameter.getContext();
        this.required = parameter.isRequired();
        this.defaultValue = parameter.getDefault();
        this.label = parameter.getLabel();
        this.description = parameter.getDescription();
        List<@NonNull ParameterOption> options = parameter.getOptions();
        if (!options.isEmpty()) {
            List<ParameterOptionDTO> optionDtos = new ArrayList<>(options.size());
                optionDtos.add(new ParameterOptionDTO(option.getValue(), option.getLabel()));
            this.options = optionDtos;
        List<@NonNull FilterCriteria> filterCriterias = parameter.getFilterCriteria();
        if (!filterCriterias.isEmpty()) {
            List<FilterCriteriaDTO> filterCriteriaDtos = new ArrayList<>(filterCriteria.size());
            for (FilterCriteria filterCriteria : filterCriterias) {
                filterCriteriaDtos.add(new FilterCriteriaDTO(filterCriteria.getName(), filterCriteria.getValue()));
            this.filterCriteria = filterCriteriaDtos;
        this.groupName = parameter.getGroupName();
        this.advanced = parameter.isAdvanced();
        this.limitToOptions = parameter.getLimitToOptions();
        this.multipleLimit = parameter.getMultipleLimit();
        this.unit = parameter.getUnit();
        this.unitLabel = parameter.getUnitLabel();
        this.verify = parameter.isVerifyable();
        return Objects.hash(advanced, context, defaultValue, description, filterCriteria, groupName, label,
                limitToOptions, max, min, multiple, multipleLimit, options, pattern, readOnly, required, step, type,
                unit, unitLabel, verify);
        if (!(obj instanceof YamlConfigDescriptionParameterDTO)) {
        YamlConfigDescriptionParameterDTO other = (YamlConfigDescriptionParameterDTO) obj;
        return Objects.equals(advanced, other.advanced) && Objects.equals(context, other.context)
                && Objects.equals(defaultValue, other.defaultValue) && Objects.equals(description, other.description)
                && Objects.equals(filterCriteria, other.filterCriteria) && Objects.equals(groupName, other.groupName)
                && Objects.equals(label, other.label) && Objects.equals(limitToOptions, other.limitToOptions)
                && Objects.equals(max, other.max) && Objects.equals(min, other.min)
                && Objects.equals(multiple, other.multiple) && Objects.equals(multipleLimit, other.multipleLimit)
                && Objects.equals(options, other.options) && Objects.equals(pattern, other.pattern)
                && Objects.equals(readOnly, other.readOnly) && required == other.required
                && Objects.equals(step, other.step) && type == other.type && Objects.equals(unit, other.unit)
                && Objects.equals(unitLabel, other.unitLabel) && Objects.equals(verify, other.verify);
        StringBuilder builder = new StringBuilder(getClass().getSimpleName());
        builder.append(" [");
            builder.append("context=").append(context).append(", ");
            builder.append("defaultValue=").append(defaultValue).append(", ");
            builder.append("description=").append(description).append(", ");
            builder.append("label=").append(label).append(", ");
        builder.append("required=").append(required).append(", ");
            builder.append("type=").append(type).append(", ");
            builder.append("min=").append(min).append(", ");
            builder.append("max=").append(max).append(", ");
            builder.append("step=").append(step).append(", ");
            builder.append("pattern=").append(pattern).append(", ");
            builder.append("readOnly=").append(readOnly).append(", ");
            builder.append("multiple=").append(multiple).append(", ");
        if (multipleLimit != null) {
            builder.append("multipleLimit=").append(multipleLimit).append(", ");
            builder.append("groupName=").append(groupName).append(", ");
            builder.append("advanced=").append(advanced).append(", ");
            builder.append("verify=").append(verify).append(", ");
            builder.append("limitToOptions=").append(limitToOptions).append(", ");
            builder.append("unit=").append(unit).append(", ");
            builder.append("unitLabel=").append(unitLabel).append(", ");
            builder.append("options=").append(options).append(", ");
            builder.append("filterCriteria=").append(filterCriteria);
        builder.append("]");
     * Creates a {@link List} of {@link ConfigDescriptionParameter}s from a {@link Map} of parameter names and
     * {@link YamlConfigDescriptionParameterDTO}s, to be used during deserialization.
     * @param configDescriptionDtos the {@link Map} of {@link String} and {@link YamlConfigDescriptionParameterDTO}
     *            pairs.
     * @return The corresponding {@link List} of {@link ConfigDescriptionParameter}s.
    public static @NonNull List<@NonNull ConfigDescriptionParameter> mapConfigDescriptions(
            @NonNull Map<@NonNull String, @NonNull YamlConfigDescriptionParameterDTO> configDescriptionDtos) {
        List<ConfigDescriptionParameter> result = new ArrayList<>(configDescriptionDtos.size());
        List<FilterCriteriaDTO> filterCriteriaDtos;
        List<FilterCriteria> filterCriterias;
        List<ParameterOptionDTO> parameterOptionDtos;
        List<ParameterOption> parameterOptions;
        ConfigDescriptionParameterBuilder builder;
        YamlConfigDescriptionParameterDTO parameterDto;
        for (Entry<String, YamlConfigDescriptionParameterDTO> parameterEntry : configDescriptionDtos.entrySet()) {
            parameterDto = parameterEntry.getValue();
            builder = ConfigDescriptionParameterBuilder.create(parameterEntry.getKey(), parameterDto.type)
                    .withAdvanced(parameterDto.advanced).withContext(parameterDto.context)
                    .withDefault(parameterDto.defaultValue).withDescription(parameterDto.description)
                    .withGroupName(parameterDto.groupName).withLabel(parameterDto.label)
                    .withLimitToOptions(parameterDto.limitToOptions).withMaximum(parameterDto.max)
                    .withMinimum(parameterDto.min).withMultiple(parameterDto.multiple)
                    .withMultipleLimit(parameterDto.multipleLimit).withPattern(parameterDto.pattern)
                    .withReadOnly(parameterDto.readOnly).withRequired(parameterDto.required)
                    .withStepSize(parameterDto.step).withUnit(parameterDto.unit).withUnitLabel(parameterDto.unitLabel)
                    .withVerify(parameterDto.verify);
            filterCriteriaDtos = parameterDto.filterCriteria;
            if (filterCriteriaDtos != null) {
                filterCriterias = new ArrayList<>(filterCriteriaDtos.size());
                for (FilterCriteriaDTO filterCriteriaDto : filterCriteriaDtos) {
                    filterCriterias.add(new FilterCriteria(filterCriteriaDto.name, filterCriteriaDto.value));
                builder.withFilterCriteria(filterCriterias);
            parameterOptionDtos = parameterDto.options;
            if (parameterOptionDtos != null) {
                parameterOptions = new ArrayList<>(parameterOptionDtos.size());
                for (ParameterOptionDTO optionDto : parameterOptionDtos) {
                    parameterOptions.add(new ParameterOption(optionDto.value, optionDto.label));
                builder.withOptions(parameterOptions);
            result.add(builder.build());
