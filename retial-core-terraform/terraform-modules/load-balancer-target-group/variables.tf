variable "name" {
  description = "Name of the resource"
  type        = string
  default     = null
}
variable "name_prefix" {
  description = "Name prefix of the resource"
  type        = string
  default     = null
}
variable "port" {
  description = "Port on which targets receive traffic, unless overridden when registering a specific target. Required when target_type is instance, ip or alb. Does not apply when target_type is lambda."
  type        = number
  default     = null
}
variable "protocol" {
  description = "Protocol to use for routing traffic to the targets. Should be one of GENEVE, HTTP, HTTPS, TCP, TCP_UDP, TLS, or UDP. Required when target_type is instance, ip or alb. Does not apply when target_type is lambda."
  type        = string
  default     = null
}
variable "protocol_version" {
  description = "Only applicable when protocol is HTTP or HTTPS. The protocol version. Specify GRPC to send requests to targets using gRPC. Specify HTTP2 to send requests to targets using HTTP/2. The default is HTTP1, which sends requests to targets using HTTP/1.1"
  type        = string
  default     = null
}
variable "target_type" {
  description = "Type of target that you must specify when registering targets with this target group."
  type        = string
  default     = null
}
variable "health_check" {
  description = "Health Check configuration block."
  type        = any
  default     = null

}
variable "connection_termination" {
  description = "Whether to terminate connections at the end of the deregistration timeout on Network Load Balancers."
  type        = bool
  default     = false
}
variable "ip_address_type" {
  description = "The type of IP addresses used by the target group, only supported when target type is set to ip. Possible values are ipv4 or ipv6."
  type        = string
  default     = null
}
variable "load_balancing_cross_zone_enabled" {
  description = "Indicates whether cross zone load balancing is enabled. The value is true, false or use_load_balancer_configuration. The default is use_load_balancer_configuration."
  type        = string
  default     = null
}
variable "deregistration_delay" {
  description = "Amount time for Elastic Load Balancing to wait before changing the state of a deregistering target from draining to unused. The range is 0-3600 seconds. The default value is 300 seconds."
  type        = number
  default     = null
}
variable "slow_start" {
  description = "Amount time for targets to warm up before the load balancer sends them a full share of requests. The range is 30-900 seconds or 0 to disable. The default value is 0 seconds."
  type        = number
  default     = null
}
variable "proxy_protocol_v2" {
  description = "Whether to enable support for proxy protocol v2 on Network Load Balancers. Default is false."
  type        = bool
  default     = null
}
variable "lambda_multi_value_headers_enabled" {
  description = "Whether the request and response headers exchanged between the load balancer and the Lambda function include arrays of values or strings. Only applies when target_type is lambda. Default is false."
  type        = bool
  default     = null
}
variable "load_balancing_algorithm_type" {
  description = "Determines how the load balancer selects targets when routing requests. Only applicable for Application Load Balancer Target Groups. The value is round_robin or least_outstanding_requests. The default is round_robin."
  type        = string
  default     = null
}
variable "preserve_client_ip" {
  description = "Whether client IP preservation is enabled."
  type        = bool
  default     = null
}
variable "target_failover" {
  description = "Whether the request and response headers exchanged between the load balancer and the Lambda function include arrays of values or strings. Only applies when target_type is lambda. Default is false."
  type        = any
  default     = null
}

variable "stickiness" {
  description = "Stickiness configuration block."
  type        = any
  default     = null
}
variable "vpc_id" {
  description = "VPC to put the Target Group in."
  type        = string
  default     = null
}
variable "targets" {
  description = "Target group targets"
  type        = any
  default     = {}
}
variable "tags" {
  description = "A map of tags to add to all resources"
  type        = map(string)
  default     = {}
}
