package org.openhab.core.io.rest.ui;
 * This is a data transfer object for a UI tile.
@Schema(name = "Tile")
public class TileDTO {
    public @Nullable String overlay;
    public TileDTO(String name, String url, @Nullable String overlay, String imageUrl) {
        this.overlay = overlay;
        this.imageUrl = imageUrl;
