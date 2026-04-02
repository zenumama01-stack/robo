 * The {@link RenamingTypeMigrator} is a {@link TypeMigrator} for renaming types
public class RenamingTypeMigrator implements TypeMigrator {
    private final String oldType;
    private final String newType;
    public RenamingTypeMigrator(String oldType, String newType) {
        this.oldType = oldType;
        this.newType = newType;
        return oldType;
        return newType;
