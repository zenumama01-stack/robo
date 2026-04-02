package org.openhab.core.model.script.internal.engine.action;
import org.openhab.core.model.script.actions.Audio;
public class AudioActionService implements ActionService {
    public static @Nullable AudioManager audioManager;
    public AudioActionService(final @Reference AudioManager audioManager) {
        AudioActionService.audioManager = audioManager;
    public Class<?> getActionClass() {
        return Audio.class;
