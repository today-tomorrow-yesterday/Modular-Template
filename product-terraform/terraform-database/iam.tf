##########
# IAM Role 

data "aws_iam_role" "dotnet_repo_template" {
  name = local.dotnet_repo_template_api_role_name
}