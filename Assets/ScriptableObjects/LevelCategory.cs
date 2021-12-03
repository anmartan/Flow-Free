using UnityEngine;


[CreateAssetMenu(menuName = "Flow/Level category", order = 0)]
public class LevelCategory : ScriptableObject
{
	// [TODO] arreglar y comentar por favor
	[Tooltip("Archivo que contiene los niveles")]
	public string name;

	[Tooltip("Nombre del package")]
	public LevelPack[] packages;

}
