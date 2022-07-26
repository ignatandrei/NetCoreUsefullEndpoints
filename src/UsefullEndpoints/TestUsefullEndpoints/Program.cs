using AMSWebAPI;
using UsefullExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(it => it.AllowCredentials().AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(it => true));
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapUsefullAll();
//app.MapUsefullAll("myCors", new string[] {"myAuthPolicy"});
//app.MapUsefullConfiguration();
app.UseAuthorization();

app.MapControllers();
app.UseAMS();

app.Run();
