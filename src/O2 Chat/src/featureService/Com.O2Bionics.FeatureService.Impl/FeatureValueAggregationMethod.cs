namespace Com.O2Bionics.FeatureService.Impl
{
    public enum FeatureValueAggregationMethod
    {
        // ReSharper disable once InconsistentNaming
        CAS = 0, // Customer-Addon-Service
        Default = CAS, // Default should be defined after actual member to make ToString("N") return actual member name
        Sum = 1,
        Min = 2,
        Max = 3,
    }
}