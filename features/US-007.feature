# Especificación ejecutable — generada por /discovery desde specs/criterios/US-007.yaml
@epic_id:EPIC-001
Feature: Autenticación y bloqueo de usuarios inactivos
  Como usuario del sistema
  Quiero autenticarme con mis credenciales
  Para acceder a la plataforma; y como sistema, impedir el acceso a usuarios inactivos

  Background:
    Given el módulo de autenticación del sistema

  @story_id:US-007 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Autenticar a un usuario activo con credenciales correctas
    Given un usuario con estado "Activo" y credenciales válidas
    When ingresa su email y contraseña correctos
    Then el sistema lo autentica
    And le permite acceder según los permisos de su rol

  @story_id:US-007 @origin:discovery_inicial @priority:1 @complexity:medium @error_handling
  Scenario: Bloquear a un usuario inactivo aunque sus credenciales sean correctas (R6)
    Given un usuario con estado "Inactivo" y credenciales válidas
    When intenta iniciar sesión
    Then el sistema le niega el acceso
    And muestra un mensaje indicando que la cuenta está inactiva

  @story_id:US-007 @origin:discovery_inicial @priority:1 @complexity:low @security @error_handling
  Scenario: Credenciales inválidas devuelven un mensaje genérico (anti-enumeración)
    When se intenta iniciar sesión con un email inexistente o una contraseña incorrecta
    Then el sistema rechaza el acceso
    And el mensaje no revela si el email existe o si la contraseña era el problema

  @story_id:US-007 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Email y contraseña son obligatorios
    When se intenta iniciar sesión sin email o sin contraseña
    Then el sistema no procesa el intento y exige ambos campos

  @story_id:US-007 @origin:discovery_inicial @priority:2 @complexity:low @edge_case
  Scenario: El email se trata case-insensitive al autenticar
    Given un usuario activo con email "ana@cidenet.com"
    When ingresa "ANA@cidenet.com" con la contraseña correcta
    Then el sistema identifica la misma cuenta y lo autentica

  @story_id:US-007-USR @origin:analisis_completitud @area:usuarios @priority:1 @complexity:high @security
  Scenario: Desactivar a un usuario corta su sesión activa de inmediato
    Given un usuario con sesión activa
    When un Admin lo desactiva
    Then la siguiente acción que el usuario intente es rechazada
    And se le fuerza el cierre de sesión sin esperar al próximo login

  @story_id:US-007-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium @security
  Scenario: La contraseña se valida contra su hash, nunca en texto plano
    Given la contraseña se almacena como hash con salt (Argon2id/bcrypt)
    When el usuario se autentica
    Then el sistema recomputa el hash y lo compara
    And nunca recupera ni compara el texto plano almacenado
