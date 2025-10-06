using LiteDB;

namespace TheScheduler.Data
{    
    public class LiteDBService
    {
        private const string DbPath = @"Data.db";

        public static LiteDatabase GetDatabase()
        {
            return new LiteDatabase(DbPath);
        }
    }
}
