# Especificación ejecutable — generada por /discovery (fase Gherkin) desde specs/criterios/US-001.yaml
@epic_id:EPIC-001
Feature: Crear cuenta de usuario
  Como Admin
  Quiero crear una cuenta de usuario con nombre, email, contraseña, rol y estado
  Para dar acceso a nuevas personas dentro del sistema

  Background:
    Given un Admin autenticado en el módulo de administración de usuarios

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Crear una cuenta con datos válidos
    Given el email "ana@cidenet.com" no existe en el sistema
    When el Admin completa nombre "Ana Gomez", email "ana@cidenet.com", una contraseña válida con su confirmación y rol "Editor"
    And guarda el formulario
    Then la cuenta queda creada con estado "Activo" por defecto
    And aparece en la tabla de usuarios
    And el sistema muestra una confirmación de creación

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Rechazar creación con un campo obligatorio vacío
    When el Admin deja el campo nombre vacío y completa el resto
    Then la cuenta no se crea
    And el campo nombre muestra un mensaje inline indicando que es obligatorio
    And se muestra un mensaje resumen con los pendientes del formulario

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Rechazar email con formato inválido
    When el Admin ingresa el email "ana(arroba)cidenet" sin formato válido
    Then la cuenta no se crea
    And el campo email indica que el formato no es válido

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:medium @error_handling
  Scenario: Rechazar email duplicado (R4)
    Given ya existe una cuenta con email "ana@cidenet.com"
    When el Admin intenta crear otra cuenta con email "ana@cidenet.com"
    Then la cuenta no se crea
    And el campo email indica que ya está registrado

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:medium @error_handling @data_driven
  Scenario Outline: Validar la política de contraseña
    When el Admin ingresa la contraseña "<password>"
    Then el sistema "<resultado>"

    Examples:
      | password    | resultado                                             |
      | Abcdef1x    | acepta la contraseña                                  |
      | abcdef1x    | rechaza: falta una mayúscula                          |
      | ABCDEF1X    | rechaza: falta una minúscula                          |
      | Abcdefgh    | rechaza: falta un número                              |
      | Ab1x        | rechaza: no alcanza el mínimo de 8 caracteres         |

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Rechazar cuando la contraseña y su confirmación no coinciden
    When el Admin ingresa una contraseña y una confirmación distinta
    Then la cuenta no se crea
    And el campo de confirmación muestra un mensaje inline de que no coinciden

  @story_id:US-001 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Rechazar cuando no se selecciona un rol válido
    When el Admin no selecciona ningún rol
    Then la cuenta no se crea
    And el sistema exige seleccionar exactamente un rol válido (Admin, Editor o Viewer)

  @story_id:US-001 @origin:discovery_inicial @priority:2 @complexity:low @ux
  Scenario: El botón Guardar permanece deshabilitado mientras el formulario es inválido
    Given el formulario de creación tiene al menos un campo inválido
    Then el botón Guardar está deshabilitado
    When todos los campos pasan a ser válidos
    Then el botón Guardar se habilita

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:medium @edge_case
  Scenario: El email es case-insensitive para la unicidad
    Given ya existe una cuenta con email "ana@cidenet.com"
    When el Admin intenta crear una cuenta con email "ANA@cidenet.com"
    Then la cuenta no se crea
    And se considera el mismo email que el existente

  @story_id:US-001 @origin:discovery_inicial @priority:2 @complexity:low @edge_case
  Scenario: Se recortan los espacios del nombre y del email
    When el Admin ingresa el email "  ana@cidenet.com  " y el nombre "  Ana Gomez  "
    Then el sistema recorta los espacios antes de validar y guardar
    And la cuenta se crea con email "ana@cidenet.com" y nombre "Ana Gomez"

  @story_id:US-001 @origin:discovery_inicial @priority:2 @complexity:low @edge_case @data_driven
  Scenario Outline: Validar el límite de longitud del nombre
    When el Admin ingresa un nombre de longitud "<longitud>"
    Then el sistema "<resultado>"

    Examples:
      | longitud            | resultado                    |
      | 1 caracter          | rechaza: mínimo 2 caracteres |
      | 50 caracteres       | acepta el nombre             |
      | 101 caracteres      | rechaza: máximo 100          |

  @story_id:US-001-AUD @origin:analisis_completitud @area:auditoria @priority:1 @complexity:high @edge_case
  Scenario: Recrear el email de una cuenta eliminada reactiva la cuenta anterior
    Given existió una cuenta con email "ana@cidenet.com" que fue eliminada lógicamente
    When el Admin crea una cuenta con email "ana@cidenet.com"
    Then el sistema no crea un registro nuevo ni rechaza la operación
    And reactiva la cuenta anterior, actualiza sus datos y limpia la marca de eliminación
    And se preserva la historia de esa cuenta para auditoría

  @story_id:US-001-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium @security
  Scenario: Un rol sin permiso no puede crear usuarios ni siquiera llamando al backend directo
    Given un usuario con rol "Viewer" autenticado
    When envía una petición directa al endpoint de creación de usuarios, saltándose la UI
    Then el backend rechaza la petición con un error de autorización (403)
    And no se crea ninguna cuenta
