using MotoData;
using MotoApi.Middlewares;
using Microsoft.EntityFrameworkCore;
using MotoApi.Messaging;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Conexão com banco
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar controllers
builder.Services.AddControllers();
var factory = new ConnectionFactory() 
{ 
    HostName = "localhost", 
    UserName = "guest", 
    Password = "guest" 
};
var connection = factory.CreateConnection();
var publisher = new MotoEventPublisher(connection);

// Registra o publisher como singleton
builder.Services.AddSingleton(publisher);


// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});
builder.Services.AddOpenApi();


builder.Services.AddScoped<MotoBusiness.MotoService>();


var app = builder.Build();

// Swagger só em dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ⚠ Middleware de tratamento de erros precisa vir antes de tudo
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

// Mapear endpoints dos controllers
app.MapControllers();

app.Run();
