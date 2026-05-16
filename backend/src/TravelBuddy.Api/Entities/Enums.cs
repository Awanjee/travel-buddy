namespace TravelBuddy.Api.Entities;

public enum BudgetBand
{
    Budget = 0,
    MidRange = 1,
    Luxury = 2
}

public enum EnergyLevel
{
    Low = 0,
    Moderate = 1,
    High = 2
}

public enum VisaIntent
{
    NotSure = 0,
    VisaFree = 1,
    VisaOnArrival = 2,
    EVisa = 3,
    EmbassyVisa = 4,
    AlreadyHaveVisa = 5
}

public enum CandidateType
{
    Place = 0,
    Hotel = 1,
    Activity = 2
}

public enum CandidateTag
{
    MustSee = 0,
    Popular = 1,
    OftenMissed = 2,
    HiddenGem = 3
}

public enum DecisionType
{
    Approved = 0,
    Declined = 1,
    Skipped = 2
}

public enum ConnectionStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Blocked = 3
}

public enum SwipeDirection
{
    Left = 0,
    Right = 1
}
