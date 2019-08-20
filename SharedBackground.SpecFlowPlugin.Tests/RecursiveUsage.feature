Feature: Recursive Usage
      
  Scenario: Step 1
    Given I have entered 50 into the calculator

  Scenario: Step 2
    Given the scenario 'Step 1' of 'RecursiveUsage' has been executed
    And I have also entered 70 into the calculator
  
  Scenario: Step 3
    Given the scenario 'Step 2' has been executed
    When I press add
   
  Scenario: Step 4
    Given the scenario 'Step 3' of 'RecursiveUsage' has been executed
    Then the result should be 120 on the screen
  
  