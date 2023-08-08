namespace Calidus.lib.Data.Types {
    public class UserEmail {
        [DBColumn("uid", DBColumnType.LONG_INTEGER_UNSIGNED, primaryKey: true)]
        public ulong discordId;
        [DBColumn("email", DBColumnType.STRING)]
        public string? email;
    }
}