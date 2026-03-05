module "funding_domain_module" {
  source = "../../terraform-modules/domain-module"

  providers = {
    aws = aws
  }

  # Required Variables
  business_unit = var.business_unit
  environment   = var.environment
  aws_region    = var.aws_region
  region        = var.region
  service_name  = var.service_name

  # ECS Service
  desired_service_count = var.desired_service_count
  container_port        = var.container_port

  # ECS Task
  container_cpu    = var.container_cpu
  container_memory = var.container_memory
  container_image  = var.container_image

  # Route53
  create_route53_record = var.create_route53_record
  acm_certificate_arn   = var.acm_certificate_arn

  # IAM
  service_iam_role                    = var.service_iam_role
  task_execution_iam_role             = var.task_execution_iam_role
  task_execution_parameter_store_arns = var.task_execution_parameter_store_arns
  task_execution_secrets_manager_arns = var.task_execution_secrets_manager_arns
  task_iam_role                       = var.task_iam_role
  infrastructure_iam_role             = var.infrastructure_iam_role
  permissions_boundary_arn            = var.permissions_boundary_arn

  # Tags
  tags = var.tags
}

module "event_integration" {
  source = "../../terraform-modules/event-integration"

  providers = {
    aws = aws
  }

  environment         = var.environment
  business_unit       = var.business_unit
  service_name        = var.service_name
  emb_spoke_name      = var.emb_spoke_name
  event_subscriptions = var.event_subscriptions
  tags                = var.tags
}
