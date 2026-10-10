# API_SPA — Catálogo de productos (.NET 8 + React)

[![CI](https://github.com/andrestorresaioros/API_SPA/actions/workflows/ci.yml/badge.svg)](https://github.com/andrestorresaioros/API_SPA/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET_8-512BD4?style=flat&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL_16-4169E1?style=flat&logo=postgresql&logoColor=white)
![React](https://img.shields.io/badge/React_18-61DAFB?style=flat&logo=react&logoColor=black)
![Vite](https://img.shields.io/badge/Vite-646CFF?style=flat&logo=vite&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)

Prueba técnica full stack para gestión de categorías y productos con backend en .NET y frontend en React.

## Clonado

```bash
git clone https://github.com/andrestorresaioros/API_SPA.git
cd API_SPA
```

## Descripción del proyecto

La solución expone una API REST con autenticación JWT y una SPA que cubre el flujo principal de negocio:

- login
- listado de productos
- creación de productos
- edición de productos
- eliminación de productos
- carga masiva por lotes

El alcance funcional actual se centra en Category y Product.

## Arquitectura

Se usa arquitectura por capas para mantener separación clara de responsabilidades:

- Api: entrada HTTP, configuración, controladores
- Application: casos de uso, DTOs, servicios y mapeo explicito
- Domain: entidades de negocio
- Infrastructure: EF Core, DbContext, configuración de persistencia

## Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- PostgreSQL 16
- React 18 + Vite
- xUnit
- Docker / Docker Compose

## Estructura del repositorio

- src/Api
- src/Application
- src/Domain
- src/Infrastructure
- tests/UnitTests
- tests/IntegrationTests
- frontend
- .github/workflows

## Credenciales de prueba

Usuario semilla configurado en `appsettings.json` (solo para desarrollo local; cámbialo, junto con `Jwt:Key`, antes de cualquier despliegue):

- username: admin
- password: Admin123!

## Endpoints principales

- POST /auth/login
- POST /category
- GET /category
- POST /product
- GET /products
- GET /products/{id}
- PUT /products/{id}
- DELETE /products/{id}
- POST /products/bulk

Endpoints protegidos con JWT:

- POST /category
- POST /product
- PUT /products/{id}
- DELETE /products/{id}
- POST /products/bulk

## Frontend

Rutas implementadas:

- /login
- /products
- /products/new
- /products/:id/edit

Características incluidas:

- almacenamiento de JWT en localStorage
- wrapper HTTP para adjuntar Bearer token
- protección de rutas privadas
- manejo básico de errores
- limpieza de sesión y redirección a /login ante 401 autenticado

## Cómo correr backend

1. Levantar PostgreSQL con Docker Compose:

```bash
docker compose up -d db
```

2. Aplicar migraciones con servicio auxiliar (sin depender de herramientas locales):

```bash
docker compose --profile tools run --rm migrate
```

3. Ejecutar API:

```bash
dotnet run --project src/Api
```

4. Verificar salud:

```bash
curl http://localhost:5000/health
```

## Cómo correr frontend

1. Configurar variables de entorno:

```bash
cd frontend
cp .env.example .env
```

2. Si el backend corre en localhost:5000, usar:

```bash
VITE_API_BASE_URL=http://localhost:5000
```

3. Instalar dependencias y levantar SPA:

```bash
npm install
npm run dev
```

4. Abrir:

- http://localhost:5173

## Ejecución con Docker (todo el stack)

Primera ejecución recomendada:

1. Levantar base de datos:

```bash
docker compose up -d db
```

2. Aplicar migraciones con servicio auxiliar (sin depender de herramientas locales):

```bash
docker compose --profile tools run --rm migrate
```

3. Levantar API y frontend en contenedores:

```bash
docker compose up -d --build api frontend
```

Ejecuciones siguientes (cuando la base ya tiene esquema):

```bash
docker compose up -d --build
```

Servicios y puertos:

- Frontend: http://localhost:8081
- API: http://localhost:8080
- PostgreSQL: localhost:5432 (configurable con `POSTGRES_HOST_PORT`; ver `.env.example`)

Para detener:

```bash
docker compose down
```

Para detener y limpiar volumen de BD:

```bash
docker compose down -v
```

## Ejecución local / híbrida

Modo local completo (sin contenedor de API/frontend):

1. Levantar solo BD en Docker:

```bash
docker compose up -d db
```

2. Correr API local:

```bash
dotnet run --project src/Api
```

3. Correr frontend local:

```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```

4. Asegurar que frontend apunte a API local:

```bash
VITE_API_BASE_URL=http://localhost:5000
```

## Docker

Archivo principal:

- docker-compose.yml

Servicios:

- db: PostgreSQL 16
- migrate (profile tools): aplica migraciones EF Core
- api: ASP.NET Core API
- frontend: React SPA servida con Nginx

Levantado rápido del stack completo:

```bash
docker compose up -d --build
```

## Estrategia de carga masiva

Implementada en POST /products/bulk con enfoque eficiente y simple:

- POST /products/bulk es la implementación explicita del requisito de carga masiva del reto.
- generación aleatoria de productos válidos
- selección de categorías objetivo
- inserción por lotes con AddRange + SaveChanges por batch
- desactivación temporal de AutoDetectChanges
- limpieza de ChangeTracker entre lotes
- respuesta con métricas de ejecución

Limitación actual:

- ejecución síncrona en request/response

Evolución recomendada:

- mover a procesamiento asíncrono con colas y workers

## Pruebas

Backend:

- UnitTests (xUnit)
- IntegrationTests (xUnit + WebApplicationFactory)

Ejecución:

```bash
dotnet test API_SPA.sln
```

## CI/CD

Pipeline básico en GitHub Actions:

- archivo: .github/workflows/ci.yml
- validaciones backend: restore, build y test
- validación frontend: npm ci + npm run build
- validación compose: docker compose config
- validación Docker: docker build de imagen API y frontend

## Troubleshooting mínimo

- Puerto 5432 ocupado al levantar db:
  Ejecutar con puerto alterno: POSTGRES_HOST_PORT=5433 docker compose up -d db.
- Error de CORS en frontend:
  Revisar que el origen del frontend esté incluido en Cors:AllowedOrigins.
- API no conecta a PostgreSQL en Docker:
  Verificar estado healthy del servicio db y que la cadena use Host=db en compose.
- Error por esquema inexistente al iniciar API:
  Ejecutar docker compose --profile tools run --rm migrate antes de levantar api/frontend.
- Login falla en local:
  Validar credenciales de prueba (admin/Admin123!) y VITE_API_BASE_URL en frontend/.env.

## Notas para evaluador

- El proyecto cubre el flujo full stack solicitado: login JWT, CRUD visible de productos y carga masiva.
- POST /products/bulk complementa POST /product para soportar el requisito de rendimiento de carga hasta 100.000 registros por lotes.
- El pipeline CI valida backend, frontend, compose y build de imágenes Docker para asegurar entregabilidad.

## Autor

**César Andrés Torres Bernal** · [GitHub](https://github.com/andrestorresaioros) · [LinkedIn](https://www.linkedin.com/in/andr%C3%A9s-torres10)

## Licencia

Distribuido bajo la licencia MIT. Consulta [LICENSE](LICENSE).
