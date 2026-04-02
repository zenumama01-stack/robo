package org.openhab.core.automation.module.script.rulesupport.shared.factories;
import org.openhab.core.automation.handler.ActionHandler;
public interface ScriptedActionHandlerFactory extends ScriptedHandler {
    ActionHandler get(Action action);
