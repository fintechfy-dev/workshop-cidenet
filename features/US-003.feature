# Especificación ejecutable — generada por /discovery desde specs/criterios/US-003.yaml
@epic_id:EPIC-001
Feature: Editar cuenta de usuario
  Como Admin
  Quiero editar los datos, el rol y el estado de un usuario
  Para mantener la información y los accesos al día

  Background:
    Given un Admin autenticado
    And existe una cuenta de usuario editable

  @story_id:US-003 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Editar los datos de un usuario con éxito
    When el Admin abre la edición de un usuario
    Then el formulario viene pre-cargado con nombre, email, rol y estado actuales
    When modifica el nombre y guarda
    Then la cuenta se actualiza y el cambio se refleja en la tabla
    And el sistema confirma la actualización

  @story_id:US-003 @origin:discovery_inicial @priority:1 @complexity:medium @error_handling
  Scenario: Rechazar cambio de email a uno ya usado por otra cuenta (R4)
    Given existe otra cuenta con email "otro@cidenet.com"
    When el Admin cambia el email del usuario a "otro@cidenet.com"
    Then no se guarda el cambio
    And el campo email indica el conflicto

  @story_id:US-003 @origin:discovery_inicial @priority:1 @complexity:medium @error_handling @security
  Scenario: Un Admin no puede modificar su propio rol (R2)
    Given el Admin edita su propia cuenta
    Then el selector de rol está deshabilitado
    When intenta cambiar su propio rol mediante una petición directa
    Then el sistema rechaza el cambio

  @story_id:US-003 @origin:discovery_inicial @priority:1 @complexity:high @error_handling
  Scenario: No se puede quitar el rol Admin al último Admin (derivada de R1)
    Given solo existe un usuario con rol "Admin" activo
    When se intenta cambiar el rol de ese usuario a "Editor"
    Then el sistema bloquea el cambio con un mensaje explicativo
    And el sistema conserva al menos un Admin

  @story_id:US-003 @origin:discovery_inicial @priority:2 @complexity:low @ux
  Scenario: El botón Guardar se mantiene deshabilitado sin cambios o con campos inválidos
    Given el formulario de edición no tiene cambios
    Then el botón Guardar está deshabilitado

  @story_id:US-003 @origin:discovery_inicial @priority:2 @complexity:medium @edge_case
  Scenario: Cambiar el email a una variante que solo difiere en mayúsculas es el mismo email
    Given el usuario tiene email "ana@cidenet.com"
    When el Admin cambia el email a "ANA@cidenet.com"
    Then se considera el mismo email y no hay conflicto de unicidad

  @story_id:US-003-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium @security
  Scenario: Solo Admin puede editar cuentas de otros usuarios
    Given un usuario con rol "Editor" autenticado
    When envía una petición directa para editar la cuenta de otro usuario
    Then el backend rechaza la petición con un error de autorización (403)
