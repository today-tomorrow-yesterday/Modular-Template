data "aws_caller_identity" "this" {}
data "aws_partition" "current" {}

module "aws_data" {
  source  = "terraform.clayton.net/gh_claytonhomes/aws_data/aws"
  version = "1.3.0"

  environment   = var.environment
  business_unit = var.business_unit

  providers = {
    aws = aws
  }

  vpc_include             = true
  subnets_include         = true
  security_groups_include = true
  key_pair_include        = true
  route_53_include        = true
  acm_include             = true
  wafv2_acl_include       = true
  wafv2_acl_token_include = true
}

# WAF TOKENS
data "aws_ssm_parameter" "wafv2_alb_wacl_token" {
  name = "/shared/wafv2/token/regional/alb"
}

data "aws_ssm_parameter" "wafv2_cf_wacl_token" {
  name = "/shared/wafv2/token/regional/cloudfront"
}

data "aws_ssm_parameter" "wafv2_lambda_wacl_token" {
  name = "/shared/wafv2/token/regional/lambda"
}

data "aws_ssm_parameter" "wafv2_global_lambda_wacl_token" {
  name = "/shared/wafv2/token/global/lambda"
}

resource "random_string" "target_group" {
  length  = 5
  special = false
  upper   = false
}

locals {
  vpc                  = module.aws_data.vpc
  subnets              = module.aws_data.subnets
  security_groups      = module.aws_data.security_groups
  key_pair             = module.aws_data.key_pair
  route_53             = module.aws_data.route_53
  acm_certificate      = module.aws_data.acm_certificate
  waf_web_acl_v2       = module.aws_data.wafv2_web_acl
  waf_web_acl_token_v2 = module.aws_data.wafv2_web_acl_token
  kms_keys             = module.aws_data.kms_keys

  # Runtime Secrets
  runtime_environment_secrets = [
    {
      name      = "ALB_TOKEN",
      valueFrom = data.aws_ssm_parameter.wafv2_alb_wacl_token.arn
    },
    {
      name      = "LAMBDA_TOKEN",
      valueFrom = data.aws_ssm_parameter.wafv2_lambda_wacl_token.arn
    },
    {
      name      = "CLOUDFRONT_TOKEN",
      valueFrom = data.aws_ssm_parameter.wafv2_cf_wacl_token.arn
    },
    {
      name      = "GLOBAL_LAMBDA_TOKEN",
      valueFrom = data.aws_ssm_parameter.wafv2_global_lambda_wacl_token.arn
    }
  ]

  # Parameter Store Arns the task needs access to
  task_execution_parameter_store_arns = [
    data.aws_ssm_parameter.wafv2_alb_wacl_token.arn,
    data.aws_ssm_parameter.wafv2_lambda_wacl_token.arn,
    data.aws_ssm_parameter.wafv2_cf_wacl_token.arn,
    data.aws_ssm_parameter.wafv2_global_lambda_wacl_token.arn
  ]

  # Secrets Manager Arns the task needs access to
  task_execution_secrets_manager_arns = []
}

data "aws_ecs_cluster" "retail_ecs_cluster" {
  cluster_name = "${var.environment}-${var.business_unit}-domain-ecs-cluster"
}
