# Check for the latest version of the AWS Provider here:
# https://registry.terraform.io/providers/hashicorp/aws/latest/docs

# More information about proider requirements here:
# https://developer.hashicorp.com/terraform/language/providers/requirements

# Providers
provider "aws" {
  region = coalesce(var.region, var.aws_region)

  default_tags {
    tags = merge(var.tags, { environment = var.environment })
  }
}

# Required Providers and Versions
terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.2.0"
    }
  }
  backend "s3" {
    bucket = ""
    key    = ""
    region = ""
  }
}
