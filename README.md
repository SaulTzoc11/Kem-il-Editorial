# Kem'il Editorial

Plataforma web de la editorial: catálogo y compra de libros, pedidos especiales (cotizaciones) y panel de administración.

- **Backend:** ASP.NET Core (net10.0) + Entity Framework Core + SQL Server
- **Frontend:** React 19 + Vite

## Cómo levantar el proyecto

### Base de datos
Cada desarrollador trabaja con su propia instancia local de SQL Server, restaurada desde el respaldo oficial `kemilDB.bak`. Los pasos están en [`database/README.md`](database/README.md).

Después de restaurar, ejecuta `database/usuarios_prueba.sql`. Crea los roles Mayorista e Institucional y los cinco usuarios de prueba de la tabla de abajo. Se puede ejecutar más de una vez sin duplicar datos.

Los demás scripts de `database/` (`schema.sql`, `seed.sql`, `seed_catalogo.sql`, `seed_portadas.sql`) son de referencia y de carga de datos del catálogo.

### Backend
```powershell
cd backend
dotnet run
```
Escucha en `http://localhost:5229` (Swagger en `/swagger`).

La cadena de conexión real va en `backend/appsettings.Development.json`, que **no se sube a Git**. Ahí se sobrescribe `ConnectionStrings:DefaultConnection`. Los valores de `appsettings.json` son solo de desarrollo local; en producción se debe cambiar `Jwt:Key`.

### Frontend
```powershell
cd frontend
npm install
npm run dev
```
Abre `http://localhost:5173`. Por defecto consume `http://localhost:5229/api`; para otro entorno se crea `frontend/.env` con `VITE_API_URL=...`.

## Usuarios de prueba

Contraseña de todos: `Kemil2026!` (solo para desarrollo).

| Rol | Correo |
|---|---|
| Administrador | admin@kemileditorial.com |
| Asesor | asesor@kemileditorial.com |
| Cliente (minorista) | cliente@kemileditorial.com |
| Mayorista | mayorista@kemileditorial.com |
| Institucional | institucional@kemileditorial.com |

## Autenticación y control de acceso

- **Contraseñas:** se guardan con BCrypt (factor de trabajo 12). Nunca se almacenan ni se devuelven en texto plano.
- **Sesión:** JWT de 60 minutos con id, nombre, correo y rol. En cada petición el servidor vuelve a comprobar en la base que el usuario siga activo y conserve el mismo rol, de modo que desactivar una cuenta o cambiarle el rol surte efecto de inmediato.
- **Denegar por defecto:** todo endpoint exige sesión salvo los marcados como públicos (`[AllowAnonymous]`). Una ruta nueva sin atributos queda protegida.
- **Login:** limitado a 5 intentos por minuto por IP.
- **Registro público:** siempre crea usuarios con rol Cliente. Los demás roles solo los asigna un administrador.
- **Frontend:** `ProtectedRoute` oculta las pantallas por rol, pero solo es comodidad visual. La seguridad real está en el backend.

### Qué puede hacer cada rol

| Acción | Público | Cliente | Mayorista | Institucional | Asesor | Administrador |
|---|:-:|:-:|:-:|:-:|:-:|:-:|
| Ver catálogo, calcular carrito | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Comprar (crear orden) y ver sus pedidos | | ✔ | ✔ | ✔ | | |
| Solicitar cotización (pedido especial) | | | ✔ | ✔ | | |
| Ver y responder cotizaciones | | | | | ✔ | ✔ |
| Usuarios, roles, resumen y monitoreo de órdenes | | | | | | ✔ |

Un rol sin permiso recibe **403**; sin sesión, **401**.

## Pedidos especiales (cotizaciones)

Los clientes Mayorista e Institucional envían una solicitud. El personal (Asesor o Administrador) la responde con un monto y observaciones. Una cotización ya cerrada (respondida o rechazada) no se puede modificar: responde **409**.

En la base los estados son `Pendiente`, `En revision`, `Cotizada`, `Aprobada` y `Rechazada`. La API traduce `Cotizada` a `Respondida` y `En revision` a `En revisión` para el frontend.

## Endpoints principales

| Método | Ruta | Acceso |
|---|---|---|
| POST | `/api/auth/registro` | Público |
| POST | `/api/auth/login` | Público (limitado) |
| GET | `/api/auth/me` | Sesión |
| POST | `/api/auth/usuarios` | Administrador |
| GET | `/api/usuarios` | Administrador |
| PUT | `/api/usuarios/{id}/rol` | Administrador |
| PUT | `/api/usuarios/{id}/estado` | Administrador |
| GET | `/api/roles` | Administrador |
| POST | `/api/cotizaciones` | Mayorista, Institucional |
| GET | `/api/cotizaciones/bandeja` | Asesor, Administrador |
| PUT | `/api/cotizaciones/{id}/responder` | Asesor, Administrador |
| GET | `/api/admin/resumen` | Administrador |
| GET | `/api/ordenes?estado=` | Administrador |
| POST | `/api/ordenes` | Cliente, Mayorista, Institucional |
| GET | `/api/ordenes/mis-pedidos` | Cliente, Mayorista, Institucional |