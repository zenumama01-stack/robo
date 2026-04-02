import { MJAIActionEntity } from '@memberjunction/core-entities';
import {  } from "@memberjunction/ng-entity-viewer"
@RegisterClass(BaseFormComponent, 'MJ: AI Actions') // Tell MemberJunction about this class
    selector: 'gen-mjaiaction-form',
    templateUrl: './mjaiaction.form.component.html'
export class MJAIActionFormComponent extends BaseFormComponent {
    public record!: MJAIActionEntity;
        this.initSections([
            { sectionKey: 'actionDefinition', sectionName: 'Action Definition', isExpanded: true },
            { sectionKey: 'executionSettings', sectionName: 'Execution Settings', isExpanded: true },
            { sectionKey: 'systemMetadata', sectionName: 'System Metadata', isExpanded: false },
            { sectionKey: 'aIModelActions', sectionName: 'AI Model Actions', isExpanded: false },
            { sectionKey: 'entityAIActions', sectionName: 'Entity AI Actions', isExpanded: false }
