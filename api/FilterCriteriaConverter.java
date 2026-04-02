 * The {@link FilterCriteriaConverter} creates a {@link FilterCriteria} instance
 * from a {@code criteria} XML node.
public class FilterCriteriaConverter extends GenericUnmarshaller<FilterCriteria> {
    public FilterCriteriaConverter() {
        super(FilterCriteria.class);
        String criteria = reader.getValue();
        return new FilterCriteria(name, criteria);
