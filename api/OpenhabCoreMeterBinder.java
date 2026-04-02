import io.micrometer.core.instrument.binder.MeterBinder;
 * The {@link OpenhabCoreMeterBinder} interface provides an abstraction of the OH default metrics
public interface OpenhabCoreMeterBinder extends MeterBinder {
    void unbind();
