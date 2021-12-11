using UnityEngine;


[CreateAssetMenu(menuName = "Flow/Level pack", order = 1)]
public class LevelPack : ScriptableObject
{
	[Tooltip("Pack name that will appear in the level selection menu.")]
	public string packName;

	[Tooltip("File that contains the levels for this pack.")]
	public TextAsset levels;
}