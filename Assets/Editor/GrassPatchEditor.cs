﻿using System.Collections;
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
		GameObject go = Instantiate(grassObjectPrefab, Vector3.zero, Quaternion.identity, null);
		m_CurrentGrassPatch = go.GetComponent<GrassPatchComponent>();
		Selection.activeGameObject = go;
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
	private Vector3 m_MousePos;
	
	public StateDefiningSize(GrassPatchEditor editor)
	{
		host = editor;
	}

	public override void OnEnter()
	{
		m_CurrentDefinition = 0;
	}

	public override void Tick()
	{
		Event e = Event.current;
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlID);
		GUIUtility.hotControl = controlID;
		EventType type = e.GetTypeForControl(controlID);

		if (type == EventType.KeyDown && e.keyCode == KeyCode.Escape) 
		{
			RequestTransition<StateIdle>();
			return;
		}

		if (type == EventType.MouseMove || type == EventType.MouseDrag || type == EventType.MouseDown) 
		{
			m_MousePos = Event.current.mousePosition;
			m_MousePos.y = host.GetCurrentSceneView.camera.pixelHeight - m_MousePos.y;
		}

		Ray ray = host.GetCurrentSceneView.camera.ScreenPointToRay(m_MousePos);

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
			m_GrassPatchSizeDefinitionEndAnchor = ray.origin + ray.direction * ( Vector3.Dot(planeNormal, planeOrigin - ray.origin) 
																						/ (Vector3.Dot(ray.direction, planeNormal)));

			host.GetCurrentGrassPatch.UpdateGrassSizeVisualizer(m_SpriteRenderer, m_GrassPatchSizeDefinitionStartAnchor, m_GrassPatchSizeDefinitionEndAnchor);
			if (type == EventType.MouseUp && Event.current.button == 0) 
			{
				host.GetCurrentGrassPatch.GrassGenerationBounds = new Tuple<Vector3, Vector3>(m_GrassPatchSizeDefinitionStartAnchor, m_GrassPatchSizeDefinitionEndAnchor);

				Vector3 edgeA = host.GetCurrentGrassPatch.GrassGenerationBounds.Item1;
				Vector3 edgeB = host.GetCurrentGrassPatch.GrassGenerationBounds.Item2;

				Vector3 bottomLeft = new Vector3(Mathf.Min(edgeA.x, edgeB.x), (edgeA.y + edgeB.y) / 2, Mathf.Min(edgeA.z, edgeB.z));
				Vector3 topRight = new Vector3(Mathf.Max(edgeA.x, edgeB.x), (edgeA.y + edgeB.y) / 2, Mathf.Max(edgeA.z, edgeB.z));

				Vector3 size = topRight - bottomLeft;
				// hard-code 256 x 256 maximum size texture for longest axis

				float longestAxialLength = Mathf.Max(size.x, size.z);
				float rescale = 256 / longestAxialLength;
				Vector2Int texSize = new Vector2Int((int)(rescale * size.x), (int)(rescale * size.z));

				host.GetCurrentGrassPatch.GrassMap = new Texture2D(Mathf.Abs(texSize.x), Mathf.Abs(texSize.y));
				Color32[] pixels = host.GetCurrentGrassPatch.GrassMap.GetPixels32();
				for (int i = 0; i < pixels.Length; i++)
				{
					pixels[i] = Color.black;
				}
				host.GetCurrentGrassPatch.GrassMap.SetPixels32(pixels);
				host.GetCurrentGrassPatch.GrassMap.Apply();
				RequestTransition<StateIdle>();
			}
		}
		e.Use();
	}

	public override void OnExit()
	{
		if (host.GetCurrentGrassPatch && m_SpriteRenderer)
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
		if (grassPatchComponent && m_SpriteRenderer)
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
			brushSize = GUILayout.HorizontalSlider(brushSize, 20.0f, 400.0f);
		}
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label("Brush Strength", GUILayout.Width(100.0f));
			GUILayout.Label(brushStrength.ToString(), GUILayout.Width(40.0f));
			brushStrength = GUILayout.HorizontalSlider(brushStrength, 0.0f, 0.5f); ;
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
		float RectPixelsPerTexPixel = Mathf.Min(maxSize / width, maxSize / height);
		Vector2 texSize = RectPixelsPerTexPixel * new Vector2(width, height);


		Rect rect = new Rect((Screen.width / 2 - texSize.x / 2), (Screen.height / 2 - texSize.y / 2), texSize.x, texSize.y);
		GUI.DrawTexture(rect, host.GetCurrentGrassPatch.GrassMap);

		float brushSize_PixelSpace = brushSize / RectPixelsPerTexPixel;
		float halfBrushSize_PixelSpace = brushSize_PixelSpace / 2;


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
			Vector2 pixelPositionClickPoint = (screenMousePoint - rect.position) / RectPixelsPerTexPixel;
			pixelPositionClickPoint.y = height - pixelPositionClickPoint.y;

			if (m_bIsMouseDown)
			{
				Vector2 lowerBrushBound = pixelPositionClickPoint - new Vector2(halfBrushSize_PixelSpace, halfBrushSize_PixelSpace);
				Vector2 upperBrushBound = pixelPositionClickPoint + new Vector2(halfBrushSize_PixelSpace, halfBrushSize_PixelSpace);

				lowerBrushBound.x = Mathf.Clamp(lowerBrushBound.x, 0, width);
				upperBrushBound.x = Mathf.Clamp(upperBrushBound.x, 0, width);

				lowerBrushBound.y = Mathf.Clamp(lowerBrushBound.y, 0, height);
				upperBrushBound.y = Mathf.Clamp(upperBrushBound.y, 0, height);


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

						if (sqDistFromCentre <= halfBrushSize_PixelSpace * halfBrushSize_PixelSpace)
						{
							float brushInfluence = multiplier * Mathf.Clamp01(brushStrength * (1 - Mathf.Sqrt(sqDistFromCentre) / halfBrushSize_PixelSpace));
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
