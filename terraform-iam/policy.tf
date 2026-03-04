# =========================================================================== #
# Policies for Core API Lambda
# =========================================================================== #

data "aws_iam_policy_document" "core_lambda_trust_policy" {
  statement {
    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }

    actions = ["sts:AssumeRole"]
  }
}

data "aws_iam_policy_document" "core_lambda_permissions_policy" {

  statement {
    sid    = "EnterpriseMessageBusAccess"
    effect = "Allow"
    actions = [
      "events:PutEvents"
    ]
    resources = [
      "arn:aws:events:${var.aws_region}:${local.account_id}:event-bus/${var.enterprise_message_bus_name}"
    ]
  }

  statement {
    sid    = "AllowSqsConsume"
    effect = "Allow"
    actions = [
      "sqs:ReceiveMessage",
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:ChangeMessageVisibility"
    ]
    resources = [
      "arn:aws:sqs:${var.aws_region}:${local.account_id}:${local.qualified_name}-*"
    ]
  }
}
