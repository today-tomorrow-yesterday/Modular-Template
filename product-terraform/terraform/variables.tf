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

#########
# Secrets 

variable "shared_key" {
  description = "Signing key pairs for authorization tokens"
  type        = string
  default     = ""
  sensitive   = true
}

#################
# Lambda function 

variable "lambda_handler_deploy_package_to_s3" {
  description = "Boolean indicating whether to deploy to package to S3."
  type        = string
  default     = false
  sensitive   = false
}

variable "lambda_handler_function_name" {
  description = "Name of the Lambda (excluding env and bu)"
  type        = string
  default     = ""
  sensitive   = false
}

variable "lambda_handler_description" {
  description = "Lambda function description"
  type        = string
  default     = ""
  sensitive   = false
}

variable "lambda_handler_package_name" {
  description = "Name of the Lambda package in S3"
  type        = string
  default     = ""
  sensitive   = false
}

variable "lambda_handler_handler" {
  description = "Function entrypoint in code."
  type        = string
  default     = "Domain.Product.Api"
  sensitive   = false
}

variable "lambda_handler_memory_size" {
  description = "Amount of memory in MB the Lambda Function can use at runtime"
  type        = number
  default     = 256
  sensitive   = false
}

variable "lambda_handler_runtime" {
  description = "Identifier of the Lambda function runtime. See https://docs.aws.amazon.com/lambda/latest/dg/API_CreateFunction.html#SSS-CreateFunction-request-Runtime for valid values."
  type        = string
  default     = ""
  sensitive   = false
}

variable "lambda_handler_timeout" {
  description = "Amount of time the Lambda Function has to run in seconds"
  type        = number
  default     = 3
  sensitive   = false
}

variable "lambda_handler_publish" {
  description = "Whether to publish creation/change as new Lambda Function Version"
  type        = bool
  default     = false
  sensitive   = false
}

variable "lambda_handler_include_in_vpc" {
  description = "Whether to put the Lambda into the VPC"
  type        = bool
  default     = true
  sensitive   = false
}

variable "lambda_handler_log_retention_days" {
  description = "Days to retain CloudWatch logs for the lambda function"
  type        = number
  default     = 14
  sensitive   = false
}

# =========================================================================== #
# Project Related
# =========================================================================== #
variable "project_prefix" {
  description = "The prefix to use for naming AWS resources, with words separated by dashes (e.g. clayton-built)"
  type        = string
  default     = "product-domain-api"
  sensitive   = false

  validation {
    condition     = var.project_prefix != ""
    error_message = "The project_prefix variable must not empty. It can be set in the individual config files."
  }
}

variable "lambda_path" {
  description = "The path to lambda zip files"
  type        = string
  default     = "lambda-builds"
  sensitive   = false

  validation {
    condition     = var.lambda_path != ""
    error_message = "The lambda_path variable must not empty. It can be set in the individual config files."
  }
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

# =========================================================================== #
# Logging
# =========================================================================== #
variable "logging_config" {
  description = "The logging configuration"
  type        = object({})
  default     = {}
  sensitive   = false
}

variable "logging_datadog_tags" {
  description = "A map of tags, applied to the Lambda LogGroups"
  type        = map(string)
  default     = {}
}

# =========================================================================== #
# Datadog
# =========================================================================== #
variable "lambda_datadog_variables" {
  default = {}
}

variable "build_version" {
  description = "The version of the build"
  type        = string
  default     = "1.0.0"
  sensitive   = false
}