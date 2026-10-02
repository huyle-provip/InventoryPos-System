import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { map, take } from 'rxjs/operators';

export const policyGuard: CanActivateFn = route => {
  const permissionService = inject(PermissionService);
  const router = inject(Router);
  const toaster = inject(ToasterService);
  const policy = route.data['requiredPolicy'] as string;

  return permissionService.getGrantedPolicy$(policy).pipe(
    take(1),
    map(granted => {
      if (granted) {
        return true;
      }

      toaster.warn('You do not have permission to open that page.');
      return router.parseUrl('/');
    }),
  );
};
