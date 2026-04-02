package org.openhab.core.automation.annotation;
import static java.lang.annotation.RetentionPolicy.RUNTIME;
import java.lang.annotation.ElementType;
import java.lang.annotation.Repeatable;
import java.lang.annotation.Retention;
import java.lang.annotation.Target;
 * Input parameter for an action module
@Repeatable(ActionInputs.class)
@Retention(RUNTIME)
@Target(ElementType.PARAMETER)
public @interface ActionInput {
    String name();
    String type() default "";
    String label() default "";
    String description() default "";
    String[] tags() default {};
    boolean required() default false;
    String reference() default "";
    String defaultValue() default "";
