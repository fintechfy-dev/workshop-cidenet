# Especificación ejecutable — generada por /discovery desde specs/criterios/US-006.yaml
@epic_id:EPIC-001
Feature: Configurar la matriz de permisos por rol
  Como Admin
  Quiero activar/desactivar permisos por rol en una matriz (recurso x acción)
  Para ajustar qué puede hacer cada rol sobre cada recurso

  Background:
    Given un Admin autenticado
    And la matriz de permisos base del sistema (Admin, Editor, Viewer x users, roles, permissions, reports)

  @story_id:US-006 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Ver la matriz de permisos con su estado actual
    When el Admin abre la matriz de permisos
    Then ve, por cada rol y recurso, las 4 acciones (Create, Read, Update, Delete) con su estado actual
    And los toggles reflejan la configuración vigente

  @story_id:US-006 @origin:discovery_inicial @priority:1 @complexity:medium
  Scenario: Activar o desactivar un permiso individual
    When el Admin desactiva el permiso "reports:Create" para el rol "Editor"
    Then el cambio se persiste
    And el sistema confirma la actualización

  @story_id:US-006 @origin:discovery_inicial @priority:1 @complexity:high
  Scenario: Un cambio de permiso se refleja en la autorización efectiva
    Given el Admin retira el permiso "users:Update" al rol "Editor"
    When un Editor intenta actualizar un contenido que requiere ese permiso
    Then el sistema le niega la acción

  @story_id:US-006 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Los roles son predefinidos y no se pueden crear ni eliminar
    When el Admin intenta crear un rol nuevo o eliminar uno existente
    Then el sistema no lo permite
    And solo deja configurar los permisos de los roles Admin, Editor y Viewer

  @story_id:US-006 @origin:discovery_inicial @priority:1 @complexity:high @security @error_handling
  Scenario: Un Admin no puede asignarse permisos a sí mismo (R3)
    When el Admin intenta modificar los permisos del rol Admin (el rol que él mismo posee)
    Then el sistema bloquea el cambio para evitar la auto-asignación de privilegios

  @story_id:US-006-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:high @error_handling
  Scenario: No se puede dejar al rol Admin sin sus permisos de gestión (anti-lockout)
    When se intenta desactivar los permisos de gestión esenciales del rol Admin sobre users/roles/permissions
    Then el sistema rechaza el cambio con un mensaje explicativo
    And evita bloquear la administración del sistema

  @story_id:US-006-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium @security
  Scenario: Solo Admin accede a la matriz de permisos
    Given un usuario con rol "Editor" autenticado
    When envía una petición directa para modificar la matriz de permisos
    Then el backend rechaza la petición con un error de autorización (403)
