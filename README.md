# GymApi Backend

API REST para la gestion de un gimnasio con autenticacion JWT, manejo de roles y modulos orientados a socios, entrenadores, rutinas, ejercicios, membresias y asistencia.

## Resumen

El backend de `GymApi` esta pensado para cubrir tres perfiles principales:

- `Socio`: cliente del gimnasio que puede registrarse publicamente, completar su perfil, registrar asistencia y consultar su rutina.
- `Entrenador`: usuario creado por admin, con datos propios de entrenador y enfocado en el trabajo con socios y rutinas.
- `Admin`: usuario administrativo con control operativo del gimnasio.

La API trabaja con autenticacion basada en JWT y relaciones entre entidades como `User`, `Socio`, `Entrenadore`, `Rutina`, `Ejercicio`, `Membresia` y `SocioEntrenador`.

## Objetivo del sistema

El sistema busca separar claramente:

- la cuenta de acceso (`User`)
- el rol del usuario (`Role`, `UserRole`)
- los datos de negocio asociados a cada perfil (`Socio`, `Entrenadore`)
- la vinculacion entre socio y entrenador (`SocioEntrenador`)
- los planes de entrenamiento (`Rutina`, `RutinaEjercicio`)

Esto permite que la autenticacion, la asignacion operativa y la planificacion de entrenamiento no queden mezcladas en una sola tabla.

## Arquitectura conceptual

### User
Representa la cuenta de acceso al sistema.

Campos relevantes:

- `UserId`
- `UserName`
- `NormalizedUserName`
- `Email`
- `NormalizedEmail`
- `PasswordHash`
- `PhoneNumber`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

Relaciones:

- uno a uno con `Socio`
- uno a uno con `Entrenadore`
- uno a muchos con `UserRole`
- uno a muchos con `Asistencia` cuando registra acciones administrativas

### Role y UserRole
Permiten asignar roles a los usuarios.

Roles esperados:

- `SOCIO`
- `ENTRENADOR`
- `ADMIN`

La autenticacion y autorizacion dependen de que esos roles existan correctamente en base de datos y de que el JWT incluya los claims correspondientes.

### Socio
Entidad de negocio del cliente del gimnasio.

Campos relevantes:

- `SocioId`
- `UserId`
- `FechaNacimiento`
- `Genero`
- `AlturaCm`
- `PesoKg`
- `EmergenciaNombre`
- `EmergenciaTelefono`
- `FechaRegistro`
- `IsActive`

Notas:

- `Genero` se esta manejando como una sola letra:
  - `M`
  - `F`
  - `O`

### Entrenadore
Entidad de negocio del entrenador.

Campos relevantes:

- `EntrenadorId`
- `UserId`
- `Especialidad`
- `Certificaciones`
- `FechaIngreso`
- `IsActive`

### SocioEntrenador
Representa la asignacion operativa entre socio y entrenador.

Campos relevantes:

- `SocioEntrenadorId`
- `SocioId`
- `EntrenadorId`
- `FechaAsignacion`
- `Activo`

Esta tabla no reemplaza a `Rutina`. Su funcion es indicar que un socio esta vinculado a un entrenador, incluso aunque aun no exista una rutina creada.

### Rutina
Representa un plan de entrenamiento.

Campos relevantes:

- `RutinaId`
- `SocioId`
- `EntrenadorId`
- `Nombre`
- `Objetivo`
- `FechaInicio`
- `FechaFin`
- `Activa`
- `CreatedAt`

La rutina deberia crearse sobre una relacion valida previa entre `Socio` y `Entrenador`.

### RutinaEjercicio
Relaciona ejercicios con una rutina y define parametros de ejecucion.

Campos relevantes:

- `RutinaEjercicioId`
- `RutinaId`
- `EjercicioId`
- `Orden`
- `Series`
- `Repeticiones`
- `PesoObjetivoKg`
- `DuracionSegundos`
- `DescansoSegundos`
- `Notas`

### Ejercicio
Catalogo de ejercicios.

Campos relevantes:

- `EjercicioId`
- `Nombre`
- `Descripcion`
- `GrupoMuscular`
- `IsActive`

### Membresia
Catalogo de membresias del gimnasio.

Campos relevantes:

- `MembresiaId`
- `Nombre`
- `Descripcion`
- `DuracionDias`
- `Precio`
- `EsRenovable`
- `IsActive`
- `CreatedAt`

### Asistencia
Registro de entrada y salida de socios.

Campos relevantes:

- `AsistenciaId`
- `SocioId`
- `FechaHoraEntrada`
- `FechaHoraSalida`
- `Observaciones`
- `RegistradaPorUserId`

## Flujo de autenticacion

### Login
El login se realiza con:

- `POST /api/Auth/login`

Request:

```json
{
  "email": "admin@gym.com",
  "password": "123456"
}
```

Respuesta esperada:

```json
{
  "token": "jwt_aqui"
}
```

