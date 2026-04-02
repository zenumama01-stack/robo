import org.openhab.core.automation.Rule.TemplateState;
 * @author Kai Kreuzer - Changed to using RuleBuilder
public class RuleDTOMapper {
    public static RuleDTO map(final Rule rule) {
        final RuleDTO ruleDto = new RuleDTO();
        fillProperties(rule, ruleDto);
        return ruleDto;
    public static Rule map(final RuleDTO ruleDto) {
        RuleBuilder builder = RuleBuilder.create(ruleDto.uid).withActions(ActionDTOMapper.mapDto(ruleDto.actions))
                .withConditions(ConditionDTOMapper.mapDto(ruleDto.conditions))
                .withTriggers(TriggerDTOMapper.mapDto(ruleDto.triggers))
                .withConfiguration(new Configuration(ruleDto.configuration))
                .withConfigurationDescriptions(ConfigDescriptionDTOMapper.map(ruleDto.configDescriptions))
                .withTemplateUID(ruleDto.templateUID).withVisibility(ruleDto.visibility).withTags(ruleDto.tags)
                .withName(ruleDto.name).withDescription(ruleDto.description);
        if (ruleDto.templateState == null) {
            builder.withTemplateState(ruleDto.templateUID == null ? TemplateState.NO_TEMPLATE : TemplateState.PENDING);
            builder.withTemplateState(TemplateState.typeOf(ruleDto.templateState));
    protected static void fillProperties(final Rule from, final RuleDTO to) {
        to.triggers = TriggerDTOMapper.map(from.getTriggers());
        to.conditions = ConditionDTOMapper.map(from.getConditions());
        to.actions = ActionDTOMapper.map(from.getActions());
        to.templateUID = from.getTemplateUID();
        to.templateState = from.getTemplateState().toString();
        to.name = from.getName();
