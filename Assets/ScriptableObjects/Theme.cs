using UnityEngine;


[CreateAssetMenu(menuName = "Flow/Theme", order = 2)]
public class Theme : ScriptableObject
{
	[Tooltip("Nombre del package")]
	public string themeName;

	[Tooltip("Archivo que contiene los niveles")]
	public Color[] colors;
}
