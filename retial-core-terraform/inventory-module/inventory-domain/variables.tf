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
  default     = null
}
variable "service_name" {
  description = "The prefix to use for naming AWS resources, with words separated by dashes (e.g. clayton-built)"
  type        = string
  default     = "inventory-domain-module"

  validation {
    condition     = length(trimspace(var.service_name)) > 0
    error_message = "service_name cannot be empty; provide a dash-delimited string such as 'ecs-poc'."
  }
}

# ECS Service
variable "desired_service_count" {
  description = "The desired ECS service count"
  type        = number
  default     = 1
}

variable "container_port" {
  description = "Port on the container to associate with the load balancer."
  type        = number
  default     = 8000
}

# ECS Task
variable "container_cpu" {
  description = "CPU units for the container (1024 = 1 vCPU)"
  type        = number
  default     = 512
}
variable "container_memory" {
  description = "Memory in MB for the container"
  type        = number
  default     = 1024
}
variable "container_image" {
  description = "Docker image for the application"
  type        = string
  default     = null
}

# Route53
variable "create_route53_record" {
  description = "Boolean indicating whether to create a Route53 record."
  type        = bool
  default     = true
}
variable "acm_certificate_arn" {
  description = "The ACM certificate ARN to use if not using the default CCT certificate"
  type        = string
  default     = null
}

# IAM
variable "service_iam_role" {
  description = "Map indicating whether to create the service IAM Role. Provide policy statements if true and the existing ARN if false"
  type = object({
    create = optional(bool, true)
    arn    = optional(string, null)
    policy_statements = optional(list(object({
      sid           = optional(string)
      actions       = optional(list(string))
      not_actions   = optional(list(string))
      effect        = optional(string)
      resources     = optional(list(string))
      not_resources = optional(list(string))
      principals = optional(list(object({
        type        = string
        identifiers = list(string)
      })))
      not_principals = optional(list(object({
        type        = string
        identifiers = list(string)
      })))
      condition = optional(list(object({
        test     = string
        values   = list(string)
        variable = string
      })))
    })))
  })
  default = {
    create = false
  }

  validation {
    condition = (
      var.service_iam_role.create == false ||
      length(coalesce(var.service_iam_role.policy_statements, [])) > 0
    )
    error_message = "Provide at least one policy statement when service_iam_role.create is true."
  }
}

# task_execution_iam_role
variable "task_execution_iam_role" {
  description = "Map indicating whether to create the task execution IAM Role."
  type = object({
    create = optional(bool, true)
    arn    = optional(string, null)
  })
  default = {
    create = true
  }
}

# task_execution_parameter_store_arns
variable "task_execution_parameter_store_arns" {
  description = "The parameter store arns the task needs access to"
  type        = list(string)
  default     = []
}

# task_execution_secrets_manager_arns
variable "task_execution_secrets_manager_arns" {
  description = "The secrets manager secrets the task needs access to"
  type        = list(string)
  default     = []
}

# task_iam_role
variable "task_iam_role" {
  description = "Map indicating whether to create the task IAM Role. Provide policy statements if true and the existing ARN if false"
  type = object({
    create = optional(bool, true)
    arn    = optional(string, null)
    policy_statements = optional(list(object({
      sid           = optional(string)
      actions       = optional(list(string))
      not_actions   = optional(list(string))
      effect        = optional(string)
      resources     = optional(list(string))
      not_resources = optional(list(string))
      principals = optional(list(object({
        type        = string
        identifiers = list(string)
      })))
      not_principals = optional(list(object({
        type        = string
        identifiers = list(string)
      })))
      condition = optional(list(object({
        test     = string
        values   = list(string)
        variable = string
      })))
    })))
  })
  default = {
    create = true
  }
}

# infrastructure_iam_role
variable "infrastructure_iam_role" {
  description = "Map indicating whether to create the infrastructure IAM Role. Provide policy statements if true and the existing ARN if false"
  type = object({
    create                    = optional(bool, true)
    arn                       = optional(string, null)
    attach_aws_managed_policy = optional(bool, true)
  })
  default = {
    attach_aws_managed_policy = true
    create                    = true
  }
}

# permissions_boundary_arn
variable "permissions_boundary_arn" {
  description = "value"
  type        = string
  default     = null
}

# Event Integration
variable "emb_spoke_name" {
  description = "The EventBridge event bus (spoke) name to consume events from"
  type        = string
  default     = ""
}

variable "event_subscriptions" {
  description = "List of event detail-types to subscribe to (e.g. ['rtl.sales.saleSummaryChanged'])"
  type        = list(string)
  default     = []
}

# Tags
variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = null
}
