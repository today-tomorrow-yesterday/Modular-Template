# Check for the latest version of the AWS Data module here:
# https://proget.clayton.net/feeds/Terraform/claytonhomes/aws-data/aws/versions

module "aws_data" {
  source  = "terraform.clayton.net/gh_claytonhomes/aws_data/aws"
  version = "1.3.0"

  environment   = var.environment
  business_unit = var.business_unit

  providers = {
    aws = aws
  }

  acm_include                 = true
  ec2_profile_include         = false
  key_pair_include            = false
  kms_keys_dynamodb_include   = false
  kms_keys_include            = false
  lambda_profile_include      = true
  password_rotation_include   = false
  rds_domain_include          = false
  rds_parameter_group_include = false
  route_53_include            = false
  security_groups_include     = true
  subnets_include             = true
  vpc_include                 = true
  wafv2_acl_include           = false
  wafv2_acl_token_include     = true
}

# AWS Account info
data "aws_caller_identity" "current" {}

# AWS Region info
data "aws_region" "current" {}

# Reference state file outputs here using the terraform_remote_state data resources. For more information:
# https://developer.hashicorp.com/terraform/language/state/remote-state-data

# Locals to expose data module and terraform_remote_state outputs for consumption
locals {
  # Environment Name (For naming resources)
  environment_name = (var.environment == "prod" && var.is_staging) ? "prod-staging" : var.environment

  # AWS Account Id 
  aws_account_id = data.aws_caller_identity.current.account_id

  # S3 Bucket Prefix
  s3_bucket_prefix = (var.environment == "prod" && var.is_staging) ? "prod.staging" : var.environment

  vpc                         = module.aws_data.vpc
  subnets                     = module.aws_data.subnets
  security_groups             = module.aws_data.security_groups
  key_pair                    = module.aws_data.key_pair
  route_53                    = module.aws_data.route_53
  acm_certificate             = module.aws_data.acm_certificate
  ec2_iam_instance_profile    = module.aws_data.ec2_iam_instance_profile
  lambda_iam_instance_profile = module.aws_data.lambda_iam_instance_profile
  waf_web_acl_v2              = module.aws_data.wafv2_web_acl
  waf_web_acl_token_v2        = module.aws_data.wafv2_web_acl_token
  rds_password_rotation       = module.aws_data.rds_password_rotation
  kms_keys                    = module.aws_data.kms_keys
  rds_db_parameter_group      = module.aws_data.rds_db_parameter_group
  rds_cluster_parameter_group = module.aws_data.rds_cluster_parameter_group
  rds_domain                  = module.aws_data.rds_domain
}

data "aws_iam_role" "product_domain_event_handler_lambda" {
  name = "AR-${local.qualified_project_name}-handler-lambda-role"
}