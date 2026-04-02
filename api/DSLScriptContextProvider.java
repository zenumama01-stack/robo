package org.openhab.core.model.script.runtime;
 * Interface of a provider that can provide Xbase-relevant object structures for
 * a purely string based script. This is required to support DSL rules, which
 * can have a context (variables) per file that is shared among multiple rules.
public interface DSLScriptContextProvider {
     * Identifier for scripts that are created from a DSL rule file
    String CONTEXT_IDENTIFIER = "// context: ";
     * Returns the evaluation context, i.e. the current state of the variables of the rule file.
     * @param contextName the filename of the rule file in question
    IEvaluationContext getContext(String contextName);
     * Returns the {@link XExpression}, which is the readily parsed script. As it might refer
     * to variables from the rule file scope, this script cannot be parsed independently.
     * @param ruleIndex the index of the rule within the file
     * @return the parsed script
    XExpression getParsedScript(String contextName, String ruleIndex);
