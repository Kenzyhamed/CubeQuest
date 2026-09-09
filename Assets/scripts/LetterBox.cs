using UnityEngine;
using TMPro;
using Unity.Netcode;
public class LetterBox : NetworkBehaviour
{
    [HideInInspector] public string letter;
    public TextMeshProUGUI label;

    [HideInInspector] public string meshName;
    [HideInInspector] public string colorName;
    [HideInInspector] public Mesh OrigMesh;
    [HideInInspector] public Color OrigColor;

    public MeshFilter _mf;
    public MeshRenderer _mr;

    public void Start()
    {
        if (label != null)
            letter = label.text;

        Transform child = transform.Find("GameObject");
        if (child != null)
        {
            _mf = child.GetComponent<MeshFilter>();
            _mr = child.GetComponent<MeshRenderer>();
        }

        OrigMesh  = _mf.mesh;
        OrigColor = _mr.material.color;
        ReadMeshAndColor();

        Debug.Log($"Letter: {letter}, Mesh: {meshName}, Color: {colorName}");
    }

    void Update()
    {
        ReadMeshAndColor();
    }

    void ReadMeshAndColor()
    {
        if (_mf != null && _mf.mesh != null)
            meshName = _mf.mesh.name.ToLower().Replace("instance", "").Trim();

        if (_mr != null)
        {
            Color c = _mr.material.color;
            if (c == Color.red)        colorName = "red";
            else if (c == Color.green) colorName = "green";
            else if (c == Color.blue)  colorName = "blue";
            else                       colorName = "none";
        }
    }
}