// Original Authors - Wyatt Senalik
using EternalWar.Core.Helpers;

namespace EternalWar.Client.Application.View
{
    internal abstract record ChangeHandledResult
    {
        public abstract bool CanBeConsumed { get; }
    }
    internal sealed record ChangeHandledResult_Finished : ChangeHandledResult
    {
        public static readonly ChangeHandledResult_Finished Instance = new();

        public override bool CanBeConsumed => true;

        // Private empty constructor to disallow creation to avoid unnecessary gc allocations
        private ChangeHandledResult_Finished() { }
    }
    internal sealed record ChangeHandledResult_OngoingTask(IReadOnlyPresentationTask Task) : ChangeHandledResult
    {
        public override bool CanBeConsumed => Task.IsFinished;
    }
    internal sealed record ChangeHandledResult_Composite(ValueArray<ChangeHandledResult> SubResults) : ChangeHandledResult
    {
        public override bool CanBeConsumed
        {
            get
            {
                foreach (ChangeHandledResult result in SubResults)
                {
                    if (!result.CanBeConsumed)
                        return false;
                }
                return true;
            }
        }

        public ChangeHandledResult_Composite(params ChangeHandledResult[] subResults) : this(subResults.ToValueArray())
        { }
    }

    internal static class ChangeHandledResultExtensions
    {
        public static ChangeHandledResult_Composite Combine(this ChangeHandledResult result, ChangeHandledResult otherResult) => new(result, otherResult);

        public static ChangeHandledResult_Composite Combine(this ChangeHandledResult result, params ChangeHandledResult[] otherResults)
        {
            using ValueArray<ChangeHandledResult>.Builder builder = [result, .. otherResults];
            return new ChangeHandledResult_Composite(builder.ToValueArray());
        }
    }
}
