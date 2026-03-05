# Information about output variables can be found here:
# https://developer.hashicorp.com/terraform/language/values/outputs

# Outputs
output "project_information" {
  value = {
    environment   = var.environment
    business_unit = var.business_unit
    project       = var.project
    last_deployed = timestamp()
  }
}