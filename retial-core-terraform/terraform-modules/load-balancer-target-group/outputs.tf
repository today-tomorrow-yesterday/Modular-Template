output "arn" {
  description = "ARN of the target group. Useful for passing to your Auto Scaling group."
  value       = aws_lb_target_group.this.arn
}

output "arn_suffix" {
  description = "ARN suffix of our target group - can be used with CloudWatch."
  value       = aws_lb_target_group.this.arn_suffix
}

output "name" {
  description = "Name of the target group. Useful for passing to your CodeDeploy Deployment Group."
  value       = aws_lb_target_group.this.name
}
