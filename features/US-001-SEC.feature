# Especificación ejecutable — gap de Completitud (área Seguridad) — desde specs/historias/US-001-SEC.md
@epic_id:EPIC-001 @area:seguridad
Feature: Manejo seguro de credenciales (transversal)
  Como responsable de seguridad del sistema
  Quiero que las credenciales se manejen de forma segura de extremo a extremo
  Para que ninguna fuga o endpoint indebido exponga contraseñas

  @story_id:US-001-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:low @security
  Scenario Outline: La contraseña nunca aparece en ninguna respuesta del backend
    When se consulta "<endpoint>"
    Then la respuesta no contiene el campo contraseña en ninguna forma (ni texto plano, ni enmascarada, ni hash)

    Examples:
      | endpoint                  |
      | listado de usuarios       |
      | detalle de un usuario     |
      | perfil propio             |
      | respuesta de autenticación|

  @story_id:US-001-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium @security
  Scenario: La contraseña se almacena con hash de un solo sentido y salt
    When se crea o actualiza la contraseña de un usuario
    Then se almacena con un algoritmo moderno de hashing (Argon2id o bcrypt) con salt único por usuario
    And nunca se almacena en texto plano ni con cifrado reversible

  @story_id:US-001-SEC @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium @security
  Scenario Outline: La autorización se valida en el backend en cada petición
    Given un usuario con rol "<rol>" autenticado
    When envía una petición directa a "<accion>" saltándose la UI
    Then el backend verifica el permiso y "<resultado>"

    Examples:
      | rol    | accion                        | resultado                  |
      | Viewer | crear usuario                 | rechaza con 403            |
      | Editor | eliminar usuario              | rechaza con 403            |
      | Editor | modificar matriz de permisos  | rechaza con 403            |
      | Admin  | crear usuario                 | permite la operación       |
