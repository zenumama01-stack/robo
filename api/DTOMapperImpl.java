 * {@link DTOMapper} implementation.
public class DTOMapperImpl implements DTOMapper {
    private final Logger logger = LoggerFactory.getLogger(DTOMapperImpl.class);
    public <@NonNull T> Stream<T> limitToFields(Stream<T> itemStream, @Nullable String fields) {
        if (fields == null || fields.trim().isEmpty()) {
            return itemStream;
        List<String> fieldList = Stream.of(fields.split(",")).map(String::trim).toList();
        return itemStream.map(dto -> {
            for (Field field : dto.getClass().getFields()) {
                if (!fieldList.contains(field.getName())) {
                        field.set(dto, null);
                    } catch (IllegalArgumentException | IllegalAccessException e) {
                        logger.warn("Field '{}' could not be eliminated: {}", field.getName(), e.getMessage());
