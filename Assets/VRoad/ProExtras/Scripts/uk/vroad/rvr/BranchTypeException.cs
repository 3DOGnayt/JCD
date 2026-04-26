using System;
using uk.vroad.api.route;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    [System.Serializable]
    public class BranchTypeException : Exception
    {
        public BranchTypeException(IBranch offender)
        {
        }
    }
}
