output "sqs_queue_arn" {
  description = "ARN of the SQS event queue"
  value       = try(aws_sqs_queue.event_queue[0].arn, null)
}

output "sqs_queue_url" {
  description = "URL of the SQS event queue"
  value       = try(aws_sqs_queue.event_queue[0].url, null)
}

output "sqs_queue_name" {
  description = "Name of the SQS event queue"
  value       = try(aws_sqs_queue.event_queue[0].name, null)
}

output "sqs_dlq_arn" {
  description = "ARN of the SQS dead letter queue"
  value       = try(aws_sqs_queue.event_dlq[0].arn, null)
}

output "sqs_dlq_url" {
  description = "URL of the SQS dead letter queue"
  value       = try(aws_sqs_queue.event_dlq[0].url, null)
}

output "sqs_dlq_name" {
  description = "Name of the SQS dead letter queue"
  value       = try(aws_sqs_queue.event_dlq[0].name, null)
}

output "event_rule_arn" {
  description = "ARN of the EventBridge rule"
  value       = try(aws_cloudwatch_event_rule.event_rule[0].arn, null)
}
