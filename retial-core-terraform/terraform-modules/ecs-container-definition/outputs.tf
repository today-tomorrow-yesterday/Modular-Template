output "name" {
  description = "The name of the container definiton"
  value       = var.name
}

output "definition" {
  description = "Container definition"
  value       = local.container_definition
}
