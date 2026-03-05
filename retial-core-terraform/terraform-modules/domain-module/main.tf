# -----------------------------------------------------------------------------
# ECS Cloudwatch Log Groups
resource "aws_cloudwatch_log_group" "task" {
  name              = "/aws/ecs/${var.environment}-${var.service_name}-ecs-task"
  retention_in_days = 30

  tags = { name = "/aws/ecs/${var.environment}-${var.service_name}-ecs-task" }
}

# -----------------------------------------------------------------------------
# ECS Container Definition
module "container_definition" {
  source = "../ecs-container-definition"

  name      = "${var.environment}-${var.business_unit}-${var.service_name}-container"
  image     = var.container_image
  essential = true

  # Port Mappings
  portMappings = [
    {
      containerPort = var.container_port
      protocol      = "tcp"
    }
  ]

  # Log Configuration
  logConfiguration = {
    logDriver = "awslogs"
    options = {
      awslogs-group         = aws_cloudwatch_log_group.task.name
      awslogs-region        = coalesce(var.region, var.aws_region)
      awslogs-stream-prefix = "ecs"
    }
  }

  # Environment
  environment = [
    {
      name  = "ENVIRONMENT"
      value = var.environment
    },
    {
      name  = "HOSTNAME"
      value = "0.0.0.0"
    },
    {
      name  = "SERVER_PORT"
      value = var.container_port
    }
  ]

  # Mount Points
  mountPoints = [
    {
      "sourceVolume"  = "${var.environment}-${var.business_unit}-${var.service_name}-ebs-volume"
      "containerPath" = "/tmp"
      "readOnly"      = false
    }
  ]

  # Secrets
  secrets = local.runtime_environment_secrets
}

locals {
  # JSON Encode Container Definition
  task_container_definitions = jsonencode([module.container_definition.definition])
}

# -----------------------------------------------------------------------------
# ECS Task
module "ecs_task" {
  source = "../ecs-task"

  family                   = "${var.environment}-${var.business_unit}-${var.service_name}"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.container_cpu
  memory                   = var.container_memory
  execution_role_arn       = var.task_execution_iam_role.create ? aws_iam_role.task_execution[0].arn : var.task_execution_iam_role.arn
  task_role_arn            = var.task_iam_role.create ? aws_iam_role.task[0].arn : var.task_iam_role.arn

  # Container definition
  container_definitions = local.task_container_definitions

  # Volumes
  volumes = {
    default = {
      name                = "${var.environment}-${var.business_unit}-${var.service_name}-ebs-volume"
      configure_at_launch = true
    }
  }

  tags = { name = "${var.environment}-${var.business_unit}-${var.service_name}-task-definition" }
}

# -----------------------------------------------------------------------------
# Load Balancer
module "load_balancer" {
  source = "../load-balancer"

  name               = "${var.environment}-${var.business_unit}-${var.service_name}-alb"
  load_balancer_type = "application"
  internal           = false
  enable_http2       = true
  subnets            = [local.subnets.public_x, local.subnets.public_y]
  security_groups    = [local.security_groups.public_internal, local.security_groups.public_external, local.security_groups.private]
  web_acl_arn        = local.waf_web_acl_v2.regional_public

  # Tags
  tags = { name = "${var.environment}-${var.business_unit}-${var.service_name}-alb" }
}

# -----------------------------------------------------------------------------
# Target Group
module "load_balancer_target_group" {
  source = "../load-balancer-target-group"

  name        = "${var.environment}-${var.business_unit}-${var.service_name}-${random_string.target_group.result}-tg"
  vpc_id      = local.vpc.id
  port        = var.container_port
  protocol    = "HTTP"
  target_type = "ip"
  health_check = {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200-399"
    path                = "/"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 2
  }

  # NOTE: ECS Will Handle Target Registration
  targets = {}

  # Tags
  tags = { name = "${var.environment}-${var.business_unit}-${var.service_name}-tg" }
}

# -----------------------------------------------------------------------------
# Load Balancer Listeners (HTTP automatically redirects to HTTPS)
# module "load_balancer_http_listener" {
#   source = "./modules/load_balancer_listener"

#   load_balancer_arn = module.load_balancer.load_balancer.arn
#   port              = 80
#   protocol          = "HTTP"
#   default_action = {
#     type = "redirect",
#     redirect = {
#       host        = "#{host}"
#       path        = "/#{path}"
#       port        = 443
#       protocol    = "HTTPS"
#       query       = "#{query}"
#       status_code = "HTTP_301"
#     }
#   }

#   # Tags
#   tags = { name = "${var.environment}-${var.business_unit}-${var.service_name}-http-listener" }
# }

module "load_balancer_https_listener" {
  source = "../load-balancer-listener"

  load_balancer_arn = module.load_balancer.load_balancer.arn
  port              = 443
  protocol          = "HTTPS"
  certificate_arn   = coalesce(var.acm_certificate_arn, local.acm_certificate.arn)
  default_action = {
    type             = "forward",
    target_group_arn = module.load_balancer_target_group.arn
  }

  # Tags
  tags = { name = "${var.environment}-${var.business_unit}-${var.service_name}-https-listener" }
}

# -----------------------------------------------------------------------------
# ECS Service
module "ecs_service" {
  source = "../ecs-service"

  name            = "${var.environment}-${var.business_unit}-${var.service_name}-ecs-service"
  cluster         = data.aws_ecs_cluster.retail_ecs_cluster.arn
  launch_type     = "FARGATE"
  desired_count   = var.desired_service_count
  task_definition = module.ecs_task.arn
  iam_role        = var.service_iam_role.create ? aws_iam_role.service[0].arn : var.service_iam_role.arn

  # network_configuration
  network_configuration = {
    security_groups  = [local.security_groups.public_internal, local.security_groups.public_external, local.security_groups.private, local.security_groups.clayton_datacenter]
    subnets          = [local.subnets.private_x, local.subnets.private_y]
    assign_public_ip = false
  }

  # load_balancer
  load_balancer = {
    target_group_arn = module.load_balancer_target_group.arn
    container_name   = module.container_definition.name
    container_port   = var.container_port
  }

  # volume_configuration
  volume_configurations = {
    default = {
      name = "${var.environment}-${var.business_unit}-${var.service_name}-ebs-volume"
      managed_ebs_volume = {
        role_arn         = var.infrastructure_iam_role.create ? aws_iam_role.infrastructure[0].arn : var.infrastructure_iam_role.arn
        file_system_type = "xfs"
        size_in_gb       = "5"
      }
    }
  }

  # Tags
  tags = { name = "${var.environment}-${var.business_unit}-${var.service_name}-ecs-service" }
}

# -----------------------------------------------------------------------------
# Route53 Record
resource "aws_route53_record" "record" {
  count = var.create_route53_record ? 1 : 0

  zone_id = local.route_53.zone_id
  name    = "${var.service_name}.${var.environment}.cct-${var.business_unit}.com"
  type    = "A"

  alias {
    name                   = module.load_balancer.load_balancer.dns_name
    zone_id                = module.load_balancer.load_balancer.zone_id
    evaluate_target_health = false
  }
}
