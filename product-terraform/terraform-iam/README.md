<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.0 |
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | ~> 5.53.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_aws"></a> [aws](#provider\_aws) | 5.53.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [aws_iam_role.dotnet_repo_template_role](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role) | resource |
| [aws_caller_identity.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/caller_identity) | data source |
| [aws_default_tags.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/default_tags) | data source |
| [aws_iam_policy_document.dotnet_repo_template_role_permissions_policy](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |
| [aws_iam_policy_document.dotnet_repo_template_role_trust_policy](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_aws_region"></a> [aws\_region](#input\_aws\_region) | AWS region the workload is being deployed to | `string` | `"us-east-1"` | no |
| <a name="input_business_unit"></a> [business\_unit](#input\_business\_unit) | Business unit the workload belongs to | `string` | `"rtl"` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | Environment the workload is being deployed to | `string` | `null` | no |
| <a name="input_project"></a> [project](#input\_project) | The project name to use when naming AWS resources, with words separated by dashes (e.g. clayton-built) | `string` | `""` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Tags that should be applied to a resource | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_role_information"></a> [role\_information](#output\_role\_information) | n/a |
<!-- END_TF_DOCS -->