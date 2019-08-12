Feature: Calculator

  Background: ./Background.background
    
  Scenario: Add two numbers
    When I press add
    Then the result should be 120 on the screen
    