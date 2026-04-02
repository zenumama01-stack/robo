package org.openhab.core.io.rest.core.item;
 * This is an enriched data transfer object that is used to serialize group items.
@Schema(name = "EnrichedGroupItem")
public class EnrichedGroupItemDTO extends EnrichedItemDTO {
    public EnrichedGroupItemDTO(ItemDTO itemDTO, EnrichedItemDTO[] members, String link, String state, String lastState,
            Long lastStateUpdate, Long lastStateChange, String transformedState, StateDescription stateDescription,
            String unitSymbol) {
        super(itemDTO, link, state, lastState, lastStateUpdate, lastStateChange, transformedState, stateDescription,
                null, unitSymbol);
        this.members = members;
    public EnrichedItemDTO[] members;