### Consideraciones importantes del token

El JWT debe incluir al menos:

- `userId`
- claim de `role`

Claims compatibles esperables:

- `userId`
- `role`
- `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`

Si el token no incluye el rol, el frontend no puede redirigir correctamente por perfil.

### Registro publico
El registro publico solo aplica para `Socio`:

- `POST /api/Auth/register`

Campos:

- `userName`
- `email`
- `password`
- `phoneNumber`

Este endpoint crea la cuenta de usuario y le asigna el rol `SOCIO`, pero no necesariamente crea inmediatamente el perfil completo de `Socio`.

## Reglas de negocio principales

### Socio

- puede registrarse publicamente
- luego puede completar su perfil de socio con `POST /api/Socio/completarsocio`
- solo el socio registra su propia asistencia desde su flujo normal

### Entrenador

- no se registra publicamente
- debe ser creado por admin
- debe existir un `User` y un `Entrenadore` asociados

### Admin

- administra entrenadores
- administra relaciones `SocioEntrenador`
- administra rutinas, ejercicios, membresias y asistencia manual

### Asignacion socio-entrenador

La tabla `SocioEntrenador` existe para reflejar la relacion base entre ambas partes.

Uso recomendado:

1. admin crea al entrenador
2. socio existe en el sistema
3. admin asigna un socio a un entrenador
4. luego se crean rutinas sobre esa vinculacion

Esto evita rutinas con combinaciones invalidas.

### Rutinas

La rutina no deberia usarse para reemplazar la tabla `SocioEntrenador`.

Recomendacion de negocio:

- al seleccionar un entrenador para crear una rutina, solo deberian poder elegirse socios asignados a ese entrenador

## Endpoints expuestos

## Auth

- `POST /api/Auth/login`
- `POST /api/Auth/register`

## Asistencia

- `POST /api/Asistencia/entrada`
- `PUT /api/Asistencia/salida`
- `GET /api/Asistencia/mihistorial`
- `GET /api/Asistencia/hoy`
- `POST /api/Asistencia/admin/entrada/{socioId}`

## Socio

- `GET /api/Socio`
- `POST /api/Socio`
- `GET /api/Socio/{id}`
- `PUT /api/Socio/{id}`
- `DELETE /api/Socio/{id}`
- `GET /api/Socio/miperfil`
- `POST /api/Socio/completarsocio`

## Entrenadore

- `GET /api/Entrenadore`
- `POST /api/Entrenadore`
- `GET /api/Entrenadore/{id}`
- `PUT /api/Entrenadore/{id}`
- `DELETE /api/Entrenadore/{id}`

## SocioEntrenador

- `GET /api/SocioEntrenador`
- `POST /api/SocioEntrenador`
- `GET /api/SocioEntrenador/{id}`
- `PUT /api/SocioEntrenador/{id}`
- `DELETE /api/SocioEntrenador/{id}`

## Rutina

- `GET /api/Rutina`
- `POST /api/Rutina`
- `PUT /api/Rutina/{id}`
- `DELETE /api/Rutina/{id}`

## RutinaEjercicio

- `POST /api/RutinaEjercicio/{id}/ejercicio`
- `PUT /api/RutinaEjercicio/{rEId}`
- `DELETE /api/RutinaEjercicio/{rEId}`
- `GET /api/RutinaEjercicio/{rutinaId}/ejercicios`

## Ejercicio

- `GET /api/Ejercicio`
- `POST /api/Ejercicio`
- `PUT /api/Ejercicio`
- `GET /api/Ejercicio/{id}`
- `DELETE /api/Ejercicio/{id}`

## Membresia

- `GET /api/Membresia`
- `POST /api/Membresia`
- `PUT /api/Membresia/{id}`
- `DELETE /api/Membresia/{id}`

## Role

- `GET /api/Role`
- `POST /api/Role`
- `PUT /api/Role`
- `GET /api/Role/{id}`
- `DELETE /api/Role/{id}`

## User

- `GET /api/User`
- `POST /api/User`
- `GET /api/User/{id}`
- `PUT /api/User/{id}`
- `DELETE /api/User/{id}`

## DTOs principales

### LoginRequest

```json
{
  "email": "string",
  "password": "string"
}
```

### RegisterRequest

```json
{
  "userName": "string",
  "email": "string",
  "password": "string",
  "phoneNumber": "string"
}
```

### CompletarSocioRequest

```json
{
  "fechaNacimiento": "2026-04-06",
  "genero": "M",
  "alturaCm": 175,
  "pesoKg": 72.5,
  "emergenciaNombre": "Juan Perez",
  "emergenciaTelefono": "999999999"
}
```

### CrearEntrenadoreRequest

```json
{
  "userName": "coach1",
  "email": "coach1@gym.com",
  "password": "123456",
  "phoneNumber": "999999999",
  "especialidad": "Fuerza",
  "certificaciones": "NSCA"
}
```

