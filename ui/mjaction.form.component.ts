@RegisterClass(BaseFormComponent, 'MJ: Actions') // Tell MemberJunction about this class
    selector: 'gen-mjaction-form',
    templateUrl: './mjaction.form.component.html'
export class MJActionFormComponent extends BaseFormComponent {
            { sectionKey: 'identificationHierarchy', sectionName: 'Identification & Hierarchy', isExpanded: true },
            { sectionKey: 'definitionPrompting', sectionName: 'Definition & Prompting', isExpanded: true },
            { sectionKey: 'codeApproval', sectionName: 'Code & Approval', isExpanded: false },
            { sectionKey: 'displayExecution', sectionName: 'Display & Execution', isExpanded: false },
            { sectionKey: 'params', sectionName: 'Params', isExpanded: false },
            { sectionKey: 'libraries', sectionName: 'Libraries', isExpanded: false },
            { sectionKey: 'resultCodes', sectionName: 'Result Codes', isExpanded: false },
            { sectionKey: 'mJMCPServerTools', sectionName: 'MJ: MCP Server Tools', isExpanded: false },
            { sectionKey: 'scheduledActions', sectionName: 'Scheduled Actions', isExpanded: false },
            { sectionKey: 'actionContexts', sectionName: 'Action Contexts', isExpanded: false },
            { sectionKey: 'entityActions', sectionName: 'Entity Actions', isExpanded: false },
            { sectionKey: 'executionLogs', sectionName: 'Execution Logs', isExpanded: false },
            { sectionKey: 'authorizations', sectionName: 'Authorizations', isExpanded: false },
