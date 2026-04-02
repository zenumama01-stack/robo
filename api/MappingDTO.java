 * This is a data transfer object that is used to serialize command mappings.
 * @author Laurent Garnier - New fields position and icon
 * @author Laurent Garnier - Replace field position by fields row and column
 * @author Laurent Garnier - New field releaseCommand
@Schema(name = "Mapping")
public class MappingDTO {
    public Integer row;
    public Integer column;
    public String command;
    public String releaseCommand;
    public String icon;
    public MappingDTO() {
