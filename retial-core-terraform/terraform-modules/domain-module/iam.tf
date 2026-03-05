# -----------------------------------------------------------------------------
# Service Role
# -----------------------------------------------------------------------------
resource "aws_iam_role" "service" {
  count = var.service_iam_role.create ? 1 : 0

  name        = "AR-${var.environment}-${var.business_unit}-${var.service_name}-service"
  description = "Service role for ${var.environment}-${var.business_unit}-${var.service_name}"

  assume_role_policy = data.aws_iam_policy_document.service_assume_role[0].json
  permissions_boundary = coalesce(
    var.permissions_boundary_arn,
    "arn:aws:iam::${data.aws_caller_identity.this.account_id}:policy/BP-GH-Deploy-AppRoleBoundary-${var.environment}"
  )
  force_detach_policies = true

  tags = { name = "AR-${var.environment}-${var.business_unit}-${var.service_name}-service" }
}

# Service Assume Role Policy Document
data "aws_iam_policy_document" "service_assume_role" {
  count = var.service_iam_role.create ? 1 : 0

  statement {
    sid     = "ECSServiceAssumeRole"
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["ecs.amazonaws.com"]
    }
  }
}

# -----------------------------------------------------------------------------
# Task Execution Role
# -----------------------------------------------------------------------------
resource "aws_iam_role" "task_execution" {
  count = var.task_execution_iam_role.create ? 1 : 0

  name               = "AR-${var.environment}-${var.business_unit}-${var.service_name}-task-execution"
  description        = "Task execution role for ${var.environment}-${var.business_unit}-${var.service_name}"
  assume_role_policy = data.aws_iam_policy_document.task_execution_assume_role[0].json
  permissions_boundary = coalesce(
    var.permissions_boundary_arn,
    "arn:aws:iam::${data.aws_caller_identity.this.account_id}:policy/BP-GH-Deploy-AppRoleBoundary-${var.environment}"
  )

  tags = { name = "AR-${var.environment}-${var.business_unit}-${var.service_name}-task-execution" }
}

# Task Execution Assume Role Policy Document
data "aws_iam_policy_document" "task_execution_assume_role" {
  count = var.task_execution_iam_role.create ? 1 : 0

  statement {
    sid     = "ECSTaskExecutionAssumeRole"
    actions = ["sts:AssumeRole"]
    effect  = "Allow"

    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

# -----------------------------------------------------------------------------
# Task Role
# -----------------------------------------------------------------------------
resource "aws_iam_role" "task" {
  count = var.task_iam_role.create ? 1 : 0

  name               = "AR-${var.environment}-${var.business_unit}-${var.service_name}-task"
  description        = "Task role for ${var.environment}-${var.business_unit}-${var.service_name}"
  assume_role_policy = data.aws_iam_policy_document.task_assume[0].json
  permissions_boundary = coalesce(
    var.permissions_boundary_arn,
    "arn:aws:iam::${data.aws_caller_identity.this.account_id}:policy/BP-GH-Deploy-AppRoleBoundary-${var.environment}"
  )

  tags = { name = "AR-${var.environment}-${var.business_unit}-${var.service_name}-task" }
}

# Task Assume Role Policy Document
data "aws_iam_policy_document" "task_assume" {
  count = var.task_iam_role.create ? 1 : 0

  statement {
    sid     = "ECSTasksAssumeRole"
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }

    condition {
      test     = "ArnLike"
      variable = "aws:SourceArn"
      values   = ["arn:${data.aws_partition.current.partition}:ecs:${coalesce(var.region, var.aws_region)}:${data.aws_caller_identity.this.account_id}:*"]
    }

    condition {
      test     = "StringEquals"
      variable = "aws:SourceAccount"
      values   = [data.aws_caller_identity.this.account_id]
    }
  }
}

# -----------------------------------------------------------------------------
# Infrastructure Role
# -----------------------------------------------------------------------------
resource "aws_iam_role" "infrastructure" {
  count = var.infrastructure_iam_role.create ? 1 : 0

  name        = "AR-${var.environment}-${var.business_unit}-${var.service_name}-infrastructure"
  description = "Infrastructure role for ${var.environment}-${var.business_unit}-${var.service_name}"

  assume_role_policy = data.aws_iam_policy_document.infrastructure_assume[0].json
  permissions_boundary = coalesce(
    var.permissions_boundary_arn,
    "arn:aws:iam::${data.aws_caller_identity.this.account_id}:policy/BP-GH-Deploy-AppRoleBoundary-${var.environment}"
  )
  force_detach_policies = true

  tags = { name = "AR-${var.environment}-${var.business_unit}-${var.service_name}-infrastructure" }
}

# Infrastructure Assume Role Policy Document
data "aws_iam_policy_document" "infrastructure_assume" {
  count = var.infrastructure_iam_role.create ? 1 : 0

  statement {
    sid     = "ECSServiceAssumeRole"
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["ecs.amazonaws.com"]
    }
  }
}

