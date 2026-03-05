output "arn" {
  description = "The ARN of the ECS Task Definition."
  value       = aws_ecs_task_definition.this.arn
}
