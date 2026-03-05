module "lambda_handler" {
  source  = "terraform.clayton.net/gh_claytonhomes/aws_lambda/aws"
  version = "0.1.0"

  deploy_package_to_s3 = var.lambda_handler_deploy_package_to_s3

  # Deployment bucket (For packages deployed to S3)
  s3_deploy_bucket     = var.lambda_handler_deploy_package_to_s3 ? "${local.s3_bucket_prefix}.${var.business_unit}.code-deploy" : null
  s3_deploy_bucket_key = var.lambda_handler_deploy_package_to_s3 ? "${var.project}/${var.lambda_handler_package_name}" : null

  # Filename (For packages included with the terraform)
  filename         = "${var.lambda_path}/${local.product_lambda_dotnet_project_name}.zip"
  source_code_hash = filebase64sha256("${var.lambda_path}/${local.product_lambda_dotnet_project_name}.zip")

  # Lambda settings
  function_name        = "${local.qualified_project_name}-${local.lambda_name}"
  function_description = "Lambda function for ${var.project_prefix} to get sale data by proxy"
  handler              = var.lambda_handler_handler
  runtime              = "dotnet8"
  memory_size          = 1024
  timeout              = 30
  exec_role_arn        = data.aws_iam_role.product_domain_lambda.arn

  # Pass any environment variables the Lambda needs here
  environment_variables = {
    environment                  = var.environment
    ASPNETCORE_ENVIRONMENT       = var.environment
    SSL_CERT_FILE                = local.lambda_cert_path
    DD_SITE                      = var.lambda_datadog_variables.dd_site
    DD_ENV                       = var.environment
    DD_VERSION                   = "${var.build_version}"
    DD_SERVICE                   = var.logging_datadog_tags.service
    DD_SERVERLESS_APPSEC_ENABLED = true
    DD_TRACE_SAMPLE_RATE         = 1.0
    DD_RUNTIME_METRICS_ENABLED   = true
    DD_TRACE_DEBUG               = true
    DD_TRACE_CLIENT_IP_ENABLED   = true
    DD_TRACE_PROPAGATION_STYLE   = "tracecontext"
    DD_DBM_PROPAGATION_MODE      = "full"
    DD_LOGS_INJECTION            = true
    DD_TRACE_ENABLED             = true
    DD_PROFILING_ENABLED         = true
    DD_TRACE_OTEL_ENABLED        = true
    DD_SERVERLESS_LOGS_ENABLED   = true
    DD_SERVERLESS_APPSEC_ENABLED = true
    DD_API_KEY_SECRET_ARN        = var.lambda_datadog_variables.dd_api_key_secret
    AWS_LAMBDA_EXEC_WRAPPER      = var.lambda_datadog_variables.aws_lambda_exec_wrapper
    DD_TAGS                      = "env:${var.environment} version:${var.build_version} service:${var.logging_datadog_tags.service} application:${var.tags.project} project:${var.tags.project} costcenter:${var.tags.costcenter} businessunit:${var.tags.businessunit}"
    LAMBDA_NAME                  = "${local.qualified_project_name}-${local.lambda_name}"
  }

  # Datadog Lambda Layer
  layers = var.lambda_datadog_variables.layers

  # VPC - Only Needed for calling other AWS resources in the VPC OR calling on-premise APIs in non-prod
  vpc_config = {
    subnet_ids         = [local.subnets.private_x, local.subnets.private_y, local.subnets.private_z]
    security_group_ids = [local.security_groups.clayton_datacenter, local.security_groups.private]
  }

  tags = merge(data.aws_default_tags.this.tags, {
    name = "${var.environment}-${var.business_unit}-${var.lambda_handler_function_name}"
  })
}

resource "aws_cloudwatch_log_group" "lambda_lambda_log_group" {
  name              = "/aws/lambda/${module.lambda_handler.function_name}"
  retention_in_days = var.lambda_handler_log_retention_days

  tags = merge(data.aws_default_tags.this.tags, var.logging_datadog_tags, {
    name = "${module.lambda_handler.function_name}-log"
  })
}

resource "aws_cloudwatch_log_group" "lambda_audit_log_group" {
  name              = "/aws/lambda/audit/${module.lambda_handler.function_name}"
  retention_in_days = var.lambda_handler_log_retention_days

  tags = merge(data.aws_default_tags.this.tags, var.logging_datadog_tags, {
    name = "${module.lambda_handler.function_name}-audit-log"
  })
}

# JWT Key for HBG Data Domain Access
resource "aws_secretsmanager_secret" "hbg_jwt_shared_key" {
  name                    = "${var.environment}-${var.business_unit}-hbg-jwt-shared-key"
  description             = "JWT Key for HBG Data Domain Access"
  recovery_window_in_days = 0
  tags = {
    name = "${var.environment}-${var.business_unit}-hbg-jwt-shared-key"
  }
}

resource "aws_secretsmanager_secret_version" "hbg_jwt_shared_key_version" {
  secret_id     = aws_secretsmanager_secret.hbg_jwt_shared_key.id
  secret_string = var.shared_key
}