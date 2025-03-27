namespace PlayerLibrary
{
    public class Player
    {
        public int PlayerIndex { get; set; }
        public string? Name { get; set; }
        public string? ShirtName { get; set; }
        public int Country { get; set; }
        public int Commentary { get; set; }
        public int Position { get; set; }
        public int Age { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }

        //Stats
        public int StatAttack { get; set; }
        public int StatDefense { get; set; }
        public int StatBodyBalance { get; set; }
        public int StatStamina { get; set; }
        public int StatTopSpeed { get; set; }
        public int StatAcceleration { get; set; }
        public int StatResponse { get; set; }
        public int StatAgility { get; set; }
        public int StatDribbleAccuracy { get; set; }
        public int StatDribbleSpeed { get;set; }
        public int StatShortPassAccuracy { get; set; }
        public int StatShortPassSpeed { get; set; }
        public int StatLongPassAccuracy { get;set; }
        public int StatLongPassSpeed { get;set;}
        public int StatShotAccuracy { get; set; }
        public int StatShotPower { get; set; }
        public int StatShotTechnique { get; set; }
        public int StatFreeKickAccuracy { get; set; }
        public int StatCurve { get; set; }
        public int StatHeader { get; set; }
        public int StatJump { get; set; }
        public int StatTechnique { get; set; }
        public int StatAggression { get; set; }
        public int StatMentality { get; set; }
        public int StatGoalkeepingSkills { get; set; }
        public int StatTeamWorkAbility { get; set; }
        public int StatCondition { get; set; }
        public int StatWeakFootAccuracy { get; set; }
        public int StatWeakFootFrequency { get; set; }
    }
}