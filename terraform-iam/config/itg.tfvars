environment                 = "itg"
enterprise_message_bus_name = "nonprod-rtl-emb-spoke"

tags = {
  environment     = "itg"
  businessunit    = "rtl"
  costcenter      = "rtl"
  department      = "Retail"
  owner           = "RetailMissionControl@ClaytonHomes.com"
  name            = "itg-rtl-core-api"
  project         = "Retail Core API"
  compliance      = "PII"
  confidentiality = "Internal"
  recoverytier    = "Tier4"
  iac-repo        = "TBD"
  iac-state       = "itg.rtl.us-east-1.terraform-state/global/iam/core-api/iam.tfstate"
  workload        = "core service"
}
