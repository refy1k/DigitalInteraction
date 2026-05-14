using DigitalInteraction.Services;
using DigitalInteraction.Views.Auth;
using DigitalInteraction.Views.Appeals;
using DigitalInteraction.Views.Applications;
using DigitalInteraction.Views.Profile;
using DigitalInteraction.Views.Notifications;
using DigitalInteraction.Views.Home;
using DigitalInteraction.Views.Services;
using Supabase;

namespace DigitalInteraction;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
                fonts.AddFont("Roboto-Medium.ttf", "RobotoMedium");
                fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
            });

        // Supabase клиент
        var supabaseClient = SupabaseService.GetClientAsync().Result;
        builder.Services.AddSingleton<Client>(supabaseClient);

        // Сервисы (singleton)
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<AppealService>();
        builder.Services.AddSingleton<ApplicationService>();
        builder.Services.AddSingleton<ProfileService>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddSingleton<MunicipalServiceService>();

        // Pages (transient)
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<AppealsListPage>();
        builder.Services.AddTransient<AppealDetailPage>();
        builder.Services.AddTransient<CreateAppealPage>();
        builder.Services.AddTransient<ApplicationsListPage>();
        builder.Services.AddTransient<ApplicationDetailPage>();
        builder.Services.AddTransient<CreateApplicationPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<DocumentsPage>();
        builder.Services.AddTransient<DocumentsEditPage>();
        builder.Services.AddTransient<ContactInfoPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<MunicipalServicesPage>();
        builder.Services.AddTransient<MunicipalServiceDetailPage>();
        builder.Services.AddTransient<ServiceRequestsPage>();

        return builder.Build();
    }
}