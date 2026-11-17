using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApi.Data;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Cadena de conexión y DbContext de TamboDB
var connectionString = builder.Configuration.GetConnectionString("TamboDbConnection");
builder.Services.AddDbContext<TamboDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// 2. Registro de servicios de lógica de negocio (SOA)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IPromocionService, PromocionService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();

// 3. Controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tambo+ E-commerce SOA API",
        Version = "v1",
        Description = "Servicios SOA para la aplicación de e-commerce Tambo+:\n" +
                      "- Catálogo de Productos y Búsqueda Predictiva (Servicios 1, 2, 6, 7, 9)\n" +
                      "- Promociones y Cupones (Servicios 8, 11)\n" +
                      "- Carrito de Compras (Servicio 10)\n" +
                      "- Gestión de Clientes y Autenticación (Servicios 12, 13, 14)\n" +
                      "- Checkout, Logística, Pagos y Seguimiento (Servicios 3, 4, 5, 15, 16, 17, 18, 19)"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// 4. Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tambo+ SOA API v1");
        c.RoutePrefix = "swagger"; // Disponible en https://localhost:7180/swagger/index.html
    });
}

// Redireccionar la raíz a /swagger automáticamente
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
