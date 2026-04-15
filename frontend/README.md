# Frontend (React)

SPA minima para consumir la API de productos con autenticacion JWT.

## Requisitos

- Node.js 20+
- npm 10+

## Configuracion

1. Copiar variables de entorno:

```bash
cp .env.example .env
```

2. Revisar API base URL en .env:

```bash
VITE_API_BASE_URL=http://localhost:5000
```

## Ejecucion

```bash
npm install
npm run dev
```

## Rutas

- /login
- /products
- /products/new
- /products/:id/edit

## Flujo

- Login consume POST /auth/login
- JWT se guarda en localStorage
- Wrapper HTTP adjunta Authorization Bearer automaticamente para requests autenticados
- Productos permite listar, filtrar, paginar, crear y editar
