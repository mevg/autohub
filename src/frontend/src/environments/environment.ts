/**
 * Entorno de desarrollo. En CI se genera `environment.prod.ts` con la versión
 * de GitVersion (ver `frontend-ci.yml`); cablearlo con `fileReplacements`
 * en `angular.json` cuando se quiera versión real en producción.
 */
export const environment = {
  production: false,
  version: '0.0.0',
};
