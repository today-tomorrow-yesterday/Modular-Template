locals {
  qualified_project_name                           = "${var.environment}-${var.business_unit}-${var.project_prefix}"
  product_event_handler_lambda_dotnet_project_name = "Domain.Product.EventHandler"
  lambda_cert_path                                 = "/var/task/ClaytonInternalRootCA.pem"
}

# SQS Queue for Product Events
resource "aws_sqs_queue" "product_domain_event_queue" {
  name                       = "${local.qualified_project_name}-queue"
  delay_seconds              = 0
  max_message_size           = 262144
  message_retention_seconds  = 1209600 # 14 days
  receive_wait_time_seconds  = 0
  visibility_timeout_seconds = var.lambda_timeout * 6

  tags = merge(var.tags, {
    Name        = "${local.qualified_project_name}-queue"
    Environment = var.environment
    Purpose     = "Product domain event processing"
  })
}

# SQS Queue Policy to allow EventBridge to send messages
resource "aws_sqs_queue_policy" "product_domain_event_queue_policy" {
  queue_url = aws_sqs_queue.product_domain_event_queue.url

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
        Resource = aws_sqs_queue.product_domain_event_queue.arn
        Condition = {
          ArnEquals = {
            "aws:SourceArn" = "${aws_cloudwatch_event_rule.product_domain_event_rule.arn}"
          }
        }
      }
    ]
  })
}

# SQS to Lambda
resource "aws_lambda_event_source_mapping" "sqs_to_lambda" {
  event_source_arn = aws_sqs_queue.product_domain_event_queue.arn
  function_name    = module.lambda_handler.arn
  batch_size       = var.lambda_batch_size
  enabled          = true

  scaling_config {
    maximum_concurrency = var.lambda_invocation_max_concurrency
  }

  function_response_types = ["ReportBatchItemFailures"]
}

# EventBridge Rule
resource "aws_cloudwatch_event_rule" "product_domain_event_rule" {
  name           = "${local.qualified_project_name}-rule"
  description    = "Capture product domain events and forward to SQS"
  event_bus_name = var.retail_emb_spoke_name
  event_pattern = jsonencode({
    detail-type = [
      { "equals-ignore-case" : "HBG.Quotes.Created" },
      { "equals-ignore-case" : "HBG.Quotes.Updated" },
      { "equals-ignore-case" : "HBG.Quotes.Updated.PriceChange" },
      { "equals-ignore-case" : "HBG.Products.HomeModels.Updated" },
      { "equals-ignore-case" : "HBG.Products.HomeModels.Updated.PriceChange" },
      { "equals-ignore-case" : "HBG.Products.HomeModels.Specifications.Updated.PriceChange" },
      { "equals-ignore-case" : "rtl.iSeries.autostockin.completed" },
      { "equals-ignore-case" : "rtl.iSeries.manualstockin.completed" },
      { "equals-ignore-case" : "rtl.iSeries.dealbooking.completed" },
      { "equals-ignore-case" : "rtl.iSeries.dealreversal.completed" }
    ]
    detail = {
      metadata = {
        environment = [{ "equals-ignore-case" : var.environment }]
      }
    }
  })

  tags = merge(var.tags, {
    Name        = "${local.qualified_project_name}-rule"
    Environment = var.environment
  })
}

# EventBridge Target
resource "aws_cloudwatch_event_target" "product_events_to_sqs" {
  rule           = aws_cloudwatch_event_rule.product_domain_event_rule.name
  event_bus_name = var.retail_emb_spoke_name
  target_id      = "SendToSQS"
  arn            = aws_sqs_queue.product_domain_event_queue.arn
}