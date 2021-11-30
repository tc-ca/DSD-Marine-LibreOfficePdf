using ConverterService.Configuration;
using ConverterService.Pdf;
using ConverterService.Sessions;
using ConverterService.WebApi.Files;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel((context, options) => {
    var fileUploadOptions = context.Configuration.GetSection(FileUploadOptions.FileUpload)
        .Get<FileUploadOptions>();
    options.Limits.MaxRequestBodySize = fileUploadOptions.MaxRequestBodySize;

});

builder.Services.Configure<FileUploadOptions>(
    builder.Configuration.GetSection(FileUploadOptions.FileUpload));
builder.Services.Configure<ConversionOptions>(
    builder.Configuration.GetSection(ConversionOptions.Conversion));

builder.Services.AddHostedService<ConversionDispatcher>();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ISessionRepository, InMemorySessionRepository>();
builder.Services.AddSingleton<IResultRepository, FileSystemResultRepository>();
builder.Services.AddTransient<IFileUploader, FileUploader>();
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

