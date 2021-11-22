using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zork.Common;

public class UnityOutputService : MonoBehaviour, IOutputService
{
    [SerializeField]
    private Transform OutputTextContainer;

    [SerializeField]
    private GameObject NewLinePrefab;

    public UnityOutputService() => mEntries = new List<GameObject>();
    public void Clear() => mEntries.ForEach(entry => Destroy(entry));

    public void Write(object value) => ParseAndWriteLine(value.ToString());

    public void WriteLine(object value) => ParseAndWriteLine(value.ToString());

    public void ParseAndWriteLine(string value)
    {
        WriteTextLine(value);
    }

    private void WriteNewLine()
    {
        var newLine = GameObject.Instantiate(NewLinePrefab);
        newLine.transform.SetParent(OutputTextContainer, false);
        mEntries.Add(newLine.gameObject);
    }

    private void WriteTextLine(string value)
    {
        var newLine = GameObject.Instantiate(NewLinePrefab);
        newLine.transform.SetParent(OutputTextContainer, false);
        newLine.GetComponent<TextMeshProUGUI>().text = value;
        mEntries.Add(newLine.gameObject);
    }

    private readonly List<GameObject> mEntries;
}
