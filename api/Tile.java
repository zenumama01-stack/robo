 * A tile can be registered by a UI as a service in order to appear on the main openHAB UI.
 * @author Kai Kreuzer - initial contribution
 * @author Yannick Schaus - refactored into core, remove references to dashboard
public interface Tile {
     * The name that should appear on the tile
     * @return name of the tile
     * The url to point to (if it is a local UI, it should be a relative path starting with "../")
     * @return the url
     * The url to point to for the tile.
     * (if it is a local UI, it should be a relative path starting with "../")
     * @return the tile url
    String getImageUrl();
     * An HTML5 overlay icon to use for the tile, e.g. "html5", "android" or "apple".
     * @return the overlay to use
    String getOverlay();
