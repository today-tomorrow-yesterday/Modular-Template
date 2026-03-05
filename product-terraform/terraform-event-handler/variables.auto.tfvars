# =========================================================================== #
# Region, environment and business unit
# =========================================================================== #
environment = "dev"

# =========================================================================== #
# Event Handler Lambda Configuration
# =========================================================================== #
lambda_function_name              = "product-event-handler"
lambda_handler                    = "Domain.Product.EventHandler::Domain.Product.EventHandler.Function::FunctionHandler"
lambda_runtime                    = "dotnet8"
lambda_timeout                    = 60
lambda_memory_size                = 512
lambda_publish                    = true
lambda_description                = "Processes product domain events from SQS queue"
lambda_package_name               = "Domain.Product.EventHandler.zip"
lambda_deploy_package_to_s3       = true
lambda_include_in_vpc             = false
lambda_log_retention_days         = 14
lambda_invocation_max_concurrency = 10

# =========================================================================== #
# SQS
# =========================================================================== #
lambda_batch_size = 10

# =========================================================================== #
# EventBridge
# =========================================================================== #
retail_emb_spoke_name = "nonprod-rtl-emb-spoke"

# =========================================================================== #
# Tags
# =========================================================================== #
tags = {
  environment     = "dev"
  businessunit    = "rtl"
  costcenter      = "rtl"
  department      = "Retail"
  owner           = "RetailEbiz@ClaytonHomes.com"
  name            = "dev-rtl-domain-product-event-handler"
  project         = "Product Domain Event Handler"
  audience        = "Internal"
  persistence     = "Always On"
  compliance      = "PII"
  confidentiality = "Internal"
  recoverytier    = "tier4"
  workload        = "product service"
}

logging_datadog_tags = {
  datadog-enable-logging     = true
  datadog-event-handling     = "index"
  datadog-event-filter-level = "info"
  service                    = "Domain.Product.EventHandler"
}

lambda_datadog_variables = {
  dd_site                 = "datadoghq.com"
  dd_api_key_secret       = "arn:aws:secretsmanager:us-east-1:380478784443:secret:shared-rtl-datadog-api-key-6CPj7U"
  aws_lambda_exec_wrapper = "/opt/datadog_wrapper"
  layers = [
    "arn:aws:lambda:us-east-1:464622532012:layer:dd-trace-dotnet:16",
    "arn:aws:lambda:us-east-1:464622532012:layer:Datadog-Extension:65"
  ]
}