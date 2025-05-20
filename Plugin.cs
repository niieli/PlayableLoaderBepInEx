using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KinematicCharacterController.Core;
using PlayableLoaderBepInEx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PlayableLoaderBepInEx
{
	[BepInPlugin(PluginGuid, "Playable Loader", "1.0.0")]
	public class PlayableLoaderPlugin : BaseUnityPlugin
	{
		private const string PluginGuid = "nieli.PlayableLoaderBepInEx";
		public static ManualLogSource BepinLogger;
		public static AssetBundle AssetBundle;
		public static GameObject PlayableLoaderMenu;
		private static bool _sibling;
		public static bool IsEnabled;

		public static bool Replaced;

		public static bool HasAnyChanges;

		public static ResourceModel CurrentResource;

		public static Dictionary<string, ResourceModel> ResourceDict = new();

		public static Texture2D DefaultDiffuse;

		public static ResourceModel DefaultResource;

		public static string FlagFilePath;
		public static string SaveFilePath;

		private static bool lastState;
		private Harmony _harmony;
		private static readonly string[] validScenes = ["Public Pool", "Hairball City", "Salmon Creek Forest", "Trash Kingdom", 
			"Tadpole inc", "Home", "The Bathhouse", "GarysGarden"];

		private static readonly StringBuilder LogBatch = new();
		public static bool HasDone = false;

		private void Awake()
		{
			AssetBundle = AssetBundleLoader.LoadEmbeddedAssetBundle("playable");
			BepinLogger = Logger;
			FlagFilePath = Path.Combine(Paths.PluginPath, "data", "lastSelectedEntry");
			SaveFilePath = Path.Combine(Paths.PluginPath, "data", "enabled");
			CurrentResource = null;
			Texture2D dpi = new Texture2D(2, 2);
			dpi.LoadImage(File.ReadAllBytes(Path.Combine(Paths.PluginPath, "data", "defaultpack.png")));
			DefaultResource = new ResourceModel
			{
				Name = "Niko (default)",
				Author = "Frog Vibes",
				Icon = dpi,
				IsDefault = true,
				FolderName = "________default"
			};
			DefaultDiffuse = new Texture2D(2, 2);
			DefaultDiffuse.LoadImage(File.ReadAllBytes(Path.Combine(Paths.PluginPath, "data", "default.png")));
			Refresh();
			SceneManager.sceneLoaded += OnSceneLoaded;
			_harmony = new Harmony(PluginGuid);
			_harmony.PatchAll();
			//InvokeRepeating(nameof(SaveToFiles), 10f, 10f);
		}

		private void Start()
		{
			
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			_sibling = false;
			CreateMenu();
		}

		private void CreateMenu()
		{
			if (!validScenes.Contains(SceneManager.GetActiveScene().name)) return;
			PlayableLoaderMenu = Instantiate(AssetBundle.LoadAsset<GameObject>("CustomSkin"), GameObject.Find("UI").transform, false);
			PlayableLoaderMenu.AddComponent<CustomSkin>();
			PlayableLoaderMenu.SetActive(false);
		}

		public static void Refresh()
		{
			ResourceDict.Clear();
			CurrentResource = null;
			ResourceDict.Add(DefaultResource.FolderName, DefaultResource);
			string last = (File.Exists(FlagFilePath) ? File.ReadAllText(FlagFilePath) : null);
			IsEnabled = (File.Exists(SaveFilePath) && File.ReadAllText(SaveFilePath) == "True");
			string[] directories = Directory.GetDirectories(Path.Combine(Paths.PluginPath, "playables"));
			foreach (string f in directories)
			{
				bool flag = !Directory.Exists(f);
				if (!flag)
				{
					string meta = Path.Combine(f, "info.ini");
					bool flag2 = !File.Exists(meta);
					if (!flag2)
					{
						try
						{
							ResourceModel i = ResourceModel.Parse(meta);
							string j = (i.FolderName = Path.GetFileName(f));
							ResourceDict.Add(j, i);
							BepinLogger.LogInfo(string.Concat("Found resource pack \"", i.Name, "\" for \"", j, "\""));
							bool flag3 = last != null && j.Equals(last, StringComparison.InvariantCultureIgnoreCase);
							if (flag3)
							{
								CurrentResource = i;
							}
							string res = Path.Combine(f, "niko_sprites");
							string newRes = Path.Combine(f, "assets");
							bool flag4 = !Directory.Exists(newRes) && Directory.Exists(res);
							if (flag4)
							{
								BepinLogger.LogInfo("Converting resource pack... " + i.Name);
								ResourceConverter.Convert(res, newRes);
								BepinLogger.LogInfo(" [OK!]");
							}
						}
						catch (Exception e)
						{
							BepinLogger.LogError("Failed to load " + meta);
							BepinLogger.LogError(e);
						}
					}
				}
			}
			bool flag5 = CurrentResource == null;
			if (flag5)
			{
				CurrentResource = DefaultResource;
			}
			Replaced = false;
			if (PlayableLoaderMenu != null)
				if (PlayableLoaderMenu.activeSelf)
				{
					AddButtons();
				}
			if (_sibling)
				CustomSkin.SetCurrentPlayableName($"{CurrentResource.Name} (by {CurrentResource.Author})");
		}

		private static void AddButtons()
		{
			CustomSkin.DestroyButtons();
			foreach (KeyValuePair<string, ResourceModel> r in ResourceDict)
			{
				CustomSkin.AddButton(r.Value.Icon, r.Value.Name, r.Value.Author, r.Value.FolderName).onClick.AddListener(Selected(r.Value));
			}
		}

		private static UnityAction Selected(ResourceModel r)
		{
			return () => {
				BepinLogger.LogInfo("CLICKED: " + r.Name);
				if (CurrentResource == r || !IsEnabled) return;
				CurrentResource = r;
				Replaced = false;
			};
		}

		private void Update()
		{
			if (PlayableLoaderMenu != null)
			{
				if (PlayableLoaderMenu.activeSelf && !_sibling)
				{
					CustomSkin.SetCurrentPlayableName($"{CurrentResource.Name} (by {CurrentResource.Author})");
					PlayableLoaderMenu.transform.SetAsLastSibling();
					_sibling = true;
					AddButtons();
				}
			}
			bool flag = !IsEnabled || !IsGameReady();
			if (!flag)
			{
				bool v = scrTrainManager.instance.isLoadingNewScene;
				bool flag2 = lastState != v;
				if (flag2)
				{
					Replaced = false;
					lastState = v;
				}
				
				bool flag3 = !Replaced && !scrTrainManager.instance.isLoadingNewScene;
				if (flag3)
				{
					PlayerAnimations[] o = FindObjectsOfType<PlayerAnimations>();
					foreach (PlayerAnimations x in o)
					{
						DoReplace(new[]
						{
							((scrAnimateTextureArray)GetInstanceField(typeof(PlayerAnimations), x, "animator")).Bank,
							((scrAnimateTextureArray)GetInstanceField(typeof(PlayerAnimations), x, "animatorFront")).Bank,
							((scrAnimateTextureArray)GetInstanceField(typeof(PlayerAnimations), x, "animatorBack")).Bank,
							((scrAnimateTextureArray)GetInstanceField(typeof(PlayerAnimations), x, "animatorArrayCarry")).Bank
						}, CurrentResource.IsDefault);
					}
				}
			}
		}

		private static bool IsGameReady()
		{
			return scrTrainManager.instance != null;
		}

		private static void DoReplace(SpriteBank[] an, bool reset)
		{
			bool flag = !CurrentResource.IsDefault;
			if (flag)
			{
				HasAnyChanges = true;
			}

			foreach (SpriteBank anims in an)
			{
				BepinLogger.LogInfo("Anims found!");
				AssetReplace("assets", anims, reset);
			}
			Replaced = true;
			Resources.UnloadUnusedAssets();
			GC.Collect();
			File.WriteAllText(FlagFilePath, CurrentResource.FolderName);
			if (PlayableLoaderMenu == null) return;
			if (PlayableLoaderMenu.activeSelf)
				CustomSkin.SetCurrentPlayableName($"{CurrentResource.Name} (by {CurrentResource.Author})");
		}

		public static void OnToggle(bool value)
		{
			IsEnabled = value;
			File.WriteAllText(SaveFilePath, IsEnabled.ToString());
			bool flag = !value;
			if (flag)
			{
				if (CurrentResource.IsDefault) return;
				Replaced = false;
				PlayerAnimations[] o = FindObjectsOfType<PlayerAnimations>();
				foreach (PlayerAnimations x in o)
				{
					DoReplace(new[]
					{
						((scrAnimateTextureArray)GetInstanceField(typeof(PlayerAnimations), x, "animator")).Bank,
						((scrAnimateTextureArray)GetInstanceField(typeof(PlayerAnimations), x, "animatorFront")).Bank,
						((scrAnimateTextureArray)GetInstanceField(typeof(PlayerAnimations), x, "animatorBack")).Bank,
						((scrAnimateTextureArray)GetInstanceField(typeof(PlayerAnimations), x, "animatorArrayCarry")).Bank
					},!IsEnabled || CurrentResource.IsDefault);
				}
			}
		}

		private static object GetInstanceField(Type type, object instance, string fieldName)
		{
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = type.GetField(fieldName, bindFlags);
			return field.GetValue(instance);
		}

		private static void AssetReplace(string context, SpriteBank sb, bool reset)
		{
			LogBatch.Clear();
			bool flag = sb == null;
			if (!flag)
			{
				string root = Path.Combine(Paths.PluginPath, "playables", CurrentResource.FolderName, context);
				string rootD = Path.Combine(Paths.PluginPath, "originalFiles", context);
				string assetName = sb.name.Split('|')[0];
				for (int i = 0; i < sb.Texture.depth; i++)
				{
					LogBatch.AppendLine($"Asset => {assetName}_{i}");
					string file = Path.Combine(root, $"{assetName}_{i}.png");
					string file2 = Path.Combine(rootD, $"{assetName}_{i}.png");
					bool flag2 = !File.Exists(file2);
					if (flag2)
					{
						LogBatch.AppendLine(" -> Dumping resource to file!");
						bool flag3 = !Directory.Exists(rootD);
						if (flag3)
						{
							Directory.CreateDirectory(rootD);
						}
						Texture2D txt2 = new Texture2D(sb.Texture.width, sb.Texture.height);
						txt2.SetPixels32(sb.Texture.GetPixels32(i));
						byte[] bytes = txt2.EncodeToPNG();
						File.WriteAllBytes(file2, bytes);
					}
					bool flag4 = !File.Exists((reset || CurrentResource.IsDefault) ? file2 : file);
					if (!flag4)
					{
						LogBatch.AppendLine(" -> REPLACING anim with one from external file! " + sb.Texture.format);
						Texture2D tex = new Texture2D(sb.Width, sb.Height, sb.Texture.format, true);
						bool flag5 = tex.LoadImage(File.ReadAllBytes((reset || CurrentResource.IsDefault) ? file2 : file));
						if (flag5)
						{
							tex.Compress(true);
							for (int ei = 0; ei < sb.Texture.mipmapCount; ei++)
							{
								Graphics.CopyTexture(tex, 0, Math.Min(tex.mipmapCount, ei), sb.Texture, i, ei);
							}
							LogBatch.AppendLine(" -> [OK]");
						}
						else
						{
							LogBatch.AppendLine(" -> loading file failed!!!");
						}
					}
				}
				sb.Texture.Apply();
			}
			BepinLogger.LogInfo(LogBatch);
			LogBatch.Clear();
		}

		private void SaveToFiles()
		{
			File.WriteAllText(SaveFilePath, IsEnabled.ToString());
			File.WriteAllText(FlagFilePath, CurrentResource.FolderName);
			Logger.LogInfo("Autosaved PlayableLoader settings.");
		}
		
		public class ResourceModel : IDisposable
		{
			public string Name
			{
				get => _Name ?? FolderName;
				set => _Name = value;
			}
			
			public string Author
			{
				get => _Author ?? "unknown";
				set => _Author = value;
			}
			
			public Texture Icon { get; set; }
			
			public bool IsDefault { get; set; }
			
			public string FolderName { get; set; }
			
			public ResourceModel()
			{
				Icon = null;
				_Name = null;
				_Author = null;
			}

			public static ResourceModel Parse(string file)
			{
				ResourceModel i = new ResourceModel();
				string[] array = File.ReadAllLines(file);
				foreach (string line in array)
				{
					string[] x = line.Split('=');
					string p = x[0].Trim().ToLower();
					bool flag = p.StartsWith("#") || p.StartsWith(";") || string.IsNullOrEmpty(p);
					if (!flag)
					{
						bool flag2 = x.Length == 2;
						if (!flag2)
						{
							throw new Exception("Failed to parse resource info!");
						}
						string p2 = x[1].Trim();
						string text = p;
						string text2 = text;
						if (text2 != "name")
						{
							if (text2 != "author")
							{
								if (text2 == "icon")
								{
									Texture2D ico = new Texture2D(2, 2);
									string icoPath = Path.Combine(Path.GetDirectoryName(file), p2);
									bool flag3 = File.Exists(icoPath) && ico.LoadImage(File.ReadAllBytes(icoPath));
									if (flag3)
									{
										i.Icon = ico;
									}
								}
							}
							else
							{
								i.Author = p2;
							}
						}
						else
						{
							i.Name = p2;
						}
					}
				}
				return i;
			}

			public void Dispose()
			{
				bool flag = Icon != null;
				if (flag)
				{
					Destroy(Icon);
				}
			}

			private string _Name;

			private string _Author;
		}
	}
}

