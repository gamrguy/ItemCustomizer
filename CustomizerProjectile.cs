using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ItemCustomizer
{
	public class CustomizerProjectile : GlobalProjectile
	{
		public List<int> newProjectiles = new List<int>();

		public override void SetDefaults(Projectile projectile)
		{
			newProjectiles.Add(projectile.whoAmI);
		}

		public override bool PreAI(Projectile projectile)
		{
			CustomizerProjInfo projInfo = projectile.GetModInfo<CustomizerProjInfo>(mod);

			bool hook = Main.projHook[projectile.type];
			bool pet = (projectile.type == ProjectileID.StardustGuardian) || (Main.projPet[projectile.type] && !projectile.minion && projectile.damage == 0 && !ProjectileID.Sets.LightPet[projectile.type]);
			bool lightPet = !projectile.minion && projectile.damage == 0 && ProjectileID.Sets.LightPet[projectile.type];
			
			if(projInfo.parent && !(hook || pet || lightPet) && !projectile.npcProj && projectile.owner != 255 && Main.player[projectile.owner].itemAnimation > 0 && ((projectile.friendly || !projectile.hostile) || projectile.minion) && projInfo.shaderID < 0){
				projInfo.shaderID = (mod as CustomizerMod).heldShaders[projectile.owner];
			}

			newProjectiles = new List<int>();
			return base.PreAI(projectile);
		}

		public override void PostAI(Projectile projectile)
		{
			ShadeChildren(projectile);
		}

		public override bool PreKill(Projectile projectile, int timeLeft)
		{
			newProjectiles = new List<int>();
			return base.PreKill(projectile, timeLeft);
		}

		public override void Kill(Projectile projectile, int timeLeft)
		{
			ShadeChildren(projectile);
		}

		public void ShadeChildren(Projectile projectile){
			CustomizerProjInfo info = projectile.GetModInfo<CustomizerProjInfo>(mod);

			if(info.shaderID > 0) {
				foreach(int proj in newProjectiles) {
					CustomizerProjInfo childInfo = Main.projectile[proj].GetModInfo<CustomizerProjInfo>(mod);
					childInfo.shaderID = info.shaderID;
					childInfo.parent = false;
				}
				newProjectiles = new List<int>();
			}
		}

		public override bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
			CustomizerProjInfo projInfo = projectile.GetModInfo<CustomizerProjInfo>(mod);

			//Only affect projectiles with shaders
			if(projInfo.shaderID > 0) {

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(1f, 1f, 1f) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));

				DrawData data = new DrawData();
				data.origin = projectile.Center;
				data.position = projectile.position - Main.screenPosition;
				data.scale = new Vector2(projectile.scale, projectile.scale);
				data.texture = Main.projectileTexture[projectile.type];
				data.sourceRect = data.texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame); 
				GameShaders.Armor.ApplySecondary(projInfo.shaderID, Main.player[projectile.owner], data);
				
				//only apply custom drawing to projectiles without it
				if(projectile.modProjectile == null || projectile.modProjectile.PreDraw(spriteBatch, lightColor)) {
					DrawProj(projectile.whoAmI, projectile);
					//Main.instance.DrawProj(projectile.whoAmI);
				}
				return false;
			}
			return true;
		}

		//Resets the SpriteBatch after drawing projectile, to prepare for next projectile
		public override void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(1f, 1f, 1f) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(0f, 0f, 0f)));
		}

		//This is the vanilla draw code, slightly edited to work properly.
		//Don't look at this. It's a horrifing mess, just like a lot of vanilla code.
		public void DrawProj(int i, Projectile projectile)
		{
			float num = 0f;
			float num2 = 0f;
			//Projectile projectile = Main.projectile[i];
			//Main.LoadProjectile(projectile.type);
			Vector2 mountedCenter = Main.player[projectile.owner].MountedCenter;
			if (projectile.aiStyle == 99)
			{
				Vector2 vector = mountedCenter;
				vector.Y += Main.player[projectile.owner].gfxOffY;
				float num3 = projectile.Center.X - vector.X;
				float num4 = projectile.Center.Y - vector.Y;
				Math.Sqrt((double)(num3 * num3 + num4 * num4));
				float num5 = (float)Math.Atan2((double)num4, (double)num3) - 1.57f;
				if (!projectile.counterweight) 
				{
					int num6 = -1;
					if (projectile.position.X + (float)(projectile.width / 2) < Main.player[projectile.owner].position.X + (float)(Main.player[projectile.owner].width / 2))
					{
						num6 = 1;
					}
					num6 *= -1;
					Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num4 * (float)num6), (double)(num3 * (float)num6));
				}
				bool flag = true;
				if (num3 == 0f && num4 == 0f)
				{
					flag = false;
				}
				else
				{
					float num7 = (float)Math.Sqrt((double)(num3 * num3 + num4 * num4));
					num7 = 12f / num7;
					num3 *= num7;
					num4 *= num7;
					vector.X -= num3 * 0.1f;
					vector.Y -= num4 * 0.1f;
					num3 = projectile.position.X + (float)projectile.width * 0.5f - vector.X;
					num4 = projectile.position.Y + (float)projectile.height * 0.5f - vector.Y;
				}
				while (flag)
				{
					float num8 = 12f;
					float num9 = (float)Math.Sqrt((double)(num3 * num3 + num4 * num4));
					float num10 = num9;
					if (float.IsNaN(num9) || float.IsNaN(num10))
					{
						flag = false;
					}
					else
					{
						if (num9 < 20f)
						{
							num8 = num9 - 8f;
							flag = false;
						}
						num9 = 12f / num9;
						num3 *= num9;
						num4 *= num9;
						vector.X += num3;
						vector.Y += num4;
						num3 = projectile.position.X + (float)projectile.width * 0.5f - vector.X;
						num4 = projectile.position.Y + (float)projectile.height * 0.1f - vector.Y;
						if (num10 > 12f)
						{
							float num11 = 0.3f;
							float num12 = Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y);
							if (num12 > 16f)
							{
								num12 = 16f;
							}
							num12 = 1f - num12 / 16f;
							num11 *= num12;
							num12 = num10 / 80f;
							if (num12 > 1f)
							{
								num12 = 1f;
							}
							num11 *= num12;
							if (num11 < 0f)
							{
								num11 = 0f;
							}
							num11 *= num12;
							num11 *= 0.5f;
							if (num4 > 0f)
							{
								num4 *= 1f + num11;
								num3 *= 1f - num11;
							}
							else
							{
								num12 = Math.Abs(projectile.velocity.X) / 3f;
								if (num12 > 1f)
								{
									num12 = 1f;
								}
								num12 -= 0.5f;
								num11 *= num12;
								if (num11 > 0f)
								{
									num11 *= 2f;
								}
								num4 *= 1f + num11;
								num3 *= 1f - num11;
							}
						}
						num5 = (float)Math.Atan2((double)num4, (double)num3) - 1.57f;
						int stringColor = Main.player[projectile.owner].stringColor;
						Color color = WorldGen.paintColor(stringColor);
						if (color.R < 75)
						{
							color.R = (75);
						}
						if (color.G < 75)
						{
							color.G = (75);
						}
						if (color.B < 75)
						{
							color.B = (75);
						}
						if (stringColor == 13)
						{
							color = new Color(20, 20, 20);
						}
						else if (stringColor == 14 || stringColor == 0)
						{
							color = new Color(200, 200, 200);
						}
						else if (stringColor == 28)
						{
							color = new Color(163, 116, 91);
						}
						else if (stringColor == 27)
						{
							color = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
						}
						color.A = ((byte)((float)color.A * 0.4f));
						float num13 = 0.5f;
						color = Lighting.GetColor((int)vector.X / 16, (int)(vector.Y / 16f), color);
						color = new Color((int)((byte)((float)color.R * num13)), (int)((byte)((float)color.G * num13)), (int)((byte)((float)color.B * num13)), (int)((byte)((float)color.A * num13)));
						Main.spriteBatch.Draw(Main.fishingLineTexture, new Vector2(vector.X - Main.screenPosition.X + (float)Main.fishingLineTexture.Width * 0.5f, vector.Y - Main.screenPosition.Y + (float)Main.fishingLineTexture.Height * 0.5f) - new Vector2(6f, 0f), new Rectangle?(new Rectangle(0, 0, Main.fishingLineTexture.Width, (int)num8)), color, num5, new Vector2((float)Main.fishingLineTexture.Width * 0.5f, 0f), 1f, 0, 0f);
					}
				}
			}
			if (projectile.bobber && Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].holdStyle > 0)
			{
				num = mountedCenter.X;
				num2 = mountedCenter.Y;
				num2 += Main.player[projectile.owner].gfxOffY;
				int type = Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].type;
				float gravDir = Main.player[projectile.owner].gravDir;
				if (type == 2289)
				{
					num += (float)(43 * Main.player[projectile.owner].direction);
					if (Main.player[projectile.owner].direction < 0)
					{
						num -= 13f;
					}
					num2 -= 36f * gravDir;
				}
				else if (type == 2291)
				{
					num += (float)(43 * Main.player[projectile.owner].direction);
					if (Main.player[projectile.owner].direction < 0)
					{
						num -= 13f;
					}
					num2 -= 34f * gravDir;
				}
				else if (type == 2292)
				{
					num += (float)(46 * Main.player[projectile.owner].direction);
					if (Main.player[projectile.owner].direction < 0)
					{
						num -= 13f;
					}
					num2 -= 34f * gravDir;
				}
				else if (type == 2293)
				{
					num += (float)(43 * Main.player[projectile.owner].direction);
					if (Main.player[projectile.owner].direction < 0)
					{
						num -= 13f;
					}
					num2 -= 34f * gravDir;
				}
				else if (type == 2294)
				{
					num += (float)(43 * Main.player[projectile.owner].direction);
					if (Main.player[projectile.owner].direction < 0)
					{
						num -= 13f;
					}
					num2 -= 30f * gravDir;
				}
				else if (type == 2295)
				{
					num += (float)(43 * Main.player[projectile.owner].direction);
					if (Main.player[projectile.owner].direction < 0)
					{
						num -= 13f;
					}
					num2 -= 30f * gravDir;
				}
				else if (type == 2296)
				{
					num += (float)(43 * Main.player[projectile.owner].direction);
					if (Main.player[projectile.owner].direction < 0)
					{
						num -= 13f;
					}
					num2 -= 30f * gravDir;
				}
				else if (type == 2421)
				{
					num += (float)(47 * Main.player[projectile.owner].direction);
					if (Main.player[projectile.owner].direction < 0)
					{
						num -= 13f;
					}
					num2 -= 36f * gravDir;
				}
				else if (type == 2422)
				{
					num += (float)(47 * Main.player[projectile.owner].direction);
					if (Main.player[projectile.owner].direction < 0)
					{
						num -= 13f;
					}
					num2 -= 32f * gravDir;
				}
				if (gravDir == -1f)
				{
					num2 -= 12f;
				}
				Vector2 vector2 = new Vector2(num, num2);
				vector2 = Main.player[projectile.owner].RotatedRelativePoint(vector2 + new Vector2(8f), true) - new Vector2(8f);
				float num14 = projectile.position.X + (float)projectile.width * 0.5f - vector2.X;
				float num15 = projectile.position.Y + (float)projectile.height * 0.5f - vector2.Y;
				Math.Sqrt((double)(num14 * num14 + num15 * num15));
				float num16 = (float)Math.Atan2((double)num15, (double)num14) - 1.57f;
				bool flag2 = true;
				if (num14 == 0f && num15 == 0f)
				{
					flag2 = false;
				}
				else
				{
					float num17 = (float)Math.Sqrt((double)(num14 * num14 + num15 * num15));
					num17 = 12f / num17;
					num14 *= num17;
					num15 *= num17;
					vector2.X -= num14;
					vector2.Y -= num15;
					num14 = projectile.position.X + (float)projectile.width * 0.5f - vector2.X;
					num15 = projectile.position.Y + (float)projectile.height * 0.5f - vector2.Y;
				}
				while (flag2)
				{
					float num18 = 12f;
					float num19 = (float)Math.Sqrt((double)(num14 * num14 + num15 * num15));
					float num20 = num19;
					if (float.IsNaN(num19) || float.IsNaN(num20))
					{
						flag2 = false;
					}
					else
					{
						if (num19 < 20f)
						{
							num18 = num19 - 8f;
							flag2 = false;
						}
						num19 = 12f / num19;
						num14 *= num19;
						num15 *= num19;
						vector2.X += num14;
						vector2.Y += num15;
						num14 = projectile.position.X + (float)projectile.width * 0.5f - vector2.X;
						num15 = projectile.position.Y + (float)projectile.height * 0.1f - vector2.Y;
						if (num20 > 12f)
						{
							float num21 = 0.3f;
							float num22 = Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y);
							if (num22 > 16f)
							{
								num22 = 16f;
							}
							num22 = 1f - num22 / 16f;
							num21 *= num22;
							num22 = num20 / 80f;
							if (num22 > 1f)
							{
								num22 = 1f;
							}
							num21 *= num22;
							if (num21 < 0f)
							{
								num21 = 0f;
							}
							num22 = 1f - projectile.localAI[0] / 100f;
							num21 *= num22;
							if (num15 > 0f)
							{
								num15 *= 1f + num21;
								num14 *= 1f - num21;
							}
							else
							{
								num22 = Math.Abs(projectile.velocity.X) / 3f;
								if (num22 > 1f)
								{
									num22 = 1f;
								}
								num22 -= 0.5f;
								num21 *= num22;
								if (num21 > 0f)
								{
									num21 *= 2f;
								}
								num15 *= 1f + num21;
								num14 *= 1f - num21;
							}
						}
						num16 = (float)Math.Atan2((double)num15, (double)num14) - 1.57f;
						Color color2 = Lighting.GetColor((int)vector2.X / 16, (int)(vector2.Y / 16f), new Color(200, 200, 200, 100));
						if (type == 2294)
						{
							color2 = Lighting.GetColor((int)vector2.X / 16, (int)(vector2.Y / 16f), new Color(100, 180, 230, 100));
						}
						if (type == 2295)
						{
							color2 = Lighting.GetColor((int)vector2.X / 16, (int)(vector2.Y / 16f), new Color(250, 90, 70, 100));
						}
						if (type == 2293)
						{
							color2 = Lighting.GetColor((int)vector2.X / 16, (int)(vector2.Y / 16f), new Color(203, 190, 210, 100));
						}
						if (type == 2421)
						{
							color2 = Lighting.GetColor((int)vector2.X / 16, (int)(vector2.Y / 16f), new Color(183, 77, 112, 100));
						}
						if (type == 2422)
						{
							color2 = Lighting.GetColor((int)vector2.X / 16, (int)(vector2.Y / 16f), new Color(255, 226, 116, 100));
						}
						Main.spriteBatch.Draw(Main.fishingLineTexture, new Vector2(vector2.X - Main.screenPosition.X + (float)Main.fishingLineTexture.Width * 0.5f, vector2.Y - Main.screenPosition.Y + (float)Main.fishingLineTexture.Height * 0.5f), new Rectangle?(new Rectangle(0, 0, Main.fishingLineTexture.Width, (int)num18)), color2, num16, new Vector2(((float)Main.fishingLineTexture.Width) * 0.5f, 0f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 32)
			{
				Vector2 vector3 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num23 = mountedCenter.X - vector3.X;
				float num24 = mountedCenter.Y - vector3.Y;
				float num25 = (float)Math.Atan2((double)num24, (double)num23) - 1.57f;
				bool flag3 = true;
				if (num23 == 0f && num24 == 0f)
				{
					flag3 = false;
				}
				else
				{
					float num26 = (float)Math.Sqrt((double)(num23 * num23 + num24 * num24));
					num26 = 8f / num26;
					num23 *= num26;
					num24 *= num26;
					vector3.X -= num23;
					vector3.Y -= num24;
					num23 = mountedCenter.X - vector3.X;
					num24 = mountedCenter.Y - vector3.Y;
				}
				while (flag3)
				{
					float num27 = (float)Math.Sqrt((double)(num23 * num23 + num24 * num24));
					if (num27 < 28f)
					{
						flag3 = false;
					}
					else if (float.IsNaN(num27))
					{
						flag3 = false;
					}
					else
					{
						num27 = 28f / num27;
						num23 *= num27;
						num24 *= num27;
						vector3.X += num23;
						vector3.Y += num24;
						num23 = mountedCenter.X - vector3.X;
						num24 = mountedCenter.Y - vector3.Y;
						Color color3 = Lighting.GetColor((int)vector3.X / 16, (int)(vector3.Y / 16f));
						Main.spriteBatch.Draw(Main.chain5Texture, new Vector2(vector3.X - Main.screenPosition.X, vector3.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain5Texture.Width, Main.chain5Texture.Height)), color3, num25, new Vector2(((float)Main.chain5Texture.Width) * 0.5f, ((float)Main.chain5Texture.Height) * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 73)
			{
				Vector2 vector4 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num28 = mountedCenter.X - vector4.X;
				float num29 = mountedCenter.Y - vector4.Y;
				float num30 = (float)Math.Atan2((double)num29, (double)num28) - 1.57f;
				bool flag4 = true;
				while (flag4)
				{
					float num31 = (float)Math.Sqrt((double)(num28 * num28 + num29 * num29));
					if (num31 < 25f)
					{
						flag4 = false;
					}
					else if (float.IsNaN(num31))
					{
						flag4 = false;
					}
					else
					{
						num31 = 12f / num31;
						num28 *= num31;
						num29 *= num31;
						vector4.X += num28;
						vector4.Y += num29;
						num28 = mountedCenter.X - vector4.X;
						num29 = mountedCenter.Y - vector4.Y;
						Color color4 = Lighting.GetColor((int)vector4.X / 16, (int)(vector4.Y / 16f));
						Main.spriteBatch.Draw(Main.chain8Texture, new Vector2(vector4.X - Main.screenPosition.X, vector4.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain8Texture.Width, Main.chain8Texture.Height)), color4, num30, new Vector2((float)Main.chain8Texture.Width * 0.5f, (float)Main.chain8Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 186)
			{
				Vector2 vector5 = new Vector2(projectile.localAI[0], projectile.localAI[1]);
				float num32 = Vector2.Distance(projectile.Center, vector5) - projectile.velocity.Length();
				float num33 = (float)Main.chain17Texture.Height - num32;
				if (num32 > 0f && projectile.ai[1] > 0f)
				{
					Color color5 = Lighting.GetColor((int)projectile.position.X / 16, (int)projectile.position.Y / 16);
					Main.spriteBatch.Draw(Main.chain17Texture, vector5 - Main.screenPosition, new Rectangle?(new Rectangle(0, (int)num33, Main.chain17Texture.Width, (int)num32)), color5, projectile.rotation, new Vector2((float)(Main.chain17Texture.Width / 2), 0f), 1f, 0, 0f);
				}
			}
			else if (projectile.type == 74)
			{
				Vector2 vector6 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num34 = mountedCenter.X - vector6.X;
				float num35 = mountedCenter.Y - vector6.Y;
				float num36 = (float)Math.Atan2((double)num35, (double)num34) - 1.57f;
				bool flag5 = true;
				while (flag5)
				{
					float num37 = (float)Math.Sqrt((double)(num34 * num34 + num35 * num35));
					if (num37 < 25f)
					{
						flag5 = false;
					}
					else if (float.IsNaN(num37))
					{
						flag5 = false;
					}
					else
					{
						num37 = 12f / num37;
						num34 *= num37;
						num35 *= num37;
						vector6.X += num34;
						vector6.Y += num35;
						num34 = mountedCenter.X - vector6.X;
						num35 = mountedCenter.Y - vector6.Y;
						Color color6 = Lighting.GetColor((int)vector6.X / 16, (int)(vector6.Y / 16f));
						Main.spriteBatch.Draw(Main.chain9Texture, new Vector2(vector6.X - Main.screenPosition.X, vector6.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain8Texture.Width, Main.chain8Texture.Height)), color6, num36, new Vector2((float)Main.chain8Texture.Width * 0.5f, (float)Main.chain8Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 171)
			{
				Vector2 vector7 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num38 = -projectile.velocity.X;
				float num39 = -projectile.velocity.Y;
				float num40 = 1f;
				if (projectile.ai[0] <= 17f)
				{
					num40 = projectile.ai[0] / 17f;
				}
				int num41 = (int)(30f * num40);
				float num42 = 1f;
				if (projectile.ai[0] <= 30f)
				{
					num42 = projectile.ai[0] / 30f;
				}
				float num43 = 0.4f * num42;
				float num44 = num43;
				num39 += num44;
				Vector2[] array = new Vector2[num41];
				float[] array2 = new float[num41];
				for (int j = 0; j < num41; j++)
				{
					float num45 = (float)Math.Sqrt((double)(num38 * num38 + num39 * num39));
					float num46 = 5.6f;
					if (Math.Abs(num38) + Math.Abs(num39) < 1f)
					{
						num46 *= Math.Abs(num38) + Math.Abs(num39) / 1f;
					}
					num45 = num46 / num45;
					num38 *= num45;
					num39 *= num45;
					float num47 = (float)Math.Atan2((double)num39, (double)num38) - 1.57f;
					array[j].X = vector7.X;
					array[j].Y = vector7.Y;
					array2[j] = num47;
					vector7.X += num38;
					vector7.Y += num39;
					num38 = -projectile.velocity.X;
					num39 = -projectile.velocity.Y;
					num44 += num43;
					num39 += num44;
				}
				for (int k = num41 - 1; k >= 0; k--)
				{
					vector7.X = array[k].X;
					vector7.Y = array[k].Y;
					float num48 = array2[k];
					Color color7 = Lighting.GetColor((int)vector7.X / 16, (int)(vector7.Y / 16f));
					Main.spriteBatch.Draw(Main.chain16Texture, new Vector2(vector7.X - Main.screenPosition.X, vector7.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain16Texture.Width, Main.chain16Texture.Height)), color7, num48, new Vector2((float)Main.chain16Texture.Width * 0.5f, (float)Main.chain16Texture.Height * 0.5f), 0.8f, 0, 0f);
				}
			}
			else if (projectile.type == 475)
			{
				Vector2 vector8 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num49 = -projectile.velocity.X;
				float num50 = -projectile.velocity.Y;
				float num51 = 1f;
				if (projectile.ai[0] <= 17f)
				{
					num51 = projectile.ai[0] / 17f;
				}
				int num52 = (int)(30f * num51);
				float num53 = 1f;
				if (projectile.ai[0] <= 30f)
				{
					num53 = projectile.ai[0] / 30f;
				}
				float num54 = 0.4f * num53;
				float num55 = num54;
				num50 += num55;
				Vector2[] array3 = new Vector2[num52];
				float[] array4 = new float[num52];
				for (int l = 0; l < num52; l++)
				{
					float num56 = (float)Math.Sqrt((double)(num49 * num49 + num50 * num50));
					float num57 = 5.6f;
					if (Math.Abs(num49) + Math.Abs(num50) < 1f)
					{
						num57 *= Math.Abs(num49) + Math.Abs(num50) / 1f;
					}
					num56 = num57 / num56;
					num49 *= num56;
					num50 *= num56;
					float num58 = (float)Math.Atan2((double)num50, (double)num49) - 1.57f;
					array3[l].X = vector8.X;
					array3[l].Y = vector8.Y;
					array4[l] = num58;
					vector8.X += num49;
					vector8.Y += num50;
					num49 = -projectile.velocity.X;
					num50 = -projectile.velocity.Y;
					num55 += num54;
					num50 += num55;
				}
				int num59 = 0;
				for (int m = num52 - 1; m >= 0; m--)
				{
					vector8.X = array3[m].X;
					vector8.Y = array3[m].Y;
					float num60 = array4[m];
					Color color8 = Lighting.GetColor((int)vector8.X / 16, (int)(vector8.Y / 16f));
					if (num59 % 2 == 0)
					{
						Main.spriteBatch.Draw(Main.chain38Texture, new Vector2(vector8.X - Main.screenPosition.X, vector8.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain38Texture.Width, Main.chain38Texture.Height)), color8, num60, new Vector2((float)Main.chain38Texture.Width * 0.5f, (float)Main.chain38Texture.Height * 0.5f), 0.8f, 0, 0f);
					}
					else
					{
						Main.spriteBatch.Draw(Main.chain39Texture, new Vector2(vector8.X - Main.screenPosition.X, vector8.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain39Texture.Width, Main.chain39Texture.Height)), color8, num60, new Vector2((float)Main.chain39Texture.Width * 0.5f, (float)Main.chain39Texture.Height * 0.5f), 0.8f, 0, 0f);
					}
					num59++;
				}
			}
			else if (projectile.type == 505 || projectile.type == 506)
			{
				Vector2 vector9 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num61 = -projectile.velocity.X;
				float num62 = -projectile.velocity.Y;
				float num63 = 1f;
				if (projectile.ai[0] <= 17f)
				{
					num63 = projectile.ai[0] / 17f;
				}
				int num64 = (int)(30f * num63);
				float num65 = 1f;
				if (projectile.ai[0] <= 30f)
				{
					num65 = projectile.ai[0] / 30f;
				}
				float num66 = 0.4f * num65;
				float num67 = num66;
				num62 += num67;
				Vector2[] array5 = new Vector2[num64];
				float[] array6 = new float[num64];
				for (int n = 0; n < num64; n++)
				{
					float num68 = (float)Math.Sqrt((double)(num61 * num61 + num62 * num62));
					float num69 = 5.6f;
					if (Math.Abs(num61) + Math.Abs(num62) < 1f)
					{
						num69 *= Math.Abs(num61) + Math.Abs(num62) / 1f;
					}
					num68 = num69 / num68;
					num61 *= num68;
					num62 *= num68;
					float num70 = (float)Math.Atan2((double)num62, (double)num61) - 1.57f;
					array5[n].X = vector9.X;
					array5[n].Y = vector9.Y;
					array6[n] = num70;
					vector9.X += num61;
					vector9.Y += num62;
					num61 = -projectile.velocity.X;
					num62 = -projectile.velocity.Y;
					num67 += num66;
					num62 += num67;
				}
				int num71 = 0;
				for (int num72 = num64 - 1; num72 >= 0; num72--)
				{
					vector9.X = array5[num72].X;
					vector9.Y = array5[num72].Y;
					float num73 = array6[num72];
					Color color9 = Lighting.GetColor((int)vector9.X / 16, (int)(vector9.Y / 16f));
					int num74 = 4;
					if (projectile.type == 506)
					{
						num74 = 6;
					}
					num74 += num71 % 2;
					Main.spriteBatch.Draw(Main.chainsTexture[num74], new Vector2(vector9.X - Main.screenPosition.X, vector9.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chainsTexture[num74].Width, Main.chainsTexture[num74].Height)), color9, num73, new Vector2((float)Main.chainsTexture[num74].Width * 0.5f, (float)Main.chainsTexture[num74].Height * 0.5f), 0.8f, 0, 0f);
					num71++;
				}
			}
			else if (projectile.type == 165)
			{
				Vector2 vector10 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num75 = mountedCenter.X - vector10.X;
				float num76 = mountedCenter.Y - vector10.Y;
				float num77 = (float)Math.Atan2((double)num76, (double)num75) - 1.57f;
				bool flag6 = true;
				while (flag6)
				{
					float num78 = (float)Math.Sqrt((double)(num75 * num75 + num76 * num76));
					if (num78 < 25f)
					{
						flag6 = false;
					}
					else if (float.IsNaN(num78))
					{
						flag6 = false;
					}
					else
					{
						num78 = 24f / num78;
						num75 *= num78;
						num76 *= num78;
						vector10.X += num75;
						vector10.Y += num76;
						num75 = mountedCenter.X - vector10.X;
						num76 = mountedCenter.Y - vector10.Y;
						Color color10 = Lighting.GetColor((int)vector10.X / 16, (int)(vector10.Y / 16f));
						Main.spriteBatch.Draw(Main.chain15Texture, new Vector2(vector10.X - Main.screenPosition.X, vector10.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain15Texture.Width, Main.chain15Texture.Height)), color10, num77, new Vector2((float)Main.chain15Texture.Width * 0.5f, (float)Main.chain15Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type >= 230 && projectile.type <= 235)
			{
				int num79 = projectile.type - 229;
				Vector2 vector11 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num80 = mountedCenter.X - vector11.X;
				float num81 = mountedCenter.Y - vector11.Y;
				float num82 = (float)Math.Atan2((double)num81, (double)num80) - 1.57f;
				bool flag7 = true;
				while (flag7)
				{
					float num83 = (float)Math.Sqrt((double)(num80 * num80 + num81 * num81));
					if (num83 < 25f)
					{
						flag7 = false;
					}
					else if (float.IsNaN(num83))
					{
						flag7 = false;
					}
					else
					{
						num83 = (float)Main.gemChainTexture[num79].Height / num83;
						num80 *= num83;
						num81 *= num83;
						vector11.X += num80;
						vector11.Y += num81;
						num80 = mountedCenter.X - vector11.X;
						num81 = mountedCenter.Y - vector11.Y;
						Color color11 = Lighting.GetColor((int)vector11.X / 16, (int)(vector11.Y / 16f));
						Main.spriteBatch.Draw(Main.gemChainTexture[num79], new Vector2(vector11.X - Main.screenPosition.X, vector11.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.gemChainTexture[num79].Width, Main.gemChainTexture[num79].Height)), color11, num82, new Vector2((float)Main.gemChainTexture[num79].Width * 0.5f, (float)Main.gemChainTexture[num79].Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 256)
			{
				Vector2 vector12 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num84 = mountedCenter.X - vector12.X;
				float num85 = mountedCenter.Y - vector12.Y;
				float num86 = (float)Math.Atan2((double)num85, (double)num84) - 1.57f;
				bool flag8 = true;
				while (flag8)
				{
					float num87 = (float)Math.Sqrt((double)(num84 * num84 + num85 * num85));
					if (num87 < 26f)
					{
						flag8 = false;
					}
					else if (float.IsNaN(num87))
					{
						flag8 = false;
					}
					else
					{
						num87 = 26f / num87;
						num84 *= num87;
						num85 *= num87;
						vector12.X += num84;
						vector12.Y += num85;
						num84 = Main.player[projectile.owner].position.X + (float)(Main.player[projectile.owner].width / 2) - vector12.X;
						num85 = Main.player[projectile.owner].position.Y + (float)(Main.player[projectile.owner].height / 2) - vector12.Y;
						Color color12 = Lighting.GetColor((int)vector12.X / 16, (int)(vector12.Y / 16f));
						Main.spriteBatch.Draw(Main.chain20Texture, new Vector2(vector12.X - Main.screenPosition.X, vector12.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain20Texture.Width, Main.chain20Texture.Height)), color12, num86 - 0.785f, new Vector2((float)Main.chain20Texture.Width * 0.5f, (float)Main.chain20Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 322)
			{
				Vector2 vector13 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num88 = mountedCenter.X - vector13.X;
				float num89 = mountedCenter.Y - vector13.Y;
				float num90 = (float)Math.Atan2((double)num89, (double)num88) - 1.57f;
				bool flag9 = true;
				while (flag9)
				{
					float num91 = (float)Math.Sqrt((double)(num88 * num88 + num89 * num89));
					if (num91 < 22f)
					{
						flag9 = false;
					}
					else if (float.IsNaN(num91))
					{
						flag9 = false;
					}
					else
					{
						num91 = 22f / num91;
						num88 *= num91;
						num89 *= num91;
						vector13.X += num88;
						vector13.Y += num89;
						num88 = mountedCenter.X - vector13.X;
						num89 = mountedCenter.Y - vector13.Y;
						Color color13 = Lighting.GetColor((int)vector13.X / 16, (int)(vector13.Y / 16f));
						Main.spriteBatch.Draw(Main.chain29Texture, new Vector2(vector13.X - Main.screenPosition.X, vector13.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain29Texture.Width, Main.chain29Texture.Height)), color13, num90, new Vector2((float)Main.chain29Texture.Width * 0.5f, (float)Main.chain29Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 315)
			{
				Vector2 vector14 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num92 = mountedCenter.X - vector14.X;
				float num93 = mountedCenter.Y - vector14.Y;
				float num94 = (float)Math.Atan2((double)num93, (double)num92) - 1.57f;
				bool flag10 = true;
				while (flag10)
				{
					float num95 = (float)Math.Sqrt((double)(num92 * num92 + num93 * num93));
					if (num95 < 50f)
					{
						flag10 = false;
					}
					else if (float.IsNaN(num95))
					{
						flag10 = false;
					}
					else
					{
						num95 = 40f / num95;
						num92 *= num95;
						num93 *= num95;
						vector14.X += num92;
						vector14.Y += num93;
						num92 = mountedCenter.X - vector14.X;
						num93 = mountedCenter.Y - vector14.Y;
						Color color14 = Lighting.GetColor((int)vector14.X / 16, (int)(vector14.Y / 16f));
						Main.spriteBatch.Draw(Main.chain28Texture, new Vector2(vector14.X - Main.screenPosition.X, vector14.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain28Texture.Width, Main.chain28Texture.Height)), color14, num94, new Vector2((float)Main.chain28Texture.Width * 0.5f, (float)Main.chain28Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 331)
			{
				Vector2 vector15 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num96 = mountedCenter.X - vector15.X;
				float num97 = mountedCenter.Y - vector15.Y;
				float num98 = (float)Math.Atan2((double)num97, (double)num96) - 1.57f;
				bool flag11 = true;
				while (flag11)
				{
					float num99 = (float)Math.Sqrt((double)(num96 * num96 + num97 * num97));
					if (num99 < 30f)
					{
						flag11 = false;
					}
					else if (float.IsNaN(num99))
					{
						flag11 = false;
					}
					else
					{
						num99 = 24f / num99;
						num96 *= num99;
						num97 *= num99;
						vector15.X += num96;
						vector15.Y += num97;
						num96 = mountedCenter.X - vector15.X;
						num97 = mountedCenter.Y - vector15.Y;
						Color color15 = Lighting.GetColor((int)vector15.X / 16, (int)(vector15.Y / 16f));
						Main.spriteBatch.Draw(Main.chain30Texture, new Vector2(vector15.X - Main.screenPosition.X, vector15.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain30Texture.Width, Main.chain30Texture.Height)), color15, num98, new Vector2((float)Main.chain30Texture.Width * 0.5f, (float)Main.chain30Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 332)
			{
				int num100 = 0;
				Vector2 vector16 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num101 = mountedCenter.X - vector16.X;
				float num102 = mountedCenter.Y - vector16.Y;
				float num103 = (float)Math.Atan2((double)num102, (double)num101) - 1.57f;
				bool flag12 = true;
				while (flag12)
				{
					float num104 = (float)Math.Sqrt((double)(num101 * num101 + num102 * num102));
					if (num104 < 30f)
					{
						flag12 = false;
					}
					else if (float.IsNaN(num104))
					{
						flag12 = false;
					}
					else
					{
						int i2 = (int)vector16.X / 16;
						int j2 = (int)vector16.Y / 16;
						if (num100 == 0)
						{
							Lighting.AddLight(i2, j2, 0f, 0.2f, 0.2f);
						}
						if (num100 == 1)
						{
							Lighting.AddLight(i2, j2, 0.1f, 0.2f, 0f);
						}
						if (num100 == 2)
						{
							Lighting.AddLight(i2, j2, 0.2f, 0.1f, 0f);
						}
						if (num100 == 3)
						{
							Lighting.AddLight(i2, j2, 0.2f, 0f, 0.2f);
						}
						num104 = 16f / num104;
						num101 *= num104;
						num102 *= num104;
						vector16.X += num101;
						vector16.Y += num102;
						num101 = mountedCenter.X - vector16.X;
						num102 = mountedCenter.Y - vector16.Y;
						Color color16 = Lighting.GetColor((int)vector16.X / 16, (int)(vector16.Y / 16f));
						Main.spriteBatch.Draw(Main.chain31Texture, new Vector2(vector16.X - Main.screenPosition.X, vector16.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, Main.chain31Texture.Height / 4 * num100, Main.chain31Texture.Width, Main.chain31Texture.Height / 4)), color16, num103, new Vector2((float)Main.chain30Texture.Width * 0.5f, (float)(Main.chain30Texture.Height / 8)), 1f, 0, 0f);
						Main.spriteBatch.Draw(Main.chain32Texture, new Vector2(vector16.X - Main.screenPosition.X, vector16.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, Main.chain31Texture.Height / 4 * num100, Main.chain31Texture.Width, Main.chain31Texture.Height / 4)), new Color(200, 200, 200, 0), num103, new Vector2((float)Main.chain30Texture.Width * 0.5f, (float)(Main.chain30Texture.Height / 8)), 1f, 0, 0f);
						num100++;
						if (num100 > 3)
						{
							num100 = 0;
						}
					}
				}
			}
			else if (projectile.type == 372 || projectile.type == 383 || projectile.type == 396 || projectile.type == 403 || projectile.type == 404 || projectile.type == 446 || (projectile.type >= 486 && projectile.type <= 489) || (projectile.type >= 646 && projectile.type <= 649) || projectile.type == 652)
			{
				Texture2D texture2D = null;
				Color transparent = Color.Transparent;
				Texture2D texture2D2 = Main.chain33Texture;
				if (projectile.type == 383)
				{
					texture2D2 = Main.chain34Texture;
				}
				if (projectile.type == 396)
				{
					texture2D2 = Main.chain35Texture;
				}
				if (projectile.type == 403)
				{
					texture2D2 = Main.chain36Texture;
				}
				if (projectile.type == 404)
				{
					texture2D2 = Main.chain37Texture;
				}
				if (projectile.type == 446)
				{
					texture2D2 = Main.extraTexture[3];
				}
				if (projectile.type >= 486 && projectile.type <= 489)
				{
					texture2D2 = Main.chainsTexture[projectile.type - 486];
				}
				if (projectile.type >= 646 && projectile.type <= 649)
				{
					texture2D2 = Main.chainsTexture[projectile.type - 646 + 8];
					texture2D = Main.chainsTexture[projectile.type - 646 + 12];
					transparent = new Color(255, 255, 255, 127);
				}
				if (projectile.type == 652)
				{
					texture2D2 = Main.chainsTexture[16];
				}
				Vector2 vector17 = projectile.Center;
				Rectangle? rectangle = null;
				Vector2 vector18 = new Vector2((float)texture2D2.Width * 0.5f, (float)texture2D2.Height * 0.5f);
				float num105 = (float)texture2D2.Height;
				float num106 = 0f;
				if (projectile.type == 446)
				{
					int num107 = 7;
					int num108 = (int)projectile.localAI[0] / num107;
					rectangle = new Rectangle?(new Rectangle(0, texture2D2.Height / 4 * num108, texture2D2.Width, texture2D2.Height / 4));
					vector18.Y /= 4f;
					num105 /= 4f;
				}
				int type2 = projectile.type;
				if (type2 != 383)
				{
					if (type2 != 446)
					{
						switch (type2)
						{
						case 487:
							num106 = 8f;
							break;
						case 489:
							num106 = 10f;
							break;
						}
					}
					else
					{
						num106 = 20f;
					}
				}
				else
				{
					num106 = 14f;
				}
				if (num106 != 0f)
				{
					float num109 = -1.57f;
					Vector2 vector19 = new Vector2((float)Math.Cos((double)(projectile.rotation + num109)), (float)Math.Sin((double)(projectile.rotation + num109)));
					vector17 -= vector19 * num106;
					vector19 = mountedCenter - vector17;
					vector19.Normalize();
					vector17 -= vector19 * num105 / 2f;
				}
				Vector2 vector20 = mountedCenter - vector17;
				float num110 = (float)Math.Atan2((double)vector20.Y, (double)vector20.X) - 1.57f;
				bool flag13 = true;
				if (float.IsNaN(vector17.X) && float.IsNaN(vector17.Y))
				{
					flag13 = false;
				}
				if (float.IsNaN(vector20.X) && float.IsNaN(vector20.Y))
				{
					flag13 = false;
				}
				while (flag13)
				{
					float num111 = vector20.Length();
					if (num111 < num105 + 1f)
					{
						flag13 = false;
					}
					else
					{
						Vector2 vector21 = vector20;
						vector21.Normalize();
						vector17 += vector21 * num105;
						vector20 = mountedCenter - vector17;
						Color color17 = Lighting.GetColor((int)vector17.X / 16, (int)(vector17.Y / 16f));
						if (projectile.type == 396)
						{
							color17 *= (float)(255 - projectile.alpha) / 255f;
						}
						if (projectile.type == 446)
						{
							color17 = projectile.GetAlpha(color17);
						}
						if (projectile.type == 488)
						{
							Lighting.AddLight(vector17, 0.2f, 0f, 0.175f);
							color17 = new Color(255, 255, 255, 255);
						}
						if (projectile.type >= 646 && projectile.type <= 649)
						{
							color17 = projectile.GetAlpha(color17);
						}
						Main.spriteBatch.Draw(texture2D2, vector17 - Main.screenPosition, rectangle, color17, num110, vector18, 1f, 0, 0f);
						if (texture2D != null)
						{
							Main.spriteBatch.Draw(texture2D, vector17 - Main.screenPosition, rectangle, transparent, num110, vector18, 1f, 0, 0f);
						}
					}
				}
			}
			else if (projectile.aiStyle == 7)
			{
				Vector2 vector22 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num112 = mountedCenter.X - vector22.X;
				float num113 = mountedCenter.Y - vector22.Y;
				float num114 = (float)Math.Atan2((double)num113, (double)num112) - 1.57f;
				bool flag14 = true;
				while (flag14)
				{
					float num115 = (float)Math.Sqrt((double)(num112 * num112 + num113 * num113));
					if (num115 < 25f)
					{
						flag14 = false;
					}
					else if (float.IsNaN(num115))
					{
						flag14 = false;
					}
					else
					{
						num115 = 12f / num115;
						num112 *= num115;
						num113 *= num115;
						vector22.X += num112;
						vector22.Y += num113;
						num112 = mountedCenter.X - vector22.X;
						num113 = mountedCenter.Y - vector22.Y;
						Color color18 = Lighting.GetColor((int)vector22.X / 16, (int)(vector22.Y / 16f));
						Main.spriteBatch.Draw(Main.chainTexture, new Vector2(vector22.X - Main.screenPosition.X, vector22.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chainTexture.Width, Main.chainTexture.Height)), color18, num114, new Vector2((float)Main.chainTexture.Width * 0.5f, (float)Main.chainTexture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 262)
			{
				float num116 = projectile.Center.X;
				float num117 = projectile.Center.Y;
				float num118 = projectile.velocity.X;
				float num119 = projectile.velocity.Y;
				float num120 = (float)Math.Sqrt((double)(num118 * num118 + num119 * num119));
				num120 = 4f / num120;
				if (projectile.ai[0] == 0f)
				{
					num116 -= projectile.velocity.X * num120;
					num117 -= projectile.velocity.Y * num120;
				}
				else
				{
					num116 += projectile.velocity.X * num120;
					num117 += projectile.velocity.Y * num120;
				}
				Vector2 vector23 = new Vector2(num116, num117);
				num118 = mountedCenter.X - vector23.X;
				num119 = mountedCenter.Y - vector23.Y;
				float num121 = (float)Math.Atan2((double)num119, (double)num118) - 1.57f;
				if (projectile.alpha == 0)
				{
					int num122 = -1;
					if (projectile.position.X + (float)(projectile.width / 2) < mountedCenter.X)
					{
						num122 = 1;
					}
					if (Main.player[projectile.owner].direction == 1)
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num119 * (float)num122), (double)(num118 * (float)num122));
					}
					else
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num119 * (float)num122), (double)(num118 * (float)num122));
					}
				}
				bool flag15 = true;
				while (flag15)
				{
					float num123 = (float)Math.Sqrt((double)(num118 * num118 + num119 * num119));
					if (num123 < 25f)
					{
						flag15 = false;
					}
					else if (float.IsNaN(num123))
					{
						flag15 = false;
					}
					else
					{
						num123 = 12f / num123;
						num118 *= num123;
						num119 *= num123;
						vector23.X += num118;
						vector23.Y += num119;
						num118 = mountedCenter.X - vector23.X;
						num119 = mountedCenter.Y - vector23.Y;
						Color color19 = Lighting.GetColor((int)vector23.X / 16, (int)(vector23.Y / 16f));
						Main.spriteBatch.Draw(Main.chain22Texture, new Vector2(vector23.X - Main.screenPosition.X, vector23.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain22Texture.Width, Main.chain22Texture.Height)), color19, num121, new Vector2((float)Main.chain22Texture.Width * 0.5f, (float)Main.chain22Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 273)
			{
				float num124 = projectile.Center.X;
				float num125 = projectile.Center.Y;
				float num126 = projectile.velocity.X;
				float num127 = projectile.velocity.Y;
				float num128 = (float)Math.Sqrt((double)(num126 * num126 + num127 * num127));
				num128 = 4f / num128;
				if (projectile.ai[0] == 0f)
				{
					num124 -= projectile.velocity.X * num128;
					num125 -= projectile.velocity.Y * num128;
				}
				else
				{
					num124 += projectile.velocity.X * num128;
					num125 += projectile.velocity.Y * num128;
				}
				Vector2 vector24 = new Vector2(num124, num125);
				num126 = mountedCenter.X - vector24.X;
				num127 = mountedCenter.Y - vector24.Y;
				float num129 = (float)Math.Atan2((double)num127, (double)num126) - 1.57f;
				if (projectile.alpha == 0)
				{
					int num130 = -1;
					if (projectile.position.X + (float)(projectile.width / 2) < mountedCenter.X)
					{
						num130 = 1;
					}
					if (Main.player[projectile.owner].direction == 1)
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num127 * (float)num130), (double)(num126 * (float)num130));
					}
					else
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num127 * (float)num130), (double)(num126 * (float)num130));
					}
				}
				bool flag16 = true;
				while (flag16)
				{
					float num131 = (float)Math.Sqrt((double)(num126 * num126 + num127 * num127));
					if (num131 < 25f)
					{
						flag16 = false;
					}
					else if (float.IsNaN(num131))
					{
						flag16 = false;
					}
					else
					{
						num131 = 12f / num131;
						num126 *= num131;
						num127 *= num131;
						vector24.X += num126;
						vector24.Y += num127;
						num126 = mountedCenter.X - vector24.X;
						num127 = mountedCenter.Y - vector24.Y;
						Color color20 = Lighting.GetColor((int)vector24.X / 16, (int)(vector24.Y / 16f));
						Main.spriteBatch.Draw(Main.chain23Texture, new Vector2(vector24.X - Main.screenPosition.X, vector24.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain23Texture.Width, Main.chain23Texture.Height)), color20, num129, new Vector2((float)Main.chain23Texture.Width * 0.5f, (float)Main.chain23Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 481)
			{
				float num132 = projectile.Center.X;
				float num133 = projectile.Center.Y;
				float num134 = projectile.velocity.X;
				float num135 = projectile.velocity.Y;
				float num136 = (float)Math.Sqrt((double)(num134 * num134 + num135 * num135));
				num136 = 4f / num136;
				if (projectile.ai[0] == 0f)
				{
					num132 -= projectile.velocity.X * num136;
					num133 -= projectile.velocity.Y * num136;
				}
				else
				{
					num132 += projectile.velocity.X * num136;
					num133 += projectile.velocity.Y * num136;
				}
				Vector2 vector25 = new Vector2(num132, num133);
				num134 = mountedCenter.X - vector25.X;
				num135 = mountedCenter.Y - vector25.Y;
				float num137 = (float)Math.Atan2((double)num135, (double)num134) - 1.57f;
				if (projectile.alpha == 0)
				{
					int num138 = -1;
					if (projectile.position.X + (float)(projectile.width / 2) < mountedCenter.X)
					{
						num138 = 1;
					}
					if (Main.player[projectile.owner].direction == 1)
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num135 * (float)num138), (double)(num134 * (float)num138));
					}
					else
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num135 * (float)num138), (double)(num134 * (float)num138));
					}
				}
				bool flag17 = true;
				while (flag17)
				{
					float num139 = 0.85f;
					float num140 = (float)Math.Sqrt((double)(num134 * num134 + num135 * num135));
					float num141 = num140;
					if ((double)num140 < (double)Main.chain40Texture.Height * 1.5)
					{
						flag17 = false;
					}
					else if (float.IsNaN(num140))
					{
						flag17 = false;
					}
					else
					{
						num140 = (float)Main.chain40Texture.Height * num139 / num140;
						num134 *= num140;
						num135 *= num140;
						vector25.X += num134;
						vector25.Y += num135;
						num134 = mountedCenter.X - vector25.X;
						num135 = mountedCenter.Y - vector25.Y;
						if (num141 > (float)(Main.chain40Texture.Height) * 2)
						{
							for (int num142 = 0; num142 < 2; num142++)
							{
								float num143 = 0.75f;
								float num144;
								if (num142 == 0)
								{
									num144 = Math.Abs(Main.player[projectile.owner].velocity.X);
								}
								else
								{
									num144 = Math.Abs(Main.player[projectile.owner].velocity.Y);
								}
								if (num144 > 10f)
								{
									num144 = 10f;
								}
								num144 /= 10f;
								num143 *= num144;
								num144 = num141 / 80f;
								if (num144 > 1f)
								{
									num144 = 1f;
								}
								num143 *= num144;
								if (num143 < 0f)
								{
									num143 = 0f;
								}
								if (!float.IsNaN(num143))
								{
									if (num142 == 0)
									{
										if (Main.player[projectile.owner].velocity.X < 0f && projectile.Center.X < mountedCenter.X)
										{
											num135 *= 1f - num143;
										}
										if (Main.player[projectile.owner].velocity.X > 0f && projectile.Center.X > mountedCenter.X)
										{
											num135 *= 1f - num143;
										}
									}
									else
									{
										if (Main.player[projectile.owner].velocity.Y < 0f && projectile.Center.Y < mountedCenter.Y)
										{
											num134 *= 1f - num143;
										}
										if (Main.player[projectile.owner].velocity.Y > 0f && projectile.Center.Y > mountedCenter.Y)
										{
											num134 *= 1f - num143;
										}
									}
								}
							}
						}
						Color color21 = Lighting.GetColor((int)vector25.X / 16, (int)(vector25.Y / 16f));
						Main.spriteBatch.Draw(Main.chain40Texture, new Vector2(vector25.X - Main.screenPosition.X, vector25.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain40Texture.Width, Main.chain40Texture.Height)), color21, num137, new Vector2((float)Main.chain40Texture.Width * 0.5f, (float)Main.chain40Texture.Height * 0.5f), num139, 0, 0f);
					}
				}
			}
			else if (projectile.type == 271)
			{
				float num145 = projectile.Center.X;
				float num146 = projectile.Center.Y;
				float num147 = projectile.velocity.X;
				float num148 = projectile.velocity.Y;
				float num149 = (float)Math.Sqrt((double)(num147 * num147 + num148 * num148));
				num149 = 4f / num149;
				if (projectile.ai[0] == 0f)
				{
					num145 -= projectile.velocity.X * num149;
					num146 -= projectile.velocity.Y * num149;
				}
				else
				{
					num145 += projectile.velocity.X * num149;
					num146 += projectile.velocity.Y * num149;
				}
				Vector2 vector26 = new Vector2(num145, num146);
				num147 = mountedCenter.X - vector26.X;
				num148 = mountedCenter.Y - vector26.Y;
				float num150 = (float)Math.Atan2((double)num148, (double)num147) - 1.57f;
				if (projectile.alpha == 0)
				{
					int num151 = -1;
					if (projectile.position.X + (float)(projectile.width / 2) < mountedCenter.X)
					{
						num151 = 1;
					}
					if (Main.player[projectile.owner].direction == 1)
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num148 * (float)num151), (double)(num147 * (float)num151));
					}
					else
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num148 * (float)num151), (double)(num147 * (float)num151));
					}
				}
				bool flag18 = true;
				while (flag18)
				{
					float num152 = (float)Math.Sqrt((double)(num147 * num147 + num148 * num148));
					if (num152 < 25f)
					{
						flag18 = false;
					}
					else if (float.IsNaN(num152))
					{
						flag18 = false;
					}
					else
					{
						num152 = 12f / num152;
						num147 *= num152;
						num148 *= num152;
						vector26.X += num147;
						vector26.Y += num148;
						num147 = mountedCenter.X - vector26.X;
						num148 = mountedCenter.Y - vector26.Y;
						Color color22 = Lighting.GetColor((int)vector26.X / 16, (int)(vector26.Y / 16f));
						Main.spriteBatch.Draw(Main.chain18Texture, new Vector2(vector26.X - Main.screenPosition.X, vector26.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain18Texture.Width, Main.chain18Texture.Height)), color22, num150, new Vector2((float)Main.chain18Texture.Width * 0.5f, (float)Main.chain18Texture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.aiStyle == 13)
			{
				float num153 = projectile.position.X + 8f;
				float num154 = projectile.position.Y + 2f;
				float num155 = projectile.velocity.X;
				float num156 = projectile.velocity.Y;
				if (num155 == 0f && num156 == 0f)
				{
					num156 = 0.0001f;
				}
				float num157 = (float)Math.Sqrt((double)(num155 * num155 + num156 * num156));
				num157 = 20f / num157;
				if (projectile.ai[0] == 0f)
				{
					num153 -= projectile.velocity.X * num157;
					num154 -= projectile.velocity.Y * num157;
				}
				else
				{
					num153 += projectile.velocity.X * num157;
					num154 += projectile.velocity.Y * num157;
				}
				Vector2 vector27 = new Vector2(num153, num154);
				num155 = mountedCenter.X - vector27.X;
				num156 = mountedCenter.Y - vector27.Y;
				float num158 = (float)Math.Atan2((double)num156, (double)num155) - 1.57f;
				if (projectile.alpha == 0)
				{
					int num159 = -1;
					if (projectile.position.X + (float)(projectile.width / 2) < mountedCenter.X)
					{
						num159 = 1;
					}
					if (Main.player[projectile.owner].direction == 1)
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num156 * (float)num159), (double)(num155 * (float)num159));
					}
					else
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num156 * (float)num159), (double)(num155 * (float)num159));
					}
				}
				bool flag19 = true;
				while (flag19)
				{
					float num160 = (float)Math.Sqrt((double)(num155 * num155 + num156 * num156));
					if (num160 < 25f)
					{
						flag19 = false;
					}
					else if (float.IsNaN(num160))
					{
						flag19 = false;
					}
					else
					{
						num160 = 12f / num160;
						num155 *= num160;
						num156 *= num160;
						vector27.X += num155;
						vector27.Y += num156;
						num155 = mountedCenter.X - vector27.X;
						num156 = mountedCenter.Y - vector27.Y;
						Color color23 = Lighting.GetColor((int)vector27.X / 16, (int)(vector27.Y / 16f));
						Main.spriteBatch.Draw(Main.chainTexture, new Vector2(vector27.X - Main.screenPosition.X, vector27.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chainTexture.Width, Main.chainTexture.Height)), color23, num158, new Vector2((float)Main.chainTexture.Width * 0.5f, (float)Main.chainTexture.Height * 0.5f), 1f, 0, 0f);
					}
				}
			}
			else if (projectile.type == 190)
			{
				float num161 = projectile.position.X + (float)(projectile.width / 2);
				float num162 = projectile.position.Y + (float)(projectile.height / 2);
				float num163 = projectile.velocity.X;
				float num164 = projectile.velocity.Y;
				Math.Sqrt((double)(num163 * num163 + num164 * num164));
				Vector2 vector28 = new Vector2(num161, num162);
				num163 = mountedCenter.X - vector28.X;
				num164 = mountedCenter.Y + Main.player[projectile.owner].gfxOffY - vector28.Y;
				Math.Atan2((double)num164, (double)num163);
				if (projectile.alpha == 0)
				{
					int num165 = -1;
					if (projectile.position.X + (float)(projectile.width / 2) < mountedCenter.X)
					{
						num165 = 1;
					}
					if (Main.player[projectile.owner].direction == 1)
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num164 * (float)num165), (double)(num163 * (float)num165));
					}
					else
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num164 * (float)num165), (double)(num163 * (float)num165));
					}
				}
			}
			else if (projectile.aiStyle == 15)
			{
				Vector2 vector29 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num166 = mountedCenter.X - vector29.X;
				float num167 = mountedCenter.Y - vector29.Y;
				float num168 = (float)Math.Atan2((double)num167, (double)num166) - 1.57f;
				if (projectile.alpha == 0)
				{
					int num169 = -1;
					if (projectile.position.X + (float)(projectile.width / 2) < mountedCenter.X)
					{
						num169 = 1;
					}
					if (Main.player[projectile.owner].direction == 1)
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num167 * (float)num169), (double)(num166 * (float)num169));
					}
					else
					{
						Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(num167 * (float)num169), (double)(num166 * (float)num169));
					}
				}
				bool flag20 = true;
				while (flag20)
				{
					float num170 = (float)Math.Sqrt((double)(num166 * num166 + num167 * num167));
					if (num170 < 25f)
					{
						flag20 = false;
					}
					else if (float.IsNaN(num170))
					{
						flag20 = false;
					}
					else
					{
						if (projectile.type == 154 || projectile.type == 247)
						{
							num170 = 18f / num170;
						}
						else
						{
							num170 = 12f / num170;
						}
						num166 *= num170;
						num167 *= num170;
						vector29.X += num166;
						vector29.Y += num167;
						num166 = mountedCenter.X - vector29.X;
						num167 = mountedCenter.Y - vector29.Y;
						Color color24 = Lighting.GetColor((int)vector29.X / 16, (int)(vector29.Y / 16f));
						if (projectile.type == 25)
						{
							Main.spriteBatch.Draw(Main.chain2Texture, new Vector2(vector29.X - Main.screenPosition.X, vector29.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain2Texture.Width, Main.chain2Texture.Height)), color24, num168, new Vector2((float)Main.chain2Texture.Width * 0.5f, (float)Main.chain2Texture.Height * 0.5f), 1f, 0, 0f);
						}
						else if (projectile.type == 35)
						{
							Main.spriteBatch.Draw(Main.chain6Texture, new Vector2(vector29.X - Main.screenPosition.X, vector29.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain6Texture.Width, Main.chain6Texture.Height)), color24, num168, new Vector2((float)Main.chain6Texture.Width * 0.5f, (float)Main.chain6Texture.Height * 0.5f), 1f, 0, 0f);
						}
						else if (projectile.type == 247)
						{
							Main.spriteBatch.Draw(Main.chain19Texture, new Vector2(vector29.X - Main.screenPosition.X, vector29.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain19Texture.Width, Main.chain19Texture.Height)), color24, num168, new Vector2((float)Main.chain19Texture.Width * 0.5f, (float)Main.chain19Texture.Height * 0.5f), 1f, 0, 0f);
						}
						else if (projectile.type == 63)
						{
							Main.spriteBatch.Draw(Main.chain7Texture, new Vector2(vector29.X - Main.screenPosition.X, vector29.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain7Texture.Width, Main.chain7Texture.Height)), color24, num168, new Vector2((float)Main.chain7Texture.Width * 0.5f, (float)Main.chain7Texture.Height * 0.5f), 1f, 0, 0f);
						}
						else if (projectile.type == 154)
						{
							Main.spriteBatch.Draw(Main.chain13Texture, new Vector2(vector29.X - Main.screenPosition.X, vector29.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain13Texture.Width, Main.chain13Texture.Height)), color24, num168, new Vector2((float)Main.chain13Texture.Width * 0.5f, (float)Main.chain13Texture.Height * 0.5f), 1f, 0, 0f);
						}
						else
						{
							Main.spriteBatch.Draw(Main.chain3Texture, new Vector2(vector29.X - Main.screenPosition.X, vector29.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.chain3Texture.Width, Main.chain3Texture.Height)), color24, num168, new Vector2((float)Main.chain3Texture.Width * 0.5f, (float)Main.chain3Texture.Height * 0.5f), 1f, 0, 0f);
						}
					}
				}
			}
			Color color25 = Lighting.GetColor((int)((double)projectile.position.X + (double)projectile.width * 0.5) / 16, (int)(((double)projectile.position.Y + (double)projectile.height * 0.5) / 16.0));
			if (projectile.hide && !ProjectileID.Sets.DontAttachHideToAlpha[projectile.type])
			{
				color25 = Lighting.GetColor((int)mountedCenter.X / 16, (int)(mountedCenter.Y / 16f));
			}
			if (projectile.type == 14)
			{
				color25 = Color.White;
			}
			int num171 = 0;
			int num172 = 0;
			if (projectile.type == 175)
			{
				num171 = 10;
			}
			if (projectile.type == 392)
			{
				num171 = -2;
			}
			if (projectile.type == 499)
			{
				num171 = 12;
			}
			if (projectile.bobber)
			{
				num171 = 8;
			}
			if (projectile.type == 519)
			{
				num171 = 6;
				num172 -= 6;
			}
			if (projectile.type == 520)
			{
				num171 = 12;
			}
			if (projectile.type == 492)
			{
				num172 -= 4;
				num171 += 5;
			}
			if (projectile.type == 498)
			{
				num171 = 6;
			}
			if (projectile.type == 489)
			{
				num171 = -2;
			}
			if (projectile.type == 486)
			{
				num171 = -6;
			}
			if (projectile.type == 525)
			{
				num171 = 5;
			}
			if (projectile.type == 488)
			{
				num172 -= 8;
			}
			if (projectile.type == 373)
			{
				num172 = -10;
				num171 = 6;
			}
			if (projectile.type == 375)
			{
				num172 = -11;
				num171 = 12;
			}
			if (projectile.type == 423)
			{
				num172 = -5;
			}
			if (projectile.type == 346)
			{
				num171 = 4;
			}
			if (projectile.type == 331)
			{
				num172 = -4;
			}
			if (projectile.type == 254)
			{
				num171 = 3;
			}
			if (projectile.type == 273)
			{
				num172 = 2;
			}
			if (projectile.type == 335)
			{
				num171 = 6;
			}
			if (projectile.type == 162)
			{
				num171 = 1;
				num172 = 1;
			}
			if (projectile.type == 377)
			{
				num171 = -6;
			}
			if (projectile.type == 353)
			{
				num171 = 36;
				num172 = -12;
			}
			if (projectile.type == 324)
			{
				num171 = 22;
				num172 = -6;
			}
			if (projectile.type == 266)
			{
				num171 = 10;
				num172 = -10;
			}
			if (projectile.type == 319)
			{
				num171 = 10;
				num172 = -12;
			}
			if (projectile.type == 315)
			{
				num171 = -13;
				num172 = -6;
			}
			if (projectile.type == 313 && projectile.height != 54)
			{
				num172 = -12;
				num171 = 20;
			}
			if (projectile.type == 314)
			{
				num172 = -8;
				num171 = 0;
			}
			if (projectile.type == 269)
			{
				num171 = 18;
				num172 = -14;
			}
			if (projectile.type == 268)
			{
				num171 = 22;
				num172 = -2;
			}
			if (projectile.type == 18)
			{
				num171 = 3;
				num172 = 3;
			}
			if (projectile.type == 16)
			{
				num171 = 6;
			}
			if (projectile.type == 17 || projectile.type == 31)
			{
				num171 = 2;
			}
			if (projectile.type == 25 || projectile.type == 26 || projectile.type == 35 || projectile.type == 63 || projectile.type == 154)
			{
				num171 = 6;
				num172 -= 6;
			}
			if (projectile.type == 28 || projectile.type == 37 || projectile.type == 75)
			{
				num171 = 8;
			}
			if (projectile.type == 29 || projectile.type == 470 || projectile.type == 637)
			{
				num171 = 11;
			}
			if (projectile.type == 43)
			{
				num171 = 4;
			}
			if (projectile.type == 208)
			{
				num171 = 2;
				num172 -= 12;
			}
			if (projectile.type == 209)
			{
				num171 = 4;
				num172 -= 8;
			}
			if (projectile.type == 210)
			{
				num171 = 2;
				num172 -= 22;
			}
			if (projectile.type == 251)
			{
				num171 = 18;
				num172 -= 10;
			}
			if (projectile.type == 163 || projectile.type == 310)
			{
				num171 = 10;
			}
			if (projectile.type == 69 || projectile.type == 70)
			{
				num171 = 4;
				num172 = 4;
			}
			float num173 = (float)(Main.projectileTexture[projectile.type].Width - projectile.width) * 0.5f + (float)projectile.width * 0.5f;
			if (projectile.type == 50 || projectile.type == 53 || projectile.type == 515)
			{
				num172 = -8;
			}
			if (projectile.type == 473)
			{
				num172 = -6;
				num171 = 2;
			}
			if (projectile.type == 72 || projectile.type == 86 || projectile.type == 87)
			{
				num172 = -16;
				num171 = 8;
			}
			if (projectile.type == 74)
			{
				num172 = -6;
			}
			if (projectile.type == 99)
			{
				num171 = 1;
			}
			if (projectile.type == 655)
			{
				num171 = 1;
			}
			if (projectile.type == 111)
			{
				num171 = 18;
				num172 = -16;
			}
			if (projectile.type == 334)
			{
				num172 = -18;
				num171 = 8;
			}
			if (projectile.type == 200)
			{
				num171 = 12;
				num172 = -12;
			}
			if (projectile.type == 211)
			{
				num171 = 14;
				num172 = 0;
			}
			if (projectile.type == 236)
			{
				num171 = 30;
				num172 = -14;
			}
			if (projectile.type >= 191 && projectile.type <= 194)
			{
				num171 = 26;
				if (projectile.direction == 1)
				{
					num172 = -10;
				}
				else
				{
					num172 = -22;
				}
			}
			if (projectile.type >= 390 && projectile.type <= 392)
			{
				num172 = 4 * projectile.direction;
			}
			if (projectile.type == 112)
			{
				num171 = 12;
			}
			int arg_536F_0 = projectile.type;
			if (projectile.type == 517)
			{
				num171 = 6;
			}
			if (projectile.type == 516)
			{
				num171 = 6;
			}
			if (projectile.type == 127)
			{
				num171 = 8;
			}
			if (projectile.type == 155)
			{
				num171 = 3;
				num172 = 3;
			}
			if (projectile.type == 397)
			{
				num173 -= 1f;
				num171 = -2;
				num172 = -2;
			}
			if (projectile.type == 398)
			{
				num171 = 8;
			}
			SpriteEffects spriteEffects = 0;
			if (projectile.spriteDirection == -1)
			{
				spriteEffects = (SpriteEffects)1;
			}
			if (projectile.type == 221)
			{
				for (int num174 = 1; num174 < 10; num174++)
				{
					float num175 = projectile.velocity.X * (float)num174 * 0.5f;
					float num176 = projectile.velocity.Y * (float)num174 * 0.5f;
					Color alpha = projectile.GetAlpha(color25);
					float num177 = 0f;
					if (num174 == 1)
					{
						num177 = 0.9f;
					}
					if (num174 == 2)
					{
						num177 = 0.8f;
					}
					if (num174 == 3)
					{
						num177 = 0.7f;
					}
					if (num174 == 4)
					{
						num177 = 0.6f;
					}
					if (num174 == 5)
					{
						num177 = 0.5f;
					}
					if (num174 == 6)
					{
						num177 = 0.4f;
					}
					if (num174 == 7)
					{
						num177 = 0.3f;
					}
					if (num174 == 8)
					{
						num177 = 0.2f;
					}
					if (num174 == 9)
					{
						num177 = 0.1f;
					}
					alpha.R = ((byte)((float)alpha.R * num177));
					alpha.G = ((byte)((float)alpha.G * num177));
					alpha.B = ((byte)((float)alpha.B * num177));
					alpha.A = (byte)((float)alpha.A * num177);
					int num178 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
					int num179 = num178 * projectile.frame;
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172 - num175, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY - num176), new Rectangle?(new Rectangle(0, num179, Main.projectileTexture[projectile.type].Width, num178)), alpha, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
				}
			}
			if (projectile.type == 408 || projectile.type == 435 || projectile.type == 436 || projectile.type == 438 || projectile.type == 452 || projectile.type == 454 || projectile.type == 459 || projectile.type == 462 || projectile.type == 503 || projectile.type == 532 || projectile.type == 533 || projectile.type == 573 || projectile.type == 582 || projectile.type == 585 || projectile.type == 592 || projectile.type == 601 || projectile.type == 636 || projectile.type == 638 || projectile.type == 640 || projectile.type == 639 || projectile.type == 424 || projectile.type == 425 || projectile.type == 426)
			{
				Texture2D texture2D3 = Main.projectileTexture[projectile.type];
				int num180 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
				int num181 = num180 * projectile.frame;
				Rectangle rectangle2 = new Rectangle(0, num181, texture2D3.Width, num180);
				Vector2 vector30 = rectangle2.Size() / 2f;
				if (projectile.type == 503)
				{
					vector30.Y = 70f;
				}
				if (projectile.type == 438)
				{
				}
				if (projectile.type == 452)
				{
				}
				if (projectile.type == 408)
				{
				}
				if (projectile.type == 636)
				{
					vector30.Y = 10f;
				}
				if (projectile.type == 638)
				{
					vector30.Y = 2f;
				}
				if (projectile.type == 640 || projectile.type == 639)
				{
					vector30.Y = 5f;
				}
				int num182 = 8;
				int num183 = 2;
				float num184 = 1f;
				float num185 = 0f;
				if (projectile.type == 503)
				{
					num182 = 9;
					num183 = 3;
					num184 = 0.5f;
				}
				else if (projectile.type == 582)
				{
					num182 = 10;
					num183 = 2;
					num184 = 0.7f;
					num185 = 0.2f;
				}
				else if (projectile.type == 638)
				{
					num182 = 5;
					num183 = 1;
					num184 = 1f;
				}
				else if (projectile.type == 639)
				{
					num182 = 10;
					num183 = 1;
					num184 = 1f;
				}
				else if (projectile.type == 640)
				{
					num182 = 20;
					num183 = 1;
					num184 = 1f;
				}
				else if (projectile.type == 436)
				{
					num183 = 2;
					num184 = 0.5f;
				}
				else if (projectile.type == 424 || projectile.type == 425 || projectile.type == 426)
				{
					num182 = 10;
					num183 = 2;
					num184 = 0.6f;
				}
				else if (projectile.type == 438)
				{
					num182 = 10;
					num183 = 2;
					num184 = 1f;
				}
				else if (projectile.type == 452)
				{
					num182 = 10;
					num183 = 3;
					num184 = 0.5f;
				}
				else if (projectile.type == 454)
				{
					num182 = 5;
					num183 = 1;
					num184 = 0.2f;
				}
				else if (projectile.type == 462)
				{
					num182 = 7;
					num183 = 1;
					num184 = 0.2f;
				}
				else if (projectile.type == 585)
				{
					num182 = 7;
					num183 = 1;
					num184 = 0.2f;
				}
				else if (projectile.type == 459)
				{
					num182 = (int)(projectile.scale * 8f);
					num183 = num182 / 4;
					if (num183 < 1)
					{
						num183 = 1;
					}
					num184 = 0.3f;
				}
				else if (projectile.type == 532)
				{
					num182 = 10;
					num183 = 1;
					num184 = 0.7f;
					num185 = 0.2f;
				}
				else if (projectile.type == 592)
				{
					num182 = 10;
					num183 = 2;
					num184 = 1f;
				}
				else if (projectile.type == 601)
				{
					num182 = 8;
					num183 = 1;
					num184 = 0.3f;
				}
				else if (projectile.type == 636)
				{
					num182 = 20;
					num183 = 3;
					num184 = 0.5f;
				}
				else if (projectile.type == 533)
				{
					if (projectile.ai[0] >= 6f && projectile.ai[0] <= 8f)
					{
						num182 = ((projectile.ai[0] == 6f) ? 8 : 4);
						num183 = 1;
						if (projectile.ai[0] != 7f)
						{
							num185 = 0.2f;
						}
					}
					else
					{
						num183 = (num182 = 0);
					}
				}
				for (int num186 = 1; num186 < num182; num186 += num183)
				{
					Color color26 = color25;
					if (projectile.type == 408 || projectile.type == 435)
					{
						color26 = Color.Lerp(color26, Color.Blue, 0.5f);
					}
					else if (projectile.type == 436)
					{
						color26 = Color.Lerp(color26, Color.LimeGreen, 0.5f);
					}
					else if (projectile.type >= 424 && projectile.type <= 426)
					{
						color26 = Color.Lerp(color26, Color.Red, 0.5f);
					}
					else if (projectile.type == 640 || projectile.type == 639)
					{
						color26.A = 127;
					}
					color26 = projectile.GetAlpha(color26);
					if (projectile.type == 438)
					{
						color26.G = (byte)(color26.G / num186);
						color26.B = (byte)(color26.B / num186);
					}
					else if (projectile.type == 592)
					{
						color26.R = (byte)(color26.R / num186);
						color26.G = (byte)(color26.G / num186);
					}
					else if (projectile.type == 640)
					{
						color26.R = (byte)(color26.R / num186);
						color26.A = (byte)(color26.A / num186);
					}
					else if (projectile.type >= 424 && projectile.type <= 426)
					{
						color26.B = (byte)(color26.B / num186);
						color26.G = (byte)(color26.G / num186);
						color26.A = (byte)(color26.A / num186);
					}
					color26 *= (float)(num182 - num186) / ((float)ProjectileID.Sets.TrailCacheLength[projectile.type] * 1.5f);
					Vector2 vector31 = projectile.oldPos[num186];
					float num187 = projectile.rotation;
					SpriteEffects spriteEffects2 = spriteEffects;
					if (ProjectileID.Sets.TrailingMode[projectile.type] == 2)
					{
						num187 = projectile.oldRot[num186];
						spriteEffects2 = (SpriteEffects)((projectile.oldSpriteDirection[num186] == -1) ? 1 : 0);
					}
					Main.spriteBatch.Draw(texture2D3, vector31 + projectile.Size / 2f - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(rectangle2), color26, num187 + projectile.rotation * num185 * (float)(num186 - 1) * (float)(-(float)spriteEffects.HasFlag((SpriteEffects)1).ToDirectionInt()), vector30, MathHelper.Lerp(projectile.scale, num184, (float)num186 / 15f), spriteEffects2, 0f);
				}
				Color color27 = projectile.GetAlpha(color25);
				if (projectile.type == 640)
				{
					color27 = Color.Transparent;
				}
				Main.spriteBatch.Draw(texture2D3, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(rectangle2), color27, projectile.rotation, vector30, projectile.scale, spriteEffects, 0f);
				if (projectile.type == 503)
				{
					Main.spriteBatch.Draw(Main.extraTexture[36], projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(rectangle2), Color.White, projectile.localAI[0], vector30, projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 533)
				{
					Main.spriteBatch.Draw(Main.glowMaskTexture[128], projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(rectangle2), Color.White * 0.3f, projectile.rotation, vector30, projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 601)
				{
					Color white = Color.White;
					white.A = (0);
					Main.spriteBatch.Draw(texture2D3, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(rectangle2), white, projectile.rotation, vector30, projectile.scale * 0.7f, spriteEffects, 0f);
				}
			}
			else if (projectile.type == 440 || projectile.type == 449 || projectile.type == 606)
			{
				Rectangle rectangle3 = new Rectangle((int)Main.screenPosition.X - 500, (int)Main.screenPosition.Y - 500, Main.screenWidth + 1000, Main.screenHeight + 1000);
				if (projectile.getRect().Intersects(rectangle3))
				{
					Vector2 vector32 = new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY);
					float num188 = 100f;
					float num189 = 3f;
					if (projectile.type == 606)
					{
						num188 = 150f;
						num189 = 3f;
					}
					if (projectile.ai[1] == 1f)
					{
						num188 = (float)((int)projectile.localAI[0]);
					}
					for (int num190 = 1; num190 <= (int)projectile.localAI[0]; num190++)
					{
						Vector2 vector33 = Vector2.Normalize(projectile.velocity) * (float)num190 * num189;
						Color color28 = projectile.GetAlpha(color25);
						color28 *= (num188 - (float)num190) / num188;
						color28.A = (0);
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector32 - vector33, null, color28, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					}
				}
			}
			else if (projectile.type == 651)
			{
				Player player = Main.player[projectile.owner];
				Point point = new Vector2(projectile.ai[0], projectile.ai[1]).ToPoint();
				Point point2 = projectile.Center.ToTileCoordinates();
				Color color29 = new Color(255, 255, 255, 0);
				Color color30 = new Color(127, 127, 127, 0);
				int num191 = 1;
				float num192 = 0f;
				Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode toolMode = Terraria.GameContent.UI.WiresUI.Settings.ToolMode;
				bool flag21 = toolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Actuator);
				if (toolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Red))
				{
					num192 += 1f;
					color30 = Color.Lerp(color30, Color.Red, 1f / num192);
				}
				if (toolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Blue))
				{
					num192 += 1f;
					color30 = Color.Lerp(color30, Color.Blue, 1f / num192);
				}
				if (toolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Green))
				{
					num192 += 1f;
					color30 = Color.Lerp(color30, new Color(0, 255, 0), 1f / num192);
				}
				if (toolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Yellow))
				{
					num192 += 1f;
					color30 = Color.Lerp(color30, new Color(255, 255, 0), 1f / num192);
				}
				if (toolMode.HasFlag(Terraria.GameContent.UI.WiresUI.Settings.MultiToolMode.Cutter))
				{
					color29 = new Color(50, 50, 50, 255);
				}
				color30.A = (0);
				if (point == point2)
				{
					Vector2 vector34 = point2.ToVector2() * 16f - Main.screenPosition;
					Rectangle value = new Rectangle(0, 0, 16, 16);
					if (flag21)
					{
						Main.spriteBatch.Draw(Main.wireUITexture[11], vector34, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector34, new Rectangle?(value), color30, 0f, Vector2.Zero, 1f, 0, 0f);
					value.Y = 18;
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector34, new Rectangle?(value), color29, 0f, Vector2.Zero, 1f, 0, 0f);
				}
				else if (point.X == point2.X)
				{
					int num193 = point2.Y - point.Y;
					int num194 = Math.Sign(num193);
					Vector2 vector35 = point.ToVector2() * 16f - Main.screenPosition;
					Rectangle value2 = new Rectangle((num193 * num191 > 0) ? 72 : 18, 0, 16, 16);
					if (flag21)
					{
						Main.spriteBatch.Draw(Main.wireUITexture[11], vector35, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector35, new Rectangle?(value2), color30, 0f, Vector2.Zero, 1f, 0, 0f);
					value2.Y = 18;
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector35, new Rectangle?(value2), color29, 0f, Vector2.Zero, 1f, 0, 0f);
					for (int num195 = point.Y + num194; num195 != point2.Y; num195 += num194)
					{
						vector35 = new Vector2((float)(point.X * 16), (float)(num195 * 16)) - Main.screenPosition;
						value2.Y = 0;
						value2.X = 90;
						if (flag21)
						{
							Main.spriteBatch.Draw(Main.wireUITexture[11], vector35, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
						}
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector35, new Rectangle?(value2), color30, 0f, Vector2.Zero, 1f, 0, 0f);
						value2.Y = 18;
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector35, new Rectangle?(value2), color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					vector35 = point2.ToVector2() * 16f - Main.screenPosition;
					value2 = new Rectangle((num193 * num191 > 0) ? 18 : 72, 0, 16, 16);
					if (flag21)
					{
						Main.spriteBatch.Draw(Main.wireUITexture[11], vector35, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector35, new Rectangle?(value2), color30, 0f, Vector2.Zero, 1f, 0, 0f);
					value2.Y = 18;
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector35, new Rectangle?(value2), color29, 0f, Vector2.Zero, 1f, 0, 0f);
				}
				else if (point.Y == point2.Y)
				{
					int num196 = point2.X - point.X;
					int num197 = Math.Sign(num196);
					Vector2 vector36 = point.ToVector2() * 16f - Main.screenPosition;
					Rectangle value3 = new Rectangle((num196 > 0) ? 36 : 144, 0, 16, 16);
					if (flag21)
					{
						Main.spriteBatch.Draw(Main.wireUITexture[11], vector36, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector36, new Rectangle?(value3), color30, 0f, Vector2.Zero, 1f, 0, 0f);
					value3.Y = 18;
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector36, new Rectangle?(value3), color29, 0f, Vector2.Zero, 1f, 0, 0f);
					for (int num198 = point.X + num197; num198 != point2.X; num198 += num197)
					{
						vector36 = new Vector2((float)(num198 * 16), (float)(point.Y * 16)) - Main.screenPosition;
						value3.Y = 0;
						value3.X = 180;
						if (flag21)
						{
							Main.spriteBatch.Draw(Main.wireUITexture[11], vector36, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
						}
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector36, new Rectangle?(value3), color30, 0f, Vector2.Zero, 1f, 0, 0f);
						value3.Y = 18;
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector36, new Rectangle?(value3), color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					vector36 = point2.ToVector2() * 16f - Main.screenPosition;
					value3 = new Rectangle((num196 > 0) ? 144 : 36, 0, 16, 16);
					if (flag21)
					{
						Main.spriteBatch.Draw(Main.wireUITexture[11], vector36, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector36, new Rectangle?(value3), color30, 0f, Vector2.Zero, 1f, 0, 0f);
					value3.Y = 18;
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector36, new Rectangle?(value3), color29, 0f, Vector2.Zero, 1f, 0, 0f);
				}
				else
				{
					Math.Abs(point.X - point2.X);
					Math.Abs(point.Y - point2.Y);
					int num199 = Math.Sign(point2.X - point.X);
					int num200 = Math.Sign(point2.Y - point.Y);
					Point p = default(Point);
					bool flag22 = false;
					bool flag23 = player.direction == 1;
					int num201;
					int num202;
					int num203;
					if (flag23)
					{
						p.X = point.X;
						num201 = point.Y;
						num202 = point2.Y;
						num203 = num200;
					}
					else
					{
						p.Y = point.Y;
						num201 = point.X;
						num202 = point2.X;
						num203 = num199;
					}
					Vector2 vector37 = point.ToVector2() * 16f - Main.screenPosition;
					Rectangle value4 = new Rectangle(0, 0, 16, 16);
					if (!flag23)
					{
						value4.X = ((num203 > 0) ? 36 : 144);
					}
					else
					{
						value4.X = ((num203 > 0) ? 72 : 18);
					}
					if (flag21)
					{
						Main.spriteBatch.Draw(Main.wireUITexture[11], vector37, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color30, 0f, Vector2.Zero, 1f, 0, 0f);
					value4.Y = 18;
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color29, 0f, Vector2.Zero, 1f, 0, 0f);
					int num204 = num201 + num203;
					while (num204 != num202 && !flag22)
					{
						if (flag23)
						{
							p.Y = num204;
						}
						else
						{
							p.X = num204;
						}
						if (WorldGen.InWorld(p.X, p.Y, 1))
						{
							Tile tile = Main.tile[p.X, p.Y];
							if (tile != null)
							{
								vector37 = p.ToVector2() * 16f - Main.screenPosition;
								value4.Y = 0;
								if (!flag23)
								{
									value4.X = 180;
								}
								else
								{
									value4.X = 90;
								}
								if (flag21)
								{
									Main.spriteBatch.Draw(Main.wireUITexture[11], vector37, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
								}
								Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color30, 0f, Vector2.Zero, 1f, 0, 0f);
								value4.Y = 18;
								Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color29, 0f, Vector2.Zero, 1f, 0, 0f);
							}
						}
						num204 += num203;
					}
					if (flag23)
					{
						p.Y = point2.Y;
						num201 = point.X;
						num202 = point2.X;
						num203 = num199;
					}
					else
					{
						p.X = point2.X;
						num201 = point.Y;
						num202 = point2.Y;
						num203 = num200;
					}
					vector37 = p.ToVector2() * 16f - Main.screenPosition;
					value4 = new Rectangle(0, 0, 16, 16);
					if (!flag23)
					{
						value4.X += ((num199 > 0) ? 144 : 36);
						value4.X += ((num200 * num191 > 0) ? 72 : 18);
					}
					else
					{
						value4.X += ((num199 > 0) ? 36 : 144);
						value4.X += ((num200 * num191 > 0) ? 18 : 72);
					}
					if (flag21)
					{
						Main.spriteBatch.Draw(Main.wireUITexture[11], vector37, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color30, 0f, Vector2.Zero, 1f, 0, 0f);
					value4.Y = 18;
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color29, 0f, Vector2.Zero, 1f, 0, 0f);
					int num205 = num201 + num203;
					while (num205 != num202 && !flag22)
					{
						if (!flag23)
						{
							p.Y = num205;
						}
						else
						{
							p.X = num205;
						}
						if (WorldGen.InWorld(p.X, p.Y, 1))
						{
							Tile tile = Main.tile[p.X, p.Y];
							if (tile != null)
							{
								vector37 = p.ToVector2() * 16f - Main.screenPosition;
								value4.Y = 0;
								if (!flag23)
								{
									value4.X = 90;
								}
								else
								{
									value4.X = 180;
								}
								if (flag21)
								{
									Main.spriteBatch.Draw(Main.wireUITexture[11], vector37, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
								}
								Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color30, 0f, Vector2.Zero, 1f, 0, 0f);
								value4.Y = 18;
								Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color29, 0f, Vector2.Zero, 1f, 0, 0f);
							}
						}
						num205 += num203;
					}
					vector37 = point2.ToVector2() * 16f - Main.screenPosition;
					value4 = new Rectangle(0, 0, 16, 16);
					if (!flag23)
					{
						value4.X += ((num200 * num191 > 0) ? 18 : 72);
					}
					else
					{
						value4.X += ((num199 > 0) ? 144 : 36);
					}
					if (flag21)
					{
						Main.spriteBatch.Draw(Main.wireUITexture[11], vector37, null, color29, 0f, Vector2.Zero, 1f, 0, 0f);
					}
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color30, 0f, Vector2.Zero, 1f, 0, 0f);
					value4.Y = 18;
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], vector37, new Rectangle?(value4), color29, 0f, Vector2.Zero, 1f, 0, 0f);
				}
			}
			else if (projectile.type == 586)
			{
				float num206 = 300f;
				if (projectile.ai[0] >= 100f)
				{
					num206 = MathHelper.Lerp(300f, 600f, (projectile.ai[0] - 100f) / 200f);
				}
				if (num206 > 600f)
				{
					num206 = 600f;
				}
				if (projectile.ai[0] >= 500f)
				{
					num206 = MathHelper.Lerp(600f, 1200f, (projectile.ai[0] - 500f) / 100f);
				}
				float rotation = projectile.rotation;
				Texture2D texture2D4 = Main.projectileTexture[projectile.type];
				Color alpha2 = projectile.GetAlpha(color25);
				alpha2.A = (byte)(alpha2.A / 2);
				int num207 = (int)(projectile.ai[0] / 6f);
				Vector2 spinningpoint = new Vector2(0f, -num206);
				int num208 = 0;
				while ((float)num208 < 10f)
				{
					Rectangle rectangle4 = texture2D4.Frame(1, 5, 0, (num207 + num208) % 5);
					float num209 = rotation + 0.628318548f * (float)num208;
					Vector2 vector38 = spinningpoint.RotatedBy((double)num209, default(Vector2)) / 3f + projectile.Center - Main.screenPosition;
					Main.spriteBatch.Draw(texture2D4, vector38, new Rectangle?(rectangle4), alpha2, num209, rectangle4.Size() / 2f, projectile.scale, 0, 0f);
					num208++;
				}
				int num210 = 0;
				while ((float)num210 < 20f)
				{
					Rectangle rectangle5 = texture2D4.Frame(1, 5, 0, (num207 + num210) % 5);
					float num211 = -rotation + 0.314159274f * (float)num210;
					num211 *= 2f;
					Vector2 vector39 = spinningpoint.RotatedBy((double)num211, default(Vector2)) + projectile.Center - Main.screenPosition;
					Main.spriteBatch.Draw(texture2D4, vector39, new Rectangle?(rectangle5), alpha2, num211, rectangle5.Size() / 2f, projectile.scale, 0, 0f);
					num210++;
				}
			}
			else if (projectile.type == 536 || projectile.type == 591 || projectile.type == 607)
			{
				Texture2D texture2D5 = Main.projectileTexture[projectile.type];
				Vector2 vector40 = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Vector2 vector41 = new Vector2(1f, projectile.velocity.Length() / (float)texture2D5.Height);
				Main.spriteBatch.Draw(texture2D5, vector40, null, projectile.GetAlpha(color25), projectile.rotation, texture2D5.Frame(1, 1, 0, 0).Bottom(), vector41, spriteEffects, 0f);
			}
			else if (projectile.type == 409)
			{
				Texture2D texture2D6 = Main.projectileTexture[projectile.type];
				int num212 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
				int num213 = num212 * projectile.frame;
				int num214 = 10;
				int num215 = 2;
				float num216 = 0.5f;
				for (int num217 = 1; num217 < num214; num217 += num215)
				{
					Vector2 arg_7A45_0 = Main.npc[i].oldPos[num217];
					Color color31 = color25;
					color31 = projectile.GetAlpha(color31);
					color31 *= (float)(num214 - num217) / 15f;
					//projectile.oldPos[num217] - Main.screenPosition + new Vector2(num173 + (float)num172, (float)(projectile.height / 2) + projectile.gfxOffY);
					Main.spriteBatch.Draw(texture2D6, projectile.oldPos[num217] + new Vector2((float)projectile.width, (float)projectile.height) / 2f - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(new Rectangle(0, num213, texture2D6.Width, num212)), color31, projectile.rotation, new Vector2((float)texture2D6.Width / 2f, (float)num212 / 2f), MathHelper.Lerp(projectile.scale, num216, (float)num217 / 15f), spriteEffects, 0f);
				}
				Main.spriteBatch.Draw(texture2D6, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(new Rectangle(0, num213, texture2D6.Width, num212)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)texture2D6.Width / 2f, (float)num212 / 2f), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 437)
			{
				Texture2D texture2D7 = Main.projectileTexture[projectile.type];
				int num218 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
				int num219 = num218 * projectile.frame;
				int num220 = 10;
				int num221 = 2;
				float num222 = 0.2f;
				for (int num223 = 1; num223 < num220; num223 += num221)
				{
					Vector2 arg_7CBE_0 = Main.npc[i].oldPos[num223];
					Color color32 = color25;
					color32 = projectile.GetAlpha(color32);
					color32 *= (float)(num220 - num223) / 15f;
					//projectile.oldPos[num223] - Main.screenPosition + new Vector2(num173 + (float)num172, (float)(projectile.height / 2) + projectile.gfxOffY);
					Main.spriteBatch.Draw(texture2D7, projectile.oldPos[num223] + new Vector2((float)projectile.width, (float)projectile.height) / 2f - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(new Rectangle(0, num219, texture2D7.Width, num218)), color32, projectile.rotation, new Vector2((float)texture2D7.Width / 2f, (float)num218 / 2f), MathHelper.Lerp(projectile.scale, num222, (float)num223 / 15f), spriteEffects, 0f);
				}
				Main.spriteBatch.Draw(texture2D7, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(new Rectangle(0, num219, texture2D7.Width, num218)), Color.White, projectile.rotation, new Vector2((float)texture2D7.Width / 2f, (float)num218 / 2f), projectile.scale + 0.2f, spriteEffects, 0f);
				Main.spriteBatch.Draw(texture2D7, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(new Rectangle(0, num219, texture2D7.Width, num218)), projectile.GetAlpha(Color.White), projectile.rotation, new Vector2((float)texture2D7.Width / 2f, (float)num218 / 2f), projectile.scale + 0.2f, spriteEffects, 0f);
			}
			else if (projectile.type == 384 || projectile.type == 386)
			{
				Texture2D texture2D8 = Main.projectileTexture[projectile.type];
				int num224 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
				int num225 = num224 * projectile.frame;
				Main.spriteBatch.Draw(texture2D8, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(new Rectangle(0, num225, texture2D8.Width, num224)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)texture2D8.Width / 2f, (float)num224 / 2f), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 439 || projectile.type == 460 || projectile.type == 600 || projectile.type == 615 || projectile.type == 630 || projectile.type == 633)
			{
				Texture2D texture2D9 = Main.projectileTexture[projectile.type];
				int num226 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
				int num227 = num226 * projectile.frame;
				Vector2 vector42 = (projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition).Floor();
				float num228 = 1f;
				if (Main.player[projectile.owner].shroomiteStealth && Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].ranged)
				{
					float num229 = Main.player[projectile.owner].stealth;
					if ((double)num229 < 0.03)
					{
						num229 = 0.03f;
					}
					float arg_81A4_0 = (1f + num229 * 10f) / 11f;
					color25 *= num229;
					num228 = num229;
				}
				if (Main.player[projectile.owner].setVortex && Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].ranged)
				{
					float num230 = Main.player[projectile.owner].stealth;
					if ((double)num230 < 0.03)
					{
						num230 = 0.03f;
					}
					float arg_8245_0 = (1f + num230 * 10f) / 11f;
					color25 = color25.MultiplyRGBA(new Color(Vector4.Lerp(Vector4.One, new Vector4(0f, 0.12f, 0.16f, 0f), 1f - num230)));
					num228 = num230;
				}
				Main.spriteBatch.Draw(texture2D9, vector42, new Rectangle?(new Rectangle(0, num227, texture2D9.Width, num226)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)texture2D9.Width / 2f, (float)num226 / 2f), projectile.scale, spriteEffects, 0f);
				if (projectile.type == 439)
				{
					Main.spriteBatch.Draw(Main.glowMaskTexture[35], vector42, new Rectangle?(new Rectangle(0, num227, texture2D9.Width, num226)), new Color(255, 255, 255, 0) * num228, projectile.rotation, new Vector2((float)texture2D9.Width / 2f, (float)num226 / 2f), projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 615)
				{
					Main.spriteBatch.Draw(Main.glowMaskTexture[192], vector42, new Rectangle?(new Rectangle(0, num227, texture2D9.Width, num226)), new Color(255, 255, 255, 127) * num228, projectile.rotation, new Vector2((float)texture2D9.Width / 2f, (float)num226 / 2f), projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 630)
				{
					Main.spriteBatch.Draw(Main.glowMaskTexture[200], vector42, new Rectangle?(new Rectangle(0, num227, texture2D9.Width, num226)), new Color(255, 255, 255, 127) * num228, projectile.rotation, new Vector2((float)texture2D9.Width / 2f, (float)num226 / 2f), projectile.scale, spriteEffects, 0f);
					if (projectile.localAI[0] > 0f)
					{
						int frameY = 6 - (int)(projectile.localAI[0] / 1f);
						texture2D9 = Main.extraTexture[65];
						Main.spriteBatch.Draw(texture2D9, vector42 + Vector2.Normalize(projectile.velocity) * 2f, new Rectangle?(texture2D9.Frame(1, 6, 0, frameY)), new Color(255, 255, 255, 127) * num228, projectile.rotation, new Vector2((float)(spriteEffects.HasFlag((SpriteEffects)1) ? texture2D9.Width : 0), (float)num226 / 2f - 2f), projectile.scale, spriteEffects, 0f);
					}
				}
				else if (projectile.type == 600)
				{
					Color portalColor = Terraria.GameContent.PortalHelper.GetPortalColor(projectile.owner, (int)projectile.ai[1]);
					portalColor.A = (70);
					Main.spriteBatch.Draw(Main.glowMaskTexture[173], vector42, new Rectangle?(new Rectangle(0, num227, texture2D9.Width, num226)), portalColor, projectile.rotation, new Vector2((float)texture2D9.Width / 2f, (float)num226 / 2f), projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 460)
				{
					if (Math.Abs(projectile.rotation - 1.57079637f) > 1.57079637f)
					{
						spriteEffects |= (SpriteEffects)2;
					}
					Main.spriteBatch.Draw(Main.glowMaskTexture[102], vector42, new Rectangle?(new Rectangle(0, num227, texture2D9.Width, num226)), new Color(255, 255, 255, 0), projectile.rotation - 1.57079637f, new Vector2((float)texture2D9.Width / 2f, (float)num226 / 2f), projectile.scale, spriteEffects, 0f);
					if (projectile.ai[0] > 180f && Main.projectile[(int)projectile.ai[1]].type == 461)
					{
						this.DrawProj((int)projectile.ai[1], Main.projectile[(int)projectile.ai[1]]);
					}
				}
				else if (projectile.type == 633)
				{
					float num231 = (float)Math.Cos((double)(6.28318548f * (projectile.ai[0] / 30f))) * 2f + 2f;
					if (projectile.ai[0] > 120f)
					{
						num231 = 4f;
					}
					for (float num232 = 0f; num232 < 4f; num232 += 1f)
					{
						Main.spriteBatch.Draw(texture2D9, vector42 + Vector2.UnitY.RotatedBy((double)(num232 * 6.28318548f / 4f), default(Vector2)) * num231, new Rectangle?(new Rectangle(0, num227, texture2D9.Width, num226)), projectile.GetAlpha(color25).MultiplyRGBA(new Color(255, 255, 255, 0)) * 0.03f, projectile.rotation, new Vector2((float)texture2D9.Width / 2f, (float)num226 / 2f), projectile.scale, spriteEffects, 0f);
					}
				}
			}
			else if (projectile.type == 442)
			{
				Texture2D texture2D10 = Main.projectileTexture[projectile.type];
				int num233 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
				int num234 = num233 * projectile.frame;
				Vector2 vector43 = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Main.spriteBatch.Draw(texture2D10, vector43, new Rectangle?(new Rectangle(0, num234, texture2D10.Width, num233)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)texture2D10.Width / 2f, (float)num233 / 2f), projectile.scale, spriteEffects, 0f);
				Main.spriteBatch.Draw(Main.glowMaskTexture[37], vector43, new Rectangle?(new Rectangle(0, num234, texture2D10.Width, num233)), new Color(255, 255, 255, 0) * (1f - (float)projectile.alpha / 255f), projectile.rotation, new Vector2((float)texture2D10.Width / 2f, (float)num233 / 2f), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 447)
			{
				Texture2D texture2D11 = Main.projectileTexture[projectile.type];
				Texture2D texture2D12 = Main.extraTexture[4];
				int num235 = texture2D11.Height / Main.projFrames[projectile.type];
				int y = num235 * projectile.frame;
				int num236 = texture2D12.Height / Main.projFrames[projectile.type];
				int num237 = num236 * projectile.frame;
				Rectangle value5 = new Rectangle(0, num237, texture2D12.Width, num236);
				Vector2 vector44 = projectile.position + new Vector2((float)projectile.width, 0f) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Main.spriteBatch.Draw(Main.extraTexture[4], vector44, new Rectangle?(value5), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)(texture2D12.Width / 2), 0f), projectile.scale, spriteEffects, 0f);
				int num238 = projectile.height - num235 - 14;
				if (num238 < 0)
				{
					num238 = 0;
				}
				if (num238 > 0)
				{
					if (num237 == num236 * 3)
					{
						num237 = num236 * 2;
					}
					Main.spriteBatch.Draw(Main.extraTexture[4], vector44 + Vector2.UnitY * (float)(num236 - 1), new Rectangle?(new Rectangle(0, num237 + num236 - 1, texture2D12.Width, 1)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)(texture2D12.Width / 2), 0f), new Vector2(1f, (float)num238), spriteEffects, 0f);
				}
				value5.Width = texture2D11.Width;
				value5.Y = y;
				Main.spriteBatch.Draw(texture2D11, vector44 + Vector2.UnitY * (float)(num236 - 1 + num238), new Rectangle?(value5), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)texture2D11.Width / 2f, 0f), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 455)
			{
				if (projectile.velocity == Vector2.Zero)
				{
					return;
				}
				Texture2D texture2D13 = Main.projectileTexture[projectile.type];
				Texture2D texture2D14 = Main.extraTexture[21];
				Texture2D texture2D15 = Main.extraTexture[22];
				float num239 = projectile.localAI[1];
				Color color33 = new Color(255, 255, 255, 0) * 0.9f;
				Main.spriteBatch.Draw(texture2D13, projectile.Center - Main.screenPosition, null, color33, projectile.rotation, texture2D13.Size() / 2f, projectile.scale, 0, 0f);
				num239 -= (float)(texture2D13.Height / 2 + texture2D15.Height) * projectile.scale;
				Vector2 vector45 = projectile.Center;
				vector45 += projectile.velocity * projectile.scale * (float)texture2D13.Height / 2f;
				if (num239 > 0f)
				{
					float num240 = 0f;
					Rectangle value6 = new Rectangle(0, 16 * (projectile.timeLeft / 3 % 5), texture2D14.Width, 16);
					while (num240 + 1f < num239)
					{
						if (num239 - num240 < (float)value6.Height)
						{
							value6.Height = (int)(num239 - num240);
						}
						Main.spriteBatch.Draw(texture2D14, vector45 - Main.screenPosition, new Rectangle?(value6), color33, projectile.rotation, new Vector2((float)(value6.Width / 2), 0f), projectile.scale, 0, 0f);
						num240 += (float)value6.Height * projectile.scale;
						vector45 += projectile.velocity * (float)value6.Height * projectile.scale;
						value6.Y += 16;
						if (value6.Y + value6.Height > texture2D14.Height)
						{
							value6.Y = 0;
						}
					}
				}
				Main.spriteBatch.Draw(texture2D15, vector45 - Main.screenPosition, null, color33, projectile.rotation, texture2D15.Frame(1, 1, 0, 0).Top(), projectile.scale, 0, 0f);
			}
			else if (projectile.type == 461)
			{
				if (projectile.velocity == Vector2.Zero)
				{
					return;
				}
				Texture2D texture2D16 = Main.projectileTexture[projectile.type];
				float num241 = projectile.localAI[1];
				Color color34 = new Color(255, 255, 255, 0) * 0.9f;
				Rectangle rectangle6 = new Rectangle(0, 0, texture2D16.Width, 22);
				Vector2 vector46 = new Vector2(0f, Main.player[projectile.owner].gfxOffY);
				Main.spriteBatch.Draw(texture2D16, projectile.Center.Floor() - Main.screenPosition + vector46, new Rectangle?(rectangle6), color34, projectile.rotation, rectangle6.Size() / 2f, projectile.scale, 0, 0f);
				num241 -= 33f * projectile.scale;
				Vector2 vector47 = projectile.Center.Floor();
				vector47 += projectile.velocity * projectile.scale * 10.5f;
				rectangle6 = new Rectangle(0, 25, texture2D16.Width, 28);
				if (num241 > 0f)
				{
					float num242 = 0f;
					while (num242 + 1f < num241)
					{
						if (num241 - num242 < (float)rectangle6.Height)
						{
							rectangle6.Height = (int)(num241 - num242);
						}
						Main.spriteBatch.Draw(texture2D16, vector47 - Main.screenPosition + vector46, new Rectangle?(rectangle6), color34, projectile.rotation, new Vector2((float)(rectangle6.Width / 2), 0f), projectile.scale, 0, 0f);
						num242 += (float)rectangle6.Height * projectile.scale;
						vector47 += projectile.velocity * (float)rectangle6.Height * projectile.scale;
					}
				}
				rectangle6 = new Rectangle(0, 56, texture2D16.Width, 22);
				Main.spriteBatch.Draw(texture2D16, vector47 - Main.screenPosition + vector46, new Rectangle?(rectangle6), color34, projectile.rotation, texture2D16.Frame(1, 1, 0, 0).Top(), projectile.scale, 0, 0f);
			}
			else if (projectile.type == 632)
			{
				if (projectile.velocity == Vector2.Zero)
				{
					return;
				}
				Texture2D tex = Main.projectileTexture[projectile.type];
				float num243 = projectile.localAI[1];
				float prismHue = projectile.GetPrismHue(projectile.ai[0]);
				Color color35 = Main.hslToRgb(prismHue, 1f, 0.5f);
				color35.A = (0);
				Vector2 vector48 = projectile.Center.Floor();
				vector48 += projectile.velocity * projectile.scale * 10.5f;
				num243 -= projectile.scale * 14.5f * projectile.scale;
				Vector2 vector49 = new Vector2(projectile.scale);
				DelegateMethods.f_1 = 1f;
				DelegateMethods.c_1 = color35 * 0.75f * projectile.Opacity;
				//projectile.oldPos[0] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Utils.DrawLaser(Main.spriteBatch, tex, vector48 - Main.screenPosition, vector48 + projectile.velocity * num243 - Main.screenPosition, vector49, new Utils.LaserLineFraming(DelegateMethods.RainbowLaserDraw));
				DelegateMethods.c_1 = new Color(255, 255, 255, 127) * 0.75f * projectile.Opacity;
				Utils.DrawLaser(Main.spriteBatch, tex, vector48 - Main.screenPosition, vector48 + projectile.velocity * num243 - Main.screenPosition, vector49 / 2f, new Utils.LaserLineFraming(DelegateMethods.RainbowLaserDraw));
			}
			else if (projectile.type == 642)
			{
				if (projectile.velocity == Vector2.Zero)
				{
					return;
				}
				Texture2D tex2 = Main.projectileTexture[projectile.type];
				float num244 = projectile.localAI[1];
				Color c_ = new Color(255, 255, 255, 127);
				Vector2 vector50 = projectile.Center.Floor();
				num244 -= projectile.scale * 10.5f;
				Vector2 vector51 = new Vector2(projectile.scale);
				DelegateMethods.f_1 = 1f;
				DelegateMethods.c_1 = c_;
				DelegateMethods.i_1 = 54000 - (int)Main.time / 2;
				//projectile.oldPos[0] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Utils.DrawLaser(Main.spriteBatch, tex2, vector50 - Main.screenPosition, vector50 + projectile.velocity * num244 - Main.screenPosition, vector51, new Utils.LaserLineFraming(DelegateMethods.TurretLaserDraw));
				DelegateMethods.c_1 = new Color(255, 255, 255, 127) * 0.75f * projectile.Opacity;
				Utils.DrawLaser(Main.spriteBatch, tex2, vector50 - Main.screenPosition, vector50 + projectile.velocity * num244 - Main.screenPosition, vector51 / 2f, new Utils.LaserLineFraming(DelegateMethods.TurretLaserDraw));
			}
			else if (projectile.type == 611)
			{
				//projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Texture2D texture2D17 = Main.projectileTexture[projectile.type];
				Color alpha3 = projectile.GetAlpha(color25);
				if (projectile.velocity == Vector2.Zero)
				{
					return;
				}
				float num245 = projectile.velocity.Length() + 16f;
				bool flag24 = num245 < 100f;
				Vector2 vector52 = Vector2.Normalize(projectile.velocity);
				Rectangle rectangle7 = new Rectangle(0, 2, texture2D17.Width, 40);
				Vector2 vector53 = new Vector2(0f, Main.player[projectile.owner].gfxOffY);
				float num246 = projectile.rotation + 3.14159274f;
				Main.spriteBatch.Draw(texture2D17, projectile.Center.Floor() - Main.screenPosition + vector53, new Rectangle?(rectangle7), alpha3, num246, rectangle7.Size() / 2f - Vector2.UnitY * 4f, projectile.scale, 0, 0f);
				num245 -= 40f * projectile.scale;
				Vector2 vector54 = projectile.Center.Floor();
				vector54 += vector52 * projectile.scale * 24f;
				rectangle7 = new Rectangle(0, 68, texture2D17.Width, 18);
				if (num245 > 0f)
				{
					float num247 = 0f;
					while (num247 + 1f < num245)
					{
						if (num245 - num247 < (float)rectangle7.Height)
						{
							rectangle7.Height = (int)(num245 - num247);
						}
						Main.spriteBatch.Draw(texture2D17, vector54 - Main.screenPosition + vector53, new Rectangle?(rectangle7), alpha3, num246, new Vector2((float)(rectangle7.Width / 2), 0f), projectile.scale, 0, 0f);
						num247 += (float)rectangle7.Height * projectile.scale;
						vector54 += vector52 * (float)rectangle7.Height * projectile.scale;
					}
				}
				Vector2 vector55 = vector54;
				vector54 = projectile.Center.Floor();
				vector54 += vector52 * projectile.scale * 24f;
				rectangle7 = new Rectangle(0, 46, texture2D17.Width, 18);
				int num248 = 18;
				if (flag24)
				{
					num248 = 9;
				}
				float num249 = num245;
				if (num245 > 0f)
				{
					float num250 = 0f;
					float num251 = num249 / (float)num248;
					num250 += num251 * 0.25f;
					vector54 += vector52 * num251 * 0.25f;
					for (int num252 = 0; num252 < num248; num252++)
					{
						float num253 = num251;
						if (num252 == 0)
						{
							num253 *= 0.75f;
						}
						Main.spriteBatch.Draw(texture2D17, vector54 - Main.screenPosition + vector53, new Rectangle?(rectangle7), alpha3, num246, new Vector2((float)(rectangle7.Width / 2), 0f), projectile.scale, 0, 0f);
						num250 += num253;
						vector54 += vector52 * num253;
					}
				}
				rectangle7 = new Rectangle(0, 90, texture2D17.Width, 48);
				Main.spriteBatch.Draw(texture2D17, vector55 - Main.screenPosition + vector53, new Rectangle?(rectangle7), alpha3, num246, texture2D17.Frame(1, 1, 0, 0).Top(), projectile.scale, 0, 0f);
			}
			else if (projectile.type == 537)
			{
				if (projectile.velocity == Vector2.Zero)
				{
					return;
				}
				Texture2D texture2D18 = Main.projectileTexture[projectile.type];
				float num254 = projectile.localAI[1];
				Color color36 = new Color(255, 255, 255, 0) * 0.9f;
				Rectangle rectangle8 = new Rectangle(0, 0, texture2D18.Width, 22);
				Vector2 vector56 = new Vector2(0f, Main.npc[(int)projectile.ai[1]].gfxOffY);
				Main.spriteBatch.Draw(texture2D18, projectile.Center.Floor() - Main.screenPosition + vector56, new Rectangle?(rectangle8), color36, projectile.rotation, rectangle8.Size() / 2f, projectile.scale, 0, 0f);
				num254 -= 33f * projectile.scale;
				Vector2 vector57 = projectile.Center.Floor();
				vector57 += projectile.velocity * projectile.scale * 10.5f;
				rectangle8 = new Rectangle(0, 25, texture2D18.Width, 28);
				if (num254 > 0f)
				{
					float num255 = 0f;
					while (num255 + 1f < num254)
					{
						if (num254 - num255 < (float)rectangle8.Height)
						{
							rectangle8.Height = (int)(num254 - num255);
						}
						Main.spriteBatch.Draw(texture2D18, vector57 - Main.screenPosition + vector56, new Rectangle?(rectangle8), color36, projectile.rotation, new Vector2((float)(rectangle8.Width / 2), 0f), projectile.scale, 0, 0f);
						num255 += (float)rectangle8.Height * projectile.scale;
						vector57 += projectile.velocity * (float)rectangle8.Height * projectile.scale;
					}
				}
				rectangle8 = new Rectangle(0, 56, texture2D18.Width, 22);
				Main.spriteBatch.Draw(texture2D18, vector57 - Main.screenPosition + vector56, new Rectangle?(rectangle8), color36, projectile.rotation, texture2D18.Frame(1, 1, 0, 0).Top(), projectile.scale, 0, 0f);
			}
			else if (projectile.type == 456)
			{
				Texture2D texture2D19 = Main.projectileTexture[projectile.type];
				Texture2D texture2D20 = Main.extraTexture[23];
				Texture2D texture2D21 = Main.extraTexture[24];
				Vector2 vector58 = new Vector2(0f, 216f);
				Vector2 vector59 = Main.npc[(int)Math.Abs(projectile.ai[0]) - 1].Center - projectile.Center + vector58;
				float num256 = vector59.Length();
				Vector2 vector60 = Vector2.Normalize(vector59);
				Rectangle rectangle9 = texture2D19.Frame(1, 1, 0, 0);
				rectangle9.Height /= 4;
				rectangle9.Y += projectile.frame * rectangle9.Height;
				color25 = Color.Lerp(color25, Color.White, 0.3f);
				Main.spriteBatch.Draw(texture2D19, projectile.Center - Main.screenPosition, new Rectangle?(rectangle9), projectile.GetAlpha(color25), projectile.rotation, rectangle9.Size() / 2f, projectile.scale, 0, 0f);
				num256 -= (float)(rectangle9.Height / 2 + texture2D21.Height) * projectile.scale;
				Vector2 vector61 = projectile.Center;
				vector61 += vector60 * projectile.scale * (float)rectangle9.Height / 2f;
				if (num256 > 0f)
				{
					float num257 = 0f;
					Rectangle rectangle10 = new Rectangle(0, 0, texture2D20.Width, texture2D20.Height);
					while (num257 + 1f < num256)
					{
						if (num256 - num257 < (float)rectangle10.Height)
						{
							rectangle10.Height = (int)(num256 - num257);
						}
						Point point3 = vector61.ToTileCoordinates();
						Color color37 = Lighting.GetColor(point3.X, point3.Y);
						color37 = Color.Lerp(color37, Color.White, 0.3f);
						Main.spriteBatch.Draw(texture2D20, vector61 - Main.screenPosition, new Rectangle?(rectangle10), projectile.GetAlpha(color37), projectile.rotation, rectangle10.Bottom(), projectile.scale, 0, 0f);
						num257 += (float)rectangle10.Height * projectile.scale;
						vector61 += vector60 * (float)rectangle10.Height * projectile.scale;
					}
				}
				Point point4 = vector61.ToTileCoordinates();
				Color color38 = Lighting.GetColor(point4.X, point4.Y);
				color38 = Color.Lerp(color38, Color.White, 0.3f);
				Rectangle value7 = texture2D21.Frame(1, 1, 0, 0);
				if (num256 < 0f)
				{
					value7.Height += (int)num256;
				}
				Main.spriteBatch.Draw(texture2D21, vector61 - Main.screenPosition, new Rectangle?(value7), color38, projectile.rotation, new Vector2((float)value7.Width / 2f, (float)value7.Height), projectile.scale, 0, 0f);
			}
			else if (projectile.type == 443)
			{
				Texture2D texture2D22 = Main.projectileTexture[projectile.type];
				float num258 = 30f;
				float num259 = num258 * 4f;
				float num260 = 6.28318548f * projectile.ai[0] / num258;
				float num261 = 6.28318548f * projectile.ai[0] / num259;
				Vector2 vector62 = -Vector2.UnitY.RotatedBy((double)num260, default(Vector2));
				float num262 = 0.75f + vector62.Y * 0.25f;
				float num263 = 0.8f - vector62.Y * 0.2f;
				int num264 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
				int num265 = num264 * projectile.frame;
				Vector2 vector63 = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Main.spriteBatch.Draw(texture2D22, vector63, new Rectangle?(new Rectangle(0, num265, texture2D22.Width, num264)), projectile.GetAlpha(color25), projectile.rotation + num261, new Vector2((float)texture2D22.Width / 2f, (float)num264 / 2f), num262, spriteEffects, 0f);
				Main.spriteBatch.Draw(texture2D22, vector63, new Rectangle?(new Rectangle(0, num265, texture2D22.Width, num264)), projectile.GetAlpha(color25), projectile.rotation + (6.28318548f - num261), new Vector2((float)texture2D22.Width / 2f, (float)num264 / 2f), num263, spriteEffects, 0f);
			}
			else if (projectile.type == 444 || projectile.type == 446 || projectile.type == 490 || projectile.type == 464 || projectile.type == 502 || projectile.type == 538 || projectile.type == 540 || projectile.type == 579 || projectile.type == 578 || projectile.type == 583 || projectile.type == 584 || projectile.type == 616 || projectile.type == 617 || projectile.type == 618 || projectile.type == 641 || (projectile.type >= 646 && projectile.type <= 649) || projectile.type == 653 || projectile.type == 186)
			{
				Vector2 vector64 = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Texture2D texture2D23 = Main.projectileTexture[projectile.type];
				Color alpha4 = projectile.GetAlpha(color25);
				Vector2 vector65 = new Vector2((float)texture2D23.Width, (float)texture2D23.Height) / 2f;
				if (projectile.type == 446)
				{
					vector65.Y = 4f;
				}
				if (projectile.type == 502)
				{
					//Main.LoadProjectile(250);
					Texture2D texture2D24 = Main.projectileTexture[250];
					Vector2 vector66 = new Vector2((float)(texture2D24.Width / 2), 0f);
					Vector2 vector67 = new Vector2((float)projectile.width, (float)projectile.height) / 2f;
					Color white2 = Color.White;
					white2.A = (127);
					for (int num266 = projectile.oldPos.Length - 1; num266 > 0; num266--)
					{
						Vector2 vector68 = projectile.oldPos[num266] + vector67;
						if (!(vector68 == vector67))
						{
							Vector2 vector69 = projectile.oldPos[num266 - 1] + vector67;
							float num267 = (vector69 - vector68).ToRotation() - 1.57079637f;
							Vector2 vector70 = new Vector2(1f, Vector2.Distance(vector68, vector69) / (float)texture2D24.Height);
							Color color39 = white2 * (1f - (float)num266 / (float)projectile.oldPos.Length);
							Main.spriteBatch.Draw(texture2D24, vector68 - Main.screenPosition, null, color39, num267, vector66, vector70, spriteEffects, 0f);
						}
					}
				}
				else if (projectile.type == 540 && projectile.velocity != Vector2.Zero)
				{
					float num268 = 0f;
					if (projectile.ai[0] >= 10f)
					{
						num268 = (projectile.ai[0] - 10f) / 10f;
					}
					if (projectile.ai[0] >= 20f)
					{
						num268 = (20f - projectile.ai[0]) / 10f;
					}
					if (num268 > 1f)
					{
						num268 = 1f;
					}
					if (num268 < 0f)
					{
						num268 = 0f;
					}
					if (num268 != 0f)
					{
						Texture2D texture2D25 = Main.extraTexture[47];
						Vector2 vector71 = new Vector2((float)(texture2D25.Width / 2), 0f);
						Color color40 = alpha4 * num268 * 0.7f;
						Vector2 vector72 = projectile.Center - Main.screenPosition;
						Vector2 vector73 = projectile.velocity.ToRotation().ToRotationVector2() * (float)texture2D23.Width / 3f;
						vector73 = Vector2.Zero;
						vector72 += vector73;
						float num269 = projectile.velocity.ToRotation() - 1.57079637f;
						Vector2 vector74 = new Vector2(1f, (projectile.velocity.Length() - vector73.Length() * 2f) / (float)texture2D25.Height);
						Main.spriteBatch.Draw(texture2D25, vector72, null, color40, num269, vector71, vector74, 0, 0f);
					}
				}
				if (projectile.type == 578 || projectile.type == 579 || projectile.type == 641)
				{
					Color color41 = alpha4 * 0.8f;
					color41.A = (byte)(color41.A / 2);
					Color color42 = Color.Lerp(alpha4, Color.Black, 0.5f);
					color42.A = (alpha4.A);
					float num270 = 0.95f + (projectile.rotation * 0.75f).ToRotationVector2().Y * 0.1f;
					color42 *= num270;
					float num271 = 0.6f + projectile.scale * 0.6f * num270;
					Main.spriteBatch.Draw(Main.extraTexture[50], vector64, null, color42, -projectile.rotation + 0.35f, vector65, num271, spriteEffects ^ (SpriteEffects)1, 0f);
					Main.spriteBatch.Draw(Main.extraTexture[50], vector64, null, alpha4, -projectile.rotation, vector65, projectile.scale, spriteEffects ^ (SpriteEffects)1, 0f);
					Main.spriteBatch.Draw(texture2D23, vector64, null, color41, -projectile.rotation * 0.7f, vector65, projectile.scale, spriteEffects ^ (SpriteEffects)1, 0f);
					Main.spriteBatch.Draw(Main.extraTexture[50], vector64, null, alpha4 * 0.8f, projectile.rotation * 0.5f, vector65, projectile.scale * 0.9f, spriteEffects, 0f);
					alpha4.A = (0);
				}
				if (projectile.type == 617)
				{
					Color color43 = alpha4 * 0.8f;
					color43.A = (byte)(color43.A / 2);
					Color color44 = Color.Lerp(alpha4, Color.Black, 0.5f);
					color44.A = (alpha4.A);
					float num272 = 0.95f + (projectile.rotation * 0.75f).ToRotationVector2().Y * 0.1f;
					color44 *= num272;
					float num273 = 0.6f + projectile.scale * 0.6f * num272;
					Main.spriteBatch.Draw(Main.extraTexture[50], vector64, null, color44, -projectile.rotation + 0.35f, vector65, num273, spriteEffects ^ (SpriteEffects)1, 0f);
					Main.spriteBatch.Draw(Main.extraTexture[50], vector64, null, alpha4, -projectile.rotation, vector65, projectile.scale, spriteEffects ^ (SpriteEffects)1, 0f);
					Main.spriteBatch.Draw(texture2D23, vector64, null, color43, -projectile.rotation * 0.7f, vector65, projectile.scale, spriteEffects ^ (SpriteEffects)1, 0f);
					Main.spriteBatch.Draw(Main.extraTexture[50], vector64, null, alpha4 * 0.8f, projectile.rotation * 0.5f, vector65, projectile.scale * 0.9f, spriteEffects, 0f);
					alpha4.A = (0);
				}
				bool flag25 = false;
				if (!(flag25 | (projectile.type == 464 && projectile.ai[1] != 1f)))
				{
					Main.spriteBatch.Draw(texture2D23, vector64, null, alpha4, projectile.rotation, vector65, projectile.scale, spriteEffects, 0f);
				}
				if (projectile.type == 464 && projectile.ai[1] != 1f)
				{
					texture2D23 = Main.extraTexture[35];
					Rectangle rectangle11 = texture2D23.Frame(1, 3, 0, 0);
					vector65 = rectangle11.Size() / 2f;
					Vector2 vector75 = new Vector2(0f, -720f).RotatedBy((double)projectile.velocity.ToRotation(), default(Vector2));
					float num274 = projectile.ai[0] % 45f / 45f;
					Vector2 spinningpoint2 = vector75 * num274;
					for (int num275 = 0; num275 < 6; num275++)
					{
						float num276 = (float)num275 * 6.28318548f / 6f;
						Vector2 vector76 = projectile.Center + spinningpoint2.RotatedBy((double)num276, default(Vector2));
						Main.spriteBatch.Draw(texture2D23, vector76 - Main.screenPosition, new Rectangle?(rectangle11), alpha4, num276 + projectile.velocity.ToRotation() + 3.14159274f, vector65, projectile.scale, spriteEffects, 0f);
						rectangle11.Y += rectangle11.Height;
						if (rectangle11.Y >= texture2D23.Height)
						{
							rectangle11.Y = 0;
						}
					}
				}
				else if (projectile.type == 490)
				{
					Main.spriteBatch.Draw(Main.extraTexture[34], vector64, null, alpha4, -projectile.rotation, Main.extraTexture[34].Size() / 2f, projectile.scale, spriteEffects, 0f);
					Main.spriteBatch.Draw(texture2D23, vector64, null, alpha4, projectile.rotation, vector65, projectile.scale * 0.42f, spriteEffects, 0f);
					Main.spriteBatch.Draw(Main.extraTexture[34], vector64, null, alpha4, -projectile.rotation, Main.extraTexture[34].Size() / 2f, projectile.scale * 0.42f, spriteEffects, 0f);
				}
				else if (projectile.type == 616)
				{
					texture2D23 = Main.glowMaskTexture[193];
					Main.spriteBatch.Draw(texture2D23, vector64, null, new Color(127, 127, 127, 0), projectile.rotation, vector65, projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type >= 646 && projectile.type <= 649)
				{
					texture2D23 = Main.glowMaskTexture[203 + projectile.type - 646];
					Main.spriteBatch.Draw(texture2D23, vector64, null, new Color(255, 255, 255, 127), projectile.rotation, vector65, projectile.scale, spriteEffects, 0f);
				}
			}
			else if (projectile.type == 465 || projectile.type == 467 || projectile.type == 468 || projectile.type == 500 || projectile.type == 518 || projectile.type == 535 || projectile.type == 539 || projectile.type == 575 || projectile.type == 574 || projectile.type == 589 || projectile.type == 590 || projectile.type == 593 || projectile.type == 602 || projectile.type == 596 || projectile.type == 612 || projectile.type == 613 || projectile.type == 614 || projectile.type == 623 || projectile.type == 625 || projectile.type == 626 || projectile.type == 627 || projectile.type == 628 || projectile.type == 634 || projectile.type == 635 || projectile.type == 643 || projectile.type == 644 || projectile.type == 645 || projectile.type == 650 || projectile.type == 652)
			{
				Vector2 vector77 = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Texture2D texture2D26 = Main.projectileTexture[projectile.type];
				Rectangle rectangle12 = texture2D26.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
				Color alpha5 = projectile.GetAlpha(color25);
				Vector2 vector78 = rectangle12.Size() / 2f;
				if (projectile.type == 539)
				{
					if (projectile.ai[0] >= 210f)
					{
						float num277 = projectile.ai[0] - 210f;
						num277 /= 20f;
						if (num277 > 1f)
						{
							num277 = 1f;
						}
						Main.spriteBatch.Draw(Main.extraTexture[46], vector77, null, new Color(255, 255, 255, 128) * num277, projectile.rotation, new Vector2(17f, 22f), projectile.scale, spriteEffects, 0f);
					}
				}
				else if (projectile.type == 602)
				{
					vector78.X = (float)(rectangle12.Width - 6);
					vector78.Y -= 1f;
					rectangle12.Height -= 2;
				}
				else if (projectile.type == 589)
				{
					rectangle12 = texture2D26.Frame(5, 1, (int)projectile.ai[1], 0);
					vector78 = rectangle12.Size() / 2f;
				}
				else if (projectile.type == 590)
				{
					rectangle12 = texture2D26.Frame(3, 1, projectile.frame, 0);
					vector78 = rectangle12.Size() / 2f;
				}
				else if (projectile.type == 650)
				{
					vector78.Y -= 4f;
				}
				else if (projectile.type == 623)
				{
					alpha5.A = (byte)(alpha5.A / 2);
				}
				else if (projectile.type >= 625 && projectile.type <= 628)
				{
					alpha5.A = (byte)(alpha5.A / 2);
				}
				else if (projectile.type == 644)
				{
					Color color45 = Main.hslToRgb(projectile.ai[0], 1f, 0.5f).MultiplyRGBA(new Color(255, 255, 255, 0));
					Main.spriteBatch.Draw(texture2D26, vector77, new Rectangle?(rectangle12), color45, projectile.rotation, vector78, projectile.scale * 2f, spriteEffects, 0f);
					Main.spriteBatch.Draw(texture2D26, vector77, new Rectangle?(rectangle12), color45, 0f, vector78, projectile.scale * 2f, spriteEffects, 0f);
					if (projectile.ai[1] != -1f && projectile.Opacity > 0.3f)
					{
						Vector2 vector79 = Main.projectile[(int)projectile.ai[1]].Center - projectile.Center;
						Vector2 vector80 = new Vector2(1f, vector79.Length() / (float)texture2D26.Height);
						float num278 = vector79.ToRotation() + 1.57079637f;
						float num279 = MathHelper.Distance(30f, projectile.localAI[1]) / 20f;
						num279 = MathHelper.Clamp(num279, 0f, 1f);
						if (num279 > 0f)
						{
							Main.spriteBatch.Draw(texture2D26, vector77 + vector79 / 2f, new Rectangle?(rectangle12), color45 * num279, num278, vector78, vector80, spriteEffects, 0f);
							Main.spriteBatch.Draw(texture2D26, vector77 + vector79 / 2f, new Rectangle?(rectangle12), alpha5 * num279, num278, vector78, vector80 / 2f, spriteEffects, 0f);
						}
					}
				}
				Main.spriteBatch.Draw(texture2D26, vector77, new Rectangle?(rectangle12), alpha5, projectile.rotation, vector78, projectile.scale, spriteEffects, 0f);
				if (projectile.type == 535)
				{
					for (int num280 = 0; num280 < 1000; num280++)
					{
						if (Main.projectile[num280].active && Main.projectile[num280].owner == projectile.owner && Main.projectile[num280].type == 536)
						{
							this.DrawProj(num280, Main.projectile[num280]);
						}
					}
				}
				else if (projectile.type == 644)
				{
					Main.spriteBatch.Draw(texture2D26, vector77, new Rectangle?(rectangle12), alpha5, 0f, vector78, projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 602)
				{
					texture2D26 = Main.extraTexture[60];
					Color color46 = alpha5;
					color46.A = (0);
					color46 *= 0.3f;
					vector78 = texture2D26.Size() / 2f;
					Main.spriteBatch.Draw(texture2D26, vector77, null, color46, projectile.rotation - 1.57079637f, vector78, projectile.scale, spriteEffects, 0f);
					texture2D26 = Main.extraTexture[59];
					color46 = alpha5;
					color46.A = (0);
					color46 *= 0.13f;
					vector78 = texture2D26.Size() / 2f;
					Main.spriteBatch.Draw(texture2D26, vector77, null, color46, projectile.rotation - 1.57079637f, vector78, projectile.scale * 0.9f, spriteEffects, 0f);
				}
				else if (projectile.type == 539)
				{
					Main.spriteBatch.Draw(Main.glowMaskTexture[140], vector77, new Rectangle?(rectangle12), new Color(255, 255, 255, 0), projectile.rotation, vector78, projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 613)
				{
					Main.spriteBatch.Draw(Main.glowMaskTexture[189], vector77, new Rectangle?(rectangle12), new Color(128 - projectile.alpha / 2, 128 - projectile.alpha / 2, 128 - projectile.alpha / 2, 0), projectile.rotation, vector78, projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 614)
				{
					Main.spriteBatch.Draw(Main.glowMaskTexture[190], vector77, new Rectangle?(rectangle12), new Color(128 - projectile.alpha / 2, 128 - projectile.alpha / 2, 128 - projectile.alpha / 2, 0), projectile.rotation, vector78, projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 574)
				{
					Main.spriteBatch.Draw(Main.glowMaskTexture[148], vector77, new Rectangle?(rectangle12), new Color(255, 255, 255, 0), projectile.rotation, vector78, projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 590)
				{
					Main.spriteBatch.Draw(Main.glowMaskTexture[168], vector77, new Rectangle?(rectangle12), new Color(127 - projectile.alpha / 2, 127 - projectile.alpha / 2, 127 - projectile.alpha / 2, 0), projectile.rotation, vector78, projectile.scale, spriteEffects, 0f);
				}
				else if (projectile.type == 623 || (projectile.type >= 625 && projectile.type <= 628))
				{
					if (Main.player[projectile.owner].ghostFade != 0f)
					{
						float num281 = Main.player[projectile.owner].ghostFade * 5f;
						for (float num282 = 0f; num282 < 4f; num282 += 1f)
						{
							Main.spriteBatch.Draw(texture2D26, vector77 + Vector2.UnitY.RotatedBy((double)(num282 * 6.28318548f / 4f), default(Vector2)) * num281, new Rectangle?(rectangle12), alpha5 * 0.1f, projectile.rotation, vector78, projectile.scale, spriteEffects, 0f);
						}
					}
				}
				else if (projectile.type == 643)
				{
					float num283 = (float)Math.Cos((double)(6.28318548f * (projectile.localAI[0] / 60f))) + 3f + 3f;
					for (float num284 = 0f; num284 < 4f; num284 += 1f)
					{
						Main.spriteBatch.Draw(texture2D26, vector77 + Vector2.UnitY.RotatedBy((double)(num284 * 1.57079637f), default(Vector2)) * num283, new Rectangle?(rectangle12), alpha5 * 0.2f, projectile.rotation, vector78, projectile.scale, spriteEffects, 0f);
					}
				}
				else if (projectile.type == 650)
				{
					int num285 = (int)(projectile.localAI[0] / 6.28318548f);
					float f = projectile.localAI[0] % 6.28318548f - 3.14159274f;
					float num286 = (float)Math.IEEERemainder((double)projectile.localAI[1], 1.0);
					if (num286 < 0f)
					{
						num286 += 1f;
					}
					int num287 = (int)Math.Floor((double)projectile.localAI[1]);
					float num288 = 5f;
					float num289 = 1f + (float)num287 * 0.02f;
					if ((float)num285 == 1f)
					{
						num288 = 7f;
					}
					Vector2 vector81 = f.ToRotationVector2() * num286 * num288 * projectile.scale;
					texture2D26 = Main.extraTexture[66];
					Main.spriteBatch.Draw(texture2D26, vector77 + vector81, null, alpha5, projectile.rotation, texture2D26.Size() / 2f, num289, 0, 0f);
				}
			}
			else if (projectile.type == 466)
			{
				Vector2 end = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Texture2D tex3 = Main.extraTexture[33];
				projectile.GetAlpha(color25);
				Vector2 scale = new Vector2(projectile.scale) / 2f;
				for (int num290 = 0; num290 < 3; num290++)
				{
					if (num290 == 0)
					{
						scale = new Vector2(projectile.scale) * 0.6f;
						DelegateMethods.c_1 = new Color(115, 204, 219, 0) * 0.5f;
					}
					else if (num290 == 1)
					{
						scale = new Vector2(projectile.scale) * 0.4f;
						DelegateMethods.c_1 = new Color(113, 251, 255, 0) * 0.5f;
					}
					else
					{
						scale = new Vector2(projectile.scale) * 0.2f;
						DelegateMethods.c_1 = new Color(255, 255, 255, 0) * 0.5f;
					}
					DelegateMethods.f_1 = 1f;
					for (int num291 = projectile.oldPos.Length - 1; num291 > 0; num291--)
					{
						if (!(projectile.oldPos[num291] == Vector2.Zero))
						{
							Vector2 start = projectile.oldPos[num291] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
							Vector2 end2 = projectile.oldPos[num291 - 1] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
							Utils.DrawLaser(Main.spriteBatch, tex3, start, end2, scale, new Utils.LaserLineFraming(DelegateMethods.LightningLaserDraw));
						}
					}
					if (projectile.oldPos[0] != Vector2.Zero)
					{
						DelegateMethods.f_1 = 1f;
						Vector2 start2 = projectile.oldPos[0] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
						Utils.DrawLaser(Main.spriteBatch, tex3, start2, end, scale, new Utils.LaserLineFraming(DelegateMethods.LightningLaserDraw));
					}
				}
			}
			else if (projectile.type == 580)
			{
				Vector2 end3 = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Texture2D tex4 = Main.extraTexture[33];
				projectile.GetAlpha(color25);
				Vector2 scale2 = new Vector2(projectile.scale) / 2f;
				for (int num292 = 0; num292 < 2; num292++)
				{
					float num293 = (projectile.localAI[1] == -1f || projectile.localAI[1] == 1f) ? -0.2f : 0f;
					if (num292 == 0)
					{
						scale2 = new Vector2(projectile.scale) * (0.5f + num293);
						DelegateMethods.c_1 = new Color(115, 244, 219, 0) * 0.5f;
					}
					else
					{
						scale2 = new Vector2(projectile.scale) * (0.3f + num293);
						DelegateMethods.c_1 = new Color(255, 255, 255, 0) * 0.5f;
					}
					DelegateMethods.f_1 = 1f;
					for (int num294 = projectile.oldPos.Length - 1; num294 > 0; num294--)
					{
						if (!(projectile.oldPos[num294] == Vector2.Zero))
						{
							Vector2 start3 = projectile.oldPos[num294] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
							Vector2 end4 = projectile.oldPos[num294 - 1] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
							Utils.DrawLaser(Main.spriteBatch, tex4, start3, end4, scale2, new Utils.LaserLineFraming(DelegateMethods.LightningLaserDraw));
						}
					}
					if (projectile.oldPos[0] != Vector2.Zero)
					{
						DelegateMethods.f_1 = 1f;
						Vector2 start4 = projectile.oldPos[0] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
						Utils.DrawLaser(Main.spriteBatch, tex4, start4, end3, scale2, new Utils.LaserLineFraming(DelegateMethods.LightningLaserDraw));
					}
				}
			}
			else if (projectile.type == 445)
			{
				Vector2 vector82 = projectile.position + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
				Texture2D texture2D27 = Main.projectileTexture[projectile.type];
				Color alpha6 = projectile.GetAlpha(color25);
				Vector2 vector83 = Main.player[projectile.owner].RotatedRelativePoint(mountedCenter, true) + Vector2.UnitY * Main.player[projectile.owner].gfxOffY;
				Vector2 vector84 = vector82 + Main.screenPosition - vector83;
				Vector2 vector85 = Vector2.Normalize(vector84);
				float num295 = vector84.Length();
				float num296 = vector84.ToRotation() + 1.57079637f;
				float num297 = -5f;
				float num298 = num297 + 30f;
				new Vector2(2f, num295 - num298);
				Vector2 vector86 = Vector2.Lerp(vector82 + Main.screenPosition, vector83 + vector85 * num298, 0.5f);
				Vector2 vector87 = -Vector2.UnitY.RotatedBy((double)(projectile.localAI[0] / 60f * 3.14159274f), default(Vector2));
				Vector2[] array7 = new Vector2[]
				{
					vector87,
					vector87.RotatedBy(1.5707963705062866, default(Vector2)),
					vector87.RotatedBy(3.1415927410125732, default(Vector2)),
					vector87.RotatedBy(4.71238911151886, default(Vector2))
				};
				if (num295 > num298)
				{
					for (int num299 = 0; num299 < 2; num299++)
					{
						Color color47 = Color.White;
						if (num299 % 2 == 0)
						{
							color47 = Color.LimeGreen;
							color47.A = (128);
							color47 *= 0.5f;
						}
						else
						{
							color47 = Color.CornflowerBlue;
							color47.A = (128);
							color47 *= 0.5f;
						}
						Vector2 vector88 = new Vector2(array7[num299].X, 0f).RotatedBy((double)num296, default(Vector2)) * 4f;
						Main.spriteBatch.Draw(Main.magicPixel, vector86 - Main.screenPosition + vector88, new Rectangle?(new Rectangle(0, 0, 1, 1)), color47, num296, Vector2.One / 2f, new Vector2(2f, num295 - num298), spriteEffects, 0f);
					}
				}
				Texture2D texture2D28 = Main.itemTexture[Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].type];
				Color color48 = Lighting.GetColor((int)vector83.X / 16, (int)vector83.Y / 16);
				Main.spriteBatch.Draw(texture2D28, vector83 - Main.screenPosition + vector85 * num297, null, color48, projectile.rotation + 1.57079637f + ((spriteEffects == null) ? 3.14159274f : 0f), new Vector2((float)((spriteEffects == null) ? 0 : texture2D28.Width), (float)texture2D28.Height / 2f) + Vector2.UnitY * 1f, Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].scale, spriteEffects, 0f);
				Main.spriteBatch.Draw(Main.glowMaskTexture[39], vector83 - Main.screenPosition + vector85 * num297, null, new Color(255, 255, 255, 0), projectile.rotation + 1.57079637f + ((spriteEffects == null) ? 3.14159274f : 0f), new Vector2((float)((spriteEffects == null) ? 0 : texture2D28.Width), (float)texture2D28.Height / 2f) + Vector2.UnitY * 1f, Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].scale, spriteEffects, 0f);
				if (num295 > num298)
				{
					for (int num300 = 2; num300 < 4; num300++)
					{
						Color color49 = Color.White;
						if (num300 % 2 == 0)
						{
							color49 = Color.LimeGreen;
							color49.A = (128);
							color49 *= 0.5f;
						}
						else
						{
							color49 = Color.CornflowerBlue;
							color49.A = (128);
							color49 *= 0.5f;
						}
						Vector2 vector89 = new Vector2(array7[num300].X, 0f).RotatedBy((double)num296, default(Vector2)) * 4f;
						Main.spriteBatch.Draw(Main.magicPixel, vector86 - Main.screenPosition + vector89, new Rectangle?(new Rectangle(0, 0, 1, 1)), color49, num296, Vector2.One / 2f, new Vector2(2f, num295 - num298), spriteEffects, 0f);
					}
				}
				float num301 = projectile.localAI[0] / 60f;
				if (num301 > 0.5f)
				{
					num301 = 1f - num301;
				}
				Main.spriteBatch.Draw(texture2D27, vector82, null, alpha6 * num301 * 2f, projectile.rotation, new Vector2((float)texture2D27.Width, (float)texture2D27.Height) / 2f, projectile.scale, spriteEffects, 0f);
				Main.spriteBatch.Draw(Main.glowMaskTexture[40], vector82, null, alpha6 * (0.5f - num301) * 2f, projectile.rotation, new Vector2((float)texture2D27.Width, (float)texture2D27.Height) / 2f, projectile.scale, spriteEffects, 0f);
			}
			else if ((projectile.type >= 393 && projectile.type <= 395) || projectile.type == 398 || projectile.type == 423 || projectile.type == 450)
			{
				Texture2D texture2D29 = Main.projectileTexture[projectile.type];
				int num302 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
				int num303 = num302 * projectile.frame;
				Main.spriteBatch.Draw(texture2D29, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY - 2f), new Rectangle?(new Rectangle(0, num303, texture2D29.Width, num302)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)texture2D29.Width / 2f, (float)num302 / 2f), projectile.scale, spriteEffects, 0f);
				if (projectile.type == 398)
				{
					texture2D29 = Main.miniMinotaurTexture;
					Main.spriteBatch.Draw(texture2D29, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY - 2f), new Rectangle?(new Rectangle(0, num303, texture2D29.Width, num302)), new Color(250, 250, 250, projectile.alpha), projectile.rotation, new Vector2((float)texture2D29.Width / 2f, (float)num302 / 2f), projectile.scale, spriteEffects, 0f);
				}
				if (projectile.type == 423)
				{
					texture2D29 = Main.glowMaskTexture[0];
					Main.spriteBatch.Draw(texture2D29, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY - 2f), new Rectangle?(new Rectangle(0, num303, texture2D29.Width, num302)), new Color(250, 250, 250, projectile.alpha), projectile.rotation, new Vector2((float)texture2D29.Width / 2f, (float)num302 / 2f), projectile.scale, spriteEffects, 0f);
				}
			}
			else if (projectile.type == 385)
			{
				Texture2D texture2D30 = Main.projectileTexture[projectile.type];
				int num304 = texture2D30.Height / Main.projFrames[projectile.type];
				int num305 = num304 * projectile.frame;
				int num306 = 8;
				int num307 = 2;
				float num308 = 0.4f;
				for (int num309 = 1; num309 < num306; num309 += num307)
				{
					Vector2 arg_D05B_0 = projectile.oldPos[num309];
					Color color50 = color25;
					color50 = projectile.GetAlpha(color50);
					color50 *= (float)(num306 - num309) / 15f;
					Color alpha7 = projectile.GetAlpha(color25);
					//projectile.oldPos[num309] - Main.screenPosition + new Vector2(num173 + (float)num172, (float)(projectile.height / 2) + projectile.gfxOffY);
					Main.spriteBatch.Draw(texture2D30, projectile.oldPos[num309] + new Vector2((float)projectile.width, (float)projectile.height) / 2f - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(new Rectangle(0, num305, texture2D30.Width, num304)), Color.Lerp(alpha7, color50, 0.3f), projectile.rotation, new Vector2((float)texture2D30.Width / 2f, (float)num304 / 2f), MathHelper.Lerp(projectile.scale, num308, (float)num309 / 15f), spriteEffects, 0f);
				}
				Main.spriteBatch.Draw(texture2D30, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Rectangle?(new Rectangle(0, num305, texture2D30.Width, num304)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)texture2D30.Width / 2f, (float)num304 / 2f), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 388)
			{
				Texture2D texture2D31 = Main.projectileTexture[projectile.type];
				int num310 = texture2D31.Height / Main.projFrames[projectile.type];
				int num311 = num310 * projectile.frame;
				int num312;
				int num313;
				if (projectile.ai[0] == 2f)
				{
					num312 = 10;
					num313 = 1;
				}
				else
				{
					num313 = 2;
					num312 = 5;
				}
				for (int num314 = 1; num314 < num312; num314 += num313)
				{
					Vector2 arg_D304_0 = Main.npc[i].oldPos[num314];
					Color color51 = color25;
					color51 = projectile.GetAlpha(color51);
					color51 *= (float)(num312 - num314) / 15f;
					Vector2 vector90 = projectile.oldPos[num314] - Main.screenPosition + new Vector2(num173 + (float)num172, (float)(projectile.height / 2) + projectile.gfxOffY);
					Main.spriteBatch.Draw(texture2D31, vector90, new Rectangle?(new Rectangle(0, num311, texture2D31.Width, num310)), color51, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
				}
				Main.spriteBatch.Draw(texture2D31, projectile.position - Main.screenPosition + new Vector2(num173 + (float)num172, (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, num311, texture2D31.Width, num310)), projectile.GetAlpha(color25), projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
			}
			else if (Main.projFrames[projectile.type] > 1)
			{
				int num315 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
				int num316 = num315 * projectile.frame;
				if (projectile.type == 111)
				{
					int r = (int)Main.player[projectile.owner].shirtColor.R;
					int g = (int)Main.player[projectile.owner].shirtColor.G;
					int b = (int)Main.player[projectile.owner].shirtColor.B;
					Color oldColor = new Color((int)((byte)r), (int)((byte)g), (int)((byte)b));
					color25 = Lighting.GetColor((int)((double)projectile.position.X + (double)projectile.width * 0.5) / 16, (int)(((double)projectile.position.Y + (double)projectile.height * 0.5) / 16.0), oldColor);
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, num316, Main.projectileTexture[projectile.type].Width, num315)), projectile.GetAlpha(color25), projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
				}
				else
				{
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, num316, Main.projectileTexture[projectile.type].Width, num315 - 1)), projectile.GetAlpha(color25), projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					if (projectile.type == 387)
					{
						Main.spriteBatch.Draw(Main.eyeLaserSmallTexture, new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, num316, Main.projectileTexture[projectile.type].Width, num315)), new Color(255, 255, 255, 0), projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					}
				}
			}
			else if (projectile.type == 383 || projectile.type == 399)
			{
				Texture2D texture2D32 = Main.projectileTexture[projectile.type];
				Main.spriteBatch.Draw(texture2D32, projectile.Center - Main.screenPosition, null, projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)texture2D32.Width, (float)texture2D32.Height) / 2f, projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 157 || projectile.type == 378)
			{
				Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + (float)(projectile.width / 2), projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2)), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)(Main.projectileTexture[projectile.type].Width / 2), (float)(Main.projectileTexture[projectile.type].Height / 2)), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 306)
			{
				Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + (float)(projectile.width / 2), projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2)), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)(Main.projectileTexture[projectile.type].Width / 2), (float)(Main.projectileTexture[projectile.type].Height / 2)), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 256)
			{
				Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + (float)(projectile.width / 2), projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2)), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)(Main.projectileTexture[projectile.type].Width / 2), (float)(Main.projectileTexture[projectile.type].Height / 2)), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.aiStyle == 27)
			{
				Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + (float)(projectile.width / 2), projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2)), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)Main.projectileTexture[projectile.type].Width, 0f), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.aiStyle == 19)
			{
				Vector2 zero = Vector2.Zero;
				if (projectile.spriteDirection == -1)
				{
					zero.X = (float)Main.projectileTexture[projectile.type].Width;
				}
				Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + (float)(projectile.width / 2), projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), projectile.GetAlpha(color25), projectile.rotation, zero, projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 451)
			{
				Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, null, projectile.GetAlpha(color25), projectile.rotation, new Vector2((float)Main.projectileTexture[projectile.type].Width, 0f), projectile.scale, spriteEffects, 0f);
			}
			else if (projectile.type == 434)
			{
				Vector2 vector91 = new Vector2(projectile.ai[0], projectile.ai[1]);
				Vector2 v = projectile.position - vector91;
				float num317 = (float)Math.Sqrt((double)(v.X * v.X + v.Y * v.Y));
				new Vector2(4f, num317);
				float num318 = v.ToRotation() + 1.57079637f;
				Vector2 vector92 = Vector2.Lerp(projectile.position, vector91, 0.5f);
				Color color52 = Color.Red;
				color52.A = (0);
				Color color53 = Color.White;
				color52 *= projectile.localAI[0];
				color53 *= projectile.localAI[0];
				float num319 = (float)Math.Sqrt((double)(projectile.damage / 50));
				Main.spriteBatch.Draw(Main.magicPixel, vector92 - Main.screenPosition, new Rectangle?(new Rectangle(0, 0, 1, 1)), color52, num318, Vector2.One / 2f, new Vector2(2f * num319, num317 + 8f), spriteEffects, 0f);
				Main.spriteBatch.Draw(Main.magicPixel, vector92 - Main.screenPosition, new Rectangle?(new Rectangle(0, 0, 1, 1)), color52, num318, Vector2.One / 2f, new Vector2(4f * num319, num317), spriteEffects, 0f);
				Main.spriteBatch.Draw(Main.magicPixel, vector92 - Main.screenPosition, new Rectangle?(new Rectangle(0, 0, 1, 1)), color53, num318, Vector2.One / 2f, new Vector2(2f * num319, num317), spriteEffects, 0f);
			}
			else
			{
				if (projectile.type == 94 && projectile.ai[1] > 6f)
				{
					for (int num320 = 0; num320 < 10; num320++)
					{
						Color alpha8 = projectile.GetAlpha(color25);
						float num321 = (float)(9 - num320) / 9f;
						alpha8.R = ((byte)((float)alpha8.R * num321));
						alpha8.G = ((byte)((float)alpha8.G * num321));
						alpha8.B = ((byte)((float)alpha8.B * num321));
						alpha8.A = ((byte)((float)alpha8.A * num321));
						float num322 = (float)(9 - num320) / 9f;
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.oldPos[num320].X - Main.screenPosition.X + num173 + (float)num172, projectile.oldPos[num320].Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), alpha8, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), num322 * projectile.scale, spriteEffects, 0f);
					}
				}
				if (projectile.type == 301)
				{
					for (int num323 = 0; num323 < 10; num323++)
					{
						Color alpha9 = projectile.GetAlpha(color25);
						float num324 = (float)(9 - num323) / 9f;
						alpha9.R = ((byte)((float)alpha9.R * num324));
						alpha9.G = ((byte)((float)alpha9.G * num324));
						alpha9.B = ((byte)((float)alpha9.B * num324));
						alpha9.A = ((byte)((float)alpha9.A * num324));
						float num325 = (float)(9 - num323) / 9f;
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.oldPos[num323].X - Main.screenPosition.X + num173 + (float)num172, projectile.oldPos[num323].Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), alpha9, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), num325 * projectile.scale, spriteEffects, 0f);
					}
				}
				if (projectile.type == 323 && projectile.alpha == 0)
				{
					for (int num326 = 1; num326 < 8; num326++)
					{
						float num327 = projectile.velocity.X * (float)num326;
						float num328 = projectile.velocity.Y * (float)num326;
						Color alpha10 = projectile.GetAlpha(color25);
						float num329 = 0f;
						if (num326 == 1)
						{
							num329 = 0.7f;
						}
						if (num326 == 2)
						{
							num329 = 0.6f;
						}
						if (num326 == 3)
						{
							num329 = 0.5f;
						}
						if (num326 == 4)
						{
							num329 = 0.4f;
						}
						if (num326 == 5)
						{
							num329 = 0.3f;
						}
						if (num326 == 6)
						{
							num329 = 0.2f;
						}
						if (num326 == 7)
						{
							num329 = 0.1f;
						}
						alpha10.R = ((byte)((float)alpha10.R * num329));
						alpha10.G = ((byte)((float)alpha10.G * num329));
						alpha10.B = ((byte)((float)alpha10.B * num329));
						alpha10.A = ((byte)((float)alpha10.A * num329));
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172 - num327, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY - num328), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), alpha10, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), num329 + 0.2f, spriteEffects, 0f);
					}
				}
				if (projectile.type == 117 && projectile.ai[0] > 3f)
				{
					for (int num330 = 1; num330 < 5; num330++)
					{
						float num331 = projectile.velocity.X * (float)num330;
						float num332 = projectile.velocity.Y * (float)num330;
						Color alpha11 = projectile.GetAlpha(color25);
						float num333 = 0f;
						if (num330 == 1)
						{
							num333 = 0.4f;
						}
						if (num330 == 2)
						{
							num333 = 0.3f;
						}
						if (num330 == 3)
						{
							num333 = 0.2f;
						}
						if (num330 == 4)
						{
							num333 = 0.1f;
						}
						alpha11.R = ((byte)((float)alpha11.R * num333));
						alpha11.G = ((byte)((float)alpha11.G * num333));
						alpha11.B = ((byte)((float)alpha11.B * num333));
						alpha11.A = ((byte)((float)alpha11.A * num333));
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172 - num331, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY - num332), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), alpha11, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					}
				}
				if (projectile.bobber)
				{
					if (projectile.ai[1] > 0f && projectile.ai[1] < 3730f && projectile.ai[0] == 1f)
					{
						int num334 = (int)projectile.ai[1];
						Vector2 center = projectile.Center;
						float num335 = projectile.rotation;
						Vector2 vector93 = center;
						float num336 = num - vector93.X;
						float num337 = num2 - vector93.Y;
						num335 = (float)Math.Atan2((double)num337, (double)num336);
						if (projectile.velocity.X > 0f)
						{
							spriteEffects = 0;
							num335 = (float)Math.Atan2((double)num337, (double)num336);
							num335 += 0.785f;
							if (projectile.ai[1] == 2342f)
							{
								num335 -= 0.785f;
							}
						}
						else
						{
							spriteEffects = (SpriteEffects)1;
							num335 = (float)Math.Atan2((double)(-(double)num337), (double)(-(double)num336));
							num335 -= 0.785f;
							if (projectile.ai[1] == 2342f)
							{
								num335 += 0.785f;
							}
						}
						Main.spriteBatch.Draw(Main.itemTexture[num334], new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y), new Rectangle?(new Rectangle(0, 0, Main.itemTexture[num334].Width, Main.itemTexture[num334].Height)), color25, num335, new Vector2((float)(Main.itemTexture[num334].Width / 2), (float)(Main.itemTexture[num334].Height / 2)), projectile.scale, spriteEffects, 0f);
					}
					else if (projectile.ai[0] <= 1f)
					{
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), projectile.GetAlpha(color25), projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					}
				}
				else
				{
					Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), projectile.GetAlpha(color25), projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					if (projectile.glowMask != -1)
					{
						Main.spriteBatch.Draw(Main.glowMaskTexture[(int)projectile.glowMask], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), new Color(250, 250, 250, projectile.alpha), projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					}
					if (projectile.type == 473)
					{
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), new Color(255, 255, 0, 0), projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					}
				}
				if (projectile.type == 106)
				{
					Main.spriteBatch.Draw(Main.lightDiscTexture, new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2)), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), new Color(200, 200, 200, 0), projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
				}
				if (projectile.type == 554 || projectile.type == 603)
				{
					for (int num338 = 1; num338 < 5; num338++)
					{
						float num339 = projectile.velocity.X * (float)num338 * 0.5f;
						float num340 = projectile.velocity.Y * (float)num338 * 0.5f;
						Color alpha12 = projectile.GetAlpha(color25);
						float num341 = 0f;
						if (num338 == 1)
						{
							num341 = 0.4f;
						}
						if (num338 == 2)
						{
							num341 = 0.3f;
						}
						if (num338 == 3)
						{
							num341 = 0.2f;
						}
						if (num338 == 4)
						{
							num341 = 0.1f;
						}
						alpha12.R = ((byte)((float)alpha12.R * num341));
						alpha12.G = ((byte)((float)alpha12.G * num341));
						alpha12.B = ((byte)((float)alpha12.B * num341));
						alpha12.A = ((byte)((float)alpha12.A * num341));
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172 - num339, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY - num340), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), alpha12, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					}
				}
				else if (projectile.type == 604)
				{
					int num342 = (int)projectile.ai[1] + 1;
					if (num342 > 7)
					{
						num342 = 7;
					}
					for (int num343 = 1; num343 < num342; num343++)
					{
						float num344 = projectile.velocity.X * (float)num343 * 1.5f;
						float num345 = projectile.velocity.Y * (float)num343 * 1.5f;
						Color alpha13 = projectile.GetAlpha(color25);
						if (num343 == 1)
						{
						}
						if (num343 == 2)
						{
						}
						if (num343 == 3)
						{
						}
						if (num343 == 4)
						{
						}
						float num346 = 0.4f - (float)num343 * 0.06f;
						num346 *= 1f - (float)projectile.alpha / 255f;
						alpha13.R = ((byte)((float)alpha13.R * num346));
						alpha13.G = ((byte)((float)alpha13.G * num346));
						alpha13.B = ((byte)((float)alpha13.B * num346));
						alpha13.A = ((byte)((float)alpha13.A * num346 / 2f));
						float num347 = projectile.scale;
						num347 -= (float)num343 * 0.1f;
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172 - num344, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY - num345), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), alpha13, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), num347, spriteEffects, 0f);
					}
				}
				else if (projectile.type == 553)
				{
					for (int num348 = 1; num348 < 5; num348++)
					{
						float num349 = projectile.velocity.X * (float)num348 * 0.4f;
						float num350 = projectile.velocity.Y * (float)num348 * 0.4f;
						Color alpha14 = projectile.GetAlpha(color25);
						float num351 = 0f;
						if (num348 == 1)
						{
							num351 = 0.4f;
						}
						if (num348 == 2)
						{
							num351 = 0.3f;
						}
						if (num348 == 3)
						{
							num351 = 0.2f;
						}
						if (num348 == 4)
						{
							num351 = 0.1f;
						}
						alpha14.R = ((byte)((float)alpha14.R * num351));
						alpha14.G = ((byte)((float)alpha14.G * num351));
						alpha14.B = ((byte)((float)alpha14.B * num351));
						alpha14.A = ((byte)((float)alpha14.A * num351));
						Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], new Vector2(projectile.position.X - Main.screenPosition.X + num173 + (float)num172 - num349, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY - num350), new Rectangle?(new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height)), alpha14, projectile.rotation, new Vector2(num173, (float)(projectile.height / 2 + num171)), projectile.scale, spriteEffects, 0f);
					}
				}
			}
			if (projectile.type == 525 && (!Main.gamePaused || Main.gameMenu))
			{
				Vector2 vector94 = projectile.position - Main.screenPosition;
				if ((float)Main.mouseX > vector94.X && (float)Main.mouseX < vector94.X + (float)projectile.width && (float)Main.mouseY > vector94.Y && (float)Main.mouseY < vector94.Y + (float)projectile.height)
				{
					int num352 = (int)(Main.player[Main.myPlayer].Center.X / 16f);
					int num353 = (int)(Main.player[Main.myPlayer].Center.Y / 16f);
					int num354 = (int)projectile.Center.X / 16;
					int num355 = (int)projectile.Center.Y / 16;
					int lastTileRangeX = Main.player[Main.myPlayer].lastTileRangeX;
					int lastTileRangeY = Main.player[Main.myPlayer].lastTileRangeY;
					if (num352 >= num354 - lastTileRangeX && num352 <= num354 + lastTileRangeX + 1 && num353 >= num355 - lastTileRangeY && num353 <= num355 + lastTileRangeY + 1)
					{
						Main.player[Main.myPlayer].noThrow = 2;
						Main.player[Main.myPlayer].showItemIcon = true;
						Main.player[Main.myPlayer].showItemIcon2 = 3213;
						if (Terraria.GameInput.PlayerInput.UsingGamepad)
						{
							Main.player[Main.myPlayer].GamepadEnableGrappleCooldown();
						}
						if (Main.mouseRight && Main.mouseRightRelease && Player.StopMoneyTroughFromWorking == 0)
						{
							Main.mouseRightRelease = false;
							if (Main.player[Main.myPlayer].chest == -2)
							{
								Main.PlaySound(2, -1, -1, 59);
								Main.player[Main.myPlayer].chest = -1;
								Recipe.FindRecipes();
								return;
							}
							Main.player[Main.myPlayer].flyingPigChest = i;
							Main.player[Main.myPlayer].chest = -2;
							Main.player[Main.myPlayer].chestX = (int)(projectile.Center.X / 16f);
							Main.player[Main.myPlayer].chestY = (int)(projectile.Center.Y / 16f);
							Main.player[Main.myPlayer].talkNPC = -1;
							Main.npcShop = 0;
							Main.playerInventory = true;
							Main.PlaySound(2, -1, -1, 59);
							Recipe.FindRecipes();
						}
					}
				}
			}
		}
	}
}