using Buttplug;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExampleController : MonoBehaviour
{
    [SerializeField]
    private Button m_LoopButton;

    [SerializeField]
    private Slider m_SliderContoller;

    [SerializeField]
    private GameObject m_LoopingScreenBlocker;

    private Coroutine m_LoopCoroutine;

    private float m_LastSliderPosition;
    private float m_LastSliderCheckTime = -1f;
    private const float MIN_TIME_BETWEEN_CMDS = .1f;

    private void Start()
    {
        // open a connection
        DeviceConnector.Instance.Start();

        m_LoopingScreenBlocker.SetActive(false);
    }

    /// <summary>
    /// Handles the begin of the slider drag
    /// </summary>
    public void HandleSliderBeginDrag()
    {
        // save starting config
        m_LastSliderCheckTime = Time.time;
        m_LastSliderPosition = m_SliderContoller.value;
    }

    /// <summary>
    /// Handles the end of the slider drag
    /// </summary>
    public void HandleSliderEndDrag()
    {
        // check latest updated position
        if (m_SliderContoller.value != m_LastSliderPosition)
        {
            // calculate parameters
            var deltaTime = (Time.time - m_LastSliderCheckTime);
            var speed = Mathf.Abs(m_SliderContoller.value - m_LastSliderPosition) / deltaTime;

            // move device
            DeviceConnector.Instance.IssueStroke(speed, m_SliderContoller.value);
        }
    }

    /// <summary>
    /// Handles changes on the slider
    /// </summary>
    public void HandleSliderValueChange()
    {
        // calculate delta time
        var deltaTime = (Time.time - m_LastSliderCheckTime);

        // don't spam the device
        if (deltaTime < MIN_TIME_BETWEEN_CMDS)
            return;

        // calculate speed
        var speed = Mathf.Abs(m_SliderContoller.value - m_LastSliderPosition) / deltaTime;

        // save latest config
        m_LastSliderPosition = m_SliderContoller.value;
        m_LastSliderCheckTime = Time.time;

        // move device
        DeviceConnector.Instance.IssueStroke(speed, m_SliderContoller.value);
    }

    public void ToggleLoop()
    {
        // return if no device
        if (false == DeviceConnector.Instance.IsConnected)
        {
            // kill any zombie coroutine
            if (m_LoopCoroutine != null)
            {
                StopCoroutine(m_LoopCoroutine);
                m_LoopCoroutine = null;
            }

            // unblock slider conrtoller
            m_LoopingScreenBlocker.SetActive(false);

            return;
        }

        // if device was looping
        if (m_LoopCoroutine != null)
        {
            // stop loop
            StopCoroutine(m_LoopCoroutine);
            m_LoopCoroutine = null;

            // change label
            m_LoopButton.GetComponentInChildren<Text>().text = "START LOOP";

            // unblock slider conrtoller
            m_LoopingScreenBlocker.SetActive(false);
        }
        else
        {
            // block slider conrtoller
            m_LoopingScreenBlocker.SetActive(true);

            // start new loop
            m_LoopCoroutine = StartCoroutine(LoopCoroutine());

            // change label
            m_LoopButton.GetComponentInChildren<Text>().text = "STOP LOOP";
        }
    }

    private IEnumerator LoopCoroutine()
    {
        // direction flag
        var goingUp = true;

        // same duration for going up and down
        var durationInMilliseconds = 693L;

        // position to send the device to
        var position = 0f;

        // duration in seconds to issue a wait cmd
        var durationInSeconds = durationInMilliseconds / 1000.0f;

        // create wait command
        var waitCommand = new WaitForSeconds(durationInSeconds);

        // loop
        while (DeviceConnector.Instance.IsConnected)
        {
            // choose position
            position = goingUp ? .7f : .2f;

            // issue command
            DeviceConnector.Instance.IssueStroke(durationInMilliseconds, position);

            // wait...
            yield return waitCommand;

            // change direction
            goingUp = !goingUp;
        }
    }
}
