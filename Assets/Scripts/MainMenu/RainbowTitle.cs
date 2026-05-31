using UnityEngine;
using TMPro;

public class RainbowTitle : MonoBehaviour
{
    [SerializeField] private float hueShiftSpeed = 0.08f;
    [SerializeField] private float hueSpread = 0.12f;

    private TMP_Text _tmp;
    private float _hueOffset;

    private void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        _tmp.ForceMeshUpdate();
        TMP_TextInfo textInfo = _tmp.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            float hue = (_hueOffset + i * hueSpread) % 1f;
            Color32 color = Color.HSVToRGB(hue, 0.9f, 1f);

            int meshIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Color32[] verts = textInfo.meshInfo[meshIndex].colors32;
            verts[vertexIndex + 0] = color;
            verts[vertexIndex + 1] = color;
            verts[vertexIndex + 2] = color;
            verts[vertexIndex + 3] = color;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            _tmp.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }

        _hueOffset = (_hueOffset + hueShiftSpeed * Time.deltaTime) % 1f;
    }
}