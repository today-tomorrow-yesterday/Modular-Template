# Backend
terraform {
  backend "s3" {
    key    = "global/iam/core-api/iam.tfstate"
    region = "us-east-1"
  }
}
