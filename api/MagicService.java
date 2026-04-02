 * A public interface for a service from this virtual bundle which is also a {@link ConfigOptionProvider}.
public interface MagicService extends ConfigOptionProvider {
    URI CONFIG_URI = URI.create("test:magic");
