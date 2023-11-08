using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
	
	VisualElement root, buildMenu;
	UIDocument uiDocument;
	[SerializeField] BuildableManager buildables;
	[SerializeField] VisualTreeAsset buildMenuItem;
	
	[SerializeField] Buildable buildable;
	
	
	void OnEnable() 
	{
		uiDocument = GetComponent<UIDocument>();
		root = uiDocument.rootVisualElement;
		CreateBuildMenu();
	}

	private void Click(ClickEvent evt, string buildableName)
	{
		Debug.Log("Building " + buildableName);
		buildable.SetPrefab(buildableName);
	}
	
	
	void CreateBuildMenu() 
	{
		buildMenu = root.Q("build-menu");

		foreach(var buildable in buildables.Prefabs) 
		{
			VisualElement item = buildMenuItem.Instantiate();
			item.Q<Label>("buildable-name").text = buildable.name;
			item.Q<IMGUIContainer>("buildable-sprite").RegisterCallback<ClickEvent, string>(Click, buildable.name);

			
			buildMenu.Add(item);
		}
	}
}
