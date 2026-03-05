# Check for the latest version of the AWS Provider here:
# https://registry.terraform.io/providers/hashicorp/aws/latest/docs

# More information about provider requirements here:
# https://developer.hashicorp.com/terraform/language/providers/requirements

# Providers
provider "aws" {
  region = var.aws_region

  default_tags {
    tags = merge(var.tags, {
      environment  = var.environment
      businessunit = var.business_unit
      costcenter   = var.business_unit
      project      = var.project
      workload     = var.project
      name         = "${var.environment}-${var.business_unit}-${var.project}"
    })
  }
}

data "aws_default_tags" "this" {}

# Required Providers and Versions
terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.53.0"
    }
  }
}
