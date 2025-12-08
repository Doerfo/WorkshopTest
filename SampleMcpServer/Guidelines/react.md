---
description: Company React development standards
technology: react
---

# React Development Standards

## Component Structure

- Use functional components with hooks
- Keep components small and focused (single responsibility)
- Extract reusable logic into custom hooks
- Limit component file size to 200 lines

## Hooks Guidelines

- Follow Rules of Hooks strictly
- Custom hooks must start with `use` prefix
- Extract complex state logic into custom hooks
- Use `useCallback` and `useMemo` for performance optimization

## State Management

- Use `useState` for local component state
- Use Context API for shared state across few components
- Consider Redux or Zustand for complex global state
- Keep state as close to where it's used as possible

## Component Naming

- Components: PascalCase (`UserProfile`, `DataTable`)
- Props interfaces: `{ComponentName}Props`
- Files: match component name (`UserProfile.tsx`)

Example:
```typescript
interface UserProfileProps {
  userId: string;
  onUpdate?: (user: User) => void;
}

export const UserProfile: React.FC<UserProfileProps> = ({ userId, onUpdate }) => {
  // Component implementation
};
```

## Props and TypeScript

- Always type component props
- Use `React.FC` for function components
- Prefer interfaces over types for props
- Mark optional props with `?`

## Event Handlers

- Name handlers with `handle` prefix (`handleClick`, `handleSubmit`)
- Use arrow functions for event handlers
- Prevent default behavior explicitly when needed

## Performance

- Use `React.memo` for expensive components
- Implement proper key props in lists
- Avoid inline function definitions in render
- Use virtualization for long lists

## Testing

- Write tests using React Testing Library
- Test user interactions, not implementation
- Mock API calls and external dependencies
- Aim for 80%+ component coverage

## Accessibility

- Use semantic HTML elements
- Add ARIA labels where needed
- Ensure keyboard navigation works
- Test with screen readers
