# =========================================================================== #
# Region, environment and business unit
# =========================================================================== #
environment = "prod"


# =========================================================================== #
# Tags
# =========================================================================== #
tags = {
  environment     = "prod"
  businessunit    = "rtl"
  costcenter      = "rtl"
  department      = "Retail"
  owner           = "RetailEbiz@ClaytonHomes.com"
  name            = "prod-rtl-domain-product-api"
  project         = "Product Domain Api"
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
  service                    = "Domain.Product.Api"
}

lambda_datadog_variables = {
  dd_site                 = "datadoghq.com"
  dd_api_key_secret       = "arn:aws:secretsmanager:us-east-1:311309317961:secret:shared-rtl-datadog-api-key-cgrC1m"
  aws_lambda_exec_wrapper = "/opt/datadog_wrapper"
  layers = [
    "arn:aws:lambda:us-east-1:464622532012:layer:dd-trace-dotnet:16",
    "arn:aws:lambda:us-east-1:464622532012:layer:Datadog-Extension:65"
  ]
}