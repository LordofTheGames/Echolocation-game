using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class SwiftF0Runner : MonoBehaviour
{
    public string modelPath = "MicAudioModel/model.onnx";

    private InferenceSession session;
    private string inputName;
    private string outputName;

    void Awake()
    {
        string fullPath = System.IO.Path.Combine(Application.dataPath, modelPath);

        if (!System.IO.File.Exists(fullPath))
        {
            Debug.LogError($"SwiftF0Runner: model.onnx not found at {fullPath}");
            enabled = false;
            return;
        }

        session = new InferenceSession(fullPath);

        inputName = session.InputMetadata.Keys.First();
        outputName = session.OutputMetadata.Keys.First();

    }

    public float Run(float[] audio)
    {
        var tensor = new DenseTensor<float>(
            audio,
            new[] {1,audio.Length}
        );

        var inputs = new List<NamedOnnxValue>{
            NamedOnnxValue.CreateFromTensor(inputName, tensor)
        };

        using var results = session.Run(inputs);
        return results.First().AsEnumerable<float>().First();
    }

    void OnDestroy()
    {
        session?.Dispose();
    }
}