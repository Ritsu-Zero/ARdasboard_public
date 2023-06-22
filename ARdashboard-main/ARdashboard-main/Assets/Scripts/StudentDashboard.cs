using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class StudentDashboard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

{
    public int _studentID;
    public GameObject LookAt, DefaultScreen, ExtendedScreen;
    public bool ShowExplanationMark;
    public bool NeedHelp;
    private float cooldownTimer = 0f;
    private bool isOnCooldown = false;

    public TMP_Text default_text;
    private Image[] extended_screens;
    public TMP_Text[] screen1, screen2, screen3, screen4;

    public string progress;
    public string category;
    public double elo_offset; 
    public int exercise_progress;
    public double difficulty_level;
    private bool IsOnTarget;
    public int progress_status;


    // Start is called before the first frame update
    void Start()
    {
        
        IsOnTarget = false;
        //instantiate the screens and it's components
        default_text = DefaultScreen.GetComponentInChildren<TMP_Text>();
        default_text.text = "OK";
        extended_screens = ExtendedScreen.GetComponentsInChildren<Image>();

        screen1 = extended_screens[0].GetComponentsInChildren<TMP_Text>();
        screen1[0].text = "Difficulty";

        screen2 = extended_screens[1].GetComponentsInChildren<TMP_Text>();
        screen2[0].text = "Category:";

        screen3 = extended_screens[2].GetComponentsInChildren<TMP_Text>();
        screen3[0].text = "Ex. done:";

        screen4 = extended_screens[3].GetComponentsInChildren<TMP_Text>();
        screen4[0].text = "Elo offset:";

        //Extended screen is not active at default
        ExtendedScreen.SetActive(false);
        progress_status = 0;

    }

    // Update is called once per frame
    void Update()
    {
        //dashboard will always be facing the camera
        Vector3 target = LookAt.transform.position;
        target.y = transform.position.y;
        transform.rotation = Quaternion.LookRotation(transform.position - target);

        //If it is determined that a student need help and hasn't been helped in the last 5 min, show the indicator
        if (NeedHelp && !isOnCooldown)
        {
            ShowExplanationMark = true;

            //indicator disappears when the extended screen is opened
            if (ExtendedScreen.activeSelf)
            {
                ShowExplanationMark = false;
                isOnCooldown = true;
            }
        }
        //cooldown of 5 minutes after dashboard is approached when the indicator was active
        else if (isOnCooldown)
        {
            if (cooldownTimer <= 300)
            {
                cooldownTimer += Time.deltaTime;
            }

            else if (cooldownTimer > 300)
            {
                cooldownTimer = 0;
                isOnCooldown = false;
            }
        }
        
        //update the information to the screens
        update_default_screen();
        update_extended_screen();
 
    }

    //update the display of the default screen based on the information that the server has relayed to the dashboard
    private void update_default_screen()
    {
        var _canvas = DefaultScreen.GetComponent<CanvasGroup>();
        Image[] _image = DefaultScreen.GetComponentsInChildren<Image>();
        
            
        //When the dashboard is in direct sight of the camera, the dashboards gets highlighted. Otherwise it is not highlighted and transparent
        if (IsOnTarget)
        {
            _image[1].enabled = true;
            _canvas.alpha = 1;
        }
        else
        {
            _image[1].enabled = false;
            _canvas.alpha = 0.6f;
        }

        //if it is detemined that a student needs help, display the indicator and change the dashboard to non-transparent
        if (ShowExplanationMark)
        {
            _image[2].enabled = true;
            _canvas.alpha = 1;
        }
        else
        {
            _image[2].enabled = false;
        }

        //the dashboard has 4 different progress status states which gets determined in the manager class, this method update the display to the associated state
        switch (progress_status)
        {
            case 0: //standby
                default_text.text = "-";
                _image[3].enabled = false;
                _image[4].enabled = false;
                _image[5].enabled = false;
                break;
            case 1: //average, show average symbol
                default_text.text = "";
                _image[3].enabled = true;
                _image[4].enabled = false;
                _image[5].enabled = false;
                break;
            case 2: //good, show thumbs up
                default_text.text = "";
                _image[3].enabled = false;
                _image[4].enabled = true;
                _image[5].enabled = false;
                break;
            case 3: //not so good, show thumbs down
                default_text.text = "";
                _image[3].enabled = false;
                _image[4].enabled = false;
                _image[5].enabled = true;
                break;

        }    

    }

    //update the display of the extended screen based on the information that the server has relayed to the dashboard
    private void update_extended_screen()
    {
        screen1[1].text = (difficulty_level / 0.25).ToString("0.");
        screen2[1].text = category;
        screen3[1].text = exercise_progress.ToString();
        screen4[1].text = elo_offset.ToString("0.##");
    }

    //by clicking on the dashboard, switch between default screen and extended screen
    public void OnPointerClick(PointerEventData eventData)
    {
        
        if (ExtendedScreen.activeSelf)
        {
            ExtendedScreen.SetActive(false);
            DefaultScreen.SetActive(true);
        }
        else
        {
            ExtendedScreen.SetActive(true);
            DefaultScreen.SetActive(false);
        }
    }

    //when pointer hover over the dashboard, highlight it
    public void OnPointerEnter(PointerEventData eventData)
    {
        IsOnTarget = true;
    }

    //when pointer exit hover, turn off the highlight
    public void OnPointerExit(PointerEventData eventData)
    {
        IsOnTarget = false;
    }
}
