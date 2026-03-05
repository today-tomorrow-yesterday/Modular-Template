# name
variable "name" {
  description = "Name of the ecs cluster"
  type        = string
}


# configuration
variable "configuration" {
  description = "The execute command configuration for the cluster."
  type = object({
    execute_command_configuration = optional(object({
      kms_key_id = optional(string)
      log_configuration = optional(object({
        cloud_watch_encryption_enabled = optional(bool)
        cloud_watch_log_group_name     = optional(string)
        s3_bucket_encryption_enabled   = optional(bool)
        s3_bucket_name                 = optional(string)
        s3_kms_key_id                  = optional(string)
        s3_key_prefix                  = optional(string)
      }))
      logging = optional(string, "OVERRIDE")
    }))
    managed_storage_configuration = optional(object({
      fargate_ephemeral_storage_kms_key_id = optional(string)
      kms_key_id                           = optional(string)
    }))
  })
  default = {}
}


# service_connect_defaults
variable "service_connect_defaults" {
  description = "Default Service Connect namespace."
  type = object({
    namespace = string
  })
  default = null
}

# settings
variable "settings" {
  description = "Map of configuration block(s) with cluster settings."
  type = map(object({
    name  = string
    value = string
  }))
  default = {
    containerInsights = {
      name  = "containerInsights"
      value = "enabled"
    }
  }
}

# tags
variable "tags" {
  description = "Resource tags to aply to all resources."
  type        = map(string)
  default     = {}
}
