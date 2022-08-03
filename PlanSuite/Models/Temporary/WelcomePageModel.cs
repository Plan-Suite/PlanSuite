using PlanSuite.Enums;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Models.Temporary
{
    public class WelcomePageModel
    {
        public UpgradeModel UpgradePlusModel = new UpgradeModel();
    }

    public class UpgradeModel
    {
        public string LookupKey { get; set; }
        public PaymentTier PaymentTier { get; set; }
    }

    public class UpgradeSuccessModel
    {
        public Sale Sale { get; set; }
        public int Amount { get; set; }
    }
}
