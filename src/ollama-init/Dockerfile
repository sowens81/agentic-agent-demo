FROM ollama/ollama:latest

WORKDIR /agents

COPY agents /agents

# Point the ollama CLI to the running ollama service
ENV OLLAMA_HOST=http://ollama:11434

ENTRYPOINT ["sh", "-c", "\
  echo 'Waiting for Ollama server...' && \
  until ollama list >/dev/null 2>&1; do sleep 2; done && \
  echo 'Ollama is ready.' && \
  for agent in /agents/*; do \
    name=$(basename $agent); \
    echo \"Building agent: $name\"; \
    ollama pull llama3.1:8b && \
    ollama create $name -f $agent/Modelfile; \
  done && \
  echo 'All agents built.' \
"]
