# PowerShell скрипт для создания топиков Kafka
# Использование: .\scripts\create-kafka-topics.ps1

$KAFKA_CONTAINER = "nimbussite-kafka"

# Список топиков для создания
$TOPICS = @(
    "domain-events-tenants",
    "domain-events-projects",
    "domain-events-users",
    "domain-events-tasks",
    "domain-events-identity",
    "domain-events-accesspermissions"
)

Write-Host "Creating Kafka topics..." -ForegroundColor Green

foreach ($topic in $TOPICS) {
    Write-Host "Creating topic: $topic" -ForegroundColor Yellow
    docker exec -it $KAFKA_CONTAINER /opt/kafka/bin/kafka-topics.sh `
        --create `
        --topic $topic `
        --bootstrap-server localhost:9092 `
        --replication-factor 1 `
        --partitions 3 `
        --if-not-exists
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Topic $topic already exists or error occurred" -ForegroundColor Yellow
    }
}

Write-Host "`nListing all topics:" -ForegroundColor Green
docker exec -it $KAFKA_CONTAINER /opt/kafka/bin/kafka-topics.sh --list --bootstrap-server localhost:9092

Write-Host "`nDone!" -ForegroundColor Green