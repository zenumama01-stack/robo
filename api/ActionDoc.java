package org.openhab.core.model.script.engine.action;
import java.lang.annotation.Inherited;
@Inherited
public @interface ActionDoc {
    String text();
    String returns() default "";
