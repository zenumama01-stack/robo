import org.eclipse.xtext.common.types.util.JavaReflectAccess;
 * This is a customized version of {@link JavaReflectAccess}.
 * It allows for removing and updating classes in the cache used by the {@link RulesClassFinder} when add-ons are
 * installed or updated.
public class RulesJavaReflectAccess extends JavaReflectAccess {
    private ClassLoader classLoader = getClass().getClassLoader();
    private ClassFinder classFinder;
    @Inject(optional = true)
    public void setClassLoader(ClassLoader classLoader) {
        if (classLoader != this.classLoader) {
            classFinder = null;
    public ClassFinder getClassFinder() {
        if (classFinder == null) {
            classFinder = new RulesClassFinder(classLoader);
        return classFinder;
