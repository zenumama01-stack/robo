import org.eclipse.xtext.scoping.impl.ImportNormalizer;
import org.eclipse.xtext.xbase.scoping.XImportSectionNamespaceScopeProvider;
public class ScriptImportSectionNamespaceScopeProvider extends XImportSectionNamespaceScopeProvider {
    public static final QualifiedName CORE_LIBRARY_UNITS_PACKAGE = QualifiedName.create("org", "openhab", "core",
            "library", "unit");
    public static final QualifiedName CORE_LIBRARY_TYPES_PACKAGE = QualifiedName.create("org", "openhab", "core",
            "library", "types");
    public static final QualifiedName CORE_LIBRARY_ITEMS_PACKAGE = QualifiedName.create("org", "openhab", "core",
            "library", "items");
    public static final QualifiedName CORE_ITEMS_PACKAGE = QualifiedName.create("org", "openhab", "core", "items");
    public static final QualifiedName CORE_PERSISTENCE_PACKAGE = QualifiedName.create("org", "openhab", "core",
            "persistence");
    public static final QualifiedName CORE_PERSISTENCE_RIEMANNTYPE_CLASS = QualifiedName.create("org", "openhab",
            "core", "persistence", "extensions", "PersistenceExtensions", "RiemannType");
    public static final QualifiedName MODEL_SCRIPT_ACTIONS_PACKAGE = QualifiedName.create("org", "openhab", "core",
            "model", "script", "actions");
    public static final QualifiedName TIME_PACKAGE = QualifiedName.create("java", "time");
    public static final QualifiedName CHRONOUNIT_CLASS = QualifiedName.create("java", "time", "temporal", "ChronoUnit");
    public static final QualifiedName QUANTITY_PACKAGE = QualifiedName.create("javax", "measure", "quantity");
    protected List<ImportNormalizer> getImplicitImports(boolean ignoreCase) {
        List<ImportNormalizer> implicitImports = super.getImplicitImports(ignoreCase);
        implicitImports.add(doCreateImportNormalizer(CORE_LIBRARY_UNITS_PACKAGE, true, false));
        implicitImports.add(doCreateImportNormalizer(CORE_LIBRARY_TYPES_PACKAGE, true, false));
        implicitImports.add(doCreateImportNormalizer(CORE_LIBRARY_ITEMS_PACKAGE, true, false));
        implicitImports.add(doCreateImportNormalizer(CORE_ITEMS_PACKAGE, true, false));
        implicitImports.add(doCreateImportNormalizer(CORE_PERSISTENCE_PACKAGE, true, false));
        implicitImports.add(doCreateImportNormalizer(CORE_PERSISTENCE_RIEMANNTYPE_CLASS, false, false));
        implicitImports.add(doCreateImportNormalizer(MODEL_SCRIPT_ACTIONS_PACKAGE, true, false));
        implicitImports.add(doCreateImportNormalizer(TIME_PACKAGE, true, false));
        implicitImports.add(doCreateImportNormalizer(CHRONOUNIT_CLASS, false, false));
        implicitImports.add(doCreateImportNormalizer(QUANTITY_PACKAGE, true, false));
        return implicitImports;
