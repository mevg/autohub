module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [
      2,
      'always',
      [
        'feat',     // Nuevas características (ej. US-001)
        'fix',      // Corrección de bugs
        'docs',     // Cambios en documentación
        'style',    // Formateo, faltan puntos y comas, etc. (sin cambios en lógica)
        'refactor', // Refactorización de código de producción
        'perf',     // Cambios de código que mejoran el rendimiento
        'test',     // Añadir o corregir pruebas
        'chore',    // Tareas de mantenimiento, build, dependencias
        'ci',       // Cambios en scripts o workflows de CI/CD
        'revert',   // Revertir un commit anterior
      ],
    ],
    'scope-enum': [
      2,
      'always',
      [
        'backend',   // Cambios exclusivos del Backend .NET
        'frontend',  // Cambios exclusivos de Angular
        'core',      // Transversal / Infraestructura global
        'auth',      // Módulo de autenticación (US-001)
        'inventory', // Módulo de inventario / vehículos (US-010, US-011)
        'deps',      // Actualización de dependencias
      ],
    ],
    'scope-empty': [1, 'never'], // Advertencia si no se especifica un scope
    'subject-case': [2, 'always', 'lower-case'],
  },
};