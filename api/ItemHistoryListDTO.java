package org.openhab.core.io.rest.core.persistence;
 * This is a java bean that is used to serialize item lists.
public class ItemHistoryListDTO {
    public final List<ItemHistoryDTO> item = new ArrayList<>();
    public ItemHistoryListDTO() {
    public ItemHistoryListDTO(Collection<ItemHistoryDTO> list) {
        item.addAll(list);
