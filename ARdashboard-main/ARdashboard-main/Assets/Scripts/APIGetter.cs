using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;

//student class object that stores all information about a student's progress, including its dashboard
public class Student
{
    public string name { get; set; }
    public int id { get; set; }

    public int progress_status { get; set; }

    public List<Exercise> Exercises { get; set; }

    public StudentDashboard Dashboard { get; set; }

    //sync the variables with its dashboard
    public void syncDashboard()
    {
        if (Dashboard != null) 
        {
            if (Exercises != null && Exercises.Count != 0)
            {
                Exercise _current = Exercises.Last();
                Dashboard.category = _current.lp_description;
                Dashboard.elo_offset = _current.elo_offset;
                Dashboard.exercise_progress = _current.nExercises;
                Dashboard.difficulty_level = _current.difficulty_level;
                Dashboard.progress_status = progress_status;
            }
        }
             
    }

    //return the elo offset score of the last exercise
    public double getEloOffset()
    {
        if (Exercises != null && Exercises.Count != 0)
        {
            Exercise _current = Exercises.Last();
            return _current.elo_offset;
        }

        return 0;
                
    }
}
//Exercise object class 
public class Exercise
{
    public double difficulty_level { get; set; }
    public List<double> difficulty_path { get; set; }
    public double elo { get; set; }
    public List<int> elo_path { get; set; }
    public int goal { get; set; }
    public int goal_set { get; set; }
    public string lp_description { get; set; }
    public int nExercises { get; set; }
    public string objective_id { get; set; }
    public string phase { get; set; }
    public string student_id { get; set; }
    public int type { get; set; }
    public double elo_offset { get; set; }
}

public class Response
{
    public List<Student> Students { get; set; }
}

//The API class that handles the connection to https://www.leerpaden.nl/start_simulation/class_id=ansjovis3
public class API
{
    private static readonly HttpClient api_client = new HttpClient();

    //method to test the connection to the server
    static async Task _init(string[] args)
    {
        var students = await RetrieveInitData();
        if (students == null)
        {
            Console.WriteLine("The API contains no data");
            return;
        }

        if (students != null)
        {
            displaydetails(students);
        }
        else
        {
            Console.WriteLine("Failed to retrieve additional data from API abou the exercises the students made.");
        }
        while (true)
        {
            Console.WriteLine("Press 'Enter' to fetch updated data or type 'exit' to exit the program:");
            string userInput = Console.ReadLine();

            if (userInput.ToLower() == "exit")
            {
                break;
            }


            students = await UpdateExerciseData(students);


            displaydetails(students);
        }
    }

    //This method retrieves initial data by making an asynchronous HTTP GET request to the URL "https://www.leerpaden.nl/start_simulation/class_id=ansjovis3".
    public static async Task<List<Student>> RetrieveInitData()
    {
        var api_resp = await api_client.GetAsync("https://www.leerpaden.nl/start_simulation/class_id=ansjovis3");

        if (api_resp.IsSuccessStatusCode)
        {
            var response_string = await api_resp.Content.ReadAsStringAsync();
            Response api_re = JsonConvert.DeserializeObject<Response>(response_string);
            return api_re.Students;
        }

        return null;
    }

    //This method takes a list of students and updates their exercise data asynchronously
    public static async Task<List<Student>> UpdateExerciseData(List<Student> students)
    {
        List<Task<Student>> student_exercises = new List<Task<Student>>();

        foreach (var student in students)
        {
            student_exercises.Add(GetStudentDetails(student));
            
        }

        return (await Task.WhenAll(student_exercises)).ToList();
    }
    //this method  retrieves additional details and exercises for a given student.
    static async Task<Student> GetStudentDetails(Student student)
    {
        var api_resp = await api_client.GetAsync($"https://www.leerpaden.nl/get_simulation/user_id={student.id}");

        if (api_resp.IsSuccessStatusCode)
        {
            var response_exercises = await api_resp.Content.ReadAsStringAsync();
            List<Exercise> exercises = JsonConvert.DeserializeObject<List<Exercise>>(response_exercises);
            student.Exercises = exercises;
        }
        else
        {
            Console.WriteLine($"Failed to retrieve additional information for for student: {student.id}");
        }

        return student;
    }

    //this method display the student details in the console
    static void displaydetails(List<Student> students)
    {
        if (students == null)
        {
            Console.WriteLine("No data available from the API.");
            return;
        }

        foreach (var student in students)
        {
            Console.WriteLine($"Student Name: {student.name}, ID: {student.id}");
            Console.WriteLine("Exercises:");

            if (student.Exercises == null)
            {
                Console.WriteLine("No exercise information available.");
            }
            else
            {
                foreach (var exercise in student.Exercises)
                {
                    Console.WriteLine($"- elo score: {exercise.elo}, LP Description: {exercise.lp_description}");
                }
            }

            Console.WriteLine();
        }
    }
}