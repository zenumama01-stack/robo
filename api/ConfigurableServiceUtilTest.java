import static org.hamcrest.text.IsEmptyString.emptyString;
import static org.openhab.core.config.core.ConfigurableServiceUtil.*;
import java.util.Properties;
 * Tests {@link ConfigurableServiceUtil}.
public class ConfigurableServiceUtilTest {
    public void asConfigurableServiceDefinedProperties() {
        String category = "system";
        String descriptionURI = "system:inbox";
        boolean factory = true;
        String label = "Inbox";
        Properties properties = new Properties();
        properties.put(SERVICE_PROPERTY_CATEGORY, category);
        properties.put(SERVICE_PROPERTY_DESCRIPTION_URI, descriptionURI);
        properties.put(SERVICE_PROPERTY_FACTORY_SERVICE, factory);
        properties.put(SERVICE_PROPERTY_LABEL, label);
        ConfigurableService configurableService = ConfigurableServiceUtil.asConfigurableService(properties::get);
        assertThat(configurableService.annotationType(), is(ConfigurableService.class));
        assertThat(configurableService.category(), is(category));
        assertThat(configurableService.description_uri(), is(descriptionURI));
        assertThat(configurableService.factory(), is(factory));
        assertThat(configurableService.label(), is(label));
    public void asConfigurableServiceUndefinedProperties() {
        assertThat(configurableService.category(), is(emptyString()));
        assertThat(configurableService.description_uri(), is(emptyString()));
        assertThat(configurableService.factory(), is(false));
        assertThat(configurableService.label(), is(emptyString()));
