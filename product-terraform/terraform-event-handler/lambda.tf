module "lambda_handler" {
  source  = "terraform.clayton.net/gh_claytonhomes/aws_lambda/aws"
  version = "0.1.0"

  deploy_package_to_s3 = var.lambda_deploy_package_to_s3

  filename         = "${var.lambda_path}/${local.product_event_handler_lambda_dotnet_project_name}.zip"
  source_code_hash = filebase64sha256("${var.lambda_path}/${local.product_event_handler_lambda_dotnet_project_name}.zip")

  function_name        = "${local.qualified_project_name}-handler-lambda"
  function_description = "Lambda function for processing product domain events from SQS"
  handler              = var.lambda_handler
  runtime              = var.lambda_runtime
  memory_size          = var.lambda_memory_size
  timeout              = var.lambda_timeout
  exec_role_arn        = data.aws_iam_role.product_domain_event_handler_lambda.arn
  publish              = var.lambda_publish

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
    LAMBDA_NAME                  = "${local.qualified_project_name}-handler-lambda"
  }

  layers = var.lambda_datadog_variables.layers

  vpc_config = {
    subnet_ids         = [local.subnets.private_x, local.subnets.private_y, local.subnets.private_z]
    security_group_ids = [local.security_groups.clayton_datacenter, local.security_groups.private]
  }

  tags = merge(data.aws_default_tags.this.tags, {
    name = "${var.environment}-${var.business_unit}-${var.lambda_function_name}"
  })
}

resource "aws_cloudwatch_log_group" "lambda_lambda_log_group" {
  name              = "/aws/lambda/${module.lambda_handler.function_name}"
  retention_in_days = var.lambda_log_retention_days

  tags = merge(data.aws_default_tags.this.tags, var.logging_datadog_tags, {
    name = "${module.lambda_handler.function_name}-log"
  })
}