### CrearSocioEntrenadorRequest

```json
{
  "socioId": 1,
  "entrenadorId": 2
}
```

### CrearRutinaRequest

```json
{
  "socioId": 1,
  "entrenadorId": 2,
  "nombre": "Hipertrofia base",
  "objetivo": "Ganancia muscular",
  "fechaInicio": "2026-04-06",
  "fechaFin": "2026-05-06"
}
```

### CrearRutinaEjercicioRequest

```json
{
  "ejercicioId": 3,
  "orden": 1,
  "series": 4,
  "repeticiones": 10,
  "pesoObjetivoKg": 30,
  "duracionSegundos": null,
  "descansoSegundos": 90,
  "notas": "Controlar tecnica"
}
```

### CrearEjercicioRequest

```json
{
  "nombre": "Press banca",
  "descripcion": "Movimiento de empuje horizontal",
  "grupoMuscular": "Pecho"
}
```

### CrearMembresiaRequest

```json
{
  "nombre": "Mensual",
  "descripcion": "Acceso libre por 30 dias",
  "duracionDias": 30,
  "precio": 90,
  "esRenovable": true
}
```

## Respuestas observadas o asumidas

### `GET /api/Socio`
Actualmente se ha visto una respuesta resumida como:

```json
[
  {
    "SocioId": 1,
    "SocioNombre": "username4",
    "Genero": "M",
    "Altura": 163,
    "Peso": 60
  }
]
```

### `GET /api/Socio/{id}`
Se ha visto una respuesta tipo:

```json
{
  "SocioId": 1,
  "UserName": "username4",
  "Email": "mail@gym.com",
  "PesoKg": 60,
  "AlturaCm": 163,
  "FechaNacimiento": "2000-01-01",
  "Genero": "M"
}
```

### `GET /api/Rutina`
Se ha visto una respuesta tipo:

```json
[
  {
    "RutinaId": 1,
    "Socio": {
      "SocioId": 1,
      "Nombre": "username4"
    },
    "Entrenador": {
      "EntreadorId": 2,
      "Nombre": "coach1"
    },
    "Nombre": "Hipertrofia base",
    "FechaInicio": "2026-04-06",
    "FechaFin": "2026-05-06",
    "Activa": true
  }
]
```

## Observaciones tecnicas importantes

### Login y carga de roles

En `AuthController.Login`, para generar bien el token con roles, el usuario debe cargarse junto con sus roles:

```csharp
.Include(u => u.UserRoles)
.ThenInclude(ur => ur.Role)
```

Si no se cargan esas relaciones, el token puede salir sin claim de rol.

### Bug detectado en validacion de login

Si se verifica password antes de comprobar que el usuario existe, puede producirse un `500`.

Orden correcto:

1. buscar usuario
2. validar que existe
3. verificar password
4. generar token

### Bug detectado en validacion de rutinas

En la validacion de solapamiento se detecto una condicion invertida.

Incorrecto:

```csharp
if (!rutinaExiste) return BadRequest("ya existe una rutina en ese rango de fecha");
```

Correcto:

```csharp
if (rutinaExiste) return BadRequest("ya existe una rutina en ese rango de fecha");
```

### Bug detectado en validaciones de entrenador

En el metodo de creacion de entrenador se detecto este patron incorrecto:

```csharp
if(userNameExiste)return Conflict("ya existe usuario con este email");
if(userNameExiste)return Conflict("ya existe usuario con este numero");
```

Lo correcto es validar cada variable correspondiente:

```csharp
if (emailExiste) return Conflict("ya existe usuario con este email");
if (phoneNumberExiste) return Conflict("ya existe usuario con este numero");
```

## Recomendaciones

- mantener `SocioEntrenador` como asignacion base
- usar `Rutina` solo como plan de entrenamiento
- asegurar que el JWT siempre lleve el rol
- devolver respuestas consistentes en `camelCase` o `PascalCase`, pero idealmente no mezclar ambos
- documentar los response DTOs del login y de los `GET` principales en Swagger
- exponer endpoints de tipo `me` o `mi-rutina` si luego quieres simplificar mucho el frontend

## Swagger

El backend expone Swagger/OpenAPI y desde ahi puede generarse el archivo JSON de especificacion. Rutas habituales:

- `/swagger`
- `/swagger/index.html`
- `/swagger/v1/swagger.json`

## Estado funcional esperado

### Socio

- registrarse
- iniciar sesion
- completar perfil si aun no existe entidad `Socio`
- registrar entrada y salida
- consultar historial
- ver rutina

### Entrenador

- iniciar sesion con cuenta creada por admin
- ver socios asignados
- crear rutinas para socios vinculados
- consultar ejercicios

### Admin

- crear entrenadores
- ver socios
- asignar socio a entrenador
- crear rutinas respetando esa asignacion
- administrar ejercicios
- administrar membresias
- registrar asistencia manual


