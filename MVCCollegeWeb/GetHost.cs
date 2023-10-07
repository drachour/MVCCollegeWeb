namespace MVCCollegeWeb
{
    public class GetHost
    {
        public static string Get()
        {

            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var hostUrl = configuration.GetValue<string>("UrlHost");

            return hostUrl;

        }
    }
}
