using UnityEngine;


[CreateAssetMenu(menuName = "Flow/Level category", order = 0)]
public class LevelCategory : ScriptableObject
{
	[Tooltip("Name of the category, that will appear in the level selection menu.")]
	public string name;

	[Tooltip("Packs that appear in this category.")]
	public LevelPack[] packs;

}
