# URL Shortener Service

![image](https://github.com/user-attachments/assets/33a00f4b-2f01-44a9-8dbc-023ce0e8b8bb)

## 🚀 Descripción General

Un servicio de acortamiento de URLs desarrollado con ASP.NET Core 8.0 que permite transformar URLs largas en enlaces cortos y fáciles de compartir. La aplicación rastrea estadísticas de uso y proporciona un rendimiento optimizado a través de técnicas avanzadas de caché.

## 📋 Características Principales

- **Acortamiento de URLs**: Genera códigos alfanuméricos de 6 caracteres para cualquier URL válida
- **Redirección Eficiente**: Redirige rápidamente a los usuarios desde las URLs acortadas a los destinos originales
- **Validación Inteligente**: Previene el acortamiento de URLs que ya han sido procesadas por este servicio
- **Estadísticas de Acceso**: Rastrea la cantidad de accesos, información del agente de usuario y marca de tiempo
- **Alta Disponibilidad**: Optimizado para entornos de producción con balanceo de carga

## 🏗️ Arquitectura

El proyecto sigue un patrón de arquitectura limpia con capas bien definidas:

- **Controllers**: Manejo de peticiones HTTP y respuestas
- **Services**: Lógica de negocio
- **Repositories**: Acceso a almacenamiento de datos
- **Infrastructure**: Implementaciones de base de datos y caché
- **DTOs**: Objetos de transferencia de datos
- **Models**: Entidades de dominio
- **Validators**: Reglas de validación de entrada

## 🛠️ Stack Tecnológico

- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 9.0.3
- **Base de Datos**: SQL Server
- **Caché**: Redis (StackExchange.Redis)
- **Validación**: FluentValidation
- **Documentación**: Swagger/OpenAPI
- **Despliegue**: Azure Web App

## 🔌 API Endpoints

- **POST /api/UrlShortener/shorten**: Crea una URL acortada
- **GET /api/UrlShortener/{shortcode}**: Redirige a la URL original

## 📊 Esquema de Base de Datos

La base de datos incluye dos tablas principales:
- **ShortenedUrls**: Almacena la información de mapeo de URL
- **UrlAccesses**: Rastrea estadísticas de acceso

## 🔒 Consideraciones de Seguridad

- Validación de entrada para todos los datos proporcionados por el usuario
- Prevención de acortamiento de URLs que ya han sido procesadas
- Aplicación forzada de HTTPS

## 🚀 Despliegue

El servicio está desplegado actualmente en Azure Web App en:
`https://urlshortener20250320150016.azurewebsites.net/`

## 💻 Desarrollo Local

### Requisitos Previos
- .NET SDK 8.0 o superior
- SQL Server (local o en contenedor)
- Redis (local o en contenedor)

### Configuración
1. Clonar el repositorio
2. Configurar la cadena de conexión en `appsettings.Development.json`
3. Ejecutar las migraciones: `dotnet ef database update`
4. Iniciar la aplicación: `dotnet run`
