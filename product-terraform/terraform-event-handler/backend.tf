# Backend
terraform {
  backend "s3" {
    key    = "rtl-domain-product-event-handler/terraform.tfstate"
    region = "us-east-1"
  }
}