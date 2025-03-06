using System.Diagnostics.CodeAnalysis;
using Terrix.Map;

namespace Terrix.Entities
{
    public class Player
    {
        [MaybeNull] public Country Country { get; private set; }
    }
}