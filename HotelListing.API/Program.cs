using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Cors Start
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyHeader()
        .AllowAnyOrigin()
        .AllowAnyMethod()
        );
});
//Cors End

builder.Host.UseSerilog((ctx, lc)=>lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

var app = builder.Build();



// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSerilogRequestLogging();//Log the error into log file in log folder.

app.UseHttpsRedirection();
app.UseCors("AllowAll");//CORS is configured here.

app.UseAuthorization();

app.MapControllers();

app.Run();
