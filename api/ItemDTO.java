 * This is a data transfer object that is used to serialize items.
 * @author Andre Fuechsel - added tag support
@Schema(name = "Item")
public class ItemDTO {
    public List<String> groupNames;
    public ItemDTO() {
