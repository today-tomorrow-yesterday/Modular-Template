aws_region    = "us-east-1"
business_unit = "rtl"
project       = "rtl-domain-product-api"

# Lambda
lambda_handler_deploy_package_to_s3 = false
lambda_handler_function_name        = "rtl-domain-product-api"
lambda_handler_description          = "Sample Lambda"
lambda_handler_package_name         = "lambda-builds/Domain.Product.Api.zip"
lambda_handler_handler              = "Domain.Product.Api"
lambda_handler_memory_size          = 512
lambda_handler_runtime              = "dotnet8"
lambda_handler_timeout              = 30
lambda_handler_publish              = false
lambda_handler_include_in_vpc       = true

######
# Tags
tags = {
  businessunit    = "rtl"
  compliance      = "none"
  confidentiality = "internal"
  costcenter      = "rtl"
  environment     = "tbd"
  name            = "rtl-domain-product-api"
  owner           = "RetailWeb@ClaytonHomes.com"
  project         = "rtl-domain-product-api"
  persistence     = "always-on"
  recoverytier    = "tier3"
  repo            = "https://github.com/clayton-homes/rtl-domain-product-api"
  confluence      = "https://wiki.clayton.net/display/RA/Product+Domain"
  workload        = "rtl-domain-product-api"
}
