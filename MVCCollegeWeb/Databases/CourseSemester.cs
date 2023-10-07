namespace MVCCollegeWeb.Databases
{
    public class CourseSemester
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Course { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        public int SemesterId { get; set; }
        public string Semester { get; set; }
        public int TeacherId { get; set; }
        public string TeacherFirstName { get; set; }
        public string TeacherLastName { get; set; }
        public string Schedule { get; set; }
        public string Location { get; set; }
    }
}
