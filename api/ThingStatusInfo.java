 * A {@link ThingStatusInfo} represents status information of a thing which consists of
 * <li>the status itself </il>
 * <li>detail of the status</il>
 * <li>and a description of the status</il>
 * @author Dennis Nobel - Added null checks
public class ThingStatusInfo {
    private final ThingStatus status;
    private final ThingStatusDetail statusDetail;
    protected ThingStatusInfo() {
        status = ThingStatus.UNKNOWN;
        statusDetail = ThingStatusDetail.NONE;
     * @param status the status (must not be null)
     * @param statusDetail the detail of the status (must not be null)
    public ThingStatusInfo(ThingStatus status, ThingStatusDetail statusDetail, @Nullable String description) {
     * @return the status (not null)
    public ThingStatus getStatus() {
     * @return the status detail (not null)
    public ThingStatusDetail getStatusDetail() {
        String description = getDescription();
        return getStatus() + (getStatusDetail() == ThingStatusDetail.NONE ? "" : " (" + getStatusDetail() + ")")
                + (description == null || description.isBlank() ? "" : ": " + description);
        String description = this.description; // prevent NPE in case the class variable is changed between the two
                                               // calls in the next line
        result = prime * result + status.hashCode();
        result = prime * result + statusDetail.hashCode();
        ThingStatusInfo other = (ThingStatusInfo) obj;
        } else if (!Objects.equals(description, other.description)) {
