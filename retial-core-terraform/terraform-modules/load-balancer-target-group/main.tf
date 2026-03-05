
# -----------------------------------------------------------------------------
# Load Balancer Target Groups
resource "aws_lb_target_group" "this" {
  name                               = var.name_prefix != null ? null : var.name
  name_prefix                        = var.name_prefix
  vpc_id                             = var.vpc_id
  port                               = var.port
  protocol                           = var.protocol
  protocol_version                   = var.protocol_version
  target_type                        = var.target_type
  deregistration_delay               = var.deregistration_delay
  slow_start                         = var.slow_start
  proxy_protocol_v2                  = var.proxy_protocol_v2
  lambda_multi_value_headers_enabled = var.lambda_multi_value_headers_enabled
  load_balancing_algorithm_type      = var.load_balancing_algorithm_type
  connection_termination             = var.connection_termination
  preserve_client_ip                 = var.preserve_client_ip
  ip_address_type                    = var.ip_address_type

  dynamic "health_check" {
    for_each = var.health_check != null ? [var.health_check] : []

    content {
      enabled             = try(health_check.value.enabled, null)
      healthy_threshold   = try(health_check.value.healthy_threshold, null)
      unhealthy_threshold = try(health_check.value.unhealthy_threshold, null)
      timeout             = try(health_check.value.timeout, null)
      interval            = try(health_check.value.interval, null)
      path                = try(health_check.value.path, null)
      port                = try(health_check.value.port, null)
      protocol            = try(health_check.value.protocol, null)
      matcher             = try(health_check.value.matcher, null)
    }
  }

  dynamic "stickiness" {
    for_each = var.stickiness != null ? [var.stickiness] : []

    content {
      enabled         = try(stickiness.value.enabled, null)
      cookie_duration = try(stickiness.value.cookie_duration, null)
      cookie_name     = try(stickiness.value.cookie_name, null)
      type            = try(stickiness.value.type, null)
    }
  }

  dynamic "target_failover" {
    for_each = var.target_failover != null ? [var.target_failover] : []

    content {
      on_deregistration = try(target_failover.value.on_deregistration, null)
      on_unhealthy      = try(target_failover.value.on_unhealthy, null)
    }
  }

  tags = var.tags

  lifecycle {
    create_before_destroy = true
  }
}

# -----------------------------------------------------------------------------
# Target Group Attachments
module "target_group_attachment" {
  source   = "./target-group-attachment"
  for_each = var.targets

  target_group_arn  = aws_lb_target_group.this.arn
  target_id         = each.value.target_id
  port              = try(each.value.port, null)
  availability_zone = try(each.value.availability_zone, null)
  lambda_permission = try(each.value.lambda_permission, {})

  # Depends on
  depends_on = [aws_lb_target_group.this]
}
