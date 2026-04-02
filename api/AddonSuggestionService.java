import org.osgi.service.component.annotations.ReferencePolicyOption;
 * This is a {@link AddonSuggestionService} which discovers suggested add-ons for the user to install.
 * @author Mark Herwege - Install/remove finders
@Component(immediate = true, service = AddonSuggestionService.class, name = AddonSuggestionService.SERVICE_NAME, configurationPid = OpenHAB.ADDONS_SERVICE_PID)
public class AddonSuggestionService {
    public static final String SERVICE_NAME = "addon-suggestion-service";
    private final Logger logger = LoggerFactory.getLogger(AddonSuggestionService.class);
    private final Set<AddonInfoProvider> addonInfoProviders = ConcurrentHashMap.newKeySet();
    // All access must be guarded by "addonFinders"
    private final List<AddonFinder> addonFinders = new ArrayList<>();
    private volatile @Nullable AddonFinderService addonFinderService;
    private final Map<String, Boolean> baseFinderConfig = new ConcurrentHashMap<>();
    public AddonSuggestionService(@Reference LocaleProvider localeProvider, Map<String, Object> config) {
        SUGGESTION_FINDERS.forEach(f -> baseFinderConfig.put(f, false));
        synchronized (addonFinders) {
            addonFinders.clear();
        addonInfoProviders.clear();
    @Reference(cardinality = ReferenceCardinality.OPTIONAL, policy = ReferencePolicy.DYNAMIC, policyOption = ReferencePolicyOption.GREEDY)
    protected void addAddonFinderService(AddonFinderService addonFinderService) {
        this.addonFinderService = addonFinderService;
        initAddonFinderService();
    protected void removeAddonFinderService(AddonFinderService addonFinderService) {
        AddonFinderService finderService = this.addonFinderService;
        if ((finderService != null) && addonFinderService.getClass().isAssignableFrom(finderService.getClass())) {
            this.addonFinderService = null;
    public void modified(@Nullable final Map<String, Object> config) {
            AddonFinderService finderService = addonFinderService;
            baseFinderConfig.forEach((finder, currentEnabled) -> {
                String cfgParam = SUGGESTION_FINDER_CONFIGS.get(finder);
                if (cfgParam != null) {
                    boolean newEnabled = ConfigParser.valueAsOrElse(config.get(cfgParam), Boolean.class, true);
                    if (currentEnabled != newEnabled) {
                        String type = SUGGESTION_FINDER_TYPES.get(finder);
                        if (type != null) {
                            logger.debug("baseFinderConfig {} {} = {} => updating from {} to {}", finder, cfgParam,
                                    config.get(cfgParam), currentEnabled, newEnabled);
                            baseFinderConfig.put(finder, newEnabled);
                            if (finderService != null) {
                                if (newEnabled) {
                                    finderService.install(type);
                                    finderService.uninstall(type);
                            logger.warn("Failed to resolve addon suggestion finder type for suggestion finder {}",
                                    finder);
    private void initAddonFinderService() {
        if (finderService == null) {
        for (Entry<String, Boolean> entry : baseFinderConfig.entrySet()) {
            type = SUGGESTION_FINDER_TYPES.get(entry.getKey());
                if (entry.getValue() instanceof Boolean enabled) {
                    if (enabled) {
                logger.warn("Failed to resolve addon suggestion finder type for suggestion finder {}", entry.getKey());
    private boolean isFinderEnabled(AddonFinder finder) {
        if (finder instanceof BaseAddonFinder baseFinder) {
            return baseFinderConfig.getOrDefault(baseFinder.getServiceName(), true);
    public void addAddonInfoProvider(AddonInfoProvider addonInfoProvider) {
        changed();
        if (addonInfoProviders.remove(addonInfoProvider)) {
    public void addAddonFinder(AddonFinder addonFinder) {
            addonFinders.add(addonFinder);
    public void removeAddonFinder(AddonFinder addonFinder) {
            addonFinders.remove(addonFinder);
    private void changed() {
        List<AddonInfo> candidates = addonInfoProviders.stream().map(p -> p.getAddonInfos(locale))
                .flatMap(Collection::stream).toList();
            addonFinders.stream().filter(this::isFinderEnabled).forEach(f -> f.setAddonCandidates(candidates));
    public Set<AddonInfo> getSuggestedAddons(@Nullable Locale locale) {
            return addonFinders.stream().filter(this::isFinderEnabled).map(f -> f.getSuggestedAddons())
                    .flatMap(Collection::stream).collect(Collectors.toSet());
