import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;
public class EnrichedItemDTOMapperTest extends JavaTest {
        setupInterceptedLogger(EnrichedItemDTOMapper.class, LogLevel.DEBUG);
    public void testFiltering() {
        CoreItemFactory itemFactory = new CoreItemFactory(mock(UnitProvider.class));
        GroupItem group = new GroupItem("TestGroup");
        GroupItem subGroup = new GroupItem("TestSubGroup");
        GenericItem switchItem = itemFactory.createItem(CoreItemFactory.SWITCH, "TestSwitch");
        GenericItem numberItem = itemFactory.createItem(CoreItemFactory.NUMBER, "TestNumber");
        GenericItem stringItem = itemFactory.createItem(CoreItemFactory.STRING, "TestString");
        if (switchItem != null && numberItem != null && stringItem != null) {
            group.addMember(subGroup);
            group.addMember(switchItem);
            group.addMember(numberItem);
            subGroup.addMember(stringItem);
        EnrichedGroupItemDTO dto = (EnrichedGroupItemDTO) EnrichedItemDTOMapper.map(group, false, null, null, null,
        assertThat(dto.members.length, is(0));
        dto = (EnrichedGroupItemDTO) EnrichedItemDTOMapper.map(group, true, null, null, null, null);
        assertThat(dto.members.length, is(3));
        assertThat(((EnrichedGroupItemDTO) dto.members[0]).members.length, is(1));
        dto = (EnrichedGroupItemDTO) EnrichedItemDTOMapper.map(group, true,
                i -> CoreItemFactory.NUMBER.equals(i.getType()), null, null, null);
        assertThat(dto.members.length, is(1));
                i -> CoreItemFactory.NUMBER.equals(i.getType()) || i instanceof GroupItem, null, null, null);
        assertThat(dto.members.length, is(2));
        assertThat(((EnrichedGroupItemDTO) dto.members[0]).members.length, is(0));
                i -> CoreItemFactory.NUMBER.equals(i.getType()) || i.getType().equals(CoreItemFactory.STRING)
                        || i instanceof GroupItem,
    public void testDirectRecursiveMembershipDoesNotThrowStackOverflowException() {
        GroupItem groupItem1 = new GroupItem("group1");
        GroupItem groupItem2 = new GroupItem("group2");
        groupItem1.addMember(groupItem2);
        groupItem2.addMember(groupItem1);
        assertDoesNotThrow(() -> EnrichedItemDTOMapper.map(groupItem1, true, null, null, null, null));
        assertLogMessage(EnrichedItemDTOMapper.class, LogLevel.ERROR,
                "Recursive group membership found: group1 is a member of group2, but it is also one of its ancestors.");
    public void testIndirectRecursiveMembershipDoesNotThrowStackOverflowException() {
        GroupItem groupItem3 = new GroupItem("group3");
        groupItem2.addMember(groupItem3);
        groupItem3.addMember(groupItem1);
                "Recursive group membership found: group1 is a member of group3, but it is also one of its ancestors.");
    public void testDuplicateMembershipOfPlainItemsDoesNotTriggerWarning() {
        NumberItem numberItem = new NumberItem("number");
        groupItem1.addMember(numberItem);
        groupItem2.addMember(numberItem);
        EnrichedItemDTOMapper.map(groupItem1, true, null, null, null, null);
        assertNoLogMessage(EnrichedItemDTOMapper.class);
    public void testDuplicateMembershipOfGroupItemsDoesNotTriggerWarning() {
        groupItem1.addMember(groupItem3);
