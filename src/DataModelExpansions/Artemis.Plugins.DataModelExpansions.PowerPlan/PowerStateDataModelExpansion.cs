using System;
using System.Collections.Generic;
using Artemis.Core.Modules;
using Artemis.Plugins.DataModelExpansions.PowerState.DataModels;
using Artemis.Plugins.DataModelExpansions.PowerState.Utils;
using Serilog;

namespace Artemis.Plugins.DataModelExpansions.PowerState
{
    public class PowerStateDataModelExpansion : Module<PowerStateDataModel>
    {

        private readonly ILogger _logger;
        private readonly PowerPlanUtil _powerPlanUtil;

        public PowerStateDataModelExpansion(ILogger logger)
        {
            _logger = logger;
            _powerPlanUtil = new PowerPlanUtil();
        }

        public override List<IModuleActivationRequirement> ActivationRequirements => null;

        public override void Enable()
        {
            _powerPlanUtil.PowerPlanChanged += PowerPlanUtil_PowerPlanChanged;
            _powerPlanUtil.StartPlanWatcher();
            UpdatePowerPlan();
        }

        private void PowerPlanUtil_PowerPlanChanged(object sender, EventArgs e)
        {
            UpdatePowerPlan();
        }

        public override void Disable()
        {
            _powerPlanUtil.StopPlanWatcher();
            _powerPlanUtil.PowerPlanChanged -= PowerPlanUtil_PowerPlanChanged;
        }

        public override void Update(double deltaTime) { }

        private void UpdatePowerPlan()
        {
            try
            {
                DataModel.CurrentPowerPlanGuid = PowerPlanUtil.GetCurrentPowerPlanGuid();

                // We will use W32 api to get Friendly Name because this method is localization friendly whereas registry not
                DataModel.CurrentPowerPlanFriendlyName = PowerPlanUtilesW32.GetCurrentPlanFriendlyname();
                DataModel.PowerPlanChanged.Trigger(new PowerPlanEventArgs(DataModel.CurrentPowerPlanGuid, DataModel.CurrentPowerPlanFriendlyName));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }
        }

    }
}