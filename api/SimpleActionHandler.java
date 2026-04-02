package org.openhab.core.automation.module.script.rulesupport.shared.simple;
public abstract class SimpleActionHandler implements ScriptedHandler {
    public void init(Action module) {
    public abstract Object execute(Action module, Map<String, ?> inputs);
