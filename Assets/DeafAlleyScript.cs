using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class DeafAlleyScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombInfo bomb;
    public KMSelectable ModuleSelectable;
    public KMSelectable[] buttons;
    public GameObject[] shapeObjs;
    public GameObject[] collObjs;
    public GameObject cycle;
    private KMAudio.KMAudioRef sound;
    private RaycastHit[] allHit;

    private string[] objNames = new string[] { "deafShapeA", "deafShapeB", "deafShapeC", "deafShapeD", "deafShapeE", "deafShapeF", "deafShapeG", "deafShapeH", "deafShapeI", "deafShapeJ", "deafShapeK", "deafShapeL", "deafShapeM", "deafShapeN", "deafShapeO", "deafShapeP", "deafShapeQ", "deafShapeR", "deafShapeS", "deafShapeT", "deafShapeU", "deafShapeV", "deafShapeW", "deafShapeX", "deafShapeY", "deafShapeZ", "deafShapeAl", "deafShapeBl", "deafShapeCl", "deafShapeDl", "deafShapeEl", "deafShapeFl", "deafShapeGl", "deafShapeHl", "deafShapeIl", "deafShapeJl", "deafShapeKl", "deafShapeMl", "deafShapeNl", "deafShapeOl", "deafShapePl", "deafShapeQl", "deafShapeRl", "deafShapeSl", "deafShapeTl", "deafShapeUl", "deafShapeVl", "deafShapeWl", "deafShapeXl", "deafShapeYl", "deafShapeZl", "deafShape0", "deafShape1", "deafShape2", "deafShape3", "deafShape4", "deafShape5", "deafShape6", "deafShape7", "deafShape8", "deafShape9", "deafShape~", "deafShape`", "deafShape!", "deafShape@", "deafShape#", "deafShape$", "deafShape%", "deafShape^", "deafShape&", "deafShapeAst", "deafShape(", "deafShape)", "deafShape-", "deafShape_", "deafShape+", "deafShape=", "deafShape[", "deafShape]", "deafShape{", "deafShape}", "deafShapeCol", "deafShape;", "deafShape?", "deafShapeLes", "deafShape,", "deafShapeGre", "deafShape.", "deafShapeQMa", "deafShapeFSl", "deafShapeVer", "deafShapeBSl" };
    private string[] correctPresses = new string[] { "ABCD", "ABDC", "ACBD", "ACDB", "ADBC", "ADCB", "BACD", "BADC", "BCAD", "BCDA", "BDAC", "BDCA", "CABD", "CADB", "CBAD", "CBDA", "CDAB", "CDBA", "DABC", "DACB", "DBAC", "DBCA", "DCAB", "DCBA", "AAAA", "BBBB", "CCCC", "DDDD", "ABAB", "ACAC", "ADAD", "BABA", "BCBC", "BDBD", "CACA", "CBCB", "CDCD", "DADA", "DBDB", "DCDC", "ABBB", "BABB", "BBAB", "BBBA", "ACCC", "CACC", "CCAC", "CCCA", "ADDD", "DADD", "DDAD", "DDDA", "BAAA", "ABAA", "AABA", "AAAB", "BCCC", "CBCC", "CCBC", "CCCB", "BDDD", "DBDD", "DDBD", "DDDB", "CAAA", "ACAA", "AACA", "AAAC", "CBBB", "BCBB", "BBCB", "BBBC", "CDDD", "DCDD", "DDCD", "DDDC", "DAAA", "ADAA", "AADA", "AAAD", "DBBB", "BDBB", "BBDB", "BBBD", "DCCC", "CDCC", "CCDC", "CCCD", "AABB", "BBCC", "CCDD", "DDAA" };
    private string[] shapes = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "~", "`", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "_", "+", "=", "[", "]", "{", "}", ":", ";", "“", "‘", "<", ",", ">", ".", "?", "/", "|", "\\" };
    private int selectedShape = -1;
    private string input = "";
    private string lastHit = "";
    private bool playing = false;
    private bool focused = false;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        for (int i = 0; i < objNames.Length; i++)
        {
            objNames[i] = objNames[i] + moduleId;
            collObjs[i].name = objNames[i];
        }
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        ModuleSelectable.OnFocus += delegate () { focused = true; };
        ModuleSelectable.OnDefocus += delegate () { focused = false; };
    }

    void Start () {
        selectedShape = UnityEngine.Random.Range(0, shapes.Length);
        shapeObjs[selectedShape].SetActive(true);
        Debug.LogFormat("[Deaf Alley #{0}] The selected shape is {1}", moduleId, shapes[selectedShape]);
        if (selectedShape == 92)
        {
            Debug.LogFormat("[Deaf Alley #{0}] There is no correct order to press the regions in, press any region to solve the module", moduleId);
        }
        else
        {
            Debug.LogFormat("[Deaf Alley #{0}] The correct order to press the regions in is {1}", moduleId, correctPresses[selectedShape]);
        }
    }

    void Update()
    {
        if (tpcycle == null)
        {
            if (focused)
            {
                allHit = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
                List<string> names = new List<string>();
                foreach (RaycastHit hit in allHit)
                {
                    names.Add(hit.collider.name);
                    if (objNames.Contains(hit.collider.name) && !playing)
                    {
                        playing = true;
                        lastHit = hit.collider.name;
                        sound = Audio.PlaySoundAtTransformWithRef("tone", transform);
                    }
                }
                if (!names.Contains(lastHit))
                {
                    playing = false;
                    lastHit = "";
                    if (sound != null)
                        sound.StopSound();
                }
            }
            else
            {
                playing = false;
                lastHit = "";
                if (sound != null)
                    sound.StopSound();
            }
        }
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true)
        {
            pressed.AddInteractionPunch();
            if (pressed == buttons[0])
            {
                input += "A";
            }
            else if (pressed == buttons[1])
            {
                input += "B";
            }
            else if (pressed == buttons[2])
            {
                input += "C";
            }
            else if (pressed == buttons[3])
            {
                input += "D";
            }
            if (selectedShape == 92)
            {
                Debug.LogFormat("[Deaf Alley #{0}] Pressed region {1}, Module Disarmed!", moduleId, input.Last());
                shapeObjs[selectedShape].SetActive(false);
                if (sound != null)
                    sound.StopSound();
                moduleSolved = true;
                GetComponent<KMBombModule>().HandlePass();
            }
            else if (input == correctPresses[selectedShape].Substring(0, input.Length))
            {
                Debug.LogFormat("[Deaf Alley #{0}] Pressed region {1}, which is correct", moduleId, input.Last());
                if (input.Length == 4)
                {
                    Debug.LogFormat("[Deaf Alley #{0}] All regions have been pressed in the correct order, Module Disarmed!", moduleId);
                    shapeObjs[selectedShape].SetActive(false);
                    if (sound != null)
                        sound.StopSound();
                    moduleSolved = true;
                    GetComponent<KMBombModule>().HandlePass();
                }
            }
            else
            {
                Debug.LogFormat("[Deaf Alley #{0}] Pressed region {1}, which is incorrect. Strike! Module Resetting...", moduleId, input.Last());
                GetComponent<KMBombModule>().HandleStrike();
                input = "";
                shapeObjs[selectedShape].SetActive(false);
                Start();
            }
        }
    }

    //twitch plays
    private Coroutine tpcycle;

    private IEnumerator cycleHoriz()
    {
        cycle.SetActive(true);
        float t = 0f;
        while (t < 2f && !reachedEnd(0))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.07f, 0.0138f, 0.07f), new Vector3(0.05f, 0.0138f, 0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.07f, 0.0138f, 0.05f);
        t = 0f;
        while (t < 2f && !reachedEnd(0))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.07f, 0.0138f, 0.05f), new Vector3(0.07f, 0.0138f, 0.05f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.07f, 0.0138f, 0.03f);
        t = 0f;
        while (t < 2f && !reachedEnd(0))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.07f, 0.0138f, 0.03f), new Vector3(0.07f, 0.0138f, 0.03f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.07f, 0.0138f, 0.01f);
        t = 0f;
        while (t < 2f && !reachedEnd(0))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.07f, 0.0138f, 0.01f), new Vector3(0.07f, 0.0138f, 0.01f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.07f, 0.0138f, -0.01f);
        t = 0f;
        while (t < 2f && !reachedEnd(0))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.07f, 0.0138f, -0.01f), new Vector3(0.05f, 0.0138f, -0.01f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.07f, 0.0138f, -0.03f);
        t = 0f;
        while (t < 2f && !reachedEnd(0))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.07f, 0.0138f, -0.03f), new Vector3(0.07f, 0.0138f, -0.03f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.07f, 0.0138f, -0.05f);
        t = 0f;
        while (t < 2f && !reachedEnd(0))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.07f, 0.0138f, -0.05f), new Vector3(0.07f, 0.0138f, -0.05f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.07f, 0.0138f, -0.07f);
        t = 0f;
        while (t < 2f && !reachedEnd(0))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.07f, 0.0138f, -0.07f), new Vector3(0.07f, 0.0138f, -0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        tpcycle = null;
        if (sound != null)
            sound.StopSound();
        cycle.SetActive(false);
    }

    private IEnumerator cycleVert()
    {
        cycle.SetActive(true);
        float t = 0f;
        while (t < 2f && !reachedEnd(1))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.07f, 0.0138f, 0.07f), new Vector3(-0.07f, 0.0138f, -0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.05f, 0.0138f, 0.07f);
        t = 0f;
        while (t < 2f && !reachedEnd(1))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.05f, 0.0138f, 0.07f), new Vector3(-0.05f, 0.0138f, -0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.03f, 0.0138f, 0.07f);
        t = 0f;
        while (t < 2f && !reachedEnd(1))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.03f, 0.0138f, 0.07f), new Vector3(-0.03f, 0.0138f, -0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(-0.01f, 0.0138f, 0.07f);
        t = 0f;
        while (t < 2f && !reachedEnd(1))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(-0.01f, 0.0138f, 0.07f), new Vector3(-0.01f, 0.0138f, -0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(0.01f, 0.0138f, 0.07f);
        t = 0f;
        while (t < 2f && !reachedEnd(1))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(0.01f, 0.0138f, 0.07f), new Vector3(0.01f, 0.0138f, -0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(0.03f, 0.0138f, 0.07f);
        t = 0f;
        while (t < 2f && !reachedEnd(1))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(0.03f, 0.0138f, 0.07f), new Vector3(0.03f, 0.0138f, -0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(0.05f, 0.0138f, 0.07f);
        t = 0f;
        while (t < 2f && !reachedEnd(1))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(0.05f, 0.0138f, 0.07f), new Vector3(0.05f, 0.0138f, -0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        cycle.transform.localPosition = new Vector3(0.07f, 0.0138f, 0.05f);
        t = 0f;
        while (t < 2f && !reachedEnd(1))
        {
            cycle.transform.localPosition = Vector3.Lerp(new Vector3(0.07f, 0.0138f, 0.05f), new Vector3(0.07f, 0.0138f, -0.07f), t);
            t += Time.deltaTime * 0.3f;
            yield return null;
        }
        tpcycle = null;
        if (sound != null)
            sound.StopSound();
        cycle.SetActive(false);
    }

    private bool reachedEnd(int type)
    {
        if (type == 0)
        {
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.05f, 0.0138f, 0.07f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.07f, 0.0138f, 0.05f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.07f, 0.0138f, 0.03f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.07f, 0.0138f, 0.01f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.05f, 0.0138f, -0.01f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.07f, 0.0138f, -0.03f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.07f, 0.0138f, -0.05f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.07f, 0.0138f, -0.07f)) < 0.001f)
            {
                return true;
            }
        }
        else if (type == 1)
        {
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(-0.07f, 0.0138f, -0.07f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(-0.05f, 0.0138f, -0.07f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(-0.03f, 0.0138f, -0.07f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(-0.01f, 0.0138f, -0.07f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.01f, 0.0138f, -0.07f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.03f, 0.0138f, -0.07f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.05f, 0.0138f, -0.07f)) < 0.001f)
            {
                return true;
            }
            if (Vector3.Distance(cycle.transform.localPosition, new Vector3(0.07f, 0.0138f, -0.07f)) < 0.001f)
            {
                return true;
            }
        }
        return false;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} cycle horizontal/horiz/h [Feels the module horizontally from left to right and top to bottom] | !{0} cycle vertical/vert/v [Feels the module vertically from top to bottom and left to right] | !{0} press ABCD [Presses the regions in reading order]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*cycle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                if (Regex.IsMatch(parameters[1], @"^\s*horizontal\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[1], @"^\s*horiz\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[1], @"^\s*h\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    if (tpcycle != null)
                    {
                        StopCoroutine(tpcycle);
                        tpcycle = null;
                        if (sound != null)
                            sound.StopSound();
                        cycle.transform.localPosition = new Vector3(-0.07f, 0.0138f, 0.07f);
                    }
                    tpcycle = StartCoroutine(cycleHoriz());
                }
                else if (Regex.IsMatch(parameters[1], @"^\s*vertical\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[1], @"^\s*vert\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[1], @"^\s*v\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    if (tpcycle != null)
                    {
                        StopCoroutine(tpcycle);
                        tpcycle = null;
                        if (sound != null)
                            sound.StopSound();
                        cycle.transform.localPosition = new Vector3(-0.07f, 0.0138f, 0.07f);
                    }
                    tpcycle = StartCoroutine(cycleVert());
                }
                else
                {
                    yield return "sendtochaterror The specified way to feel the module '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify whether you wish to feel the module horizontally or vertically!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                string[] chars = new string[] { "A", "B", "C", "D" };
                for (int i = 0; i < parameters[1].Length; i++)
                {
                    if (!chars.Contains(parameters[1].ToUpper()[i].ToString()))
                    {
                        yield return "sendtochaterror The region to press '" + parameters[1][i] + "' is invalid!";
                        yield break;
                    }
                }
                if (parameters[1].Length > 4)
                {
                    yield return "sendtochaterror No more than 4 regions can be pressed in one command!";
                    yield break;
                }
                for (int i = 0; i < parameters[1].Length; i++)
                {
                    buttons[Array.IndexOf(chars, parameters[1].ToUpper()[i].ToString())].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify which region(s) to press!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return ProcessTwitchCommand("press " + correctPresses[selectedShape].Substring(input.Length, 4-input.Length));
    }
}