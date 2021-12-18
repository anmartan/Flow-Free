using UnityEngine;


namespace FlowFree
{
	[CreateAssetMenu(menuName = "Flow/Level category", order = 0)]
	public class Category : ScriptableObject
	{
		[Tooltip("Name of the category, that will appear in the level selection menu.")]
		public string categoryName;

		[Tooltip("Color for the category when it's shown in the menu.")]
		public Color color;

		[Tooltip("Shade of the _color for the category when it's shown in the menu.")]
		public Color shadeColor;

		[Tooltip("Packs that appear in this category.")]
		public LevelPack[] packs;

	}
}