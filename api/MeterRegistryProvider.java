package org.openhab.core.io.monitor;
import io.micrometer.core.instrument.composite.CompositeMeterRegistry;
 * The {@link MeterRegistryProvider} interface provides a means to retrieve the default OH meter registry instance
 * @author Robert Bach - Initial contribution
public interface MeterRegistryProvider {
    CompositeMeterRegistry getOHMeterRegistry();
