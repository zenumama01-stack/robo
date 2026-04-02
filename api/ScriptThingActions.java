package org.openhab.core.automation.module.script.defaultscope;
import org.openhab.core.thing.binding.ThingActions;
 * The methods of this class are made available as functions in the scripts.
 * Note: This class is a copy from the {@code org.openhab.core.model.script.internal.engine.action.ThingActionService}
 * class
 * @author Jan N. Klug - Refactored to interface
public interface ScriptThingActions {
     * Gets an actions instance of a certain scope for a given thing UID
     * @param scope the action scope
     * @param thingUid the UID of the thing
     * @return actions the actions instance or null, if not available
    ThingActions get(@Nullable String scope, @Nullable String thingUid);
