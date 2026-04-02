import org.eclipse.xtext.diagnostics.AbstractDiagnostic;
import org.eclipse.xtext.diagnostics.ExceptionDiagnostic;
public class ScriptParsingException extends ScriptException {
    private static final long serialVersionUID = -3784970293118871807L;
    public ScriptParsingException(String message, String scriptAsString) {
        super(message, scriptAsString);
    public ScriptParsingException(String message, String scriptAsString, Throwable t) {
        super(message, scriptAsString, t);
    public ScriptParsingException addDiagnosticErrors(List<Diagnostic> errors) {
        for (Diagnostic emfDiagnosticError : errors) {
            if (emfDiagnosticError instanceof AbstractDiagnostic e) {
                this.getErrors().add(new ScriptError(e.getMessage(), e.getLine(), e.getOffset(), e.getLength()));
            } else if (emfDiagnosticError instanceof ExceptionDiagnostic e) {
                this.getErrors().add(new ScriptError(emfDiagnosticError.getMessage(), -1, -1, -1));
    public ScriptParsingException addValidationIssues(Iterable<Issue> validationErrors) {
        for (Issue validationError : validationErrors) {
            this.getErrors().add(new ScriptError(validationError.getMessage(), validationError.getLineNumber(),
                    validationError.getOffset(), validationError.getLength()));
