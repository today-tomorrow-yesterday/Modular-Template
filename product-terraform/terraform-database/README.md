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

| Name | Source | Version |
|------|--------|---------|
| <a name="module_aws_data"></a> [aws\_data](#module\_aws\_data) | terraform.clayton.net/gh_claytonhomes/aws_data/aws | 1.3.0 |

## Resources

| Name | Type |
|------|------|
| [aws_dynamodb_resource_policy.dotnet_repo_template](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/dynamodb_resource_policy) | resource |
| [aws_caller_identity.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/caller_identity) | data source |
| [aws_default_tags.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/default_tags) | data source |
| [aws_iam_policy_document.dynamodb](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |
| [aws_iam_role.dotnet_repo_template](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_role) | data source |
| [aws_region.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/region) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_authorizer_cached_response_seconds"></a> [authorizer\_cached\_response\_seconds](#input\_authorizer\_cached\_response\_seconds) | Seconds to cache authorization response | `number` | `300` | no |
| <a name="input_authorizer_deploy_package_to_s3"></a> [authorizer\_deploy\_package\_to\_s3](#input\_authorizer\_deploy\_package\_to\_s3) | Boolean indicating whether to deploy to package to S3. | `string` | `false` | no |
| <a name="input_authorizer_description"></a> [authorizer\_description](#input\_authorizer\_description) | Authorizer description | `string` | `""` | no |
| <a name="input_authorizer_function_name"></a> [authorizer\_function\_name](#input\_authorizer\_function\_name) | Name of the Authorizer (excluding env and bu) | `string` | `""` | no |
| <a name="input_authorizer_handler"></a> [authorizer\_handler](#input\_authorizer\_handler) | Function entrypoint in code. | `string` | `""` | no |
| <a name="input_authorizer_include_in_vpc"></a> [authorizer\_include\_in\_vpc](#input\_authorizer\_include\_in\_vpc) | Whether to put the Authorizer Lambda into the VPC | `bool` | `false` | no |
| <a name="input_authorizer_log_retention_days"></a> [authorizer\_log\_retention\_days](#input\_authorizer\_log\_retention\_days) | Days to retain CloudWatch logs for the authorizer | `number` | `14` | no |
| <a name="input_authorizer_memory_size"></a> [authorizer\_memory\_size](#input\_authorizer\_memory\_size) | Amount of memory in MB the Authorizer Lambda Function can use at runtime | `number` | `128` | no |
| <a name="input_authorizer_package_name"></a> [authorizer\_package\_name](#input\_authorizer\_package\_name) | Name of the Authorizer Lambda package in S3 | `string` | `""` | no |
| <a name="input_authorizer_publish"></a> [authorizer\_publish](#input\_authorizer\_publish) | Whether to publish creation/change as new Lambda Function Version | `bool` | `false` | no |
| <a name="input_authorizer_runtime"></a> [authorizer\_runtime](#input\_authorizer\_runtime) | Identifier of the Authorizer's Lambda function runtime. See https://docs.aws.amazon.com/lambda/latest/dg/API_CreateFunction.html#SSS-CreateFunction-request-Runtime for valid values. | `string` | `""` | no |
| <a name="input_authorizer_timeout"></a> [authorizer\_timeout](#input\_authorizer\_timeout) | Amount of time the Authorizer Lambda Function has to run in seconds | `number` | `3` | no |
| <a name="input_aws_region"></a> [aws\_region](#input\_aws\_region) | AWS region workload is being deployed to | `string` | `"us-east-1"` | no |
| <a name="input_business_unit"></a> [business\_unit](#input\_business\_unit) | Business unit workload belongs to | `string` | `null` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | Environment workload is being deployed to | `string` | `null` | no |
| <a name="input_is_staging"></a> [is\_staging](#input\_is\_staging) | Indicates a staging environment (Prod only) | `bool` | `false` | no |
| <a name="input_log_retention_days"></a> [log\_retention\_days](#input\_log\_retention\_days) | Days to retain CloudWatch logs | `number` | `14` | no |
| <a name="input_project"></a> [project](#input\_project) | The project to use for naming AWS resources, with words separated by dashes (e.g. clayton-built) | `string` | `""` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Resource tags | `map(string)` | `null` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_project_information"></a> [project\_information](#output\_project\_information) | Outputs |
<!-- END_TF_DOCS -->