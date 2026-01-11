# Agentic Platform Engineering (C# + Semantic Kernel + Ollama)

A **local-first, auditable, agentic AI orchestration platform** built in **C#** using **Microsoft Semantic Kernel** and **locally hosted LLMs (Ollama)**.

This project demonstrates how to build **safe, deterministic, multi-agent systems** where:
- LLMs reason but do **not** control execution
- All state is explicit and persisted
- Human approval is a first-class concept
- Agents are declarative, replaceable, and capability-driven

> Think of this as a **Kubernetes-style control plane for AI agents**.

---

## ✨ Key Features

- 🧠 **Multi-Agent Architecture**
  - Declarative agent definitions (`agent.yaml`)
  - Capability-based agent routing
  - Dedicated LLM model per agent

- 🧭 **Deterministic Orchestration**
  - Non-LLM orchestrator (C#)
  - Explicit workflows and execution steps
  - No hidden agent-to-agent communication

- 🛑 **Human-in-the-Loop by Design**
  - Approval steps are explicit and resumable
  - Execution pauses safely until approval is granted or rejected

- 📜 **Full Auditability**
  - Execution state, steps, and approvals persisted to PostgreSQL
  - Replayable, inspectable runs
  - Clear separation of execution vs history

- 🔒 **Local-First & Secure**
  - Runs entirely on local LLMs via Ollama
  - No cloud dependencies required
  - Ready for future cloud migration

---

## 🏗 Architecture Overview

At a high level:

```
User / CLI / API
        |
        v
+----------------------+
| Orchestrator (C#)    |  <-- Control Plane
| - Planning           |
| - Agent routing      |
| - Policy enforcement |
| - Approvals          |
| - State persistence  |
+----------+-----------+
           |
           v
+----------------------+
| Agent Runtime (SK)   |
| - Infra Agent        |
| - Security Agent     |
| - Docs Agent         |
| - Planner Agent      |
+----------+-----------+
           |
           v
+----------------------+
| Local LLMs (Ollama)  |
+----------------------+

+----------------------+
| PostgreSQL           |
| - Executions         |
| - Steps              |
| - Approvals          |
+----------------------+
```

LLMs are treated as **stateless reasoning engines**.  
All control flow, memory, and execution lives outside the model.

---

## 📂 Repository Structure

```
.
├── agents/                     # Declarative agent definitions
│   ├── infra-agent/
│   ├── security-agent/
│   ├── docs-agent/
│   └── planner-agent/
│
├── docs/                       # Architecture & design docs
│   ├── Agentic-Platform-Design.md
│   └── C4-Architecture.md
│
├── databases/
│   └── db_migrations.sql       # PostgreSQL schema
│
├── src/
│   └── orchestrator/
│       ├── Cloud.Infra.Core
│       ├── Cloud.Infra.Infrastructure
│       └── Cloud.Infra.Orchestrator
│
├── docker-compose.yml
└── README.md
```

---

## 🚀 Getting Started

### 1️⃣ Prerequisites

- .NET 8+
- Docker & Docker Compose
- PostgreSQL
- Ollama

---

### 2️⃣ Start Ollama and Load Models

```
docker-compose up
```

Verify models:

```
docker exec -it ollama ollama list
```

---

### 3️⃣ Configure Environment Variables

```
export OLLAMA_ENDPOINT=http://localhost:11434/v1
export OLLAMA_API_KEY=ollama
export POSTGRESQL_CONNECTION_STRING="Host=localhost;Port=5432;Database=agentic;Username=postgres;Password=postgres"
```

---

### 4️⃣ Run the Orchestrator

```
dotnet run --project src/orchestrator/Cloud.Infra.Orchestrator
```

The orchestrator will:
- Load agent definitions
- Apply database migrations (if present)
- Create or resume an execution
- Pause for approvals when required

---

## 🤖 Agents

Agents are defined declaratively using `agent.yaml`.

Example:

```
apiVersion: agent.platform/v1
kind: Agent
metadata:
  name: infra-agent
  description: Produces infrastructure plans
spec:
  capabilities:
    - infrastructure
  model:
    runtime: ollama
    base: llama3
  behavior:
    temperature: 0.2
```

Agents:
- Are stateless
- Cannot persist data
- Cannot execute tools directly
- Only produce artifacts (plans, reviews, docs)

---

## 🧭 Planner Agent (Agentic Behavior)

The **Planner Agent** introduces controlled autonomy.

It:
- Accepts a high-level goal
- Proposes a structured execution plan (JSON)
- Assigns capabilities and intent to each step

The orchestrator:
- Validates the plan
- Enforces policy
- Executes deterministically

> The planner proposes.  
> The orchestrator decides.

---

## 🧪 Example Use Cases

- Secure cloud infrastructure planning
- Architecture reviews
- Security assessments
- Documentation generation
- Human-approved automation pipelines

---

## 🛡 Design Principles

- LLMs do not own state
- Control flow is deterministic
- Memory is explicit and scoped
- Agents are replaceable
- Approvals are mandatory for risk
- Failure is expected and recoverable

---

## 🤝 Contributing

Contributions are welcome.

Good contribution areas:
- New agent types
- Planner improvements
- Critic / review agents
- Tool execution layer (safe & gated)
- Storage adapters
- Tests and documentation

### Contribution Guidelines

1. Preserve determinism
2. Do not move control flow into LLMs
3. Keep agents stateless
4. Make approvals explicit
5. Prefer clarity over cleverness

Open an issue before large changes.

---

## 📄 License

MIT (or update as appropriate)

---

## 🙌 Acknowledgements

Built with:
- Microsoft Semantic Kernel
- Ollama
- PostgreSQL
- Dapper

---

## ⭐ Why This Repo Exists

This project exists to demonstrate **how agentic AI should actually be built**:
- Safely
- Transparently
- With engineering discipline

If you’re interested in **agent platforms, AI orchestration, or production-grade AI systems**, this repo is for you.
