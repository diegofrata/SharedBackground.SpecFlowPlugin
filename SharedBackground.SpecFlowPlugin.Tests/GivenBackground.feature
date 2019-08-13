Feature: Given Background
      
  Scenario: Add two numbers
    Given the background steps of 'SharedBackground.feature' have been executed
    When I press add
    Then the result should be 120 on the screen
    