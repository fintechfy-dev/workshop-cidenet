# Especificación ejecutable — gap de Completitud (área Mantenimiento) — desde specs/historias/US-001-MNT.md
@epic_id:EPIC-001 @area:mantenimiento
Feature: Arranque del sistema y mantenimiento de datos
  Como operador del sistema
  Quiero un arranque bien definido y un mantenimiento sostenible
  Para que el sistema sea usable desde el despliegue

  @story_id:US-001-MNT @origin:analisis_completitud @area:mantenimiento @priority:1 @complexity:medium @smoke
  Scenario: El sistema arranca con un Admin inicial sembrado
    Given una base de datos recién desplegada y vacía
    Then existe un usuario Admin inicial sembrado (seed)
    And es posible iniciar sesión con él para crear los demás usuarios

  @story_id:US-001-MNT @origin:analisis_completitud @area:mantenimiento @priority:1 @complexity:medium @security
  Scenario: El Admin inicial debe cambiar su contraseña en el primer acceso
    Given el Admin inicial con la contraseña sembrada
    When inicia sesión por primera vez
    Then el sistema le exige cambiar la contraseña antes de permitirle operar

  @story_id:US-001-MNT @origin:analisis_completitud @area:mantenimiento @priority:3 @complexity:medium @edge_case
  Scenario: Un proceso de retención puede purgar registros eliminados hace tiempo
    Given registros con soft-delete más antiguos que la política de retención (p. ej. 12 o 24 meses)
    When se ejecuta el proceso de retención configurado
    Then esos registros pueden purgarse físicamente
    But no forma parte del flujo de eliminación por defecto
