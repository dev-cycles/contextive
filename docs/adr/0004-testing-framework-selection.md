# Testing Framework Selection 

* Status: accepted
* Deciders: Chris Simon
* Date: 2021-11-01

## Decision Summary

Use [Expecto](https://github.com/haf/expecto) as a unit testing framework and [Unquote](https://github.com/SwensenSoftware/unquote) for assertions.

## Context and Problem Statement

There are a number of frameworks and libraries to support various types of testing that are compatible with F# (given the decision to use F# in ADR-0002). The project needs to select a framework/library to support unit and integration testing.

## Decision Drivers

* Documentation
* Support
* F# 'native' feel

## Considered Options

* Unit testing frameworks:
  * [NUnit](https://nunit.org/)
  * [Xunit](https://xunit.net/)
  * [Expecto](https://github.com/haf/expecto)
* Assertion Libraries
  * [FsUnit](https://fsprojects.github.io/FsUnit/)
  * [Unquote](https://github.com/SwensenSoftware/unquote)

## Decision Outcome

Chosen option: "Expecto" for the unit testing framework, because of its support for F# idiomatic design.  "Unquote" for assertion library for the same reason.

Rationale based on analysis here: https://devonburriss.me/review-fsharp-test-libs/ and here: https://newbedev.com/what-unit-testing-frameworks-are-available-for-f