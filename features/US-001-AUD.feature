# Especificación ejecutable — gap de Completitud (área Auditoría) — desde specs/historias/US-001-AUD.md
@epic_id:EPIC-001 @area:auditoria
Feature: Registro de auditoría (audit trail)
  Como auditor / responsable de gobernanza
  Quiero que toda acción quede registrada
  Para reconstruir después quién hizo qué, sobre quién y cuándo

  @story_id:US-001-AUD @origin:analisis_completitud @area:auditoria @priority:1 @complexity:medium
  Scenario Outline: Registrar cada acción con actor, acción, entidad y fecha
    When se ejecuta la acción "<accion>"
    Then el sistema registra una entrada de auditoría con actor, acción, entidad afectada y marca de tiempo

    Examples:
      | accion                        |
      | crear usuario                 |
      | consultar la tabla de usuarios|
      | editar usuario                |
      | cambiar el rol de un usuario  |
      | activar o desactivar usuario  |
      | eliminar usuario              |
      | configurar la matriz de permisos |
      | iniciar sesión                |

  @story_id:US-001-AUD @origin:analisis_completitud @area:auditoria @priority:1 @complexity:medium @error_handling
  Scenario: El registro de auditoría es inmutable (append-only)
    Given existen entradas de auditoría
    When se intenta editar o borrar una entrada del registro
    Then el sistema no lo permite

  @story_id:US-001-AUD @origin:analisis_completitud @area:auditoria @priority:1 @complexity:medium @edge_case
  Scenario: El registro de auditoría sobrevive al soft-delete de la cuenta
    Given una cuenta con entradas de auditoría
    When la cuenta se elimina lógicamente
    Then sus entradas de auditoría se conservan
