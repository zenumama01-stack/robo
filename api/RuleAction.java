 * Marker annotation for an action module
public @interface RuleAction {
    String label();
    Visibility visibility() default Visibility.VISIBLE;
