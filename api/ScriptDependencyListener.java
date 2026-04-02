 * Interface that allows listener to be notified of script dependencies (libraries)
public interface ScriptDependencyListener extends Consumer<String> {
    void accept(String dependency);
