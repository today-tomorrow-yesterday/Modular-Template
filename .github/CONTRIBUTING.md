# Contributing Guide

This document outlines the branch strategy, protection rules, and deployment process for the Rtl.Core project.

---

## Branch Strategy

```
feature/* ──► development ──► main
              (dev/itg/qua)    (prod)
```

| Branch | Purpose | Deploys To |
|--------|---------|------------|
| `feature/*` | New features, bug fixes | dev, itg (manual) |
| `development` | Integration branch | dev (auto), itg/qua (manual) |
| `main` | Production-ready code | prod (auto) |

---

## Branch Protection Rules

### `development` Branch

| Rule | Setting |
|------|---------|
| Require pull request | Yes |
| Required approvals | **2** |
| Dismiss stale reviews on new commits | Yes |
| Require status checks to pass | Yes |
| Required status checks | `build-test` |
| Require branch to be up to date | Yes |
| Include administrators | Yes |

### `main` Branch

| Rule | Setting |
|------|---------|
| Require pull request | Yes |
| Required approvals | **2** |
| Dismiss stale reviews on new commits | Yes |
| Require status checks to pass | Yes |
| Required status checks | `build-test` |
| Require branch to be up to date | Yes |
| Restrict pushes | Only from `development` branch merges |
| Include administrators | Yes |

---

## Setting Up Branch Protection (GitHub)

1. Go to **Settings** > **Branches** > **Add branch protection rule**

2. For `development`:
   - Branch name pattern: `development`
   - Check: "Require a pull request before merging"
   - Set "Required approvals" to `2`
   - Check: "Dismiss stale pull request approvals when new commits are pushed"
   - Check: "Require status checks to pass before merging"
   - Search and select: `build-test`
   - Check: "Require branches to be up to date before merging"
   - Check: "Do not allow bypassing the above settings"

3. Repeat for `main` with same settings

---

## Deployment Environments

### Environment Summary

| Environment | Auto-Deploy Trigger | Manual Deploy | Branch Restriction |
|-------------|---------------------|---------------|-------------------|
| `dev` | PR merge → `development` | Any branch | None |
| `itg` | - | Any branch | None |
| `qua` | - | Manual only | `development` only |
| `prod` | PR merge → `main` | Not available | Auto-deploy only |

### Environment Protection (GitHub)

Configure in **Settings** > **Environments**:

| Environment | Required Reviewers | Wait Timer | Deployment Branch |
|-------------|-------------------|------------|-------------------|
| `dev` | 0 | - | All branches |
| `itg` | 0 | - | All branches |
| `qua` | 1 | - | `development` only |
| `prod` | 2 | 10 minutes | `main` only |

### Required Secrets Per Environment

Each environment needs these secrets configured:

- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_REGION`
- `DB_URL` (for migrations, when enabled)
- `DB_USER` (for migrations, when enabled)
- `DB_PASSWORD` (for migrations, when enabled)

---

## Deployment Workflow

### Automatic Deployments

Deployments are gated on successful image builds to prevent race conditions.

```
PR merged to development
         │
         ▼
   Build & Test (build.yml)
         │
         ▼
   Build Docker Images
         │
         ▼ (workflow_run trigger)
   Deploy to DEV (all modules)


PR merged to main (from development)
         │
         ▼
   Build & Test (build.yml)
         │
         ▼
   Build Docker Images
         │
         ▼ (workflow_run trigger)
   Deploy to PROD (all modules)
```

### Manual Deployments

1. Go to **Actions** > **Deploy Module**
2. Click **Run workflow**
3. Select:
   - **Module**: Single module or "All"
   - **Environment**: dev, itg, or qua
   - **Image tag**: Specific tag or "latest"
4. Click **Run workflow**

Note: QUA deployments require the workflow to be run from the `development` branch.

---

## Development Workflow

### Starting New Work

```bash
# Create feature branch from development
git checkout development
git pull origin development
git checkout -b feature/my-feature
```

### Testing Your Changes

```bash
# Push feature branch
git push -u origin feature/my-feature

# Manual deploy to dev for testing (via GitHub Actions UI)
# Or deploy to itg for integration testing
```

### Submitting for Review

1. Create PR: `feature/my-feature` → `development`
2. Wait for `build-test` status check to pass
3. Request review from 2 team members
4. Address feedback, push updates
5. Once approved, merge PR
6. Auto-deploys to `dev` environment

### Releasing to Production

1. Create PR: `development` → `main`
2. Request review from 2 team members
3. Once approved, merge PR
4. Auto-deploys to `prod` environment

---

## Modules

| Module | ECR Image | ECS Service |
|--------|-----------|-------------|
| SampleOrders | `modular-api-sampleorders` | `sampleorders-service` |
| SampleSales | `modular-api-samplesales` | `samplesales-service` |

---

## Troubleshooting

### PR blocked - status checks failing

- Check the `build-test` job in Actions tab
- Fix any build errors or test failures
- Push fixes to your branch

### PR blocked - needs approvals

- Request review from team members
- Minimum 2 approvals required for `development` and `main`

### Cannot deploy to QUA

- QUA deployments must be triggered from the `development` branch
- Switch to `development` branch in the workflow dropdown

### Cannot manually deploy to PROD

- PROD deployments are automatic only (on merge to `main`)
- This is by design for safety
- Merge your changes to `main` to trigger production deployment
