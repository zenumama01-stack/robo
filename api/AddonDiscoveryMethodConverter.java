package org.openhab.core.addon.internal.xml;
import org.openhab.core.addon.AddonDiscoveryMethod;
import org.openhab.core.addon.AddonMatchProperty;
import org.openhab.core.addon.AddonParameter;
import org.openhab.core.config.core.xml.util.GenericUnmarshaller;
import org.openhab.core.config.core.xml.util.NodeIterator;
import com.thoughtworks.xstream.converters.UnmarshallingContext;
import com.thoughtworks.xstream.io.HierarchicalStreamReader;
 * The {@link AddonDiscoveryMethodConverter} is a concrete implementation of the {@code XStream}
 * {@link com.thoughtworks.xstream.converters.Converter}
 * interface used to convert add-on discovery method information within an XML document into a
 * {@link AddonDiscoveryMethod} object.
public class AddonDiscoveryMethodConverter extends GenericUnmarshaller<AddonDiscoveryMethod> {
    public AddonDiscoveryMethodConverter() {
        super(AddonDiscoveryMethod.class);
    public @Nullable Object unmarshal(HierarchicalStreamReader reader, UnmarshallingContext context) {
        List<?> nodes = (List<?>) context.convertAnother(context, List.class);
        NodeIterator nodeIterator = new NodeIterator(nodes);
        String serviceType = requireNonEmpty((String) nodeIterator.nextValue("service-type", true),
                "Service type is null or empty");
        Object paramObject = nodeIterator.nextList("discovery-parameters", false);
        List<AddonParameter> parameters = !(paramObject instanceof List<?> list) ? null
                : list.stream().filter(AddonParameter.class::isInstance).map(e -> ((AddonParameter) e)).toList();
        Object matchPropObject = nodeIterator.nextList("match-properties", false);
        List<AddonMatchProperty> matchProperties = !(matchPropObject instanceof List<?> list) ? null
                : list.stream().filter(AddonMatchProperty.class::isInstance).map(e -> ((AddonMatchProperty) e))
        nodeIterator.assertEndOfType();
        return new AddonDiscoveryMethod().setServiceType(serviceType).setParameters(parameters)
                .setMatchProperties(matchProperties);
