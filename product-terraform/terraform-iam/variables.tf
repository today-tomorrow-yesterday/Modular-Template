# Information about input variables can be found here:
# https://developer.hashicorp.com/terraform/language/values/variables

#########
# General

variable "aws_region" {
  description = "AWS region the workload is being deployed to"
  type        = string
  default     = "us-east-1"
  sensitive   = false

  validation {
    condition     = var.aws_region != null
    error_message = "The aws region variable must not empty. It can be set in the individual config files."
  }
}

variable "business_unit" {
  description = "Business unit the workload belongs to"
  type        = string
  default     = "rtl"
  sensitive   = false

  validation {
    condition     = var.business_unit != null
    error_message = "The business unit variable must not empty. It can be set in the individual config files."
  }
}

variable "environment" {
  description = "Environment the workload is being deployed to"
  type        = string
  default     = null
  sensitive   = false

  validation {
    condition     = var.environment != null
    error_message = "The environment variable must not empty. It can be set in the individual config files."
  }
}

variable "project" {
  description = "The project name to use when naming AWS resources, with words separated by dashes (e.g. clayton-built)"
  type        = string
  default     = ""
  sensitive   = false

  validation {
    condition     = var.project != ""
    error_message = "The project  variable must not empty. It can be set in the individual config files."
  }
}

# =========================================================================== #
# Project Related
# =========================================================================== #
variable "project_prefix" {
  description = "The prefix to use for naming AWS resources, with words separated by dashes (e.g. clayton-built)"
  type        = string
  default     = "product-domain"
  sensitive   = false

  validation {
    condition     = var.project_prefix != ""
    error_message = "The project_prefix variable must not empty. It can be set in the individual config files."
  }
}

# =========================================================================== #
# Tags

# Information about required tags can be found here:
# https://wiki.clayton.net/display/ECT/AWS+Tagging

variable "tags" {
  description = "Tags that should be applied to a resource"
  type        = map(string)
  default     = {}
  sensitive   = false

  # Information about required tags can be found here:
  # https://wiki.clayton.net/display/ECT/AWS+Tagging

  validation {
    condition     = alltrue([for required_tag in ["environment", "businessunit", "compliance", "confidentiality", "costcenter", "owner", "name", "project", "persistence", "recoverytier", "workload", "terraformstate"] : (lookup(var.tags, required_tag, "") != "")])
    error_message = "Required tags must be present and not empty. It can be set in the individual config files."
  }
}