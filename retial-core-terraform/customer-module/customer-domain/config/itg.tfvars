# ====================================================== #
# Region, environment, and business unit
# ====================================================== #
environment   = "itg"
aws_region    = "us-east-1"
region        = "us-east-1"
business_unit = "rtl"

# ====================================================== #
# AWS ECS Tags
# ====================================================== #
tags = {
  environment     = "itg"
  businessunit    = "rtl"
  costcenter      = "rtl"
  department      = "Retail"
  owner           = "RetailEbizDevs@Clayton.net"
  name            = "itg-rtl-customer-domain-module"
  project         = "Retail Core API Project"
  audience        = "Internal"
  persistance     = "Always On"
  compliance      = "NPI"
  confidentiality = "Internal"
  recoverytier    = "tier4"
  workload        = "Customer Module Domain API"
}

# ====================================================== #
# IAM Role configuration
# ====================================================== #
service_iam_role = {
  create = true
  policy_statements = [
    {
      sid    = "AllowTargetGroupRegistration"
      effect = "Allow"
      actions = [
        "elasticloadbalancing:DeregisterTargets",
        "elasticloadbalancing:RegisterTargets",
        "elasticloadbalancing:DescribeLoadBalancers",
        "elasticloadbalancing:DescribeTargetHealth"
      ]
      resources = ["*"]
    },
    {
      sid    = "AllowServiceUpdates"
      effect = "Allow"
      actions = [
        "ecs:DescribeServices",
        "ecs:DescribeTaskDefinition",
        "ecs:UpdateService"
      ]
      resources = ["*"]
    },
    {
      sid    = "AllowAutoScalingIntegration"
      effect = "Allow"
      actions = [
        "application-autoscaling:RegisterScalableTarget",
        "application-autoscaling:DeregisterScalableTarget",
        "application-autoscaling:DescribeScalableTargets",
        "application-autoscaling:DescribeScalingActivities",
        "application-autoscaling:DescribeScalingPolicies",
        "application-autoscaling:PutScalingPolicy",
        "application-autoscaling:DeleteScalingPolicy"
      ]
      resources = ["*"]
    }
  ]
}

task_iam_role = {
  create = true
  policy_statements = [
    {
      sid    = "AllowEventBridgePublish"
      effect = "Allow"
      actions = [
        "events:PutEvents"
      ]
      resources = ["*"]
    },
    {
      sid    = "AllowSQSAccess"
      effect = "Allow"
      actions = [
        "sqs:ReceiveMessage",
        "sqs:DeleteMessage",
        "sqs:GetQueueAttributes",
        "sqs:GetQueueUrl",
        "sqs:ChangeMessageVisibility"
      ]
      resources = ["*"]
    }
  ]
}

infrastructure_iam_role = {
  create                    = true
  attach_aws_managed_policy = true
}

# ====================================================== #
# Event Integration
# ====================================================== #
emb_spoke_name      = "nonprod-rtl-emb-spoke"
event_subscriptions = []
