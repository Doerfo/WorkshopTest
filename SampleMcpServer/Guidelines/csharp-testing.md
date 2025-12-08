---
description: Company-specific C# testing guidelines
technology: csharp
aspect: testing
---

# C# Testing Standards

## Unit Testing Framework

- Use **xUnit** as the primary testing framework
- Test coverage minimum: **80%** for all public APIs
- All tests must be deterministic and repeatable

## Naming Conventions

- Test class: `{ClassUnderTest}Tests`
- Test method: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`
- Use descriptive names that explain the test purpose

## Test Structure

Follow the **Arrange-Act-Assert** pattern:

```csharp
[Fact]
public void Add_TwoPositiveNumbers_ReturnsSum()
{
    // Arrange
    var calculator = new Calculator();
    var a = 5;
    var b = 3;
    
    // Act
    var result = calculator.Add(a, b);
    
    // Assert
    Assert.Equal(8, result);
}
```

## Mocking

- Use **Moq** for creating test doubles
- Mock external dependencies (databases, APIs, file systems)
- Avoid mocking value objects or simple data structures

## Test Coverage

- All public methods must have at least one test
- Critical business logic requires multiple test cases
- Edge cases and error conditions must be tested
- Use code coverage tools to identify gaps

## Integration Tests

- Mark integration tests with `[Trait("Category", "Integration")]`
- Integration tests should use test databases or containers
- Clean up resources after integration tests complete

## Best Practices

- Keep tests fast (<100ms per unit test)
- One assertion per test when possible
- Avoid test interdependencies
- Use test fixtures for setup/teardown
- Never use `Thread.Sleep()` in tests
