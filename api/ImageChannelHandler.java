import org.openhab.core.thing.binding.generic.ChannelHandler;
import org.openhab.core.thing.binding.generic.ChannelHandlerContent;
 * The {@link ImageChannelHandler} implements {@link org.openhab.core.library.items.ImageItem} conversions
public class ImageChannelHandler implements ChannelHandler {
    private final Consumer<State> updateState;
    public ImageChannelHandler(Consumer<State> updateState) {
        this.updateState = updateState;
    public void process(@Nullable ChannelHandlerContent content) {
        if (content == null) {
            updateState.accept(UnDefType.UNDEF);
        String mediaType = content.getMediaType();
        updateState.accept(
                new RawType(content.getRawContent(), mediaType != null ? mediaType : RawType.DEFAULT_MIME_TYPE));
    public void send(Command command) {
        throw new IllegalStateException("Read-only channel");
