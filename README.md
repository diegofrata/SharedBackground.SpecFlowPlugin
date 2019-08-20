# SharedBackground.SpecFlowPlugin

[![NuGet Status](http://img.shields.io/nuget/v/sharedbackground.specflowplugin.svg?style=flat)](https://www.nuget.org/packages/sharedbackground.specflowplugin/)
[![NuGet Status](http://img.shields.io/nuget/vpre/sharedbackground.specflowplugin.svg?style=flat)](https://www.nuget.org/packages/sharedbackground.specflowplugin/)

A SpecFlow plugin to share background steps with multiple features and scenarios.

You can use it with a Background statement:

```gherkin
Background: the background steps of 'SharedBackground' have been executed
```

Or as a step definition:

```gherkin
Given the background steps of 'SharedBackground' have been executed
```

You can also reference specific scenarios, rather than a background definition:

```gherkin
Given the scenario 'Add two numbers' of 'SharedBackground' has been executed
```

It also support recursively importing scenario definitions:

```gherkin
@ignore
Scenario: Enter first number
  Given I have entered 50

@ignore
Scenario: Enter second number
  Given the scenario 'Enter first number' has been executed
  And I have also entered 50  

Scenario: Add two numbers
  Given the scenario 'Enter second number' has been executed
  When I add the two numbers
  Then the result is 100
```


### Installation

Just add SharedBackground.SpecFlowPlugin NuGet package to your SpecFlow test project and you're good to go.