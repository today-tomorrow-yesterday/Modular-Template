# -----------------------------------------------------------------------------
# Target Group Attachments
resource "aws_lb_target_group_attachment" "this" {
  target_group_arn  = var.target_group_arn
  target_id         = var.target_id
  port              = var.port
  availability_zone = var.availability_zone

  depends_on = [aws_lambda_permission.this]
}

# -----------------------------------------------------------------------------
# Lambda Permission 
resource "aws_lambda_permission" "this" {
  count = try(length(keys(var.lambda_permission)), 0) == 0 ? 0 : 1

  statement_id        = try(var.lambda_permission.statement_id, null)
  statement_id_prefix = try(var.lambda_permission.statement_id_prefix, null)
  action              = try(var.lambda_permission.action, "lambda:InvokeFunction")
  function_name       = try(var.lambda_permission.function_name, null)
  principal           = try(var.lambda_permission.principal, "elasticloadbalancing.amazonaws.com")
  qualifier           = try(var.lambda_permission.qualifier, null)
  source_arn          = var.target_group_arn
}
