 * A DTO that is used on the REST API to provide infos about {@link HumanLanguageInterpreter} to UIs.
@Schema(name = "HumanLanguageInterpreter")
public class HumanLanguageInterpreterDTO {
    public Set<String> locales;
