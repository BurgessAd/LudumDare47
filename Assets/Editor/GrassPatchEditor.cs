using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class GrassPatchEditor : EditorWindow
{
	private GrassPatchComponent m_CurrentGrassPatch;

	private GameObject grassObjectPrefab;

	private StateMachine m_StateMachine;

	private SceneView m_CurrentViewingWindow;

	private static GrassPatchEditor window;


    [MenuItem("Window/Grass Patch Editor")]
    public static void ShowWindow() 
    {
		GetWindow(typeof(GrassPatchEditor));
	}

	private void OnEnable()
	{
		m_StateMachine = new StateMachine(new StateIdle(this));
		m_StateMachine.AddState(new StateDefiningSize(this));
		m_StateMachine.AddState(new StateDrawingTexture(this));
		m_StateMachine.InitializeStateMachine();
		SceneView.duringSceneGui += OnSceneGUI;
		window = this;
	}

	private void OnDisable()
	{
		SceneView.duringSceneGui -= OnSceneGUI;
		window = null;
	}

	private void OnSceneGUI(SceneView sv) 
	{
		int i = 0;
		m_CurrentViewingWindow = sv;
		m_StateMachine.LateTick();
	}
	private void OnGUI()
	{
		grassObjectPrefab = (GameObject)EditorGUILayout.ObjectField(grassObjectPrefab, typeof(GameObject), false);
		if (m_CurrentGrassPatch != null)
		{
			// grass patch not null, active object might or might not be - doesnt matter, if its changed, return to idle state.
			if (Selection.activeGameObject != m_CurrentGrassPatch.gameObject)
			{
				m_StateMachine.RequestTransition(typeof(StateIdle));
			}
		}
		else if (Selection.activeGameObject)
		{
			// grass was null, currently have an object
			if (Selection.activeGameObject.TryGetComponent(out GrassPatchComponent grass))
			{
				m_CurrentGrassPatch = grass;
				m_StateMachine.RequestTransition(typeof(StateIdle));
			}
		}

		m_StateMachine.Tick();
		HandleUtility.Repaint();
	}
	public GrassPatchComponent GetCurrentGrassPatch => m_CurrentGrassPatch;

	public SceneView GetCurrentSceneView => m_CurrentViewingWindow;

	public void CreateNewGrassObject() 
	{
		GameObject go = Instantiate(grassObjectPrefab);
		m_CurrentGrassPatch = go.GetComponent<GrassPatchComponent>();
		Selection.activeGameObject = go;
	}


	public Rect GenerateScreenSpaceRect()
	{
		Vector3 edgeA = GetCurrentGrassPatch.GrassGenerationBounds.Item1;
		Vector3 edgeB = GetCurrentGrassPatch.GrassGenerationBounds.Item2;

		Vector3 bottomLeft = new Vector3(Mathf.Min(edgeA.x, edgeB.x), (edgeA.y + edgeB.y) / 2, Mathf.Min(edgeA.z, edgeB.z));
		Vector3 topRight = new Vector3(Mathf.Max(edgeA.x, edgeB.x), (edgeA.y + edgeB.y) / 2, Mathf.Max(edgeA.z, edgeB.z));

		Vector3 bottomLeftScreenSpace = GetCurrentSceneView.camera.WorldToScreenPoint(bottomLeft);
		Vector3 topRightScreenSpace = GetCurrentSceneView.camera.WorldToScreenPoint(topRight);
		Vector3 size = topRightScreenSpace - bottomLeftScreenSpace;

		return new Rect(bottomLeftScreenSpace.x, bottomLeftScreenSpace.y, size.x, size.y);
	}

}

public class StateIdle : AStateBase 
{
	private string m_CurrentGrassName = "New Grass Object";
	private readonly GrassPatchEditor host;
	public StateIdle(GrassPatchEditor editor) 
	{
		m_CurrentGrassName = "New Grass Object";
		host = editor;
	}

