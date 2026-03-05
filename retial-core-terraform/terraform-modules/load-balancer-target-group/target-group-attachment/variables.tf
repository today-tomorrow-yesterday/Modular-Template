
variable "target_group_arn" {
  description = "The ARN of the target group to register the target."
  type        = string
  default     = null
}
variable "target_id" {
  description = "The ID of the target. This is the Instance ID for an instance, or the container ID for an ECS container."
  type        = string
  default     = null
}
variable "port" {
  description = "Port on which targets receive traffic. Required when target_type is instance, ip or alb. Does not apply when target_type is lambda."
  type        = number
  default     = null
}
variable "availability_zone" {
  description = "The Availability Zone where the IP address of the target is to be registered."
  type        = string
  default     = null
}
variable "lambda_permission" {
  description = "Lambda permission information."
  type        = map(any)
  default     = {}
}
