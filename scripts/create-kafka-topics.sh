#!/bin/bash

# Скрипт для создания топиков Kafka, если их нет
# Использование: ./scripts/create-kafka-topics.sh

KAFKA_BOOTSTRAP_SERVER="${KAFKA_BOOTSTRAP_SERVER:-kafka:9092}"
MAX_RETRIES=30
RETRY_DELAY=2

# Функция для проверки доступности Kafka
wait_for_kafka() {
    echo "Waiting for Kafka to be ready..."
    for i in $(seq 1 $MAX_RETRIES); do
        if /opt/kafka/bin/kafka-broker-api-versions.sh --bootstrap-server $KAFKA_BOOTSTRAP_SERVER > /dev/null 2>&1; then
            echo "Kafka is ready!"
            return 0
        fi
        echo "Attempt $i/$MAX_RETRIES: Kafka is not ready yet, waiting ${RETRY_DELAY}s..."
        sleep $RETRY_DELAY
    done
    echo "Kafka is not available after $MAX_RETRIES attempts"
    return 1
}

# Функция для создания топика, если его нет
create_topic_if_not_exists() {
    local topic=$1
    local partitions=${2:-1}
    local replication_factor=${3:-1}
    
    # Проверяем, существует ли топик
    if /opt/kafka/bin/kafka-topics.sh --bootstrap-server $KAFKA_BOOTSTRAP_SERVER --list | grep -q "^${topic}$"; then
        echo "Topic '$topic' already exists, skipping..."
        return 0
    fi
    
    echo "Creating topic: $topic"
    /opt/kafka/bin/kafka-topics.sh \
        --bootstrap-server $KAFKA_BOOTSTRAP_SERVER \
        --create \
        --topic $topic \
        --partitions $partitions \
        --replication-factor $replication_factor \
        --if-not-exists
    
    if [ $? -eq 0 ]; then
        echo "Topic '$topic' created successfully"
    else
        echo "Failed to create topic '$topic'"
        return 1
    fi
}

# Список топиков для создания
TOPICS=(
    "domain-events-tenants"
    "domain-events-projects"
    "domain-events-users"
    "domain-events-tasks"
    "domain-events-identity"
    "domain-events-accesspermissions"
)

# Ждем готовности Kafka
if ! wait_for_kafka; then
    echo "Failed to connect to Kafka. Exiting."
    exit 1
fi

# Создаем топики
echo "Creating Kafka topics..."
for topic in "${TOPICS[@]}"; do
    create_topic_if_not_exists "$topic" 1 1
done

# Ждем синхронизации топиков в Kafka
echo "Waiting for topics to be available in Kafka..."
sleep 5

# Проверяем, что все топики доступны
echo "Verifying topics are available..."
for topic in "${TOPICS[@]}"; do
    if /opt/kafka/bin/kafka-topics.sh --bootstrap-server $KAFKA_BOOTSTRAP_SERVER --list | grep -q "^${topic}$"; then
        echo "✓ Topic '$topic' is available"
    else
        echo "✗ Topic '$topic' is NOT available"
        exit 1
    fi
done

# Выводим список всех топиков
echo ""
echo "Listing all topics:"
/opt/kafka/bin/kafka-topics.sh --bootstrap-server $KAFKA_BOOTSTRAP_SERVER --list

echo ""
echo "All topics created and verified successfully!"

