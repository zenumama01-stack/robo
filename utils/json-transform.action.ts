import { JSONPath } from "jsonpath-plus";
import jmespath from "jmespath";
 * Action that transforms JSON data using JSONPath or JMESPath expressions
 * Enables powerful querying and transformation of JSON structures
 * // Extract values using JSONPath
 *   ActionName: 'JSON Transform',
 *     Value: { users: [{ name: 'John', age: 30 }, { name: 'Jane', age: 25 }] }
 *     Name: 'Expression',
 *     Value: '$.users[?(@.age > 26)].name'
 * // Transform using JMESPath
 *     Value: { items: [{ price: 10, qty: 2 }, { price: 20, qty: 1 }] }
 *     Value: 'items[].{total: price * qty}'
 *     Name: 'TransformType',
 *     Value: 'JMESPath'
@RegisterClass(BaseAction, "JSON Transform")
export class JSONTransformAction extends BaseAction {
     * Transforms JSON data using JSONPath or JMESPath expressions
     *   - InputData: JSON object or array to transform
     *   - Expression: JSONPath or JMESPath query expression
     *   - TransformType: "JSONPath" | "JMESPath" (default: "JSONPath")
     *   - Multiple: Boolean - return all matches vs first match (JSONPath only, default: true)
     *   - Flatten: Boolean - flatten array results (default: false)
     *   - WrapScalar: Boolean - wrap scalar results in object (default: false)
     * @returns Transformed data based on the expression
            let inputData: any;
            const expressionParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'expression');
            const transformTypeParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'transformtype');
            const multipleParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'multiple');
            const flattenParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'flatten');
            const wrapScalarParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'wrapscalar');
            if (!expressionParam?.Value) {
                    Message: "Expression parameter is required",
            const expression = expressionParam.Value.toString();
            const transformType = (transformTypeParam?.Value?.toString() || 'JSONPath').toUpperCase();
            const multiple = multipleParam?.Value?.toString()?.toLowerCase() === 'true';
            const flatten = flattenParam?.Value?.toString()?.toLowerCase() === 'true';
            const wrapScalar = wrapScalarParam?.Value?.toString()?.toLowerCase() === 'true';
            if (transformType === 'JSONPATH') {
                // Use JSONPath
                        path: expression,
                        json: inputData,
                        resultType: (multiple ? 'all' : 'value') as 'all' | 'value',
                        wrap: false,
                        flatten: flatten
                    result = JSONPath(options);
                    // Handle empty results
                    if (result === undefined || (Array.isArray(result) && result.length === 0)) {
                        Message: `JSONPath error: ${error instanceof Error ? error.message : String(error)}`,
                        ResultCode: "EXPRESSION_ERROR"
            } else if (transformType === 'JMESPATH') {
                // Use JMESPath
                    result = jmespath.search(inputData, expression);
                        Message: `JMESPath error: ${error instanceof Error ? error.message : String(error)}`,
                    Message: `Invalid TransformType: ${transformType}. Must be 'JSONPath' or 'JMESPath'`,
            // Wrap scalar results if requested
            if (wrapScalar && (typeof result === 'string' || typeof result === 'number' || typeof result === 'boolean')) {
                result = { value: result };
                transformType: transformType,
                expression: expression,
                inputType: Array.isArray(inputData) ? 'array' : typeof inputData,
                resultType: result === null ? 'null' : Array.isArray(result) ? 'array' : typeof result
            // Add match count for arrays
                output['matchCount'] = result.length;
                Message: `Failed to transform JSON: ${error instanceof Error ? error.message : String(error)}`,
