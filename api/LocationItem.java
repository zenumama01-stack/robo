 * A LocationItem can be used to store GPS related informations, addresses...
 * This is useful for location awareness related functions
public class LocationItem extends GenericItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(PointType.class, UnDefType.class);
    private static final List<Class<? extends Command>> ACCEPTED_COMMAND_TYPES = List.of(PointType.class,
    public LocationItem(String name) {
        super(CoreItemFactory.LOCATION, name);
     * Send a PointType command to the item.
    public void send(PointType command) {
    public void send(PointType command, @Nullable String source) {
     * Compute the distance with another Point type,
     * http://stackoverflow.com/questions/837872/calculate-distance-in-meters-when-you-know-longitude-and-latitude-in-
     * java
     * @param awayItem the point to calculate the distance with
     * @return distance between the two points in meters
    public DecimalType distanceFrom(@Nullable LocationItem awayItem) {
        if (awayItem != null && awayItem.state instanceof PointType awayPoint
                && this.state instanceof PointType thisPoint) {
            return thisPoint.distanceFrom(awayPoint);
        return new DecimalType(-1);
        if (timeSeries.getStates().allMatch(s -> s.state() instanceof PointType)) {
