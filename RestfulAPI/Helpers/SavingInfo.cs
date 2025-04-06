using System;
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

public class SavingInfo
{

    public static string GetPath()
    {
        string fileName = "Full_Roster.csv";
        FileInfo f = new FileInfo(fileName);
        return f.FullName;

    }

    public static void UpdateFile(List<Student> list)
    {
        DeleteAll();
        for (var i = 0; i < list.Count; i++)
        {
            WriteTo(list[i]);
        }
    }

    public static string Parse(Student student, string name)
    {
        name = student.Name;
        var schedule = "";
        var num = student.NumberOfCourses;
        for (var i = 0; i < num; i++)
        {
            schedule += (student.CourseList[i].CourseType) + "//";
        }
        var grade = student.Grade;
        var major = student.Major;
        var photo = student.Photo;
        var newLine = string.Format("{0},{1},{2},{3},{4},{5}", name, schedule, num, grade, major, photo);
        return newLine;
    }

    public static void WriteTo(Student student)
    {
        var csv = new StringBuilder();
        csv.AppendLine(Parse(student, student.Name));
        string path = GetPath();
        File.AppendAllText(path, csv.ToString());
    }

    public static void Writting(Student person, string Name)
    {
        string path = GetPath();
        string[] lines = File.ReadAllLines(path);
        for (var i = 0; i < lines.Length; i++)
        {
            var fields = lines[i].Split(",");
            if (fields[0].Equals(person.Name))
            {
                if (Name.Equals(person.Name))
                {
                    lines[i] = Parse(person, person.Name);
                }
                else
                {
                    File.AppendAllText(path, "lines");
                    lines[i] = Parse(person, Name);
                }
                File.WriteAllLines(path, lines);
                break;
            }
        }
    }

    public static void Deletion(Student person)
    {
        string path = GetPath();
        string[] lines = File.ReadAllLines(path);
        for (var i = 0; i < lines.Length; i++)
        {
            var fields = lines[i].Split(",");
            if (fields[0].Equals(person.Name))
            {
                int newlength = lines.Length - 1;
                lines[i] = lines[newlength];

                //lines = lines.Where((val, idx) => idx != lines.Length -1).ToArray();
                string[] finallines = lines[0..newlength];//new string[lines.Length-1];
                                                          // Array.Copy(lines, 0, finallines, 0, lines.Length - 1);

                File.WriteAllLines(path, finallines);
                break;
            }
        }
    }

    public static void DeleteAll()
    {
        string path = GetPath();
        File.WriteAllText(path, String.Empty);
    }

    public static List<Student> updatedroster()
    {
        var _roster = new List<Student>();
        string path = GetPath();
        using (TextFieldParser parser = new TextFieldParser(path))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                int i = 0;
                Student oldstudent = new Student();
                foreach (string field in fields)
                {
                    if (i == 0) oldstudent.Name = field;
                    else if (i == 1)
                    {
                        var splitstring = field.Split("//");
                        Schedule[] listofcourses = Enumerable.Range(0, splitstring.Length - 1).Select(index => new Schedule
                        {
                            CourseType = splitstring[index]

                        }).ToArray();
                        oldstudent.CourseList = listofcourses;
                    }
                    else if (i == 2) oldstudent.NumberOfCourses = Int32.Parse(field);
                    else if (i == 3) oldstudent.Grade = field;
                    else if (i == 4) oldstudent.Major = field;
                    else if (i == 5) oldstudent.Photo = field;
                    i++;
                }
                _roster.Add(oldstudent);
            }
        }
        return _roster;
    }
}
