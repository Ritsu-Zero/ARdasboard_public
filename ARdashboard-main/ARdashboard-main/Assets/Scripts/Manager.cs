using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public StudentDashboard DashboardPrefab;
    public GameObject Glasses;
    public List<Student> students_list;
    
    public List<Vector3> coords;
    private bool isUpdatingStudentData = false;
    float updateTime;
    float updateInterval = 10f;

    void Start()
    {
        //list of coordinates for the positions of the students
        coords = new List<Vector3>();

        coords.Add(new Vector3(-1f, -0.5f, 2f));
        coords.Add(new Vector3(0f, -0.5f, 2f));
        coords.Add(new Vector3(2f, -0.5f, 2f));
        coords.Add(new Vector3(3f, -0.5f, 2f));
        coords.Add(new Vector3(-1f, -0.5f, 4f));
        coords.Add(new Vector3(0f, -0.5f, 4f));
        coords.Add(new Vector3(2f, -0.5f, 4f));
        coords.Add(new Vector3(3f, -0.5f, 4f));
        coords.Add(new Vector3(-1f, -0.5f, 6f));
        coords.Add(new Vector3(0f, -0.5f, 6f));
        coords.Add(new Vector3(2f, -0.5f, 6f));
        coords.Add(new Vector3(3f, -0.5f, 6f));

        GetStudentData();
        updateTime = 0f;

    }

    // Update is called once per frame
    void Update()
    {
        //retrieve informations and update dasboards every 10 seconds
        updateTime += Time.deltaTime;
        if(updateTime > updateInterval)
        {
            updateTime = 0;
            if (students_list != null && !isUpdatingStudentData)
            {
                StartCoroutine(UpdateStudentDataCoroutine());
                CalcEloOffset();
                sync_dashboard();
                recommender_system();

            }
        }
        
    }

    
    private IEnumerator UpdateStudentDataCoroutine()
    {
        Debug.Log("Updating student started...");
        isUpdatingStudentData = true;
        yield return UpdateStudentData();
        isUpdatingStudentData = false;
        Debug.Log("Updating student Data completed");
    }

    //create dasboards for each student of a list
    private void init_student_dashboard(List<Student> _list)
    {
        students_list = new List<Student>();

        for ( int i = 0; i < coords.Count; i++)
        {
            students_list.Add(_list[i]);
            students_list[i].Dashboard = Instantiate(DashboardPrefab);
            students_list[i].Dashboard.transform.position = coords[i];
            students_list[i].Dashboard._studentID = students_list[i].id;
            students_list[i].Dashboard.LookAt = Glasses;
        }
    }
    
    //fetch the students from the API and call the method to create their dashboards
    private async void GetStudentData()
    {
        List<Student> _list;
        _list = await API.RetrieveInitData();

        if (_list == null)
        {
            Debug.Log("No data available from the API.");
            var dashboard = Instantiate(DashboardPrefab);
            dashboard.LookAt = Glasses;
            dashboard.transform.position = new Vector3(0f, 0f, 3f);
            dashboard.progress_status = 0;
            dashboard.default_text.text = "no connection to API";

            return;
        }

        init_student_dashboard(_list);
    }

    //update the student data from the API
    private async Task UpdateStudentData()
    {      
        await API.UpdateExerciseData(students_list);
    }

    //calculate the average Elo score of the class for each exercise 
    private List<double> CalcEloAverageClass()
    {      
        List<Double> exercise_elo_total = new List<double>();
        //Enumerate the total elo score for each exercises
        foreach (Student _student in students_list)
        {
            if (_student.Exercises != null)
            {
                for (int i = 0; i < _student.Exercises.Count; i++)
                {
                    double _elo = _student.Exercises[i].elo;
                    if (exercise_elo_total.Count-1 < i)
                    {
                        exercise_elo_total.Add(_elo);
                    }
                    else
                    {
                        exercise_elo_total[i] += _elo;
                    }
                }
            }           
        }
        //calculate the class average elo score of each exercises
        for(int j = 0; j < exercise_elo_total.Count; j++)
        {
            exercise_elo_total[j] = exercise_elo_total[j] / students_list.Count;
        }

        return exercise_elo_total;       
    }

    //calculate the offset of the Elo score to the average Elo score of the class for each exercise of each students
    private void CalcEloOffset()
    {
        List<double> avg_elo_class = CalcEloAverageClass();
       
        if (avg_elo_class != null)
        {
            foreach (Student _student in students_list)
            {
                if (_student.Exercises != null && _student.Exercises.Count > 0)
                {
                    int _current = _student.Exercises.Count - 1;
                    _student.Exercises[_current].elo_offset = _student.Exercises[_current].elo - avg_elo_class[_current];
                }
            }
               
        }
        
    }

    //sync the dashboards of each students with the fetched data
    private void sync_dashboard()
    {
        foreach (Student student in students_list)
        {
            student.syncDashboard();
        }
    }

    //the reccommender system determining the progress each students based on their relative performance to the class
    private void recommender_system()
    {
        foreach(Student _student in students_list)
        {
            double offset = _student.getEloOffset();
            if (offset < -20)
            {
                _student.progress_status = 3;
                _student.Dashboard.NeedHelp = true;
            }              
            else if (offset >= 20)
                _student.progress_status = 2;
            else if (offset >= -20 && offset < 20)
                _student.progress_status = 1;
            else
                _student.progress_status = 0;

        }
    }

}
