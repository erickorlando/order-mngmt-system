#!/bin/bash

# Script para reconstruir microservicios en desarrollo
echo "🔄 Reconstruyendo microservicios..."

# Detener contenedores
echo "⏹️  Deteniendo contenedores..."
docker-compose down

# Eliminar imágenes específicas
echo "🗑️  Eliminando imágenes de microservicios..."
docker rmi ordermanagementsystem-orders-api ordermanagementsystem-vendors-api 2>/dev/null

# Reconstruir sin caché
echo "🔨 Reconstruyendo imágenes..."
docker-compose build --no-cache orders-api vendors-api

# Levantar servicios
echo "🚀 Levantando servicios..."
docker-compose up -d

echo "✅ Reconstrucción completada!"
echo "📊 Verificando estado de los servicios..."
docker-compose ps 