# SharedBackground.SpecFlowPlugin

[![NuGet Status](http://img.shields.io/nuget/v/sharedbackground.specflowplugin.svg?style=flat)](https://www.nuget.org/packages/sharedbackground.specflowplugin/)

A SpecFlow plugin to share background steps with multiple features and scenarios.

You can use it with a Background statement:

```gherkin
Background: file:SharedBackground.feature
```

Or as a step definition:

```gherkin
Given the background steps of 'SharedBackground.feature' have been executed
```


### Installation

Just add SharedBackground.SpecFlowPlugin NuGet package to your SpecFlow test project and you're good to go.