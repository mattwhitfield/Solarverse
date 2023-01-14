using Solarverse.Core.Integration.GivEnergy;
using Solarverse.Core.Integration.GivEnergy.Models;
using Solarverse.Core.Integration.Octopus;
using Solarverse.Core.Integration.Octopus.Models;
using Solarverse.Core.Integration.Solcast;
using Solarverse.Core.Integration.Solcast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solarverse.Core.Services
{
    public interface IIntegrationProvider
    {
        IGivEnergyClient GivEnergyClient { get; }
        
        ISolcastClient SolcastClient { get; }

        IOctopusClient OctopusClient { get; }
    }

    public interface IDataUpdateService
    {
        void Update(CurrentState currentState);
        void Update(ForecastSet forecast);
        void UpdateIncomingRates(AgileRates agileRates);
        void UpdateOutgoingRates(AgileRates agileRates);
    }

    public interface IControlPlanFactory
    {
        void CreatePlan();

        void CheckForAdaptations(Integration.GivEnergy.Models.CurrentState currentState);
    }

    public interface IControlPlanExecutor
    {
        Task<bool> ExecutePlan();
    }

    public interface IDataStore
    {
    }
}
