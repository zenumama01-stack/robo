package org.openhab.core.model.core;
 * These are the event types that can occur as model repository changes
public enum EventType {
    MODIFIED
package org.openhab.core.types;
 * Due to the duality of some types (which can be states and commands at the
 * same time), we need to be able to differentiate what the meaning of a
 * message on the bus is - does "item ON" mean that its state has changed to
 * ON or that it should turn itself ON? To decide this, we send the event
 * type as an additional information on the event bus for each message.
    COMMAND("command"),
    UPDATE("update");
    EventType(String name) {
