locals {
  account_id = data.aws_caller_identity.current.account_id
}

data "aws_caller_identity" "current" {}

# =========================================================================== #
# Policies for Product Domain API Lambda
# =========================================================================== #

data "aws_iam_policy_document" "product_domain_api_lambda_trust_policy" {
  statement {
    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }

    actions = ["sts:AssumeRole"]
  }
}

data "aws_iam_policy_document" "product_domain_api_lambda_permissions_policy" {

  statement {
    sid    = "AWSLambdaVPCAccessExecutionRole"
    effect = "Allow"
    actions = [
      "ec2:CreateNetworkInterface",
      "ec2:DescribeNetworkInterfaces",
      "ec2:DeleteNetworkInterface",
      "ec2:AssignPrivateIpAddresses",
      "ec2:UnassignPrivateIpAddresses",
      "ec2:DescribeInstances",
      "ec2:AttachNetworkInterface"
    ]
    resources = ["*"]
  }

  statement {
    sid    = "AllowLogging"
    effect = "Allow"
    actions = [
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = [
      "arn:aws:logs:*:${local.account_id}:log-group:/aws/lambda/${local.api_qualified_name}-*:*",
      "arn:aws:logs:*:${local.account_id}:log-group:/aws/lambda/audit/${local.api_qualified_name}-*:*"
    ]
  }

  statement {
    sid    = "GetSecrets"
    effect = "Allow"
    actions = [
      "secretsmanager:GetSecretValue",
      "secretsmanager:DescribeSecret"
    ]
    resources = [
      "arn:aws:secretsmanager:*:${local.account_id}:secret:shared-rtl-datadog-api-key-*",
      "arn:aws:secretsmanager:*:${local.account_id}:secret:${var.environment}-rtl-rds-sql-svc_product_readwrite-*",
      "arn:aws:secretsmanager:*:${local.account_id}:secret:${var.environment}-rtl-hbg-jwt-shared-key-*",
      "arn:aws:secretsmanager:*:${local.account_id}:secret:${var.environment}-${var.business_unit}-iseries-jwt-signing-key-*"
    ]
  }

  statement {
    sid    = "AllowParameterStore"
    effect = "Allow"
    actions = [
      "ssm:GetParameter",
      "ssm:GetParameters"
    ]
    resources = [
      "arn:aws:ssm:*:${local.account_id}:parameter/shared/wafv2/token/regional/lambda",
      "arn:aws:ssm:*:${local.account_id}:parameter/shared/wafv2/token/global/lambda"
    ]
  }
}



# =========================================================================== #
# Policies for Product Domain Event Handler Lambda
# =========================================================================== #
data "aws_iam_policy_document" "product_domain_event_handler_lambda_trust_policy" {
  statement {
    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }

    actions = ["sts:AssumeRole"]
  }
}

data "aws_iam_policy_document" "product_domain_event_handler_lambda_permissions_policy" {

  statement {
    sid    = "AWSLambdaVPCAccessExecutionRole"
    effect = "Allow"
    actions = [
      "ec2:CreateNetworkInterface",
      "ec2:DescribeNetworkInterfaces",
      "ec2:DeleteNetworkInterface",
      "ec2:AssignPrivateIpAddresses",
      "ec2:UnassignPrivateIpAddresses",
      "ec2:DescribeInstances",
      "ec2:AttachNetworkInterface"
    ]
    resources = ["*"]
  }

  statement {
    sid    = "AllowSqs"
    effect = "Allow"
    actions = [
      "sqs:ReceiveMessage",
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:ChangeMessageVisibility"
    ]
    resources = [
      "arn:aws:sqs:*:${local.account_id}:${var.environment}-${var.business_unit}-product-domain-event-queue"
    ]
  }

  statement {
    sid    = "AllowLogging"
    effect = "Allow"
    actions = [
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = [
      "arn:aws:logs:*:${local.account_id}:log-group:/aws/lambda/${local.event_handler_qualified_name}-*:*",
      "arn:aws:logs:*:${local.account_id}:log-group:/aws/lambda/audit/${local.event_handler_qualified_name}-*:*"
    ]
  }

  statement {
    sid    = "GetSecrets"
    effect = "Allow"
    actions = [
      "secretsmanager:GetSecretValue",
      "secretsmanager:DescribeSecret"
    ]
    resources = [
      "arn:aws:secretsmanager:*:${local.account_id}:secret:shared-rtl-datadog-api-key-*",
      "arn:aws:secretsmanager:*:${local.account_id}:secret:${var.environment}-rtl-rds-sql-svc_product_readwrite-*",
      "arn:aws:secretsmanager:*:${local.account_id}:secret:${var.environment}-rtl-hbg-jwt-shared-key-*",
      "arn:aws:secretsmanager:*:${local.account_id}:secret:${var.environment}-${var.business_unit}-iseries-jwt-signing-key-*"
    ]
  }

  statement {
    sid    = "AllowParameterStore"
    effect = "Allow"
    actions = [
      "ssm:GetParameter",
      "ssm:GetParameters"
    ]
    resources = [
      "arn:aws:ssm:*:${local.account_id}:parameter/shared/wafv2/token/regional/lambda",
      "arn:aws:ssm:*:${local.account_id}:parameter/shared/wafv2/token/global/lambda"
    ]
  }
}