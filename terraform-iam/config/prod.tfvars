environment                 = "prod"
enterprise_message_bus_name = "prod-rtl-emb-spoke"

tags = {
  environment     = "prod"
  businessunit    = "rtl"
  costcenter      = "rtl"
  department      = "Retail"
  owner           = "RetailMissionControl@ClaytonHomes.com"
  name            = "prod-rtl-core-api"
  project         = "Retail Core API"
  compliance      = "PII"
  confidentiality = "Internal"
  recoverytier    = "Tier4"
  iac-repo        = "TBD"
  iac-state       = "prod.rtl.us-east-1.terraform-state/global/iam/core-api/iam.tfstate"
  workload        = "core service"
}
