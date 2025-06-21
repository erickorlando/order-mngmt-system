# Script para reconstruir microservicios en desarrollo
Write-Host "🔄 Reconstruyendo microservicios..." -ForegroundColor Yellow

# Detener contenedores
Write-Host "⏹️  Deteniendo contenedores..." -ForegroundColor Blue
docker-compose down

# Eliminar imágenes específicas
Write-Host "🗑️  Eliminando imágenes de microservicios..." -ForegroundColor Blue
docker rmi ordermanagementsystem-orders-api ordermanagementsystem-vendors-api 2>$null

# Reconstruir sin caché
Write-Host "🔨 Reconstruyendo imágenes..." -ForegroundColor Green
docker-compose build --no-cache orders-api vendors-api

# Levantar servicios
Write-Host "🚀 Levantando servicios..." -ForegroundColor Green
docker-compose up -d

Write-Host "✅ Reconstrucción completada!" -ForegroundColor Green
Write-Host "📊 Verificando estado de los servicios..." -ForegroundColor Cyan
docker-compose ps 