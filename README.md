# API_SPA

Prueba tecnica full stack para gestion de categorias y productos con backend en .NET y frontend en React.

## Clonado

```bash
git clone <REPO_URL>
cd API_SPA
```

## Descripcion del proyecto

La solucion expone una API REST con autenticacion JWT y una SPA que cubre el flujo principal de negocio:

- login
- listado de productos
- creacion de productos
- edicion de productos
- eliminacion de productos
- carga masiva por lotes

El alcance funcional actual se centra en Category y Product.

## Arquitectura

Se usa arquitectura por capas para mantener separacion clara de responsabilidades:

- Api: entrada HTTP, configuracion, controladores
- Application: casos de uso, DTOs, servicios y mapeo explicito
- Domain: entidades de negocio
- Infrastructure: EF Core, DbContext, configuracion de persistencia

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

Usuario semilla configurado en appsettings:

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

Caracteristicas incluidas:

- almacenamiento de JWT en localStorage
- wrapper HTTP para adjuntar Bearer token
- proteccion de rutas privadas
- manejo basico de errores
- limpieza de sesion y redireccion a /login ante 401 autenticado

## Como correr backend

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

## Como correr frontend

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

## Ejecucion con Docker (todo el stack)

Primera ejecucion recomendada:

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
- PostgreSQL: localhost:5432 (configurable con POSTGRES_HOST_PORT)

Para detener:

```bash
docker compose down
```

Para detener y limpiar volumen de BD:

```bash
docker compose down -v
```

## Ejecucion local / hibrida

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

Levantado rapido del stack completo:

```bash
docker compose up -d --build
```

## Estrategia de carga masiva

Implementada en POST /products/bulk con enfoque eficiente y simple:

- POST /products/bulk es la implementacion explicita del requisito de carga masiva del reto.
- generacion aleatoria de productos validos
- seleccion de categorias objetivo
- insercion por lotes con AddRange + SaveChanges por batch
- desactivacion temporal de AutoDetectChanges
- limpieza de ChangeTracker entre lotes
- respuesta con metricas de ejecucion

Limitacion actual:

- ejecucion sincrona en request/response

Evolucion recomendada:

- mover a procesamiento asincrono con colas y workers

## Pruebas

Backend:

- UnitTests (xUnit)
- IntegrationTests (xUnit + WebApplicationFactory)

Ejecucion:

```bash
dotnet test API_SPA.sln
```

## CI/CD

Pipeline basico en GitHub Actions:

- archivo: .github/workflows/ci.yml
- validaciones backend: restore, build y test
- validacion frontend: npm ci + npm run build
- validacion compose: docker compose config
- validacion Docker: docker build de imagen API y frontend

## Troubleshooting minimo

- Puerto 5432 ocupado al levantar db:
  Ejecutar con puerto alterno: POSTGRES_HOST_PORT=5433 docker compose up -d db.
- Error de CORS en frontend:
  Revisar que el origen del frontend este incluido en Cors:AllowedOrigins.
- API no conecta a PostgreSQL en Docker:
  Verificar estado healthy del servicio db y que la cadena use Host=db en compose.
- Error por esquema inexistente al iniciar API:
  Ejecutar docker compose --profile tools run --rm migrate antes de levantar api/frontend.
- Login falla en local:
  Validar credenciales de prueba (admin/Admin123!) y VITE_API_BASE_URL en frontend/.env.

## Notas para evaluador

- El proyecto cubre el flujo full stack solicitado: login JWT, CRUD visible de productos y carga masiva.
- POST /products/bulk complementa POST /product para soportar el requisito de rendimiento de carga hasta 100.000 registros por lotes.
- El pipeline CI valida backend, frontend, compose y build de imagenes Docker para asegurar entregabilidad.
