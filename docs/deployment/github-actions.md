# GitHub Actions CI/CD

## Overview

Rtl.Core uses GitHub Actions for continuous integration and deployment. The pipeline builds all module images in parallel and supports independent deployment of each module.

## Workflow Files

| File | Purpose |
|------|---------|
| `.github/workflows/build.yml` | Build and push all module images |
| `.github/workflows/deploy-orders.yml` | Deploy Orders module |
| `.github/workflows/deploy-sales.yml` | Deploy Sales module |
| `.github/workflows/deploy-customer.yml` | Deploy Customer module |
| `.github/workflows/deploy-organization.yml` | Deploy Organization module |
| `.github/workflows/deploy-sample.yml` | Deploy Sample module |

## Build Workflow

The build workflow uses matrix strategy to build all modules in parallel:

```yaml
name: Build Module Images

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

env:
  ECR_REGISTRY: ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.${{ secrets.AWS_REGION }}.amazonaws.com

jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        module: [Orders, Sales, Customer, Organization, Sample]

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ secrets.AWS_REGION }}

      - name: Login to Amazon ECR
        uses: aws-actions/amazon-ecr-login@v2

      - name: Build and push ${{ matrix.module }} image
        run: |
          docker build \
            -f src/Api/Host.${{ matrix.module }}/Dockerfile \
            -t $ECR_REGISTRY/modular-api-${{ matrix.module | lower }}:${{ github.sha }} \
            -t $ECR_REGISTRY/modular-api-${{ matrix.module | lower }}:latest \
            .

          docker push $ECR_REGISTRY/modular-api-${{ matrix.module | lower }}:${{ github.sha }}
          docker push $ECR_REGISTRY/modular-api-${{ matrix.module | lower }}:latest
```

## Deploy Workflow (Per Module)

Each module has its own deploy workflow for independent deployment:

```yaml
name: Deploy Orders

on:
  workflow_dispatch:
    inputs:
      image_tag:
        description: 'Image tag to deploy (default: latest)'
        required: false
        default: 'latest'
  workflow_run:
    workflows: ["Build Module Images"]
    types: [completed]
    branches: [main]

env:
  ECS_CLUSTER: modular-cluster
  ECS_SERVICE: orders-service
  ECR_REGISTRY: ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.${{ secrets.AWS_REGION }}.amazonaws.com

jobs:
  deploy:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' || github.event_name == 'workflow_dispatch' }}

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ secrets.AWS_REGION }}

      - name: Determine image tag
        id: image
        run: |
          if [ "${{ github.event_name }}" == "workflow_dispatch" ]; then
            echo "tag=${{ github.event.inputs.image_tag }}" >> $GITHUB_OUTPUT
          else
            echo "tag=${{ github.event.workflow_run.head_sha }}" >> $GITHUB_OUTPUT
          fi

      - name: Update ECS task definition
        run: |
          # Get current task definition
          aws ecs describe-task-definition \
            --task-definition orders-task \
            --query 'taskDefinition' > task-def.json

          # Update image tag
          jq --arg IMAGE "$ECR_REGISTRY/modular-api-orders:${{ steps.image.outputs.tag }}" \
            '.containerDefinitions[0].image = $IMAGE' task-def.json > new-task-def.json

          # Register new task definition
          aws ecs register-task-definition \
            --cli-input-json file://new-task-def.json

      - name: Deploy to ECS
        run: |
          aws ecs update-service \
            --cluster $ECS_CLUSTER \
            --service $ECS_SERVICE \
            --task-definition orders-task \
            --force-new-deployment

      - name: Wait for deployment
        run: |
          aws ecs wait services-stable \
            --cluster $ECS_CLUSTER \
            --services $ECS_SERVICE
```

## Required Secrets

Configure these secrets in GitHub repository settings:

| Secret | Description |
|--------|-------------|
| `AWS_ACCOUNT_ID` | AWS account ID (12-digit number) |
| `AWS_ACCESS_KEY_ID` | IAM user access key |
| `AWS_SECRET_ACCESS_KEY` | IAM user secret key |
| `AWS_REGION` | AWS region (e.g., `us-east-1`) |

## ECR Repository Setup

Create ECR repositories for each module:

```bash
# Create repositories
aws ecr create-repository --repository-name modular-api-orders
aws ecr create-repository --repository-name modular-api-sales
aws ecr create-repository --repository-name modular-api-customer
aws ecr create-repository --repository-name modular-api-organization
aws ecr create-repository --repository-name modular-api-sample

# Set lifecycle policy (optional - clean up old images)
aws ecr put-lifecycle-policy \
  --repository-name modular-api-orders \
  --lifecycle-policy-text '{
    "rules": [{
      "rulePriority": 1,
      "description": "Keep last 10 images",
      "selection": {
        "tagStatus": "any",
        "countType": "imageCountMoreThan",
        "countNumber": 10
      },
      "action": { "type": "expire" }
    }]
  }'
```

## Manual Deployment

Deploy a specific version manually:

1. Go to **Actions** tab in GitHub
2. Select **Deploy Orders** (or other module)
3. Click **Run workflow**
4. Enter image tag (e.g., `abc123` or `latest`)
5. Click **Run workflow**

## Rollback

To rollback to a previous version:

1. Find the commit SHA of the working version
2. Run manual deployment with that SHA as image tag

Or use AWS CLI:

```bash
# List task definition revisions
aws ecs list-task-definitions --family-prefix orders-task

# Update service to use previous revision
aws ecs update-service \
  --cluster modular-cluster \
  --service orders-service \
  --task-definition orders-task:5  # Previous revision number
```

## Branch Protection

Recommended branch protection rules for `main`:

- Require pull request reviews
- Require status checks (build workflow)
- Require branches to be up to date

## Caching

The workflow uses Docker layer caching for faster builds:

```yaml
- name: Set up Docker Buildx
  uses: docker/setup-buildx-action@v3

- name: Build and push
  uses: docker/build-push-action@v5
  with:
    context: .
    file: src/Api/Host.${{ matrix.module }}/Dockerfile
    push: true
    tags: ${{ env.ECR_REGISTRY }}/modular-api-${{ matrix.module }}:${{ github.sha }}
    cache-from: type=gha
    cache-to: type=gha,mode=max
```

## Notifications

Add Slack notifications for deployment status:

```yaml
- name: Notify Slack
  if: always()
  uses: slackapi/slack-github-action@v1
  with:
    channel-id: 'deployments'
    slack-message: |
      Deployment of ${{ matrix.module }} ${{ job.status }}
      Commit: ${{ github.sha }}
      Actor: ${{ github.actor }}
  env:
    SLACK_BOT_TOKEN: ${{ secrets.SLACK_BOT_TOKEN }}
```

## Monitoring

After deployment, monitor:

1. **ECS Service Events**: Check for deployment issues
2. **CloudWatch Logs**: Application logs
3. **Health Check Endpoint**: `/health` on each module
4. **ALB Target Group Health**: Load balancer health checks

## Related Documentation

- [ECS Task Definitions](./ecs-task-definitions.md)
- [Module Deployment Architecture](../architecture/module-deployment.md)
