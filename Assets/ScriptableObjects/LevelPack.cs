using UnityEngine;

namespace FlowFree
{
	[CreateAssetMenu(menuName = "Flow/Level pack", order = 1)]
	public class LevelPack : ScriptableObject
	{
		[Tooltip("Pack name that will appear in the level selection menu.")]
		public string packName; // Pack name that will appear in the level selection menu.

		[Tooltip("File that contains the levels for this pack.")]
		public TextAsset levels; // File that contains the levels for this pack.

		[Tooltip(
			"Whether this pack is blocked, meaning, only the first level that has not been finished is available.")]
		public bool
			blocked; // Whether this pack is blocked, meaning, only the first level that has not been finished is available.
	}
}
