locals {
  api_qualified_name           = "${var.environment}-${var.business_unit}-${var.project_prefix}-api"
  event_handler_qualified_name = "${var.environment}-${var.business_unit}-${var.project_prefix}-event-handler"
}

# =========================================================================== #
# Role 
# =========================================================================== #

locals {
  product_domain_api_lambda_name = "${local.api_qualified_name}-lambda"
  api_iam_role_name              = "AR-${local.api_qualified_name}-lambda-role"

  product_domain_event_handler_lambda_name = "${local.event_handler_qualified_name}-lambda"
  event_handler_iam_role_name              = "AR-${local.event_handler_qualified_name}-lambda-role"
}