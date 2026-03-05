
# -----------------------------------------------------------------------------
# Load Balancer
resource "aws_lb" "this" {
  name                             = var.name_prefix != null ? null : var.name
  name_prefix                      = var.name_prefix
  load_balancer_type               = var.load_balancer_type
  internal                         = var.internal
  subnets                          = var.subnets
  security_groups                  = contains(["application", "network"], var.load_balancer_type) ? var.security_groups : null
  idle_timeout                     = var.load_balancer_type == "application" ? var.idle_timeout : null
  drop_invalid_header_fields       = var.load_balancer_type == "application" ? var.drop_invalid_header_fields : null
  enable_deletion_protection       = var.enable_deletion_protection
  enable_cross_zone_load_balancing = var.load_balancer_type == "network" ? var.enable_cross_zone_load_balancing : null
  enable_http2                     = var.load_balancer_type == "application" ? var.enable_http2 : null
  customer_owned_ipv4_pool         = var.customer_owned_ipv4_pool
  ip_address_type                  = var.ip_address_type
  desync_mitigation_mode           = var.desync_mitigation_mode
  enable_waf_fail_open             = var.enable_waf_fail_open

  dynamic "access_logs" {
    for_each = try(length(keys(var.access_logs)), {}) == 0 ? [] : [var.access_logs]

    content {
      enabled = try(access_logs.value.enabled, try(access_logs.value.bucket, null) != null)
      bucket  = try(access_logs.value.bucket, null)
      prefix  = try(access_logs.value.prefix, null)
    }
  }

  dynamic "subnet_mapping" {
    for_each = try(length(var.subnet_mapping), []) == 0 ? [] : var.subnet_mapping

    content {
      subnet_id            = try(subnet_mapping.value.subnet_id, null)
      allocation_id        = try(subnet_mapping.value.allocation_id, null)
      private_ipv4_address = try(subnet_mapping.value.private_ipv4_address, null)
      ipv6_address         = try(subnet_mapping.value.ipv6_address, null)
    }
  }

  timeouts {
    create = var.load_balancer_create_timeout
    update = var.load_balancer_update_timeout
    delete = var.load_balancer_delete_timeout
  }

  # Tags
  tags = merge(var.tags, { name = coalesce(var.name_prefix, var.name) })
}

# -----------------------------------------------------------------------------
# WACL V2 Association
resource "aws_wafv2_web_acl_association" "this" {
  count = var.web_acl_arn != null ? 1 : 0

  resource_arn = aws_lb.this.arn
  web_acl_arn  = var.web_acl_arn

  # Depends on
  depends_on = [aws_lb.this]
}
