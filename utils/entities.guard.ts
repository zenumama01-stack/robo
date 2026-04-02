  ActivatedRouteSnapshot,
  RouterStateSnapshot,
} from '@angular/router';
import {EntityPermissionType, Metadata} from "@memberjunction/core";
export function checkUserEntityPermissions(type: EntityPermissionType): (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => any {
    return (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
      const appName = route.params['appName'];
      const entityName = route.params['entityName'];
      if (md && md.Entities) {
        const entity = md.Entities.find(e => {
          return e.Name === entityName 
        const permissions = entity?.GetUserPermisions(md.CurrentUser);
        if (permissions) {
          let bAllowed: boolean = false;
            case EntityPermissionType.Create:
              bAllowed = permissions.CanCreate;
            case EntityPermissionType.Read:
              bAllowed = permissions.CanRead;
            case EntityPermissionType.Update:
              bAllowed = permissions.CanUpdate;
            case EntityPermissionType.Delete:
              bAllowed = permissions.CanDelete;
          return bAllowed
        return false; // entity metadata not loaded yet
