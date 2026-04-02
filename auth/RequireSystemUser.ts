import { DirectiveBuilder } from '../types.js';
const DIRECTIVE_NAME = 'RequireSystemUser';
export function RequireSystemUser(): PropertyDecorator & MethodDecorator & ClassDecorator;
export function RequireSystemUser(): PropertyDecorator | MethodDecorator | ClassDecorator {
  return (targetOrPrototype, propertyKey, descriptor) => Directive(`@${DIRECTIVE_NAME}`)(targetOrPrototype, propertyKey, descriptor);
export const requireSystemUserDirective: DirectiveBuilder = {
        ...fieldConfig,
        resolve: (source, args, context, info) => {
            if (!context.userPayload.isSystemUser) {
              throw new AuthorizationError('Operation not permitted for this user');
          return fieldConfig.resolve(source, args, context, info);
