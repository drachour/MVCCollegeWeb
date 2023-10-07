namespace MVCCollegeWeb.Utils.JsonResponses
{
    public class ResponseJson
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }

        public ResponseJson()
        {

            this.Status = "failed";
            this.Message = "unknown error";
            this.Data = "";

        }
    }
}
