using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.ECS.Systems;

public interface ITime
{
    public float Time { get; }
    public double TimeDouble { get; }

    public float DeltaTime { get; }
    public double DeltaTimeDouble { get; }

    public float FixedDeltaTime { get; }
    public double FixedDeltaTimeDouble { get; }

    public float TimeScale { get; set; }    
}
