using Martina.Abstractions;
using Martina.Services;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Martina.Extensions;

public static class ServiceCollectionsExtensions
{
    public static void AddBuptSchedular(this IServiceCollection collection)
    {
        collection.AddSingleton<BuptSchedular>();
        collection.AddSingleton<ISchedular, BuptSchedular>(provider => provider.GetRequiredService<BuptSchedular>());
        collection.AddHostedService<BuptSchedular>(provider => provider.GetRequiredService<BuptSchedular>());
    }

    public static void AddMartinaSwagger(this IServiceCollection collection)
    {
        collection.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "巴普特廉价酒店管理系统API阶段",
                Description = """
                              WebSocket接口地址：

                              /api/airConditioner/ws 推送所有房间的空调状态信息

                              /api/airConditioner/ws/{roomId} 推送指定房间的空调状态

                              参数: roomId 房间的ID

                              /api/time 推送当前的系统时间
                              """
            });
            options.DocInclusionPredicate((_, api) => api.HttpMethod != null);
            options.AddSecurityDefinition("oauth2",
                new OpenApiSecurityScheme
                {
                    Description = "Need 'Authorization' header using JWT token.",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
            // 添加XML注释的内容
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Martina.xml"));
        });
    }
}
