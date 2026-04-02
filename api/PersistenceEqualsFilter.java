package org.openhab.core.persistence.filter;
 * The {@link PersistenceEqualsFilter} is a filter that allows only specific values to pass
 * The filter returns {@code false} if the string representation of the item's state is not in the given list
public class PersistenceEqualsFilter extends PersistenceFilter {
    private final Collection<String> values;
    private final boolean inverted;
    public PersistenceEqualsFilter(String name, Collection<String> values, @Nullable Boolean inverted) {
        this.inverted = inverted != null && inverted;
    public Collection<String> getValues() {
    public boolean getInverted() {
        return inverted;
    public boolean apply(Item item) {
        return values.contains(item.getState().toFullString()) != inverted;
    public void persisted(Item item) {
        return String.format("%s [name=%s, value=%s, inverted=]", getClass().getSimpleName(), getName(), values);
