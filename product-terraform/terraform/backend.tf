# Backend
terraform {
  backend "s3" {
    key    = "rtl-domain-product-api/terraform.tfstate"
    region = "us-east-1"
  }
}