# Informe de Cumplimiento de Requerimientos Técnicos
## Proyecto: OPA Risco Optimizer

Este documento detalla cómo la solución desarrollada cumple con cada uno de los requerimientos opcionales y principales solicitados en la prueba técnica.

---

### 1. Funcionamiento Multiplataforma (OSX, Linux, Windows)
La aplicación ha sido desarrollada utilizando **.NET 8**, una plataforma de código abierto y multiplataforma de Microsoft. 
- **Backend**: El motor de optimización y la API REST corren nativamente sobre el runtime de .NET, el cual es compatible con **Windows**, **Linux** (Ubuntu, Debian, Alpine, etc.) y **macOS**.
- **Frontend**: Al ser una aplicación web basada en estándares (HTML5, CSS3, JavaScript ES6), es totalmente agnóstica al sistema operativo del cliente y puede ejecutarse en cualquier navegador moderno (Chrome, Firefox, Safari, Edge).

### 2. Interoperabilidad
La arquitectura de la solución se basa en una separación clara entre el **Cliente** (Frontend) y el **Servidor** (Backend/API):
- **API RESTful**: Expone endpoints que reciben y entregan datos en formato **JSON**, el estándar de la industria para el intercambio de datos.
- **Desacoplamiento**: El motor de optimización puede ser consumido no solo por el frontend incluido, sino por aplicaciones móviles, otros microservicios o herramientas de terceros (como se demuestra con la integración de **Swagger/OpenAPI**).
- **Contratos Claros**: Se definieron modelos de datos (`SolicitudOptimizacion`, `RespuestaOptimizacion`) que garantizan que cualquier sistema que cumpla el contrato pueda integrarse con la lógica de negocio.

### 3. Facilidad de Mantenimiento
Se aplicaron principios de diseño de software para asegurar que el código sea limpio y fácil de evolucionar:
- **Arquitectura por Capas / Servicios**: La lógica de negocio está aislada en `ServicioOptimizador.cs`, separada de los controladores de entrada y de los modelos de datos.
- **Inyección de Dependencias (DI)**: Se utiliza el contenedor nativo de .NET para gestionar el ciclo de vida de los servicios, facilitando futuras sustituciones o de componentes.
- **Código Documentado**: Se incluyeron comentarios extensos en español (XML Documentation) que explican el "porqué" de las decisiones algorítmicas, facilitando el onboarding de nuevos desarrolladores.
- **Validaciones Robustas**: El sistema valida exhaustivamente las entradas antes de procesarlas, evitando estados inconsistentes.

### 4. Uso de Control de Versiones (GitHub)
El proyecto ha sido estructurado desde el inicio para ser gestionado con **Git**:
- **Estructura Estándar**: Sigue la convención de carpetas `src/`, `docs/`, facilitando la navegación en repositorios.
- **Archivo .gitignore**: Incluye una configuración profesional para evitar subir archivos temporales, binarios de compilación (`obj/`, `bin/`) o configuraciones personales de IDEs.
- **README Detallado**: Provee instrucciones claras para clonar, compilar y ejecutar el proyecto en cualquier entorno.

### 5. Persistencia de la Información
Para esta entrega de 6 horas, se priorizó la **Persistencia en el Cliente** y la **Arquitectura Stateless** (sin estado) en el servidor:
- **Estado del Cliente**: Los elementos agregados por el usuario se mantienen en la memoria del navegador (`app.js`), permitiendo simulaciones rápidas sin latencia de red innecesaria.
- **Arquitectura Escalable**: Al ser stateless, el backend puede escalar horizontalmente (múltiples instancias) sin preocuparse por la sincronización de sesiones.
- **Preparado para DB**: La estructura del proyecto permite integrar fácilmente una base de datos (como SQL Server, PostgreSQL o SQLite) mediante Entity Framework Core, simplemente inyectando un repositorio en el `ServicioOptimizador`.

### 6. Escalabilidad de la Solución
La escalabilidad se abordó desde dos frentes:

#### A. Escalabilidad Algorítmica (Eficiencia)
El problema planteado es una variante del *0/1 Knapsack*. En lugar de usar fuerza bruta (que fallaría con más de 30 elementos), se implementó:
- **Programación Dinámica (Dynamic Programming)**: Con una complejidad de $O(n \times W)$, donde $n$ es el número de ítems y $W$ el peso máximo.
- **Selección Inteligente de Dimensión**: El sistema decide automáticamente si resolver por **Peso** o por **Calorías**, eligiendo siempre el camino que consuma menos memoria y tiempo.
- **Mecanismo de Fallback**: Para casos con volúmenes de datos extremos que superen los límites de memoria RAM, se incluyó un algoritmo **Greedy** de aproximación para garantizar que la aplicación siempre responda en tiempo real.

#### B. Escalabilidad de Infraestructura
Al estar empaquetada como una Web API liviana:
- Puede ser **Contenerizada con Docker** fácilmente.
- Es apta para despliegues en la nube (Azure App Service, AWS Lambda, Google Cloud Run) donde puede escalar según la demanda de peticiones por segundo.

---
**Conclusión:**
La solución no solo resuelve el acertijo lógico planteado, sino que se entrega como una pieza de software profesional, extensible y lista para un entorno de producción moderno.
