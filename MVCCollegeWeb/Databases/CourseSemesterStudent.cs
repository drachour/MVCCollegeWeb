namespace MVCCollegeWeb.Databases
{
    public class CourseSemesterStudent
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Grade { get; set; }
    }
}
