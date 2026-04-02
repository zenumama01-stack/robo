import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.JsonNode;
@Component(immediate = true, service = Parser.class, property = { "parser.type=parser.template", "format=yaml" })
public class TemplateYAMLParser extends AbstractJacksonYAMLParser<Template> {
            Set<Template> templates = new HashSet<>();
            JsonNode rootNode = YAML_MAPPER.readTree(reader);
            if (rootNode.isArray()) {
                List<RuleTemplateDTO> templateDtos = YAML_MAPPER.convertValue(rootNode,
                        new TypeReference<List<RuleTemplateDTO>>() {
                for (RuleTemplateDTO templateDTO : templateDtos) {
                    templates.add(RuleTemplateDTOMapper.map(templateDTO));
                RuleTemplateDTO templateDto = YAML_MAPPER.convertValue(rootNode, new TypeReference<RuleTemplateDTO>() {
                reader.close();
