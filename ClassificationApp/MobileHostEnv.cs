using System;

namespace ClassificationApp
{
    public sealed class MobileHostEnv
    {
        public required String ServerUrl { get; set; } = "192.168.0.129";
        public required String ServerPort { get; set; } = "5000";

    }
}
