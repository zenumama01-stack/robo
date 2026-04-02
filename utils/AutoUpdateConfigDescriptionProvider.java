package org.openhab.core.thing.internal;
 * Provider of the config description for the auto update policy metadata.
public class AutoUpdateConfigDescriptionProvider implements MetadataConfigDescriptionProvider {
        return "autoupdate";
        return "Auto Update";
                new ParameterOption("true", "Enforce an auto update"), //
                new ParameterOption("false", "Veto an auto update") //
