import java.util.Vector;
 * Combines multiple class loaders into one.
public class CombinedClassLoader extends ClassLoader {
    private final Logger logger = LoggerFactory.getLogger(CombinedClassLoader.class);
    private final Map<ClassLoader, Set<Class<?>>> delegateClassLoaders;
    private static String info(final ClassLoader classLoader, final Set<Class<?>> clazzes) {
        return String.format("classloader \"%s\" for \"%s\"", classLoader, clazzes);
    public static CombinedClassLoader fromClasses(final ClassLoader parent, final Stream<Class<?>> delegateClasses) {
        final Map<ClassLoader, Set<Class<?>>> cls = new HashMap<>();
        delegateClasses.forEach(clazz -> {
            ClassLoader classLoader = clazz.getClassLoader();
                cls.compute(classLoader, (k, v) -> {
                    if (v == null) {
                        final Set<Class<?>> set = new HashSet<>();
                        set.add(clazz);
                        v.add(clazz);
        return new CombinedClassLoader(parent, cls);
    public static CombinedClassLoader fromClassLoaders(final ClassLoader parent,
            final ClassLoader... delegateClassLoaders) {
        return fromClassLoaders(parent, Arrays.stream(delegateClassLoaders));
            final Stream<ClassLoader> delegateClassLoaders) {
        return new CombinedClassLoader(parent,
                delegateClassLoaders.collect(Collectors.toMap(cl -> cl, cl -> Set.of())));
    private CombinedClassLoader(ClassLoader parent, Map<ClassLoader, Set<Class<?>>> delegateClassLoaders) {
        this.delegateClassLoaders = Collections.unmodifiableMap(delegateClassLoaders);
    protected Class<?> findClass(@Nullable String name) throws ClassNotFoundException {
            throw new ClassNotFoundException("Cannot load class with null name");
        for (final Entry<ClassLoader, Set<Class<?>>> entry : delegateClassLoaders.entrySet()) {
                final Class<?> clazz = entry.getKey().loadClass(name);
                    logger.debug("Loaded class \"{}\" by {}", name, info(entry.getKey(), entry.getValue()));
                return clazz;
            } catch (final ClassNotFoundException ex) {
        throw new ClassNotFoundException("Delegates cannot load class with name: " + name);
    protected @Nullable URL findResource(@Nullable String name) {
        // Try to get the resource from one of the delegate class loaders.
        // Return the first found one.
        // If no delegate class loader can get the resource, return null.
        return delegateClassLoaders.keySet().stream().map(cl -> cl.getResource(name)).filter(Objects::nonNull)
    protected Enumeration<URL> findResources(@Nullable String name) throws IOException {
            return Collections.emptyEnumeration();
        final Vector<URL> vector = new Vector<>();
        for (final ClassLoader delegate : delegateClassLoaders.keySet()) {
            final Enumeration<URL> enumeration = delegate.getResources(name);
                vector.add(enumeration.nextElement());
        return vector.elements();
