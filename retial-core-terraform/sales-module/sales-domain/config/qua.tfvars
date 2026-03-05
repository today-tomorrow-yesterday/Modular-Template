# ====================================================== #
# Region, environment, and business unit
# ====================================================== #
environment   = "qua"
aws_region    = "us-east-1"
region        = "us-east-1"
business_unit = "rtl"

# ====================================================== #
# AWS ECS Tags
# ====================================================== #
tags = {
  environment     = "qua"
  businessunit    = "rtl"
  costcenter      = "rtl"
  department      = "Retail"
  owner           = "RetailEbizDevs@Clayton.net"
  name            = "qua-rtl-sales-domain-module"
  project         = "Retail Core API Project"
  audience        = "Internal"
  persistance     = "Always On"
  compliance      = "NPI"
  confidentiality = "Internal"
  recoverytier    = "tier4"
  workload        = "Sales Module Domain API"
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
emb_spoke_name = "qua-rtl-emb-spoke"
event_subscriptions = [
  # Customer
  "rtl.customer.partyCreated",
  "rtl.customer.partyNameChanged",
  "rtl.customer.partyHomeCenterChanged",
  "rtl.customer.partyContactPointsChanged",
  "rtl.customer.partySalesAssignmentsChanged",
  "rtl.customer.partyCoBuyerChanged",
  "rtl.customer.partyOnboardedFromLoan",
  "rtl.customer.partyLifecycleAdvanced",
  "rtl.customer.partyMailingAddressChanged",
  # Inventory
  "rtl.inventory.onLotHomeAddedToInventory",
  "rtl.inventory.onLotHomeRemovedFromInventory",
  "rtl.inventory.onLotHomePriceRevised",
  "rtl.inventory.onLotHomeDetailsRevised",
  "rtl.inventory.landParcelAddedToInventory",
  "rtl.inventory.landParcelRemovedFromInventory",
  "rtl.inventory.landParcelDetailsRevised",
  "rtl.inventory.landParcelAppraisalRevised",
  # Organization
  "rtl.organization.userAccessGranted",
  "rtl.organization.userAccessChanged",
  "rtl.organization.homeCenterChanged",
  # Funding
  "rtl.funding.fundingRequestSubmitted",
  "rtl.funding.fundingRequestStatusChanged",
]
