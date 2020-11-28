using UnityEngine;


/// <summary>
/// Options available in the program ui
/// </summary>
public enum LANGUAGE_TYPE
{
    KOREAN,
    ENGLISH,

    CHINESE_SIMPLIFIED,
    CHINESE_TRADITIONAL,
    GERMAN,
    SPAIN,
    FRANCE,
    INDONESIA,
    ARABIC,
    ITALY,
    JAPAN,
    MALAYSIA,
    PORTUGAL_AND_BRAZIL,
    RUSSIA,
    THAILAND,
    TURKEY,
    TAIWAN,
    VIETNAM
}

/// <summary>
/// UI language table types
/// </summary>
public enum UI_TEXT_TYPE
{
    OBJECT,
    SYSTEM,
    UI
}

public static class LogColorTag // In the current mono version, the enum default argument is unintelligible because of a compilation error.
{
    public const int None = -1;

    // To view color values : http://docs.unity3d.com/Manual/StyledText.html
    public const int Black = 0;
    public const int Blue = 1;
    public const int Brown = 2;
    public const int Cyan = 3;
    public const int DarkBlue = 4;
    public const int Green = 5;
    public const int Grey = 6;
    public const int LightBlue = 7;
    public const int Lime = 8; // #00ff00
    public const int Magenta = 9;
    public const int Maroon = 10;
    public const int Navy = 11;
    public const int Olive = 12;
    public const int Orange = 13;
    public const int Purple = 14;
    public const int Red = 15;
    public const int Silver = 16;
    public const int Teal = 17;
    public const int White = 18;
    public const int Yellow = 19;
}

public static class LogColorTagText
{
    public static string[] values = { "black", "blue", "brown", "cyan", "darkblue", "green", "grey", "lightblue", "lime", "magenta", "maroon", "navy", "olive", "orange", "purple", "red", "silver", "teal", "white", "yellow" };
}

public enum LazyUpdateType
{
    Every60s,
    Every10s,
    Every5s,
    Every1s,
    Every25ms,
    Every50ms,
    Every100ms,
    Every500ms,
}

public class Layers
{
    public static readonly int Default;
    public static readonly int UI;
    public static readonly int Ground;
    public static readonly int Obstacles;
    public static readonly int Buildings;
    public static readonly int Characters;
    public static readonly int TransparentFX;
    public static readonly int Beacons;
    public static readonly int NoMove;
    public static readonly int SkillActionFocus;  // Layer to render using the Skill feature
    public static readonly int SkillActionDimmer; // A shade layer for darkening the background of the SkillActionFocus layer.
    public static readonly int Doll;
    public static readonly int HeroView;

    static Layers()
    {
        Default = LayerMask.NameToLayer(@"Default");
        UI = LayerMask.NameToLayer(@"UI");
        Ground = LayerMask.NameToLayer(@"Ground");
        Obstacles = LayerMask.NameToLayer(@"Obstacles");
        Buildings = LayerMask.NameToLayer(@"Buildings");
        Characters = LayerMask.NameToLayer(@"Characters");
        TransparentFX = LayerMask.NameToLayer(@"TransparentFX");
        Beacons = LayerMask.NameToLayer(@"Beacons");
        NoMove = LayerMask.NameToLayer(@"NoMove");
        SkillActionFocus = LayerMask.NameToLayer(@"SkillActionFocus");
        SkillActionDimmer = LayerMask.NameToLayer(@"SkillActionDimmer");
        Doll = LayerMask.NameToLayer(@"Doll");
        HeroView = LayerMask.NameToLayer(@"HeroView");
    }
}

public class LayerMasks
{
    public static readonly int Default;
    public static readonly int UI;
    public static readonly int UI3D;
    public static readonly int Ground;
    public static readonly int Obstacles;
    public static readonly int Buildings;
    public static readonly int Characters;
    public static readonly int TransparentFX;
    public static readonly int Beacons;
    public static readonly int NoMove;
    public static readonly int SkillActionFocus;
    public static readonly int SkillActionDimmer;

    public static readonly int MainCameraEventMask;

    static LayerMasks()
    {
        Default = 1 << Layers.Default;
        UI = 1 << Layers.UI;
        Ground = 1 << Layers.Ground;
        Obstacles = 1 << Layers.Obstacles;
        Buildings = 1 << Layers.Buildings;
        Characters = 1 << Layers.Characters;
        TransparentFX = 1 << Layers.TransparentFX;
        Beacons = 1 << Layers.Beacons;
        NoMove = 1 << Layers.NoMove;
        SkillActionFocus = 1 << Layers.SkillActionFocus;
        SkillActionDimmer = 1 << Layers.SkillActionDimmer;

        MainCameraEventMask = Ground | Characters | Obstacles | NoMove/* | ActionCut*/;
    }
}

public class Tags
{
    public const string Untagged = @"Untagged";
    public const string Beacon = @"Beacon";
    public const string MainCamera = @"MainCamera";
}

public enum SOUND_TYPE
{
    BGM,
    SFX,
    UI,
    VOICE,
    AMBIENT
}