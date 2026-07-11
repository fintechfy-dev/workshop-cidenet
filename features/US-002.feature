# Especificación ejecutable — generada por /discovery desde specs/criterios/US-002.yaml
@epic_id:EPIC-001
Feature: Consultar usuarios (tabla)
  Como Admin
  Quiero ver una tabla paginada de usuarios con búsqueda y filtros
  Para encontrar y gestionar cuentas rápidamente

  Background:
    Given un Admin autenticado
    And existen varias cuentas de usuario con distintos roles y estados

  @story_id:US-002 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Ver la tabla paginada con sus columnas
    When el Admin abre la tabla de usuarios
    Then ve las columnas nombre, email, rol y estado por cada usuario
    And la tabla está paginada con 10 usuarios por página
    And ve el total de registros y las acciones por fila (editar, eliminar)

  @story_id:US-002 @origin:discovery_inicial @priority:1 @complexity:medium @data_driven
  Scenario Outline: Buscar por nombre o email de forma parcial y case-insensitive
    When el Admin escribe "<termino>" en el buscador
    Then la tabla muestra "<resultado>"

    Examples:
      | termino   | resultado                                         |
      | ana       | las cuentas cuyo nombre o email contiene "ana"    |
      | ANA       | el mismo resultado que "ana" (case-insensitive)   |
      | @cidenet  | las cuentas cuyo email contiene "@cidenet"        |

  @story_id:US-002 @origin:discovery_inicial @priority:1 @complexity:medium
  Scenario: Combinar filtros de rol y estado con la búsqueda
    When el Admin filtra por rol "Editor" y estado "Activo"
    Then la tabla muestra solo las cuentas Editor activas
    And los filtros se pueden combinar con el término de búsqueda

  @story_id:US-002 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Búsqueda o filtros sin resultados
    When el Admin aplica criterios que no coinciden con ninguna cuenta
    Then la tabla muestra un estado vacío con un mensaje claro
    And no se muestra ningún error

  @story_id:US-002 @origin:discovery_inicial @priority:2 @complexity:low @edge_case
  Scenario: Recortar espacios del término de búsqueda
    When el Admin busca "  ana  "
    Then el sistema recorta los espacios antes de filtrar
    And busca por "ana"

  @story_id:US-002 @origin:discovery_inicial @priority:2 @complexity:medium @edge_case
  Scenario: Solicitar una página fuera de rango
    When el Admin solicita una página que excede el total
    Then el sistema devuelve la página válida más cercana sin fallar

  @story_id:US-002-AUD @origin:analisis_completitud @area:auditoria @priority:1 @complexity:medium @edge_case
  Scenario: La tabla no muestra cuentas eliminadas lógicamente
    Given existe una cuenta que fue eliminada lógicamente (soft-delete)
    When el Admin consulta la tabla de usuarios
    Then la cuenta eliminada no aparece en la operación normal
    But el registro se conserva para auditoría

  @story_id:US-002-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium @security
  Scenario: El Editor ve la tabla en solo lectura, el Viewer no accede
    Given un usuario con rol "Editor" autenticado
    When abre la tabla de usuarios
    Then ve la lista pero sin acciones de gestión (editar/eliminar)
    Given un usuario con rol "Viewer" autenticado
    When intenta acceder a la tabla de usuarios
    Then el sistema le niega el acceso

  @story_id:US-002-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:low @security
  Scenario: La respuesta de la tabla nunca incluye la contraseña
    When el Admin consulta la tabla de usuarios
    Then ninguna fila de la respuesta contiene el campo contraseña en ninguna forma
