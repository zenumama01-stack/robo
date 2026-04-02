import { PropertyConstraint } from './component-constraints';
 * Definition of a single property of a component.
export interface ComponentProperty {
     * The name of the property
     * A description of what this property is used for
     * The type of the property.
     * It can be one of 'string', 'number', 'boolean', 'object', 'array', 'function', or 'any'.
     * For complex types, use extended syntax like "Array<string | ColumnDef>".
     * These types are for aligning users of the component. Components are in JavaScript and do not
     * actually enforce types at runtime, but this is used to provide guidance for users (AI and Human)
     * Indicates if this property is required for the component to function correctly.
     * If true, the component will not work without this property being set.
     * The default value, if any, for this property if it is not provided.
     * Optional list of possible values for this property.
    possibleValues?: string[];
     * Optional constraints that validate this property's value at lint-time.
     * Used to catch errors early by validating prop values against business rules.
 * Definition of a single event of a component.
export interface ComponentEvent {
     * The name of the event
     * A description of what this event does
     * An array of parameters that this event can emit.
    parameters?: ComponentEventParameter[];
export interface ComponentEventParameter {
     * A description of what this parameter is used for
     * The type of the parameter. 
    type: 'string' | 'number' | 'boolean' | 'object' | 'array' | 'function' | 'any';
