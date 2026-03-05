# Sales Domain - Event Integration via Terraform

## Overview

The Sales Domain module uses two Terraform constructs to support publishing events to EventBridge and consuming events from SQS:

1. **Task Role Policy Statements** (`task_iam_role.policy_statements`) - Grants the ECS task IAM permissions to call EventBridge and SQS APIs
2. **Event Integration Module** (`module "event_integration"`) - Provisions the SQS queue, EventBridge rule, and target that route events into the queue

The ECS application polls SQS internally using `SqsPollingJobBase` (Quartz job). No Lambda is involved.

## Architecture

```
                          PUBLISHING
  +-----------------+     events:PutEvents     +---------------------+
  | Sales ECS Task  | -----------------------> | EventBridge Bus     |
  | (Fargate)       |                          | (internal bus)      |
  +-----------------+                          +---------------------+

                          CONSUMING
  +---------------------+    EventBridge Rule     +------------------+
  | EMB Spoke           | ----------------------> | SQS Event Queue  |
  | (emb_spoke_name)    |   (filters detail-type  | (per-module)     |
  +---------------------+    + environment)        +--------+---------+
                                                            |
                                                   sqs:ReceiveMessage
                                                            |
                                                   +--------v---------+
                                                   | Sales ECS Task   |
                                                   | SqsPollingJobBase|
                                                   +------------------+
```

## How It Works

### Publishing Events

The ECS task calls `events:PutEvents` directly against an EventBridge bus. The IAM permission is granted via `task_iam_role.policy_statements` in the tfvars:

```hcl
# From config/dev.tfvars
task_iam_role = {
  create = true
  policy_statements = [
    {
      sid    = "AllowEventBridgePublish"
      effect = "Allow"
      actions = [
        "events:PutEvents"
      ]
      resources = ["*"]
    },
    ...
  ]
}
```

These statements flow through the domain-module's `policy.tf` dynamic block and get attached to the ECS Task Role (`AR-{env}-rtl-sales-domain-module-task`). The task role has an ECS trust policy (not Lambda), so the running container can assume it.

### Consuming Events

Consumption is handled by the `event-integration` module, which creates:

| Resource | Name Pattern | Purpose |
|----------|-------------|---------|
| SQS Queue | `{env}-rtl-sales-domain-module-event-queue` | Receives matched events |
| SQS Queue Policy | - | Allows EventBridge to `sqs:SendMessage` |
| EventBridge Rule | `{env}-rtl-sales-domain-module-event-rule` | Filters events by `detail-type` and `metadata.environment` |
| EventBridge Target | `SendToSQS` | Routes matched events to the SQS queue |

The rule listens on the EMB spoke bus and uses **case-insensitive matching** on both the event `detail-type` and the `metadata.environment` field:

```json
{
  "detail-type": [
    { "equals-ignore-case": "rtl.Sales.OrderCreated" }
  ],
  "detail": {
    "metadata": {
      "environment": [{ "equals-ignore-case": "dev" }]
    }
  }
}
```

The ECS task also needs SQS permissions to poll the queue. These are granted via the same `task_iam_role.policy_statements`:

```hcl
{
  sid    = "AllowSQSAccess"
  effect = "Allow"
  actions = [
    "sqs:ReceiveMessage",
    "sqs:DeleteMessage",
    "sqs:GetQueueAttributes",
    "sqs:GetQueueUrl",
    "sqs:ChangeMessageVisibility"
  ]
  resources = ["*"]
}
```

### Conditional Creation

The event-integration module only creates resources when **both** conditions are met:

```hcl
local.create = length(var.event_subscriptions) > 0 && var.emb_spoke_name != ""
```

With `event_subscriptions = []`, no SQS queue or EventBridge rule is created. This means the Sales module currently has IAM permissions ready but no consumer infrastructure until subscriptions are added.

## Environment Configuration

| Environment | EMB Spoke Name | Publish Bus |
|-------------|---------------|-------------|
| dev | `nonprod-rtl-emb-spoke` | `dev-rtl-internal-event-bridge` |
| itg | `nonprod-rtl-emb-spoke` | `nonprod-rtl-emb-spoke` |
| qua | `qua-rtl-emb-spoke` | `qua-rtl-emb-spoke` |
| prod | `prod-rtl-emb-spoke` | `prod-rtl-emb-spoke` |

## File Layout

```
sales-module/sales-domain/
  main.tf              # Calls domain-module + event-integration
  variables.tf         # Declares emb_spoke_name, event_subscriptions, task_iam_role w/ policy_statements
  config/
    dev.tfvars         # emb_spoke_name = "nonprod-rtl-emb-spoke", event_subscriptions = []
    itg.tfvars         # emb_spoke_name = "nonprod-rtl-emb-spoke", event_subscriptions = []
    qua.tfvars         # emb_spoke_name = "qua-rtl-emb-spoke",    event_subscriptions = []
    prod.tfvars        # emb_spoke_name = "prod-rtl-emb-spoke",   event_subscriptions = []
```

Shared modules referenced:

```
terraform-modules/
  domain-module/       # ECS task definition, IAM roles (task role now has dynamic policy_statements)
  event-integration/   # SQS queue + EventBridge rule/target (conditionally created)
```

## How to Enable Event Consumption

To subscribe the Sales module to specific events, add detail-types to `event_subscriptions` in the tfvars:

```hcl
# config/dev.tfvars
emb_spoke_name      = "nonprod-rtl-emb-spoke"
event_subscriptions = [
  "rtl.Inventory.StockUpdated",
  "rtl.Funding.FundingApproved"
]
```

On the next `terraform apply`, this will create:
- `dev-rtl-sales-domain-module-event-queue` (SQS)
- `dev-rtl-sales-domain-module-event-rule` (EventBridge rule filtering those two detail-types)
- EventBridge target routing matches into the queue

The application's `SqsPollingJobBase` Quartz job then picks up messages from the queue URL, which is available as a Terraform output (`module.event_integration.sqs_queue_url`).

## Optionality

The Sales module can independently operate in any of these modes:

| Mode | `task_iam_role.policy_statements` | `event_subscriptions` |
|------|----------------------------------|----------------------|
| **Publish only** | Include `events:PutEvents`, omit SQS | `[]` |
| **Consume only** | Include SQS actions, omit `events:PutEvents` | `["rtl.Some.Event"]` |
| **Both** | Include both | `["rtl.Some.Event"]` |
| **Neither** | Remove event statements | `[]` |

Currently configured for: **Both (ready)** - IAM permissions in place, consumer infrastructure activates when subscriptions are added.
