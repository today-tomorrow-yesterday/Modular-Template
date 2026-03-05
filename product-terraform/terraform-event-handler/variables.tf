variable "aws_region" {
  description = "AWS region workload is being deployed to"
  type        = string
  default     = "us-east-1"
}

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

variable "is_staging" {
  description = "Indicates a staging environment (Prod only)"
  type        = bool
  default     = false
}

variable "project" {
  description = "The project to use for naming AWS resources, with words separated by dashes (e.g. clayton-built)"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = null
}

# Event Handler Lambda Variables
variable "lambda_function_name" {
  description = "Name of the Event Handler Lambda (excluding env and bu)"
  type        = string
  default     = ""
}

variable "lambda_handler" {
  description = "Function entrypoint in code"
  type        = string
  default     = ""
}

variable "lambda_runtime" {
  description = "Identifier of the Lambda function runtime"
  type        = string
  default     = ""
}

variable "lambda_timeout" {
  description = "Amount of time the Lambda Function has to run in seconds"
  type        = number
  default     = 60
}

variable "lambda_memory_size" {
  description = "Amount of memory in MB the Lambda Function can use at runtime"
  type        = number
  default     = 512
}

variable "lambda_publish" {
  description = "Whether to publish creation/change as new Lambda Function Version"
  type        = bool
  default     = false
}

variable "lambda_description" {
  description = "Lambda function description"
  type        = string
  default     = ""
}

variable "lambda_package_name" {
  description = "Name of the Event Handler Lambda package in S3"
  type        = string
  default     = ""
}

variable "lambda_deploy_package_to_s3" {
  description = "Boolean indicating whether to deploy package to S3"
  type        = string
  default     = false
}

variable "lambda_include_in_vpc" {
  description = "Whether to put the Lambda into the VPC"
  type        = bool
  default     = false
}

variable "lambda_log_retention_days" {
  description = "Days to retain CloudWatch logs for the lambda function"
  type        = number
  default     = 14
}

variable "lambda_batch_size" {
  description = "Maximum number of messages to retrieve from SQS in a single batch"
  type        = number
  default     = 10
}

variable "lambda_invocation_max_concurrency" {
  description = "Maximum number of concurrent Lambda invocations"
  type        = number
  default     = 10
}

variable "project_prefix" {
  description = "The prefix to use for naming AWS resources, with words separated by dashes (e.g. clayton-built)"
  type        = string
  default     = "product-domain-event"
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

variable "logging_datadog_tags" {
  description = "A map of tags, applied to the Lambda LogGroups"
  type        = map(string)
  default     = {}
}

variable "retail_emb_spoke_name" {
  description = "The name of the Retail Event Mesh Spoke to subscribe to"
  type        = string
  default     = ""
  sensitive   = false

  validation {
    condition     = var.retail_emb_spoke_name != ""
    error_message = "The ${var.retail_emb_spoke_name} variable must not empty. It can be set in the individual config files."
  }
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