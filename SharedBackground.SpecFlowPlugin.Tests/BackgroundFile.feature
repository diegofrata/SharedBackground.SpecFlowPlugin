Feature: Background File

  Background: the background steps of 'Shared/SharedBackground' have been executed
  
  Scenario: Use background grammar
    When I press add
    Then the result should be 120 on the screen
    