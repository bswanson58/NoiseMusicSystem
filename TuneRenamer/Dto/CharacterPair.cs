namespace TuneRenamer.Dto {
    public class CharacterPair {
        public  char    StartCharacter { get; }
        public  char    EndCharacter { get; }
        public  string  Description { get; }

        public CharacterPair( char start, char end, string description ) {
            StartCharacter = start;
            EndCharacter = end;
            Description = description;
        }
    }
}
