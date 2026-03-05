# -----------------------------------------------------------------------------
# ECS Cloudwatch Log Groups
resource "aws_cloudwatch_log_group" "cluster" {
  name              = "/aws/ecs/${var.environment}-${var.business_unit}-domain-ecs-cluster"
  retention_in_days = 30

  tags = { name = "/aws/ecs/${var.environment}-${var.business_unit}-domain-ecs-cluster" }
}

# -----------------------------------------------------------------------------
# ECS Cluster
module "ecs_cluster" {
  source = "../terraform-modules/ecs-cluster"

  name = "${var.environment}-${var.business_unit}-domain-ecs-cluster"

  # Settings
  settings = {
    containerInsights = {
      name  = "containerInsights"
      value = "enabled"
    }
  }

  configuration = {
    execute_command_configuration = {
      log_configuration = {
        cloud_watch_log_group_name = aws_cloudwatch_log_group.cluster.name
      }
    }
  }

  # Tags
  tags = { name = "${var.environment}-${var.business_unit}-domain-ecs-cluster" }
}
