using System;
using System.Collections.Generic;
using System.Text;

namespace VmsPlusNTC
{
    class LoginResult
    {
        public bool IsActivation { get; set; }
        public bool Success { get; set; }
        public bool LockedOut { get; set; }
        public string Error { get; set; }
        public string ActivationToken { get; set; }
    }
}
