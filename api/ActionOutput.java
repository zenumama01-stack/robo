import static org.openhab.core.automation.internal.module.handler.AnnotationActionHandler.MODULE_RESULT;
 * Output parameter for an action module
@Repeatable(ActionOutputs.class)
@Target(ElementType.METHOD)
public @interface ActionOutput {
     * Name of the output parameter
     * There are some reserved names that make the UI render the output pretty and not just as a text field:
     * <li>qrCode: Render the output as a QR code.</li>
     * @return the name of the output parameter
    String name() default MODULE_RESULT;
     * Type of the output parameter
     * There are some special types that make the UI render the output pretty and not just as a text field:
     * @return the type of the output parameter
    String type();