public class MainMenuPatch
{
	[HarmonyPatch(typeof(MainMenu), "OnOpen")]
	public static class MainMenuOpenPatch
	{
		private static void Postfix(MainMenu __instance)
		{
			if (PlayableLoaderPlugin.PlayableLoaderMenu == null) return;
			PlayableLoaderPlugin.PlayableLoaderMenu.transform.SetParent(__instance.transform, false);
			PlayableLoaderPlugin.PlayableLoaderMenu.transform.localPosition = new Vector3(-467.4f, -356.6f, 0f);
			PlayableLoaderPlugin.PlayableLoaderMenu.SetActive(true);
			if (CustomSkin.customSkinPanel == null) return;
			if (CustomSkin.customSkinPanel.activeSelf)
			{
				__instance.transform.parent.Find("Control bar").gameObject.SetActive(false);
			}
		}
	}
	[HarmonyPatch(typeof(MainMenu), "OnClose")]
	public static class MainMenuClosePatch
	{
		private static void Postfix(MainMenu __instance)
		{
			if (PlayableLoaderPlugin.PlayableLoaderMenu == null) return;
			PlayableLoaderPlugin.PlayableLoaderMenu.transform.SetParent(__instance.transform, false);
			PlayableLoaderPlugin.PlayableLoaderMenu.transform.localPosition = new Vector3(-467.4f, -356.6f, 0f);
			PlayableLoaderPlugin.PlayableLoaderMenu.SetActive(false);
		}
	}
}