using UnityEngine;

[CreateAssetMenu(fileName = "IntegerLibrary", menuName = "ScriptableObjects/IntegerLibrary", order = 1)]
public class IntegerLibrary : ScriptableObject
{
	[SerializeField]
	private int[] vals;

	public int this[int i] => vals[i];

	public int Length => vals.Length;
}
