output "load_balancer" {
  description = "The properties of the load balancer."
  value = {
    id         = aws_lb.this.id
    arn        = aws_lb.this.arn
    dns_name   = aws_lb.this.dns_name
    arn_suffix = aws_lb.this.arn_suffix
    zone_id    = aws_lb.this.zone_id
  }
}
