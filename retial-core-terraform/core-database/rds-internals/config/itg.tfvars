aws_region          = "us-east-1"
aws_dr_region       = "us-east-2"
tag_project         = "Retail Core API Project"
tag_owner           = "datateam@claytonhomes.com"
tag_costcenter      = "rtl"
tag_compliance      = "pii"
tag_confidentiality = "restricted"
tag_recovery_tier   = "tier4"
tag_octo_project    = "local"
tag_octo_release    = "local"
tag_workload        = "Retail Core API RDS Internals"
tag_tf_state_bucket = "itg.rtl.us-east-1.terraform-state"
tag_tf_state_key    = "retail-domain-module-database/rds-internals.tfstate"
environment         = "itg"
business_unit       = "rtl"

rds_cluster_state_bucket = "itg.rtl.us-east-1.terraform-state"
rds_cluster_state_key    = "retail-domain-module-database/rds-cluster.tfstate"

# Driven by EFCore
databases = {
  "sales" = {
    "name" : "sales",
    "schemas" : ["sales", "cache", "messaging", "packages", "migrations", "public", "cdc"],
    "owner" : "AWSData-PG-Service-sales-Elevated"
  },
  "customer" = {
    "name" : "customer",
    "schemas" : ["customers", "messaging", "migrations", "public", "cdc"],
    "owner" : "AWSData-PG-Service-customer-Elevated"
  },
  "funding" = {
    "name" : "funding",
    "schemas" : ["fundings", "messaging", "cache", "migrations", "public", "cdc"],
    "owner" : "AWSData-PG-Service-funding-Elevated"
  },
  "inventory" = {
    "name" : "inventory",
    "schemas" : ["inventories", "messaging", "cache", "migrations", "public", "cdc"],
    "owner" : "AWSData-PG-Service-inventory-Elevated"
  },
  "organization" = {
    "name" : "organization",
    "schemas" : ["organizations", "messaging", "migrations", "public", "cdc"],
    "owner" : "AWSData-PG-Service-organization-Elevated"
  }
}
