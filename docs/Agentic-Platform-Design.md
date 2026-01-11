# Agentic Platform Design

**C# + Semantic Kernel + Local LLM (Ollama)**

---

## 1. Purpose

This document describes the architecture and design of a local-first, agentic AI platform built using:

- C#
- Microsoft Semantic Kernel
- Locally hosted LLMs (via Ollama)
- Declarative agent configuration
- Explicit orchestration and state management

The goal is to create a **safe, auditable, extensible multi-agent system** suitable for platform and infrastructure engineering use cases.

---

## 2. Core Design Principles

1. **LLMs are stateless reasoners**
2. **State lives outside the LLM**
3. **Agents have single, well-defined responsibilities**
4. **The orchestrator owns control flow**
5. **All memory is explicit and scoped**
6. **Agents produce artifacts, not side effects**
7. **Execution requires approval**

---

## 3. High-Level Architecture

```text
User / API / CLI
        |
        v
+----------------------+
| Orchestrator (C#)    |  <-- Control plane
| - Intent parsing     |
| - Planning           |
| - Agent routing      |
| - Policy enforcement |
| - State management   |
+----------+-----------+
           |
           v
+----------------------+
| Agent Runtime (SK)   |
| - Infra Agent        |
| - Security Agent     |
| - Docs Agent         |
+----------+-----------+
           |
           v
+----------------------+
| Tool / MCP Layer     |
| - Terraform tools    |
| - Validators         |
| - Execution tools    |
+----------+-----------+
           |
           v
+----------------------+
| State / Memory       |
| / Knowledge Store    |
+----------------------+
```

---

## 4. Component Responsibilities

### 4.1 Orchestrator (Non-LLM)

The orchestrator is a **deterministic C# service**, not an agent.

Responsibilities:
- Interpret user intent
- Create and manage execution plans
- Select appropriate agents
- Enforce permissions and policies
- Persist state and history
- Request and validate human approvals

Think of the orchestrator as:
> A Kubernetes control plane for AI agents

---

### 4.2 Agents (LLM-backed)

Each agent:
- Is backed by a dedicated LLM model
- Has a single responsibility
- Cannot execute tools unless explicitly allowed
- Has no direct access to storage
- Produces artifacts only (plans, docs, reviews)

Examples:
- Infra Agent → infrastructure plans
- Security Agent → risk analysis
- Docs Agent → documentation

Agents are **replaceable** and **stateless**.

---

### 4.3 Tool / MCP Layer

Tools are exposed via explicit APIs (local or remote).

Characteristics:
- Language-agnostic
- Permission-gated
- Observable
- Testable independently of agents

Examples:
- Terraform plan executor
- Policy validator
- Markdown generator

Agents request tool usage; orchestrator decides.

---

## 5. State, Data, and Knowledge Management

There are **four distinct kinds of state**. They must never be conflated.

---

### 5.1 Execution State (Short-lived)

Definition:
> The current state of an in-flight request.

Examples:
- Current goal
- Active plan step
- Intermediate agent outputs

Characteristics:
- Short-lived
- Deterministic
- Serializable
- Replayable

Example structure:

```json
{
  "requestId": "abc-123",
  "goal": "Create secure dev environment",
  "currentStep": 2,
  "agentResults": {
    "infra-agent": "...",
    "security-agent": "..."
  }
}
```

Storage (POC):
- In-memory objects

---

### 5.2 Operational Memory (History)

Definition:
> A durable record of what has happened.

Examples:
- Previous runs
- Decisions made
- Approvals granted
- Errors encountered

Characteristics:
- Append-only
- Auditable
- Queryable
- Human-readable

Conceptually:
> Event sourcing for agent systems

Storage (POC):
- JSON files
- Later: database (e.g., Cosmos DB, Postgres)

---

### 5.3 Knowledge / Reference Data

Definition:
> Curated information the system uses to reason.

Examples:
- Azure best practices
- Security standards
- Platform conventions
- Runbooks

Characteristics:
- Version-controlled
- Long-lived
- Human-curated
- Not conversational memory

Storage (POC):
- Markdown files in repo
- Git-based knowledge base

Retrieval:
- Explicit selection by orchestrator
- Optional RAG in future

---

### 5.4 Agent Memory (Scoped, Explicit)

Definition:
> Temporary context injected into agents.

Rules:
- Agents do not own memory
- Memory is scoped to a request or step
- Memory is explicitly passed in context

Good uses:
- Carrying context across steps
- Remembering preferences during a run

Bad uses:
- Hidden long-term memory
- Implicit learning across runs

---

## 6. Role of Semantic Kernel

Semantic Kernel is the **agent runtime**, not the orchestrator.

It provides:
- Prompt execution
- Function / tool calling
- Context variables
- Short-term memory
- Planning primitives

It does NOT provide:
- Long-term storage
- Agent routing
- Policy enforcement
- Human approval workflows

Those remain orchestration concerns.

---

## 7. Recommended State Strategy (POC Phase)

| State Type           | Storage Mechanism       |
|----------------------|------------------------|
| Execution state      | In-memory objects      |
| Operational memory   | JSON files             |
| Knowledge base       | Markdown in Git        |
| Agent memory         | SK context variables   |

This design allows easy evolution without redesign.

---

## 8. Agent Configuration Model

Each agent is defined declaratively:

- `Modelfile` → LLM behavior
- `agent.yaml` → Metadata, capabilities, permissions

Agents are discovered and registered at runtime.

---

## 9. Design Guarantees

This architecture ensures:

- Deterministic orchestration
- Clear separation of concerns
- Auditable behavior
- Safe execution boundaries
- Replaceable models and agents
- Future cloud migration readiness

---

## 10. Readiness Checklist (Before Coding)

Before writing code, the system design answers:

- Where does execution state live? ✔
- Where is history stored? ✔
- Where is knowledge curated? ✔
- Who decides which agent runs? ✔
- Who approves execution? ✔

---

## 11. Next Step

With this design in place, the next step is to:

- Define the C# solution structure
- Implement the Orchestrator core
- Create an Agent Registry
- Load `agent.yaml`
- Instantiate Semantic Kernel per agent

No tools. No planners. Just foundations.

---
