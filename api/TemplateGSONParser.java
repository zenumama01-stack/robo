 * This class can parse and serialize sets of {@link Template}s.
@Component(immediate = true, service = Parser.class, property = { "parser.type=parser.template", "format=json" })
public class TemplateGSONParser extends AbstractGSONParser<Template> {
    public Set<Template> parse(InputStreamReader reader) throws ParsingException {
                Set<Template> templates = new LinkedHashSet<>();
                    List<RuleTemplateDTO> templateDtos = gson.fromJson(jr, new TypeToken<List<RuleTemplateDTO>>() {
                    for (RuleTemplateDTO templateDto : templateDtos) {
                        templates.add(RuleTemplateDTOMapper.map(templateDto));
                    RuleTemplateDTO template = gson.fromJson(jr, RuleTemplateDTO.class);
                    templates.add(RuleTemplateDTOMapper.map(template));
            throw new ParsingException(new ParsingNestedException(ParsingNestedException.TEMPLATE, null, e));
