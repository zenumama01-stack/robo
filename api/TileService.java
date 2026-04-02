package org.openhab.core.ui.internal.tiles;
import org.openhab.core.ui.tiles.ExternalServiceTile;
import org.openhab.core.ui.tiles.ExternalServiceTile.TileBuilder;
 * This component registers the UI tiles.
 * @author Laurent Garnier - internationalization
 * @author Hilbrand Bouwkamp - internationalization
 * @author Yannick Schaus - refactor into tile service, remove dashboard components
@Component(immediate = true, name = "org.openhab.core.ui.tiles")
public class TileService implements TileProvider {
    private static final String LINK_NAME = "link-name";
    private static final String LINK_URL = "link-url";
    private static final String LINK_IMAGEURL = "link-imageurl";
    private final Logger logger = LoggerFactory.getLogger(TileService.class);
    private final Set<Tile> tiles = new CopyOnWriteArraySet<>();
    public TileService(Map<String, Object> properties) {
        addTilesForExternalServices(properties);
    protected void addTile(Tile tile) {
        tiles.add(tile);
    protected void removeTile(Tile tile) {
        tiles.remove(tile);
    public Stream<Tile> getTiles() {
        return tiles.stream();
    private void addTilesForExternalServices(Map<String, Object> properties) {
        for (String key : properties.keySet()) {
            if (key.endsWith(LINK_NAME)) {
                if (key.length() > LINK_NAME.length()) {
                    // get prefix from link name
                    String linkname = key.substring(0, key.length() - LINK_NAME.length());
                    String name = (String) properties.getOrDefault(linkname + LINK_NAME, "");
                    String url = (String) properties.getOrDefault(linkname + LINK_URL, "");
                    String imageUrl = (String) properties.getOrDefault(linkname + LINK_IMAGEURL, "");
                    if (!name.isEmpty() && !url.isEmpty()) {
                        TileBuilder builder = new ExternalServiceTile.TileBuilder().withName(name).withUrl(url);
                        if (!imageUrl.isEmpty()) {
                            builder = builder.withImageUrl(imageUrl);
                        Tile newTile = builder.build();
                        addTile(newTile);
                        logger.debug("Tile added: {}", newTile);
                        logger.warn("Ignore invalid tile '{}': {}", linkname, name);
