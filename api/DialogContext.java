 * Describes dialog configured services and options.
public record DialogContext(@Nullable DTService dt, @Nullable String keyword, STTService stt, TTSService tts,
        @Nullable Voice voice, List<HumanLanguageInterpreter> hlis, AudioSource source, AudioSink sink, Locale locale,
        String dialogGroup, @Nullable String locationItem, @Nullable String listeningItem,
        @Nullable String listeningMelody) {
     * Builder for {@link DialogContext}
     * Allows to describe a dialog context without requiring the involved services to be loaded
        private @Nullable AudioSource source;
        private @Nullable AudioSink sink;
        private @Nullable DTService dt;
        private @Nullable STTService stt;
        private @Nullable TTSService tts;
        private @Nullable Voice voice;
        private List<HumanLanguageInterpreter> hlis = List.of();
        // options
        private String dialogGroup = "default";
        private @Nullable String locationItem;
        private @Nullable String listeningItem;
        private @Nullable String listeningMelody;
        private String keyword;
        public Builder(String keyword, Locale locale) {
            this.keyword = keyword;
        public Builder withSource(@Nullable AudioSource source) {
        public Builder withSink(@Nullable AudioSink sink) {
            this.sink = sink;
        public Builder withKS(@Nullable KSService service) {
                this.dt = service;
        public Builder withDT(@Nullable DTService service) {
        public Builder withSTT(@Nullable STTService service) {
                this.stt = service;
        public Builder withTTS(@Nullable TTSService service) {
                this.tts = service;
        public Builder withHLI(@Nullable HumanLanguageInterpreter service) {
                this.hlis = List.of(service);
        public Builder withHLIs(Collection<HumanLanguageInterpreter> services) {
            return withHLIs(new ArrayList<>(services));
        public Builder withHLIs(List<HumanLanguageInterpreter> services) {
            if (!services.isEmpty()) {
                this.hlis = services;
        public Builder withKeyword(@Nullable String keyword) {
            if (keyword != null && !keyword.isBlank()) {
        public Builder withVoice(@Nullable Voice voice) {
                this.voice = voice;
        public Builder withDialogGroup(@Nullable String dialogGroup) {
            if (dialogGroup != null) {
                this.dialogGroup = dialogGroup;
        public Builder withLocationItem(@Nullable String locationItem) {
                this.locationItem = locationItem;
        public Builder withListeningItem(@Nullable String listeningItem) {
                this.listeningItem = listeningItem;
        public Builder withMelody(@Nullable String listeningMelody) {
            if (listeningMelody != null) {
                this.listeningMelody = listeningMelody;
        public Builder withLocale(@Nullable Locale locale) {
         * Creates a new {@link DialogContext}
         * @return a {@link DialogContext} with the configured components and options
         * @throws IllegalStateException if a required dialog component is missing
        public DialogContext build() throws IllegalStateException {
            DTService dtService = dt;
            STTService sttService = stt;
            TTSService ttsService = tts;
            List<HumanLanguageInterpreter> hliServices = hlis;
            AudioSource audioSource = source;
            AudioSink audioSink = sink;
            if (sttService == null || ttsService == null || hliServices.isEmpty() || audioSource == null
                    || audioSink == null) {
                    errors.add("missing stt service");
                    errors.add("missing tts service");
                    errors.add("missing interpreters");
                    errors.add("missing audio source");
                    errors.add("missing audio sink");
                throw new IllegalStateException("Cannot build dialog context: " + String.join(", ", errors) + ".");
                return new DialogContext(dtService, keyword, sttService, ttsService, voice, hliServices, audioSource,
                        audioSink, locale, dialogGroup, locationItem, listeningItem, listeningMelody);
