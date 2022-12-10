using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameOverlay;
using UniTASPlugin.ReverseInvoker;
using ScreenOrig = UnityEngine.Screen;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable CommentTypo

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal static class Screen
{
    [HarmonyPatch(typeof(ScreenOrig), "showCursor", MethodType.Setter)]
    internal class set_showCursor
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool value)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            Overlay.UnityCursorVisible = value;
            if (Overlay.ShowCursor)
                value = false;
            return true;
        }
    }
}

/*
[HarmonyPatch(typeof(ScreenOrig), nameof(ScreenOrig.width), MethodType.Getter)]
class widthGetter
{
    static System.Exception Cleanup(System.Reflection.MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result)
    {
   if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
   return true;
        __result = TAS.Screen.Width;
        return false;
    }
}

[HarmonyPatch(typeof(ScreenOrig), nameof(ScreenOrig.height), MethodType.Getter)]
class heightGetter
{
    static System.Exception Cleanup(System.Reflection.MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result)
    {
   if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
   return true;
        __result = TAS.Screen.Height;
        return false;
    }
}*/

/*
// Token: 0x170001D3 RID: 467
// (get) Token: 0x06000820 RID: 2080
public static extern float dpi { [NativeName("GetDPI")][MethodImpl(MethodImplOptions.InternalCall)] get; }

// Token: 0x06000821 RID: 2081
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern void RequestOrientation(ScreenOrientation orient);

// Token: 0x06000822 RID: 2082
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern ScreenOrientation GetScreenOrientation();

// Token: 0x170001D4 RID: 468
// (get) Token: 0x06000823 RID: 2083 RVA: 0x0000C468 File Offset: 0x0000A668
// (set) Token: 0x06000824 RID: 2084 RVA: 0x0000C480 File Offset: 0x0000A680
public static ScreenOrientation orientation
{
	get
	{
		return ScreenOrig.GetScreenOrientation();
	}
	set
	{
		bool flag = value == ScreenOrientation.Unknown;
		if (flag)
		{
			Debug.Log("ScreenOrientation.Unknown is deprecated. Please use ScreenOrientation.AutoRotation");
			value = ScreenOrientation.AutoRotation;
		}
		ScreenOrig.RequestOrientation(value);
	}
}

// Token: 0x170001D5 RID: 469
// (get) Token: 0x06000825 RID: 2085
// (set) Token: 0x06000826 RID: 2086
[NativeProperty("ScreenTimeout")]
public static extern int sleepTimeout { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

// Token: 0x06000827 RID: 2087
[NativeName("GetIsOrientationEnabled")]
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern bool IsOrientationEnabled(EnabledOrientation orient);

// Token: 0x06000828 RID: 2088
[NativeName("SetIsOrientationEnabled")]
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern void SetOrientationEnabled(EnabledOrientation orient, bool enabled);

// Token: 0x170001D6 RID: 470
// (get) Token: 0x06000829 RID: 2089 RVA: 0x0000C4B0 File Offset: 0x0000A6B0
// (set) Token: 0x0600082A RID: 2090 RVA: 0x0000C4C8 File Offset: 0x0000A6C8
public static bool autorotateToPortrait
{
	get
	{
		return ScreenOrig.IsOrientationEnabled(EnabledOrientation.kAutorotateToPortrait);
	}
	set
	{
		ScreenOrig.SetOrientationEnabled(EnabledOrientation.kAutorotateToPortrait, value);
	}
}

// Token: 0x170001D7 RID: 471
// (get) Token: 0x0600082B RID: 2091 RVA: 0x0000C4D4 File Offset: 0x0000A6D4
// (set) Token: 0x0600082C RID: 2092 RVA: 0x0000C4EC File Offset: 0x0000A6EC
public static bool autorotateToPortraitUpsideDown
{
	get
	{
		return ScreenOrig.IsOrientationEnabled(EnabledOrientation.kAutorotateToPortraitUpsideDown);
	}
	set
	{
		ScreenOrig.SetOrientationEnabled(EnabledOrientation.kAutorotateToPortraitUpsideDown, value);
	}
}

// Token: 0x170001D8 RID: 472
// (get) Token: 0x0600082D RID: 2093 RVA: 0x0000C4F8 File Offset: 0x0000A6F8
// (set) Token: 0x0600082E RID: 2094 RVA: 0x0000C510 File Offset: 0x0000A710
public static bool autorotateToLandscapeLeft
{
	get
	{
		return ScreenOrig.IsOrientationEnabled(EnabledOrientation.kAutorotateToLandscapeLeft);
	}
	set
	{
		ScreenOrig.SetOrientationEnabled(EnabledOrientation.kAutorotateToLandscapeLeft, value);
	}
}

// Token: 0x170001D9 RID: 473
// (get) Token: 0x0600082F RID: 2095 RVA: 0x0000C51C File Offset: 0x0000A71C
// (set) Token: 0x06000830 RID: 2096 RVA: 0x0000C534 File Offset: 0x0000A734
public static bool autorotateToLandscapeRight
{
	get
	{
		return ScreenOrig.IsOrientationEnabled(EnabledOrientation.kAutorotateToLandscapeRight);
	}
	set
	{
		ScreenOrig.SetOrientationEnabled(EnabledOrientation.kAutorotateToLandscapeRight, value);
	}
}

// Token: 0x170001DA RID: 474
// (get) Token: 0x06000831 RID: 2097 RVA: 0x0000C540 File Offset: 0x0000A740
public static Resolution currentResolution
{
	get
	{
		Resolution result;
		ScreenOrig.get_currentResolution_Injected(out result);
		return result;
	}
}

// Token: 0x170001DB RID: 475
// (get) Token: 0x06000832 RID: 2098
// (set) Token: 0x06000833 RID: 2099
public static extern bool fullScreen { [NativeName("IsFullscreen")][MethodImpl(MethodImplOptions.InternalCall)] get; [NativeName("RequestSetFullscreenFromScript")][MethodImpl(MethodImplOptions.InternalCall)] set; }

// Token: 0x170001DC RID: 476
// (get) Token: 0x06000834 RID: 2100
// (set) Token: 0x06000835 RID: 2101
public static extern FullScreenMode fullScreenMode { [NativeName("GetFullscreenMode")][MethodImpl(MethodImplOptions.InternalCall)] get; [NativeName("RequestSetFullscreenModeFromScript")][MethodImpl(MethodImplOptions.InternalCall)] set; }

// Token: 0x170001DD RID: 477
// (get) Token: 0x06000836 RID: 2102 RVA: 0x0000C558 File Offset: 0x0000A758
public static Rect safeArea
{
	get
	{
		Rect result;
		ScreenOrig.get_safeArea_Injected(out result);
		return result;
	}
}

// Token: 0x170001DE RID: 478
// (get) Token: 0x06000837 RID: 2103
public static extern Rect[] cutouts { [FreeFunction("ScreenScripting::GetCutouts")][MethodImpl(MethodImplOptions.InternalCall)] get; }

// Token: 0x06000838 RID: 2104
[NativeName("RequestResolution")]
[MethodImpl(MethodImplOptions.InternalCall)]
public static extern void SetResolution(int width, int height, FullScreenMode fullscreenMode, [UnityEngine.Internal.DefaultValue("0")] int preferredRefreshRate);

// Token: 0x06000839 RID: 2105 RVA: 0x0000C56D File Offset: 0x0000A76D
public static void SetResolution(int width, int height, FullScreenMode fullscreenMode)
{
	ScreenOrig.SetResolution(width, height, fullscreenMode, 0);
}

// Token: 0x0600083A RID: 2106 RVA: 0x0000C57A File Offset: 0x0000A77A
public static void SetResolution(int width, int height, bool fullscreen, [UnityEngine.Internal.DefaultValue("0")] int preferredRefreshRate)
{
	ScreenOrig.SetResolution(width, height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, preferredRefreshRate);
}

// Token: 0x0600083B RID: 2107 RVA: 0x0000C58D File Offset: 0x0000A78D
public static void SetResolution(int width, int height, bool fullscreen)
{
	ScreenOrig.SetResolution(width, height, fullscreen, 0);
}

// Token: 0x170001DF RID: 479
// (get) Token: 0x0600083C RID: 2108 RVA: 0x0000C59C File Offset: 0x0000A79C
public static Vector2Int mainWindowPosition
{
	get
	{
		return ScreenOrig.GetMainWindowPosition();
	}
}

// Token: 0x170001E0 RID: 480
// (get) Token: 0x0600083D RID: 2109 RVA: 0x0000C5B4 File Offset: 0x0000A7B4
public static DisplayInfo mainWindowDisplayInfo
{
	get
	{
		return ScreenOrig.GetMainWindowDisplayInfo();
	}
}

// Token: 0x0600083E RID: 2110 RVA: 0x0000C5CC File Offset: 0x0000A7CC
public static void GetDisplayLayout(List<DisplayInfo> displayLayout)
{
	bool flag = displayLayout == null;
	if (flag)
	{
		throw new ArgumentNullException();
	}
	ScreenOrig.GetDisplayLayoutImpl(displayLayout);
}

// Token: 0x0600083F RID: 2111 RVA: 0x0000C5F0 File Offset: 0x0000A7F0
public static AsyncOperation MoveMainWindowTo(in DisplayInfo display, Vector2Int position)
{
	return ScreenOrig.MoveMainWindowImpl(display, position);
}

// Token: 0x06000840 RID: 2112 RVA: 0x0000C60C File Offset: 0x0000A80C
[FreeFunction("GetMainWindowPosition")]
private static Vector2Int GetMainWindowPosition()
{
	Vector2Int result;
	ScreenOrig.GetMainWindowPosition_Injected(out result);
	return result;
}

// Token: 0x06000841 RID: 2113 RVA: 0x0000C624 File Offset: 0x0000A824
[FreeFunction("GetMainWindowDisplayInfo")]
private static DisplayInfo GetMainWindowDisplayInfo()
{
	DisplayInfo result;
	ScreenOrig.GetMainWindowDisplayInfo_Injected(out result);
	return result;
}

// Token: 0x06000842 RID: 2114
[FreeFunction("GetDisplayLayout")]
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern void GetDisplayLayoutImpl(List<DisplayInfo> displayLayout);

// Token: 0x06000843 RID: 2115 RVA: 0x0000C639 File Offset: 0x0000A839
[FreeFunction("MoveMainWindow")]
private static AsyncOperation MoveMainWindowImpl(in DisplayInfo display, Vector2Int position)
{
	return ScreenOrig.MoveMainWindowImpl_Injected(display, ref position);
}

// Token: 0x170001E1 RID: 481
// (get) Token: 0x06000844 RID: 2116
public static extern Resolution[] resolutions { [FreeFunction("ScreenScripting::GetResolutions")][MethodImpl(MethodImplOptions.InternalCall)] get; }

// Token: 0x170001E2 RID: 482
// (get) Token: 0x06000845 RID: 2117
// (set) Token: 0x06000846 RID: 2118
public static extern float brightness { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

// Token: 0x170001E3 RID: 483
// (get) Token: 0x06000847 RID: 2119 RVA: 0x0000C644 File Offset: 0x0000A844
// (set) Token: 0x06000848 RID: 2120 RVA: 0x0000C660 File Offset: 0x0000A860
[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use Cursor.lockState and Cursor.visible instead.", false)]
public static bool lockCursor
{
	get
	{
		return CursorLockMode.Locked == Cursor.lockState;
	}
	set
	{
		if (value)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}

// Token: 0x0600084A RID: 2122
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern void get_currentResolution_Injected(out Resolution ret);

// Token: 0x0600084B RID: 2123
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern void get_safeArea_Injected(out Rect ret);

// Token: 0x0600084C RID: 2124
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern void GetMainWindowPosition_Injected(out Vector2Int ret);

// Token: 0x0600084D RID: 2125
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern void GetMainWindowDisplayInfo_Injected(out DisplayInfo ret);

// Token: 0x0600084E RID: 2126
[MethodImpl(MethodImplOptions.InternalCall)]
private static extern AsyncOperation MoveMainWindowImpl_Injected(in DisplayInfo display, ref Vector2Int position);

 
 
 
 
 
 ==== MODERN
 
 
 
 
 
 

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x060008FE RID: 2302
		public static extern float dpi { [NativeName("GetDPI")] [MethodImpl(MethodImplOptions.InternalCall)] get; }

		// Token: 0x060008FF RID: 2303
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void RequestOrientation(ScreenOrientation orient);

		// Token: 0x06000900 RID: 2304
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern ScreenOrientation GetScreenOrientation();

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x06000901 RID: 2305 RVA: 0x0000E7C0 File Offset: 0x0000C9C0
		// (set) Token: 0x06000902 RID: 2306 RVA: 0x0000E7D8 File Offset: 0x0000C9D8
		public static ScreenOrientation orientation
		{
			get
			{
				return ScreenOrig.GetScreenOrientation();
			}
			set
			{
				bool flag = value == ScreenOrientation.Unknown;
				if (flag)
				{
					Debug.Log("ScreenOrientation.Unknown is deprecated. Please use ScreenOrientation.AutoRotation");
					value = ScreenOrientation.AutoRotation;
				}
				ScreenOrig.RequestOrientation(value);
			}
		}

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x06000903 RID: 2307
		// (set) Token: 0x06000904 RID: 2308
		[NativeProperty("ScreenTimeout")]
		public static extern int sleepTimeout { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

		// Token: 0x06000905 RID: 2309
		[NativeName("GetIsOrientationEnabled")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool IsOrientationEnabled(EnabledOrientation orient);

		// Token: 0x06000906 RID: 2310
		[NativeName("SetIsOrientationEnabled")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetOrientationEnabled(EnabledOrientation orient, bool enabled);

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x06000907 RID: 2311 RVA: 0x0000E808 File Offset: 0x0000CA08
		// (set) Token: 0x06000908 RID: 2312 RVA: 0x0000E820 File Offset: 0x0000CA20
		public static bool autorotateToPortrait
		{
			get
			{
				return ScreenOrig.IsOrientationEnabled(EnabledOrientation.kAutorotateToPortrait);
			}
			set
			{
				ScreenOrig.SetOrientationEnabled(EnabledOrientation.kAutorotateToPortrait, value);
			}
		}

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x06000909 RID: 2313 RVA: 0x0000E82C File Offset: 0x0000CA2C
		// (set) Token: 0x0600090A RID: 2314 RVA: 0x0000E844 File Offset: 0x0000CA44
		public static bool autorotateToPortraitUpsideDown
		{
			get
			{
				return ScreenOrig.IsOrientationEnabled(EnabledOrientation.kAutorotateToPortraitUpsideDown);
			}
			set
			{
				ScreenOrig.SetOrientationEnabled(EnabledOrientation.kAutorotateToPortraitUpsideDown, value);
			}
		}

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x0600090B RID: 2315 RVA: 0x0000E850 File Offset: 0x0000CA50
		// (set) Token: 0x0600090C RID: 2316 RVA: 0x0000E868 File Offset: 0x0000CA68
		public static bool autorotateToLandscapeLeft
		{
			get
			{
				return ScreenOrig.IsOrientationEnabled(EnabledOrientation.kAutorotateToLandscapeLeft);
			}
			set
			{
				ScreenOrig.SetOrientationEnabled(EnabledOrientation.kAutorotateToLandscapeLeft, value);
			}
		}

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x0600090D RID: 2317 RVA: 0x0000E874 File Offset: 0x0000CA74
		// (set) Token: 0x0600090E RID: 2318 RVA: 0x0000E88C File Offset: 0x0000CA8C
		public static bool autorotateToLandscapeRight
		{
			get
			{
				return ScreenOrig.IsOrientationEnabled(EnabledOrientation.kAutorotateToLandscapeRight);
			}
			set
			{
				ScreenOrig.SetOrientationEnabled(EnabledOrientation.kAutorotateToLandscapeRight, value);
			}
		}

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x0600090F RID: 2319 RVA: 0x0000E898 File Offset: 0x0000CA98
		public static Resolution currentResolution
		{
			get
			{
				Resolution result;
				ScreenOrig.get_currentResolution_Injected(out result);
				return result;
			}
		}

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x06000910 RID: 2320
		// (set) Token: 0x06000911 RID: 2321
		public static extern bool fullScreen { [NativeName("IsFullscreen")] [MethodImpl(MethodImplOptions.InternalCall)] get; [NativeName("RequestSetFullscreenFromScript")] [MethodImpl(MethodImplOptions.InternalCall)] set; }

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x06000912 RID: 2322
		// (set) Token: 0x06000913 RID: 2323
		public static extern FullScreenMode fullScreenMode { [NativeName("GetFullscreenMode")] [MethodImpl(MethodImplOptions.InternalCall)] get; [NativeName("RequestSetFullscreenModeFromScript")] [MethodImpl(MethodImplOptions.InternalCall)] set; }

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x06000914 RID: 2324 RVA: 0x0000E8B0 File Offset: 0x0000CAB0
		public static Rect safeArea
		{
			get
			{
				Rect result;
				ScreenOrig.get_safeArea_Injected(out result);
				return result;
			}
		}

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x06000915 RID: 2325
		public static extern Rect[] cutouts { [FreeFunction("ScreenScripting::GetCutouts")] [MethodImpl(MethodImplOptions.InternalCall)] get; }

		// Token: 0x06000916 RID: 2326 RVA: 0x0000E8C5 File Offset: 0x0000CAC5
		[NativeName("RequestResolution")]
		public static void SetResolution(int width, int height, FullScreenMode fullscreenMode, RefreshRate preferredRefreshRate)
		{
			ScreenOrig.SetResolution_Injected(width, height, fullscreenMode, ref preferredRefreshRate);
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x0000E8D4 File Offset: 0x0000CAD4
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("SetResolution(int, int, FullScreenMode, int) is obsolete. Use SetResolution(int, int, FullScreenMode, RefreshRate) instead.")]
		public static void SetResolution(int width, int height, FullScreenMode fullscreenMode, [DefaultValue("0")] int preferredRefreshRate)
		{
			bool flag = preferredRefreshRate < 0;
			if (flag)
			{
				preferredRefreshRate = 0;
			}
			ScreenOrig.SetResolution(width, height, fullscreenMode, new RefreshRate
			{
				numerator = (uint)preferredRefreshRate,
				denominator = 1U
			});
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x0000E910 File Offset: 0x0000CB10
		public static void SetResolution(int width, int height, FullScreenMode fullscreenMode)
		{
			ScreenOrig.SetResolution(width, height, fullscreenMode, new RefreshRate
			{
				numerator = 0U,
				denominator = 1U
			});
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x0000E940 File Offset: 0x0000CB40
		[Obsolete("SetResolution(int, int, bool, int) is obsolete. Use SetResolution(int, int, FullScreenMode, RefreshRate) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetResolution(int width, int height, bool fullscreen, [DefaultValue("0")] int preferredRefreshRate)
		{
			bool flag = preferredRefreshRate < 0;
			if (flag)
			{
				preferredRefreshRate = 0;
			}
			ScreenOrig.SetResolution(width, height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, new RefreshRate
			{
				numerator = (uint)preferredRefreshRate,
				denominator = 1U
			});
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x0000E981 File Offset: 0x0000CB81
		public static void SetResolution(int width, int height, bool fullscreen)
		{
			ScreenOrig.SetResolution(width, height, fullscreen, 0);
		}

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x0600091B RID: 2331 RVA: 0x0000E990 File Offset: 0x0000CB90
		public static Vector2Int mainWindowPosition
		{
			get
			{
				return ScreenOrig.GetMainWindowPosition();
			}
		}

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x0600091C RID: 2332 RVA: 0x0000E9A8 File Offset: 0x0000CBA8
		public static DisplayInfo mainWindowDisplayInfo
		{
			get
			{
				return ScreenOrig.GetMainWindowDisplayInfo();
			}
		}

		// Token: 0x0600091D RID: 2333 RVA: 0x0000E9C0 File Offset: 0x0000CBC0
		public static void GetDisplayLayout(List<DisplayInfo> displayLayout)
		{
			bool flag = displayLayout == null;
			if (flag)
			{
				throw new ArgumentNullException();
			}
			ScreenOrig.GetDisplayLayoutImpl(displayLayout);
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x0000E9E4 File Offset: 0x0000CBE4
		public static AsyncOperation MoveMainWindowTo(in DisplayInfo display, Vector2Int position)
		{
			return ScreenOrig.MoveMainWindowImpl(display, position);
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x0000EA00 File Offset: 0x0000CC00
		[FreeFunction("GetMainWindowPosition")]
		private static Vector2Int GetMainWindowPosition()
		{
			Vector2Int result;
			ScreenOrig.GetMainWindowPosition_Injected(out result);
			return result;
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x0000EA18 File Offset: 0x0000CC18
		[FreeFunction("GetMainWindowDisplayInfo")]
		private static DisplayInfo GetMainWindowDisplayInfo()
		{
			DisplayInfo result;
			ScreenOrig.GetMainWindowDisplayInfo_Injected(out result);
			return result;
		}

		// Token: 0x06000921 RID: 2337
		[FreeFunction("GetDisplayLayout")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetDisplayLayoutImpl(List<DisplayInfo> displayLayout);

		// Token: 0x06000922 RID: 2338 RVA: 0x0000EA2D File Offset: 0x0000CC2D
		[FreeFunction("MoveMainWindow")]
		private static AsyncOperation MoveMainWindowImpl(in DisplayInfo display, Vector2Int position)
		{
			return ScreenOrig.MoveMainWindowImpl_Injected(display, ref position);
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x06000923 RID: 2339
		public static extern Resolution[] resolutions { [FreeFunction("ScreenScripting::GetResolutions")] [MethodImpl(MethodImplOptions.InternalCall)] get; }

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06000924 RID: 2340
		// (set) Token: 0x06000925 RID: 2341
		public static extern float brightness { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000926 RID: 2342 RVA: 0x0000EA38 File Offset: 0x0000CC38
		// (set) Token: 0x06000927 RID: 2343 RVA: 0x0000EA54 File Offset: 0x0000CC54
		[Obsolete("Use Cursor.lockState and Cursor.visible instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool lockCursor
		{
			get
			{
				return CursorLockMode.Locked == Cursor.lockState;
			}
			set
			{
				if (value)
				{
					Cursor.visible = false;
					Cursor.lockState = CursorLockMode.Locked;
				}
				else
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
			}
		}

		// Token: 0x06000929 RID: 2345
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void get_currentResolution_Injected(out Resolution ret);

		// Token: 0x0600092A RID: 2346
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void get_safeArea_Injected(out Rect ret);

		// Token: 0x0600092B RID: 2347
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetResolution_Injected(int width, int height, FullScreenMode fullscreenMode, [In] ref RefreshRate preferredRefreshRate);

		// Token: 0x0600092C RID: 2348
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetMainWindowPosition_Injected(out Vector2Int ret);

		// Token: 0x0600092D RID: 2349
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetMainWindowDisplayInfo_Injected(out DisplayInfo ret);

		// Token: 0x0600092E RID: 2350
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern AsyncOperation MoveMainWindowImpl_Injected(in DisplayInfo display, [In] ref Vector2Int position);
 */