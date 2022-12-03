using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateTex2D_Window : EditorWindow
{
    private ComputeShader compute;
    private RenderTexture renderTexture;

    public int resolution = 256;
    public Vector2 offset = Vector2.zero;
    public float amplitude = 1f;
    public float frequency = 1;
    public int octaves = 1;
    public float threshholdBot = 0;
    public float threshholdTop = 0;

    NoiseTypes noise;
    public enum NoiseTypes
    {
        Classic, Simplex
        //FlowField, Veroni, Worly, Cellular, Warped
    }

    EffectTypes effect;
    public enum EffectTypes
    {
        Default, Abs, AbsLayers
    }

    [MenuItem("Tools/Generate Noise Texture")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<GenerateTex2D_Window>("Generate Texture");
    }

    void OnEnable()
    {
        if(compute==null)
        {
            compute = (ComputeShader)Resources.Load("GenerateNoiseCompute", typeof(ComputeShader));
        }

        if (renderTexture==null)
        {
            renderTexture = new RenderTexture(resolution, resolution, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
    }
    
    void OnDisable()
    {
        if(renderTexture != null)
            DestroyImmediate(renderTexture);
    }

    public void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(true);
        compute = EditorGUILayout.ObjectField(new GUIContent ("Compute Shader"), compute, typeof (ComputeShader), false) as ComputeShader;
        EditorGUI.EndDisabledGroup();

        resolution = EditorGUILayout.IntField("Texture Resolution", resolution);
        using ( new GUILayout.HorizontalScope() ) 
        {
            GUILayout.Label("Noise Type");
            noise = (NoiseTypes)EditorGUILayout.EnumPopup(noise);
        }

        using ( new GUILayout.HorizontalScope() ) 
        {
            GUILayout.Label("Effect Type");
            effect = (EffectTypes)EditorGUILayout.EnumPopup(effect);
        }
        GUILayout.Space(20);

        // amplitude = EditorGUILayout.FloatField("Amplitude", amplitude);
        frequency = EditorGUILayout.FloatField("Frequency", frequency);
        offset = EditorGUILayout.Vector2Field("Offset", offset);

        threshholdBot = EditorGUILayout.FloatField("Bottom Threshhold", threshholdBot);
        threshholdTop = EditorGUILayout.FloatField("Top Threshhold", threshholdTop);
        GUILayout.Space(20);

        octaves = EditorGUILayout.IntField("Octaves", octaves);
    
        if(GUILayout.Button("Save Texture"))
        {
            SavePNG();
        }

        if(compute != null)
        {
            Compute((int)noise);
            GUILayout.Space(20);
            GUILayout.Label(renderTexture);
        }
    }

    public void Compute(int type)
    {
        compute.SetTexture(0, "Result", renderTexture);
        compute.SetFloat("_Resolution", resolution);

        compute.SetVector("_Offset", offset);
        compute.SetFloat("_Amplitude", amplitude);
        compute.SetFloat("_Frequency", frequency);
        compute.SetInt("_Octaves", octaves);

        compute.SetInt("_NoiseID", (int)noise);
        compute.SetInt("_EffectID", (int)effect);

        compute.SetFloat("_ThreshholdBottom", threshholdBot);
        compute.SetFloat("_ThreshholdTop", threshholdTop);

        compute.Dispatch(0, renderTexture.width/8, renderTexture.height/8, 1);
    }

    public void SavePNG()
    {
        if(renderTexture == null)
            return;

        string filePath = Application.dataPath + "/Textures/Noise/";
        var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath + "NewTexture.png");
        if(!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        RenderTexture.active = renderTexture;
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
       
        File.WriteAllBytes(uniqueFileName, bytes);
        DestroyImmediate(tex);

        AssetDatabase.Refresh();
        Debug.Log(filePath + uniqueFileName);
    }
}
