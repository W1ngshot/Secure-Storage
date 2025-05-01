using SecureStorage.Core.Features.ChangePassword;
using SecureStorage.Core.Features.CreateUser;
using SecureStorage.Core.Features.GetFields;
using SecureStorage.Core.Features.ResetPassword;
using SecureStorage.Core.Features.UpdateUser;

namespace SecureStorage.API.ServiceExtensions;

public static class CoreHandlersServiceExtensions
{
    public static IServiceCollection AddCoreHandlers(this IServiceCollection services)
    {
        services.AddScoped<CreateUserCommandHandler>();
        services.AddScoped<GetFieldsQueryHandler>();
        services.AddScoped<UpdateUserCommandHandler>();
        services.AddScoped<ChangePasswordCommandHandler>();
        services.AddScoped<ResetPasswordCommandHandler>();
        
        return services;
    }
}