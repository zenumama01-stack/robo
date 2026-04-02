package org.openhab.core.ui.icon.internal;
import java.nio.file.FileVisitOption;
import org.openhab.core.ui.icon.AbstractResourceIconProvider;
import org.openhab.core.ui.icon.IconProvider;
import org.openhab.core.ui.icon.IconSet;
 * The custom icon provider supports custom icons in the configurations/icons
 * folder. If a custom icon is found, it will be used over the standard system icon.
@Component(immediate = true, service = { IconProvider.class })
public class CustomIconProvider extends AbstractResourceIconProvider {
    public CustomIconProvider(final @Reference TranslationProvider i18nProvider) {
        super(i18nProvider);
    private @Nullable Path getIconFile(String filename, String iconSetId) {
        Path folder = Path.of(OpenHAB.getConfigFolder(), "icons", iconSetId);
        try (Stream<Path> stream = Files.walk(folder, FileVisitOption.FOLLOW_LINKS)) {
            return stream.filter(file -> !Files.isDirectory(file) && filename.equals(file.getFileName().toString()))
    protected @Nullable InputStream getResource(String iconSetId, String resourceName) {
        Path file = getIconFile(resourceName, iconSetId);
                return Files.newInputStream(file);
    protected boolean hasResource(String iconSetId, String resourceName) {
        return getIconFile(resourceName, iconSetId) != null;
    public Set<IconSet> getIconSets(@Nullable Locale locale) {
    protected Integer getPriority() {
