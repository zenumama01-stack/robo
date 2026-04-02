import { MJGlobal } from './Global';
 * Decorate your class with this to register it with the MJGlobal class factory.
 * @param key a string that is later used to retrieve a given registration - this should be unique for each baseClass/key combination, if multiple registrations exist for a given baseClass/key combination, the highest priority registration will be used to create class instances
 * @param autoRegisterWithRootClass If true, will automatically register the subclass with the root class of the baseClass hierarchy. This ensures proper priority ordering when multiple subclasses are registered in a hierarchy. Defaults to false to preserve the original registration contract.
 * @returns an instance of the class that was registered for the combination of baseClass/key (with highest priority if more than one)
export function RegisterClass(baseClass: unknown, key: string | null = null, priority: number = 0, skipNullKeyWarning: boolean = false, autoRegisterWithRootClass: boolean = false): (constructor: Function) => void {
    return function (constructor: Function) {
        // Invoke the registration method
        MJGlobal.Instance.ClassFactory.Register(baseClass, constructor, key, priority, skipNullKeyWarning, autoRegisterWithRootClass);
