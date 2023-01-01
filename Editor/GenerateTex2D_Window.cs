using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateTex2D_Window : EditorWindow
{
    private ComputeShader _compute;
    private RenderTexture _renderTexture;

    private int _resolution = 256;
    private int _seed = 69420;

    private int _octaves = 3;
    private float _frequency = 5;
    private float _amplitude = 1f;
    private float _lacunarity = 2;
    private float _gain = 0.5f;

    private float _warpStrength = 0.4f;
    private Vector2 _offset = Vector2.zero;
    private Vector2 _threshhold = new Vector2(0, 0);

    NoiseTypes _noise;
    public enum NoiseTypes
    {
        Classic, ClassicBillow, ClassicRidged,
        Simplex, SimplexBillow, SimplexRidged,
        // Veroni,
        // Worly,

        // FlowField, Cellular, Warped, Analitical Derivative
    }

    EffectTypes _effect;
    public enum EffectTypes
    {
        Default, Abs, OneMinusAbs, Raw, Map, Remap
    }

    [MenuItem("Tools/Generate Noise Texture")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<GenerateTex2D_Window>("Generate Texture");
    }

    void OnEnable()
    {
        if(_compute==null)
        {
            _compute = (ComputeShader)Resources.Load("GenerateNoiseCompute", typeof(ComputeShader));
        }

        if (_renderTexture==null)
        {
            _renderTexture = new RenderTexture(_resolution, _resolution, 24);
            _renderTexture.enableRandomWrite = true;
            _renderTexture.Create();
        }
    }
    
    void OnDisable()
    {
        if(_renderTexture != null)
            DestroyImmediate(_renderTexture);
    }

    public void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(true);
        _compute = EditorGUILayout.ObjectField(new GUIContent ("Compute Shader"), _compute, typeof (ComputeShader), false) as ComputeShader;
        EditorGUI.EndDisabledGroup();

        _resolution = EditorGUILayout.IntField("Texture Resolution", _resolution);
        using ( new GUILayout.HorizontalScope() ) 
        {
            GUILayout.Label("Noise Type");
            _noise = (NoiseTypes)EditorGUILayout.EnumPopup(_noise);
        }

        using ( new GUILayout.HorizontalScope() ) 
        {
            GUILayout.Label("Effect Type");
            _effect = (EffectTypes)EditorGUILayout.EnumPopup(_effect);
        }

        // Display if Remap Effect Selected
        // if(_effect == EffectTypes.Default)
        //     _threshhold = EditorGUILayout.Vector2Field("Threshhold", _threshhold);

        GUILayout.Space(20);

        GUILayout.Label("Noise Parameters");
        _octaves = EditorGUILayout.IntField("Octaves", _octaves);
        // _seed = EditorGUILayout.IntField("Seed", _seed);
        // _amplitude = EditorGUILayout.FloatField("Amplitude", amplitude);
        _frequency = EditorGUILayout.FloatField("Frequency", _frequency);
        _lacunarity = EditorGUILayout.FloatField("Lacunarity", _lacunarity);
        _gain = EditorGUILayout.FloatField("Gain", _gain);
        _warpStrength = EditorGUILayout.FloatField("WarpStrength", _warpStrength);
        _offset = EditorGUILayout.Vector2Field("Offset", _offset);
        GUILayout.Space(20);
    
        if(GUILayout.Button("Save Texture"))
        {
            SavePNG();
        }

        if(_compute != null)
        {
            Compute((int)_noise);
            GUILayout.Space(20);
            GUILayout.Label(_renderTexture);
        }
    }

    public void Compute(int type)
    {
        _compute.SetTexture(0, "Result", _renderTexture);
        _compute.SetFloat("Resolution", _resolution);

        _compute.SetFloat("Seed", _seed);
        _compute.SetInt("Octaves", _octaves);
        
        _compute.SetFloat("Frequency", _frequency);
        _compute.SetFloat("Amplitude", _amplitude);
        _compute.SetFloat("Lacunarity", _lacunarity);
        _compute.SetFloat("Gain", _gain);

        _compute.SetFloat("WarpStrength", _warpStrength);
        _compute.SetVector("Offset", _offset);

        _compute.SetInt("NoiseID", (int)_noise);
        _compute.SetInt("EffectID", (int)_effect);

        _compute.SetVector("Threshhold", _threshhold);

        _compute.Dispatch(0, _renderTexture.width/8, _renderTexture.height/8, 1);
    }

    public void SavePNG()
    {
        if(_renderTexture == null)
            return;

        string filePath = Application.dataPath + "/Textures/";
        if(!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath + "NewTexture.png");

        RenderTexture.active = _renderTexture;
        Texture2D tex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGB24, false);

        tex.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
       
        File.WriteAllBytes(uniqueFileName, bytes);
        DestroyImmediate(tex);

        AssetDatabase.Refresh();
        Debug.Log(uniqueFileName);
    }
}
