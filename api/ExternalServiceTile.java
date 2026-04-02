package org.openhab.core.ui.tiles;
 * The dashboard tile for external services.
 * @author Yannick Schaus - moved into core, remove references to dashboard
public class ExternalServiceTile implements Tile {
    private @Nullable String overlay;
    private String imageUrl;
    private ExternalServiceTile(TileBuilder builder) {
        this.name = builder.name;
        this.url = builder.url;
        this.overlay = builder.overlay;
        this.imageUrl = builder.imageUrl;
    public @Nullable String getOverlay() {
        return overlay;
    public String getImageUrl() {
        return imageUrl;
        final int maxlen = 100;
        String limitedImageUrl = imageUrl;
        if (limitedImageUrl.length() > maxlen) {
            limitedImageUrl = imageUrl.substring(0, maxlen) + "...";
        return "[name=" + name + ", url=" + url + ", overlay=" + overlay + ", imageUrl=" + limitedImageUrl + "]";
    public static class TileBuilder {
        private String imageUrl = "";
        public TileBuilder withName(String name) {
        public TileBuilder withUrl(String url) {
        public TileBuilder withOverlay(@Nullable String overlay) {
        public TileBuilder withImageUrl(String imageUrl) {
        public ExternalServiceTile build() {
            return new ExternalServiceTile(this);
