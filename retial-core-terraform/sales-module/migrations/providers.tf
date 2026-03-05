terraform {
  required_providers {
    aws = {
      version = "= 5.70.0"
      source  = "hashicorp/aws"
    }
  }
}

provider "aws" {
  region = var.aws_region
}
