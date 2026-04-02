import { Component, Input, OnInit, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { GridComponent, AddEvent, EditEvent, CancelEvent, SaveEvent, RemoveEvent } from '@progress/kendo-angular-grid';
import { FormGroup, FormControl, Validators } from '@angular/forms';
    selector: 'mj-template-params-grid',
    templateUrl: './template-params-grid.component.html',
    styleUrls: ['./template-params-grid.component.css']
export class TemplateParamsGridComponent implements OnInit, OnChanges {
    @Input() editMode: boolean = false;
    @ViewChild(GridComponent) grid!: GridComponent;
    // Grid editing
    public editedRowIndex: number | undefined = undefined;
    public editedParam: MJTemplateParamEntity | null = null;
    public formGroup: FormGroup | undefined;
    // Type options for dropdown
    public typeOptions = [
        { text: 'Scalar', value: 'Scalar' },
        { text: 'Array', value: 'Array' },
        { text: 'Object', value: 'Object' },
        { text: 'Record', value: 'Record' },
        { text: 'Entity', value: 'Entity' }
    constructor() {}
        if (this.template?.ID) {
        if (changes['template'] && this.template?.ID) {
                console.error('Failed to load template params:', results.ErrorMessage);
                    'Failed to load template parameters',
    // Grid editing handlers
    public addHandler(args: AddEvent): void {
        if (!this.editMode || !this.template?.ID) return;
        // Create new parameter entity
        this.createNewParam().then(newParam => {
            if (newParam) {
                // Close any existing edits
                this.closeEditor();
                // Add to array at the beginning
                this.templateParams = [newParam, ...this.templateParams];
                // Enter edit mode for the new row
                this.editedRowIndex = 0;
                this.editedParam = newParam;
                this.formGroup = this.createFormGroup(newParam);
                // Close the add new row
                this.grid.closeRow(args.rowIndex);
                // Edit the newly added row
                this.grid.editRow(0, this.formGroup);
    public editHandler(args: EditEvent): void {
        if (!this.editMode) return;
        const { dataItem, rowIndex } = args;
        // Set up editing
        this.editedRowIndex = rowIndex;
        this.editedParam = dataItem;
        this.formGroup = this.createFormGroup(dataItem);
    public cancelHandler(args: CancelEvent): void {
        const { rowIndex, dataItem } = args;
        // If this is a new unsaved parameter, remove it
        if (!dataItem.ID && rowIndex !== undefined) {
            this.templateParams.splice(rowIndex, 1);
            this.templateParams = [...this.templateParams];
    public async saveHandler(args: SaveEvent): Promise<void> {
        if (!this.formGroup || !this.formGroup.valid) return;
        const formValue = this.formGroup.value;
        // Update the entity with form values
        dataItem.Name = formValue.name;
        dataItem.Type = formValue.type;
        dataItem.IsRequired = formValue.isRequired;
        dataItem.Description = formValue.description;
        dataItem.DefaultValue = formValue.defaultValue;
        // Handle linked parameter fields for Entity type
        if (formValue.type === 'Entity') {
            dataItem.LinkedParameterName = formValue.linkedParameterName;
            dataItem.LinkedParameterField = formValue.linkedParameterField;
            dataItem.LinkedParameterName = null;
            dataItem.LinkedParameterField = null;
            const saved = await dataItem.Save();
                    `Parameter "${dataItem.Name}" saved successfully`,
                // Update the array to trigger change detection
                    `Failed to save parameter: ${dataItem.LatestResult?.Message || 'Unknown error'}`,
                // Reload to revert changes
            console.error('Error saving parameter:', error);
                'Error saving parameter',
    public async removeHandler(args: RemoveEvent): Promise<void> {
        const param = args.dataItem as MJTemplateParamEntity;
        if (!confirm(`Are you sure you want to delete the parameter "${param.Name}"?`)) {
            if (param.ID) {
                        `Parameter "${param.Name}" deleted successfully`,
                    // Remove from array
                    const index = this.templateParams.indexOf(param);
                        this.templateParams.splice(index, 1);
                        `Failed to delete parameter: ${param.LatestResult?.Message || 'Unknown error'}`,
    private createFormGroup(param: MJTemplateParamEntity): FormGroup {
            name: new FormControl(param.Name, Validators.required),
            type: new FormControl(param.Type || 'Scalar', Validators.required),
            isRequired: new FormControl(param.IsRequired || false),
            description: new FormControl(param.Description),
            defaultValue: new FormControl(param.DefaultValue),
            linkedParameterName: new FormControl(param.LinkedParameterName),
            linkedParameterField: new FormControl(param.LinkedParameterField)
    private closeEditor(): void {
        if (this.editedRowIndex !== undefined) {
            this.grid.closeRow(this.editedRowIndex);
        this.editedRowIndex = undefined;
        this.editedParam = null;
        this.formGroup = undefined;
    private async createNewParam(): Promise<MJTemplateParamEntity | null> {
        if (!this.template?.ID) return null;
            const newParam = await md.GetEntityObject<MJTemplateParamEntity>('MJ: Template Params');
            newParam.TemplateID = this.template.ID;
            newParam.Type = 'Scalar';
            return newParam;
            console.error('Error creating new parameter:', error);
    public isInEditMode(rowIndex: number): boolean {
        return this.editedRowIndex === rowIndex;
    // Helper for displaying type with icon
    public getTypeIcon(type: string): string {
            case 'Record': return 'fa-file';
    // Helper for type descriptions
    public getTypeDescription(type: string): string {
            case 'Scalar': return 'Single value (text, number, etc.)';
            case 'Array': return 'List of values';
            case 'Object': return 'JSON object';
            case 'Record': return 'Single record from an entity';
            case 'Entity': return 'Multiple records from an entity';
