package org.openhab.core.automation.parser;
 * This interface provides opportunity to plug different parsers, for example JSON, GSON or other.
public interface Parser<T> {
     * Specifies the type of the parser whose is the type of the parsed automation objects.
     * Example : "parser.type" = "parser.module.type";
     * It is used as registration property of the corresponding service.
    String PARSER_TYPE = "parser.type";
     * Defines one of the possible values of property {@link #PARSER_TYPE}.
    String PARSER_MODULE_TYPE = "parser.module.type";
    String PARSER_TEMPLATE = "parser.template";
    String PARSER_RULE = "parser.rule";
     * Defines a service registration property used for recognition of which file format is supported by the parser.
     * Example : "format" = "json";
    String FORMAT = "format";
     * Defines a possible value of property {@link #FORMAT}. It means that the parser supports {@code JSON} format.
    String FORMAT_JSON = "json";
     * Defines a possible value of property {@link #FORMAT}. It means that the parser supports {@code YAML} format.
    String FORMAT_YAML = "yaml";
     * Loads a file with some particular format and parse it to the corresponding automation objects.
     * @param reader {@link InputStreamReader} which reads from a file containing automation object representations.
     * @return a set of automation objects. Each object represents the result of parsing of one object.
     * @throws ParsingException is thrown when json format is wrong or there is a semantic error in description of
     *             the automation objects.
    Set<T> parse(InputStreamReader reader) throws ParsingException;
     * Records the automation objects in a file with some particular format.
     * @param dataObjects provides an objects for export.
     * @param writer is {@link OutputStreamWriter} used to write the automation objects in a file.
    void serialize(Set<T> dataObjects, OutputStreamWriter writer) throws Exception;
