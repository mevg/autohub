# Pendientes — AutoHub

## Urgente

- [ ] Rotar el secreto JWT anterior: estaba commiteado en `appsettings.Development.json` y sigue en el historial de git. Ya se reemplazó por uno nuevo de solo-desarrollo, pero si esa cadena se reutilizó en otro entorno, cambiarla allí.
- [ ] Cambiar la contraseña del admin de desarrollo (`SeedAdmin:Password` en `appsettings.Development.json`, commiteada). Solo afecta a entornos locales, pero no debe llegar a ningún entorno compartido.
- [ ] Definir la estrategia de migraciones en producción: hoy `Program.cs` ejecuta `MigrateAndSeedAsync()` al arrancar. Decidir si se mantiene o se mueve a un flag (`Database:MigrateOnStartup`) o al pipeline de despliegue antes del primer deploy real.
- [x] Commitear el trabajo de autenticación (`33e25c2 feat(auth): ...`).
- [x] Commitear el trabajo de theming frontend + backend de branding + config VS Code (`397db39`, `84ef0e9`, `faa0a0a`).
- [x] Configuración de VS Code para correr y depurar Angular y .NET (`.vscode/` raíz + corrección de `src/frontend/.vscode/` a pnpm).
- [x] Implementar el backend de branding (`GET /api/settings/branding` + `PUT` + upload con política `Admin`, migración `AddBranding`, seed de defaults).
- [x] Commitear el módulo de vehículos backend (`6a96513 feat(backend): add vehicles crud with image upload`).
- [x] Auth frontend: tolerar JWT sin padding + `returnUrl`/`reason=expired` al expulsar (`a341369`).
- [ ] Commitear fix de redirección HTTPS (solo prod redirige; en dev el navegador perdía el `Authorization` en el 307 → 401 + logout al abrir Vehículos). **Requiere reiniciar tu API del 5116 para que tome efecto**.

## Pendientes / avisos

- [ ] `SeedAdmin` sin configurar → el arranque solo registra un warning y no crea admin. Verificar en cada entorno que las variables `SeedAdmin__Email` / `SeedAdmin__Password` existan, o el login no tendrá usuarios.
- [ ] `JwtSettings__Secret` vacío o < 32 bytes → la API falla al arrancar (fail-fast intencional). Asegurar la variable en todos los entornos no-dev.
- [ ] El log `server-20260919-235553.log` en la raíz es un resto anterior: no commitear, borrar.
- [ ] `docker/` (`docker-compose.yml`, Dockerfiles) y `README.md` raíz están vacíos: no usarlos como referencia hasta completarlos.
- [ ] Proyectos `tests/Domain`: sigue el placeholder `UnitTest1` sin aserciones. Agregar tests cuando `Domain` tenga lógica o eliminarlo.
- [ ] `app.html` sigue siendo el placeholder de Angular con estilos inline: reemplazarlo por el layout real que consuma el branding (`bg-primary`, logo, nombre).
- [ ] El panel `/admin/branding` (paso 5) presupone login + guardas de rol en el frontend, que aún no existen.
- [ ] `danger` (rojo medio `#EE4040`) quedó fijo por diseño, no es editable desde branding. Si debe ser configurable, agregarlo al modelo.
- [x] Sin definir: tamaño máximo de upload (fijado en 5 MB; slots `logo|favicon|banner`, máx. 5 banners), URLs de assets relativas al mismo origen (`/branding/...`).
- [ ] Los assets se guardan en `wwwroot/branding/` local: con múltiples instancias o contenedores efímeros se necesita volumen compartido u object storage.
- [ ] CI frontend usa `pnpm run build -- --configuration production`: con pnpm 11.5.2 esa forma falla en validación de schema (verificado localmente); funciona `pnpm run build --configuration=production` o `pnpm exec ng build --configuration production`. Ajustar `frontend-ci.yml` o fijar sintaxis.
- [ ] Dapper: `COUNT(*)` devuelve `bigint` → castear a `::int` en SQL o el mapeo a `int` revienta (ver `DapperVehicleQueries`). Identificadores PascalCase (`"Vehicles"`, `"primary"`) siempre comillados.
- [ ] EF: para hijos en agregados usar `Add` explícito del store (`AddImage`), no solo `collection.Add` (verificado: sin eso EF emitió UPDATE contra fila inexistente).

## Nice to have

- [ ] Refresh tokens + revocación (hoy solo access token de 8h).
- [ ] Endpoint de registro para admins (hoy alta solo vía seed) y gestión de roles.
- [ ] Política de contraseñas (longitud/complejidad) en un validator de registro/seed.
- [ ] Rate limiting en `POST /api/auth/login` contra fuerza bruta.
- [x] Login y guardas de ruta en el frontend Angular (implementados: `/login`, `authGuard`/`guestGuard`, interceptor; falta registro y panel `/admin/branding`).
- [ ] Auditoría de logins (último acceso, intentos fallidos).
- [ ] Purga del secreto viejo del historial de git (`git filter-repo` o rotación documentada) si el repo se vuelve público.
- [ ] Object storage (S3 o similar) para logo/banners en lugar de disco local.
- [ ] Preview en vivo y drag&drop en el panel de branding.
- [ ] Modo oscuro derivado de la paleta de marca.
- [ ] Branding multi-tenant (una marca por cliente) si el producto lo pide.
