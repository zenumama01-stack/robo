package org.openhab.core.model.thing.serializer;
import org.eclipse.xtext.Keyword;
import org.eclipse.xtext.nodemodel.ILeafNode;
import org.eclipse.xtext.serializer.analysis.ISyntacticSequencerPDAProvider.ISynNavigable;
public class ThingSyntacticSequencerExtension extends AbstractThingSyntacticSequencer {
    protected void emit_ModelThing_ThingKeyword_0_q(EObject semanticObject, ISynNavigable transition,
            List<INode> nodes) {
        ILeafNode node = nodes != null && nodes.size() == 1 && nodes.getFirst() instanceof ILeafNode
                ? (ILeafNode) nodes.getFirst()
        Keyword keyword = grammarAccess.getModelThingAccess().getThingKeyword_0();
        acceptUnassignedKeyword(keyword, keyword.getValue(), node);
