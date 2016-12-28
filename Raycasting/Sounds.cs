using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
   public class Sounds
    {
        public static Sounds Instance { get; set; }
        private Sounds(){}
        static Sounds() { Sounds.Instance = new Sounds(); }
        public SoundEffect Bump { get; set; }
    }
}
