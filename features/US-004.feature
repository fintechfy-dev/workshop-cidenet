# Especificación ejecutable — generada por /discovery desde specs/criterios/US-004.yaml
@epic_id:EPIC-001
Feature: Eliminar cuenta de usuario
  Como Admin
  Quiero eliminar una cuenta de usuario con confirmación explícita
  Para revocar el acceso de personas que ya no deben tenerlo

  Background:
    Given un Admin autenticado
    And existen varias cuentas de usuario

  @story_id:US-004 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Eliminar una cuenta con confirmación explícita
    When el Admin pulsa eliminar en la fila de un usuario
    Then se abre un modal que muestra nombre, email y rol del usuario
    When el Admin confirma explícitamente
    Then la cuenta se elimina lógicamente y desaparece de la tabla
    And el sistema confirma la operación

  @story_id:US-004 @origin:discovery_inicial @priority:1 @complexity:high @error_handling
  Scenario: No se puede eliminar el último Admin (R1)
    Given solo existe un usuario con rol "Admin" activo
    When el Admin intenta eliminar esa cuenta
    Then la operación se bloquea
    And se muestra un mensaje: "No se puede eliminar el último administrador del sistema"

  @story_id:US-004 @origin:discovery_inicial @priority:2 @complexity:medium @error_handling @security
  Scenario: Un Admin no puede eliminar su propia cuenta
    Given un Admin autenticado
    When intenta eliminar su propia cuenta
    Then el sistema bloquea la operación
    And le indica que debe hacerlo otro Admin

  @story_id:US-004 @origin:discovery_inicial @priority:2 @complexity:medium @edge_case
  Scenario: Eliminar una cuenta que ya fue eliminada en paralelo
    Given otro Admin ya eliminó la cuenta objetivo
    When el Admin confirma la eliminación de esa cuenta
    Then el sistema informa que la cuenta ya no existe, sin fallar

  @story_id:US-004 @origin:discovery_inicial @priority:2 @complexity:low @ux
  Scenario: El modal advierte cuando el usuario a eliminar es Admin
    Given hay más de un Admin activo
    When el Admin abre el modal de eliminación de otro Admin
    Then el modal muestra una advertencia destacada por tratarse de un Admin

  @story_id:US-004-AUD @origin:analisis_completitud @area:auditoria @priority:1 @complexity:medium @edge_case
  Scenario: La eliminación es lógica y conserva el registro para auditoría
    When el Admin elimina una cuenta
    Then la cuenta se marca como eliminada (soft-delete)
    And el registro no se borra físicamente
    And nada de lo que dependía de la cuenta se pierde

  @story_id:US-004-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium @security
  Scenario: Solo Admin puede eliminar cuentas
    Given un usuario con rol "Editor" autenticado
    When envía una petición directa para eliminar una cuenta
    Then el backend rechaza la petición con un error de autorización (403)
