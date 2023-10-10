using MVCCollegeWeb.Databases;
using MVCCollegeWeb.Utils.JsonResponses;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MVCCollegeWeb.Models
{
    public class IndexViewModel
    {
        private List<CourseSemester>? Semesters { get; set; }
        private List<Person>? peoples { get; set; }
        public List<CourseSemester>? FilterSemester { get; set; }
        public List<CourseSemesterStudent>? CourseSemesterStudents { get; set; }
        public List<Person>? CourseTeacher { get; set; }
        public CourseSemester? CourseInfo { get; set; }

        private static IndexViewModel? _instance;

        public static IndexViewModel GetInstance()
        {
            _instance ??= new IndexViewModel();
            return _instance;
        }

        public async Task GetCourses()
        {
            string parameters = $"{{\"parameters\":[]}}";
            var data = await GetDataFromApi<CourseSemester>("CourseSemesterGetAll", parameters);
            if (data != null)
            {
                Semesters = data;
                SortBySemester();
            }
        }

        public async Task GetPersons()
        {
            string parameters = $"{{\"parameters\":[]}}";
            var data = await GetDataFromApi<Person>("PersonGetAll", parameters);
            if (data != null)
            {
                peoples = data;
            }
        }

        public async Task GetStudentsByCourse(string scheduleId)
        {

            CourseInfo = Semesters.Where(s => s.Id.ToString().Equals(scheduleId)).FirstOrDefault();

            string parameters = $"{{\"parameters\":[\"{CourseInfo.CourseId}\"]}}";
            var data = await GetDataFromApi<CourseSemesterStudent>("CourseSemesterStudentGetAll", parameters);
            if (data != null)
            {
                CourseSemesterStudents = data;
            }

            if (CourseInfo != null)
            {
                List<int> teacherIds = Semesters
                    .Where(s => s.Course.Equals(CourseInfo.Course))
                    .Select(s => s.TeacherId)
                    .Distinct()
                    .ToList();

                CourseTeacher = peoples.FindAll(p => teacherIds.Contains(p.Id));
            }
        }

        private async Task<List<T>> GetDataFromApi<T>(string methodName, string parameters)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(GetHost.Get());
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync($"College?method={methodName}&parameters={parameters}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseSTR = await response.Content.ReadAsStringAsync();
                        string cleanResponse = "";
                        cleanResponse = responseSTR.Replace(@"\", "");
                        cleanResponse = cleanResponse.Substring(1, cleanResponse.Length - 2);
                        var responseJson = JsonSerializer.Deserialize<ResponseJsonData<T>>(cleanResponse);

                        return responseJson.data;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        return new List<T>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                return new List<T>();
            }
        }


        #region sorting
        public void SortBySemester()
        {
            var semesterOrder = new List<string> { "Winter", "Spring", "Summer", "Fall" };

            FilterSemester = Semesters.OrderBy(s => DateTime.ParseExact(s.Semester.Split(' ')[1], "yyyy", null))
                             .ThenBy(s => semesterOrder.IndexOf(s.Semester.Split(' ')[0]))
                             .ToList();
        }

        private void SortByCourse()
        {
            FilterSemester = Semesters.OrderBy(s => s.Course).ToList();
        }

        #endregion

        #region Filter
        public void FilterByCourse(string courseName)
        {

            FilterSemester = Semesters.FindAll(s => s.Course.Contains(courseName, StringComparison.OrdinalIgnoreCase));
        }

        public void FilterByDepartment(string departmentName)
        {
            if (departmentName == "All")
            {
                FilterSemester = Semesters;
                return;
            }

            FilterSemester = FilterSemester.FindAll(s => s.Department.Equals(departmentName, StringComparison.OrdinalIgnoreCase));
        }
        public void FilterBySemester(string semesterName)
        {
            if (semesterName == "All")
            {
                FilterSemester = Semesters;
                return;
            }

            FilterSemester = Semesters.FindAll(s => s.Semester.Equals(semesterName, StringComparison.OrdinalIgnoreCase));
        }
        public void FilterByDayAndTime(string day, TimeSpan time)
        {
            FilterSemester = Semesters.FindAll(s => IsInSchedule(s.Schedule, day, time));
        }
        public void FilterBySemesterAndSchedule(string semesterName, string day, TimeSpan time)
        {
            FilterSemester = Semesters.FindAll(s => s.Semester.Equals(semesterName, StringComparison.OrdinalIgnoreCase) && IsInSchedule(s.Schedule, day, time));
        }

        private bool IsInSchedule(string schedule, string day, TimeSpan time)
        {
            var daySchedules = schedule.Split('/');
            foreach (var daySchedule in daySchedules)
            {
                var parts = daySchedule.Trim().Split(' ');
                if (parts.Length < 5) continue;
                if (!parts[0].Equals(day, StringComparison.OrdinalIgnoreCase)) continue;

                var startTime = TimeSpan.Parse(parts[1]);
                var endTime = TimeSpan.Parse(parts[3]);

                if (time >= startTime && time <= endTime) return true;
            }
            return false;
        }

        #endregion

    }
}
