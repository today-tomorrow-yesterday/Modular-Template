# Check for the latest version of the AWS Provider here:
# https://registry.terraform.io/providers/hashicorp/aws/latest/docs

# More information about provider requirements here:
# https://developer.hashicorp.com/terraform/language/providers/requirements

# Providers
provider "aws" {
  region = var.aws_region
}

data "aws_default_tags" "this" {}

terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.53.0"
    }
  }
}