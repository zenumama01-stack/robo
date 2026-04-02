import { Arg, Ctx, Field, InputType, Int, Mutation, ObjectType, registerEnumType } from 'type-graphql';
import { CompositeKey, KeyValuePair, LogError, Metadata, TransactionVariable, BaseEntity, EntityDeleteOptions, EntitySaveOptions } from '@memberjunction/core';
export enum TransactionVariableType {
    Define = "Define",
    Use = "Use",
registerEnumType(TransactionVariableType, {
    name: "TransactionVariableType",  
    description: "Specifies the type of variable: Define or Use",
export enum TransactionOperationType {
    Delete = "Delete"
registerEnumType(TransactionOperationType, {
    name: "TransactionOperationType",  
    description: "Specifies the type of operation: Create, Update, or Delete",
export class TransactionVariableInputType {
    ItemIndex!: number;
    FieldName!: string;
    @Field(() => TransactionVariableType) 
    Type!: TransactionVariableType;
export class TransactionItemInputType {
    EntityObjectJSON: string;
    @Field(() => TransactionOperationType)
    OperationType: TransactionOperationType;
export class TransactionInputType {
    @Field(() => [TransactionItemInputType])
    Items: TransactionItemInputType[];
    @Field(() => [TransactionVariableInputType], {nullable: true})
    Variables?: TransactionVariableInputType[] | null;
export class TransactionOutputType {
    ErrorMessages: string[];
    ResultsJSON: string[];
export class TransactionResolver {
    @Mutation(() => TransactionOutputType)
    async ExecuteTransactionGroup(
    @Arg('group', () => TransactionInputType ) group: TransactionInputType,
            // we have received the transaction group information via the network, now we need to reconstruct our TransactionGroup object and run it
            const entityObjects: BaseEntity[] = [];
            const objectValues: any[] = [];
                // instantiate a new entity object for the item
                const entity = await md.GetEntityObject(item.EntityName, context.userPayload.userRecord);
                entityObjects.push(entity); // save for later for mapping variables if needed
                // get the values from the payload
                const itemValues = SafeJSONParse(item.EntityObjectJSON);
                // build a primary key for the item
                const pkey = new CompositeKey(entity.PrimaryKeys.map(pk => {
                    const kv = new KeyValuePair();
                    kv.FieldName = pk.Name;
                    kv.Value = itemValues[pk.Name];
                    return kv;
                switch (item.OperationType) {
                    case "Update":
                    case "Create":
                        if (item.OperationType === "Update") {
                            await entity.InnerLoad(pkey);
                        objectValues.push(itemValues);
                        entity.SetMany(itemValues, true);
                        entity.TransactionGroup = tg;
                    case "Delete":
                        objectValues.push(entity.GetDataObject());
            // now, we need to set the variables
            if (group.Variables && group.Variables.length > 0) {
                for (const networkVar of group.Variables) {
                    // for each variable, add it to the transaction group and map the index from the network payload to the specific entity object loaded up above
                    if (networkVar.ItemIndex >= 0 && networkVar.ItemIndex < entityObjects.length) {
                        const entityObject = entityObjects[networkVar.ItemIndex];
                        const newVar = new TransactionVariable(networkVar.Name, entityObject, networkVar.FieldName, networkVar.Type);
                        tg.AddVariable(newVar);
                        throw new Error(`TransactionResolver::ExecuteTransactionGroup --- Error\n\n' + 'Invalid ItemIndex ${networkVar.ItemIndex} in TransactionVariable "${JSON.stringify(networkVar)}"`);
            // after all that, we are ready to roll, so let's run the TG
                // success!
                return await this.PrepareReturnValue(true, entityObjects, objectValues, group);
                // failure, send back the results
                return await this.PrepareReturnValue(false, entityObjects, objectValues, group);
            throw new Error('TransactionResolver::ExecuteTransactionGroup --- Error\n\n' + err);
    protected async PrepareReturnValue(success: boolean, entityObjects: BaseEntity[], objectValues: any[], group: TransactionInputType): Promise<TransactionOutputType> {
        const jsonResults = [];
        for (let i = 0; i < group.Items.length; i++) {
            const item = group.Items[i];
            if (item.OperationType==='Delete') {
                jsonResults.push(JSON.stringify(objectValues[i]));
                // create or update, return what is in the database
                jsonResults.push(await entityObjects[i].GetDataObjectJSON());
            ErrorMessages: entityObjects.map(e => JSON.stringify(e.LatestResult)),
            ResultsJSON: jsonResults
