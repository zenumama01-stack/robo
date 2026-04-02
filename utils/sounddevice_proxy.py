    import sounddevice as sounddevice  # type: ignore
SOUNDDEVICE_INSTRUCTIONS = format_instructions(library="sounddevice", extra="voice_helpers")
class SounddeviceProxy(LazyProxy[Any]):
            import sounddevice  # type: ignore
            raise MissingDependencyError(SOUNDDEVICE_INSTRUCTIONS) from err
        return sounddevice
    sounddevice = SounddeviceProxy()
