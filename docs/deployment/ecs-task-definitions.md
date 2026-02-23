# ECS Task Definitions

## Overview

Each module runs as a separate ECS task. Task definitions specify the container configuration, resource allocation, and environment settings.

## Task Definition Structure

Task definitions are stored in the `ecs/` directory:

```
ecs/
├── orders-task-definition.json
├── sales-task-definition.json
├── customer-task-definition.json
├── inventory-task-definition.json
├── organization-task-definition.json
└── sample-task-definition.json
```

## Task Definition Template

```json
{
  "family": "orders-task",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "256",
  "memory": "512",
  "executionRoleArn": "arn:aws:iam::${AWS_ACCOUNT_ID}:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::${AWS_ACCOUNT_ID}:role/ecsTaskRole",
  "containerDefinitions": [
    {
      "name": "orders-api",
      "image": "${ECR_REGISTRY}/modular-api-orders:${IMAGE_TAG}",
      "essential": true,
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "ASPNETCORE_URLS",
          "value": "http://+:8080"
        }
      ],
      "secrets": [
        {
          "name": "ConnectionStrings__Database",
          "valueFrom": "arn:aws:ssm:${AWS_REGION}:${AWS_ACCOUNT_ID}:parameter/modular/orders/database-connection"
        },
        {
          "name": "ConnectionStrings__Cache",
          "valueFrom": "arn:aws:ssm:${AWS_REGION}:${AWS_ACCOUNT_ID}:parameter/modular/redis-connection"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/orders-api",
          "awslogs-region": "${AWS_REGION}",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ]
}
```

## Resource Allocation

Recommended Fargate configurations:

| Module | CPU | Memory | Use Case |
|--------|-----|--------|----------|
| Orders | 256 | 512 | Low traffic |
| Orders | 512 | 1024 | Medium traffic |
| Orders | 1024 | 2048 | High traffic |

Scale based on:
- Request volume
- Response time requirements
- Memory usage patterns

## Environment Variables

### Required Variables

| Variable | Description |
|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Environment name (Production, Staging) |
| `ASPNETCORE_URLS` | Listening URL (`http://+:8080`) |

### Secrets (from SSM Parameter Store)

| Secret | SSM Path |
|--------|----------|
| Database connection | `/modular/{module}/database-connection` |
| Redis connection | `/modular/redis-connection` |
| AWS credentials | Use task role instead |

## IAM Roles

### Task Execution Role

Allows ECS to pull images and read secrets:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "ssm:GetParameters",
        "ssm:GetParameter"
      ],
      "Resource": "arn:aws:ssm:*:*:parameter/modular/*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "*"
    }
  ]
}
```

### Task Role

Allows application to access AWS services:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "sqs:SendMessage",
        "sqs:ReceiveMessage",
        "sqs:DeleteMessage",
        "sqs:GetQueueAttributes"
      ],
      "Resource": "arn:aws:sqs:*:*:modular-*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject"
      ],
      "Resource": "arn:aws:s3:::modular-*/*"
    }
  ]
}
```

## Health Checks

### Container Health Check

Defined in task definition:

```json
"healthCheck": {
  "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
  "interval": 30,
  "timeout": 5,
  "retries": 3,
  "startPeriod": 60
}
```

### Load Balancer Health Check

Configure in target group:

| Setting | Value |
|---------|-------|
| Path | `/health` |
| Protocol | HTTP |
| Port | traffic-port |
| Healthy threshold | 2 |
| Unhealthy threshold | 3 |
| Timeout | 5 seconds |
| Interval | 30 seconds |

## Logging

CloudWatch log groups per module:

```
/ecs/orders-api
/ecs/sales-api
/ecs/customer-api
/ecs/inventory-api
/ecs/organization-api
/ecs/sample-api
```

Create log groups:

```bash
aws logs create-log-group --log-group-name /ecs/orders-api
aws logs put-retention-policy --log-group-name /ecs/orders-api --retention-in-days 30
```

## Service Configuration

Create ECS service for each module:

```bash
aws ecs create-service \
  --cluster modular-cluster \
  --service-name orders-service \
  --task-definition orders-task \
  --desired-count 2 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-xxx],securityGroups=[sg-xxx],assignPublicIp=DISABLED}" \
  --load-balancers "targetGroupArn=arn:aws:elasticloadbalancing:...,containerName=orders-api,containerPort=8080"
```

## Auto Scaling

Configure auto scaling based on CPU or request count:

```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/modular-cluster/orders-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 1 \
  --max-capacity 10

# Create scaling policy
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/modular-cluster/orders-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name orders-cpu-scaling \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration '{
    "TargetValue": 70.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
    },
    "ScaleOutCooldown": 60,
    "ScaleInCooldown": 120
  }'
```

## Blue/Green Deployments

For zero-downtime deployments, use CodeDeploy:

```json
{
  "deploymentController": {
    "type": "CODE_DEPLOY"
  }
}
```

## Troubleshooting

### Task Won't Start

```bash
# Check stopped task reason
aws ecs describe-tasks \
  --cluster modular-cluster \
  --tasks arn:aws:ecs:...:task/xxx \
  --query 'tasks[0].stoppedReason'

# Check container exit code
aws ecs describe-tasks \
  --cluster modular-cluster \
  --tasks arn:aws:ecs:...:task/xxx \
  --query 'tasks[0].containers[0].exitCode'
```

### Health Check Failures

```bash
# Check CloudWatch logs
aws logs tail /ecs/orders-api --follow

# Test health endpoint locally
curl http://localhost:8080/health
```

### Out of Memory

Increase memory in task definition:

```json
{
  "memory": "1024",
  "containerDefinitions": [{
    "memory": 896,
    "memoryReservation": 512
  }]
}
```

## Related Documentation

- [GitHub Actions CI/CD](./github-actions.md)
- [Module Deployment Architecture](../architecture/module-deployment.md)
