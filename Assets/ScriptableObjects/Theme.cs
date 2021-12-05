using UnityEngine;


[CreateAssetMenu(menuName = "Flow/Theme", order = 2)]
public class Theme : ScriptableObject
{
	[Tooltip("Name for the theme, which will appear when the user chooses a theme.")]
	public string themeName;

	[Tooltip("Colors that define the theme, which will be used for the flows in the levels.")]
	public Color[] colors;
}
