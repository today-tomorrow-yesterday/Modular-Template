# -----------------------------------------------------------------------------
# Load Balancer HTTPS Listeners
resource "aws_lb_listener" "this" {
  load_balancer_arn = var.load_balancer_arn

  port            = var.port
  protocol        = var.protocol
  certificate_arn = var.certificate_arn
  ssl_policy      = var.ssl_policy

  dynamic "default_action" {
    for_each = [var.default_action]

    # Defaults to forward action if type not specified
    content {
      type             = try(default_action.value.type, "forward")
      target_group_arn = contains([null, "", "forward"], try(default_action.value.type, "")) ? try(default_action.value.target_group_key, null) != null ? lookup(var.target_group_arns, try(default_action.value.target_group_key, ""), null) : try(default_action.value.target_group_arn, null) : null

      # Forward
      dynamic "forward" {
        for_each = length(keys(try(default_action.value.forward, {}))) == 0 ? [] : [try(default_action.value.forward, {})]

        content {
          dynamic "target_group" {
            for_each = forward.value.target_groups

            content {
              arn    = try(target_group.value.target_group_key, null) != null ? lookup(var.target_group_arns, try(target_group.value.target_group_key, ""), null) : try(target_group.value.arn, null)
              weight = try(target_group.value.weight, null)
            }
          }

          dynamic "stickiness" {
            for_each = length(keys(try(forward.value.stickiness, {}))) == 0 ? [] : [try(forward.value.stickiness, {})]

            content {
              enabled  = try(stickiness.value.enabled, null)
              duration = try(stickiness.value.duration, null)
            }
          }
        }
      }

      # Redirect
      dynamic "redirect" {
        for_each = length(keys(try(default_action.value.redirect, {}))) == 0 ? [] : [try(default_action.value.redirect, {})]

        content {
          path        = try(redirect.value.path, null)
          host        = try(redirect.value.host, null)
          port        = try(redirect.value.port, null)
          protocol    = try(redirect.value.protocol, null)
          query       = try(redirect.value.query, null)
          status_code = try(redirect.value.status_code, null)
        }
      }

      # Fixed Respones
      dynamic "fixed_response" {
        for_each = length(keys(try(default_action.value.fixed_response, {}))) == 0 ? [] : [try(default_action.value.fixed_response, {})]

        content {
          content_type = try(fixed_response.value.content_type, null)
          message_body = try(fixed_response.value.message_body, null)
          status_code  = try(fixed_response.value.status_code, null)
        }
      }

      # Authenticate Cognito
      dynamic "authenticate_cognito" {
        for_each = length(keys(try(default_action.value.authenticate_cognito, {}))) == 0 ? [] : [try(default_action.value.authenticate_cognito, {})]

        content {
          # Max 10 extra params
          authentication_request_extra_params = try(authenticate_cognito.value.authentication_request_extra_params, null)
          on_unauthenticated_request          = try(authenticate_cognito.value.on_authenticated_request, null)
          scope                               = try(authenticate_cognito.value.scope, null)
          session_cookie_name                 = try(authenticate_cognito.value.session_cookie_name, null)
          session_timeout                     = try(authenticate_cognito.value.session_timeout, null)
          user_pool_arn                       = try(authenticate_cognito.value.user_pool_arn, null)
          user_pool_client_id                 = try(authenticate_cognito.value.user_pool_client_id, null)
          user_pool_domain                    = try(authenticate_cognito.value.user_pool_domain, null)
        }
      }

      # Authienticate OIDC
      dynamic "authenticate_oidc" {
        for_each = length(keys(lookup(default_action.value, "authenticate_oidc", {}))) == 0 ? [] : [lookup(default_action.value, "authenticate_oidc", {})]

        content {
          # Max 10 extra params
          authentication_request_extra_params = try(authenticate_oidc.value.authentication_request_extra_params, null)
          authorization_endpoint              = try(authenticate_oidc.value.authorization_endpoint, null)
          client_id                           = try(authenticate_oidc.value.client_id, null)
          client_secret                       = try(authenticate_oidc.value.client_secret, null)
          issuer                              = try(authenticate_oidc.value.issuer, null)
          on_unauthenticated_request          = try(authenticate_oidc.value.on_unauthenticated_request, null)
          scope                               = try(authenticate_oidc.value.scope, null)
          session_cookie_name                 = try(authenticate_oidc.value.session_cookie_name, null)
          session_timeout                     = try(authenticate_oidc.value.session_timeout, null)
          token_endpoint                      = try(authenticate_oidc.value.token_endpoint, null)
          user_info_endpoint                  = try(authenticate_oidc.value.user_info_endpoint, null)
        }
      }
    }
  }

  dynamic "default_action" {
    for_each = contains(["authenticate-oidc", "authenticate-cognito"], try(var.default_action.type, null)) ? [var.default_action] : []
    content {
      type             = "forward"
      target_group_arn = try(default_action.value.target_group_arn, null)
    }
  }

  tags = var.tags
}
