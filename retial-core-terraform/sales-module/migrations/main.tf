data "aws_secretsmanager_secret_version" "elevated" {
  secret_id = var.secret_id
}

locals {
  username = jsondecode(data.aws_secretsmanager_secret_version.elevated.secret_string)["username"]
  host     = sensitive(jsondecode(data.aws_secretsmanager_secret_version.elevated.secret_string)["host"])
  password = sensitive(jsondecode(data.aws_secretsmanager_secret_version.elevated.secret_string)["password"])
  port     = jsondecode(data.aws_secretsmanager_secret_version.elevated.secret_string)["port"]
}

resource "null_resource" "apply_migrations" {
  for_each   = toset(var.db_contexts)
  depends_on = [data.aws_secretsmanager_secret_version.elevated]

  provisioner "local-exec" {
    working_dir = "${path.module}/../../../${var.working_directory}"
    command     = <<EOT
        dotnet ef database update \
          --context ${each.value} \
          --project ${var.ef_project} \
          --startup-project ${var.startup_project}
    EOT

    environment = {
      ConnectionString = "Host=${local.host};Port=${local.port};Username=${local.username};Password=\"${local.password}\";Database=${var.db_name};SSLMode=Require;TrustServerCertificate=true;"
    }
  }

  triggers = {
    always_run = "${timestamp()}"
  }
}
