# -----------------------------------------------------------------------------
# ECS Cluster Resource
# -----------------------------------------------------------------------------
resource "aws_ecs_cluster" "this" {
  name = var.name

  dynamic "configuration" {
    for_each = try(length(keys(var.configuration)), 0) > 0 ? [var.configuration] : []

    content {
      # execute_command_configuration
      dynamic "execute_command_configuration" {
        for_each = try(length(keys(configuration.value.execute_command_configuration)), 0) > 0 ? [configuration.value.execute_command_configuration] : []

        content {
          kms_key_id = try(execute_command_configuration.value.kms_key_id, null)

          # log_configuration
          dynamic "log_configuration" {
            for_each = try(length(keys(execute_command_configuration.value.log_configuration)), 0) > 0 ? [execute_command_configuration.value.log_configuration] : []

            content {
              cloud_watch_encryption_enabled = try(log_configuration.value.cloud_watch_encryption_enabled, false)
              cloud_watch_log_group_name     = try(log_configuration.value.cloud_watch_log_group_name, null)
              s3_bucket_encryption_enabled   = try(log_configuration.value.s3_bucket_encryption_enabled, null)
              s3_bucket_name                 = try(log_configuration.value.s3_bucket_name, null)
              s3_key_prefix                  = try(log_configuration.value.s3_key_prefix, null)
            }
          }

          logging = try(execute_command_configuration.value.logging, null)
        }
      }

      # managed_storage_configuration
      dynamic "managed_storage_configuration" {
        for_each = try(length(keys(configuration.value.managed_storage_configuration)), 0) > 0 ? [configuration.value.managed_storage_configuration] : []

        content {
          fargate_ephemeral_storage_kms_key_id = try(managed_storage_configuration.value.fargate_ephemeral_storage_kms_key_id, null)
          kms_key_id                           = try(managed_storage_configuration.value.kms_key_id, null)
        }
      }
    }
  }

  # service_connect_defaults
  dynamic "service_connect_defaults" {
    for_each = try(length(keys(var.service_connect_defaults)), 0) > 0 ? [var.service_connect_defaults] : []

    content {
      namespace = service_connect_defaults.value.namespace
    }
  }

  # settings
  dynamic "setting" {
    for_each = try(length(keys(var.settings)), 0) > 0 ? var.settings : {}

    content {
      name  = setting.value.name
      value = setting.value.value
    }
  }

  tags = merge(var.tags, { name = var.name })
}

# -----------------------------------------------------------------------------
# ECS Cluster Capacity Providers
# -----------------------------------------------------------------------------
# TODO
