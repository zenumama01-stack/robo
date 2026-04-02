package org.openhab.core.sitemap;
 * A representation of a sitemap {@link Button} widget. Button widgets should have a parent {@link Buttongrid} widget.
public interface Button extends NonLinkableWidget {
     * Get button row in grid.
     * @return row
    int getRow();
     * Set button row in grid.
    void setRow(int row);
     * Get button column in grid.
     * @return column
    int getColumn();
     * Set button column in grid.
     * @param column
    void setColumn(int column);
     * True if the button is stateless, by default a button is stateful.
     * @return stateless
    boolean isStateless();
     * Set stateless parameter for button.
     * @param stateless
    void setStateless(@Nullable Boolean stateless);
     * Get button command, will be executed when the button is clicked.
     * @return cmd
    String getCmd();
     * Set button command.
     * @param cmd
    void setCmd(String cmd);
     * Get button release command, will be executed when the button is released.
     * @return releaseCmd
    String getReleaseCmd();
     * Set the button release command.
     * @param releaseCmd
    void setReleaseCmd(@Nullable String releaseCmd);
