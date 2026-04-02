package org.openhab.core.voice.internal.text;
import org.openhab.core.voice.text.AbstractRuleBasedInterpreter;
import org.openhab.core.voice.text.Expression;
import org.openhab.core.voice.text.Rule;
 * A human language command interpretation service.
 * @author Tilman Kamp - Initial contribution
 * @author Kai Kreuzer - Added further German interpretation rules
 * @author Laurent Garnier - Added French interpretation rules
 * @author Miguel Álvarez - Added Spanish interpretation rules
 * @author Miguel Álvarez - Added item's dynamic rules
@Component(service = HumanLanguageInterpreter.class)
public class StandardInterpreter extends AbstractRuleBasedInterpreter {
    public static final String VOICE_SYSTEM_NAMESPACE = "voiceSystem";
    private Logger logger = LoggerFactory.getLogger(StandardInterpreter.class);
    public StandardInterpreter(final @Reference EventPublisher eventPublisher,
            final @Reference ItemRegistry itemRegistry, @Reference MetadataRegistry metadataRegistry) {
        super(eventPublisher, itemRegistry, metadataRegistry);
        return Set.of(Locale.ENGLISH, Locale.GERMAN, Locale.FRENCH, Locale.of("es"));
    public void createRules(@Nullable Locale locale) {
        /* ***************************** ENGLISH ***************************** */
        if (locale == null || Objects.equals(locale.getLanguage(), Locale.ENGLISH.getLanguage())) {
            Expression onOff = alt(cmd("on", OnOffType.ON), cmd("off", OnOffType.OFF));
            Expression turn = alt("turn", "switch");
            Expression put = alt("put", "bring");
            Expression of = opt("of");
            Expression the = opt("the");
            Expression to = opt("to");
            Expression color = alt(cmd("white", HSBType.WHITE), cmd("pink", HSBType.fromRGB(255, 96, 208)),
                    cmd("yellow", HSBType.fromRGB(255, 224, 32)), cmd("orange", HSBType.fromRGB(255, 160, 16)),
                    cmd("purple", HSBType.fromRGB(128, 0, 128)), cmd("red", HSBType.RED), cmd("green", HSBType.GREEN),
                    cmd("blue", HSBType.BLUE));
            addRules(Locale.ENGLISH,
                    /* OnOffType */
                    itemRule(seq(turn, the), /* item */ onOff),
                    itemRule(seq(turn, onOff) /* item */),
                    /* IncreaseDecreaseType */
                    itemRule(seq(cmd(alt("dim", "decrease", "lower", "soften"), IncreaseDecreaseType.DECREASE),
                            the) /*
                                  * item
                                  */),
                    itemRule(seq(cmd(alt("brighten", "increase", "harden", "enhance"), IncreaseDecreaseType.INCREASE),
                            the) /* item */),
                    /* ColorType */
                    itemRule(seq(opt("set"), the, opt("color"), of, the), /* item */ seq(to, color)),
                    /* UpDownType */
                    itemRule(seq(put, the), /* item */ cmd("up", UpDownType.UP)),
                    itemRule(seq(put, the), /* item */ cmd("down", UpDownType.DOWN)),
                    /* NextPreviousType */
                    itemRule("move",
                            /* item */ seq(opt("to"),
                                    alt(cmd("next", NextPreviousType.NEXT),
                                            cmd("previous", NextPreviousType.PREVIOUS)))),
                    /* PlayPauseType */
                    itemRule(seq(cmd("play", PlayPauseType.PLAY), the) /* item */),
                    itemRule(seq(cmd("pause", PlayPauseType.PAUSE), the) /* item */),
                    /* RewindFastForwardType */
                    itemRule(seq(cmd("rewind", RewindFastforwardType.REWIND), the) /* item */),
                    itemRule(seq(cmd(seq(opt("fast"), "forward"), RewindFastforwardType.FASTFORWARD), the) /* item */),
                    /* StopMoveType */
                    itemRule(seq(cmd("stop", StopMoveType.STOP), the) /* item */),
                    itemRule(seq(cmd(alt("start", "move", "continue"), StopMoveType.MOVE), the) /* item */),
                    /* RefreshType */
                    itemRule(seq(cmd("refresh", RefreshType.REFRESH), the) /* item */)
            /* Item description commands */
            addRules(Locale.ENGLISH, createItemDescriptionRules( //
                    (allowedItems, labeledCmd) -> restrictedItemRule(allowedItems, //
                            seq(alt("set", "change"), opt(the)), /* item */ seq(to, labeledCmd)//
                    ), //
                    Locale.ENGLISH).toArray(Rule[]::new));
        /* ***************************** GERMAN ***************************** */
        if (locale == null || Objects.equals(locale.getLanguage(), Locale.GERMAN.getLanguage())) {
            Expression einAnAus = alt(cmd("ein", OnOffType.ON), cmd("an", OnOffType.ON), cmd("aus", OnOffType.OFF));
            Expression denDieDas = opt(alt("den", "die", "das"));
            Expression schalte = alt("schalt", "schalte", "mach");
            Expression pause = alt("pause", "stoppe");
            Expression mache = alt("mach", "mache", "fahre");
            Expression spiele = alt("spiele", "spiel", "starte");
            Expression zu = alt("zu", "zum", "zur");
            Expression naechste = alt("nächste", "nächstes", "nächster");
            Expression vorherige = alt("vorherige", "vorheriges", "vorheriger");
            Expression farbe = alt(cmd("weiß", HSBType.WHITE), cmd("pink", HSBType.fromRGB(255, 96, 208)),
                    cmd("gelb", HSBType.fromRGB(255, 224, 32)), cmd("orange", HSBType.fromRGB(255, 160, 16)),
                    cmd("lila", HSBType.fromRGB(128, 0, 128)), cmd("rot", HSBType.RED), cmd("grün", HSBType.GREEN),
                    cmd("blau", HSBType.BLUE));
            addRules(Locale.GERMAN,
                    itemRule(seq(schalte, denDieDas), /* item */ einAnAus),
                    itemRule(seq(cmd(alt("dimme"), IncreaseDecreaseType.DECREASE), denDieDas) /* item */),
                    itemRule(seq(schalte, denDieDas),
                            /* item */ cmd(alt("dunkler", "weniger"), IncreaseDecreaseType.DECREASE)),
                            /* item */ cmd(alt("heller", "mehr"), IncreaseDecreaseType.INCREASE)),
                    itemRule(seq(schalte, denDieDas), /* item */ seq(opt("auf"), farbe)),
                    itemRule(seq(mache, denDieDas), /* item */ cmd("hoch", UpDownType.UP)),
                    itemRule(seq(mache, denDieDas), /* item */ cmd("runter", UpDownType.DOWN)),
                    itemRule("wechsle",
                            /* item */ seq(opt(zu),
                                    alt(cmd(naechste, NextPreviousType.NEXT),
                                            cmd(vorherige, NextPreviousType.PREVIOUS)))),
                    itemRule(seq(cmd(spiele, PlayPauseType.PLAY), the) /* item */),
                    itemRule(seq(cmd(pause, PlayPauseType.PAUSE), the) /* item */)
            addRules(Locale.GERMAN, createItemDescriptionRules( //
                            seq(schalte, denDieDas), /* item */ seq(opt("auf"), labeledCmd)//
                    Locale.GERMAN).toArray(Rule[]::new));
        /* ***************************** FRENCH ***************************** */
        if (locale == null || Objects.equals(locale.getLanguage(), Locale.FRENCH.getLanguage())) {
            Expression allume = alt("allume", "démarre", "active");
            Expression eteins = alt("éteins", "stoppe", "désactive", "coupe");
            Expression lela = opt(alt("le", "la", "les", "l"));
            Expression poursurdude = opt(alt("pour", "sur", "du", "de"));
            Expression couleur = alt(cmd("blanc", HSBType.WHITE), cmd("rose", HSBType.fromRGB(255, 96, 208)),
                    cmd("jaune", HSBType.fromRGB(255, 224, 32)), cmd("orange", HSBType.fromRGB(255, 160, 16)),
                    cmd("violet", HSBType.fromRGB(128, 0, 128)), cmd("rouge", HSBType.RED), cmd("vert", HSBType.GREEN),
                    cmd("bleu", HSBType.BLUE));
            addRules(Locale.FRENCH,
                    itemRule(seq(cmd(allume, OnOffType.ON), lela) /* item */),
                    itemRule(seq(cmd(eteins, OnOffType.OFF), lela) /* item */),
                    itemRule(seq(cmd("augmente", IncreaseDecreaseType.INCREASE), lela) /* item */),
                    itemRule(seq(cmd("diminue", IncreaseDecreaseType.DECREASE), lela) /* item */),
                    itemRule(seq(cmd("plus", IncreaseDecreaseType.INCREASE), "de") /* item */),
                    itemRule(seq(cmd("moins", IncreaseDecreaseType.DECREASE), "de") /* item */),
                    itemRule(seq("couleur", couleur, opt("pour"), lela) /* item */),
                    itemRule(seq(cmd("reprise", PlayPauseType.PLAY), "lecture", poursurdude, lela) /* item */),
                    itemRule(seq(cmd("pause", PlayPauseType.PAUSE), "lecture", poursurdude, lela) /* item */),
                    itemRule(seq(alt("plage", "piste"),
                            alt(cmd("suivante", NextPreviousType.NEXT), cmd("précédente", NextPreviousType.PREVIOUS)),
                            poursurdude, lela) /* item */),
                    itemRule(seq(cmd("monte", UpDownType.UP), lela) /* item */),
                    itemRule(seq(cmd("descends", UpDownType.DOWN), lela) /* item */),
                    itemRule(seq(cmd("arrête", StopMoveType.STOP), lela) /* item */),
                    itemRule(seq(cmd(alt("bouge", "déplace"), StopMoveType.MOVE), lela) /* item */),
                    itemRule(seq(cmd("rafraîchis", RefreshType.REFRESH), lela) /* item */)
            addRules(Locale.FRENCH, createItemDescriptionRules( //
                            seq("mets", lela), /* item */ seq(poursurdude, lela, labeledCmd)//
                    Locale.FRENCH).toArray(Rule[]::new));
        /* ***************************** SPANISH ***************************** */
        Locale localeES = Locale.of("es");
        if (locale == null || Objects.equals(locale.getLanguage(), localeES.getLanguage())) {
            Expression encenderApagar = alt(cmd(alt("enciende", "encender"), OnOffType.ON),
                    cmd(alt("apaga", "apagar"), OnOffType.OFF));
            Expression cambiar = alt("cambia", "cambiar");
            Expression poner = alt("pon", "poner");
            Expression preposicion = opt(alt("a", "de", "en"));
            Expression articulo = opt(alt("el", "la", "los", "las"));
            Expression nombreColor = alt(cmd("blanco", HSBType.WHITE), cmd("rosa", HSBType.fromRGB(255, 96, 208)),
                    cmd("amarillo", HSBType.fromRGB(255, 224, 32)), cmd("naranja", HSBType.fromRGB(255, 160, 16)),
                    cmd("púrpura", HSBType.fromRGB(128, 0, 128)), cmd("rojo", HSBType.RED), cmd("verde", HSBType.GREEN),
                    cmd("azul", HSBType.BLUE));
            addRules(localeES,
                    itemRule(seq(encenderApagar, articulo)/* item */),
                    itemRule(seq(cmd(alt("baja", "suaviza", "bajar", "suavizar"), IncreaseDecreaseType.DECREASE),
                            articulo) /*
                    itemRule(seq(cmd(alt("sube", "aumenta", "subir", "aumentar"), IncreaseDecreaseType.INCREASE),
                            articulo) /* item */),
                    itemRule(seq(cambiar, articulo, opt("color"), preposicion, articulo),
                            /* item */ seq(opt("a"), nombreColor)),
                    itemRule(seq(poner, articulo), /* item */ cmd("arriba", UpDownType.UP)),
                    itemRule(seq(poner, articulo), /* item */ cmd("abajo", UpDownType.DOWN)),
                    itemRule(seq(cambiar, opt(articulo)),
                            /* item */ seq(opt("a"),
                                    alt(cmd("siguiente", NextPreviousType.NEXT),
                                            cmd("anterior", NextPreviousType.PREVIOUS)))),
                    itemRule(
                            seq(opt(poner),
                                            cmd("anterior", NextPreviousType.PREVIOUS)),
                                    "en"),
                            opt(articulo) /* item */ ),
                    itemRule(seq(cmd(alt("continuar", "continúa", "reanudar", "reanuda", "play"), PlayPauseType.PLAY),
                            alt(articulo, "en")) /*
                    itemRule(seq(cmd(alt("pausa", "pausar", "detén", "detener"), PlayPauseType.PAUSE),
                    itemRule(seq(cmd(alt("rebobina", "rebobinar"), RewindFastforwardType.REWIND),
                            alt(articulo, "en")) /* item */),
                    itemRule(seq(cmd(alt("avanza", "avanzar"), RewindFastforwardType.FASTFORWARD),
                    itemRule(seq(cmd(alt("para", "parar", "stop"), StopMoveType.STOP), articulo) /* item */),
                    itemRule(seq(cmd(alt("mueve", "mover"), StopMoveType.MOVE), articulo) /* item */),
                    itemRule(seq(cmd(alt("recarga", "refresca", "recargar", "refrescar"), RefreshType.REFRESH),
                            articulo) /* item */)
            addRules(localeES, createItemDescriptionRules( //
                            seq(alt(cambiar, poner), opt(articulo)), /* item */ seq(preposicion, labeledCmd)//
                    localeES).toArray(Rule[]::new));
        return "system";
        return "Built-in Interpreter";
    private List<Rule> createItemDescriptionRules(CreateItemDescriptionRule creator, Locale locale) {
        // Map different item state/command labels with theirs values by item
        HashMap<String, HashMap<Item, String>> options = new HashMap<>();
        List<Rule> customRules = new ArrayList<>();
        for (var item : itemRegistry.getItems()) {
            customRules.addAll(createItemMetadataRules(locale, item));
            var stateDesc = item.getStateDescription(locale);
            if (stateDesc != null) {
                stateDesc.getOptions().forEach(op -> {
                    var label = op.getLabel();
                        label = op.getValue();
                    var optionValueByItem = options.getOrDefault(label, new HashMap<>());
                    optionValueByItem.put(item, op.getValue());
                    options.put(label, optionValueByItem);
            var commandDesc = item.getCommandDescription(locale);
            if (commandDesc != null) {
                commandDesc.getCommandOptions().forEach(op -> {
                        label = op.getCommand();
                    optionValueByItem.put(item, op.getCommand());
        // create rules
        return Stream.concat(customRules.stream(), options.entrySet().stream() //
                .map(entry -> {
                    String label = entry.getKey();
                    Map<Item, String> commandByItem = entry.getValue();
                    String[] labelParts = Arrays.stream(label.split("\\s")).filter(p -> !p.isBlank())
                    Expression labeledCmd = cmd(seq((Object[]) labelParts),
                            new ItemStateCommandSupplier(label, commandByItem));
                    return creator.itemDescriptionRule(commandByItem.keySet(), labeledCmd);
                })) //
    private List<Rule> createItemMetadataRules(Locale locale, Item item) {
        var interpreterMetadata = metadataRegistry.get(new MetadataKey(VOICE_SYSTEM_NAMESPACE, item.getName()));
        if (interpreterMetadata == null) {
        return Arrays.stream(interpreterMetadata.getValue().split("\n")) //
                .map(line -> this.parseItemCustomRules(locale, item, line.trim(), interpreterMetadata)) //
                .flatMap(List::stream) //
    private interface CreateItemDescriptionRule {
        Rule itemDescriptionRule(Set<Item> allowedItemNames, Expression labeledCmd);
    private record ItemStateCommandSupplier(String label,
            Map<Item, String> commandByItem) implements ItemCommandSupplier {
        public @Nullable Command getItemCommand(Item item) {
            String textCommand = commandByItem.get(item);
            if (textCommand == null) {
            return TypeParser.parseCommand(item.getAcceptedCommandTypes(), textCommand);
        public String getCommandLabel() {
        public List<Class<? extends Command>> getCommandClasses(@Nullable Item item) {
                return commandByItem.keySet().stream()//
                        .flatMap(i -> i.getAcceptedCommandTypes().stream())//
                        .distinct()//
            } else if (commandByItem.containsKey(item)) {
                return item.getAcceptedCommandTypes();
