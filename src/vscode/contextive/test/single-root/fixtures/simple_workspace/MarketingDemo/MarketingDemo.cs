/// <summary>
/// A domain entity for a Page for tracking visits, bounce-rate and referrers to the p
/// </summary>
public class Page
{
    public Page() {}

    public Guid PageId { get; }

    public decimal BounceRate { get; }

    public void Bounce() {}

    public bool LandingPage { get;}
}