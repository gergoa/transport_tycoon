using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.Core.Map
{
    public class BridgeProperties
    {
        public int Cost { get; set; }
        public int MaxLength { get; set; }
        public float SpeedLimit { get; set; }

        public static BridgeProperties Get(BridgeType type)
        {
            return type switch
            {
                BridgeType.Wooden => new BridgeProperties
                {
                    Cost = 10,
                    MaxLength = 3,
                    SpeedLimit = 1.0f
                },
                BridgeType.Steel => new BridgeProperties
                {
                    Cost = 6,
                    MaxLength = 6,
                    SpeedLimit = 2.0f
                },
                BridgeType.Highway => new BridgeProperties
                {
                    Cost = 4,
                    MaxLength = 10,
                    SpeedLimit = 3.0f
                },
                _ => throw new Exception("Unknown bridge type")
            };
        }
    }
}
