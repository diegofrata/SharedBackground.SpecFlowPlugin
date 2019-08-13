Feature: Background File

  Background: file:SharedBackground.feature
  
  Scenario: Add two numbers
    When I press add
    Then the result should be 120 on the screen
    