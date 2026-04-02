 * This is a {@link AudioStream}, which can also provide information about its absolute length and get cloned.
 * @author Gwendal Roulleau - Separate getClonedStream and length into their own interface.
 * @deprecated You should consider using {@link ClonableAudioStream} and/or {@link SizeableAudioStream} to detect audio
 *             stream capabilities
public abstract class FixedLengthAudioStream extends AudioStream implements SizeableAudioStream, ClonableAudioStream {
