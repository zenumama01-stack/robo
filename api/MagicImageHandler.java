 * The {@link MagicImageHandler} is responsible for handling commands, which are
public class MagicImageHandler extends BaseThingHandler {
    private @NonNullByDefault({}) String url;
    public MagicImageHandler(Thing thing) {
        if (command == RefreshType.REFRESH) {
            RawType image = HttpUtil.downloadImage(url, 30000);
            if (image != null) {
                updateState(channelUID, image);
        url = (String) getConfig().get("url");
        if (url == null || url.isEmpty()) {
            updateStatus(ThingStatus.OFFLINE, ThingStatusDetail.CONFIGURATION_ERROR, "The URL must not be blank");
