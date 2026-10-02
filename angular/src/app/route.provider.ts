import { RoutesService, eLayoutType } from '@abp/ng.core';
import { APP_INITIALIZER } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  { provide: APP_INITIALIZER, useFactory: configureRoutes, deps: [RoutesService], multi: true },
];

function configureRoutes(routesService: RoutesService) {
  return () => {
    routesService.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/inventory',
        name: '::Menu:Catalog',
        iconClass: 'fas fa-boxes',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: '/inventory/categories',
        name: '::Menu:Categories',
        requiredPolicy: 'InventoryPos.Categories',
        parentName: '::Menu:Catalog',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/inventory/products',
        name: '::Menu:Products',
        requiredPolicy: 'InventoryPos.Products',
        parentName: '::Menu:Catalog',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: '/inventory/stock',
        name: '::Menu:StockTransactions',
        requiredPolicy: 'InventoryPos.StockTransactions',
        parentName: '::Menu:Catalog',
        order: 3,
        layout: eLayoutType.application,
      },
      {
        path: '/inventory/sales',
        name: '::Menu:SaleOrders',
        requiredPolicy: 'InventoryPos.SaleOrders',
        parentName: '::Menu:Catalog',
        order: 4,
        layout: eLayoutType.application,
      },
    ]);
  };
}
