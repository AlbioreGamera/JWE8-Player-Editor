namespace PlayerLibrary
{
    public enum PlayerPosition : uint
    {
        GK = 0,
        UNK = 1,
        CBW = 2,
        CBT = 3,
        SB = 4,
        DMF = 5,
        CMF = 6,
        SMF = 7,
        OMF = 8,
        WF = 9,
        CF = 10,
    }

    public enum FreeKickType : uint
    {
        FK_A, FK_B, FK_C, FK_D, FK_E, FK_F, FK_G, FK_H,
    }

    public enum PenaltyKickType : uint
    {
        PK_A, PK_B, PK_C,
    }

    public enum WristbandType : uint
    {
        WRISTBAND_NONE,
        WRISTBAND_LEFT,
        WRISTBAND_RIGHT,
        WRISTBAND_LEFT_AND_RIGHT,
    }

    public enum Foot : uint
    {
        RIGHT_FOOT,
        LEFT_FOOT,
    }

    public enum InjuryResistance : uint
    {
        INJURY_C = 0,
        INJURY_B = 1,
        INJURY_A = 2,
    }

    public enum FaceType : uint
    {
        BUILD = 0,
        ORIGINAL = 1,
        PRESET = 2,
    }

    public enum EyesColor2 : uint
    {
        BLACK_1, BLACK_2, DARK_GREY_1, DARK_GREY_2, BROWN_1, BROWN_2,
        LIGHT_BLUE_1, LIGHT_BLUE_2, BLUE_1, BLUE_2, GREEN_1, GREEN_2,
    }

    public enum BootsType : uint
    {
        BOOT_EDITABLE_1,
        BOOT_EDITABLE_2,
        BOOT_ADIDAS_F50_BLACK_WHITE,
        BOOT_ADIDAS_F50_RED_BLACK,
        BOOT_ADIDAS_F50_BLACK_GREY,
        BOOT_ADIDAS_F50_WHITE_RED,
        BOOT_ADIDAS_PREDATOR_BLACK_YELLOW,
        BOOT_ADIDAS_PREDATOR_BLACK_WHITE,
        BOOT_ADIDAS_PREDATOR_WHITE_BLUE,
    }

    public enum BootsColor : uint
    {
        BOOT_COLOR_BLACK,
        BOOT_COLOR_RED,
        BOOT_COLOR_BLUE,
        BOOT_COLOR_GREEN,
        BOOT_COLOR_BROWN,
        BOOT_COLOR_YELLOW,
        BOOT_COLOR_WHITE,
        BOOT_COLOR_GREY,
    }

    public enum SleevesLength : uint
    {
        SLEEVES_SEASONAL,
        SLEEVES_ALWAYS_SHORT,
        SLEEVES_ALWAYS_LONG,
    }

    public enum YesNo : uint
    {
        NO,
        YES,
    }

    public enum GlovesOption : uint
    {
        GLOVES_NONE,
        GLOVES_AUTO,
    }

    public enum SocksHeight : uint
    {
        SOCKS_HIGH,
        SOCKS_MEDIUM,
        SOCKS_LOW,
    }

    public enum SkinColour : uint
    {
        SKIN_COLOUR_1,
        SKIN_COLOUR_2,
        SKIN_COLOUR_3,
        SKIN_COLOUR_4,
    }

    public enum CapColour : uint
    {
        CAP_COLOR_WHITE,
        CAP_COLOR_RED,
        CAP_COLOR_NAVY,
        CAP_COLOR_GREY,
        CAP_COLOR_BLACK,
        CAP_COLOR_LIME,
        CAP_COLOR_YELLOW,
        CAP_COLOR_FULL_RED,
    }

    public enum BandanaColour : uint
    {
        BANDANA_COLOUR_WHITE,
        BANDANA_COLOUR_BLACK,
        BANDANA_COLOUR_RED,
        BANDANA_COLOUR_NAVY,
        BANDANA_COLOUR_PURPLE,
        BANDANA_COLOUR_TURQUOISE,
        BANDANA_COLOUR_YELLOW,
        BANDANA_COLOUR_GREEN,
    }
}