# Ollama Container Setup

## Start and check models

### Start the model

```bash
docker-compose up
```


### verify the model exists

```bash
docker exec -it ollama ollama list
```

### Test each of the models

#### infra-agent-model**

**Test using container**

```bash
docker exec -it ollama ollama run infra-agent \
"Create a step-by-step plan for provisioning a secure Azure dev environment for a .NET API. Do not execute anything."
```

**Test using an api call**

```pwsh
$body = @{
    model = "infra-agent"
    messages = @(
        @{ role = "user"; content = "Create a step-by-step plan for a secure Azure dev environment." }
    )
    temperature = 0.2
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:11434/v1/chat/completions" `
  -Method Post -ContentType "application/json" -Body $body

$response.choices[0].message.content

```

```bash
curl -s http://localhost:11434/v1/chat/completions \
  -H "Content-Type: application/json" \
  -d '{
    "model": "infra-agent",
    "messages": [
      {
        "role": "user",
        "content": "Create a step-by-step plan for a secure Azure dev environment."
      }
    ],
    "temperature": 0.2
  }' | jq -r '.choices[0].message.content'
```

#### security-agent-model**

**Test using container**

```bash
docker exec -it ollama ollama run security-agent \
"Review a proposed Azure dev environment and identify security risks and missing controls."
```

**Test using an api call**

```pwsh
$body = @{
    model = "security-agent"
    messages = @(
        @{ role = "user"; content = "Review a proposed Azure dev environment and identify security risks." }
    )
    temperature = 0.1
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:11434/v1/chat/completions" `
  -Method Post -ContentType "application/json" -Body $body

$response.choices[0].message.content

```

```bash
curl -s http://localhost:11434/v1/chat/completions \
  -H "Content-Type: application/json" \
  -d '{
    "model": "security-agent",
    "messages": [
      {
        "role": "user",
        "content": "Review a proposed Azure dev environment and identify security risks."
      }
    ],
    "temperature": 0.1
  }' | jq -r '.choices[0].message.content'

```

#### docs-agent-model**

**Test using container**

```bash
docker exec -it ollama ollama run docs-agent \
"Generate a README.md for a secure Azure dev environment."
```

**Test using an api call**

```pwsh
$body = @{
    model = "docs-agent"
    messages = @(
        @{ role = "user"; content = "Generate a README.md for a secure Azure dev environment." }
    )
    temperature = 0.4
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:11434/v1/chat/completions" `
  -Method Post -ContentType "application/json" -Body $body

$response.choices[0].message.content

```

```bash
curl -s http://localhost:11434/v1/chat/completions \
  -H "Content-Type: application/json" \
  -d '{
    "model": "docs-agent",
    "messages": [
      {
        "role": "user",
        "content": "Generate a README.md for a secure Azure dev environment."
      }
    ],
    "temperature": 0.4
  }' | jq -r '.choices[0].message.content'

```
