import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'InventoryPos',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44395/',
    redirectUri: baseUrl,
    clientId: 'InventoryPos_App',
    responseType: 'code',
    scope: 'offline_access InventoryPos',
    requireHttps: true,
  },
  apis: {
    default: {
      url: 'https://localhost:44395',
      rootNamespace: 'InventoryPos',
    },
  },
} as Environment;
