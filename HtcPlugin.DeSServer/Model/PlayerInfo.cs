namespace HtcPlugin.DeSServer.Model {
    public class PlayerInfo {

        public string PlayerId { get; }
        public int GradeS { get; }
        public int GradeA { get; }
        public int GradeB { get; }
        public int GradeC { get; }
        public int GradeD { get; }
        public uint Logins { get; }
        public uint Sessions { get; }
        public int MsgRating { get; }
        public int Tendency { get; }
        public int DesiredTendency { get; }
        public bool UseDesired { get; }
        public uint PlayTime { get; }

        public PlayerInfo(string playerId, int gradeS, int gradeA, int gradeB, int gradeC, int gradeD, uint logins, uint sessions, int msgRating, int tendency, int desiredTendency, bool useDesired, uint playTime) {
            PlayerId = playerId;
            GradeS = gradeS;
            GradeA = gradeA;
            GradeB = gradeB;
            GradeC = gradeC;
            GradeD = gradeD;
            Logins = logins;
            Sessions = sessions;
            MsgRating = msgRating;
            Tendency = tendency;
            DesiredTendency = desiredTendency;
            UseDesired = useDesired;
            PlayTime = playTime;
        }
    }
}
