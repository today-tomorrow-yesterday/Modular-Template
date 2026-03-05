# Information about input variables can be found here:
# https://developer.hashicorp.com/terraform/language/values/variables

#########
# General

variable "business_unit" {
  description = "Business unit workload belongs to"
  type        = string
  default     = null
  sensitive   = false
}
variable "environment" {
  description = "Environment workload is being deployed to"
  type        = string
  default     = null
  sensitive   = false
}
variable "aws_region" {
  description = "AWS region workload is being deployed to"
  type        = string
  default     = "us-east-1"
  sensitive   = false
}
variable "project" {
  description = "The project to use for naming AWS resources, with words separated by dashes (e.g. clayton-built)"
  type        = string
  default     = ""
  sensitive   = false
}
variable "is_staging" {
  description = "Indicates a staging environment (Prod only)"
  type        = bool
  default     = false
}


######
# Tags

# Information about required tags can be found here:
# https://wiki.clayton.net/display/ECT/AWS+Tagging

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = null
  sensitive   = false

  validation {
    condition     = alltrue([for required_tag in ["environment", "businessunit", "costcenter", "owner", "name", "project", "workload", "compliance", "confidentiality", "recoverytier"] : (lookup(var.tags, required_tag, "") != "")])
    error_message = "Required tags (environment, businessunit, costcenter, owner, name, project, workload, compliance, confidentiality, recoverytier) must not be empty. Tags should be set in variables.auto.tfvars."
  }
}