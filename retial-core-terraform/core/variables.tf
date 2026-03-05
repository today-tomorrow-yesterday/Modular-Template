variable "business_unit" {
  description = "Business unit workload belongs to"
  type        = string
  default     = "rtl"
}
variable "environment" {
  description = "Environment workload is being deployed to"
  type        = string
  default     = null
}
variable "aws_region" {
  description = "AWS region workload is being deployed to"
  type        = string
  default     = "us-east-1"
}
variable "region" {
  description = "Optional alias for aws_region to support shared tfvars files"
  type        = string
  default     = "us-east-1"
}
# Tags
variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = null
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
