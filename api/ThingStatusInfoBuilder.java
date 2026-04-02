 * {@link ThingStatusInfoBuilder} is responsible for creating {@link ThingStatusInfo}s.
public class ThingStatusInfoBuilder {
    private ThingStatusDetail statusDetail;
    private ThingStatusInfoBuilder(ThingStatus status, ThingStatusDetail statusDetail, @Nullable String description) {
     * Creates a status info builder for the given status and detail.
     * @return status info builder
    public static ThingStatusInfoBuilder create(ThingStatus status, ThingStatusDetail statusDetail) {
        return new ThingStatusInfoBuilder(status, statusDetail, null);
     * Creates a status info builder for the given status.
    public static ThingStatusInfoBuilder create(ThingStatus status) {
        return create(status, ThingStatusDetail.NONE);
     * Appends a description to the status to build.
    public ThingStatusInfoBuilder withDescription(@Nullable String description) {
     * Appends a status detail to the status to build.
     * @param statusDetail the status detail (must not be null)
    public ThingStatusInfoBuilder withStatusDetail(ThingStatusDetail statusDetail) {
     * Builds and returns the status info.
     * @return status info
    public ThingStatusInfo build() {
        return new ThingStatusInfo(status, statusDetail, description);
