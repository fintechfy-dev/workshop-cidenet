# Especificación ejecutable — gap de Completitud (área Monitoreo) — desde specs/historias/US-001-MON.md
@epic_id:EPIC-001 @area:monitoreo
Feature: Observabilidad y alertas
  Como equipo de operación
  Quiero observar eventos críticos y recibir alertas
  Para reaccionar a tiempo ante ataques, riesgos de gobernanza o fallos

  @story_id:US-001-MON @origin:analisis_completitud @area:monitoreo @priority:2 @complexity:medium
  Scenario: Alertar ante un pico de inicios de sesión fallidos
    Given un umbral de intentos fallidos de login en una ventana de tiempo
    When los intentos fallidos superan ese umbral
    Then el sistema genera una alerta (posible ataque de fuerza bruta)

  @story_id:US-001-MON @origin:analisis_completitud @area:monitoreo @priority:2 @complexity:medium
  Scenario: Alertar cuando quedan muy pocos Admin activos
    When el número de Admin activos cae a uno (o por debajo del umbral definido)
    Then el sistema genera una alerta de riesgo de gobernanza

  @story_id:US-001-MON @origin:analisis_completitud @area:monitoreo @priority:2 @complexity:medium @error_handling
  Scenario: Observar y alertar los errores de backend en operaciones
    When ocurre un error de backend al crear, editar o eliminar una cuenta
    Then el evento queda observable
    And puede disparar una alerta al equipo
