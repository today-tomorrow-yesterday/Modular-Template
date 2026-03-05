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
| <a name="module_lambda_handler"></a> [lambda\_handler](#module\_lambda\_handler) | terraform.clayton.net/gh_claytonhomes/aws_lambda/aws | 0.1.0 |

## Resources

| Name | Type |
|------|------|
| [aws_cloudwatch_log_group.lambda_lambda_log_group](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/cloudwatch_log_group) | resource |
| [aws_caller_identity.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/caller_identity) | data source |
| [aws_default_tags.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/default_tags) | data source |
| [aws_region.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/region) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_aws_region"></a> [aws\_region](#input\_aws\_region) | AWS region workload is being deployed to | `string` | `"us-east-1"` | no |
| <a name="input_business_unit"></a> [business\_unit](#input\_business\_unit) | Business unit workload belongs to | `string` | `null` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | Environment workload is being deployed to | `string` | `null` | no |
| <a name="input_is_staging"></a> [is\_staging](#input\_is\_staging) | Indicates a staging environment (Prod only) | `bool` | `false` | no |
| <a name="input_lambda_handler_deploy_package_to_s3"></a> [lambda\_handler\_deploy\_package\_to\_s3](#input\_lambda\_handler\_deploy\_package\_to\_s3) | Boolean indicating whether to deploy to package to S3. | `string` | `false` | no |
| <a name="input_lambda_handler_description"></a> [lambda\_handler\_description](#input\_lambda\_handler\_description) | Lambda function description | `string` | `""` | no |
| <a name="input_lambda_handler_function_name"></a> [lambda\_handler\_function\_name](#input\_lambda\_handler\_function\_name) | Name of the Lambda (excluding env and bu) | `string` | `""` | no |
| <a name="input_lambda_handler_handler"></a> [lambda\_handler\_handler](#input\_lambda\_handler\_handler) | Function entrypoint in code. | `string` | `""` | no |
| <a name="input_lambda_handler_include_in_vpc"></a> [lambda\_handler\_include\_in\_vpc](#input\_lambda\_handler\_include\_in\_vpc) | Whether to put the Lambda into the VPC | `bool` | `false` | no |
| <a name="input_lambda_handler_log_retention_days"></a> [lambda\_handler\_log\_retention\_days](#input\_lambda\_handler\_log\_retention\_days) | Days to retain CloudWatch logs for the lambda function | `number` | `14` | no |
| <a name="input_lambda_handler_memory_size"></a> [lambda\_handler\_memory\_size](#input\_lambda\_handler\_memory\_size) | Amount of memory in MB the Lambda Function can use at runtime | `number` | `256` | no |
| <a name="input_lambda_handler_package_name"></a> [lambda\_handler\_package\_name](#input\_lambda\_handler\_package\_name) | Name of the Lambda package in S3 | `string` | `""` | no |
| <a name="input_lambda_handler_publish"></a> [lambda\_handler\_publish](#input\_lambda\_handler\_publish) | Whether to publish creation/change as new Lambda Function Version | `bool` | `false` | no |
| <a name="input_lambda_handler_runtime"></a> [lambda\_handler\_runtime](#input\_lambda\_handler\_runtime) | Identifier of the Lambda function runtime. See https://docs.aws.amazon.com/lambda/latest/dg/API_CreateFunction.html#SSS-CreateFunction-request-Runtime for valid values. | `string` | `""` | no |
| <a name="input_lambda_handler_timeout"></a> [lambda\_handler\_timeout](#input\_lambda\_handler\_timeout) | Amount of time the Lambda Function has to run in seconds | `number` | `3` | no |
| <a name="input_project"></a> [project](#input\_project) | The project to use for naming AWS resources, with words separated by dashes (e.g. clayton-built) | `string` | `""` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | Resource tags | `map(string)` | `null` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_project_information"></a> [project\_information](#output\_project\_information) | Outputs |
<!-- END_TF_DOCS -->