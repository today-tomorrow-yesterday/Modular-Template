locals {
  qualified_project_name             = "${var.environment}-${var.business_unit}-${var.project_prefix}"
  lambda_cert_path                   = "/var/task/ClaytonInternalRootCA.pem"
  github_env_variables               = jsondecode(file("${path.module}/terraform.tfvars.json"))
  product_lambda_dotnet_project_name = "Domain.Product.Api"
  lambda_name                        = "product-lambda"
}