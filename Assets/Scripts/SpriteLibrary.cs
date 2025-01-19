using UnityEngine;

[CreateAssetMenu(fileName = "SpriteLibrary", menuName = "ScriptableObjects/SpriteLibrary", order = 1)]
public class SpriteLibrary : ScriptableObject
{
	[SerializeField]
	private Sprite[] sprites;

	public Sprite this[int i] => sprites[i];

	public int Length => sprites.Length;
}
