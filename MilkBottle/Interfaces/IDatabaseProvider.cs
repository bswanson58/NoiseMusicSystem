using System;
using LanguageExt;
using LiteDB;

namespace MilkBottle.Interfaces {
    interface IDatabaseProvider : IDisposable {
        Either<Exception, LiteDatabase>    GetDatabase();
    }
}
