# Check for the latest version of the AWS Provider here:
# https://registry.terraform.io/providers/hashicorp/aws/latest/docs

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = merge(var.tags, {
      environment    = var.environment
      name           = "${var.environment}-${var.business_unit}-${var.project}"
      terraformstate = "${var.environment}.${var.business_unit}.${var.aws_region}.terraform-state/global/iam/${var.environment}.${var.business_unit}.${var.project}/iam.tfstate"
    })
  }
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