import { Metadata, TransactionGroupBase, TransactionResult } from "@memberjunction/core";
import { SQLServerDataProvider } from "./SQLServerDataProvider";
 * SQL Server implementation of the TransactionGroupBase
export class SQLServerTransactionGroup extends TransactionGroupBase {
        const items = this.PendingTransactions;
        if (items.length > 0) {
            const pool: sql.ConnectionPool = items[0].ExtraData.dataSource; // Now expects a ConnectionPool
            // start a transaction, if anything fails we'll handle the rollback
                if (this.Variables.length > 0) {
                    // need to execute in order since there are dependencies between the transaction items for the given variables
                        // execute the individual query
                        let result, bSuccess: boolean = false;
                            const numValueSet = this.SetEntityValuesFromVariables(item.BaseEntity); // set the variables that this item needs
                            if (numValueSet > 0 && item.OperationType !== 'Delete') {
                                // for creates/updates where we set 1+ variable into the entity, we need to update the instruction
                                // GetSaveSQL is async because it may need to encrypt field values
                                const bCreate = item.OperationType === 'Create';
                                const spName = sqlProvider.GetCreateUpdateSPName(item.BaseEntity, bCreate);
                                const newInstruction = await sqlProvider.GetSaveSQL(item.BaseEntity, bCreate, spName, item.BaseEntity.ContextCurrentUser);
                                item.Instruction = newInstruction; // update the instruction with the new values
                            // Create a request for this transaction
                            const request = new sql.Request(transaction);
                            // Add parameters if any
                            if (item.Vars && Array.isArray(item.Vars)) {
                                item.Vars.forEach((value, index) => {
                                item.Instruction = item.Instruction.replace(/\?/g, () => `@p${paramIndex++}`);
                            // Log the SQL statement before execution
                            const description = `${item.OperationType} ${item.ExtraData?.entityName || 'entity'} (Transaction Group)`;
                            await SQLServerDataProvider.LogSQLStatement(
                                item.Instruction,
                                item.Vars,
                                true, // isMutation
                                item.ExtraData?.simpleSQLFallback
                            const queryResult = await request.query(item.Instruction);
                            const rawResult = queryResult.recordset;
                            if (rawResult && rawResult.length > 0) {
                                // Process the result to handle timezone conversions and decryption
                                result = await sqlProvider.ProcessEntityRows(rawResult, item.BaseEntity.EntityInfo, item.BaseEntity.ContextCurrentUser);
                                this.SetVariableValuesFromEntity(item.BaseEntity, result[0]); // set the variables that this item defines after the save is done
                            bSuccess = (result && result.length > 0); // success if we have a result and it has rows 
                            result = e; // push the exception to the result
                            bSuccess = false; // mark as failed
                            // CRITICAL FIX: Immediately rollback on first failure
                                LogError(`Failed to rollback after operation error: ${rollbackError}`);
                            // Create result for the failed operation
                            returnResults.push(new TransactionResult(item, result, bSuccess));
                            // Throw error immediately to stop processing
                            throw new Error(`Transaction rolled back due to operation failure: ${errorMessage}`);
                        // save the results
                        returnResults.push(new TransactionResult(item, result && result.length > 0 ? result[0] : result, bSuccess));
                    // execute individually since there are no variable dependencies, but we want to avoid 
                    // variable conflicts between different stored procedure calls that might use same variable names
                        let result: any = null, bSuccess: boolean = false;
                                const modifiedInstruction = item.Instruction.replace(/\?/g, () => `@p${paramIndex++}`);
                                    modifiedInstruction,
                                const queryResult = await request.query(modifiedInstruction);
                // NOTE: Failure checking is now handled immediately in catch blocks above
                // If we reach this point, all operations succeeded
                // Enhanced error handling for commit failures or operation failures
                // Note: If this is an operation failure, the transaction may already be rolled back
                    // Only attempt rollback if the error doesn't indicate transaction was already rolled back
                    if (!errorMessage.includes('Transaction rolled back due to operation failure')) {
                    LogError(`Failed to rollback after commit error: ${rollbackError}`);
                // Re-throw the original error (which may already indicate rollback occurred)
                    throw new Error(`Transaction failed: ${String(error)}. All changes have been rolled back.`);
