using UnityEngine;

[CreateAssetMenu(fileName = "MaterialLibrary", menuName = "ScriptableObjects/MaterialLibrary", order = 1)]
public class MaterialLibrary : ScriptableObject
{
	[SerializeField]
	private Material[] colors;

	[SerializeField]
	private Material[] uiColors;

	public Material this[int i] => colors[i];

	public int Length => colors.Length;

	public Material GetUIColor(int index)
	{
		return uiColors[index];
	}
}
