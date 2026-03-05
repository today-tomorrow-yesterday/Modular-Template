# name
variable "name" {
  description = "Name of the ecs service"
  type        = string
}

# cluster
variable "cluster" {
  description = "ARN of the ecs cluster to attach the service to"
  type        = string
  default     = null
}

# launch_type
variable "launch_type" {
  description = "Launch type on which to run the service. The valid values are `EC2`, `FARGATE`, and `EXTERNAL`."
  type        = string
  default     = "FARGATE"

  validation {
    condition     = contains(["EC2", "FARGATE", "EXTERNAL"], var.launch_type)
    error_message = "The launch_type variable can only be set to 'EC2', 'FARGATE', or 'EXTERNAL'."
  }
}

# desired_count
variable "desired_count" {
  description = "Number of instances of the task definition to keep running"
  type        = number
  default     = 1
}

# task_definition
variable "task_definition" {
  description = "Family and revision (family:revision) or full ARN of the task definition that you want to run in your service."
  type        = string
  default     = null
}

# network_configuration
variable "network_configuration" {
  description = "Network configuration for the service."
  type = object({
    subnets          = list(string)
    security_groups  = optional(list(string), [])
    assign_public_ip = optional(bool, false)
  })
  default = null
}

# load_balancer
variable "load_balancer" {
  description = "Configuration block for load balancers"
  type = object({
    container_name   = string
    container_port   = number
    elb_name         = optional(string)
    target_group_arn = optional(string)
  })
  default = null
}

# wait_for_steady_state
variable "wait_for_steady_state" {
  description = "If true, Terraform will wait for the service to reach a steady state before continuing. Default is `false`"
  type        = bool
  default     = null
}

# availability_zone_rebalancing
variable "availability_zone_rebalancing" {
  description = "ECS automatically redistributes tasks within a service across Availability Zones (AZs) to mitigate the risk of impaired application availability due to underlying infrastructure failures and task lifecycle activities. The valid values are `ENABLED` and `DISABLED`. Defaults to `DISABLED`"
  type        = string
  default     = null
}

# deployment_maximum_percent
variable "deployment_maximum_percent" {
  description = "Upper limit (as a percentage of the service's `desired_count`) of the number of running tasks that can be running in a service during a deployment"
  type        = number
  default     = 200
}

# deployment_minimum_healthy_percent
variable "deployment_minimum_healthy_percent" {
  description = "Lower limit (as a percentage of the service's `desired_count`) of the number of running tasks that must remain running and healthy in a service during a deployment"
  type        = number
  default     = 66
}

# enable_ecs_managed_tags
variable "enable_ecs_managed_tags" {
  description = "Specifies whether to enable Amazon ECS managed tags for the tasks within the service"
  type        = bool
  default     = true
}

# enable_execute_command
variable "enable_execute_command" {
  description = "Specifies whether to enable Amazon ECS Exec for the tasks within the service"
  type        = bool
  default     = false
}

# force_delete
variable "force_delete" {
  description = "Enable to delete a service even if it wasn't scaled down to zero tasks. It's only necessary to use this if the service uses the `REPLICA` scheduling strategy"
  type        = bool
  default     = null
}

# force_new_deployment
variable "force_new_deployment" {
  description = "Enable to force a new task deployment of the service. This can be used to update tasks to use a newer Docker image with same image/tag combination, roll Fargate tasks onto a newer platform version, or immediately deploy `ordered_placement_strategy` and `placement_constraints` updates"
  type        = bool
  default     = true
}

# health_check_grace_period_seconds
variable "health_check_grace_period_seconds" {
  description = "Seconds to ignore failing load balancer health checks on newly instantiated tasks to prevent premature shutdown, up to 2147483647. Only valid for services configured to use load balancers"
  type        = number
  default     = null
}

# iam_role
variable "iam_role" {
  description = "ARN of the IAM role that allows Amazon ECS to make calls to your load balancer on your behalf."
  type        = string
  default     = null
}

# alarms
variable "alarms" {
  description = "Information about the CloudWatch alarms"
  type = object({
    alarm_names = list(string)
    enable      = optional(bool, true)
    rollback    = optional(bool, true)
  })
  default = null
}

# capacity_provider_strategy
variable "capacity_provider_strategy" {
  description = "Capacity provider strategies to use for the service. Can be one or more"
  type = map(object({
    base              = optional(number)
    capacity_provider = string
    weight            = optional(number)
  }))
  default = null
}

