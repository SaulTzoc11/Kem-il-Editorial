# Base de Datos — Kem'il Editorial (`kemilDB`)

Este directorio contiene las instrucciones y recursos necesarios para la gestión, restauración y actualización de la base de datos relacional del proyecto **Kem'il Editorial** en **SQL Server**.

---

## 🛠️ Flujo y Metodología de Trabajo

Para garantizar la estabilidad y rapidez durante el desarrollo local, **cada desarrollador trabajará con su propia instancia local de SQL Server** en su máquina (`localhost`). 

Para asegurar que todos contemos con el mismo esquema, tablas, llaves foráneas y datos de prueba sincronizados, el punto de verdad de la base de datos se maneja a través de archivos de respaldo oficial (**`.bak`**).

---

## Pasos para la Configuración Inicial (Entorno Local)

Si estás configurando el proyecto por primera vez en tu equipo, sigue estos pasos:

### 1. Obtener el archivo de respaldo
* Ubica o copia el archivo `kemilDB.bak` (proporcionado en el repositorio o compartido por el equipo) en una ruta local de tu computadora (por ejemplo, la carpeta por defecto de backups de SQL Server).

### 2. Restaurar la Base de Datos en SQL Server Management Studio (SSMS)
1. Abre **SQL Server Management Studio (SSMS)** y conéctate a tu servidor local.
2. En el **Explorador de Objetos** (*Object Explorer*), haz clic derecho sobre la carpeta **Databases** (Bases de datos) y selecciona **Restore Database...** (Restaurar base de datos).
3. En la ventana emergente, selecciona la opción **Device** (Dispositivo) en la sección de origen (*Source*) y haz clic en los tres puntos (`...`).
4. Haz clic en **Add** (Agregar), busca el archivo `kemilDB.bak` en tu equipo y presiona **OK**.
5. Verifica que en *Database* el nombre de destino sea exactamente **`kemilDB`**.
6. Haz clic en **OK** en la parte inferior para completar la restauración.

### 3. Verificar las Restricciones y Datos Iniciales
* La base de datos ya incluye los catálogos base (Roles, Géneros, Editoriales, Autores, Stock) y tablas operativas (Usuarios, Libros, Facturas, Cotizaciones, Carrito, etc.).
* **Importante:** La tabla `libro` cuenta con una restricción de verificación (`CK_libro_formato`). Los valores permitidos para el campo `formato` son únicamente los definidos en el esquema (por ejemplo: `'Tapa dura'`, `'E-book'`).

---

## Conexión con el Backend (.NET Web API)

Asegúrate de que en tu archivo `backend/appsettings.json` (o `appsettings.Development.json`) la cadena de conexión apunte a tu servidor local de SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=kemilDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}