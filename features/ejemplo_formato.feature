# Ejemplo de formato Gherkin — dominio de biblioteca, NO relacionado al módulo real del taller.
# Esto es solo una referencia de formato para /discovery: tags, Background, Scenario,
# Scenario Outline. Los .feature reales del módulo de usuarios se generan durante el taller.

@epic_id:EJEMPLO-000
Feature: Prestamo de libros en una biblioteca
  Como bibliotecario
  Quiero registrar el prestamo de un libro a un socio
  Para llevar control de que ejemplares estan disponibles

  Background:
    Given la biblioteca tiene el libro "Cien Anios de Soledad" con 2 copias disponibles

  @story_id:EJ-001 @priority:1 @complexity:low @smoke
  Scenario: Prestar un libro disponible
    Given el socio "Ana" no tiene prestamos activos
    When el bibliotecario presta "Cien Anios de Soledad" a "Ana"
    Then el prestamo queda registrado
    And quedan 1 copias disponibles del libro

  @story_id:EJ-001 @priority:1 @complexity:low @error_handling
  Scenario: Intentar prestar un libro sin copias disponibles
    Given el libro "Cien Anios de Soledad" no tiene copias disponibles
    When el bibliotecario intenta prestar "Cien Anios de Soledad" a "Ana"
    Then el sistema muestra error "No hay copias disponibles"

  @story_id:EJ-002 @priority:2 @complexity:low @data_driven
  Scenario Outline: Validar limite de prestamos simultaneos
    Given el socio "<socio>" tiene <prestamos_activos> prestamos activos
    When el bibliotecario intenta prestarle un libro adicional
    Then el sistema <resultado>

    Examples:
      | socio  | prestamos_activos | resultado                                   |
      | Carlos | 2                  | permite el prestamo                         |
      | Laura  | 5                  | muestra error "Limite de prestamos alcanzado" |
