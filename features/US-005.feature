# Especificación ejecutable — generada por /discovery desde specs/criterios/US-005.yaml
@epic_id:EPIC-001
Feature: Editar el propio perfil
  Como usuario de cualquier rol
  Quiero editar mi propio perfil
  Para mantener mis datos actualizados, sin cambiar mi rol ni mis permisos

  Background:
    Given un usuario autenticado con estado "Activo"

  @story_id:US-005 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Actualizar mis datos personales
    When el usuario abre su propio perfil
    And actualiza su nombre y guarda
    Then el sistema actualiza únicamente su propia cuenta
    And confirma la actualización

  @story_id:US-005 @origin:discovery_inicial @priority:1 @complexity:medium @security @error_handling
  Scenario: No puedo editar el perfil de otro usuario
    When el usuario envía una petición directa para editar la cuenta de otra persona
    Then el backend rechaza la petición con un error de autorización (403)

  @story_id:US-005 @origin:discovery_inicial @priority:1 @complexity:low @security
  Scenario: No puedo cambiar mi propio rol ni mis permisos (R2, R3)
    When el usuario abre su perfil
    Then los campos de rol y estado se muestran como solo lectura
    And no puede modificarlos

  @story_id:US-005 @origin:discovery_inicial @priority:2 @complexity:medium @error_handling
  Scenario: Cambiar mi contraseña exige confirmarla y cumplir la política
    When el usuario cambia su contraseña
    Then la nueva debe cumplir la política (>=8, con mayúscula, minúscula y número)
    And debe confirmarse
    And se recomienda exigir la contraseña actual para autorizar el cambio

  @story_id:US-005 @origin:discovery_inicial @priority:2 @complexity:medium @error_handling
  Scenario: No puedo cambiar mi email a uno ya usado por otra cuenta
    Given existe otra cuenta con email "otro@cidenet.com"
    When el usuario cambia su email a "otro@cidenet.com"
    Then no se guarda el cambio
    And el campo email indica el conflicto

  @story_id:US-005-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:low @security
  Scenario: La respuesta del perfil nunca incluye la contraseña
    When el usuario abre su perfil
    Then la respuesta no contiene el campo contraseña en ninguna forma
