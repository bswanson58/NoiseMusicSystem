using System.Threading.Tasks;

namespace MilkBottle.Interfaces {
    interface IDatabaseBuilder {
        bool        WaitForDatabaseReconcile();

        Task<bool>  ReconcileDatabase();
    }
}
