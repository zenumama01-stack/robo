import org.eclipse.xtext.common.types.access.impl.ClassFinder;
import org.eclipse.xtext.common.types.access.impl.ClassNameUtil;
 * This is a customized version of the {@link ClassFinder}.
public class RulesClassFinder extends ClassFinder {
    private static class Null {
    private static final Class<?> NULL_CLASS = Null.class;
    private final ClassLoader classLoader;
    private final ClassNameUtil classNameUtil = new ClassNameUtil();
    protected RulesClassFinder(ClassLoader classLoader) {
        super(classLoader);
        this.classLoader = classLoader;
    public Class<?> forName(String name) throws ClassNotFoundException {
        RulesClassCache cache = RulesClassCache.getInstance();
        Class<?> result = cache.get(name);
            if (result == NULL_CLASS) {
                throw CACHED_EXCEPTION;
            result = forName(classNameUtil.normalizeClassName(name), classLoader);
            cache.put(name, result);
            cache.put(name, NULL_CLASS);
