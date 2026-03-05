variable "environment" {
  description = "Environment workload is being deployed to"
  type        = string
}

variable "business_unit" {
  description = "Business unit workload belongs to"
  type        = string
  default     = "rtl"
}

variable "service_name" {
  description = "The service name used for resource naming (e.g. sales-domain-module)"
  type        = string
}

variable "emb_spoke_name" {
  description = "The EventBridge event bus (spoke) name to consume events from"
  type        = string
  default     = ""
}

variable "event_subscriptions" {
  description = "List of event detail-types to subscribe to (e.g. ['rtl.customer.partyCreated']). Resources are only created when this list is non-empty."
  type        = list(string)
  default     = []
}

variable "sqs_message_retention_seconds" {
  description = "SQS message retention period in seconds"
  type        = number
  default     = 1209600 # 14 days
}

variable "sqs_visibility_timeout_seconds" {
  description = "SQS visibility timeout in seconds"
  type        = number
  default     = 300 # 5 minutes
}

variable "sqs_max_receive_count" {
  description = "Number of times a message can be received before being sent to the DLQ"
  type        = number
  default     = 5
}

variable "sqs_dlq_message_retention_seconds" {
  description = "DLQ message retention period in seconds"
  type        = number
  default     = 1209600 # 14 days
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}
