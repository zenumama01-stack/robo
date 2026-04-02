import java.io.InterruptedIOException;
import java.io.OutputStream;
import java.io.PipedInputStream;
import java.io.PipedOutputStream;
import java.util.LinkedList;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.atomic.AtomicBoolean;
 * This is an implementation of an {@link AudioStream} used to transmit raw audio data to a sink.
 * It just pipes the audio through it, the default pipe size is equal to 0.5 seconds of audio,
 * the implementation locks if you set a pipe size lower to the byte length used to write.
 * In order to support audio multiplex out of the box you should create a {@link PipedAudioStream.Group} instance
 * which can be used to create the {@link PipedAudioStream} connected to it and then write to all of them though the
 * group.
public class PipedAudioStream extends AudioStream {
    private final PipedInputStream pipedInput;
    private final PipedOutputStream pipedOutput;
    private final AtomicBoolean closed = new AtomicBoolean(false);
    private final LinkedList<Runnable> onCloseChain = new LinkedList<>();
    protected PipedAudioStream(AudioFormat format, int pipeSize, PipedOutputStream outputStream) throws IOException {
        this.pipedOutput = outputStream;
        this.pipedInput = new PipedInputStream(outputStream, pipeSize);
        return this.format;
        if (closed.get()) {
        return pipedInput.read();
    public int read(byte @Nullable [] b) throws IOException {
        return pipedInput.read(b);
    public int read(byte @Nullable [] b, int off, int len) throws IOException {
        return pipedInput.read(b, off, len);
        if (closed.getAndSet(true)) {
        if (!this.onCloseChain.isEmpty()) {
            this.onCloseChain.forEach(Runnable::run);
            this.onCloseChain.clear();
        pipedOutput.close();
        pipedInput.close();
     * Add a new handler that will be executed on stream close.
     * It will be chained to the previous handler if any, and executed in order.
     * @param onClose block to run on stream close
    public void onClose(Runnable onClose) {
        this.onCloseChain.add(onClose);
    protected PipedOutputStream getOutputStream() {
        return pipedOutput;
     * Creates a new piped stream group used to open new streams and write data to them.
     * Internal pipe size is 0.5s.
     * @param format the audio format of the group audio streams
     * @return a group instance
    public static Group newGroup(AudioFormat format) {
        int pipeSize = Math.round(( //
        (float) Objects.requireNonNull(format.getFrequency()) * //
                (float) Objects.requireNonNull(format.getBitDepth()) * //
                (float) Objects.requireNonNull(format.getChannels()) //
        ) / 2f);
        return new Group(format, pipeSize);
     * @param pipeSize the pipe size of the created streams
     * @return a piped stream group instance
    public static Group newGroup(AudioFormat format, int pipeSize) {
     * The {@link PipedAudioStream.Group} is an {@link OutputStream} implementation that can be use to
     * create one or more {@link PipedAudioStream} instances and write to them at once.
     * The created {@link PipedAudioStream} instances are removed from the group when closed.
    public static class Group extends OutputStream {
        private final int pipeSize;
        private final ConcurrentLinkedQueue<PipedAudioStream> openPipes = new ConcurrentLinkedQueue<>();
        private final Logger logger = LoggerFactory.getLogger(Group.class);
        protected Group(AudioFormat format, int pipeSize) {
            this.pipeSize = pipeSize;
         * Creates a new {@link PipedAudioStream} connected to the group.
         * The stream unregisters itself from the group on close.
         * @return a new {@link PipedAudioStream} to pipe data written to the group
         * @throws IOException when unable to create the stream
        public PipedAudioStream getAudioStreamInGroup() throws IOException {
            var pipedOutput = new PipedOutputStream();
            var audioStream = new PipedAudioStream(format, pipeSize, pipedOutput);
            if (!openPipes.add(audioStream)) {
                audioStream.close();
                throw new IOException("Unable to add new piped stream to group");
            audioStream.onClose(() -> {
                if (!openPipes.remove(audioStream)) {
                    logger.warn("Trying to remove an unregistered stream, this is not expected");
            return audioStream;
         * Returns true if this group has no streams connected.
         * @return true if this group has no streams connected
        public boolean isEmpty() {
            return openPipes.isEmpty();
         * Returns the number of streams connected.
         * @return the number of streams connected
        public int size() {
            return openPipes.size();
        public void write(byte @Nullable [] b, int off, int len) {
            synchronized (openPipes) {
                for (var pipe : openPipes) {
                        pipe.getOutputStream().write(b, off, len);
                    } catch (InterruptedIOException e) {
                        logger.warn("InterruptedIOException while writing to pipe: {}", e.getMessage());
                        logger.warn("IOException while writing to pipe: {}", e.getMessage());
                        logger.warn("RuntimeException while writing to pipe: {}", e.getMessage());
        public void write(int b) throws IOException {
                        pipe.getOutputStream().write(b);
        public void write(byte @Nullable [] bytes) {
                        pipe.getOutputStream().write(bytes);
                        logger.warn("InterruptedIOException on pipe flush: {}", e.getMessage());
                        logger.warn("IOException on pipe flush: {}", e.getMessage());
                        logger.warn("RuntimeException on pipe flush: {}", e.getMessage());
        public void flush() {
                        pipe.getOutputStream().flush();
        public void close() {
                        pipe.close();
                        logger.warn("InterruptedIOException closing pipe: {}", e.getMessage());
                        logger.warn("IOException closing pipe: {}", e.getMessage());
                        logger.warn("RuntimeException closing pipe: {}", e.getMessage());
                openPipes.clear();
