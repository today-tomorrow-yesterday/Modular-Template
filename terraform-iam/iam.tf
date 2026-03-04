# =========================================================================== #
# Core API Lambda Role
# =========================================================================== #

locals {
  core_lambdas  = ["${local.qualified_name}-core-lambda"]
  iam_role_name = "AR-${local.qualified_name}-core-lambda-role"
}

resource "aws_iam_role" "core_lambda_role" {
  name               = local.iam_role_name
  description        = "Allows Core API Lambda to publish to EventBridge and consume from SQS"
  assume_role_policy = data.aws_iam_policy_document.core_lambda_trust_policy.json

  inline_policy {
    name   = "emb_access_policy"
    policy = data.aws_iam_policy_document.core_lambda_permissions_policy.json
  }

  permissions_boundary = "arn:aws:iam::${local.account_id}:policy/BP-GH-Deploy-AppRoleBoundary-${var.environment}"

  tags = merge(var.tags, {
    name           = local.iam_role_name,
    targetresource = join(" ", local.core_lambdas)
  })
}
