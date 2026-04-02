 * An ImageItem holds the binary image data as its status.
public class ImageItem extends GenericItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(RawType.class, UnDefType.class);
    public ImageItem(String name) {
        super(CoreItemFactory.IMAGE, name);
        if (timeSeries.getStates().allMatch(s -> s.state() instanceof RawType)) {
