import org.openhab.core.types.CommandDescription;
 * This is an enriched data transfer object that is used to serialize items with dynamic data like the state, the state
 * description and the link.
 * @author Kai Kreuzer - Added metadata
 * @author Mark Herwege - Added default unit symbol
 * @author Mark Herwege - Added parent groups
@Schema(name = "EnrichedItem")
public class EnrichedItemDTO extends ItemDTO {
    public String link;
    public String state;
    public String transformedState;
    public StateDescription stateDescription;
    public CommandDescription commandDescription;
    public String lastState;
    public Long lastStateUpdate;
    public Long lastStateChange;
    public String unitSymbol;
    public Map<String, Object> metadata;
    public EnrichedItemDTO[] parents = null;
    public EnrichedItemDTO(ItemDTO itemDTO, String link, String state, String lastState, Long lastStateUpdate,
            Long lastStateChange, String transformedState, StateDescription stateDescription,
            CommandDescription commandDescription, String unitSymbol) {
        this.transformedState = transformedState;
        this.stateDescription = stateDescription;
        this.commandDescription = commandDescription;
        this.lastState = lastState;
        this.lastStateUpdate = lastStateUpdate;
        this.lastStateChange = lastStateChange;
        this.unitSymbol = unitSymbol;
