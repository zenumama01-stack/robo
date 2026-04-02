import com.google.gson.stream.JsonReader;
import com.google.gson.stream.JsonToken;
 * This class can parse and serialize sets of {@link Rule}s.
@Component(immediate = true, service = Parser.class, property = { "parser.type=parser.rule", "format=json" })
public class RuleGSONParser extends AbstractGSONParser<Rule> {
    public Set<Rule> parse(InputStreamReader reader) throws ParsingException {
        JsonReader jr = new JsonReader(reader);
            Set<Rule> rules = new LinkedHashSet<>();
            if (jr.hasNext()) {
                JsonToken token = jr.peek();
                if (JsonToken.BEGIN_ARRAY.equals(token)) {
                    List<RuleDTO> ruleDtos = gson.fromJson(jr, new TypeToken<List<RuleDTO>>() {
                    }.getType());
                    for (RuleDTO ruleDto : ruleDtos) {
                        rules.add(RuleDTOMapper.map(ruleDto));
                    RuleDTO ruleDto = gson.fromJson(jr, RuleDTO.class);
            throw new ParsingException(new ParsingNestedException(ParsingNestedException.RULE, null, e));
                jr.close();
