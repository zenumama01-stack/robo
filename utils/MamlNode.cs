    /// MamlNode is an xml node in MAML schema. Maml schema includes formatting oriented tags like para, list
    /// etc, which needs to be taken care of during display. As a result, xml node in Maml schema can't be
    /// converted into PSObject directly with XmlNodeAdapter.
    /// MamlNode class provides logic in converting formatting tags into the format acceptable by monad format
    /// and output engine.
    /// Following three kinds of formating tags are supported per our agreement with Maml team,
    ///     1. para,
    ///         <para>
    ///             para text here
    ///         </para>
    ///     2. list,
    ///         <list class="ordered|unordered">
    ///             <listItem>
    ///                 <para>
    ///                     listItem Text here
    ///                 </para>
    ///             </listItem>
    ///         </list>
    ///     3. definition list,
    ///         <definitionList>
    ///             <definitionListItem>
    ///                 <term>
    ///                     definition term text here
    ///                 </term>
    ///                 <definition>
    ///                     <para>
    ///                         definition text here
    ///                     </para>
    ///                 </definition>
    ///             </definitionListItem>
    ///         </definitionList>
    /// After processing, content of these three tags will be converted into textItem and its derivations,
    ///     1. para => paraTextItem
    ///         <textItem class="paraTextItem">
    ///             <text>para text here</text>
    ///         </textItem>
    ///     2. list => a list of listTextItem's (which can be ordered or unordered)
    ///         <textItem class="unorderedListTextItem">
    ///             <tag>*</tag>
    ///             <text>text for list item 1</text>
    ///             <text>text for list item 2</text>
    ///     3. definitionList => a list of definitionTextItem's
    ///         <definitionListItem>
    ///             <term>definition term here</term>
    ///             <definition>definition text here</definition>
    ///         </definitionListItem>
    internal class MamlNode
        internal MamlNode(XmlNode xmlNode)
            _xmlNode = xmlNode;
        private readonly XmlNode _xmlNode;
        /// Underline xmlNode for this MamlNode object.
        internal XmlNode XmlNode
                return _xmlNode;
        private PSObject _mshObject;
        /// MshObject which is converted from XmlNode.
        internal PSObject PSObject
                if (_mshObject == null)
                    // There is no XSLT to convert docs to supported maml format
                    // We dont want comments etc to spoil our format.
                    // So remove all unsupported nodes before constructing help
                    RemoveUnsupportedNodes(_xmlNode);
                    _mshObject = GetPSObject(_xmlNode);
                return _mshObject;
        #region Conversion of xmlNode => PSObject
        /// Convert an xmlNode into an PSObject. There are four scenarios,
        ///     1. Null xml, this will return an PSObject wrapping a null object.
        ///     2. Atomic xml, which is an xmlNode with only one simple text child node
        ///         <atomicXml attribute="value">
        ///             atomic xml text
        ///         </atomicXml>
        ///        In this case, an PSObject that wraps string "atomic xml text" will be returned with following properties
        ///             attribute => name
        ///     3. Composite xml, which is an xmlNode with structured child nodes, but not a special case for Maml formatting.
        ///         <compositeXml attribute="attribute">
        ///             <singleChildNode>
        ///                 single child node text
        ///             </singleChildNode>
        ///             <dupChildNode>
        ///                 dup child node text 1
        ///             </dupChildNode>
        ///                 dup child node text 2
        ///         </compositeXml>
        ///        In this case, an PSObject will base generated based on an inside PSObject,
        ///        which in turn has following properties
        ///             a. property "singleChildNode", with its value an PSObject wrapping string "single child node text"
        ///             b. property "dupChildNode", with its value an PSObject array wrapping strings for two dupChildNode's
        ///        The outside PSObject will have property,
        ///             a. property "attribute", with its value an PSObject wrapping string "attribute"
        ///     4. Maml formatting xml, this is a special case for Composite xml, for example
        ///         <description attribute="value">
        ///             <para>
        ///                 para 1
        ///             </para>
        ///             <list>
        ///                 <listItem>
        ///                         list item 1
        ///                 </listItem>
        ///                         list item 2
        ///             </list>
        ///             <definitionList>
        ///                 <definitionListItem>
        ///                     <term>
        ///                         term 1
        ///                     </term>
        ///                     <definition>
        ///                         definition list item 1
        ///                     </definition>
        ///                 </definitionListItem>
        ///                         term 2
        ///                         definition list item 2
        ///             </definitionList>
        ///         </description>
        ///         In this case, an PSObject based on an PSObject array will be created. The inside PSObject array
        ///         will contain following items
        ///             . a MamlParaTextItem based on "para 1"
        ///             . a MamlUnorderedListItem based on "list item 1"
        ///             . a MamlUnorderedListItem based on "list item 2"
        ///             . a MamlDefinitionListItem based on "definition list item 1"
        ///             . a MamlDefinitionListItem based on "definition list item 2"
        ///         The outside PSObject will have a property
        ///             attribute => "value"
        private PSObject GetPSObject(XmlNode xmlNode)
            if (xmlNode == null)
                return new PSObject();
            if (IsAtomic(xmlNode))
                mshObject = new PSObject(xmlNode.InnerText.Trim());
            else if (IncludeMamlFormatting(xmlNode))
                mshObject = new PSObject(GetMamlFormattingPSObjects(xmlNode));
                mshObject = new PSObject(GetInsidePSObject(xmlNode));
                // Add typeNames to this MSHObject and create views so that
                // the output is readable. This is done only for complex nodes.
                if (xmlNode.Attributes["type"] != null)
                    if (string.Equals(xmlNode.Attributes["type"].Value, "field", StringComparison.OrdinalIgnoreCase))
                        mshObject.TypeNames.Add("MamlPSClassHelpInfo#field");
                    else if (string.Equals(xmlNode.Attributes["type"].Value, "method", StringComparison.OrdinalIgnoreCase))
                        mshObject.TypeNames.Add("MamlPSClassHelpInfo#method");
                mshObject.TypeNames.Add("MamlCommandHelpInfo#" + xmlNode.LocalName);
            if (xmlNode.Attributes != null)
                foreach (XmlNode attribute in xmlNode.Attributes)
                    mshObject.Properties.Add(new PSNoteProperty(attribute.Name, attribute.Value));
        /// Get inside PSObject created based on inside nodes of xmlNode.
        /// The inside PSObject will be based on null. It will created one
        /// property per inside node grouping by node names.
        /// For example, for xmlNode like,
        ///     <command>
        ///         <name>get-item</name>
        ///         <note>note 1</note>
        ///         <note>note 2</note>
        ///     </command>
        /// It will create an PSObject based on null, with following two properties
        ///     . property 1: name="name" value=an PSObject to wrap string "get-item"
        ///     . property 2: name="note" value=an PSObject array with following two PSObjects
        ///         1. PSObject wrapping string "note 1"
        ///         2. PSObject wrapping string "note 2"
        private PSObject GetInsidePSObject(XmlNode xmlNode)
            Hashtable properties = GetInsideProperties(xmlNode);
                mshObject.Properties.Add(new PSNoteProperty((string)enumerator.Key, enumerator.Value));
        /// This is for getting inside properties of an XmlNode. Properties are
        /// stored in a hashtable with key as property name and value as property value.
        /// Inside node with same node names will be grouped into one property with
        /// property value as an array.
        /// Since we don't know whether an node name will be used more than once,
        /// We are making each property value is an array (PSObject[]) to start with.
        /// At the end, SimplifyProperties will be called to reduce PSObject[] containing
        /// only one element to PSObject itself.
        private Hashtable GetInsideProperties(XmlNode xmlNode)
            Hashtable properties = new Hashtable(StringComparer.OrdinalIgnoreCase);
                return properties;
            if (xmlNode.ChildNodes != null)
                foreach (XmlNode childNode in xmlNode.ChildNodes)
                    AddProperty(properties, childNode.LocalName, GetPSObject(childNode));
            return SimplifyProperties(properties);
        /// Removes unsupported child nodes recursively from the given
        /// xml node so that they wont spoil the format.
        /// <param name="xmlNode">
        /// Node whose children are verified for maml.
        private static void RemoveUnsupportedNodes(XmlNode xmlNode)
            // Start with the first child..
            // We want to modify only children..
            // The current node is taken care by the callee..
            XmlNode childNode = xmlNode.FirstChild;
            while (childNode != null)
                // We dont want Comments..so remove..
                if (childNode.NodeType == XmlNodeType.Comment)
                    XmlNode nodeToRemove = childNode;
                    childNode = childNode.NextSibling;
                    // Remove this node and its children if any..
                    xmlNode.RemoveChild(nodeToRemove);
                    // Search children...
                    RemoveUnsupportedNodes(childNode);
        /// This is for adding a property into a property hashtable.
        /// As mentioned in comment of GetInsideProperties, property values stored in
        /// property hashtable is an array to begin with.
        /// The property value to be added is an mshObject whose base object can be an
        /// PSObject array itself. In that case, each PSObject in the array will be
        /// added separately into the property value array. This case can only happen when
        /// an node with maml formatting node inside is treated. The side effect of this
        /// is that the properties for outside mshObject will be lost. An example of this
        /// is that,
        /// <command>
        ///     <description attrib1="value1">
        ///         <para></para>
        ///         <list></list>
        ///         <definitionList></definitionList>
        ///     </description>
        /// </command>
        /// After the processing, PSObject corresponding to command will have an property
        /// with name "description" and a value of an PSObject array created based on
        /// maml formatting node inside "description" node. The attribute of description node
        /// "attrib1" will be lost. This seems to be OK with current practice of authoring
        /// monad command help.
        /// <param name="properties">Property hashtable.</param>
        /// <param name="name">Property name.</param>
        /// <param name="mshObject">Property value.</param>
        private static void AddProperty(Hashtable properties, string name, PSObject mshObject)
            ArrayList propertyValues = (ArrayList)properties[name];
            if (propertyValues == null)
                propertyValues = new ArrayList();
                properties[name] = propertyValues;
            if (mshObject.BaseObject is PSCustomObject || !mshObject.BaseObject.GetType().Equals(typeof(PSObject[])))
                propertyValues.Add(mshObject);
            PSObject[] mshObjects = (PSObject[])mshObject.BaseObject;
            for (int i = 0; i < mshObjects.Length; i++)
                propertyValues.Add(mshObjects[i]);
        /// This is for simplifying property value array of only one element.
        /// As mentioned in comments for GetInsideProperties, this is needed
        /// to reduce an array of only one PSObject into the PSObject itself.
        /// A side effect of this function is to turn property values from
        /// ArrayList into PSObject[].
        private static Hashtable SimplifyProperties(Hashtable properties)
            Hashtable result = new Hashtable(StringComparer.OrdinalIgnoreCase);
                ArrayList propertyValues = (ArrayList)enumerator.Value;
                if (propertyValues == null || propertyValues.Count == 0)
                if (propertyValues.Count == 1)
                    if (!IsMamlFormattingPSObject((PSObject)propertyValues[0]))
                        PSObject mshObject = (PSObject)propertyValues[0];
                        // Even for strings or other basic types, they need to be contained in PSObject in case
                        // there is attributes for this object.
                        result[enumerator.Key] = mshObject;
                result[enumerator.Key] = propertyValues.ToArray(typeof(PSObject));
        /// An xmlNode is atomic if it contains no structured inside nodes.
        private static bool IsAtomic(XmlNode xmlNode)
            if (xmlNode.ChildNodes == null)
            if (xmlNode.ChildNodes.Count > 1)
            if (xmlNode.ChildNodes.Count == 0)
            if (xmlNode.ChildNodes[0].GetType().Equals(typeof(XmlText)))
        #region Maml formatting
        /// Check whether an xmlNode contains childnodes which is for
        /// maml formatting.
        private static bool IncludeMamlFormatting(XmlNode xmlNode)
            if (xmlNode.ChildNodes == null || xmlNode.ChildNodes.Count == 0)
                if (IsMamlFormattingNode(childNode))
        /// Check whether a node is for maml formatting. This include following nodes,
        ///     a. para
        ///     b. list
        ///     c. definitionList.
        private static bool IsMamlFormattingNode(XmlNode xmlNode)
            if (xmlNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
            if (xmlNode.LocalName.Equals("list", StringComparison.OrdinalIgnoreCase))
            if (xmlNode.LocalName.Equals("definitionList", StringComparison.OrdinalIgnoreCase))
        /// Check whether an mshObject is created from a maml formatting node.
        private static bool IsMamlFormattingPSObject(PSObject mshObject)
            Collection<string> typeNames = mshObject.TypeNames;
            return typeNames[typeNames.Count - 1].Equals("MamlTextItem", StringComparison.OrdinalIgnoreCase);
        /// Convert an xmlNode containing maml formatting nodes into an PSObject array.
        /// For example, for node,
        ///    <description attribute="value">
        ///        <para>
        ///            para 1
        ///        </para>
        ///        <list>
        ///            <listItem>
        ///                <para>
        ///                    list item 1
        ///                </para>
        ///            </listItem>
        ///                    list item 2
        ///        </list>
        ///        <definitionList>
        ///            <definitionListItem>
        ///                <term>
        ///                    term 1
        ///                </term>
        ///                <definition>
        ///                    definition list item 1
        ///                </definition>
        ///            </definitionListItem>
        ///                    term 2
        ///                    definition list item 2
        ///        </definitionList>
        ///    </description>
        ///    In this case, an PSObject based on an PSObject array will be created. The inside PSObject array
        ///    will contain following items
        ///        . a MamlParaTextItem based on "para 1"
        ///        . a MamlUnorderedListItem based on "list item 1"
        ///        . a MamlUnorderedListItem based on "list item 2"
        ///        . a MamlDefinitionListItem based on "definition list item 1"
        ///        . a MamlDefinitionListItem based on "definition list item 2"
        private PSObject[] GetMamlFormattingPSObjects(XmlNode xmlNode)
            int paraNodes = GetParaMamlNodeCount(xmlNode.ChildNodes);
            // Don't trim the content if this is an "introduction" node.
            bool trim = !string.Equals(xmlNode.Name, "maml:introduction", StringComparison.OrdinalIgnoreCase);
                if (childNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
                    PSObject paraPSObject = GetParaPSObject(childNode, count != paraNodes, trim: trim);
                    if (paraPSObject != null)
                        mshObjects.Add(paraPSObject);
                if (childNode.LocalName.Equals("list", StringComparison.OrdinalIgnoreCase))
                    ArrayList listPSObjects = GetListPSObjects(childNode);
                    for (int i = 0; i < listPSObjects.Count; i++)
                        mshObjects.Add(listPSObjects[i]);
                if (childNode.LocalName.Equals("definitionList", StringComparison.OrdinalIgnoreCase))
                    ArrayList definitionListPSObjects = GetDefinitionListPSObjects(childNode);
                    for (int i = 0; i < definitionListPSObjects.Count; i++)
                        mshObjects.Add(definitionListPSObjects[i]);
                // If we get here, there is some tags that is not supported by maml.
                WriteMamlInvalidChildNodeError(xmlNode, childNode);
            return (PSObject[])mshObjects.ToArray(typeof(PSObject));
        /// Gets the number of para nodes.
        private static int GetParaMamlNodeCount(XmlNodeList nodes)
            foreach (XmlNode childNode in nodes)
                    if (childNode.InnerText.Trim().Equals(string.Empty))
        /// Write an error to helpsystem to indicate an invalid maml child node.
        /// <param name="node"></param>
        /// <param name="childNode"></param>
        private void WriteMamlInvalidChildNodeError(XmlNode node, XmlNode childNode)
            ErrorRecord errorRecord = new ErrorRecord(new ParentContainsErrorRecordException("MamlInvalidChildNodeError"), "MamlInvalidChildNodeError", ErrorCategory.SyntaxError, null);
            errorRecord.ErrorDetails = new ErrorDetails(typeof(MamlNode).Assembly, "HelpErrors", "MamlInvalidChildNodeError", node.LocalName, childNode.LocalName, GetNodePath(node));
            this.Errors.Add(errorRecord);
        /// Write an error to help system to indicate an invalid child node count.
        /// <param name="childNodeName"></param>
        private void WriteMamlInvalidChildNodeCountError(XmlNode node, string childNodeName, int count)
            ErrorRecord errorRecord = new ErrorRecord(new ParentContainsErrorRecordException("MamlInvalidChildNodeCountError"), "MamlInvalidChildNodeCountError", ErrorCategory.SyntaxError, null);
            errorRecord.ErrorDetails = new ErrorDetails(typeof(MamlNode).Assembly, "HelpErrors", "MamlInvalidChildNodeCountError", node.LocalName, childNodeName, count, GetNodePath(node));
        private static string GetNodePath(XmlNode xmlNode)
            if (xmlNode.ParentNode == null)
                return "\\" + xmlNode.LocalName;
            return GetNodePath(xmlNode.ParentNode) + "\\" + xmlNode.LocalName + GetNodeIndex(xmlNode);
        private static string GetNodeIndex(XmlNode xmlNode)
            if (xmlNode == null || xmlNode.ParentNode == null)
            foreach (XmlNode siblingNode in xmlNode.ParentNode.ChildNodes)
                if (siblingNode == xmlNode)
                    index = total++;
                if (siblingNode.LocalName.Equals(xmlNode.LocalName, StringComparison.OrdinalIgnoreCase))
                    total++;
            if (total > 1)
                return "[" + index.ToString("d", CultureInfo.CurrentCulture) + "]";
        /// Convert a para node into an mshObject.
        ///    <para>
        ///        para text
        ///    </para>
        ///    In this case, an PSObject of type "MamlParaTextItem" will be created with following property
        ///        a. text="para text"
        /// <param name="newLine"></param>
        /// <param name="trim"></param>
        private static PSObject GetParaPSObject(XmlNode xmlNode, bool newLine, bool trim = true)
            if (!xmlNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
            if (newLine && !xmlNode.InnerText.Trim().Equals(string.Empty))
                sb.AppendLine(xmlNode.InnerText.Trim());
                var innerText = xmlNode.InnerText;
                if (trim)
                    innerText = innerText.Trim();
                sb.Append(innerText);
            mshObject.Properties.Add(new PSNoteProperty("Text", sb.ToString()));
            mshObject.TypeNames.Add("MamlParaTextItem");
            mshObject.TypeNames.Add("MamlTextItem");
        /// Convert a list node into an PSObject array.
        ///    <list class="ordered">
        ///        <listItem>
        ///            <para>
        ///                text for list item 1
        ///            </para>
        ///        </listItem>
        ///                text for list item 2
        ///    </list>
        /// In this case, an array of PSObject, each of type "MamlOrderedListText" will be created with following
        /// two properties,
        ///        a. tag=" 1. " or " 2. "
        ///        b. text="text for list item 1" or "text for list item 2"
        /// In the case of unordered list, similar PSObject will created with type to be "MamlUnorderedListText" and tag="*"
        private ArrayList GetListPSObjects(XmlNode xmlNode)
                return mshObjects;
            if (!xmlNode.LocalName.Equals("list", StringComparison.OrdinalIgnoreCase))
            bool ordered = IsOrderedList(xmlNode);
            int index = 1;
                if (childNode.LocalName.Equals("listItem", StringComparison.OrdinalIgnoreCase))
                    PSObject listItemPSObject = GetListItemPSObject(childNode, ordered, ref index);
                    if (listItemPSObject != null)
                        mshObjects.Add(listItemPSObject);
        /// Check whether a list is ordered or not.
        private static bool IsOrderedList(XmlNode xmlNode)
            if (xmlNode.Attributes == null || xmlNode.Attributes.Count == 0)
                if (attribute.Name.Equals("class", StringComparison.OrdinalIgnoreCase)
                    && attribute.Value.Equals("ordered", StringComparison.OrdinalIgnoreCase))
        /// Convert an listItem node into an PSObject with property "tag" and "text"
        /// <param name="ordered"></param>
        private PSObject GetListItemPSObject(XmlNode xmlNode, bool ordered, ref int index)
            if (!xmlNode.LocalName.Equals("listItem", StringComparison.OrdinalIgnoreCase))
            string text = string.Empty;
                WriteMamlInvalidChildNodeCountError(xmlNode, "para", 1);
                    text = childNode.InnerText.Trim();
            string tag = string.Empty;
            if (ordered)
                tag = index.ToString("d2", CultureInfo.CurrentCulture);
                tag += ". ";
                tag = "* ";
            mshObject.Properties.Add(new PSNoteProperty("Text", text));
            mshObject.Properties.Add(new PSNoteProperty("Tag", tag));
                mshObject.TypeNames.Add("MamlOrderedListTextItem");
                mshObject.TypeNames.Add("MamlUnorderedListTextItem");
        /// Convert definitionList node into an array of PSObject, an for
        /// each definitionListItem node inside this node.
        private ArrayList GetDefinitionListPSObjects(XmlNode xmlNode)
            if (!xmlNode.LocalName.Equals("definitionList", StringComparison.OrdinalIgnoreCase))
                if (childNode.LocalName.Equals("definitionListItem", StringComparison.OrdinalIgnoreCase))
                    PSObject definitionListItemPSObject = GetDefinitionListItemPSObject(childNode);
                    if (definitionListItemPSObject != null)
                        mshObjects.Add(definitionListItemPSObject);
                // If we get here, we found some node that is not supported.
        /// Convert an definitionListItem node into an PSObject
        /// For example
        ///        <definitionListItem>
        ///            <term>
        ///                term text
        ///            </term>
        ///            <definition>
        ///                    definition text
        ///            </definition>
        ///        </definitionListItem>
        /// In this case, an PSObject of type "definitionListText" will be created with following
        /// properties
        ///        a. term="term text"
        ///        b. definition="definition text"
        private PSObject GetDefinitionListItemPSObject(XmlNode xmlNode)
            if (!xmlNode.LocalName.Equals("definitionListItem", StringComparison.OrdinalIgnoreCase))
            string term = null;
            string definition = null;
                if (childNode.LocalName.Equals("term", StringComparison.OrdinalIgnoreCase))
                    term = childNode.InnerText.Trim();
                if (childNode.LocalName.Equals("definition", StringComparison.OrdinalIgnoreCase))
                    definition = GetDefinitionText(childNode);
            if (string.IsNullOrEmpty(term))
            mshObject.Properties.Add(new PSNoteProperty("Term", term));
            mshObject.Properties.Add(new PSNoteProperty("Definition", definition));
            mshObject.TypeNames.Add("MamlDefinitionTextItem");
        /// Get the text for definition. The will treat some intermediate nodes like "definition" and "para"
        private string GetDefinitionText(XmlNode xmlNode)
            if (!xmlNode.LocalName.Equals("definition", StringComparison.OrdinalIgnoreCase))
        #region Preformatted string processing
        /// This is for getting preformatted text from an xml document.
        /// Normally in xml document, preformatted text will be indented by
        /// a fix amount based on its position. The task of this function
        /// is to remove that fixed amount from the text.
        /// For example, in xml,
        /// <preformatted>
        ///     void function()
        ///         // call some other function here;
        /// </preformatted>
        /// we can find that the preformatted text are indented unanimously
        /// by 4 spaces because of its position in xml.
        /// After massaging in this function, the result text will be,
        /// void function
        ///     // call some other function here;
        /// please notice that the indention is reduced.
        private static string GetPreformattedText(string text)
            // we are assuming tabsize=4 here.
            // It is discouraged to use tab in preformatted text.
            string noTabText = text.Replace("\t", "    ");
            string[] lines = noTabText.Split('\n');
            string[] trimedLines = TrimLines(lines);
            if (trimedLines == null || trimedLines.Length == 0)
            int minIndentation = GetMinIndentation(trimedLines);
            string[] shortedLines = new string[trimedLines.Length];
            for (int i = 0; i < trimedLines.Length; i++)
                if (IsEmptyLine(trimedLines[i]))
                    shortedLines[i] = trimedLines[i];
                    shortedLines[i] = trimedLines[i].Remove(0, minIndentation);
            for (int i = 0; i < shortedLines.Length; i++)
                result.AppendLine(shortedLines[i]);
        /// Trim empty lines from the either end of an string array.
        /// <param name="lines">Lines to trim.</param>
        /// <returns>An string array with empty lines trimed on either end.</returns>
        private static string[] TrimLines(string[] lines)
            if (lines == null || lines.Length == 0)
            for (i = 0; i < lines.Length; i++)
                if (!IsEmptyLine(lines[i]))
            int start = i;
            if (start == lines.Length)
            for (i = lines.Length - 1; i >= start; i--)
            int end = i;
            string[] result = new string[end - start + 1];
            for (i = start; i <= end; i++)
                result[i - start] = lines[i];
        /// Get minimum indentation of a paragraph.
        /// <param name="lines"></param>
        private static int GetMinIndentation(string[] lines)
            int minIndentation = -1;
            for (int i = 0; i < lines.Length; i++)
                if (IsEmptyLine(lines[i]))
                int indentation = GetIndentation(lines[i]);
                if (minIndentation < 0 || indentation < minIndentation)
                    minIndentation = indentation;
            return minIndentation;
        /// Get indentation of a line, i.e., number of spaces
        /// at the beginning of the line.
        /// <param name="line"></param>
        private static int GetIndentation(string line)
            if (IsEmptyLine(line))
            string leftTrimedLine = line.TrimStart(' ');
            return line.Length - leftTrimedLine.Length;
        /// Test whether a line is empty.
        /// A line is empty if it contains only white spaces.
        private static bool IsEmptyLine(string line)
            string trimedLine = line.Trim();
            if (string.IsNullOrEmpty(trimedLine))
        /// maml text.
        internal Collection<ErrorRecord> Errors { get; } = new Collection<ErrorRecord>();
