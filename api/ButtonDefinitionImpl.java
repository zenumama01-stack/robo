package org.openhab.core.sitemap.internal;
public class ButtonDefinitionImpl implements ButtonDefinition {
    private int row;
    private int column;
    private String cmd = "";
    private String label = "";
    private @Nullable String icon;
    public int getRow() {
    public void setRow(int row) {
    public int getColumn() {
    public void setColumn(int column) {
    public String getCmd() {
    public void setCmd(String cmd) {
    public @Nullable String getIcon() {
    public void setIcon(@Nullable String icon) {
        this.icon = icon;
