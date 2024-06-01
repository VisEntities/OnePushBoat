using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("One Push Boat", "VisEntities", "1.0.0")]
    [Description("Unflips flipped boats with one push and optionally mounts the pusher to the driver seat.")]
    public class OnePushBoat : RustPlugin
    {
        #region Fields

        private static OnePushBoat _plugin;
        private static Configuration _config;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Mount Pusher To Driver Seat")]
            public bool MountPusherToDriverSeat { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                MountPusherToDriverSeat = true
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        private object OnVehiclePush(BaseBoat boat, BasePlayer player)
        {
            if (player == null || boat == null)
                return null;

            if (!boat.IsFlipped())
                return null;

            UnflipBoat(boat);

            if (_config.MountPusherToDriverSeat)
            {
                var (driverSeat, driverSeatIndex) = GetDriverSeat(boat);
                if (driverSeat != null && driverSeatIndex != -1)
                {
                    driverSeat.mountable.MountPlayer(player);
                }
            }

            return true;
        }

        #endregion Oxide Hooks

        #region Boat Unflipping

        private void UnflipBoat(BaseBoat boat)
        {
            boat.transform.rotation = Quaternion.Euler(0, boat.transform.rotation.eulerAngles.y, 0);
            boat.rigidBody.angularVelocity = Vector3.zero;
            boat.rigidBody.velocity = Vector3.zero;
        }

        #endregion Boat Unflipping

        #region Driver Seat Retrieval

        private (BaseVehicle.MountPointInfo, int) GetDriverSeat(BaseVehicle vehicle)
        {
            if (vehicle == null)
                return (null, -1);

            int index = 0;
            foreach (var mountPoint in vehicle.allMountPoints)
            {
                if (mountPoint.isDriver && mountPoint.mountable != null)
                {
                    return (mountPoint, index);
                }
                index++;
            }
            return (null, -1);
        }

        #endregion Driver Seat Retrieval
    }
}