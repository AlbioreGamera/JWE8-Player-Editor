using System;

namespace PlayerLibrary
{
    public class Player
    {
        // Identificación básica
        public int PlayerIndex { get; set; }
        public string? Name { get; set; }
        public string? ShirtName { get; set; }
        public int Country { get; set; }
        public int Commentary { get; set; }
        public int Position { get; set; }
        public int Age { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }

        // --- 0x34: Posición registrada + habilidades base ---
        public PlayerPosition RegPosition { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Balance { get; set; }
        public int Stamina { get; set; }

        // --- 0x38: Velocidad / posiciones secundarias ---
        public int TopSpeed { get; set; }
        public int Acceleration { get; set; }
        public int Response { get; set; }
        public int DribbleAccuracy { get; set; }
        public bool CanPlayGK { get; set; }
        public bool CanPlayCBW { get; set; }
        public bool CanPlayCBT { get; set; }

        // --- 0x3c ---
        public int DribbleSpeed { get; set; }
        public int ShortPassAccuracy { get; set; }
        public int ShortPassSpeed { get; set; }
        public int LongPassAccuracy { get; set; }
        public bool CanPlaySB { get; set; }
        public bool CanPlayDMF { get; set; }
        public bool CanPlayCMF { get; set; }
        public bool CanPlaySMF { get; set; }

        // --- 0x40 ---
        public int LongPassSpeed { get; set; }
        public int ShotPower { get; set; }
        public int ShotAccuracy { get; set; }
        public int ShotTechnique { get; set; }
        public bool CanPlayAMF { get; set; }
        public bool CanPlayCF { get; set; }
        public bool CanPlayWF { get; set; }
        public bool Dribbling { get; set; }

        // --- 0x44 ---
        public int Jump { get; set; }
        public int Header { get; set; }
        public int Technique { get; set; }
        public int Curling { get; set; }
        public bool PostPlayer { get; set; }
        public bool Positioning { get; set; }
        public bool Reaction { get; set; }
        public bool Scoring { get; set; }

        // --- 0x48 ---
        public int Aggression { get; set; }
        public int Mentality { get; set; }
        public int Condition { get; set; }
        public int Consistency { get; set; }
        public int FreeKickAccuracy { get; set; }
        public bool Playmaking { get; set; }
        public bool Passing { get; set; }
        public bool Penalties { get; set; }
        public bool OneOnOneScoring { get; set; }
        public bool LongThrow { get; set; }

        // --- 0x4c ---
        public int TeamWorkAbility { get; set; }
        public int Agility { get; set; }
        public int GoalKeepingSkills { get; set; }
        public Foot DominantFoot { get; set; }
        public int WeakFootAccuracy { get; set; }
        public int WeakFootFrequency { get; set; }
        public FreeKickType FreeKickType { get; set; }

        // --- 0x50 ---
        public bool OneTouchPass { get; set; }
        public bool Marking { get; set; }
        public bool DLineControl { get; set; }
        public bool DirectGKKick { get; set; }
        public bool OneOnOneStopper { get; set; }
        public bool PenaltyStopper { get; set; }
        public bool BothFeetFeint { get; set; }
        public bool Outside { get; set; }
        public InjuryResistance Injury { get; set; }
        public bool SlidingTackle { get; set; }

        // --- 0x54 ---
        public PenaltyKickType PenaltyKickType { get; set; }
        public int GoalCelebration1 { get; set; }
        public int GoalCelebration2 { get; set; }
        public bool TacticalDribble { get; set; }
        public bool LinePositioning { get; set; }
        public bool Side { get; set; }
        public bool Center { get; set; }

        // --- 0x58: Apariencia física ---
        public int HeadHeight { get; set; }
        public int HeadWidth { get; set; }
        public SkinColour SkinColour { get; set; }
        public BootsColor BootsTongueColor { get; set; }
        public BootsColor BootsLineColor2 { get; set; }

        // --- 0x5c ---
        public int HairId { get; set; }
        public int HairColourPattern { get; set; }
        public int EyesColor1 { get; set; }
        public EyesColor2 EyesColor2 { get; set; }
        public int FacialHairType { get; set; }
        public SleevesLength Sleeves { get; set; }

        // --- 0x60 ---
        public int FacialHairColour { get; set; }
        public int SunGlassesType { get; set; }
        public int SunGlassesColour { get; set; }
        public int MaskType { get; set; }
        public int MaskColour { get; set; }
        public YesNo CapType { get; set; }
        public CapColour CapColour { get; set; }
        public WristbandType WristbandType { get; set; }
        public BootsType BootsType { get; set; }
        public BootsColor BootsBaseColor { get; set; }
        public BootsColor BootsLineColor1 { get; set; }

        // --- 0x64 ---
        public int FaceId { get; set; }
        public BootsColor BootsStudsColour { get; set; }
        public YesNo Tape { get; set; }
        public int HairColorR { get; set; }
        public int HairColorG { get; set; }
        public int HairColorB { get; set; }
        public GlovesOption Gloves { get; set; }

        // --- 0x68 ---
        public SocksHeight Socks { get; set; }
        public int NeckLength { get; set; }
        public int ChestMeasurement { get; set; }
        public int ArmCircumference { get; set; }
        public int WaisteCircumference { get; set; }
        public int LegCircumference { get; set; }
        public int CalfCircumference { get; set; }
        public FaceType FaceType { get; set; }
        public int LegLength { get; set; }

        // --- 0x6c ---
        public BandanaColour BandanaColour { get; set; }
        public int ShoulderHeight { get; set; }
        public int ShoulderWidth { get; set; }
        public int Nationality { get; set; }

        // --- 0x74: Rasgos faciales ---
        public int BrowsHeight { get; set; }
        public int EyesPosition { get; set; }
        public int EyesAngle { get; set; }
        public int EyesSize { get; set; }
        public int NoseHeight { get; set; }
        public int JawWidth { get; set; }
        public int JawChin { get; set; }
        public int NoseWidth { get; set; }
        public int MouthSize { get; set; }
        public int MouthThicknessOfLips { get; set; }
        public int MouthType { get; set; }

        // --- 0x78 ---
        public int BrowsTypeAngle { get; set; }
        public int NoseType { get; set; }
        public int CheeksType { get; set; }
        public int MouthPosition { get; set; }
        public int EyesType { get; set; }
        public int JawType { get; set; }
    }
}