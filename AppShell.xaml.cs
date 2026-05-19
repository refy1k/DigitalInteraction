using DigitalInteraction.Views.Appeals;
using DigitalInteraction.Views.Applications;
using DigitalInteraction.Views.Profile;
using DigitalInteraction.Views.Services;
using DigitalInteraction.Views.Auth;
using DigitalInteraction.Views.Profile;

namespace DigitalInteraction;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(AppealDetailPage), typeof(AppealDetailPage));
        Routing.RegisterRoute(nameof(CreateAppealPage), typeof(CreateAppealPage));
        Routing.RegisterRoute(nameof(ApplicationDetailPage), typeof(ApplicationDetailPage));
        Routing.RegisterRoute(nameof(CreateApplicationPage), typeof(CreateApplicationPage));
        Routing.RegisterRoute(nameof(DocumentsPage), typeof(DocumentsPage));
        Routing.RegisterRoute(nameof(ContactInfoPage), typeof(ContactInfoPage));
        Routing.RegisterRoute(nameof(MunicipalServicesPage), typeof(MunicipalServicesPage));
        Routing.RegisterRoute(nameof(MunicipalServiceDetailPage), typeof(MunicipalServiceDetailPage));
        Routing.RegisterRoute(nameof(ServiceRequestsPage), typeof(ServiceRequestsPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(EmailVerificationPage), typeof(EmailVerificationPage));

    }
}