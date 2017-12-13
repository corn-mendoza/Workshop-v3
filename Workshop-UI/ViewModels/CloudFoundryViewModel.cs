using Steeltoe.Extensions.Configuration.CloudFoundry;


namespace Fortune_Teller_UI.ViewModels.Fortunes
{
    public class CloudFoundryViewModel
    {
        public CloudFoundryViewModel(CloudFoundryApplicationOptions appOptions, CloudFoundryServicesOptions servOptions)
        {
            CloudFoundryServices = servOptions;
            CloudFoundryApplication = appOptions;
        }
        public CloudFoundryServicesOptions CloudFoundryServices { get;}
        public CloudFoundryApplicationOptions CloudFoundryApplication { get;}
    }
}
