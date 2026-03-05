# DEPRECATED: This module creates a Lambda IAM role with Lambda trust policy.
# For ECS-based domain modules, use the task_iam_role.policy_statements in each
# domain module's tfvars instead. The domain-module's Task Role has the correct
# ECS trust policy and now supports dynamic policy_statements for EventBridge/SQS.
# Remove this module after migration is verified across all environments.

locals {
  qualified_name = "${var.environment}-${var.business_unit}-${var.project_prefix}"
}
