using System.Threading.Tasks;

namespace MilkBottle.Interfaces {
    interface IDatabaseBuilder {
        Task<bool>  ReconcileDatabase();
    }
}
