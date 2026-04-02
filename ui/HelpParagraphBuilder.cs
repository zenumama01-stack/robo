using System.Windows.Documents;
namespace Microsoft.Management.UI.Internal
    /// Builds a help paragraph for a cmdlet.
    internal class HelpParagraphBuilder : ParagraphBuilder
        /// Indentation size.
        internal const int IndentSize = 4;
        /// new line separators.
        private static readonly string[] Separators = new[] { "\r\n", "\n" };
        /// Object with the cmdelt.
        private readonly PSObject psObj;
        /// Initializes a new instance of the HelpParagraphBuilder class.
        /// <param name="paragraph">Paragraph being built.</param>
        /// <param name="psObj">Object with help information.</param>
        internal HelpParagraphBuilder(Paragraph paragraph, PSObject psObj)
            : base(paragraph)
            this.psObj = psObj;
            this.AddTextToParagraphBuilder();
        /// Enum for category of Help.
        private enum HelpCategory
            DscResource,
            Class
        /// Gets the string value of a property or null if it could not be retrieved.
        /// <param name="psObj">Object with the property.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>The string value of a property or null if it could not be retrieved.</returns>
        internal static string GetPropertyString(PSObject psObj, string propertyName)
            Debug.Assert(psObj != null, "ensured by caller");
            object value = GetPropertyObject(psObj, propertyName);
            return value.ToString();
        /// Adds the help text to the paragraph.
        internal void AddTextToParagraphBuilder()
            this.ResetAllText();
            string strCategory = HelpParagraphBuilder.GetProperty(this.psObj, "Category").Value.ToString();
            HelpCategory category = HelpCategory.Default;
            if (string.Equals(strCategory, "DscResource", StringComparison.OrdinalIgnoreCase))
                category = HelpCategory.DscResource;
            else if (string.Equals(strCategory, "Class", StringComparison.OrdinalIgnoreCase))
                category = HelpCategory.Class;
            if (HelpParagraphBuilder.GetProperty(this.psObj, "Syntax") == null)
                if (category == HelpCategory.Default)
                    // if there is no syntax, this is not the standard help
                    // it might be an about page
                    this.AddText(this.psObj.ToString(), false);
            switch (category)
                case HelpCategory.Class:
                    this.AddDescription(HelpWindowSettings.Default.HelpSynopsysDisplayed, HelpWindowResources.SynopsisTitle, "Introduction");
                    this.AddMembers(HelpWindowSettings.Default.HelpParametersDisplayed, HelpWindowResources.PropertiesTitle);
                    this.AddMembers(HelpWindowSettings.Default.HelpParametersDisplayed, HelpWindowResources.MethodsTitle);
                case HelpCategory.DscResource:
                    this.AddStringSection(HelpWindowSettings.Default.HelpSynopsysDisplayed, "Synopsis", HelpWindowResources.SynopsisTitle);
                    this.AddDescription(HelpWindowSettings.Default.HelpDescriptionDisplayed, HelpWindowResources.DescriptionTitle, "Description");
                    this.AddParameters(HelpWindowSettings.Default.HelpParametersDisplayed, HelpWindowResources.PropertiesTitle, "Properties", HelpCategory.DscResource);
                    this.AddParameters(HelpWindowSettings.Default.HelpParametersDisplayed, HelpWindowResources.ParametersTitle, "Parameters", HelpCategory.Default);
                    this.AddSyntax(HelpWindowSettings.Default.HelpSyntaxDisplayed, HelpWindowResources.SyntaxTitle);
            this.AddInputOrOutputEntries(HelpWindowSettings.Default.HelpInputsDisplayed, HelpWindowResources.InputsTitle, "inputTypes", "inputType");
            this.AddInputOrOutputEntries(HelpWindowSettings.Default.HelpOutputsDisplayed, HelpWindowResources.OutputsTitle, "returnValues", "returnValue");
            this.AddNotes(HelpWindowSettings.Default.HelpNotesDisplayed, HelpWindowResources.NotesTitle);
            this.AddExamples(HelpWindowSettings.Default.HelpExamplesDisplayed, HelpWindowResources.ExamplesTitle);
            this.AddNavigationLink(HelpWindowSettings.Default.HelpRelatedLinksDisplayed, HelpWindowResources.RelatedLinksTitle);
            this.AddStringSection(HelpWindowSettings.Default.HelpRemarksDisplayed, "Remarks", HelpWindowResources.RemarksTitle);
        /// Gets the object property or null if it could not be retrieved.
        /// <returns>The object property or null if it could not be retrieved.</returns>
        private static PSPropertyInfo GetProperty(PSObject psObj, string propertyName)
            return psObj.Properties[propertyName];
        /// Gets a PSObject and then a value from it or null if the value could not be retrieved.
        /// <param name="psObj">PSObject that contains another PSObject as a property.</param>
        /// <param name="psObjectName">Property name that contains the PSObject.</param>
        /// <param name="propertyName">Property name in the inner PSObject.</param>
        /// <returns>The string from the inner psObject property or null if it could not be retrieved.</returns>
        private static string GetInnerPSObjectPropertyString(PSObject psObj, string psObjectName, string propertyName)
            PSObject innerPsObj = GetPropertyObject(psObj, psObjectName) as PSObject;
            if (innerPsObj == null)
            object value = GetPropertyObject(innerPsObj, propertyName);
        /// Gets the value of a property or null if the value could not be retrieved.
        /// <returns>The value of a property or null if the value could not be retrieved.</returns>
        private static object GetPropertyObject(PSObject psObj, string propertyName)
            PSPropertyInfo property = HelpParagraphBuilder.GetProperty(psObj, propertyName);
            if (property == null)
            object value = null;
                value = property.Value;
            catch (ExtendedTypeSystemException)
                // ignore this exception
        /// Gets the text from a property of type PSObject[] where the first object has a text property.
        /// <param name="psObj">Objhect to get text from.</param>
        /// <param name="propertyText">Property with PSObject[] containing text.</param>
        /// <returns>The text from a property of type PSObject[] where the first object has a text property.</returns>
        private static string GetTextFromArray(PSObject psObj, string propertyText)
            PSObject[] introductionObjects = HelpParagraphBuilder.GetPropertyObject(psObj, propertyText) as PSObject[];
            if (introductionObjects != null && introductionObjects.Length > 0)
                return GetPropertyString(introductionObjects[0], "text");
        /// Returns the largest size of a group of strings.
        /// <param name="strs">Strings to evaluate the largest size from.</param>
        /// <returns>The largest size of a group of strings.</returns>
        private static int LargestSize(params string[] strs)
            int returnValue = 0;
            foreach (string str in strs)
                if (str != null && str.Length > returnValue)
                    returnValue = str.Length;
            return returnValue;
        /// Splits the string adding indentation before each line.
        /// <param name="str">String to add indentation to.</param>
        /// <returns>The string indented.</returns>
        private static string AddIndent(string str)
            return HelpParagraphBuilder.AddIndent(str, 1);
        /// <param name="numberOfIdents">Number of indentations.</param>
        private static string AddIndent(string str, int numberOfIdents)
            StringBuilder indent = new StringBuilder();
            indent.Append(' ', numberOfIdents * HelpParagraphBuilder.IndentSize);
            return HelpParagraphBuilder.AddIndent(str, indent.ToString());
        /// <param name="indentString">Indentation string.</param>
        private static string AddIndent(string str, string indentString)
            if (str == null)
                return string.Empty;
            string[] lines = str.Split(Separators, StringSplitOptions.None);
            StringBuilder returnValue = new StringBuilder();
            foreach (string line in lines)
                // Indentation is not localized
                returnValue.Append($"{indentString}{line}\r\n");
            if (returnValue.Length > 2)
                // remove the last \r\n
                returnValue.Remove(returnValue.Length - 2, 2);
            return returnValue.ToString();
        /// Get the object array value of a property.
        /// <param name="obj">Object containing the property.</param>
        /// <param name="propertyName">Property with the array value.</param>
        /// <returns>The object array value of a property.</returns>
        private static object[] GetPropertyObjectArray(PSObject obj, string propertyName)
            object innerObject;
            if ((innerObject = HelpParagraphBuilder.GetPropertyObject(obj, propertyName)) == null)
            if (innerObject is PSObject)
                return new[] { innerObject };
            object[] innerObjectArray = innerObject as object[];
            return innerObjectArray;
        /// Adds a section that contains only a string.
        /// <param name="setting">True if it should add the segment.</param>
        /// <param name="sectionName">Name of the section to add.</param>
        /// <param name="sectionTitle">Title of the section.</param>
        private void AddStringSection(bool setting, string sectionName, string sectionTitle)
            string propertyValue;
            if (!setting || (propertyValue = HelpParagraphBuilder.GetPropertyString(this.psObj, sectionName)) == null)
            this.AddText(sectionTitle, true);
            this.AddText("\r\n", false);
            this.AddText(HelpParagraphBuilder.AddIndent(propertyValue), false);
            this.AddText("\r\n\r\n", false);
        /// Adds the help syntax segment.
        private void AddSyntax(bool setting, string sectionTitle)
            PSObject syntaxObject;
            if (!setting || (syntaxObject = HelpParagraphBuilder.GetPropertyObject(this.psObj, "Syntax") as PSObject) == null)
            object[] syntaxItemsObj = HelpParagraphBuilder.GetPropertyObjectArray(syntaxObject, "syntaxItem");
            if (syntaxItemsObj == null || syntaxItemsObj.Length == 0)
            foreach (object syntaxItemObj in syntaxItemsObj)
                PSObject syntaxItem = syntaxItemObj as PSObject;
                if (syntaxItem == null)
                string commandName = GetPropertyString(syntaxItem, "name");
                object[] parameterObjs = HelpParagraphBuilder.GetPropertyObjectArray(syntaxItem, "parameter");
                if (commandName == null || parameterObjs == null || parameterObjs.Length == 0)
                string commandStart = string.Create(CultureInfo.CurrentCulture, $"{commandName} ");
                this.AddText(HelpParagraphBuilder.AddIndent(commandStart), false);
                foreach (object parameterObj in parameterObjs)
                    PSObject parameter = parameterObj as PSObject;
                    if (parameter == null)
                    string parameterValue = GetPropertyString(parameter, "parameterValue");
                    string position = GetPropertyString(parameter, "position");
                    string required = GetPropertyString(parameter, "required");
                    string parameterName = GetPropertyString(parameter, "name");
                    if (position == null || required == null || parameterName == null)
                    string parameterType = parameterValue == null ? string.Empty : string.Create(CultureInfo.CurrentCulture, $"<{parameterValue}>");
                    string parameterOptionalOpenBrace, parameterOptionalCloseBrace;
                    if (string.Equals(required, "true", StringComparison.OrdinalIgnoreCase))
                        parameterOptionalOpenBrace = string.Empty;
                        parameterOptionalCloseBrace = string.Empty;
                        parameterOptionalOpenBrace = "[";
                        parameterOptionalCloseBrace = "]";
                    string parameterNameOptionalOpenBrace, parameterNameOptionalCloseBrace;
                    if (string.Equals(position, "named", StringComparison.OrdinalIgnoreCase))
                        parameterNameOptionalOpenBrace = parameterNameOptionalCloseBrace = string.Empty;
                        parameterNameOptionalOpenBrace = "[";
                        parameterNameOptionalCloseBrace = "]";
                    string paramterPrefix = string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}{1}-",
                        parameterOptionalOpenBrace,
                        parameterNameOptionalOpenBrace);
                    this.AddText(paramterPrefix, false);
                    this.AddText(parameterName, true);
                    string paramterSuffix = string.Format(
                        "{0} {1}{2} ",
                        parameterNameOptionalCloseBrace,
                        parameterType,
                        parameterOptionalCloseBrace);
                    this.AddText(paramterSuffix, false);
                string commonParametersText = string.Format(
                    "[<{0}>]\r\n\r\n",
                    HelpWindowResources.CommonParameters);
                this.AddText(commonParametersText, false);
        /// Adds the help description segment.
        /// <param name="propertyName">PropertyName that has description.</param>
        private void AddDescription(bool setting, string sectionTitle, string propertyName)
            PSObject[] descriptionObjects;
            if (!setting ||
                 (descriptionObjects = HelpParagraphBuilder.GetPropertyObject(this.psObj, propertyName) as PSObject[]) == null ||
                 descriptionObjects.Length == 0)
            foreach (PSObject description in descriptionObjects)
                string descriptionText = GetPropertyString(description, "text");
                this.AddText(HelpParagraphBuilder.AddIndent(descriptionText), false);
        /// Adds the help examples segment.
        private void AddExamples(bool setting, string sectionTitle)
            if (!setting)
            PSObject exampleRootObject = HelpParagraphBuilder.GetPropertyObject(this.psObj, "Examples") as PSObject;
            if (exampleRootObject == null)
            object[] exampleObjects = HelpParagraphBuilder.GetPropertyObjectArray(exampleRootObject, "example");
            if (exampleObjects == null || exampleObjects.Length == 0)
            foreach (object exampleObj in exampleObjects)
                PSObject example = exampleObj as PSObject;
                if (example == null)
                string introductionText = null;
                introductionText = GetTextFromArray(example, "introduction");
                string codeText = GetPropertyString(example, "code");
                string title = GetPropertyString(example, "title");
                if (codeText == null)
                if (title != null)
                    this.AddText(HelpParagraphBuilder.AddIndent(title), false);
                string codeLine = string.Format(
                    "{0}{1}\r\n\r\n",
                    introductionText,
                    codeText);
                this.AddText(HelpParagraphBuilder.AddIndent(codeLine), false);
                PSObject[] remarks = HelpParagraphBuilder.GetPropertyObject(example, "remarks") as PSObject[];
                if (remarks == null)
                foreach (PSObject remark in remarks)
                    string remarkText = GetPropertyString(remark, "text");
                    if (remarkText == null)
                    this.AddText(remarkText, false);
        private void AddMembers(bool setting, string sectionTitle)
            if (!setting || string.IsNullOrEmpty(sectionTitle))
            PSObject memberRootObject = HelpParagraphBuilder.GetPropertyObject(this.psObj, "Members") as PSObject;
            if (memberRootObject == null)
            object[] memberObjects = HelpParagraphBuilder.GetPropertyObjectArray(memberRootObject, "member");
            if (memberObjects == null)
            foreach (object memberObj in memberObjects)
                string description = null;
                string memberText = null;
                PSObject member = memberObj as PSObject;
                if (member == null)
                string name = GetPropertyString(member, "title");
                string type = GetPropertyString(member, "type");
                string propertyType = null;
                if (string.Equals("field", type, StringComparison.OrdinalIgnoreCase))
                    PSObject fieldData = HelpParagraphBuilder.GetPropertyObject(member, "fieldData") as PSObject;
                    if (fieldData != null)
                        PSObject propertyTypeObject = HelpParagraphBuilder.GetPropertyObject(fieldData, "type") as PSObject;
                        if (propertyTypeObject != null)
                            propertyType = GetPropertyString(propertyTypeObject, "name");
                            description = GetPropertyString(propertyTypeObject, "description");
                        memberText = string.Create(CultureInfo.CurrentCulture, $" [{propertyType}] {name}\r\n");
                else if (string.Equals("method", type, StringComparison.OrdinalIgnoreCase))
                    FormatMethodData(member, name, out memberText, out description);
                if (!string.IsNullOrEmpty(memberText))
                    this.AddText(HelpParagraphBuilder.AddIndent(string.Empty), false);
                    this.AddText(memberText, true);
                    if (description != null)
                        this.AddText(HelpParagraphBuilder.AddIndent(description, 2), false);
        private static void FormatMethodData(PSObject member, string name, out string memberText, out string description)
            memberText = null;
            description = null;
            if (member == null || string.IsNullOrEmpty(name))
            string returnType = null;
            StringBuilder parameterText = new StringBuilder();
            // Get method return type
            PSObject returnTypeObject = HelpParagraphBuilder.GetPropertyObject(member, "returnValue") as PSObject;
            if (returnTypeObject != null)
                PSObject returnTypeData = HelpParagraphBuilder.GetPropertyObject(returnTypeObject, "type") as PSObject;
                if (returnTypeData != null)
                    returnType = GetPropertyString(returnTypeData, "name");
            // Get method description.
            PSObject[] methodDescriptions = HelpParagraphBuilder.GetPropertyObject(member, "introduction") as PSObject[];
            if (methodDescriptions != null)
                foreach (var methodDescription in methodDescriptions)
                    description = GetPropertyString(methodDescription, "Text");
                    // If we get an text we do not need to iterate more.
                    if (!string.IsNullOrEmpty(description))
            // Get method parameters.
            PSObject parametersObject = HelpParagraphBuilder.GetPropertyObject(member, "parameters") as PSObject;
            if (parametersObject != null)
                PSObject[] paramObject = HelpParagraphBuilder.GetPropertyObject(parametersObject, "parameter") as PSObject[];
                if (paramObject != null)
                    foreach (var param in paramObject)
                        string parameterName = GetPropertyString(param, "name");
                        string parameterType = null;
                        PSObject parameterTypeData = HelpParagraphBuilder.GetPropertyObject(param, "type") as PSObject;
                        if (parameterTypeData != null)
                            parameterType = GetPropertyString(parameterTypeData, "name");
                            // If there is no type for the parameter, we expect it is System.Object
                            if (string.IsNullOrEmpty(parameterType))
                                parameterType = "object";
                        string paramString = string.Create(CultureInfo.CurrentCulture, $"[{parameterType}] ${parameterName},");
                        parameterText.Append(paramString);
                    if (string.Equals(parameterText[parameterText.Length - 1].ToString(), ",", StringComparison.OrdinalIgnoreCase))
                        parameterText = parameterText.Remove(parameterText.Length - 1, 1);
            memberText = string.Create(CultureInfo.CurrentCulture, $" [{returnType}] {name}({parameterText})\r\n");
        /// Adds the help parameters segment.
        /// <param name="paramPropertyName">Name of the property which has properties.</param>
        /// <param name="helpCategory">Category of help.</param>
        private void AddParameters(bool setting, string sectionTitle, string paramPropertyName, HelpCategory helpCategory)
            PSObject parameterRootObject = HelpParagraphBuilder.GetPropertyObject(this.psObj, paramPropertyName) as PSObject;
            if (parameterRootObject == null)
            object[] parameterObjects = null;
            // Root object for Class has members not parameters.
            if (helpCategory != HelpCategory.Class)
                parameterObjects = HelpParagraphBuilder.GetPropertyObjectArray(parameterRootObject, "parameter");
            if (parameterObjects == null || parameterObjects.Length == 0)
            foreach (object parameterObj in parameterObjects)
                string name = GetPropertyString(parameter, "name");
                string description = GetTextFromArray(parameter, "description");
                string pipelineinput = GetPropertyString(parameter, "pipelineInput");
                string defaultValue = GetPropertyString(parameter, "defaultValue");
                string acceptWildcard = GetPropertyString(parameter, "globbing");
                if (string.IsNullOrEmpty(name))
                if (helpCategory == HelpCategory.DscResource)
                    this.AddText(HelpParagraphBuilder.AddIndent("-"), false);
                this.AddText(name, true);
                string parameterText = string.Format(
                    " <{0}>\r\n",
                    parameterValue);
                this.AddText(parameterText, false);
                int largestSize = HelpParagraphBuilder.LargestSize(
                    HelpWindowResources.ParameterRequired,
                    HelpWindowResources.ParameterPosition,
                    HelpWindowResources.ParameterDefaultValue,
                    HelpWindowResources.ParameterPipelineInput,
                    HelpWindowResources.ParameterAcceptWildcard);
                // justification of parameter values is not localized
                string formatString = string.Format(
                    "{{0,-{0}}}{{1}}",
                    largestSize + 2);
                string tableLine;
                tableLine = string.Format(
                    formatString,
                    required);
                this.AddText(HelpParagraphBuilder.AddIndent(tableLine, 2), false);
                // these are not applicable for Dsc Resource help
                if (helpCategory != HelpCategory.DscResource)
                        position);
                        defaultValue);
                        pipelineinput);
                        HelpWindowResources.ParameterAcceptWildcard,
                        acceptWildcard);
        /// Adds the help navigation links segment.
        private void AddNavigationLink(bool setting, string sectionTitle)
            PSObject linkRootObject = HelpParagraphBuilder.GetPropertyObject(this.psObj, "RelatedLinks") as PSObject;
            if (linkRootObject == null)
            PSObject[] linkObjects;
            if ((linkObjects = HelpParagraphBuilder.GetPropertyObject(linkRootObject, "navigationLink") as PSObject[]) == null ||
                linkObjects.Length == 0)
            foreach (PSObject linkObject in linkObjects)
                string text = GetPropertyString(linkObject, "linkText");
                string uri = GetPropertyString(linkObject, "uri");
                string linkLine = string.IsNullOrEmpty(uri) ? text : string.Format(
                    HelpWindowResources.LinkTextFormat,
                    text,
                    uri);
                this.AddText(HelpParagraphBuilder.AddIndent(linkLine), false);
        /// Adds the help input or output segment.
        /// <param name="inputOrOutputProperty">Property with the outter object.</param>
        /// <param name="inputOrOutputInnerProperty">Property with the inner object.</param>
        private void AddInputOrOutputEntries(bool setting, string sectionTitle, string inputOrOutputProperty, string inputOrOutputInnerProperty)
            PSObject rootObject = HelpParagraphBuilder.GetPropertyObject(this.psObj, inputOrOutputProperty) as PSObject;
            if (rootObject == null)
            object[] inputOrOutputObjs;
            inputOrOutputObjs = HelpParagraphBuilder.GetPropertyObjectArray(rootObject, inputOrOutputInnerProperty);
            if (inputOrOutputObjs == null || inputOrOutputObjs.Length == 0)
            foreach (object inputOrOutputObj in inputOrOutputObjs)
                PSObject inputOrOutput = inputOrOutputObj as PSObject;
                if (inputOrOutput == null)
                string type = HelpParagraphBuilder.GetInnerPSObjectPropertyString(inputOrOutput, "type", "name");
                string description = GetTextFromArray(inputOrOutput, "description");
                this.AddText(HelpParagraphBuilder.AddIndent(type), false);
                    this.AddText(HelpParagraphBuilder.AddIndent(description), false);
        /// Adds the help notes segment.
        private void AddNotes(bool setting, string sectionTitle)
            PSObject rootObject = HelpParagraphBuilder.GetPropertyObject(this.psObj, "alertSet") as PSObject;
            string note = GetTextFromArray(rootObject, "alert");
            if (note == null)
            this.AddText(HelpParagraphBuilder.AddIndent(note), false);
