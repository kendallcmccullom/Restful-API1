using System;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using HelloPlatform;
using HelloPlatform.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.VisualBasic.FileIO;
using static System.Net.Mime.MediaTypeNames;


/// <summary>
/// Helper methods
/// </summary>
public class Adapter
{
    //public static IWebHostEnvironment _webHostEnvironment;
    /// <summary>
    /// Check that student exists on the roster
    /// </summary>
    public static Student Check(String Name, List<Student> roster)
    {

        int count = roster.Count;
        for (var i = 0; i < count; i++)
        {
            Student student = roster[i];
            if (student.Name == Name)
            {
                return student;
            }
        }
        return null;
    }

    /// <summary>
    /// Check if course exists
    /// </summary>
    public static Boolean CheckCourses(Student student, String course)
    {
        Schedule[] courses = student.CourseList;
        for (var i = 0; i < student.NumberOfCourses; i++)
        {
            if (courses[i].CourseType.Equals(course))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if subject is valid
    /// </summary>
    public static Boolean CheckSubject(List<string> courses, String course)
    {
        for (var i = 0; i < courses.Count; i++)
        {
            if (course.Equals(courses[i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Isolate subject when subject + level entered
    /// </summary>
    public static String IsolateSubject(String course)
    {
        String subject = "";
        int i = 0;
        foreach (char c in course)
        {
            i++;
            if (c >= '0' && c <= '9')
            {

                break;
            }
            subject += c;
        }
        return subject.Remove(subject.Length - 1);

    }
    public static Schedule[] GetCourses(int Number_of_Courses, List<String> courses)
    {
        var random = new Random();
        int index = random.Next(0, courses.Count);
        Number_of_Courses = Math.Clamp(Number_of_Courses, 1, 10);

        Schedule[] listofcourses = Enumerable.Range(1, Number_of_Courses).Select(index => new Schedule
        {
            CourseType = courses[random.Next(0, courses.Count)] + " " + random.Next(0, 1000),

        }).ToArray();
        return listofcourses;
    }

    /// <summary>
    /// Create dictionary to get students by grade
    /// </summary>
    public static Dictionary<String, List<String>> ByGrade(Student[] _roster)
     {
         Dictionary<string, List<String>> grades1 = new Dictionary<string, List<String>>();
         grades1.Add("freshman", new List<string>());
         grades1.Add("sophmore", new List<string>());
         grades1.Add("junior", new List<string>());
         grades1.Add("senior", new List<string>());
         
         for (var i = 0; i<_roster.Length; i++)
         {
            Student student = _roster[i];
            string grade_level = student.Grade;
            grades1[grade_level].Add(student.Name + ", " + student.Major);
         }
         return grades1;
    }

}
