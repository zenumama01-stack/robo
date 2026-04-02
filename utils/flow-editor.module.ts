import { FFlowModule } from '@foblex/flow';
// Generic components
import { FlowEditorComponent } from './components/flow-editor.component';
import { FlowNodeComponent } from './components/flow-node.component';
import { FlowPaletteComponent } from './components/flow-palette.component';
import { FlowToolbarComponent } from './components/flow-toolbar.component';
import { FlowStatusBarComponent } from './components/flow-status-bar.component';
// Agent-specific components
import { FlowAgentEditorComponent } from './agent-editor/flow-agent-editor.component';
import { AgentPropertiesPanelComponent } from './agent-editor/agent-properties-panel.component';
import { AgentStepListComponent } from './agent-editor/agent-step-list.component';
    // Generic
    FlowEditorComponent,
    FlowNodeComponent,
    FlowPaletteComponent,
    FlowToolbarComponent,
    FlowStatusBarComponent,
    // Agent-specific
    FlowAgentEditorComponent,
    AgentPropertiesPanelComponent,
    AgentStepListComponent
    FFlowModule,
    CodeEditorModule
    // Generic — for any consumer
export class FlowEditorModule { }
