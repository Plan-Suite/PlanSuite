using PlanSuite.Services;

namespace PlanSuite.Models.Temporary
{
    public abstract class BaseViewModel
    {
        public LocalisationService Localisation { get; private set; }
    }
}
