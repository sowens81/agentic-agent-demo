CREATE TABLE IF NOT EXISTS executions_tbl (
    Id UUID PRIMARY KEY,
    Goal TEXT NOT NULL,
    CurrentStepIndex INT NOT NULL,
    StartedAt TIMESTAMPTZ NOT NULL,
    UpdatedAt TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS execution_steps_tbl (
    ExecutionId UUID NOT NULL,
    StepIndex INT NOT NULL,
    Capability TEXT NOT NULL,
    AgentName TEXT NOT NULL,
    InputData TEXT NOT NULL,
    OutputData TEXT NOT NULL,
    ExecutedAt TIMESTAMPTZ NOT NULL,
    PRIMARY KEY (ExecutionId, StepIndex),
    FOREIGN KEY (ExecutionId) REFERENCES executions_tbl(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS approvals_tbl (
    ExecutionId UUID NOT NULL,
    StepName     TEXT NOT NULL,
    Summary TEXT NOT NULL,
    Approved BOOLEAN NULL,
    ApprovedBy TEXT NULL,
    ApprovedAt TIMESTAMPTZ NULL,
    PRIMARY KEY (ExecutionId, StepName),
    FOREIGN KEY (ExecutionId) REFERENCES executions_tbl(Id) ON DELETE CASCADE
);
