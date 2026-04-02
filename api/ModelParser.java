 * This interface has to be implemented by services that register an EMF model parser
public interface ModelParser {
     * Returns the file extensions of the models this parser registers for.
     * @return file extension of model files
    String getExtension();
