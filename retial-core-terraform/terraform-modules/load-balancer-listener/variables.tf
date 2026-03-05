variable "port" {
  description = "Port on which the load balancer is listening. Not valid for Gateway Load Balancers."
  type        = number
  default     = null
}
variable "protocol" {
  description = "Protocol for connections from clients to the load balancer. For Application Load Balancers, valid values are HTTP and HTTPS, with a default of HTTP. For Network Load Balancers, valid values are TCP, TLS, UDP, and TCP_UDP. Not valid to use UDP or TCP_UDP if dual-stack mode is enabled. Not valid for Gateway Load Balancers."
  type        = string
  default     = null
}
variable "certificate_arn" {
  description = "ARN of the default SSL server certificate. Exactly one certificate is required if the protocol is HTTPS."
  type        = string
  default     = null
}
variable "load_balancer_listener" {
  description = "Load Balancer Listener"
  type        = any
  default     = {}
}
variable "load_balancer_arn" {
  description = "ARN of the Load Balancer"
  type        = string
  default     = ""
}
variable "ssl_policy" {
  description = "Name of the SSL Policy for the listener. Required if protocol is HTTPS or TLS."
  type        = string
  default     = "ELBSecurityPolicy-TLS13-1-2-2021-06"
}
variable "default_action" {
  description = "Configuration block for default actions."
  type        = any
  default     = {}
}
variable "target_group_arns" {
  description = "Target Group ARNs to look up keys"
  type        = any
  default     = {}
}
variable "tags" {
  description = "A map of tags to add to all resources"
  type        = map(string)
  default     = {}
}
