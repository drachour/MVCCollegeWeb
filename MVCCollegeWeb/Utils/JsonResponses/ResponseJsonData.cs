using MVCCollegeWeb.Databases;

namespace MVCCollegeWeb.Utils.JsonResponses
{
    public class ResponseJsonData<T>
    {
        public string status { get; set; }
        public string message { get; set; }
        public List<T> data { get; set; }

        public ResponseJsonData()
        {

            this.status = "failed";
            this.message = "unknown error";
            this.data = new List<T>();

        }
    }
}
