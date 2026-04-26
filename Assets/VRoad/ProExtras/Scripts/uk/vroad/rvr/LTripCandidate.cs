using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public interface LTripCandidate
    {
        void NewCandidatesAvailable();

        void CandidateChosen();

        void CandidateLitChanged();

        void NewDealPriceChanged(int newPrice);
    }
}
