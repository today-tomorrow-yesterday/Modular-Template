# =========================================================================== #
# Region, environment and business unit
# =========================================================================== #
variable "aws_region" {
  description = "AWS region the workload is being deployed to"
  type        = string
  default     = "us-east-1"
  sensitive   = false

  validation {
    condition     = var.aws_region != null
    error_message = "The aws region variable must not be empty."
  }
}

variable "business_unit" {
  description = "Business unit the workload belongs to"
  type        = string
  default     = "rtl"
  sensitive   = false

  validation {
    condition     = var.business_unit != null
    error_message = "The business unit variable must not be empty."
  }
}

variable "environment" {
  description = "Environment the workload is being deployed to"
  type        = string
  default     = null
  sensitive   = false

  validation {
    condition     = var.environment != null
    error_message = "The environment variable must not be empty."
  }
}

# =========================================================================== #
# Enterprise Event Bus
# =========================================================================== #
variable "enterprise_message_bus_name" {
  description = "The name of the enterprise event bus"
  type        = string
  default     = null
  sensitive   = false

  validation {
    condition     = var.enterprise_message_bus_name != null
    error_message = "The enterprise_message_bus_name variable must not be empty."
  }
}

# =========================================================================== #
# Project Related
# =========================================================================== #
variable "project_prefix" {
  description = "The prefix to use for naming AWS resources, with words separated by dashes"
  type        = string
  default     = "core-api"
  sensitive   = false

  validation {
    condition     = var.project_prefix != ""
    error_message = "The project_prefix variable must not be empty."
  }
}

# =========================================================================== #
# Tags
# =========================================================================== #
variable "tags" {
  description = "Tags that should be applied to a resource"
  type        = map(string)
  default     = {}
  sensitive   = false

  # Information about required tags can be found here:
  # https://wiki.clayton.net/display/ECT/AWS+Tagging

  validation {
    condition     = alltrue([for required_tag in ["environment", "businessunit", "costcenter", "owner", "name", "project", "compliance", "confidentiality", "recoverytier", "workload"] : (lookup(var.tags, required_tag, "") != "")])
    error_message = "Required tags must be present and not empty."
  }
}
