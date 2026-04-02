 * Console command completer for send and update
public class ItemConsoleCommandCompleter implements ConsoleCommandCompleter {
    private final @Nullable Function<Item, Class<?>[]> dataTypeGetter;
    public ItemConsoleCommandCompleter(ItemRegistry itemRegistry) {
        this.dataTypeGetter = null;
    public ItemConsoleCommandCompleter(ItemRegistry itemRegistry, Function<Item, Class<?>[]> dataTypeGetter) {
        this.dataTypeGetter = dataTypeGetter;
            return new StringsCompleter(itemRegistry.getAll().stream().map(Item::getName).toList(), true).complete(args,
                    cursorArgumentIndex, cursorPosition, candidates);
        var localDataTypeGetter = dataTypeGetter;
        if (cursorArgumentIndex == 1 && localDataTypeGetter != null) {
                Item item = itemRegistry.getItemByPattern(args[0]);
                Stream<Class<?>> enums = Stream.of(localDataTypeGetter.apply(item)).filter(Class::isEnum);
                Stream<? super Enum<?>> enumConstants = enums.flatMap(
                        t -> Stream.of(Objects.requireNonNull(((Class<? extends Enum<?>>) t).getEnumConstants())));
                return new StringsCompleter(enumConstants.map(Object::toString).toList(), false).complete(args,
            } catch (ItemNotFoundException | ItemNotUniqueException e) {
