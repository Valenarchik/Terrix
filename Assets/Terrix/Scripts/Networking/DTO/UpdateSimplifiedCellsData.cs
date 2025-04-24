using System;
using System.Collections.Generic;

namespace Terrix.Network.DTO
{
    public class UpdateSimplifiedCellsData
    {
        public int PlayerId { get; }
        public List<SimplifiedCellChangeData> ChangeData { get; }

        public UpdateSimplifiedCellsData(int playerId, List<SimplifiedCellChangeData> changeData)
        {
            PlayerId = playerId;
            ChangeData = changeData ?? throw new ArgumentNullException(nameof(changeData));
        }
    }
}