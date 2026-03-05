# https://docs.aws.amazon.com/AmazonECS/latest/APIReference/API_ContainerDefinition.html

# -----------------------------------------------------------------------------
# ECS Container Definition
# -----------------------------------------------------------------------------
locals {
  definition = {
    name                   = var.name
    command                = var.command
    cpu                    = var.cpu
    essential              = var.essential
    dependsOn              = var.dependsOn
    disableNetworking      = var.disableNetworking
    dnsSearchDomains       = var.dnsSearchDomains
    dnsServers             = var.dnsServers
    dockerLabels           = var.dockerLabels
    dockerSecurityOptions  = var.dockerSecurityOptions
    entrypoint             = var.entrypoint
    environment            = var.environment
    environmentFiles       = var.environmentFiles
    extraHosts             = var.extraHosts
    firelensConfiguration  = var.firelensConfiguration
    healthCheck            = var.healthCheck
    hostname               = var.hostname
    image                  = var.image
    interactive            = var.interactive
    links                  = var.links
    linuxParameters        = var.linuxParameters
    logConfiguration       = var.logConfiguration
    memory                 = var.memory
    memoryReservation      = var.memoryReservation
    mountPoints            = var.mountPoints
    portMappings           = var.portMappings
    privileged             = var.privileged
    pseudoTerminal         = var.pseudoTerminal
    readonlyRootFilesystem = var.readonlyRootFilesystem
    repositoryCredentials  = var.repositoryCredentials
    resourceRequirements   = var.resourceRequirements
    restartPolicy          = var.restartPolicy
    secrets                = var.secrets
    startTimeout           = var.startTimeout
    stopTimeout            = var.stopTimeout
    systemControls         = var.systemControls
    ulimits                = var.ulimits
    user                   = var.user
    versionConsistency     = var.versionConsistency
    volumesFrom            = var.volumesFrom
    workingDirectory       = var.workingDirectory
  }

  container_definition = { for k, v in local.definition : k => v if v != null }
}
