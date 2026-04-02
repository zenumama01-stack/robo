package org.openhab.core.magic.binding.internal.automation.modules;
 * Marker for the multiple action module
@Component(immediate = true, service = MagicMultiActionMarker.class, //
        property = Constants.SERVICE_PID + "=org.openhab.MagicMultiAction")
@ConfigurableService(category = "RuleActions", label = "Magic Multi Actions Service", description_uri = "automationAction:magicMultiAction", factory = true)
public class MagicMultiActionMarker {
