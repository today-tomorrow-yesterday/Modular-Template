# The role polices are configured in terraform.tfvars. If you need to add additional 
# statements that need dynamic values derived from varibales, you can add the below in 
# the appropriate policy document. 

# Documentation can be found at https://docs.aws.amazon.com/AmazonECS/latest/developerguide/security-ecs-iam-role-overview.html

# -----------------------------------------------------------------------------
# Role Policy Documents
# -----------------------------------------------------------------------------

# Service Policy Document
data "aws_iam_policy_document" "service" {
  count = var.service_iam_role.create ? 1 : 0

  dynamic "statement" {
    for_each = coalesce(var.service_iam_role.policy_statements, [])

    content {
      sid           = try(statement.value.sid, null)
      effect        = try(statement.value.effect, "Allow")
      actions       = try(statement.value.actions, null)
      not_actions   = try(statement.value.not_actions, null)
      resources     = try(statement.value.resources, null)
      not_resources = try(statement.value.not_resources, null)

      dynamic "principals" {
        for_each = coalesce(try(statement.value.principals, null), [])

        content {
          type        = principals.value.type
          identifiers = principals.value.identifiers
        }
      }

      dynamic "not_principals" {
        for_each = coalesce(try(statement.value.not_principals, null), [])

        content {
          type        = not_principals.value.type
          identifiers = not_principals.value.identifiers
        }
      }

      dynamic "condition" {
        for_each = coalesce(try(statement.value.condition, null), [])

        content {
          test     = condition.value.test
          variable = condition.value.variable
          values   = condition.value.values
        }
      }
    }
  }

  # Add Aditional Statements Here
}

locals {
  task_execution_iam_role = merge(
    var.task_execution_iam_role,
    {
      ssm_arns    = concat(var.task_execution_parameter_store_arns, local.task_execution_parameter_store_arns)
      secret_arns = concat(var.task_execution_secrets_manager_arns, local.task_execution_secrets_manager_arns)
    }
  )
}

