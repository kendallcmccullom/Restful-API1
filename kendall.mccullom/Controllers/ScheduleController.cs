using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Transactions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace HelloPlatform.Controllers
{
    /// <summary>
    /// Student Schedule Controller
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")] // Makes the default Content-Type for all responses "application/json". Put something different above your method if you want to override it.
    public class ScheduleController : ControllerBase
    {
        public static List<Student> Roster = SavingInfo.updatedroster();
        private static List<string> courses = new List<string>() { "art", "biology", "chemistry", "physics", "communications","music", "math", "drama", "english", "language", "computer science", "history", "science", "law" };
        private List<String> gradeLevel = new List<string>() { "freshman", "sophmore", "junior", "senior" };

        /// <summary>
        /// Adds a new student with their schedule and number of desire courses.
        /// </summary>
        [HttpPost("{Name}, {Number_of_Courses} /addperson")]
        [ProducesResponseType(typeof(Student), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(Student), StatusCodes.Status409Conflict)]
        [Produces("application/json")]
        public IActionResult Post([FromBody] Student person, string Name, string Major, int Number_of_Courses) 
        {
            //setting limit on number of courses that a student can register for
            Number_of_Courses = Math.Clamp(Number_of_Courses, 0, 10);
            Student checking = Adapter.Check(Name, Roster);
            //if student already on roster, updates to preferred number of courses
            if (Adapter.Check(Name, Roster) != null && Number_of_Courses != checking.NumberOfCourses)
            {
                checking.CourseList = Adapter.GetCourses(Number_of_Courses, courses);
                checking.NumberOfCourses = Number_of_Courses;
                return Ok("Student already in system, updating preferences");
            }
            //otherwise add student to the roster
            person.CourseList = Adapter.GetCourses(Number_of_Courses, courses);
            person.Name = Name;
            person.NumberOfCourses = Number_of_Courses;
            Random rnd = new Random();
            person.Grade = gradeLevel[rnd.Next(gradeLevel.Count)];
            //if the inputted major is not valid, makes the major undeclared
            if (!courses.Contains(Major.ToLower()) || Major == null)
            {
                person.Major = "undeclared";
            }
            else
            {
                person.Major = Major;
            }
            //initally everyone's photo is set to the same generic photo
            person.Photo = "blank.jpg";
            Roster.Add(person);
            SavingInfo.WriteTo(person); //person.Name, person.CourseList, person.NumberOfCourses.ToString(), person.Grade, person.major, person.Photo);
            return CreatedAtAction(nameof(Get), new { id = person.Name }, person);
        }

         /// <summary>
         /// Adds a new course to specific student.
         /// </summary>
         [HttpPost("{Name}/addcourse")]
         [ProducesResponseType(typeof(Student), StatusCodes.Status201Created)]
         [Produces("application/json")]
         public IActionResult Post(string Name, string CourseName)
         {
             Schedule Course = new Schedule();
             Course.CourseType = CourseName.ToLower();

             //if course is null - add a random course otherwise add specific course to course list
             Schedule[] totalCourses = null;
             Student checkName = Adapter.Check(Name, Roster);

             //if it is a new subject, adds the subject to the courses list
             String newcourse = Adapter.IsolateSubject(CourseName);
             if (Adapter.CheckSubject(courses, newcourse))
             {
                 PostSubj(newcourse);
             }

             if (checkName != null)
             {
                 if (Course == null)
                 {
                     Schedule[] newCourse = Adapter.GetCourses(1, courses);
                     totalCourses = newCourse.Concat(checkName.CourseList).ToArray();
                     //checkName.CourseList//Add(getCourses(1));// = getCourses(1);
                 }
                 //Making sure no more than 10 classes per person
                 else if (checkName.NumberOfCourses >= 10)
                 {
                     return BadRequest("Reached max number of courses");
                 }
                 //Making sure not adding a course already on person's schedule
                 else if (Adapter.CheckCourses(checkName, CourseName) == false)
                 {
                     return Ok(CourseName + " is already on " + Name + "'s schedule");
                 }
                 else
                 {
                     Schedule[] newCourse = {Course};
                     totalCourses = newCourse.Concat(checkName.CourseList).ToArray();
                    
                 }
                 checkName.CourseList = totalCourses;
                 checkName.NumberOfCourses += 1;
                 SavingInfo.UpdateFile(Roster);
                 return CreatedAtAction(nameof(Get), new { id = checkName.CourseList }, checkName);
             }
             return NotFound(Name + " not registered");
         }

         /// <summary>
         /// Add x number of courses to specific student
         /// </summary>
         [HttpPost("{Name}, {Number_of_Courses}")]
         public IActionResult PostSubjNum(string Name, int Number_of_Courses) // ticket JSON (POST) -> /api/v1/tickets
         {
             Student checkName = Adapter.Check(Name, Roster);
             //Making sure not adding classes so that student has more than 10 classes
             if (checkName.NumberOfCourses + Number_of_Courses > 10)
             {
                 Number_of_Courses = 10 - checkName.NumberOfCourses;
             }
             if (checkName != null)
             {
                 Schedule[] newCourse = Adapter.GetCourses(Number_of_Courses, courses);
                 Schedule[] totalCourses = newCourse.Concat(checkName.CourseList).ToArray();
                 checkName.CourseList = totalCourses;
                 checkName.NumberOfCourses += Number_of_Courses;
                 SavingInfo.UpdateFile(Roster);
                 return CreatedAtAction(nameof(Get), new { id = checkName.CourseList }, checkName);
             }
             return NotFound(Name + " not registered.");
         }

         /// <summary>
         /// Add subject to list of subjects.
         /// </summary>
         [HttpPost("{Subject}/addsubject")]
         [ProducesResponseType(typeof(Student), StatusCodes.Status201Created)]
         [ProducesResponseType(StatusCodes.Status204NoContent)]
         [ProducesResponseType(typeof(Student), StatusCodes.Status409Conflict)]
         [Produces("application/json")]
         public IActionResult PostSubj(string Subject) 
         {
             Subject = Subject.ToLower();
             if (Adapter.CheckSubject(courses, Subject) == false)
             { 
                     return Ok(Subject + " already exists as a subject");
             }
             courses.Add(Subject);
             return Ok(courses);
         }

         /// <summary>
         /// Gets all students' names and schedules..
         /// </summary>
         [HttpGet]
         public IActionResult Get() 
         {
             return Ok(Roster);
         }

         /// <summary>
         /// Get all students of specific grade level.
         /// </summary>
         [HttpGet ("{Grade_Level}/getGradeLevel")]
         public IActionResult GetClassmates(string Grade_Level)
         {
             Grade_Level = Grade_Level.ToLower();
             if (!gradeLevel.Contains(Grade_Level))
             {
                 return BadRequest("Not a valid grade level.");
             }
             List <String> classmates = new List <String> ();
             int count = Roster.Count;
             for (var i = 0; i < count; i++)
             {
                 Student student = Roster[i];
                 if (student.Grade.Equals(Grade_Level))
                 {
                    classmates.Add (student.Name);
                 }
             }
             return Ok(classmates);
         }

         /// <summary>
         /// Get list of students of specific major.
         /// </summary>
         [HttpGet("{Major}/getmajor")]
         public IActionResult GetMajors(string Major)
         {
             Major = Major.ToLower();
             if (!courses.Contains(Major))
             {
                 return BadRequest(Major + " is not a valid major.");
             }
             List<String> classmates = new List<String>();
             int count = Roster.Count;
             for (var i = 0; i < count; i++)
             {
                 Student student = Roster[i];
                 if (student.Major.Equals(Major))
                 {
                     classmates.Add(student.Name);
                 }
             }
             return Ok(classmates);
         }
        
         /// <summary>
         /// Find Student's Schedule
         /// </summary>
         [HttpGet("{Name}/getname")]
         public IActionResult Get(string Name)
         {
             Student checkName = Adapter.Check(Name, Roster);
             if (checkName != null)
             {
                 return CreatedAtAction(nameof(Get), new { id = checkName.CourseList }, checkName);
             }
             return NotFound(Name + " not registered.");
         }
        
         /// <summary>
         /// Get list of current subjects offered.
         /// </summary>
         [HttpGet("/getSubjects")]
         public IActionResult GetSubjects()
         {
             return Ok(courses);
        }
        
         /// <summary>
         /// Update Name
         /// </summary>
         [HttpPatch("{Original_Name}, {New_Name}/replacename")]
         [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]
         public IActionResult Patch([FromBody] Student person, string Original_Name, string New_Name)
         {
             Student checkOriginal = Adapter.Check(Original_Name, Roster);
             Student checkNew = Adapter.Check(New_Name, Roster);
             if(checkOriginal != null && checkNew == null)
             {
                checkOriginal.Name = New_Name;
                SavingInfo.UpdateFile(Roster);
                return CreatedAtAction(nameof(Get), new { id = checkOriginal.Name }, checkOriginal);
             }
             if(checkOriginal != null && checkNew != null)
             {
                 return BadRequest("New name, " + New_Name +", already on roster");
             }
             return BadRequest(Original_Name + " not on roster, can't be replaced");
         }

         /// <summary>
         /// Update a course
         /// </summary>
         [HttpPatch("{Name}, {Old_Course}, {New_Course}")]
         [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]
         public IActionResult Patch([FromBody] Student student, string Name, string Old_Course, string New_Course)
         {
             Student checkName = Adapter.Check(Name, Roster);
             if (checkName == null)
             {
                 return NotFound(Name + " not registered.");
             }
             if (checkName != null)
             {
                 if (Adapter.CheckCourses(checkName, New_Course) == false)
                 {
                     return Ok(New_Course + " is already on " + Name +"'s schedule");
                 }
                 for (var i = 0; i < checkName.NumberOfCourses; i++)
                 {
                     if (checkName.CourseList[i].CourseType.Equals(Old_Course))
                     {
                         checkName.CourseList[i].CourseType = New_Course;
                         break;
                     }
                 }
             }
             SavingInfo.UpdateFile(Roster);
             return CreatedAtAction(nameof(Get), new { id = checkName.Name }, checkName); 
         }

         /// <summary>
         /// Change student's grade level.
         /// </summary>
         [HttpPatch("{Name}, {Change_Grade}/replacegrade")]
         [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]
         public IActionResult Patch(string Name, string Change_Grade)
         {
             Student checkName = Adapter.Check(Name, Roster);
             if (checkName == null )
             {
                 return BadRequest(Name + " not on roster, can't be replaced");
             }
             for (var i = 0;i<4; i++)
             {
                 if (gradeLevel[i].Equals(Change_Grade))
                 {
                    checkName.Grade = Change_Grade;
                    SavingInfo.UpdateFile(Roster);
                    return CreatedAtAction(nameof(Get), new { id = checkName.Name }, checkName); 
                 }
             }
             return BadRequest("Not a valid grade level");
         }

         /// <summary>
         /// Change student's .major
         /// </summary>
         [HttpPatch("{Name}, {New_Major}/replacemajor")]
         [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]
         public IActionResult PatchMajor(string Name, string New_Major)
         {
             Student checkName = Adapter.Check(Name, Roster);
             if (checkName == null)
             {
                 return BadRequest("Name not on roster, can't be replaced");
             }
             if (courses.Contains(New_Major.ToLower()))
             {
                 checkName.Major = New_Major;
                SavingInfo.UpdateFile(Roster);
                return CreatedAtAction(nameof(Get), new { id = checkName.Name }, checkName);
             }
             checkName.Major = "undeclared";
            return BadRequest("Not a valid major");
         }

         /// <summary>
         /// Deletes all students and schedules.
         /// </summary>
         [HttpDelete()]
         public IActionResult Delete([FromBody] Student person)
         {
             Roster.Clear();
             SavingInfo.DeleteAll();
             return Ok(Roster);
         }

         /// <summary>
         /// Deletes specific student.
         /// </summary>
         [HttpDelete("{Name}/deleteperson")]
         [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]
         public IActionResult Delete([FromBody] Student person, string Name)
         { 
             //if name in the list, remove it, if name not in list, return 
             int count = Roster.Count;
             for (var i = 0; i < count; i++)
             {
                 Student student = Roster[i];
                 if (student.Name == Name)
                 {
                    Roster.RemoveAt(i);
                    SavingInfo.UpdateFile(Roster);
                    return Ok(Roster);

                 }
             }
             return NotFound(Name + " not on roster"); 
         }

         /// <summary>
         /// Deletes specific course for student
         /// </summary>
         [HttpDelete("{Name}, {CourseName}/deletecourse")]
         [ProducesResponseType(typeof(Student), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]
         public IActionResult Delete(string Name, string CourseName) // ticket JSON (POST) -> /api/v1/tickets
         {
             Student checkName = Adapter.Check(Name, Roster);

             if (checkName == null)
             {
                 return NotFound("Not a name on the roster");
             }
             if (checkName.CourseList.Length == 0)
             {
                 return CreatedAtAction(nameof(Get), new { id = checkName.CourseList }, checkName);
             }
             int j = 0;
             int index = checkName.NumberOfCourses;
             Schedule[] newCourseList = new Schedule[index-1];
             Boolean exists = false;

             if (checkName != null)
             {
                 if (index == 0)
                 {
                    return NotFound("Not a course this student is taking");
                 }
                 for (var i = 0; i < index; i++)
                 {
                    if (checkName.CourseList[i].CourseType.Equals(CourseName))
                    {
                         exists = true;
                         checkName.NumberOfCourses -= 1;
                         continue;
                    }
                    if (j == (index - 1))
                    {
                        return NotFound("Not a course this student is taking");
                    }
                    newCourseList[j] = (checkName.CourseList)[i];
                     j+=1;
                 }
                if (exists)
                {
                     checkName.CourseList = newCourseList;
                }
             }
             SavingInfo.UpdateFile(Roster);
             return CreatedAtAction(nameof(Get), new { id = checkName.CourseList }, checkName);
         }

         /// <summary>
         /// Deletes x number of courses for student
         /// </summary>
         [HttpDelete("{Name}, {Num_of_Courses}/deletenumcourses")]
         public IActionResult DeleteNum(string Name, int Num_of_Courses) // ticket JSON (POST) -> /api/v1/tickets
         {
             Student checkName = Adapter.Check(Name, Roster);

             if (checkName == null)
             {
                 return NotFound(Name + " not registered.");
             }
             int index = checkName.NumberOfCourses - Num_of_Courses;
             if (index <= 0)
             {
                 checkName.CourseList = new Schedule[0];
                 checkName.NumberOfCourses = 0;
                SavingInfo.UpdateFile(Roster);
                return CreatedAtAction(nameof(Get), new { id = checkName.CourseList }, checkName);
             }
             Schedule[] newCourseList = new Schedule[index];

             for (var i = 0; i < index; i++)
             {
                 newCourseList[i] = checkName.CourseList[i];
             }
             checkName.CourseList = newCourseList;
             checkName.NumberOfCourses = index;
            SavingInfo.UpdateFile(Roster);
            return CreatedAtAction(nameof(Get), new { id = checkName.CourseList }, checkName);
         }


         /// <summary>
         /// Returns list of registered students in alphabetical order
         /// </summary>
         [HttpGet("/alphabetical")]
         public IActionResult GetAlphabetical()
         {
             List<String> alpha = new List<String>();
             for (var i = 0; i < Roster.Count; i++)
             {
                 Student student = Roster[i];
                 alpha.Add(student.Name +", " +student.Grade + ", " + student.Major);
             }
             alpha.Sort();
             return Ok(alpha);
         }

         /// <summary>
         /// Organizes students by grade
         /// </summary>
         [HttpGet("/bygrade")]
         public IActionResult GetByGrade()
         {
             Dictionary<string, List<String>> grades = new Dictionary<string, List<String>>();
             grades.Add("freshman", new List<string> ());
             grades.Add("sophmore", new List<string>());
             grades.Add("junior", new List<string>());
             grades.Add("senior", new List<string>());
             for (var i = 0; i < Roster.Count; i++)
             {
                 Student student = Roster[i];
                 string grade_level = student.Grade;
                 grades[grade_level].Add(student.Name + ", " +student.Major);                   

             }
             return Ok(grades);
         }

         /// <summary>
         /// Organizes students by major
         /// </summary>
         [HttpGet("/bymajor")]
         public IActionResult GetByMajor()
         {
             Dictionary<string, List<String>> majors = new Dictionary<string, List<String>>();
             for (var i = 0; i < courses.Count; i++)
             {
                 majors.Add(courses[i], new List<string>());
             }
             for (var i = 0; i < Roster.Count; i++)
             {
                 Student student = Roster[i];
                 string major = student.Major;
                 majors[major].Add(student.Name + ", " + student.Grade);

             }
             return Ok(majors);
         }

         /// <summary>
         /// Counts number of students total
         /// </summary>
         [HttpGet("/numstudents")]
         public IActionResult GetNumstudents()
         { 
             return Ok(Roster.Count + " student(s) registered total.");
         }
    }
}
