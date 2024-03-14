WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication application = builder.Build();

await application.RunAsync();