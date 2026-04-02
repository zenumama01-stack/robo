import org.openhab.core.items.ItemBuilderFactory;
import org.openhab.core.items.dto.ItemDTOMapper;
 * The {@link FileFormatItemDTOMapper} is a utility class to map items into file format item data transfer objects
 * (DTOs).
public class FileFormatItemDTOMapper {
     * Maps item into file format item DTO object.
     * @param item the item
     * @param metadata some metadata
     * @param format the format to be used to format the item state, can be NULL
     * @param channelLinks some items channel links
     * @return file format item DTO object
    public static FileFormatItemDTO map(Item item, Collection<Metadata> metadata, @Nullable String format,
            Collection<ItemChannelLink> channelLinks) {
        ItemDTO itemDto = ItemDTOMapper.map(item);
        FileFormatItemDTO dto = new FileFormatItemDTO(itemDto, itemDto instanceof GroupItemDTO);
        dto.format = format;
        Map<String, MetadataDTO> metadataDTO = new LinkedHashMap<>();
            if (item.getName().equals(md.getUID().getItemName())) {
                MetadataDTO mdDTO = new MetadataDTO();
                mdDTO.value = md.getValue();
                mdDTO.config = md.getConfiguration().isEmpty() ? null : md.getConfiguration();
                metadataDTO.put(md.getUID().getNamespace(), mdDTO);
        if (!metadataDTO.isEmpty()) {
            dto.metadata = metadataDTO;
        List<FileFormatChannelLinkDTO> channelLinksDTO = new ArrayList<>();
        channelLinks.forEach(link -> {
            if (item.getName().equals(link.getItemName())) {
                channelLinksDTO.add(new FileFormatChannelLinkDTO(link.getLinkedUID().getAsString(),
                        link.getConfiguration().getProperties().isEmpty() ? null
                                : link.getConfiguration().getProperties()));
        if (!channelLinksDTO.isEmpty()) {
            dto.channelLinks = channelLinksDTO;
        return dto;
     * Maps file format item DTO object into item.
     * @param dto the file format item DTO object
     * @param itemBuilderFactory the item builder factory
     * @return item
    public static @Nullable Item map(FileFormatItemDTO dto, ItemBuilderFactory itemBuilderFactory) {
        if (GroupItem.TYPE.equals(dto.type)) {
            GroupItemDTO groupDto = new GroupItemDTO();
            groupDto.type = dto.type;
            groupDto.name = dto.name;
            groupDto.label = dto.label;
            groupDto.category = dto.category;
            groupDto.tags = dto.tags;
            groupDto.groupNames = dto.groupNames;
            groupDto.groupType = dto.groupType;
            groupDto.function = dto.function;
            return ItemDTOMapper.map(groupDto, itemBuilderFactory);
        return ItemDTOMapper.map(dto, itemBuilderFactory);
     * Maps file format item DTO object into a collection of metadata including channels links
     * provided through the "channel" namespace.
     * @return the collection of metadata
    public static Collection<Metadata> mapMetadata(FileFormatItemDTO dto) {
        String name = dto.name;
        Collection<Metadata> metadata = new ArrayList<>();
        if (dto.channelLinks != null) {
            for (FileFormatChannelLinkDTO link : dto.channelLinks) {
                MetadataKey key = new MetadataKey("channel", name);
                metadata.add(new Metadata(key, link.channelUID, link.configuration));
        if (dto.metadata != null) {
            for (Map.Entry<String, MetadataDTO> md : dto.metadata.entrySet()) {
                MetadataKey key = new MetadataKey(md.getKey(), name);
                metadata.add(new Metadata(key, Objects.requireNonNull(md.getValue().value), md.getValue().config));
