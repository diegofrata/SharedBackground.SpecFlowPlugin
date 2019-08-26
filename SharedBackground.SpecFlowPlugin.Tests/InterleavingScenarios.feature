Feature: Interleaving Scenarios
      
  Scenario: Step 1
    Given I have entered 50 into the calculator

  Scenario: Step 2
    Given the scenario 'Step 1' of 'RecursiveUsage' has been executed
    And I have also entered 70 into the calculator
  
  Scenario: Step 3
    Given the scenario 'Step 2' has been executed
    When I press add
   
  Scenario: Interleaving steps
    Given the scenario 'Step 1' has been executed
    And the scenario 'Step 2' is redefined
    And I have also entered 70 into the calculator    
    When the scenario 'Step 3' is executed
    Then the result should be 120 on the screen
  
  