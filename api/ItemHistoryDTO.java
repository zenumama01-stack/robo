package org.openhab.core.persistence.dto;
 * This is a java bean that is used to serialize items to JSON.
@Schema(name = "ItemHistory")
public class ItemHistoryDTO {
    public String datapoints;
    public List<HistoryDataBean> data = new ArrayList<>();
    public ItemHistoryDTO() {
     * Add a new record to the data history.
     * This method returns a double value equal to the state. This may be used for comparison by the caller.
     * @param time the time of the record
     * @param state the state at this time
    public void addData(long time, State state) {
        HistoryDataBean newVal = new HistoryDataBean();
        newVal.time = time;
            // we strip the unit from the state, since historic item states are expected to be all in the default unit
            newVal.state = quantityState.toBigDecimal().toString();
        } else if (state instanceof DecimalType decimalType) {
            // use BigDecimal.toString() to hit the internal cache
            newVal.state = decimalType.toBigDecimal().toString();
            newVal.state = state.toString();
        data.add(newVal);
     * Sort the data history by time.
    public void sortData() {
        data.sort(Comparator.comparingLong(o -> o.time));
    public static class HistoryDataBean {
        public long time;
