variable "environment" {
  type    = string
  default = "dev"
}

variable "sub_environment" {
  type    = string
  default = ""
}

variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "secret_id" {
  type    = string
  default = ""
}

variable "db_contexts" {
  description = "List of EF Core DBContext class names to migrate."
  type        = list(string)
  default     = ["OnlineAppDbContext"]
}

variable "db_name" {
  description = "Name of the PostGreSQL database to connect to for migrations"
  type        = string
  default     = "vox"
}

variable "ef_project" {
  description = "The path to the csproj containing the DbContext."
  type        = string
  default     = "VMF.Digital.VOX.Infrastructure.Data.EntityFramework/VMF.Digital.VOX.Infrastructure.Data.EntityFramework.csproj"
}

variable "startup_project" {
  description = "The path to the csproj containing the StartUp.cs file."
  type        = string
  default     = "VMF.Digital.VOX.App.Api/VMF.Digital.VOX.App.Api.csproj"
}

variable "working_directory" {
  description = "The path containing project source files."
  type        = string
  default     = "../"
}
