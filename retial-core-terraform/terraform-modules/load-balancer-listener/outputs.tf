output "arn" {
  description = "The ARN of the load balancer listener created."
  value       = aws_lb_listener.this.arn
}
output "id" {
  description = "The ID of the load balancer listener created."
  value       = aws_lb_listener.this.id
}
output "port" {
  description = "The port of the load balancer listener created."
  value       = aws_lb_listener.this.id
}
