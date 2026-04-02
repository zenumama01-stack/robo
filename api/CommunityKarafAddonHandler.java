package org.openhab.core.addon.marketplace.karaf.internal.community;
import static org.openhab.core.addon.marketplace.MarketplaceConstants.*;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.net.MalformedURLException;
import java.net.URISyntaxException;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardCopyOption;
import java.util.concurrent.ScheduledExecutorService;
import java.util.stream.Stream;
import org.apache.karaf.kar.KarService;
import org.openhab.core.OpenHAB;
import org.openhab.core.addon.marketplace.MarketplaceAddonHandler;
import org.openhab.core.addon.marketplace.MarketplaceHandlerException;
import org.openhab.core.common.ThreadPoolManager;
import org.openhab.core.util.UIDUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
 * A {@link CommunityKarafAddonHandler} implementation, which handles add-ons as KAR files and installs them
 * using the {@link KarService}.
 * @author Kai Kreuzer - Initial contribution and API
 * @author Yannick Schaus - refactoring
 * @author Jan N. Klug - refactor to support kar files
@Component(immediate = true)
public class CommunityKarafAddonHandler implements MarketplaceAddonHandler {
    private static final Path KAR_CACHE_PATH = Path.of(OpenHAB.getUserDataFolder(), "marketplace", "kar");
    private static final List<String> SUPPORTED_EXT_TYPES = List.of("automation", "binding", "misc", "persistence",
            "transformation", "ui", "voice");
    private static final String KAR_EXTENSION = ".kar";
    private final Logger logger = LoggerFactory.getLogger(CommunityKarafAddonHandler.class);
    private final ScheduledExecutorService scheduler = ThreadPoolManager
            .getScheduledPool(ThreadPoolManager.THREAD_POOL_NAME_COMMON);
    private final KarService karService;
    private boolean isReady = false;
    public CommunityKarafAddonHandler(@Reference KarService karService) {
        this.karService = karService;
        scheduler.execute(this::ensureCachedKarsAreInstalled);
    public boolean supports(String type, String contentType) {
        return SUPPORTED_EXT_TYPES.contains(type) && KAR_CONTENT_TYPE.equals(contentType);
    private Stream<Path> karFilesStream(Path addonDirectory) throws IOException {
        if (Files.isDirectory(addonDirectory)) {
            try (Stream<Path> files = Files.list(addonDirectory)) {
                return files.map(Path::getFileName).filter(path -> path.toString().endsWith(KAR_EXTENSION)).toList()
                        .stream();
        return Stream.empty();
    private String pathToKarRepoName(Path path) {
        String fileName = path.getFileName().toString();
        return fileName.substring(0, fileName.length() - KAR_EXTENSION.length());
    @SuppressWarnings("null")
    public boolean isInstalled(String addonId) {
            Path addonDirectory = getAddonCacheDirectory(addonId);
            List<String> repositories = karService.list();
            return karFilesStream(addonDirectory).findFirst().map(this::pathToKarRepoName).map(repositories::contains)
                    .orElse(false);
        } catch (Exception e) {
            logger.warn("Failed to determine installation status for {}: ", addonId, e);
    public void install(Addon addon) throws MarketplaceHandlerException {
        URL sourceUrl;
            sourceUrl = (new URI((String) addon.getProperties().get(KAR_DOWNLOAD_URL_PROPERTY))).toURL();
        } catch (IllegalArgumentException | MalformedURLException | URISyntaxException e) {
            throw new MarketplaceHandlerException("Malformed source URL: " + e.getMessage(), e);
        addKarToCache(addon.getUid(), sourceUrl);
        installFromCache(addon.getUid());
    public void uninstall(Addon addon) throws MarketplaceHandlerException {
            Path addonPath = getAddonCacheDirectory(addon.getUid());
            for (Path path : karFilesStream(addonPath).toList()) {
                String karRepoName = pathToKarRepoName(path);
                if (repositories.contains(karRepoName)) {
                    karService.uninstall(karRepoName);
                Files.delete(addonPath.resolve(path));
            Files.delete(addonPath);
            throw new MarketplaceHandlerException("Failed uninstalling KAR: " + e.getMessage(), e);
     * Downloads a KAR file from a remote source and puts it in the local cache with the add-on ID.
     * @param addonId the add-on ID
     * @param sourceUrl the (online) source where the KAR file can be found
     * @throws MarketplaceHandlerException on error
    private void addKarToCache(String addonId, URL sourceUrl) throws MarketplaceHandlerException {
            String fileName = new File(sourceUrl.toURI().getPath()).getName();
            Path addonFile = getAddonCacheDirectory(addonId).resolve(fileName);
            Files.createDirectories(addonFile.getParent());
            InputStream source = sourceUrl.openStream();
            Files.copy(source, addonFile, StandardCopyOption.REPLACE_EXISTING);
        } catch (IOException | URISyntaxException e) {
            throw new MarketplaceHandlerException("Cannot copy KAR to local cache: " + e.getMessage(), e);
    private void installFromCache(String addonId) throws MarketplaceHandlerException {
        Path addonPath = getAddonCacheDirectory(addonId);
        if (Files.isDirectory(addonPath)) {
            try (Stream<Path> files = Files.list(addonPath)) {
                List<Path> karFiles = files.toList();
                if (karFiles.size() != 1) {
                    throw new MarketplaceHandlerException(
                            "The local cache folder doesn't contain a single file: " + addonPath, null);
                    karService.install(karFiles.getFirst().toUri(), false);
                            "Cannot install KAR from marketplace cache: " + e.getMessage(), e);
            } catch (IOException e) {
                throw new MarketplaceHandlerException("Could not list files in cache directory " + addonPath, e);
    private void ensureCachedKarsAreInstalled() {
        if (Files.isDirectory(KAR_CACHE_PATH)) {
            try (Stream<Path> files = Files.list(KAR_CACHE_PATH)) {
                files.filter(Files::isDirectory).map(this::addonIdFromPath).filter(addonId -> !isInstalled(addonId))
                        .forEach(addonId -> {
                            logger.info("Reinstalling missing marketplace KAR: {}", addonId);
                                installFromCache(addonId);
                            } catch (MarketplaceHandlerException e) {
                                logger.warn("Failed reinstalling add-on from cache", e);
                logger.warn("Failed to re-install KARs: {}", e.getMessage());
        isReady = true;
    private String addonIdFromPath(Path path) {
        String pathName = UIDUtils.decode(path.getFileName().toString());
        return pathName.contains(":") ? pathName : "marketplace:" + pathName;
    private Path getAddonCacheDirectory(String addonId) {
        String dir = addonId.startsWith("marketplace:") ? addonId.replace("marketplace:", "")
                : UIDUtils.encode(addonId);
        return KAR_CACHE_PATH.resolve(dir);
    public boolean isReady() {
        return isReady;
