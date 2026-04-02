 * This class is used to present status of a rule. The status consists of three parts:
 * The main status, a status detail and a string description.
 * @author Kai Kreuzer - Refactored to match ThingStatusInfo implementation
public class RuleStatusInfo {
    private @NonNullByDefault({}) RuleStatus status;
    private @NonNullByDefault({}) RuleStatusDetail statusDetail;
     * Default constructor for deserialization e.g. by Gson.
    protected RuleStatusInfo() {
     * Constructs a status info.
     * @param status the status
    public RuleStatusInfo(RuleStatus status) {
        this(status, RuleStatusDetail.NONE);
     * @param statusDetail the detail of the status
    public RuleStatusInfo(RuleStatus status, RuleStatusDetail statusDetail) {
        this(status, statusDetail, null);
     * @param description the description of the status
    public RuleStatusInfo(RuleStatus status, RuleStatusDetail statusDetail, @Nullable String description) {
        this.status = status;
        this.statusDetail = statusDetail;
     * Gets the status itself.
     * @return the status
    public RuleStatus getStatus() {
     * Gets the detail of the status.
     * @return the status detail
    public RuleStatusDetail getStatusDetail() {
        return statusDetail;
     * Gets the description of the status.
     * @return the description
        boolean hasDescription = getDescription() != null && !getDescription().isEmpty();
        return getStatus() + (getStatusDetail() == RuleStatusDetail.NONE ? "" : " (" + getStatusDetail() + ")")
                + (hasDescription ? ": " + getDescription() : "");
        result = prime * result + ((description == null) ? 0 : description.hashCode());
        result = prime * result + ((status == null) ? 0 : status.hashCode());
        result = prime * result + ((statusDetail == null) ? 0 : statusDetail.hashCode());
        RuleStatusInfo other = (RuleStatusInfo) obj;
        if (description == null) {
            if (other.description != null) {
        } else if (!description.equals(other.description)) {
        if (status != other.status) {
        if (statusDetail != other.statusDetail) {