# Task Execution Policy Document
data "aws_iam_policy_document" "task_exececution" {
  count = local.task_execution_iam_role.create ? 1 : 0

  # Access to Parameter Store
  dynamic "statement" {
    for_each = length(local.task_execution_iam_role.ssm_arns) > 0 ? [true] : []

    content {
      sid       = "GetSSMParams"
      actions   = ["ssm:GetParameters"]
      resources = local.task_execution_iam_role.ssm_arns
    }
  }

  # Access to Secrets
  dynamic "statement" {
    for_each = length(local.task_execution_iam_role.secret_arns) > 0 ? [true] : []

    content {
      sid       = "GetSecrets"
      actions   = ["secretsmanager:GetSecretValue"]
      resources = local.task_execution_iam_role.secret_arns
    }
  }

  # Create and put log streams
  statement {
    sid = "ECSLogs"
    actions = [
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = ["*"]
  }

  # Access to ECR
  statement {
    sid = "ECSECR"
    actions = [
      "ecr:GetAuthorizationToken",
      "ecr:BatchCheckLayerAvailability",
      "ecr:GetDownloadUrlForLayer",
      "ecr:BatchGetImage",
    ]
    resources = ["*"]
  }

  # Add Aditional Statements Here If You Need To Access Dynamic Values
}

# Task Policy Document
data "aws_iam_policy_document" "task" {
  count = var.task_iam_role.create ? 1 : 0

  statement {
    sid = "ECSExecution"
    actions = [
      "ssmmessages:CreateControlChannel",
      "ssmmessages:CreateDataChannel",
      "ssmmessages:OpenControlChannel",
      "ssmmessages:OpenDataChannel",
    ]
    resources = ["*"]
  }

  statement {
    sid    = "ECSBedRock"
    effect = "Allow"
    actions = [
      "bedrock:InvokeModel",
      "bedrock:InvokeModelWithResponseStream"
    ]
    resources = ["*"]
  }

  dynamic "statement" {
    for_each = coalesce(var.task_iam_role.policy_statements, [])

    content {
      sid           = try(statement.value.sid, null)
      effect        = try(statement.value.effect, "Allow")
      actions       = try(statement.value.actions, null)
      not_actions   = try(statement.value.not_actions, null)
      resources     = try(statement.value.resources, null)
      not_resources = try(statement.value.not_resources, null)

      dynamic "principals" {
        for_each = coalesce(try(statement.value.principals, null), [])

        content {
          type        = principals.value.type
          identifiers = principals.value.identifiers
        }
      }

      dynamic "not_principals" {
        for_each = coalesce(try(statement.value.not_principals, null), [])

        content {
          type        = not_principals.value.type
          identifiers = not_principals.value.identifiers
        }
      }

      dynamic "condition" {
        for_each = coalesce(try(statement.value.condition, null), [])

        content {
          test     = condition.value.test
          variable = condition.value.variable
          values   = condition.value.values
        }
      }
    }
  }
}

# Infrastructure Policy Document
data "aws_iam_policy_document" "infrastructure" {
  count = var.infrastructure_iam_role.create ? 1 : 0

  # ECS Volum permissions
  statement {
    sid    = "ECSVolume"
    effect = "Allow"
    actions = [
      "ec2:CreateVolume",
      "ec2:DeleteVolume",
      "ec2:DescribeVolumes",
      "ec2:AttachVolume",
      "ec2:DetachVolume",
      "ec2:CreateTags",
      "ec2:DeleteTags",
      "kms:Encrypt",
      "kms:Decrypt",
      "kms:DescribeKey"
    ]
    resources = ["*"]
  }

  # Add Aditional Statements Here
}

# -----------------------------------------------------------------------------
# Policy Attachments
# -----------------------------------------------------------------------------

# Service Policy
resource "aws_iam_policy" "service" {
  count = var.service_iam_role.create ? 1 : 0

  name        = "AP-${var.environment}-${var.service_name}-service"
  description = "Service role IAM policy for ${var.environment}-${var.service_name}"
  policy      = data.aws_iam_policy_document.service[0].json

  tags = { name = "AP-${var.environment}-${var.service_name}-service" }
}

# Service Policy Attachment
resource "aws_iam_role_policy_attachment" "service" {
  count = var.service_iam_role.create ? 1 : 0

  role       = aws_iam_role.service[0].name
  policy_arn = aws_iam_policy.service[0].arn
}

# Task Execution Policy
resource "aws_iam_policy" "task_execution" {
  count = var.task_execution_iam_role.create ? 1 : 0

  name        = "AP-${var.environment}-${var.service_name}-task-execution"
  description = "Task execution role IAM policy for ${var.environment}-${var.service_name}"
  policy      = data.aws_iam_policy_document.task_exececution[0].json

  tags = { name = "AP-${var.environment}-${var.service_name}-task-execution" }
}

# Task Execution Policy Attachment
resource "aws_iam_role_policy_attachment" "task_execution" {
  count = var.task_execution_iam_role.create ? 1 : 0

  role       = aws_iam_role.task_execution[0].name
  policy_arn = aws_iam_policy.task_execution[0].arn
}

# Task Policy
resource "aws_iam_policy" "task" {
  count = var.task_iam_role.create ? 1 : 0

  name        = "AP-${var.environment}-${var.service_name}-task"
  description = "Task role IAM policy for ${var.environment}-${var.service_name}"
  policy      = data.aws_iam_policy_document.task[0].json

  tags = { name = "AP-${var.environment}-${var.service_name}-task" }
}

# Task Policy Attachment
resource "aws_iam_role_policy_attachment" "task" {
  count = var.task_iam_role.create ? 1 : 0

  role       = aws_iam_role.task[0].name
  policy_arn = aws_iam_policy.task[0].arn
}

# Infrastructure Policy
resource "aws_iam_policy" "infrastructure" {
  count = var.infrastructure_iam_role.create ? 1 : 0

  name        = "AP-${var.environment}-${var.service_name}-infrastructure"
  description = "Infrastructure role IAM policy for ${var.environment}-${var.service_name}"
  policy      = data.aws_iam_policy_document.infrastructure[0].json

  tags = { name = "AP-${var.environment}-${var.service_name}-infrastructure" }
}

# Infrastructure Policy Attachment
resource "aws_iam_role_policy_attachment" "infrastructure" {
  count = var.infrastructure_iam_role.create ? 1 : 0

  role       = aws_iam_role.infrastructure[0].name
  policy_arn = aws_iam_policy.infrastructure[0].arn
}

# AWS Managed Infrastructure Policy For Volumes Attachment
resource "aws_iam_role_policy_attachment" "infrastructure_iam_role_ebs_policy" {
  count = var.infrastructure_iam_role.create && var.infrastructure_iam_role.attach_aws_managed_policy ? 1 : 0

  role       = aws_iam_role.infrastructure[0].name
  policy_arn = "arn:${data.aws_partition.current.partition}:iam::aws:policy/service-role/AmazonECSInfrastructureRolePolicyForVolumes"
}
