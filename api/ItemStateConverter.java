public interface ItemStateConverter {
     * Convert the given {@link State} to a state which is acceptable for the given {@link Item}.
     * @param state the {@link State} to be converted.
     * @param item the {@link Item} for which the given state will be converted.
     * @return the converted {@link State} according to an accepted State of the given Item. Will return the original
     *         state in case item was {@code null}.
    State convertToAcceptedState(@Nullable State state, @Nullable Item item);
