 * This is a utility class to convert between the Rule Templates and RuleTemplateDTO objects.
public class RuleTemplateDTOMapper {
    public static RuleTemplateDTO map(final RuleTemplate template) {
        final RuleTemplateDTO templateDTO = new RuleTemplateDTO();
        fillProperties(template, templateDTO);
        return templateDTO;
    public static RuleTemplate map(final RuleTemplateDTO ruleTemplateDto) {
        return new RuleTemplate(ruleTemplateDto.uid, ruleTemplateDto.label, ruleTemplateDto.description,
                ruleTemplateDto.tags, TriggerDTOMapper.mapDto(ruleTemplateDto.triggers),
                ConditionDTOMapper.mapDto(ruleTemplateDto.conditions), ActionDTOMapper.mapDto(ruleTemplateDto.actions),
                ConfigDescriptionDTOMapper.map(ruleTemplateDto.configDescriptions), ruleTemplateDto.visibility);
    protected static void fillProperties(final RuleTemplate from, final RuleTemplateDTO to) {
