# Information about output variables can be found here:
# https://developer.hashicorp.com/terraform/language/values/outputs

#########
# Outputs

output "role_information" {
  value = {
    environment                 = var.environment
    business_unit               = var.business_unit
    api_iam_role_name           = local.api_iam_role_name
    event_handler_iam_role_name = local.event_handler_iam_role_name
    project                     = var.project
    last_deployed               = timestamp()
  }
}