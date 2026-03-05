# Backend
terraform {
  backend "s3" {
    key    = "rtl-domain-product-api-database/terraform.tfstate"
    region = "us-east-1"
  }
}