	public override void Tick()
	{
		using (new GUILayout.HorizontalScope()) 
		{
			m_CurrentGrassName = GUILayout.TextField(m_CurrentGrassName);
			if (GUILayout.Button("Add New Grass Object"))
			{
				host.CreateNewGrassObject();
			}
		}

		using (new EditorGUI.DisabledScope(host.GetCurrentGrassPatch == null))
		{
			if (GUILayout.Button("Define Grass Bounds"))
			{
				RequestTransition<StateDefiningSize>();
			}
		}
		using (new EditorGUI.DisabledScope(host.GetCurrentGrassPatch == null || !host.GetCurrentGrassPatch.HasBoundsDefined))
		{
			if (GUILayout.Button("Paint Grass Density Map"))
			{
				RequestTransition<StateDrawingTexture>();
			}
			if (GUILayout.Button("Build Grass Mesh"))
			{
				host.GetCurrentGrassPatch.CreateGrassFromParams();
			}
		}
	}
}

public class StateDefiningSize : AStateBase 
{
	private readonly GrassPatchEditor host;

	private Vector3 m_GrassPatchSizeDefinitionStartAnchor;
	private Vector3 m_GrassPatchSizeDefinitionEndAnchor;
	private SpriteRenderer m_SpriteRenderer;
	private int m_CurrentDefinition;
	
	public StateDefiningSize(GrassPatchEditor editor)
	{
		host = editor;
	}

	public override void OnEnter()
	{
		m_CurrentDefinition = 0;
	}
	bool m_bIsMouseDown = false;
	public override void LateTick() 
	{
		Handles.BeginGUI();

		switch (m_CurrentDefinition) 
		{
			case (0):
				Handles.DrawWireCube(m_GrassPatchSizeDefinitionStartAnchor, Vector3.one / 4);
				break;
			case (1):
				Handles.DrawWireCube(m_GrassPatchSizeDefinitionStartAnchor, Vector3.one / 4);
				Handles.DrawWireCube(m_GrassPatchSizeDefinitionEndAnchor, Vector3.one / 4);

				Vector3 minCorner = Vector3.Min(m_GrassPatchSizeDefinitionStartAnchor, m_GrassPatchSizeDefinitionEndAnchor);
				Vector3 maxCorner = Vector3.Max(m_GrassPatchSizeDefinitionStartAnchor, m_GrassPatchSizeDefinitionEndAnchor);

				Handles.DrawLine(maxCorner, new Vector3(maxCorner.x, maxCorner.y, minCorner.z));
				Handles.DrawLine(new Vector3(maxCorner.x, maxCorner.y, minCorner.z), minCorner);
				Handles.DrawLine(minCorner, new Vector3(minCorner.x, minCorner.y, maxCorner.z));
				Handles.DrawLine(new Vector3(minCorner.x, minCorner.y, maxCorner.z), maxCorner);
				break;
		}
		Handles.EndGUI();
	}

	public override void Tick()
	{
		Event e = Event.current;
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlID);
		GUIUtility.hotControl = controlID;
		EventType type = e.GetTypeForControl(controlID);

		if (type == EventType.MouseDown)
		{
			m_bIsMouseDown = true;
			e.Use();
		}

		if (type == EventType.MouseUp)
		{
			m_bIsMouseDown = false;
			e.Use();
		}

		if (type == EventType.KeyDown && e.keyCode == KeyCode.Escape) 
		{
			RequestTransition<StateIdle>();
			return;
		}

		Vector3 mousePos = Event.current.mousePosition;
		mousePos.y = host.GetCurrentSceneView.camera.pixelHeight - mousePos.y;

		Ray ray = host.GetCurrentSceneView.camera.ScreenPointToRay(mousePos);

