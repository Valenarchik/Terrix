using System.Diagnostics.CodeAnalysis;
using Terrix.Model.Map;

namespace Terrix.Model.Entities
{
    public class Player
    {
        [MaybeNull] public Country Country { get; private set; }
    }
}