Feature: Inline Usage
      
  Scenario: Use background inline
    Given the background steps of 'Shared/SharedBackground' have been executed
    When I press add
    Then the result should be 120 on the screen


  Scenario: Use scenario inline
    Given the scenario 'Two and Two' of 'Shared/SharedScenario' has been executed
    When I press add
    Then the result should be 4 on the screen


  Scenario: Maintain whatever given/when/then keyword used
    Given the scenario 'Use scenario inline' has been executed
    And I erased the second number
    When I press add
    Then the result should be 2 on the screen
        