# deployment_circuit_breaker
variable "deployment_circuit_breaker" {
  description = "Configuration block for deployment circuit breaker"
  type = object({
    enable   = bool
    rollback = bool
  })
  default = null
}

# deployment_controller
variable "deployment_controller" {
  description = "Configuration block for deployment controller configuration"
  type = object({
    type = optional(string)
  })
  default = null
}

# ordered_placement_strategy
variable "ordered_placement_strategy" {
  description = "Service level strategy rules that are taken into consideration during task placement. List from top to bottom in order of precedence"
  type = map(object({
    field = optional(string)
    type  = string
  }))
  default = null
}

# placement_constraints
variable "placement_constraints" {
  description = "Configuration block for rules that are taken into consideration during task placement (up to max of 10). This is set at the service, see `task_definition_placement_constraints` for setting at the task definition"
  type = map(object({
    expression = optional(string)
    type       = string
  }))
  default = null
}

# platform_version
variable "platform_version" {
  description = "Platform version on which to run your service. Only applicable for `launch_type` set to `FARGATE`. Defaults to `LATEST`"
  type        = string
  default     = null
}

# propagate_tags
variable "propagate_tags" {
  description = "Specifies whether to propagate the tags from the task definition or the service to the tasks. The valid values are `SERVICE` and `TASK_DEFINITION`"
  type        = string
  default     = null
}

# scheduling_strategy
variable "scheduling_strategy" {
  description = "Scheduling strategy to use for the service. The valid values are `REPLICA` and `DAEMON`. Defaults to `REPLICA`"
  type        = string
  default     = null
}

# service_connect_configuration
variable "service_connect_configuration" {
  description = "The ECS Service Connect configuration for this service to discover and connect to services, and be discovered by, and connected from, other services within a namespace"
  type = object({
    enabled = optional(bool, true)
    log_configuration = optional(object({
      log_driver = string
      options    = optional(map(string))
      secret_option = optional(list(object({
        name       = string
        value_from = string
      })))
    }))
    namespace = optional(string)
    service = optional(list(object({
      client_alias = optional(object({
        dns_name = optional(string)
        port     = number
      }))
      discovery_name        = optional(string)
      ingress_port_override = optional(number)
      port_name             = string
      timeout = optional(object({
        idle_timeout_seconds        = optional(number)
        per_request_timeout_seconds = optional(number)
      }))
      tls = optional(object({
        issuer_cert_authority = object({
          aws_pca_authority_arn = string
        })
        kms_key  = optional(string)
        role_arn = optional(string)
      }))
    })))
  })
  default = null
}

# triggers
variable "triggers" {
  description = "Map of arbitrary keys and values that, when changed, will trigger an in-place update (redeployment). Useful with `timestamp()`"
  type        = map(string)
  default     = null
}

# service_registries
variable "service_registries" {
  description = "Service discovery registries for the service"
  type = object({
    container_name = optional(string)
    container_port = optional(number)
    port           = optional(number)
    registry_arn   = string
  })
  default = null
}

# volume_configurations
variable "volume_configurations" {
  description = "Configurations for volumes specified in the task definition as a volume that is configured at launch time"
  type = map(object({
    name = string
    managed_ebs_volume = object({
      encrypted        = optional(bool)
      file_system_type = optional(string)
      iops             = optional(number)
      kms_key_id       = optional(string)
      role_arn         = optional(string)
      size_in_gb       = optional(number)
      snapshot_id      = optional(string)
      tag_specifications = optional(list(object({
        propagate_tags = optional(string, "TASK_DEFINITION")
        resource_type  = string
        tags           = optional(map(string))
      })))
      throughput  = optional(number)
      volume_type = optional(string)
    })
  }))
  default = null
}

# vpc_lattice_configurations
variable "vpc_lattice_configurations" {
  description = "The VPC Lattice configuration for your service that allows Lattice to connect, secure, and monitor your service across multiple accounts and VPCs"
  type = object({
    role_arn         = string
    target_group_arn = string
    port_name        = string
  })
  default = null
}

# timeouts
variable "timeouts" {
  description = "Create, update, and delete timeout configurations for the service"
  type = object({
    create = optional(string)
    update = optional(string)
    delete = optional(string)
  })
  default = null
}

# tags
variable "tags" {
  description = "Resource tags to aply to all resources."
  type        = map(string)
  default     = {}
}
