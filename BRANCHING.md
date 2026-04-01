# Branching Strategy

## Overview

Git Flow with `development` as the integration branch. Feature branches merge to `development`, which is CI/CD deployed to lower environments. Production releases merge from `development` to `main`.

## Branch Flow

```
feature/PROJ-123 ──→ development ──→ main
                       │        │
                       ▼        ▼
                   staging  production
```

## Environment Mapping

| Branch | Environments | Purpose |
|--------|--------------|---------|
| `development` | staging | Integration testing, QA validation |
| `main` | production | Live releases |

## Branch Naming

| Type | Format | Example |
|------|--------|---------|
| Feature | `feature/{JIRA-TICKET}` | `feature/PROJ-123` |
| Bug fix | `bugfix/{JIRA-TICKET}` | `bugfix/PROJ-456` |
| Hotfix | `hotfix/{JIRA-TICKET}` | `hotfix/PROJ-789` |
| Release | `release/v{version}` | `release/v1.2.0` |

Optional description after ticket: `feature/PROJ-123-order-validation`

## Commit Messages

```
feat: add customer reassignment endpoint
fix: resolve null reference in order validation
refactor: consolidate DbConnectionFactory pattern
docs: update branching strategy
chore: update package dependencies
test: add integration tests for product creation
```

## Quick Reference

```bash
# Start feature work
git checkout development && git pull
git checkout -b feature/PROJ-123

# Push and create PR to development
git push -u origin feature/PROJ-123

# After merge to development, cleanup
git checkout development && git pull
git branch -d feature/PROJ-123

# Production release (from development to main)
git checkout main && git pull
git merge development
git push
```

## Merge Strategy: Squash and Merge

**All PRs must use "Squash and Merge"** to maintain a clean, linear commit history.

### Why Squash?

| Benefit | Description |
|---------|-------------|
| **Clean History** | One commit per feature/fix in `ses` and `main` |
| **Atomic Changes** | Each commit represents a complete, deployable unit |
| **Easy Rollback** | Revert a single commit to undo an entire feature |
| **Readable Log** | `git log --oneline` shows meaningful feature history |
| **Bisect Friendly** | `git bisect` works effectively for debugging |

### Squash Commit Message Format

When squashing, GitHub will prompt for a final commit message. Use this format:

```
<type>: <description> (#<PR-number>)

<optional body with details>

JIRA: PROJ-123
```

**Example:**
```
feat: add customer reassignment endpoint (#142)

- Added ReassignCustomerCommand and handler
- Created PUT /customers/{id}/reassign endpoint
- Added validation for sales rep existence

JIRA: PROJ-123
```

### GitHub Settings (Repository Admins)

Configure these settings in **Settings → General → Pull Requests**:

- [x] Allow squash merging
- [ ] Allow merge commits *(disable)*
- [ ] Allow rebase merging *(disable)*
- [x] Default to pull request title for squash merge commits
- [x] Always suggest updating pull request branches

### PR Title Convention

Since PR titles become commit messages after squash, follow commit message format:

```
feat: add customer reassignment endpoint
fix: resolve null reference in order validation
refactor: consolidate DbConnectionFactory pattern
```

**Bad PR titles:**
- ❌ "PROJ-123"
- ❌ "Fixed stuff"
- ❌ "WIP changes"

**Good PR titles:**
- ✅ "feat: add customer reassignment endpoint"
- ✅ "fix: resolve order validation null reference"

## Branch Protection

### development
- Require PR with approval
- Require passing CI (build, tests)
- Require squash merge (disable other merge types)
- No direct commits

### main
- Require PR with approval
- Require passing CI (build, tests)
- Require squash merge (disable other merge types)
- No direct commits
- Merges from `development` only

## Anti-Patterns

| Don't | Do Instead |
|-------|------------|
| Branch without JIRA ticket | `feature/PROJ-123` |
| Long-lived branches (weeks) | Break into smaller PRs |
| Direct commits to development/main | Always use PRs |
| Giant PRs (1000+ lines) | Smaller, focused PRs |
| Merge to main without QA sign-off | Validate in staging environment first |
| Use "Create a merge commit" | Always use "Squash and merge" |
| Use "Rebase and merge" | Always use "Squash and merge" |
| Vague PR titles like "fixes" | Descriptive: `fix: resolve order validation error` |
| Multiple unrelated changes in one PR | One logical change per PR |
