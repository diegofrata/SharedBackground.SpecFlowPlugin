Feature: Given Background
      
  Scenario: Use background inline
    Given the background steps of 'Shared/SharedBackground' have been executed
    When I press add
    Then the result should be 120 on the screen


  Scenario: Use scenario inline
    Given the scenario 'Two and Two' of 'Shared/SharedScenario' has been executed
    When I press add
    Then the result should be 4 on the screen
    