# Information about output variables can be found here:
# https://developer.hashicorp.com/terraform/language/values/outputs

# Outputs
output "project_information" {
  value = {
    environment   = var.environment
    business_unit = var.business_unit
    project       = var.project
    # If DynamoDB: table_name    = aws_dynamodb_table.dotnet_repo_template.name
    last_deployed = timestamp()
  }
}