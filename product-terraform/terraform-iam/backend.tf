# Backend
terraform {
  backend "s3" {
    key    = "rtl-domain-product-api-iam/terraform.tfstate"
    region = var.aws_region
  }
}