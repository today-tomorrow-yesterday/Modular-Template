# container_definitions
variable "container_definitions" {
  description = "value"
  type        = any
  default     = null
}

# cpu
variable "cpu" {
  description = "Number of cpu units used by the task. If the `requires_compatibilities` is `FARGATE` this field is required"
  type        = number
  default     = 1024
}

# enable_fault_injection
variable "enable_fault_injection" {
  description = "Enables fault injection and allows for fault injection requests to be accepted from the task's containers. Default is `false`"
  type        = bool
  default     = null
}

# ephemeral_storage
variable "ephemeral_storage" {
  description = "The amount of ephemeral storage to allocate for the task. This parameter is used to expand the total amount of ephemeral storage available, beyond the default amount, for tasks hosted on AWS Fargate"
  type = object({
    size_in_gib = number
  })
  default = null
}

# family
variable "family" {
  description = "A unique name for your task definition"
  type        = string
  default     = null
}

# execution_role_arn
variable "execution_role_arn" {
  description = "ARN of the task execution role that the Amazon ECS container agent and the Docker daemon can assume."
  type        = string
  default     = null
}

# task_role_arn
variable "task_role_arn" {
  description = "ARN of IAM role that allows your Amazon ECS container task to make calls to other AWS services."
  type        = string
  default     = null
}

# ipc_mode
variable "ipc_mode" {
  description = "IPC resource namespace to be used for the containers in the task The valid values are `host`, `task`, and `none`"
  type        = string
  default     = null
}

# memory
variable "memory" {
  description = "Amount (in MiB) of memory used by the task. If the `requires_compatibilities` is `FARGATE` this field is required"
  type        = number
  default     = 2048
}

# network_mode
variable "network_mode" {
  description = "Docker networking mode to use for the containers in the task. Valid values are `none`, `bridge`, `awsvpc`, and `host`"
  type        = string
  default     = "awsvpc"
}

# pid_mode
variable "pid_mode" {
  description = "Process namespace to use for the containers in the task. The valid values are `host` and `task`"
  type        = string
  default     = null
}

# placement_constraints
variable "placement_constraints" {
  description = "Configuration block for rules that are taken into consideration during task placement (up to max of 10). This is set at the task definition, see `placement_constraints` for setting at the service"
  type = map(object({
    expression = optional(string)
    type       = string
  }))
  default = null
}

# proxy_configuration
variable "proxy_configuration" {
  description = "Configuration block for the App Mesh proxy"
  type = object({
    container_name = string
    properties     = optional(map(string))
    type           = optional(string)
  })
  default = null
}

# requires_compatibilities
variable "requires_compatibilities" {
  description = "Set of launch types required by the task. The valid values are `EC2` and `FARGATE`"
  type        = list(string)
  default     = ["FARGATE"]
}

# runtime_platform
variable "runtime_platform" {
  description = "Configuration block for `runtime_platform` that containers in your task may use"
  type = object({
    cpu_architecture        = optional(string, "X86_64")
    operating_system_family = optional(string, "LINUX")
  })
  default = {
    operating_system_family = "LINUX"
    cpu_architecture        = "X86_64"
  }
}

# skip_destroy
variable "skip_destroy" {
  description = "Whether to retain the old revision when the resource is destroyed or replacement is necessary. Default is `false`"
  type        = bool
  default     = false
}

# volume
variable "volumes" {
  description = "Configuration block for volumes that containers in your task may use"
  type = map(object({
    name                = optional(string)
    host_path           = optional(string)
    configure_at_launch = optional(bool)
    docker_volume_configuration = optional(object({
      autoprovision = optional(bool)
      driver        = optional(string)
      driver_opts   = optional(map(string))
      labels        = optional(map(string))
      scope         = optional(string)
    }))
    efs_volume_configuration = optional(object({
      authorization_config = optional(object({
        access_point_id = optional(string)
        iam             = optional(string)
      }))
      file_system_id          = string
      root_directory          = optional(string)
      transit_encryption      = optional(string)
      transit_encryption_port = optional(number)
    }))
    fsx_windows_file_server_volume_configuration = optional(object({
      authorization_config = optional(object({
        credentials_parameter = string
        domain                = string
      }))
      file_system_id = string
      root_directory = string
    }))
  }))
  default = null
}

# track_latest
variable "track_latest" {
  description = "Whether should track latest `ACTIVE` task definition on AWS or the one created with the resource stored in state. Default is `false`. Useful in the event the task definition is modified outside of this resource"
  type        = bool
  default     = true
}

# tags
variable "tags" {
  description = "Resource tags to aply to all resources."
  type        = map(string)
  default     = {}
}
