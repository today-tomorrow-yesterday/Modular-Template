# =========================================================================== #
# IAM Role for Product Domain API Lambda
# =========================================================================== #
resource "aws_iam_role" "product_domain_api_lambda_role" {
  name               = local.api_iam_role_name
  description        = "Sets up the role for the Product Domain API Lambda"
  assume_role_policy = data.aws_iam_policy_document.product_domain_api_lambda_trust_policy.json

  permissions_boundary = "arn:aws:iam::${local.account_id}:policy/BP-GH-Deploy-AppRoleBoundary-${var.environment}"

  tags = {
    name           = local.api_iam_role_name,
    targetresource = local.product_domain_api_lambda_name
  }
}

resource "aws_iam_role_policy" "product_domain_api_lambda_role_policy" {
  name   = "permissions_policy"
  role   = aws_iam_role.product_domain_api_lambda_role.id
  policy = data.aws_iam_policy_document.product_domain_api_lambda_permissions_policy.json
}

# =========================================================================== #
# IAM Role for Product Domain Event Handler Lambda
# =========================================================================== #
resource "aws_iam_role" "product_domain_event_handler_lambda_role" {
  name               = local.event_handler_iam_role_name
  description        = "Sets up the role for the Product Domain Event Handler Lambda"
  assume_role_policy = data.aws_iam_policy_document.product_domain_event_handler_lambda_trust_policy.json

  permissions_boundary = "arn:aws:iam::${local.account_id}:policy/BP-GH-Deploy-AppRoleBoundary-${var.environment}"

  tags = {
    name           = local.event_handler_iam_role_name,
    targetresource = local.product_domain_event_handler_lambda_name
  }
}

resource "aws_iam_role_policy" "product_domain_event_handler_lambda_role_policy" {
  name   = "permissions_policy"
  role   = aws_iam_role.product_domain_event_handler_lambda_role.id
  policy = data.aws_iam_policy_document.product_domain_event_handler_lambda_permissions_policy.json
}