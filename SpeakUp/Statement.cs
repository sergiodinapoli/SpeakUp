using RimWorld;
using Verse;

namespace SpeakUp
{
    public class Statement
    {
        public Pawn
            Emitter,
            Reciever,
            Gossipee;

        public InteractionDef IntDef;
        public Talk Talk;
        public int 
            Timing,
            Iteration;

        public Statement(Pawn emitter, Pawn recipient, Pawn gossipee, int timing, InteractionDef intDef, Talk talk, int iteration)
        {
            Emitter = emitter;
            Reciever = recipient;
            Gossipee = gossipee;
            Timing = timing;
            IntDef = intDef;
            Talk = talk;
            Iteration = iteration;
        }
    }
}