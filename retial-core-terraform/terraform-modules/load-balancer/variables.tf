variable "name" {
  type        = string
  description = "The name of the LB. This name must be unique within your AWS account, can have a maximum of 32 characters, must contain only alphanumeric characters or hyphens, and must not begin or end with a hyphen. If not specified, Terraform will autogenerate a name beginning with tf-lb."
  default     = null
}
variable "name_prefix" {
  type        = string
  description = "Creates a unique name beginning with the specified prefix. Conflicts with name."
  default     = null
}
variable "internal" {
  type        = bool
  description = "If true, the LB will be internal"
  default     = false
}
variable "load_balancer_type" {
  type        = string
  description = "The type of load balancer to create. Possible values are application, gateway, or network. The default value is application."
  default     = "application"
}
variable "web_acl_arn" {
  description = "WACL Arn to use in front of LB."
  type        = string
  default     = null
}
variable "vpc_id" {
  description = "VPC to put the NLB in."
  type        = string
  default     = ""
}
variable "subnets" {
  description = "A list of subnet IDs to attach to the LB. Subnets cannot be updated for Load Balancers of type network. Changing this value for load balancers of type network will force a recreation of the resource."
  type        = list(string)
  default     = null
}
variable "subnet_mapping" {
  description = "A list of subnet mapping blocks describing subnets to attach to network load balancer"
  type        = list(map(string))
  default     = []
}
variable "security_groups" {
  description = "A list of security group IDs to assign to the LB. Only valid for Load Balancers of type application."
  type        = list(string)
  default     = null
}
variable "idle_timeout" {
  description = "The time in seconds that the connection is allowed to be idle. Only valid for Load Balancers of type application."
  type        = number
  default     = 60
}
variable "drop_invalid_header_fields" {
  description = "Indicates whether HTTP headers with header fields that are not valid are removed by the load balancer (true) or routed to targets (false). The default is false. Elastic Load Balancing requires that message header names contain only alphanumeric characters and hyphens. Only valid for Load Balancers of type application."
  type        = bool
  default     = false
}
variable "enable_deletion_protection" {
  description = "If true, deletion of the load balancer will be disabled via the AWS API. This will prevent Terraform from deleting the load balancer."
  type        = bool
  default     = false
}
variable "enable_cross_zone_load_balancing" {
  description = "If true, cross-zone load balancing of the load balancer will be enabled. This is a network load balancer feature. "
  type        = bool
  default     = false
}
variable "enable_http2" {
  description = "Indicates whether HTTP/2 is enabled in application load balancers."
  type        = bool
  default     = true
}
variable "customer_owned_ipv4_pool" {
  description = "The ID of the customer owned ipv4 pool to use for this load balancer."
  type        = bool
  default     = null
}
variable "ip_address_type" {
  description = "The type of IP addresses used by the subnets for your load balancer. The possible values are ipv4 and dualstack."
  type        = string
  default     = "ipv4"
}
variable "access_logs" {
  description = "Map containing access logging configuration for load balancer."
  type        = map(string)
  default     = {}
}
variable "tags" {
  description = "A map of tags to add to all resources"
  type        = map(string)
  default     = {}
}
variable "load_balancer_create_timeout" {
  description = "Timeout value when creating the LB."
  type        = string
  default     = "10m"
}
variable "desync_mitigation_mode" {
  description = "Determines how the load balancer handles requests that might pose a security risk to an application due to HTTP desync. Valid values are monitor, defensive (default), strictest."
  type        = string
  default     = null
}
variable "enable_waf_fail_open" {
  description = "Indicates whether to allow a WAF-enabled load balancer to route requests to targets if it is unable to forward the request to AWS WAF."
  type        = bool
  default     = null
}
variable "load_balancer_delete_timeout" {
  description = "Timeout value when deleting the LB."
  type        = string
  default     = "10m"
}
variable "load_balancer_update_timeout" {
  description = "Timeout value when updating the LB."
  type        = string
  default     = "10m"
}
