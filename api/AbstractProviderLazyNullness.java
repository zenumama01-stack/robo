 * This class can be used as workaround if you want to create a sub-class of an abstract provider but you are not able
 * to annotate the generic type argument with a non-null annotation. This is for example the case if you implement the
 * class by Xtend.
 * @param <E> type of the provided elements
public abstract class AbstractProviderLazyNullness<E> extends AbstractProvider<@NonNull E> {
