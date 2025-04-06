using System.Diagnostics;


//using Microsoft.AspNetCore.Mvc;

namespace HelloPlatform
{
    /// <summary>
    /// Student class
    /// </summary>
    public class Student
    {
        /// <summary>
        /// Name of Student.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// List of Courses for Student.
        /// </summary>
        public Schedule[]? CourseList { get; set; }

        /// <summary>
        /// Number of Courses for Student.
        /// </summary>
        public int NumberOfCourses { get; set; }

        /// <summary>
        /// Grade Level of Student.
        /// </summary>
        public string? Grade {  get; set; }

        /// <summary>
        /// Major of Student
        /// </summary>
        public string? Major {  get; set; }

        /// <summary>
        /// Name of file 
        /// </summary>
        public string Photo { get; set; }

    }
}
