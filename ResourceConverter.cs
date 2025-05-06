using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PlayableLoaderBepInEx;

internal class ResourceConverter
	{
		private static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
		{
			Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
			Color[] rpixels = result.GetPixels(0);
			float incX = 1f / source.width * (source.width / (float)targetWidth);
			float incY = 1f / source.height * (source.height / (float)targetHeight);
			for (int px = 0; px < rpixels.Length; px++)
			{
				rpixels[px] = source.GetPixelBilinear(incX * (px % (float)targetWidth), incY * Mathf.Floor(px / targetWidth));
			}
			result.SetPixels(rpixels, 0);
			result.Apply();
			return result;
		}

		public static void Convert(string path, string targetPath)
		{
			bool flag = !Directory.Exists(targetPath);
			if (flag)
			{
				Directory.CreateDirectory(targetPath);
			}
			foreach (KeyValuePair<string, SpriteConvertData> x in ConvertMetaData)
			{
				string f = Path.Combine(path, x.Key + ".png");
				bool flag2 = File.Exists(f);
				if (flag2)
				{
					Texture2D t = new Texture2D(512 * x.Value.ImagesInSprite, 512);
					bool flag3 = !t.LoadImage(File.ReadAllBytes(f), false);
					if (!flag3)
					{
						for (int i = 0; i < x.Value.ImagesInSprite; i++)
						{
							Color[] u = t.GetPixels(t.height * i, 0, t.height, t.height);
							Texture2D v = new Texture2D(t.height, t.height);
							v.SetPixels(u);
							Texture2D w = ScaleTexture(v, 512, 512);
							string f2 = Path.Combine(targetPath, string.Format("{0}{1}.png", x.Value.Prefix, x.Value.StartIndex + i));
							if (x.Key == "imgNikoIdleF")
								f2 = Path.Combine(targetPath, string.Format("{0}{1}.png", x.Value.Prefix, 20 + i));
							File.WriteAllBytes(f2, w.EncodeToPNG());
						}
					}
				}
			}
			string xx = Path.Combine(targetPath, "Player_" + 4 + ".png");
			string xy = Path.Combine(targetPath, "Player_" + 5 + ".png");
			string xxa = Path.Combine(targetPath, "Player_" + 4 + ".png");
			string xyb = Path.Combine(targetPath, "Player_" + 5 + ".png");
			bool flag4 = File.Exists(xx);
			if (flag4)
			{
				File.Copy(xx, xxa);
			}
			bool flag5 = File.Exists(xy);
			if (flag5)
			{
				File.Copy(xy, xyb);
			}
		}

		private static Dictionary<string, SpriteConvertData> ConvertMetaData = new Dictionary<string, SpriteConvertData>
		{
			{
				"imgNikoClimbB",
				new SpriteConvertData(2, "Player_", 0)
			},
			{
				"imgNikoClimbF",
				new SpriteConvertData(2, "Player_", 2)
			},
			{
				"imgNikoConfused",
				new SpriteConvertData(1, "Player_", 67)
			},
			{
				"imgNikoDisappointed",
				new SpriteConvertData(2, "Player_", 68)
			},
			{
				"imgNikoDiveB",
				new SpriteConvertData(1, "Player_", 6)
			},
			{
				"imgNikoDiveF",
				new SpriteConvertData(1, "Player_", 7)
			},
			{
				"imgNikoDone",
				new SpriteConvertData(1, "Player_", 8)
			},
			{
				"imgNikoFallB",
				new SpriteConvertData(2, "Player_", 9)
			},
			{
				"imgNikoFallF",
				new SpriteConvertData(2, "Player_", 11)
			},
			{
				"imgNikoGet",
				new SpriteConvertData(1, "Player_", 13)
			},
			{
				"imgNikoHangB",
				new SpriteConvertData(1, "Player_", 14)
			},
			{
				"imgNikoHangF",
				new SpriteConvertData(1, "Player_", 15)
			},
			{
				"imgNikoHappy",
				new SpriteConvertData(1, "Player_", 70)
			},
			{
				"imgNikoHurtB",
				new SpriteConvertData(1, "Player_", 16)
			},
			{
				"imgNikoHurtF",
				new SpriteConvertData(1, "Player_", 17)
			},
			{
				"imgNikoIdleB",
				new SpriteConvertData(2, "Player_", 18)
			},
			{
				"imgNikoIdleF",
				new SpriteConvertData(2, "Player_", 4)
			},
			{
				"imgNikoJumpB",
				new SpriteConvertData(2, "Player_", 24)
			},
			{
				"imgNikoJumpF",
				new SpriteConvertData(2, "Player_", 26)
			},
			{
				"imgNikoListening",
				new SpriteConvertData(1, "Player_", 71)
			},
			{
				"imgNikoNod",
				new SpriteConvertData(2, "Player_", 72)
			},
			{
				"imgNikoPhoneHappy",
				new SpriteConvertData(2, "Player_", 30)
			},
			{
				"imgNikoPhoneSad",
				new SpriteConvertData(2, "Player_", 32)
			},
			{
				"imgNikoProud",
				new SpriteConvertData(2, "Player_", 74)
			},
			{
				"imgNikoRunB",
				new SpriteConvertData(8, "Player_", 34)
			},
			{
				"imgNikoRunF",
				new SpriteConvertData(8, "Player_", 42)
			},
			{
				"imgNikoShocked",
				new SpriteConvertData(1, "Player_", 76)
			},
			{
				"imgNikoSitting",
				new SpriteConvertData(2, "Player_", 50)
			},
			{
				"imgNikoSittingAnxious",
				new SpriteConvertData(2, "Player_", 52)
			},
			{
				"imgNikoSittingHappy",
				new SpriteConvertData(2, "Player_", 54)
			},
			{
				"imgNikoSurprised",
				new SpriteConvertData(1, "Player_", 77)
			},
			{
				"imgNikoSuspicious",
				new SpriteConvertData(1, "Player_", 78)
			},
			{
				"imgNikoSwimB",
				new SpriteConvertData(2, "Player_", 56)
			},
			{
				"imgNikoSwimF",
				new SpriteConvertData(2, "Player_", 58)
			},
			{
				"imgNikoUpset",
				new SpriteConvertData(2, "Player_", 60)
			},
			{
				"imgNikoWallbumpBack",
				new SpriteConvertData(1, "Player_", 62)
			},
			{
				"imgNikoWallbumpFront",
				new SpriteConvertData(1, "Player_", 63)
			},
			{
				"imgNikoWallhop",
				new SpriteConvertData(1, "Player_", 64)
			},
			{
				"imgNikoWalljumpback",
				new SpriteConvertData(1, "Player_", 65)
			},
			{
				"imgNikoWalljumpFront",
				new SpriteConvertData(1, "Player_", 66)
			},
		};

		private class SpriteConvertData
		{
			public SpriteConvertData(int imagesInSprite, string prefix, int startIndex)
			{
				ImagesInSprite = imagesInSprite;
				Prefix = prefix;
				StartIndex = startIndex;
			}

			public int ImagesInSprite;

			public string Prefix;

			public int StartIndex;
		}
	}