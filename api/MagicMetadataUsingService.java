 * Example service which makes use of the metadata of the "magic" namespace.
public class MagicMetadataUsingService {
    private final Logger logger = LoggerFactory.getLogger(MagicMetadataUsingService.class);
    private final ScheduledExecutorService scheduler = ThreadPoolManager.getScheduledPool("magic");
    public MagicMetadataUsingService(final @Reference MetadataRegistry metadataRegistry) {
        job = scheduler.scheduleWithFixedDelay(() -> run(), 30, 30, TimeUnit.SECONDS);
            job = null;
    private void run() {
        metadataRegistry.stream().filter(MetadataPredicates.hasNamespace("magic")).forEach(metadata -> {
            logger.info("Item {} is {} with {}", metadata.getUID().getItemName(), metadata.getValue(),
                    metadata.getConfiguration());