		if (type == EventType.MouseDown && Event.current.button == 0)
		{
			if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, host.GetCurrentGrassPatch.GrassGenerationLayer, QueryTriggerInteraction.Ignore))
			{
				m_GrassPatchSizeDefinitionStartAnchor = hit.point;
				m_CurrentDefinition = 1;
				m_SpriteRenderer = host.GetCurrentGrassPatch.CreateGrassPaintVisualizer(m_GrassPatchSizeDefinitionStartAnchor).GetComponent<SpriteRenderer>();
			}
		}


		if (m_CurrentDefinition == 1) 
		{
			Vector3 planeOrigin = m_GrassPatchSizeDefinitionStartAnchor;
			Vector3 planeNormal = Vector3.up;
			m_GrassPatchSizeDefinitionEndAnchor = ray.origin + ray.direction * (Mathf.Abs( Vector3.Dot(ray.direction, ray.origin - planeOrigin) 
																						/ (Vector3.Dot(ray.direction, planeNormal)) ));

			host.GetCurrentGrassPatch.UpdateGrassSizeVisualizer(m_SpriteRenderer, m_GrassPatchSizeDefinitionStartAnchor, m_GrassPatchSizeDefinitionEndAnchor);
			if (type == EventType.MouseUp && Event.current.button == 0) 
			{
				host.GetCurrentGrassPatch.GrassGenerationBounds = new Tuple<Vector3, Vector3>(m_GrassPatchSizeDefinitionStartAnchor, m_GrassPatchSizeDefinitionEndAnchor);
				Rect imageRect = host.GenerateScreenSpaceRect();

				// hard-code 256 x 256 maximum size texture for longest axis

				float longestAxialLength = Mathf.Max(imageRect.width, imageRect.height);
				Vector2Int texSize = new Vector2Int((int)(256 * imageRect.width / longestAxialLength), (int)(256 * imageRect.height / longestAxialLength));
				host.GetCurrentGrassPatch.GrassMap = new Texture2D(Mathf.Abs(texSize.x), Mathf.Abs(texSize.y));
				Color[] pixels = host.GetCurrentGrassPatch.GrassMap.GetPixels();
				for (int i = 0; i < pixels.Length; i++)
				{
					pixels[i].r = 0;
					pixels[i].g = 0;
					pixels[i].b = 0;
					pixels[i].a = 1;
				}
				host.GetCurrentGrassPatch.GrassMap.SetPixels(pixels);
				host.GetCurrentGrassPatch.GrassMap.Apply();
				RequestTransition<StateIdle>();
			}
		}
		e.Use();
	}

	public override void OnExit()
	{
		host.GetCurrentGrassPatch.DeleteGrassPaintVisualizer(m_SpriteRenderer.gameObject);
	}
}

public class StateDrawingTexture : AStateBase 
{
	private readonly GrassPatchEditor host;
	private GrassPatchComponent grassPatchComponent;
	private float brushSize = 1f;
	private float brushStrength = 0.5f;
	private float brushHardness = 0.0f;
	private BrushType brushType = BrushType.Add;
	private Channel brushChannel = Channel.Density;
	private float textureOpacity = 0.5f;
	private SpriteRenderer m_SpriteRenderer;

	private readonly string[] channelChoices = { "Density", "Height" };
	private readonly string[] brushChoices = { "Add", "Remove" };

	private enum BrushType 
	{
		Add,
		Remove
	}

	private enum Channel 
	{
		Density,
		Height
	}

	public StateDrawingTexture(GrassPatchEditor editor)
	{
		host = editor;
	}

	public override void OnEnter()
	{
		grassPatchComponent = host.GetCurrentGrassPatch;
		m_bIsMouseDown = false;
		m_SpriteRenderer = host.GetCurrentGrassPatch.CreateGrassPaintVisualizer(host.GetCurrentGrassPatch.GrassGenerationBounds.Item1).GetComponent<SpriteRenderer>();
		grassPatchComponent.UpdateGrassPaintVisualizer(m_SpriteRenderer);
	}

	public override void OnExit()
	{
		grassPatchComponent.DeleteGrassPaintVisualizer(m_SpriteRenderer.gameObject);
		base.OnExit();
	}

