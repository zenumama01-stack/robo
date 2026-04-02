 * This is a utility class to convert between the respective object and its DTO.
 * @author Kai Kreuzer - added editable field
public class EnrichedRuleDTOMapper extends RuleDTOMapper {
    public static EnrichedRuleDTO map(final Rule rule, final RuleManager ruleEngine,
            final ManagedRuleProvider managedRuleProvider) {
        final EnrichedRuleDTO enrichedRuleDto = new EnrichedRuleDTO();
        fillProperties(rule, enrichedRuleDto);
        enrichedRuleDto.status = ruleEngine.getStatusInfo(rule.getUID());
        enrichedRuleDto.editable = managedRuleProvider.get(rule.getUID()) != null;
        return enrichedRuleDto;
