# Check for the latest version of the AWS Provider here:
# https://registry.terraform.io/providers/hashicorp/aws/latest/docs

provider "aws" {
  region = var.aws_region
}

terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.38.0"
    }
  }
}