	bool m_bIsMouseDown = false;
	public override void Tick()
	{
		Event e = Event.current;
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		EventType type = e.GetTypeForControl(controlID);

		if ((type == EventType.KeyDown && e.keyCode == KeyCode.Escape) || GUILayout.Button("Finish"))
		{
			RequestTransition<StateIdle>();
			return;
		}
		
		brushType = (BrushType)EditorGUILayout.Popup((int)brushType, brushChoices);
		brushChannel = (Channel)EditorGUILayout.Popup((int)brushChannel, channelChoices);

		using (new GUILayout.HorizontalScope()) 
		{
			GUILayout.Label("Texture Opacity", GUILayout.Width(100.0f));
			GUILayout.Label(textureOpacity.ToString(), GUILayout.Width(40.0f));
			textureOpacity = GUILayout.HorizontalSlider(textureOpacity, 0.0f, 1.0f);
		}
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label("Brush Size", GUILayout.Width(100.0f));
			GUILayout.Label(brushSize.ToString(), GUILayout.Width(40.0f));
			brushSize = GUILayout.HorizontalSlider(brushSize, 0.0f, 100.0f);
		}
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label("Brush Strength", GUILayout.Width(100.0f));
			GUILayout.Label(brushStrength.ToString(), GUILayout.Width(40.0f));
			brushStrength = GUILayout.HorizontalSlider(brushStrength, 0.0f, 2.0f); ;
		}

		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label("Brush Hardness", GUILayout.Width(100.0f));
			GUILayout.Label(brushHardness.ToString(), GUILayout.Width(40.0f));
			brushHardness = GUILayout.HorizontalSlider(brushHardness, 0.0f, 2.0f); ;
		}

		// tex is nested into window and window is nested in screen

		float width = host.GetCurrentGrassPatch.GrassMap.width;
		float height = host.GetCurrentGrassPatch.GrassMap.height;

		float maxSize = 600;
		float scalar = Mathf.Min(maxSize / width, maxSize / height);
		Vector2 texSize = scalar * new Vector2(width, height);


		Rect rect = new Rect((Screen.width/2 - texSize.x / 2), (Screen.height/2 - texSize.y / 2), texSize.x, texSize.y);
		GUI.DrawTexture(rect, host.GetCurrentGrassPatch.GrassMap);

		Vector2 screenMousePoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - host.position.position;

		if (type == EventType.MouseDown)
		{
			m_bIsMouseDown = true;
			e.Use();
		}

		if (type == EventType.MouseUp)
		{
			m_bIsMouseDown = false;
			e.Use();
		}

		if (rect.Contains(screenMousePoint, true))
		{
			Vector2 pixelPositionClickPoint = (screenMousePoint - rect.position) / scalar;
			pixelPositionClickPoint.y = height - pixelPositionClickPoint.y;
			Debug.Log(pixelPositionClickPoint);

			if (m_bIsMouseDown)
			{
				Vector2 lowerBrushBound = pixelPositionClickPoint - new Vector2(brushSize / 2, brushSize / 2);
				Vector2 upperBrushBound = pixelPositionClickPoint + new Vector2(brushSize / 2, brushSize / 2);

				lowerBrushBound.x = Mathf.Clamp(lowerBrushBound.x, 0, texSize.x-1);
				upperBrushBound.x = Mathf.Clamp(upperBrushBound.x, 0, texSize.x-1);

				lowerBrushBound.y = Mathf.Clamp(lowerBrushBound.y, 0, texSize.y - 1);
				upperBrushBound.y = Mathf.Clamp(upperBrushBound.y, 0, texSize.y - 1);


				float multiplier = brushType == BrushType.Add ? 1 : -1;
				Color[] gridColors = grassPatchComponent.GrassMap.GetPixels();

				// tex is row by row, so x first, then y
				// and the ID should be pixelHeight * rowWidth + pixelWidth
				for (int i = (int)lowerBrushBound.x; i < (int)upperBrushBound.x; i++)
				{
					for (int j = (int)lowerBrushBound.y; j < (int)upperBrushBound.y; j++)
					{
						Vector2 centreOffset = new Vector2(i, j) - pixelPositionClickPoint;

						float sqDistFromCentre = centreOffset.sqrMagnitude;

						if (sqDistFromCentre <= brushSize * brushSize)
						{
							float brushInfluence = multiplier * Mathf.Clamp01(brushStrength * (1 - Mathf.Sqrt(sqDistFromCentre) / brushSize));
							int colorId = i + j * (int)width;
							switch (brushChannel)
							{
								case (Channel.Density):
									gridColors[colorId].r = Mathf.Clamp01(gridColors[colorId].r + brushInfluence);
									break;
								case (Channel.Height):
									gridColors[colorId].g = Mathf.Clamp01(gridColors[colorId].g + brushInfluence);
									break;
							}

						}
					}
				}
				grassPatchComponent.GrassMap.SetPixels(gridColors);
				grassPatchComponent.GrassMap.Apply();

				grassPatchComponent.UpdateGrassPaintVisualizer(m_SpriteRenderer);
			}
		}
	}
}
