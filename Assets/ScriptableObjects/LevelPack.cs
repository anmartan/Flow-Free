using UnityEngine;


[CreateAssetMenu(menuName = "Flow/Level pack", order = 1)]
public class LevelPack : ScriptableObject
{
	[Tooltip("Nombre del package")]
	public string packName;

	[Tooltip("Archivo que contiene los niveles")]
	public TextAsset levels;
}
