package org.openhab.core.automation;
 * This interface represents automation {@code Action} modules which are the expected result of {@link Rule}s execution.
 * They describe the actual work that should be performed by the Rule as a response to a trigger.
 * Each Action can provide information to the next Actions in the list through its {@link Output}s. The actions have
 * {@link Input}s to process input data from other Actions or {@link Trigger}s.
 * Actions can be configured.
 * The building elements of the Actions are {@link ConfigDescriptionParameter}s, {@link Input}s and {@link Output}s.
 * They are defined by the corresponding {@link ActionType}.
 * Action modules are placed in the <b>actions</b> section of the {@link Rule} definition.
 * @author Yordan Mihaylov - Initial contribution
 * @author Ana Dimova - Initial contribution
 * @author Vasil Ilchev - Initial contribution
public interface Action extends Module {
     * Gets the input references of the Action. The references define how the {@link Input}s of this {@link Module} are
     * connected to {@link Output}s of other {@link Module}s.
     * @return a map with the input references of this action.
    Map<String, String> getInputs();
