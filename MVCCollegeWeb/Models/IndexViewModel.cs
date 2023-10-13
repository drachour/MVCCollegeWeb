using MVCCollegeWeb.Databases;
using MVCCollegeWeb.Utils.JsonResponses;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MVCCollegeWeb.Models
{
    public class IndexViewModel
    {
        private readonly ILogger _logger;
        private List<CourseSemester>? Semesters { get; set; }
        public List<Person>? StudentListing { get; set; }
        public List<CourseSemester>? FilterSemester { get; set; }
        public List<CourseSemesterStudent>? CourseSemesterStudents { get; set; }
        public List<Person>? CourseTeacher { get; set; }
        public CourseSemester? CourseInfo { get; set; }


        private static IndexViewModel? _instance;

        public IndexViewModel(ILogger logger)
        {
            this._logger = logger;
        }

        public static IndexViewModel GetInstance(ILogger logger)
        {
            return _instance ??= new IndexViewModel(logger);
        }

        public async Task GetCourses()
        {
            string parameters = $"{{\"parameters\":[]}}";
            var data = await GetDataApi<CourseSemester>("CourseSemesterGetAll", parameters);
            if (data != null)
            {
                Semesters = data;
                SortBySemester();
            }
        }
        public async Task GetStudents()
        {
            StudentListing = await GetPerson(1);
        }
        public async Task GetStudentsByCourse(string scheduleId)
        {

            CourseInfo = Semesters.Where(s => s.Id.ToString().Equals(scheduleId)).FirstOrDefault();

            string parameters = $"{{\"parameters\":[\"{CourseInfo.Id}\"]}}";
            var data = await GetDataApi<CourseSemesterStudent>("CourseSemesterStudentGetAll", parameters);
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

                List<Person> tmp = await GetPerson(0);
                CourseTeacher = tmp;
                //CourseTeacher = tmp.FindAll(p => teacherIds.Contains(p.Id));
            }
        }
        public List<Person> GetStudentAvailable()
        {
            List<Person> students = new List<Person>();
            try
            {
                var currentStudentIds = CourseSemesterStudents.Select(s => s.StudentId).ToList();

                _logger.LogInformation("Current Student IDs: " + string.Join(", ", currentStudentIds));

                if (StudentListing == null)
                {
                    StudentListing = GetPerson(1).Result;
                }

                _logger.LogInformation("All Student IDs: " + string.Join(", ", StudentListing.Select(s => s.Id)));

                students = StudentListing.Where(student => !currentStudentIds.Contains(student.Id)).ToList();

                _logger.LogInformation("Available Student IDs: " + string.Join(", ", students.Select(s => s.Id)));
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Exception: " + ex.Message);
            }
            return students;
        }
        public async Task InsertStudentCourse(int studentId)
        {
            string parameters = $"{{\"parameters\":[\"{CourseInfo.Id}\",\"{studentId}\"]}}";
            await PostDataApi("CourseSemesterStudentInsert", parameters, 0);
            await GetStudentsByCourse(CourseInfo.Id.ToString());
        }
        public async Task UpdateGrade(int studentId, decimal grade)
        {
            string parameters = $"{{\"parameters\":[\"{CourseInfo.Id}\",\"{studentId}\",\"{grade}\"]}}";
            await PostDataApi("CourseSemesterStudentUpdateGrade", parameters, 1);
        }
        public async Task UpdateCourseTeacher(int teacherId)
        {
            string parameters = $"{{\"parameters\":[\"{CourseInfo.Id}\",\"{teacherId}\"]}}";
            await PostDataApi("CourseSemesterUpdateTeacher", parameters, 1);
        }
        public async Task UpdatePersonInfo(Person updateStudent)
        {
            string parameters = $"{{\"parameters\":[\"{updateStudent.Id}\",\"{updateStudent.FirstName}\",\"{updateStudent.LastName}\",\"{updateStudent.Phone}\",\"{updateStudent.Email}\"]}}";
            await PostDataApi("PersonUpdateInfo", parameters, 2);
        }
        public async Task RemoveStudentCourse(int studentId)
        {
            string parameters = $"{{\"parameters\":[\"{CourseInfo.Id}\",\"{studentId}\"]}}";
            await PostDataApi("CourseSemesterStudentDelete", parameters, 3);
        }

        private async Task<List<Person>> GetPerson(int option)
        {
            string parameters = "";
            List<Person> data = new List<Person>();
            if (option == 0)
            {
                parameters = $"{{\"parameters\":[\"Teacher\"]}}";
                data = await GetDataApi<Person>("PersonGetAll", parameters);
            }
            else if (option == 1)
            {
                parameters = $"{{\"parameters\":[\"Student\"]}}";
                data = await GetDataApi<Person>("PersonGetAll", parameters);
            }

            return data;
        }
        private async Task<List<T>> GetDataApi<T>(string methodName, string parameters)
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
                        _logger.LogInformation($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        return new List<T>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception occurred: {ex.Message}");
                return new List<T>();
            }
        }
        private async Task PostDataApi(string methodName, string parameters, int option)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    HttpResponseMessage? response = null;
                    client.BaseAddress = new Uri(GetHost.Get());
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    if(option == 0)
                    {
                        response = await client.PostAsync($"College?method={methodName}&parameters={parameters}", null);
                    }
                    else if(option == 1)
                    {
                        response = await client.PatchAsync($"College?method={methodName}&parameters={parameters}", null);
                    }
                    else if (option == 2)
                    {
                        response = await client.PutAsync($"College?method={methodName}&parameters={parameters}", null);
                    }
                    else if (option == 3)
                    {
                        response = await client.DeleteAsync($"College?method={methodName}&parameters={parameters}");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                    else
                    {
                        await GetCourses();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception occurred: {ex.Message}");
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
