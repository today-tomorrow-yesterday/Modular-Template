# -----------------------------------------------------------------------------
# ECS Service Resource
# -----------------------------------------------------------------------------
resource "aws_ecs_service" "this" {
  name                               = var.name
  cluster                            = var.cluster
  launch_type                        = var.launch_type
  desired_count                      = var.desired_count
  wait_for_steady_state              = var.wait_for_steady_state
  task_definition                    = var.task_definition
  availability_zone_rebalancing      = var.availability_zone_rebalancing
  deployment_maximum_percent         = var.deployment_maximum_percent
  deployment_minimum_healthy_percent = var.deployment_minimum_healthy_percent
  enable_ecs_managed_tags            = var.enable_ecs_managed_tags
  enable_execute_command             = var.enable_execute_command
  force_delete                       = var.force_delete
  force_new_deployment               = var.force_new_deployment
  health_check_grace_period_seconds  = var.health_check_grace_period_seconds
  //iam_role                           = var.iam_role
  platform_version    = var.platform_version
  propagate_tags      = var.propagate_tags
  scheduling_strategy = var.scheduling_strategy
  triggers            = var.triggers

  # network_configuration
  dynamic "network_configuration" {
    for_each = try(length(keys(var.network_configuration)), 0) > 0 ? [var.network_configuration] : []

    content {
      subnets          = network_configuration.value.subnets
      security_groups  = network_configuration.value.security_groups
      assign_public_ip = network_configuration.value.assign_public_ip
    }
  }

  # load_balancer
  dynamic "load_balancer" {
    for_each = try(length(keys(var.load_balancer)), 0) > 0 ? [var.load_balancer] : []

    content {
      container_name   = load_balancer.value.container_name
      container_port   = load_balancer.value.container_port
      elb_name         = load_balancer.value.elb_name
      target_group_arn = load_balancer.value.target_group_arn
    }
  }

  # alarms
  dynamic "alarms" {
    for_each = try(length(keys(var.alarms)), 0) > 0 ? [var.alarms] : []

    content {
      alarm_names = alarms.value.alarm_names
      enable      = alarms.value.enable
      rollback    = alarms.value.rollback
    }
  }

  # capacity_provider_strategy
  dynamic "capacity_provider_strategy" {
    for_each = try(length(keys(var.capacity_provider_strategy)), 0) > 0 ? var.capacity_provider_strategy : {}

    content {
      base              = capacity_provider_strategy.value.base
      capacity_provider = capacity_provider_strategy.value.capacity_provider
      weight            = capacity_provider_strategy.value.weight
    }
  }

  # deployment_circuit_breaker
  dynamic "deployment_circuit_breaker" {
    for_each = try(length(keys(var.deployment_circuit_breaker)), 0) > 0 ? [var.deployment_circuit_breaker] : []

    content {
      enable   = deployment_circuit_breaker.value.enable
      rollback = deployment_circuit_breaker.value.rollback
    }
  }

  # deployment_controller
  dynamic "deployment_controller" {
    for_each = try(length(keys(var.deployment_controller)), 0) > 0 ? [var.deployment_controller] : []

    content {
      type = deployment_controller.value.type
    }
  }

  # ordered_placement_strategy
  dynamic "ordered_placement_strategy" {
    for_each = try(length(keys(var.ordered_placement_strategy)), 0) > 0 ? var.ordered_placement_strategy : {}

    content {
      field = ordered_placement_strategy.value.field
      type  = ordered_placement_strategy.value.type
    }
  }

  # placement_constraints
  dynamic "placement_constraints" {
    for_each = try(length(keys(var.placement_constraints)), 0) > 0 ? var.placement_constraints : {}

    content {
      expression = placement_constraints.value.expression
      type       = placement_constraints.value.type
    }
  }

  # service_connect_configuration
  dynamic "service_connect_configuration" {
    for_each = try(length(keys(var.service_connect_configuration)), 0) > 0 ? [var.service_connect_configuration] : []

    content {
      enabled = service_connect_configuration.value.enabled

      dynamic "log_configuration" {
        for_each = service_connect_configuration.value.log_configuration != null ? [service_connect_configuration.value.log_configuration] : []

        content {
          log_driver = log_configuration.value.log_driver
          options    = log_configuration.value.options

          dynamic "secret_option" {
            for_each = log_configuration.value.secret_option != null ? log_configuration.value.secret_option : []

            content {
              name       = secret_option.value.name
              value_from = secret_option.value.value_from
            }
          }
        }
      }

      namespace = service_connect_configuration.value.namespace

      dynamic "service" {
        for_each = service_connect_configuration.value.service != null ? service_connect_configuration.value.service : []

        content {
          dynamic "client_alias" {
            for_each = service.value.client_alias != null ? [service.value.client_alias] : []

            content {
              dns_name = client_alias.value.dns_name
              port     = client_alias.value.port
            }
          }

          discovery_name        = service.value.discovery_name
          ingress_port_override = service.value.ingress_port_override
          port_name             = service.value.port_name

          dynamic "timeout" {
            for_each = service.value.timeout != null ? [service.value.timeout] : []

            content {
              idle_timeout_seconds        = timeout.value.idle_timeout_seconds
              per_request_timeout_seconds = timeout.value.per_request_timeout_seconds
            }
          }

          dynamic "tls" {
            for_each = service.value.tls != null ? [service.value.tls] : []

            content {
              dynamic "issuer_cert_authority" {
                for_each = tls.value.issuer_cert_authority

                content {
                  aws_pca_authority_arn = issuer_cert_authority.value.aws_pca_authority_arn
                }
              }

              kms_key  = tls.value.kms_key
              role_arn = tls.value.role_arn
            }
          }
        }
      }
    }
  }

  # service_registries
  dynamic "service_registries" {
    for_each = try(length(keys(var.service_registries)), 0) > 0 ? [var.service_registries] : []

    content {
      container_name = service_registries.value.container_name
      container_port = service_registries.value.container_port
      port           = service_registries.value.port
      registry_arn   = service_registries.value.registry_arn
    }
  }

  # volume_configuration
  dynamic "volume_configuration" {
    for_each = try(length(keys(var.volume_configurations)), 0) > 0 ? var.volume_configurations : {}

    content {
      name = try(volume_configuration.value.name, volume_configuration.key)

      dynamic "managed_ebs_volume" {
        for_each = [volume_configuration.value.managed_ebs_volume]

        content {
          encrypted        = managed_ebs_volume.value.encrypted
          file_system_type = managed_ebs_volume.value.file_system_type
          iops             = managed_ebs_volume.value.iops
          kms_key_id       = managed_ebs_volume.value.kms_key_id
          role_arn         = managed_ebs_volume.value.role_arn
          size_in_gb       = managed_ebs_volume.value.size_in_gb
          snapshot_id      = managed_ebs_volume.value.snapshot_id

          dynamic "tag_specifications" {
            for_each = managed_ebs_volume.value.tag_specifications != null ? managed_ebs_volume.value.tag_specifications : []

            content {
              resource_type  = tag_specifications.value.resource_type
              propagate_tags = tag_specifications.value.propagate_tags
              tags           = tag_specifications.value.tags
            }
          }

          throughput  = managed_ebs_volume.value.throughput
          volume_type = managed_ebs_volume.value.volume_type
        }
      }
    }
  }

  # vpc_lattice_configurations
  dynamic "vpc_lattice_configurations" {
    for_each = try(length(keys(var.vpc_lattice_configurations)), 0) > 0 ? [var.vpc_lattice_configurations] : []

    content {
      role_arn         = vpc_lattice_configurations.value.role_arn
      target_group_arn = vpc_lattice_configurations.value.target_group_arn
      port_name        = vpc_lattice_configurations.value.port_name
    }
  }

  # timeouts
  dynamic "timeouts" {
    for_each = try(length(keys(var.timeouts)), 0) > 0 ? [var.timeouts] : []

    content {
      create = timeouts.value.create
      update = timeouts.value.update
      delete = timeouts.value.delete
    }
  }

  lifecycle {
    ignore_changes = [
      desired_count, # Always ignored
    ]
  }

  tags = merge(var.tags, { name = var.name })
}
