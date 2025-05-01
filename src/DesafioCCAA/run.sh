#!/bin/bash

NETWORK_NAME="desafio-network"
BASE_DIR="/dados"

DIRECTORIES=(
  "$BASE_DIR/elasticsearch"
  "$BASE_DIR/logstash/logs"
  "$BASE_DIR/logstash/data"
  "$BASE_DIR/redis/logs"
  "$BASE_DIR/redis"
  "$BASE_DIR/sqlserver/logs"
  "$BASE_DIR/sqlserver/data"
)

echo "Criando diretórios necessários..."
for DIR in "${DIRECTORIES[@]}"; do
  if [ ! -d "$DIR" ]; then
    sudo mkdir -p "$DIR"
    echo "Criado: $DIR"
  else
    echo "Diretório já existe: $DIR"
  fi
done

echo "Configurando permissões para todos os diretórios..."
for DIR in "${DIRECTORIES[@]}"; do
  sudo chmod -R 777 "$DIR"
done
sudo chown -R 10001:0 "$BASE_DIR/sqlserver/data"
sudo chown -R 10001:0 "$BASE_DIR/sqlserver/logs"
sudo chown -R 10001:0 /dados/sqlserver/data
sudo chown -R 10001:0 /dados/sqlserver/log
sudo chmod -R 777 /dados/sqlserver/data
sudo chmod -R 777 /dados/sqlserver/log

echo "Permissões configuradas em todos os diretórios."

echo "Verificando se a network '$NETWORK_NAME' já existe..."
if [ ! "$(docker network ls --filter name=^${NETWORK_NAME}$ --format '{{.Name}}')" ]; then
  docker network create "$NETWORK_NAME"
  echo "Network '$NETWORK_NAME' criada com sucesso."
else
  echo "Network '$NETWORK_NAME' já existe."
fi

echo "Iniciando os contêineres com docker compose..."
docker compose up --build -d

echo "Configuração concluída com sucesso!"