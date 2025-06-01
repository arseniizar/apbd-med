using Microsoft.EntityFrameworkCore;
using Tutorial10.Database;
using Tutorial10.Endpoints;
using Tutorial10.Repository;
using Tutorial10.Repository.Interfaces;
using Tutorial10.Service;
using Tutorial10.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PrescriptionDbContext>(options =>
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    options.UseInMemoryDatabase("PrescriptionDb"));

builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Prescription API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prescription API V1"); });

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PrescriptionDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

app.UseHttpsRedirection();

// app.UseAuthorization(); 

app.MapPrescriptionEndpoints();
app.MapPatientEndpoints();

app.Run();