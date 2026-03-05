# Tags and General
aws_region          = "us-east-1"
aws_dr_region       = "us-east-2"
tag_tf_state_bucket = "itg.rtl.us-east-1.terraform-state"
tag_tf_state_key    = "retail-domain-module-database/rds-cluster.tfstate"
tag_project         = "Retail Core API Project"
tag_owner           = "RetailEbizDevs@Clayton.net"
tag_costcenter      = "rtl"
tag_compliance      = "pii"
tag_confidentiality = "restricted"
tag_recovery_tier   = "tier4"
tag_octo_project    = "local"
tag_octo_release    = "local"
tag_workload        = "Retail Core API RDS Cluster"
tag_service         = "service-tag"
env                 = "itg"
environment         = "itg"
business_unit       = "rtl"

# Additional Vars
apply_immediately               = false
enabled_cloudwatch_logs_exports = ["postgresql"]
engine_version                  = "17.5"
instance_class                  = "db.t3.medium"
master_username                 = "root"
performance_insights_enabled    = false
preferred_backup_window         = "04:30-05:00"
preferred_maintenance_window    = "wed:02:00-wed:04:00"
replica_count                   = 1
replica_scale_enabled           = true
replica_scale_max               = 4
replica_scale_min               = 1
replica_scale_cpu               = 60
replica_scale_in_cooldown       = 300
replica_scale_out_cooldown      = 300
snapshot_identifier             = ""
suffix                          = ""
vpc_security_group_ids          = []
/*
serverless_v2_settings = {
  "settings" = {
    "max_capacity" : "4"
    "min_capacity" : "2"
  }
}
*/
