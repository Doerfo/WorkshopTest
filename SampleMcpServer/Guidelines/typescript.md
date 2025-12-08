---
description: Company TypeScript coding standards
technology: typescript
---

# TypeScript Coding Standards

## Type Safety

- Always enable `strict` mode in tsconfig.json
- Avoid `any` type - use `unknown` when type is truly unknown
- Use type guards for narrowing types
- Prefer interfaces over type aliases for object shapes

## Code Organization

- One class/interface per file
- Group related functionality into modules
- Use barrel exports (index.ts) for clean imports
- Keep files under 300 lines

## Naming Conventions

- Classes: PascalCase (`UserService`, `DataProcessor`)
- Interfaces: PascalCase, prefix with `I` if needed (`ILogger`, `IRepository`)
- Functions/methods: camelCase (`getUserById`, `processData`)
- Constants: UPPER_SNAKE_CASE (`API_BASE_URL`, `MAX_RETRIES`)
- Private members: prefix with underscore (`_internalState`)

## Error Handling

- Use custom error classes extending `Error`
- Always provide meaningful error messages
- Handle promises with try/catch in async functions
- Never swallow errors silently

Example:
```typescript
export class ValidationError extends Error {
  constructor(message: string, public field: string) {
    super(message);
    this.name = 'ValidationError';
  }
}
```

## Async/Await

- Prefer async/await over raw promises
- Always handle promise rejections
- Use `Promise.all()` for parallel operations
- Add proper typing to async functions

## Documentation

- Use JSDoc comments for public APIs
- Document complex logic with inline comments
- Keep README.md up to date with API changes

## Testing

- Write tests for all public APIs
- Use Jest as the testing framework
- Aim for 80%+ code coverage
- Mock external dependencies
