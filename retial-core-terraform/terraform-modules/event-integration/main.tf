locals {
  create             = length(var.event_subscriptions) > 0 && var.emb_spoke_name != ""
  qualified_name     = "${var.environment}-${var.business_unit}-${var.service_name}"
}

# SQS Queue for domain events
resource "aws_sqs_queue" "event_queue" {
  count = local.create ? 1 : 0

  name                       = "${local.qualified_name}-event-queue"
  delay_seconds              = 0
  max_message_size           = 262144
  message_retention_seconds  = var.sqs_message_retention_seconds
  receive_wait_time_seconds  = 0
  visibility_timeout_seconds = var.sqs_visibility_timeout_seconds

  tags = merge(var.tags, {
    Name        = "${local.qualified_name}-event-queue"
    Environment = var.environment
    Purpose     = "${var.service_name} domain event processing"
  })
}

# SQS Queue Policy to allow EventBridge to send messages
resource "aws_sqs_queue_policy" "event_queue_policy" {
  count = local.create ? 1 : 0

  queue_url = aws_sqs_queue.event_queue[0].url

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowEventBridgeToSendMessage"
        Effect = "Allow"
        Principal = {
          Service = "events.amazonaws.com"
        }
        Action   = "sqs:SendMessage"
        Resource = aws_sqs_queue.event_queue[0].arn
        Condition = {
          ArnEquals = {
            "aws:SourceArn" = aws_cloudwatch_event_rule.event_rule[0].arn
          }
        }
      }
    ]
  })
}

# EventBridge Rule — filter events on EMB spoke by detail-type
resource "aws_cloudwatch_event_rule" "event_rule" {
  count = local.create ? 1 : 0

  name           = "${local.qualified_name}-event-rule"
  description    = "Capture domain events for ${var.service_name} and forward to SQS"
  event_bus_name = var.emb_spoke_name
  event_pattern = jsonencode({
    detail-type = [for dt in var.event_subscriptions : { "equals-ignore-case" : dt }]
    detail = {
      metadata = {
        environment = [{ "equals-ignore-case" : var.environment }]
      }
    }
  })

  tags = merge(var.tags, {
    Name        = "${local.qualified_name}-event-rule"
    Environment = var.environment
  })
}

# EventBridge Target — route matched events to SQS
resource "aws_cloudwatch_event_target" "events_to_sqs" {
  count = local.create ? 1 : 0

  rule           = aws_cloudwatch_event_rule.event_rule[0].name
  event_bus_name = var.emb_spoke_name
  target_id      = "SendToSQS"
  arn            = aws_sqs_queue.event_queue[0].arn
}
