using MVCCollegeWeb.Databases;
using MVCCollegeWeb.Utils.JsonResponses;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MVCCollegeWeb.Models
{
    public class LoginViewModel
    {
        private bool _isAuthenticated = false;
        private static LoginViewModel? _instance;

        public static LoginViewModel GetInstance()
        {
            _instance ??= new LoginViewModel();
            return _instance;
        }
        private LoginViewModel() { }

        public bool IsAuthenticated => _isAuthenticated;

        public async Task Authenticated(string username, string password)
        {
            try
            {

                using var client = new HttpClient();

                client.BaseAddress = new Uri(GetHost.Get());
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //GET Method
                string method = "AccountValidate";
                string parameters = $"{{\"parameters\":[\"{username}\",\"{password}\"]}}";

                HttpResponseMessage response = await client.GetAsync("College?method=" + method + "&parameters=" + parameters);

                if (response.IsSuccessStatusCode)
                {

                    string responseSTR = await response.Content.ReadAsStringAsync();

                    string cleanResponse = "";
                    cleanResponse = responseSTR.Replace(@"\", "");
                    cleanResponse = cleanResponse.Substring(1, cleanResponse.Length - 2);

                    var responseJson = JsonSerializer.Deserialize<ResponseJsonData<Account>>(cleanResponse);

                    var data = responseJson.status;

                    if (data != "success")
                    {
                        _isAuthenticated = false;
                        return;
                    }

                    _isAuthenticated = true;
                }
                else
                {
                    _isAuthenticated = false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Logout()
        {
            _isAuthenticated = false;
        }
    }
}
