Kubernetes & Service Mesh notes

1) Service Mesh
- Për prodhim përdorni Istio ose Linkerd. Instalimi bëhet në cluster (eks, AKS, EKS, GKE).
- Konfiguroni mTLS, ingress gateway, dhe telemetry (Prometheus/Grafana).

2) Event-driven
- Përdorni Kafka për throughput të lartë, ose RabbitMQ për setup më të thjeshtë.
- Në Kubernetes, përdorni Strimzi për Kafka operator.

3) Helm Charts
- Shtoni helm charts për secilin mikroshërbi me Deployment, Service, ConfigMap për connection strings dhe secret për password.

4) Lokalisht
- Përdorni `docker-compose.yml` për zhvillim lokal (RabbitMQ + MSSQL + services).

Komanda për të ngritur zhvillim lokal:

```powershell
cd "c:\Users\jonuk\OneDrive\Desktop\Projekti per Rekomandime"
docker compose up --build
```

Për Istio në cluster (shembull):

- Instaloni Istio CLI: https://istio.io/
- `istioctl install --set profile=demo -y`
- Label namespace: `kubectl label namespace default istio-injection=enabled`

Për Kafka në k8s: përdorni Strimzi operator.
